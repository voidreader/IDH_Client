using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class InvenSys : SubSysBase, ISequenceAction
{
	InvenUI ui;

	internal InvenSys() : base(SubSysType.Inven)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Item,
        };
    }

	protected override void EnterSysInner(ParaBase _para)
	{
		Name = "가방";

		ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/PanelInvenUI", GameCore.Instance.ui_root).GetComponent<InvenUI>();
		ui.Init();

        if(_para != null)
        {
            var para = _para.GetPara<StoryPara>();
            ui.CBSelectTab(para.storyKey);
        }
        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, ui.GetTutorialTransformList);
        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.InventoryScene);
        base.EnterSysInner(_para);
	}

	internal void CBASD()
	{
		Debug.Log("Done!");
		ui.GetSelectedIds();
		Debug.Log("Select First : " + ui.GetSelectedIds()[0]);
	}

	protected override void RegisterHandler()
	{
		handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
		handlerMap.Add(GameEventType.ANS_CHARACTER_SALE, ANS_CHARACTER_SALE); 
		handlerMap.Add(GameEventType.ANS_ITEM_SALE, ANS_ITEM_SALE);
		handlerMap.Add(GameEventType.ANS_ACCOUNT_CHARSLOT, ANS_ACCOUNT_CHAR_SLOT);
		handlerMap.Add(GameEventType.ANS_ACCOUNT_EQUIPSLOT, ANS_ACCOUNT_EQUIP_SLOT);
		
		base.RegisterHandler();
	}

	protected override void SetActive(bool _active)
	{
		base.SetActive(_active);
	}

	protected override void UnegisterHandler()
	{
		base.UnegisterHandler();
	}

	protected override void ExitSysInner(ParaBase _para)
	{
		ui.Destroy();
		ui = null;
		base.ExitSysInner(_para);

        GameCore.Instance.PlayerDataMgr.newHeroCardUidList.Clear();
        GameCore.Instance.PlayerDataMgr.newItemCardUidList.Clear();
	}


	private bool ANS_CHARACTER_SALE(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		if (code == 0)
		{
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Item_Get);

            ui.Unselect(); // 선택 해제
//			var ids = ui.GetSelectedIds();  // 선택된 모든 카드의 아이디 가져 오기
			var length = ui.cachedIds.Length;
			for (int i = 0; i < length; ++i)
			{
				long id = ui.cachedIds[i];
				GameCore.Instance.PlayerDataMgr.RemoveUnit(id);	// 데이터 삭제
				ui.RemoveCard(id);															// 카드 삭제
			}
            ui.ResetScrollInBound();

            GameCore.Instance.PlayerDataMgr.SetCardSData(para.GetField("GOLD"));
			GameCore.Instance.CommonSys.UpdateMoney();
			return true;
		}
		else
		{
			GameCore.Instance.ShowAlert("삭제 실패");
		}

		return false;
	}

	private bool ANS_ITEM_SALE(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
            case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Item_Get);

                ui.Unselect(); // 선택 해제
			    //var ids = ui.GetSelectedIds();  // 선택된 모든 카드의 아이디 가져 오기
			    var length = ui.cachedIds.Length;
			    for (int i = 0; i < length; ++i)
			    {
				    long id = ui.cachedIds[i];
				    var count = GameCore.Instance.PlayerDataMgr.RemoveItem(id, ui.cachedCount);
                    if (count <= 0)                     // 데이터 삭제
                        ui.RemoveCard(id);                // 카드 삭제
                    else
                    {
                        var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(id);
                        if (sdata.type == CardType.Interior)
                            ui.GetCard(id).SetCount(count-sdata.myRoomCount);   // 카운트 업데이트
                        else
                            ui.GetCard(id).SetCount(count);   // 카운트 업데이트
                    }
			    }
                ui.ResetScrollInBound();

                //GameCore.Instance.PlayerDataMgr.SetCardSData(para.GetField(""));

                GameCore.Instance.PlayerDataMgr.SetCardSData(para.GetField("GOLD"));
			    GameCore.Instance.CommonSys.UpdateMoney();
			    return true;

            case 1:
                GameCore.Instance.ShowAlert("삭제 실패");
                return true;
            case 2:
                string richText2 = "숙소에 배치된 아이템은 판매 할 수 없습니다.";
                GameCore.Instance.ShowAlert(richText2);
                return true;
        }

		return false;
	}

	private bool ANS_ACCOUNT_EQUIP_SLOT(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");
		json = json.GetField("DATA");

		if (code == 0)
		{
			int slotCnt = 0;
			json.GetField(ref slotCnt, "INVEN_SLOT");
			GameCore.Instance.PlayerDataMgr.LocalUserData.EquipItemSlotLimitCount = slotCnt;
			ui.UpdateCount();

			var common = GameCore.Instance.DataMgr.GetInventoryConstData();
			var count = common.defSlot;
			var slot = GameCore.Instance.PlayerDataMgr.EquipItemSlotLimitCount;
			for (int i = 0; i < 10; ++i)
			{
				count += common.expendSlotCount[i];
				if (count >= slot)
				{
					GameCore.Instance.PlayerDataMgr.SetCardSData(json.GetField("CASH"));
					//GameCore.Instance.PlayerDataMgr.Cash -= common.expendSlotCost[i];
					GameCore.Instance.CommonSys.UpdateMoney();
					break;
				}
			}
			GameCore.Instance.ShowAlert("확장 되었습니다.");
		}
		else if (code == 1)
		{
			GameCore.Instance.ShowAlert("재화가 부족합니다.");
		}
		else
		{
			GameCore.Instance.ShowAlert("실패");
		}
		return true;
	}

	private bool ANS_ACCOUNT_CHAR_SLOT(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");
		json = json.GetField("DATA");

		if (code == 0)
		{
			int slotCnt = 0;
			json.GetField(ref slotCnt, "CHARACTER_SLOT");
			GameCore.Instance.PlayerDataMgr.LocalUserData.HeroSlotLimitCount = slotCnt;
			ui.UpdateCount();

			var common = GameCore.Instance.DataMgr.GetCharacterConstData();
			var count = common.defSlot;
			var slot = GameCore.Instance.PlayerDataMgr.HeroSlotLimitCount;
			for (int i = 0; i < 10; ++i)
			{
				count += common.expendSlotCount[i];
				if (count >= slot)
				{
					GameCore.Instance.PlayerDataMgr.SetCardSData(json.GetField("CASH"));
					//GameCore.Instance.PlayerDataMgr.Cash -= common.expendSlotCost[i];
					GameCore.Instance.CommonSys.UpdateMoney();
					break;
				}
			}
			GameCore.Instance.ShowAlert("확장 되었습니다.");
		}
		else if (code == 1)
		{
			GameCore.Instance.ShowAlert("재화가 부족합니다.");
		}
		else
		{
			GameCore.Instance.ShowAlert("실패");
		}
		return true;
	}

    public void ResetUI()
    {
        ui.Init(true);
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        return nActionList;
    }
}
