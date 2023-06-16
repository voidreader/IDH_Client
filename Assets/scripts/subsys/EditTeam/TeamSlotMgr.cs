using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSlotMgr
{
	GameObject root;

	HeroSlot[] slots;
	Dictionary<int, SpineCharacterCtrl> spinePool;

	private int count = 6;

	internal int Count { get { return count; } }
	internal int EmptyCount { get; private set; }

    internal Transform GetHeroSlot(int pos)
    {
        return slots[pos].transform;
    }

	internal TeamSlotMgr(GameObject _go, Action<int> _cbClick, Action<int> _cbPress, Action<long> _cbMenage, Action<long> _cbUnpos, Action<int> _cbSwap)
	{
		EmptyCount = count;

		root = _go;
		spinePool = new Dictionary<int, SpineCharacterCtrl>();
		slots = new HeroSlot[count];
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i] = UnityCommonFunc.GetComponentByName<HeroSlot>(root, "slot" + (i + 1));
			slots[i].Init(i, _cbClick, _cbPress, _cbMenage, _cbUnpos, _cbSwap);
			slots[i].SetSelect(false);
			slots[i].SetButton(false);
		}

		// Anchor Setting
		//var anc = GetComponent<UIAnchor>();

	}

	/// <summary>
	///  캐릭터 추가
	///  캐릭터 중복확인을 하지 않으므로 무결한 데이터를 보내야 한다.
	/// </summary>
	/// <param name="_idx"></param>
	/// <param name="_id"></param>
	internal void SetCharacter(int _idx, long _id, bool _bPrintChemi)
	{
		if (_idx < 0 || slots.Length <= _idx)
			return;

		var newUnit = GameCore.Instance.PlayerDataMgr.GetUnitData(_id);
		if (newUnit == null)
		{
			Debug.LogError("Not Find Character Data" + _id);
			return;
		}

		long oldID = slots[_idx].Id;
		SpineCharacterCtrl newChar = PopCache(_id);
		SpineCharacterCtrl oldChar = null;

		CalcChemi(_idx, false, true);
		if (newChar == null)	oldChar = slots[_idx].SetCharacter(_id);            // 캐시가 없는 경우
		else									oldChar = slots[_idx].SetCharacter(_id, newChar);   // 캐시가 있는 경우

		if (oldChar == null)	--EmptyCount;								// 빈슬롯 카운터 업데이트
		else									PushCache(oldID, oldChar);  // 스파인 캐릭터 캐싱

		// chemi & team buff Effect
		CalcChemi(_idx, _bPrintChemi, false);

	}


	// 캐릭터 제거
	internal void RemoveCharacter(int _idx)
	{
		// chemi & team buff Effect
		CalcChemi(_idx, false, true); // 캐릭터가 제거 되기 전에 계산해야하므로...

		long oldID = slots[_idx].Id;
		var oldChar = slots[_idx].RemoveCharacter();

		// 스파인 캐릭터 캐싱
		PushCache(oldID, oldChar);

		++EmptyCount;
	}

	internal void PushCache(long _id, SpineCharacterCtrl _spine)
	{
		if (_id != -1 && _spine != null)
		{
			var prefabId = GameCore.Instance.PlayerDataMgr.GetUnitData(_id).prefabId;
			if (!spinePool.ContainsKey(prefabId))
			{
				_spine.gameObject.SetActive(false);
				spinePool.Add(prefabId, _spine);
			}
			else
			{
				Debug.LogError(" already exist same spine character. " + _id);
				GameObject.Destroy(_spine.gameObject);
			}
		}
	}

	internal SpineCharacterCtrl PopCache(long _id)
	{
		var unit = GameCore.Instance.PlayerDataMgr.GetUnitData(_id);
		if (unit == null)
			return null;

		if (spinePool.ContainsKey(unit.prefabId))
		{
			var spine = spinePool[unit.prefabId];
			spinePool.Remove(unit.prefabId);
			spine.gameObject.SetActive(true);
			return spine;
		}

		return null;
	}

	// 캐릭터 위치 변경(스왑)
	internal void SwapCharacter(int _idx1, int _idx2)
	{
		var id1 = slots[_idx1].Id;
		var id2 = slots[_idx2].Id;

		CalcChemi(_idx1, false, true);
		CalcChemi(_idx2, false, true);

		SpineCharacterCtrl character1 = slots[_idx1].RemoveCharacter();
		SpineCharacterCtrl character2 = slots[_idx2].RemoveCharacter();

		if (character1 != null) slots[_idx2].SetCharacter(id1, character1);
		if (character2 != null) slots[_idx1].SetCharacter(id2, character2);

		CalcChemi(_idx1, false, false);
		CalcChemi(_idx2, false, false);
	}

	// 슬롯 하이라이트
	internal void SetHighlight(int _idx, bool _highlight)
	{
		slots[_idx].SetAffect(false);
		slots[_idx].SetSelect(_highlight);
		slots[_idx].SetButton(false);
	}

	// 캐릭터 선택 하이라이트
	internal void SetSelect(int _idx, bool _highlight)
	{
		slots[_idx].SetAffect(false);
		slots[_idx].SetSelect(_highlight);
		slots[_idx].SetButton(_highlight);
	}

	// 어펙트 하이라이트
	internal void SetAffect(int _idx, bool _highlight)
	{
		slots[_idx].SetSelect(false);
		slots[_idx].SetAffect(_highlight);
		slots[_idx].SetButton(false);
	}

	// 해당 슬롯 캐릭터 정보 반환
	internal long GetID(int _idx)
	{
		return slots[_idx].Id;
	}

    internal HeroSlot GetSlot(int _idx)
    {
        return slots[_idx];
    }

	// 유닛의 유니크 아이디로 슬롯을 찾음
	internal int GetSlotIndexByUID(long _id)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].Id == _id)
				return i;
		}

		// 없음
		return -1;
	}

	// 유닛의 캐릭터 아이디로 슬롯을 찾음
	internal int GetSlotIndexByCharIDType(int _cid)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsEmpty())
				continue;

			var charId = GameCore.Instance.PlayerDataMgr.GetUnitData(slots[i].Id).charIdType;
			if (charId == _cid)
				return i;
		}

		// 없음
		return -1;
	}
    // 유닛의 캐릭터 아이디로 슬롯을 찾음
    internal int GetSlotIndexByCharID(int _cid)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
                continue;

            var charId = GameCore.Instance.PlayerDataMgr.GetUnitData(slots[i].Id).charId;
            if (charId == _cid)
                return i;
        }

        // 없음
        return -1;
    }

    // 캐미 & 팀 스킬 
    private void CalcChemiAll(bool _bPrint)
	{
		for (int i = 0; i < slots.Length; i++)
			slots[i].ResetChemistry();

		for (int i = 0; i < slots.Length; ++i)
		{
			if (slots[i].IsEmpty())
				continue;

			for (int j = i+1; j < slots.Length; ++j)
			{
				if (slots[j].IsEmpty())
					continue;

				CalcChemi(i, j, _bPrint, true);
			}
		}
	}

	private void CalcChemi(int _slotNum, bool _bPrint, bool _remove)
	{
		if (slots[_slotNum].IsEmpty())
			return;

		for (int i = 0; i < slots.Length; ++i)
		{
			if (slots[i].IsEmpty() || i == _slotNum)
				continue;

			CalcChemi(_slotNum, i, _bPrint, _remove);
		}
	}

	private void CalcChemi(int _slot1, int _slot2, bool _bPrint, bool _remove)
	{
		if (slots[_slot1].IsEmpty() || slots[_slot2].IsEmpty())
			return;

		var id1 = GameCore.Instance.PlayerDataMgr.GetUnitData(slots[_slot1].Id).charIdType;
		var id2 = GameCore.Instance.PlayerDataMgr.GetUnitData(slots[_slot2].Id).charIdType;

		var chemi1 = GameCore.Instance.DataMgr.GetchemistryDataInFindMap(id1, id2);
		var chemi2 = GameCore.Instance.DataMgr.GetchemistryDataInFindMap(id2, id1);
		if (chemi1 == null && chemi2 == null)
			return;

		if (chemi1 != null)
		{
			if (_remove)	slots[_slot1].DecChemistry();
			else					slots[_slot1].IncChemistry();

			if (_bPrint && !_remove)
			{
				for (int j = 0; j < chemi1.teamBuffIds.Length; ++j)
				{
					if (chemi1.teamBuffIds[j] == -1)	continue;
					var effect = GameCore.Instance.DataMgr.GetEffectData(chemi1.teamBuffIds[j]);
					slots[_slot1].AddPrintChemistry(GetBuffstring(effect));
				}
			}
		}

		if (chemi2 != null)
		{
			if (_remove)	slots[_slot2].DecChemistry();
			else					slots[_slot2].IncChemistry();

			if (_bPrint && !_remove)
			{
				for (int j = 0; j < chemi2.teamBuffIds.Length; ++j)
				{
					if (chemi2.teamBuffIds[j] == -1)	continue;
					var effect = GameCore.Instance.DataMgr.GetEffectData(chemi2.teamBuffIds[j]);
					slots[_slot2].AddPrintChemistry(GetBuffstring(effect));
				}
			}
		}
	}


	private string GetBuffstring(EffectDataMap _effect)
	{
		string str = "";
		switch (_effect.type)
		{
			case EffectType.CounterAtk:			str = "반격!"; break;
			case EffectType.FollowAtk:			str = "협공!"; break;
			case EffectType.SetGuard:				str = "가드!"; break;
			case EffectType.ProportionDmg:	str = "정확한 한방"; break;
			case EffectType.Buff:
				switch (_effect.buffData.tgStat)
				{
					case UnitStat.Hp:			str = "최대체력" + _effect.buffData.value1 + "%"; break;
					case UnitStat.Attack:		str = "공격력" + _effect.buffData.value1 + "%"; break;
					case UnitStat.Armor:		str = "방어력" + _effect.buffData.value1 + "%"; break;
					case UnitStat.Vigor:		str = "행동력" + _effect.buffData.value1 + "%"; break;
					case UnitStat.Agility:	    str = "민첩성" + _effect.buffData.value1 + "%"; break;
					case UnitStat.Concent:	    str = "집중력" + _effect.buffData.value1 + "%"; break;
					case UnitStat.Mental:		str = "정신력" + _effect.buffData.value1 + "%"; break;
					case UnitStat.Recovery:     str = "회복력" + _effect.buffData.value1 + "%"; break;
						//case UnitStat.Aggro: str = "어그로" + inc + _value + "턴"; break;
				}
				break;
		}

		return str;
	}

	internal void SetAllTeamSkillIcon(bool _active)
	{
		for(int i = 0; i < slots.Length;++i)
			slots[i].SetTeamSkillIcon(_active);
	}

	internal void SetTeamSkillIcon(int _idx, bool _active)
	{
		slots[_idx].SetTeamSkillIcon(_active);
	}
}
