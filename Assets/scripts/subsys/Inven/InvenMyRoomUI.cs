using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class InvenMyRoomUI : InvenBase
{
	private GameObject btArrange;
	private GameObject btUnArrange;
	private GameObject btUnselect;
	private GameObject btAutoArrange;

	internal void Init(Action _cbArrange, Action _cbUnArrange)
	{
		btArrange = UnityCommonFunc.GetGameObjectByName(gameObject, "btArrange");
		btArrange.GetComponent<UIButton>().onClick.Add(new EventDelegate( () => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            _cbArrange(); }));

		btUnArrange = UnityCommonFunc.GetGameObjectByName(gameObject, "btUnarrange");
		btUnArrange.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            _cbUnArrange(); } ));
        
		btUnselect = UnityCommonFunc.GetGameObjectByName(gameObject, "btUnselect");
		//btUnselect.GetComponent<UIButton>().onClick.Add(new EventDelegate( () => { GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button); Unselect(); } ));
        btUnselect.gameObject.SetActive(false);

		btAutoArrange = UnityCommonFunc.GetGameObjectByName(gameObject, "btAutoArrange");
        //btAutoArrange.GetComponent<UIButton>().onClick.Add(new EventDelegate( () => { GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button); GameCore.Instance.ShowAlert("미구현"); } ));
        btAutoArrange.gameObject.SetActive(false);

		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btClose").onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            Unselect(); _cbArrange(); Destroy(); }));

		TypeFlag flags =	TypeFlag.Character |
							TypeFlag.SetInterior|
							TypeFlag.Furniture |
							TypeFlag.Prop |
							TypeFlag.Wall |
							TypeFlag.Floor;

        Init(flags, true);


		// Set Sort Callback
		listRootTable[TypeFlag.Character].SetSortCallBack(CBSortCharacter);

		// set Filter Callback
	}

	internal override void Destroy()
	{
		Destroy(btArrange);
		Destroy(btUnselect);
		Destroy(btAutoArrange);
		base.Destroy();
	}

	protected override void UpdateMutiSelect(bool _multi)
	{

	}

	protected override long[] CBCreateItemIds(TypeFlag _type)
	{
		long[]  ids = GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.Interior);
		ItemSubType subType = ItemSubType.Interior;
		switch (_type)
		{
			case TypeFlag.Character:		    return GameCore.Instance.PlayerDataMgr.GetUnitIds();
			case TypeFlag.SetInterior:	        return ids;
			case TypeFlag.Furniture:		    subType = ItemSubType.Furniture;    break;
			case TypeFlag.Prop:					subType = ItemSubType.Prop;         break;
			case TypeFlag.Wall:					subType = ItemSubType.Wall;         break;
			case TypeFlag.Floor:				subType = ItemSubType.Floor;        break;
			default: return null;
		}

		List<long> id = new List<long>();
        
		for (int i = 0; i < ids.Length; ++i)
		{
			var data = GameCore.Instance.PlayerDataMgr.GetItemData(ids[i]);
			if (data.subType == subType)
				id.Add(ids[i]);
		}
		return id.ToArray();
	}

	protected override bool CBSetCard(TypeFlag _type, CardBase _card)
	{
		switch (_type)
		{
			case TypeFlag.Character:
				var sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(_card.ID);
				if (sdata.dormitory != 0)
					_card.SetState(CardBase.States.Room + sdata.dormitory); 
				else if (GameCore.Instance.PlayerDataMgr.IsArrange( sdata.uid) )
					_card.SetState(CardBase.States.Arrangement);
				else if (sdata.dispatch != false)
					_card.SetState(CardBase.States.Detachment);
				else
					_card.SetState(CardBase.States.Normal);
				break;

			case TypeFlag.SetInterior:
				var interior = GameCore.Instance.PlayerDataMgr.GetItemSData(_card.ID);
				if (interior != null)
				{
					var cnt = interior.count - interior.myRoomCount;
					_card.SetCount(cnt);

				}
				else // 미보유
				{
					_card.SetState(CardBase.States.NotHave);
				}
				break;
			case TypeFlag.Furniture:
			case TypeFlag.Prop:
			case TypeFlag.Wall:
			case TypeFlag.Floor:
				var item = GameCore.Instance.PlayerDataMgr.GetItemSData(_card.ID);
				if( item != null )
				{
					var cnt = item.count - item.myRoomCount;
					if (cnt <= 0)
						return false;// _card.gameObject.SetActive(false);
					else
						_card.SetCount(cnt);
				}
				break;
			default:
				break;
		}

		return true;
	}

	protected override void CBChangeTab(TypeFlag _type)
	{
		btAutoArrange.SetActive(_type == TypeFlag.SetInterior);
		btUnArrange.SetActive(_type == TypeFlag.Character);
	}

	protected override void CBItemClick(TypeFlag _type, long _id)
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        switch (_type)
		{
			case TypeFlag.Character:	    break;
			//case TypeFlag.SetInterior:
			case TypeFlag.Furniture:
			case TypeFlag.Prop:
			case TypeFlag.Wall:				break;
			case TypeFlag.Floor:			break;
			default:						break;
		}
	}

	protected override void CBItemPress(TypeFlag _type, long _id)
	{
		// do Nothing
	}

	protected override void CBClickExpendButton()
	{
		// do nothing
	}

	protected override void CBClickFilterButton()
	{
		// do nothing
	}
    protected override void CBClickAlignmentButtom()
    {
        // do nothing
    }

    internal void UpdateCard(long _uid)
	{
		var card = nowListRoot.GetItem(_uid);
		CBSetCard(nowListRoot.Type, card);
	}

	internal void UpdateNowListCard()
	{
		var it = nowListRoot.GetEnumerator();
		while(it.MoveNext())
			CBSetCard(nowListRoot.Type, it.Current.Value);
	}

	private int CBSortCharacter(Transform _1, Transform _2)
	{
		var c1 = _1.GetComponent<CardBase>();
		var c2 = _2.GetComponent<CardBase>();

		if (c1.State != c2.State)
			return c1.State < c2.State ? 1 : -1;

		var data1 = (UnitDataMap)c1.Data;
		var data2 = (UnitDataMap)c2.Data;

		if (data1.rank != data2.rank)
			return data1.rank > data2.rank ? 1 : -1;

		if ((data1.id - 1) % 10 != (data2.id - 1) % 10)
			return (data1.id - 1) % 10 < (data2.id - 1) % 10 ? 1 : -1;

		// 강화 -- 데이터 테이블이 없어서 패스

		// 전투력 -- 계산식이 없어서 패스

		return 0;
	}
}
