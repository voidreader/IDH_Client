using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamEditor
{
	GameObject slotRoot;
	GameObject listRoot;

	TeamSlotMgr slot;
    internal TeamSlotMgr Slot { get { return slot; } }
	internal ItemList list;

	Action<long> cbManage;
	Action cbChanged;
	Action<long> cbSelect;
	bool isFarming;


	long[] ids;
	long selectedUnitID = -1;
	internal long SelectedUnitID
	{
		get
		{
			return selectedUnitID;
		}
		set
		{
			if (selectedUnitID != -1)
				SetSelect(selectedUnitID, false);

			selectedUnitID = value;

			if (selectedUnitID != -1)
				SetSelect(selectedUnitID, true);
		}
	}

	bool IsAffectActive = false;

	internal TeamEditor(GameObject _slotGo, GameObject _listGo, Action<long> _cbManage, Action<long> _cbSelect = null, Action _cbChanged = null, bool _init = true , bool _isFarming = false)
	{
		slotRoot = _slotGo;
		listRoot = _listGo;
		cbManage = _cbManage;
		cbChanged = _cbChanged;
		cbSelect = _cbSelect;
		isFarming = _isFarming;

		slot = new TeamSlotMgr(slotRoot, OnClickSlot, OnPressSlot, _cbManage, OnUnpos, OnSwapSlot);
		list = new ItemList(listRoot,	InvenUI.TypeFlag.Character, CardType.Character, OnClickList, OnPressList, InvenBase.HeroSortByRankDescending, CBDoFilter);

		if (_init)
			InitCard();

	}

	internal void InitCard()
	{
		if (ids != null)
		{
			list.Reposition(false);
			return;
		}

		ids = GameCore.Instance.PlayerDataMgr.GetUnitIds();
		for (int i = 0; i < ids.Length; ++i)
		{
			var unit = GameCore.Instance.PlayerDataMgr.GetUnitSData(ids[i]);
			var tf = AddCard(ids[i], unit.dispatch && isFarming, cbManage);
		}
		list.Reposition(false);
	}

	internal void UpdateCardStates()
	{
		int count = list.GetCount();
		for (int i = 0; i < count;++i)
		{
			var card = list.gameObject.transform.GetChild(i).GetComponent<CardBase>();
			var data = GameCore.Instance.PlayerDataMgr.GetUnitSData(card.ID);
			if(data.dispatch && isFarming)	card.SetState(CardBase.States.Detachment);
			else							card.SetState(CardBase.States.Normal);
		}
	}

	internal Transform AddCard(long _id, bool _printDispatch, Action<long> _cbManage)
	{
		var sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(_id);
        var data = GameCore.Instance.PlayerDataMgr.GetUnitData(_id);
        if(data.IsExpCard())
            return null;
        
        var card = CardBase.CreateBigCard(sdata, listRoot.transform);// callback은 ItemList에서 설정한다.
		card.SetManageButtonCallBack(_cbManage);
		card.SetUnposButtonCallBack(OnUnpos);
        if (sdata.GetEquipCount() != 0) card.SetInfo(CardBase.UnitInfo.Power | CardBase.UnitInfo.Equip);
        else                            card.SetInfo(CardBase.UnitInfo.Power);

		card.pressDelay = 1.5f;
		if (_printDispatch)	card.SetState(CardBase.States.Detachment, true);
		else				card.SetState(CardBase.States.Normal, true);

		// cache
		list.AddItem(card.transform, _id, card);

		return card.transform;
	}

	internal void Clear()
	{
		ClearSlot();
		SelectedUnitID = -1;
	}


	// 유닛 선택
	// by Card
	private void OnClickList(long _id)
	{
		SelectUnit(_id);
	}

	// by Slot
	private void OnClickSlot(int _idx)
	{
		//Debug.Log("click " + SelectedUnitID);
		if (IsAffectActive)
			OffAffectAllOnList();

		if (SelectedUnitID != -1)
		{
			SetArrangement(_idx, GameCore.Instance.PlayerDataMgr.GetUnitSData(SelectedUnitID), true);
			SelectedUnitID = -1;
		}
		else
		{
			var id = slot.GetID(_idx);
			if (id != -1) SelectUnit(id);
		}

		if (cbSelect != null) cbSelect(SelectedUnitID);
	}

	private void SelectUnit(long _id)
	{
		OffAffectAllOnList();

		if (SelectedUnitID == _id)
		{
			SelectedUnitID = -1;
			return;
		}

		// 클릭된 카드 활성화
		SelectedUnitID = _id;
		if (cbSelect != null) cbSelect(SelectedUnitID);
	}

	// slot on unit
	internal void SetArrangement(int _slotIdx, HeroSData _data, bool _bPrint = true, bool _bActBtn = true)
	{
		if (_data == null)
			return;

		if (_data.dispatch && isFarming)
			return;

		var data = GameCore.Instance.DataMgr.GetUnitData(_data.key);
        if (data == null)
            return;

		long id = _data.uid;
		long prevId = slot.GetID(_slotIdx);

		bool bSame = slot.GetSlotIndexByUID(_data.uid) >= 0;

		// 이미 배치된 같은 동일한 캐릭터 찾기
		int prePosSameChar = slot.GetSlotIndexByCharID(data.charId);

		// 완전 동일한 캐릭터가 동일한 슬롯으로 옮겨질때
		if (prePosSameChar == _slotIdx)
			if (slot.GetID(_slotIdx) == id)
				return;

		if (prePosSameChar != -1) // 동일한 캐릭터가 존재하는 경우
		{
			if (!bSame && slot.GetSlotIndexByCharIDType(_slotIdx) < 0)
			{
				list.GetItem(slot.GetID(prePosSameChar)).SetState(GameCore.Instance.PlayerDataMgr.GetUnitSData(id).dispatch && isFarming ? CardBase.States.Detachment : CardBase.States.Normal);
				slot.RemoveCharacter(prePosSameChar);

				slot.SetCharacter(_slotIdx, _data.uid, true);
                GameCore.Instance.ShowAlert("동일한 캐릭터가 편성되어 \n기존에 배치한 영웅이 교체되었습니다.");
			}
			else
			{
				slot.SwapCharacter(_slotIdx, prePosSameChar);
            }
		}
		else                      // 동일한 캐릭터가 존재하지 않는 경우
		{
			var removeId = slot.GetID(_slotIdx);
			if (removeId != -1)
				list.GetItem(removeId).SetState(GameCore.Instance.PlayerDataMgr.GetUnitSData(removeId).dispatch&&isFarming ? CardBase.States.Detachment : CardBase.States.Normal);
			else if( slot.EmptyCount == 1)
			{
				GameCore.Instance.ShowNotice("알람", "팀은 최대 5명까지 가능합니다.", 0);
				return;
			}
			slot.SetCharacter(_slotIdx, id, _bPrint);
		}

		list.GetItem(id).SetState(CardBase.States.Arrangement);
		if (_bActBtn)
		{
			list.GetItem(id).AddButton(CardBase.ActiveButton.Unposition);
		}
		else
		{
			list.GetItem(id).SetButton(CardBase.ActiveButton.None);
		}
		slot.SetSelect(_slotIdx, _bActBtn);

		if(cbChanged != null) cbChanged();
		list.Reposition(false);
	}

    internal void ListReposition(bool _Immediately = true)
    {
        list.Reposition(_Immediately);
    }

    internal void RemoveCharacterBySlot(int _slotNum)
	{
		var id = slot.GetID(_slotNum);
		if (id != -1)
		{
			slot.RemoveCharacter(_slotNum);
			list.GetItem(id).SetState(GameCore.Instance.PlayerDataMgr.GetUnitSData(id).dispatch && isFarming ? CardBase.States.Detachment : CardBase.States.Normal);
			list.GetItem(id).SetSelect(CardBase.SelectState.None);
			if (cbChanged != null) cbChanged();
			list.Reposition(false);
		}
	}
	// swap
	private void OnSwapSlot(int _idx)
	{
        RaycastHit2D hitInfo;
		if (UnityCommonFunc.GetCameraHitInfo2D(GameCore.Instance.GetUICam(), out hitInfo, "UI"))
		{
            if (hitInfo.collider.gameObject.GetComponent<HeroSlot>() == null)
                return;

            var cnt = slot.Count;
			for (int i = 0; i < cnt; ++i)
				if ("slot" + (i + 1) == hitInfo.collider.name)
				{
					if (_idx == i)
						return;

					slot.SwapCharacter(_idx, i);
					if (cbChanged != null) cbChanged();
					break;
				}

            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Pre);
            OffAffectAllOnList();
			SelectedUnitID = -1;
		}
	}

	// unpos unit
	private void OnUnpos(long _id)
	{
		Debug.Log("Unpos " + _id);
		for (int i = 0; i < slot.Count; ++i)
			if (slot.GetID(i) == _id)
			{
				slot.RemoveCharacter(i);
				list.GetItem(_id).SetState(GameCore.Instance.PlayerDataMgr.GetUnitSData(_id).dispatch && isFarming ? CardBase.States.Detachment : CardBase.States.Normal);
			}
		OffAffectAllOnList();
		SelectedUnitID = -1;

		if (cbChanged != null) cbChanged();
		list.Reposition(false);
	}

	internal void ClearSlot()
	{
        for (int i = 0; i < slot.Count; i++)
		{
			var id = slot.GetID(i);
			if (id != -1)
			{
				list.GetItem(id).SetState(GameCore.Instance.PlayerDataMgr.GetUnitSData(id).dispatch && isFarming ? CardBase.States.Detachment : CardBase.States.Normal);
				slot.RemoveCharacter(i);
				slot.SetSelect(i, false);
			}
		}
    }

	// affect
	private void OnPressList(long _id)
	{
		if (_id == -1)
			return;

		SetAffect(_id);
	}

	private void OnPressSlot(int _num)
	{
		if (_num == -1)
			return;

		SetAffect(slot.GetID(_num));
	}

	private void SetAffect(long _id)
	{
		OffAffectAllOnList();
		SelectedUnitID = _id;

		IsAffectActive = true;
		var unit = GameCore.Instance.PlayerDataMgr.GetUnitData(_id);
		
		// 타겟 Affect 활성화
		list.GetItem(_id).SetSelect(CardBase.SelectState.Highlight);
		var slotIdx = slot.GetSlotIndexByUID(_id);
		if(slotIdx != -1)
			slot.SetAffect(slotIdx, true);

		for (int i = 0; i < ids.Length; ++i)
		{
			var tgUnit = GameCore.Instance.PlayerDataMgr.GetUnitData(ids[i]);
			var data = GameCore.Instance.DataMgr.GetchemistryDataInFindMap(unit.charIdType, tgUnit.charIdType);
            var data2 = GameCore.Instance.DataMgr.GetchemistryDataInFindMap(tgUnit.charIdType, unit.charIdType);

            // 캐미가 있는 유닛 Affect 활성화
            if (data != null || data2 != null)
			{
				list.GetItem(ids[i]).SetSelect(CardBase.SelectState.Highlight);
				slotIdx = slot.GetSlotIndexByUID(ids[i]);
				if(slotIdx != -1)
					slot.SetAffect(slotIdx, true);
			}
		}
	}

	// filter
	internal void DoFilter(int _filter)
	{
		list.DoFilter(_filter);
	}
    
	internal bool CBDoFilter(CardBase _hero, int _filter)
	{
		// Filtering
		int rank = FilterCtrl.GetFilterRank(_filter);
		int type = FilterCtrl.GetFilterType(_filter);
		int rare = FilterCtrl.GetFilterRare(_filter);

		var data = (UnitDataMap)_hero.Data;

		return	        (rank == 0 || (1 << (4 - data.rank)     & rank) != 0) &&
						(type == 0 || (1 << (data.charType - 1) & type) != 0) &&
						(rare == 0 || (1 << (data.evolLvl-1)    & rare) != 0);
	}

	internal void SetOffTeamSkillIcon()
	{
		for (int i = 0; i < slot.Count; ++i)
			slot.SetTeamSkillIcon(i, false);
	}
	internal void SetTeamKillIcon(int _idx, bool _active)
	{
		slot.SetTeamSkillIcon(_idx, _active);
	}

	// GetTeamPos Info
	internal long GetSlotOnUnitID(int _num)
	{
		return slot.GetID(_num);
	}


	/// <summary>
	/// 모든 Affect 효과를 끈다.
	/// </summary>
	internal void OffAffectAllOnList()
	{
		if (!IsAffectActive)
			return;

        for (int i = 0; i < ids.Length; ++i)
        {
            var item = list.GetItem(ids[i]);
            if (item != null)
                item.SetSelect(CardBase.SelectState.None);
        }
		for (int i = 0; i < slot.Count; ++i)
			slot.SetAffect(i, false);

		IsAffectActive = false;
	}


	// 카드 선택시 또는 해제시 슬롯 전체와 대상 카드 한장을 하이라이트
	internal void SetSelect(long _id, bool _select)
	{
		for (int i = 0; i < slot.Count; i++)
		{
			if (slot.GetID(i) == _id)
				slot.SetSelect(i, _select);
			else
			{
				if ( slot.EmptyCount == 1 && slot.GetID(i) == -1)
					slot.SetHighlight(i, false);
				else
					slot.SetHighlight(i, _select);
			}
		}

		if (list.ContainsKey(_id))
		{
			var card = list.GetItem(_id);
			card.SetSelect(_select ? CardBase.SelectState.Select : CardBase.SelectState.None);
			if (_select)
			{
				card.SetButton(CardBase.ActiveButton.Management);
				if (card.State == CardBase.States.Arrangement)
					card.AddButton(CardBase.ActiveButton.Unposition);
			}
			else
			{
				card.SetButton(CardBase.ActiveButton.None);
			}

		}
	}


	internal void SetAffect(long _id, bool _affect)
	{
		if (!list.ContainsKey(_id))
		{
			Debug.LogError("Not Exist Unit ID");
			return;
		}

		list.GetItem(_id).SetSelect(_affect? CardBase.SelectState.Highlight : CardBase.SelectState.None);
		var num = slot.GetSlotIndexByUID(_id);
		slot.SetAffect(num, _affect);
	}


	internal int GetListCount()
	{
		return list.GetCount();
	}

	internal int GetSlotCount()
	{
		return slot.Count;
	}

	internal long GetUnitIDInSlot(int _idx)
	{
		return slot.GetID(_idx);
	}

	internal int GetSlotIndexByUID(long _id)
	{
		return slot.GetSlotIndexByUID(_id);
	}

    //sort
    internal void SetAlignment(Comparison<Transform> alignmentType)
    {
        list.SetSortCallBack(alignmentType);
    }

    internal Transform GetTeamSlotTransform(int pos)
    {
        return slot.GetSlot(pos).transform;
    }
}
