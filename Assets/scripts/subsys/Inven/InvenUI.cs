using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class InvenUI : InvenBase, ISequenceTransform
{
	GameObject btSell;
	GameObject btUnselect;

    GameObject[] btAlignmentType;
    UILabel[] alignmentTypeText;                        //정렬 타입 텍스트.
    UILabel[] alignmentIconText;                        //정렬 방식 텍스트.

    public long[] cachedIds { get; protected set; }     // 선택된 카드 아이디들.
	public int cachedCount { get; protected set; }

    private List<Collider2D> ColliderList = new List<Collider2D>();

    private Transform tutorialInvenData;

    internal Transform GetCardTransform()
    {
        var list = listRoots[0];
        foreach (var trData in list.tfTable)
        {
            //if (i >= list.GetCount() - 1)
            tutorialInvenData = trData.Key;
            return tutorialInvenData;
        }
        Debug.LogError("Can't Find CardData Transform in Tutorial");
        return null;
    }

    internal void SetOnClickAction(Action returnTutorialData)
    {
        listRoots[0].saveAction.onClickAction += returnTutorialData;
    }
    internal void SetOnPressAction(Action returnTutorialData)
    {
        listRoots[0].saveAction.onPressAction += returnTutorialData;
    }

    internal void Init(bool _reset = false)
	{
		// bottom

		//GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.AddToBottom, AddToBottomPara.Add(true, multiSelect.gameObject)));

		//btUnselect = GameCore.Instance.CreateButton("모든 선택 취소", 22, "BTN_05_01_0", () => Unselect());
		//GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.AddToBottom, AddToBottomPara.Add(false, btUnselect)));

		//btSell = GameCore.Instance.CreateButton("판매", 22, "BTN_02_01_0", () => CBSell(nowListRoot.GetSelects()));
		//GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.AddToBottom, AddToBottomPara.Add(false, btSell)));

		btUnselect = UnityCommonFunc.GetGameObjectByName(gameObject, "btUnSelect");
		btUnselect.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            Unselect(); }));
		btSell = UnityCommonFunc.GetGameObjectByName(gameObject, "btSell");
		btSell.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button); 
            CBSell(nowListRoot.GetSelects()); }));
        alignmentBox = UnityCommonFunc.GetGameObjectByName(gameObject, "AlignmentBox");

        TypeFlag flags = TypeFlag.Character |
//							TypeFlag.SetInterior |
							TypeFlag.Equip |
							TypeFlag.Resource |
							TypeFlag.Interior |
							TypeFlag.GiftBox;
		Init(flags,true, _reset);

		// Set Sort Callback
		listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByRankDescending);
		listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByRankDescending);

		// set Filter Callback
		listRootTable[TypeFlag.Character].SetFilterCallBack(CBDoFilterCharater);
		listRootTable[TypeFlag.Equip].SetFilterCallBack(CBDoFilterItem);

        if (btAlignmentType == null)
        {
            btAlignmentType = new GameObject[8];

            btAlignmentType[0] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardRankDown");
            btAlignmentType[1] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardRankUp");
            btAlignmentType[2] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardNameDown");
            btAlignmentType[3] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardNameUp");
            btAlignmentType[4] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "GetCardDown");
            btAlignmentType[5] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "GetCardUp");
            btAlignmentType[6] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardPowerDown");
            btAlignmentType[7] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardPowerUp");

            btAlignmentType[0].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByRankDescending));
            btAlignmentType[1].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByRankAscending));
            btAlignmentType[2].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByNameDescending));
            btAlignmentType[3].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByNameAscending));
            btAlignmentType[4].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByGetDescending));
            btAlignmentType[5].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByGetAscending));
            btAlignmentType[6].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByPowerDescending));
            btAlignmentType[7].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByPowerAscending));
        }

        alignmentTypeText = new UILabel[8];
        alignmentIconText = new UILabel[8];
        for(int i = 0; i < btAlignmentType.Length; ++i)
        {
            alignmentTypeText[i] = btAlignmentType[i].GetComponentsInChildren<UILabel>()[0];
            alignmentIconText[i] = btAlignmentType[i].GetComponentsInChildren<UILabel>()[1];
        }
        alignmentTypeText[0].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        alignmentIconText[0].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        var Colls = alignmentBox.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < Colls.Length; ++i)
        {
            ColliderList.Add(Colls[i]);
        }
        var btnAlignmentColls = btnAlignment.GetComponent<Collider2D>();
        ColliderList.Add(btnAlignmentColls);
    }

	internal override void Destroy()
	{
		Destroy(btSell);
		Destroy(btUnselect);
		base.Destroy();
	}

	protected override void UpdateMutiSelect(bool _multi)
	{
		btSell.SetActive(_multi);
		btUnselect.SetActive(_multi);
	}

	protected override long[] CBCreateItemIds(TypeFlag _type)
	{
		List<long> ids = new List<long>();
		switch (_type)
		{
			case TypeFlag.Character:	return GameCore.Instance.PlayerDataMgr.GetUnitIds();
			case TypeFlag.Equip:		return GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.Equipment);
			case TypeFlag.Resource:		return GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.resource);
			case TypeFlag.SetInterior:
			case TypeFlag.Interior:		return GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.Interior);
			case TypeFlag.GiftBox:		return GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.GiftBox);
			default:					return null;
		}
	}
    
	protected override bool CBSetCard(TypeFlag _type, CardBase _card)
	{
		if (_card.Data == null)
			return false;

		switch (_type)
		{
			case TypeFlag.Character:
				var sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(_card.ID);

                if (!sdata.IsExpCard())
                {
                    if (sdata.GetEquipCount() != 0) _card.SetInfo(CardBase.UnitInfo.Power | CardBase.UnitInfo.Equip);
                    else                            _card.SetInfo(CardBase.UnitInfo.Power);
                }

                if (GameCore.Instance.PlayerDataMgr.MainCharacterUID == sdata.uid)
                    _card.SetState(CardBase.States.MainCharacter);
                else if (GameCore.Instance.PlayerDataMgr.IsArrange(sdata.uid))
					_card.SetState(CardBase.States.Arrangement);
				else if (sdata.dispatch != false)
					_card.SetState(CardBase.States.Detachment);
				else if (sdata.dormitory != 0)
					_card.SetState(CardBase.States.Room + sdata.dormitory);
				else
					_card.SetState(CardBase.States.Normal);
				break;

			case TypeFlag.Equip:
				var itemSData = (ItemSData)GameCore.Instance.PlayerDataMgr.GetItemSData(_card.ID);
				if (itemSData.equipHeroUID > 0)
                    _card.SetState(CardBase.States.Arrangement);

				break;
			case TypeFlag.Resource:
				break;
			case TypeFlag.Interior:
				break;
			case TypeFlag.GiftBox:
				break;
			default:
				break;
		}

		return true;
	}

	protected override void CBItemClick(TypeFlag _type, long _id)
	{
        //GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
		switch (_type)
		{
			case TypeFlag.Character:    ShowHeroInfo(_id);           break;
			case TypeFlag.Equip:	    ShowEquipmentItemInfo(_id);  break;
			case TypeFlag.Resource:     ShowResourceItemInfo(_id);   break;
			case TypeFlag.Interior:	    ShowInteriorItemInfo(_id);   break;
			case TypeFlag.GiftBox:	                                 break;
			default:								                 break;
		}
		
	}

	protected override void CBItemPress(TypeFlag _type, long _id)
	{
		// nothing
	}

	protected override void CBChangeTab(TypeFlag _type)
	{
        ClearAlignment();
    }

	protected override void CBClickExpendButton()
	{
        var type = nowListRoot.Type;
		if(type == TypeFlag.Character)
		{
			var commonData = GameCore.Instance.DataMgr.GetCharacterConstData();
			var nowCount = GameCore.Instance.PlayerDataMgr.HeroSlotLimitCount;
			int accCount = commonData.defSlot;
			for (int i = 0 ; i < commonData.expendSlotCount.Length; ++i )
			{
				accCount += commonData.expendSlotCount[i];
				if (nowCount >= accCount)
					continue;
                var TotalCount = "[C][8000FFFF]" + nowCount + " SLOT -> " + (nowCount + commonData.expendSlotCount[i]) + " SLOT[-][/C]";
                string hl = string.Format("{0:N0}", commonData.expendSlotCost[i]);
                string str = "다음 재화를 사용하여 영웅 슬롯을 확장하시겠습니까? \n" + TotalCount;
                GameCore.Instance.ShowAgree("캐릭터 슬롯 확장", str, hl, MoneyType.Pearl, 0, () =>
				{
					GameCore.Instance.NetMgr.Req_Update_Character_Slot(commonData.expendSlotCost[i], commonData.expendSlotCount[i]);
					GameCore.Instance.CloseMsgWindow();
				});
				return;
			}
			GameCore.Instance.ShowNotice("슬롯 확장", "최대 슬롯에 도달하였습니다.", 0);
			return;
		}
		else if (type == TypeFlag.Equip)
		{
			var commonData = GameCore.Instance.DataMgr.GetInventoryConstData();
			var nowCount = GameCore.Instance.PlayerDataMgr.EquipItemSlotLimitCount;
            
            int accCount = commonData.defSlot;
			for (int i = 0; i < commonData.expendSlotCount.Length; ++i)
			{
				accCount += commonData.expendSlotCount[i];
				if (nowCount >= accCount)
					continue;
                var TotalCount = "[C][8000FFFF]" + nowCount + " SLOT -> " + (nowCount + commonData.expendSlotCount[i]) + " SLOT[-][/C]";
                string hl = string.Format("{0:N0}", commonData.expendSlotCost[i]);
                string str = "다음 재화를 사용하여 장비 슬롯을 확장하시겠습니까? \n" + TotalCount;
                GameCore.Instance.ShowAgree("장비 슬롯 확장", str, hl, MoneyType.Pearl, 0, () =>
				{
					GameCore.Instance.NetMgr.Req_Update_Item_Slot();
					GameCore.Instance.CloseMsgWindow();
				});

				return;
			}
			GameCore.Instance.ShowNotice("슬롯 확장", "최대 슬롯에 도달하였습니다.", 0);
			return;
		}
		else
		{
			// nothing
		}
	}


    /// <summary>
    /// 상세보기 관련 팝업 함수
    /// </summary>
    /// <param name="_id"></param>
	private void ShowHeroInfo(long _id)
	{
		var obj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/Card_Info", transform);
		var info = obj.GetComponent<CardInfoScript>();
		info.Init(GameCore.Instance.PlayerDataMgr.GetUnitSData(_id), true);
        var data = GameCore.Instance.PlayerDataMgr.GetUnitData(_id);
        // 알림창
        if (data.IsExpCard())
        {
            GameCore.Instance.ShowObject("상세보기", null, obj, 0, new MsgAlertBtnData[] {
                new MsgAlertBtnData("닫기", new EventDelegate(()=> {
                    GameCore.Instance.CloseMsgWindow(); Unselect(); })),
                new MsgAlertBtnData("판매", new EventDelegate(()=>{
                    CBSell(new long[] { _id });}))
            });
        }
        else
        {
            GameCore.Instance.ShowObject("상세보기", null, obj, 0, new MsgAlertBtnData[] {
                new MsgAlertBtnData("닫기", new EventDelegate(()=> {
                    GameCore.Instance.CloseMsgWindow(); Unselect(); })),
                new MsgAlertBtnData("영웅 관리", new EventDelegate(()=> {
                    GameCore.Instance.CloseMsgWindow();
                    GameCore.Instance.ChangeSubSystem(SubSysType.HeroInfo, new HeroInfoPara(_id, SubSysType.Inven));
                })),
                new MsgAlertBtnData("판매", new EventDelegate(()=>{
                    CBSell(new long[] { _id });}))
            });
        }
	}

	private void ShowEquipmentItemInfo(long _id)
	{
		var obj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/Card_Info", transform);
		var info = obj.GetComponent<CardInfoScript>();
		info.Init(GameCore.Instance.PlayerDataMgr.GetItemSData(_id), true);

		// 알림창
		GameCore.Instance.ShowObject("상세보기", null, obj, 0, new MsgAlertBtnData[] {
			new MsgAlertBtnData("닫기", new EventDelegate(()=> {
                GameCore.Instance.CloseMsgWindow(); Unselect(); })),
			new MsgAlertBtnData("강화", new EventDelegate(()=> {
                GameCore.Instance.CloseMsgWindow();
                GameCore.Instance.ChangeSubSystem(SubSysType.ItemUp, new HeroInfoPara(_id, SubSysType.Inven));
            })),
			new MsgAlertBtnData("판매", new EventDelegate(()=>{
                CBSell(new long[] { _id }); }))
		});
	}

	private void ShowInteriorItemInfo(long _id)
	{
		var obj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/Card_Info", transform);
		var info = obj.GetComponent<CardInfoScript>();
		info.Init(GameCore.Instance.PlayerDataMgr.GetItemSData(_id), true);
		// 알림창
		GameCore.Instance.ShowObject("상세보기", null, obj, 0, new MsgAlertBtnData[] {
			new MsgAlertBtnData("닫기", new EventDelegate(()=> {
                GameCore.Instance.CloseMsgWindow(); Unselect(); })),
			new MsgAlertBtnData("판매", new EventDelegate(()=>{
                CBSell(new long[] { _id },info.GetCount() ); }),
                info.maxCount != 0)
		});
	}

	private void ShowResourceItemInfo(long _id)
	{
		var obj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/Card_Info", transform);
		var info = obj.GetComponent<CardInfoScript>();
		info.Init(GameCore.Instance.PlayerDataMgr.GetItemSData(_id), true);

		// 알림창
		GameCore.Instance.ShowObject("상세보기", null, obj, 0, new MsgAlertBtnData[] {
			new MsgAlertBtnData("닫기", new EventDelegate(()=> {
                GameCore.Instance.CloseMsgWindow(); Unselect(); }))
		});
	}

	private void ShowGiftBoxInfo(long _id)
	{
		var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(_id);
		var data = GameCore.Instance.DataMgr.GetItemData(sdata.key);
		var obj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/Card_Info", transform);
		var info = obj.GetComponent<CardInfoScript>();
		info.Init(GameCore.Instance.PlayerDataMgr.GetItemSData(_id), true);

		// 알림창
		if (sdata.count < 10)
		{
			GameCore.Instance.ShowObject("상세보기", null, obj, 0, new MsgAlertBtnData[] {
				new MsgAlertBtnData("닫기", new EventDelegate(()=>{
                    GameCore.Instance.CloseMsgWindow(); })),
				new MsgAlertBtnData("열기", new EventDelegate(()=>{
                    GameCore.Instance.ShowAlert("미구현"); }))
			});
		}
		else
		{
			GameCore.Instance.ShowObject("상세보기", null, obj, 0, new MsgAlertBtnData[] {
				new MsgAlertBtnData("닫기", new EventDelegate(()=>{
                    GameCore.Instance.CloseMsgWindow(); })),
				new MsgAlertBtnData("10개 열기", new EventDelegate(()=>{
                    GameCore.Instance.ShowAlert("미구현"); })),
				new MsgAlertBtnData("열기", new EventDelegate(()=>{
                    GameCore.Instance.ShowAlert("미구현"); }))
			});
		}
	}


	/// <summary>
	/// 판매 시도
	/// </summary>
	/// <param name="_ids">판매할 항목의 UID 리스트</param>
	/// <param name="_count"> 0이 아니라면 _ids[0]만을 _count개 판매를 시도한다.</param>
	private void CBSell(long[] _ids, int _count = 0)
	{
		if (_ids.Length == 0)
			return;

		if (_count != 0)
			SellInterior(_ids[0], _count);
		else if (nowListRoot.Type == TypeFlag.Character)
			SellCharater(_ids);
		else
			SellItem(_ids);
	}

	private bool CheckSellableCharacter(long[] _ids)
	{
		var typical = GameCore.Instance.PlayerDataMgr.GetUnitSData(GameCore.Instance.PlayerDataMgr.MainCharacterUID).uid;
		string str = null;
		for(int i = 0; i< _ids.Length; ++i)
		{
			if (typical == _ids[i])
			{
				str = "대표 영웅은 판매할 수 없습니다.";
				break;
			}

			var data = GameCore.Instance.PlayerDataMgr.GetUnitSData(_ids[i]);
			if(GameCore.Instance.PlayerDataMgr.IsArrange(data.uid) )
			{
				str = "배치된 영웅은 판매 할 수 없습니다.";
				break;
			}
			if(data.dispatch == true)
			{
				str = "파견된 영웅은 판매 할 수 없습니다.";
				break;
			}
			if(data.dormitory != 0)
			{
				str = "숙소에 배치된 영웅은 판매 할 수 없습니다.";
				break;
			}
			
		}

		if (str != null)
		{
			GameCore.Instance.ShowNotice("판매 불가", str, 0);
			return false;
		}
		else
			return true;

	}

	private void CBSellCharacterInner(long[] _ids)
	{
		cachedIds = _ids;
		cachedCount = 1;
        bool isHeroEquipCheck = false;
		Debug.Log(_ids + " 영웅 카드 삭제");
		GameCore.Instance.NetMgr.Req_Sale_Character(_ids);
        for (int i = 0; i < _ids.Length; ++i)
        {
            if (GameCore.Instance.PlayerDataMgr.GetUnitSData(_ids[i]).GetEquipCount() != 0) isHeroEquipCheck = true;
        }
        if (isHeroEquipCheck) GameCore.Instance.ShowAlert("판매된 캐릭터가 착용하고 있던 장비는 가방으로 이동합니다.");
		GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, null));
	}

	private bool CheckSellableItem(long[] _ids)
	{
		string str = null;
		for (int i = 0; i < _ids.Length; ++i)
		{
			var data = GameCore.Instance.PlayerDataMgr.GetItemSData(_ids[i]);
			if (0 < data.equipHeroUID)
			{
				str = "장착된 아이템은 판매 할 수 없습니다.";
				break;
			}
		}

		if (str != null)
		{
			GameCore.Instance.ShowNotice("판매 불가", str, 0);
			return false;
		}
		else
			return true;
	}

    private void CBSellInteriorInner(long _id, int _count)
    {
        cachedIds = new long[] { _id };
        cachedCount = _count;

        GameCore.Instance.NetMgr.Req_Sell_Item(_id, _count);
        GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, null));
    }


    private void SellItem(long[] _ids)
	{
		if (!CheckSellableItem(_ids))
			return;

		int sellValue = 0;
		string str = null;
		if (_ids.Length == 1)
		{
			var item = GameCore.Instance.PlayerDataMgr.GetItemData(_ids[0]);
			sellValue = item.sellValue;

			str = string.Format("선택된 [c][8000FFFF]{0}[-][/c]를 판매 하시겠습니까?", item.name);
		}
		else // 여러개일 경우
		{
			for (int i = 0; i < _ids.Length; i++)
				sellValue += GameCore.Instance.PlayerDataMgr.GetItemData(_ids[i]).sellValue;

			str = string.Format("선택된 [c][8000FFFF]{0}개의 아이템[-][/c]을 판매하시겠습니까?", _ids.Length);
		}
		string hl = string.Format("{0:N0}", sellValue);

		GameCore.Instance.ShowAgree("아이템 판매", str, hl, MoneyType.Gold, 0, () => {
			CBSellItemInner(_ids);
			GameCore.Instance.CloseMsgWindow();
		});
	}

    private void SellCharater(long[] _ids)
    {
        if (!CheckSellableCharacter(_ids))
            return;

        int sellValue = 0;
        string str = "";
        if (_ids.Length == 1)
        {
            var unit = GameCore.Instance.PlayerDataMgr.GetUnitData(_ids[0]);
            sellValue = unit.sellValue;

            str = String.Format("선택된 [c][8000FFFF]{0}[-][/c]를 판매 하시겠습니까?", unit.name);
        }
        else // 여러개일 경우
        {
            for (int i = 0; i < _ids.Length; i++)
                sellValue += GameCore.Instance.PlayerDataMgr.GetUnitData(_ids[i]).sellValue;

            str = string.Format("선택된 [c][8000FFFF]{0}개의 영웅[-][/c]을 판매하시겠습니까?", _ids.Length);
        }

        string hl = string.Format("{0:N0}", sellValue);

        GameCore.Instance.ShowAgree(" 영웅 판매", str, hl, MoneyType.Gold, 0, () => {
            CBSellCharacterInner(_ids);
            GameCore.Instance.CloseMsgWindow();
        });
    }

    private void SellInterior(long _id, int _count)
    {
        if (!CheckSellableInterior(_id))
            return;

        var udata = GameCore.Instance.PlayerDataMgr.GetItemSData(_id);
        var item = GameCore.Instance.PlayerDataMgr.GetItemData(_id);
        var count = Mathf.Clamp(_count, 1, udata.count);
        int sellValue = item.sellValue * count;

        string str = String.Format("선택된 [c][8000FFFF]{0}[-][/c]를 [-][/c]{1}[-][/c]개 판매 하시겠습니까?", item.name, count);
        string hl = string.Format("{0:N0}", sellValue);

        GameCore.Instance.ShowAgree("아이템 판매", str, hl, MoneyType.Gold, 0, () => {
            CBSellInteriorInner(_id, count);
            GameCore.Instance.CloseMsgWindow();
        });
    }

    private void CBSellItemInner(long[] _ids)
	{
		cachedIds = _ids;
		cachedCount = 1;
		Debug.Log(_ids + " 아이템 카드 삭제");
		GameCore.Instance.NetMgr.Req_Sell_Item(_ids);
		GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, null));
	}

	private bool CheckSellableInterior(long _ids)
	{
		//GameCore.Instance.ShowAlert("인테리어 판매 검사 루틴 추가 필요");
		return true;
	}



	protected override void CBClickFilterButton()
	{
        GameObject filterObj;
		if (nowListRoot.Type == TypeFlag.Character)
		{
			if (filterObj_Char == null)
				filterObj_Char = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("CommonRsc/prefab/filter_root", transform);
			filterObj = filterObj_Char;
		}
		else
		{
			if (filterObj_Item == null)
				filterObj_Item = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("CommonRsc/prefab/filter_root_item", transform);
			filterObj = filterObj_Item;
		}
		filterObj.GetComponent<FilterCtrl>().SetPrevFilter();

        GameCore.Instance.ShowObject("필터", null, filterObj, 2, new MsgAlertBtnData[] {
            new MsgAlertBtnData("완료", new EventDelegate(() => {
                var ctrl = filterObj.GetComponent<FilterCtrl>();
                int filter = ctrl.GetFilter();
                ShowResetFilter(filter != 0);
                nowListRoot.DoFilter(filter);
                filterObj.transform.parent = transform;
                filterObj.SetActive(false);
                GameCore.Instance.CloseMsgWindow();
            }), true, null, SFX.Sfx_UI_Confirm)
		});
	}

    internal bool CBDoFilterCharater(CardBase _hero, int _filter)
    {
        // Filtering
        int rank = FilterCtrl.GetFilterRank(_filter);
        int type = FilterCtrl.GetFilterType(_filter);
        int rare = FilterCtrl.GetFilterRare(_filter);

        var data = (UnitDataMap)_hero.Data;
        return (rank == 0 || (1 << (4 - data.rank) & rank) != 0) &&
                        (type == 0 || (1 << (data.charType - 1) & type) != 0) &&
                        (rare == 0 || (1 << (data.evolLvl - 1) & rare) != 0);
    }

    internal bool CBDoFilterItem(CardBase _item, int _filter)
    {
        // Filtering
        int rank = FilterCtrl.GetFilterRank(_filter);
        int type = FilterCtrl.GetFilterType(_filter);
        int rare = FilterCtrl.GetFilterRare(_filter);

        var data = (ItemDataMap)_item.Data;
        return (rank == 0 || (1 << (4 - data.rank) & rank) != 0) &&
                        (type == 0 || (1 << (data.equipLimit - 1) & type) != 0);
    }


    #region 정렬 메서드

    protected override void CBClickAlignmentButtom()
    {
        if (!alignmentBox.activeSelf) alignmentBox.SetActive(true);
        else alignmentBox.SetActive(false);
    }

    //랭크기준 내림차순 정렬
    public void OnClickSortByRankDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        UpdateAlignmentButton(0);

        if (nowListRoot.Type == TypeFlag.Character)
        {
            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByRankDescending);
            if (filterObj_Char == null) listRootTable[TypeFlag.Character].DoFilter(31);
            else listRootTable[TypeFlag.Character].DoFilter(filterObj_Char.GetComponent<FilterCtrl>().GetFilter());

        }
        else if(nowListRoot.Type == TypeFlag.Equip)
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByRankDescending);
            if (filterObj_Item == null) listRootTable[TypeFlag.Equip].DoFilter(31);
            else listRootTable[TypeFlag.Equip].DoFilter(filterObj_Item.GetComponent<FilterCtrl>().GetFilter());
        }

        alignmentBox.gameObject.SetActive(false);
    }

    //랭크기준 오름차순 정렬
    public void OnClickSortByRankAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        UpdateAlignmentButton(1);

        if (nowListRoot.Type == TypeFlag.Character)
        {
            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByRankAscending);
            if (filterObj_Char == null) listRootTable[TypeFlag.Character].DoFilter(31);
            else listRootTable[TypeFlag.Character].DoFilter(filterObj_Char.GetComponent<FilterCtrl>().GetFilter());
        }
        else if (nowListRoot.Type == TypeFlag.Equip)
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByRankAscending);
            if (filterObj_Item == null) listRootTable[TypeFlag.Equip].DoFilter(31);
            else listRootTable[TypeFlag.Equip].DoFilter(filterObj_Item.GetComponent<FilterCtrl>().GetFilter());
        }

        alignmentBox.gameObject.SetActive(false);
    }

    //이름기준 내림차순 정렬
    public void OnClickSortByNameDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        UpdateAlignmentButton(2);

        if (nowListRoot.Type == TypeFlag.Character)
        {
            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByNameDescending);
            if (filterObj_Char == null) listRootTable[TypeFlag.Character].DoFilter(31);
            else listRootTable[TypeFlag.Character].DoFilter(filterObj_Char.GetComponent<FilterCtrl>().GetFilter());
        }
        else if (nowListRoot.Type == TypeFlag.Equip)
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByNameDescending);
            if (filterObj_Item == null) listRootTable[TypeFlag.Equip].DoFilter(31);
            else listRootTable[TypeFlag.Equip].DoFilter(filterObj_Item.GetComponent<FilterCtrl>().GetFilter());
        }

        alignmentBox.gameObject.SetActive(false);
    }

    //이름기준 오름차순 정렬
    public void OnClickSortByNameAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        UpdateAlignmentButton(3);

        if (nowListRoot.Type == TypeFlag.Character)
        {
            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByNameAscending);
            if (filterObj_Char == null) listRootTable[TypeFlag.Character].DoFilter(31);
            else listRootTable[TypeFlag.Character].DoFilter(filterObj_Char.GetComponent<FilterCtrl>().GetFilter());
        }
        else if (nowListRoot.Type == TypeFlag.Equip)
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByNameAscending);
            if (filterObj_Item == null) listRootTable[TypeFlag.Equip].DoFilter(31);
            else listRootTable[TypeFlag.Equip].DoFilter(filterObj_Item.GetComponent<FilterCtrl>().GetFilter());
        }

        alignmentBox.gameObject.SetActive(false);
    }

    //획득기준 내림차순 정렬
    public void OnClickSortByGetDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        UpdateAlignmentButton(4);

        if (nowListRoot.Type == TypeFlag.Character)
        {
            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByGetDescending);
            if (filterObj_Char == null) listRootTable[TypeFlag.Character].DoFilter(31);
            else listRootTable[TypeFlag.Character].DoFilter(filterObj_Char.GetComponent<FilterCtrl>().GetFilter());
        }
        else if (nowListRoot.Type == TypeFlag.Equip)
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByGetDescending);
            if (filterObj_Item == null) listRootTable[TypeFlag.Equip].DoFilter(31);
            else listRootTable[TypeFlag.Equip].DoFilter(filterObj_Item.GetComponent<FilterCtrl>().GetFilter());
        }

        alignmentBox.gameObject.SetActive(false);
    }

    //획득기준 오름차순 정렬
    public void OnClickSortByGetAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        UpdateAlignmentButton(5);

        if (nowListRoot.Type == TypeFlag.Character)
        {

            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByGetAscending);
            if (filterObj_Char == null) listRootTable[TypeFlag.Character].DoFilter(31);
            else listRootTable[TypeFlag.Character].DoFilter(filterObj_Char.GetComponent<FilterCtrl>().GetFilter());
        }
        else if (nowListRoot.Type == TypeFlag.Equip)
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByGetAscending);
            if (filterObj_Item == null) listRootTable[TypeFlag.Equip].DoFilter(31);
            else listRootTable[TypeFlag.Equip].DoFilter(filterObj_Item.GetComponent<FilterCtrl>().GetFilter());
        }

        alignmentBox.gameObject.SetActive(false);
    }

    //공격력 내림차순 정렬
    public void OnClickSortByPowerDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        UpdateAlignmentButton(6);

        if (nowListRoot.Type == TypeFlag.Character)
        {
            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByPowerDescending);
            if (filterObj_Char == null) listRootTable[TypeFlag.Character].DoFilter(31);
            else listRootTable[TypeFlag.Character].DoFilter(filterObj_Char.GetComponent<FilterCtrl>().GetFilter());
        }
        else if (nowListRoot.Type == TypeFlag.Equip)
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByPowerDescending);
            if (filterObj_Item == null) listRootTable[TypeFlag.Equip].DoFilter(31);
            else listRootTable[TypeFlag.Equip].DoFilter(filterObj_Item.GetComponent<FilterCtrl>().GetFilter());
        }

        alignmentBox.gameObject.SetActive(false);
    }

    //공격력 오름차순 정렬
    public void OnClickSortByPowerAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        UpdateAlignmentButton(7);

        if (nowListRoot.Type == TypeFlag.Character)
        {
            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByPowerAscending);
            if (filterObj_Char == null) listRootTable[TypeFlag.Character].DoFilter(31);
            else listRootTable[TypeFlag.Character].DoFilter(filterObj_Char.GetComponent<FilterCtrl>().GetFilter());
        }
        else if (nowListRoot.Type == TypeFlag.Equip)
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByPowerAscending);
            if (filterObj_Item == null) listRootTable[TypeFlag.Equip].DoFilter(31);
            else listRootTable[TypeFlag.Equip].DoFilter(filterObj_Item.GetComponent<FilterCtrl>().GetFilter());
        }

        alignmentBox.gameObject.SetActive(false);
    }

    public void ClearAlignment()
    {
        
        btnAlignment.GetComponentsInChildren<UILabel>()[0].text = "등급";
        btnAlignment.GetComponentsInChildren<UILabel>()[1].text = "▼";

        if (btAlignmentType != null)
        {
            for (int i = 0; i < btAlignmentType.Length; ++i)
            {
                alignmentIconText[i].color = Color.white;
                alignmentTypeText[i].color = Color.white;
            }
            alignmentTypeText[0].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
            alignmentIconText[0].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        }
        if (nowListRoot.Type == TypeFlag.Character)
        {
            listRootTable[TypeFlag.Character].SetSortCallBack(InvenBase.HeroSortByRankDescending);
            listRootTable[TypeFlag.Character].DoFilter(31);
        }
        else
        {
            listRootTable[TypeFlag.Equip].SetSortCallBack(InvenBase.ItemSortByRankDescending);
            listRootTable[TypeFlag.Equip].DoFilter(31);
        }
    }

    internal void UpdateAlignmentButton(int selectAlignmentType)
    {
        btnAlignment.GetComponentsInChildren<UILabel>()[0].text = alignmentTypeText[selectAlignmentType].text;
        btnAlignment.GetComponentsInChildren<UILabel>()[1].text = alignmentIconText[selectAlignmentType].text;

        for (int i = 0; i < btAlignmentType.Length; ++i)
        {
            if (i == selectAlignmentType)
            {
                alignmentTypeText[i].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
                alignmentIconText[i].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
            }
            else
            {
                alignmentIconText[i].color = Color.white;
                alignmentTypeText[i].color = Color.white;
            }
        }
    }

    #endregion


    private void Update()
    {
#if UNITY_EDITOR

        if (Input.GetMouseButtonDown(0))
        {
            if (alignmentBox.activeInHierarchy)
            {
            
                Ray ray = GameCore.Instance.GetUICam().ScreenPointToRay(Input.mousePosition);
                RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 99999, LayerMask.GetMask("UI"));
                
                
                
                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; ++i)
                    {
                        if (ColliderList.Contains(hits[i].collider)) return;
                    }
                }
                alignmentBox.SetActive(false);
            }
        }
#endif
        if (Input.touchCount == 0) return;

        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (alignmentBox.activeInHierarchy)
            {

                Ray ray = GameCore.Instance.GetUICam().ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 99999, LayerMask.GetMask("UI"));



                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; ++i)
                    {
                        if (ColliderList.Contains(hits[i].collider)) return;
                    }
                }
                alignmentBox.SetActive(false);
            }
        }
    }

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 6:
            case 7:
                nTutorialList.Add(new ReturnTutorialData(transform, 4));
                //nTutorialList.Add(new ReturnTutorialData(transform, 4));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
