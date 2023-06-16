using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class FarmingSys : SubSysBase, ISequenceAction
{
	FarmingUI ui;
	List<long> unitUids = new List<long>();
    Dictionary<int, List<long>> FarmingTypeUnits = new Dictionary<int, List<long>>();
    
    internal FarmingSys() : base(SubSysType.Farming)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Farming,
            InitDataType.Story,
            InitDataType.Character,
        };
    }

    bool popUpCheck = false;

	protected override void EnterSysInner(ParaBase _para)
	{
		Name = "파밍";

		//GameCore.Instance.NetMgr.Req_Current_Time();

		ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Farming/PanelFarmingUI",GameCore.Instance.ui_root).GetComponent<FarmingUI>();
		ui.Init(0, CBClickBtnManage);
        //GameCore.Instance.SetTutorialChild(false, 12, GetTutorialActionList, ui.GetTutorialTransformList);
        SetActive(true);
        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.FarmingScene);

        AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.Farming, 0, 1);

        base.EnterSysInner(_para);
	}

	protected override void RegisterHandler()
	{
		if(handlerMap == null)
		{
			handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
			handlerMap.Add(GameEventType.ANS_FARMING_CREATE, ANS_FARMING_CREATE);
			handlerMap.Add(GameEventType.ANS_FARMING_DONE, ANS_FARMING_DONE);
			handlerMap.Add(GameEventType.ANS_FARMING_CANCEL, ANS_FARMING_CANSEL);
		}

		base.RegisterHandler();
	}

	protected override void ExitSysInner(ParaBase _para)
	{
		base.ExitSysInner(_para);

		SetActive(false);

		GameObject.Destroy(ui.gameObject);
	}

    private void CBClickBtnManage(long _id)
    {
        Debug.Log("Manage " + _id);
        GameCore.Instance.ChangeSubSystem(SubSysType.HeroInfo, new HeroInfoPara(_id, SubSysType.Farming));
    }

    internal override void UpdateUI()
	{
		ui.UpdateTime();
	}

	internal bool ANS_FARMING_CREATE(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		var code = -1;
		json.GetField(ref code, "result");
        switch (code)
		{
			case 0:
				GameCore.Instance.PlayerDataMgr.AddFarmingData(json.GetField("FARMING"));
				GameCore.Instance.PlayerDataMgr.SetUnitSData(json.GetField("CHA_LIST"));
                // GameCore.Instance.PlayerDataMgr.SetItemSData(json.GetField("CHA_LIST"));

                //var j = json.GetField("CHA_LIST");
                //for (int i = 0; i < j.Count; ++i)
                //{
                //    int uid = 0;
                //    j[i].GetField(ref uid, "CHA_UID");

                //    var sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(uid);
                //    var data = GameCore.Instance.PlayerDataMgr.GetUnitData(uid);
                //    var stat = sdata.GetStat();
                //    var itemStat = stat.AddStat(sdata.GetItemStat(stat));
                //    for (int k = 1; k < (int)UnitStat.Count; ++k)
                //        Debug.Log(data.name + " : \t[" + ((UnitStat)k) + "] \t" + stat.GetStat((UnitStat)k) + "  \t" + itemStat.GetStat((UnitStat)k));
                //}

                ui.ResetList();
				ui.CloseEditTeam();
                ui.UpdateUnitCard();

                return true;

			case 1:		GameCore.Instance.ShowNotice("파밍 실패", "쿼리 오류", 0); break;
			case 2:		GameCore.Instance.ShowNotice("파밍 실패", "파밍 중복", 0); break;
			case 3:		GameCore.Instance.ShowNotice("파밍 실패", "실행 가능 수 초과", 0); break;
            case 4:		GameCore.Instance.ShowNotice("조건 불충분", "미션 조건에 맞지 않아\n 출발할 수 없습니다.", 0); break;
			default:	GameCore.Instance.ShowNotice("파밍 실패","알 수 없는 에러 " + code, 0);	break;
		}

		return false;
	}

	internal bool ANS_FARMING_DONE(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		var code = -1;
		json.GetField(ref code, "result");

        switch (code)
		{
			case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Item_Get);

                // Delete Farming Data
                var delFarming = json.GetField("DELETE_FARMING");
                int key=0;
                List<int> keys = new List<int>();
				for (int i = 0; i < delFarming.Count; ++i)
				{
					key = delFarming[i].custom_n;
                    keys.Add(key);
					GameCore.Instance.PlayerDataMgr.RemoveFarmingData(key);
				}

                var units = json.GetField("CHA_LIST");
                var unitUids = new List<long>();
                for (int j = 0; j < keys.Count; ++j)
                {
                    List<long> heroList = new List<long>();
                    for (int i = 0; i < GameCore.Instance.PlayerDataMgr.GetUserHeroSDataList().Count; ++i)
                    {
                        if(GameCore.Instance.PlayerDataMgr.GetUserHeroSDataList()[i].farming_Id == keys[j])
                        {
                            heroList.Add(GameCore.Instance.PlayerDataMgr.GetUserHeroSDataList()[i].uid);
                        }
                    }
                    if (FarmingTypeUnits.ContainsKey(keys[j])) FarmingTypeUnits[keys[j]] = heroList;
                    else FarmingTypeUnits.Add(keys[j], heroList);
                }
                if (units != null) GameCore.Instance.PlayerDataMgr.SetCardSData(units);
				//Reward (data)
				var list = GameCore.Instance.PlayerDataMgr.SetRewardItems(json.GetField("REWARD_LIST"));

                ui.UpdateUnitCard();
                // 재파밍 선택 팝업

                for (int i = 0; i < units.Count; ++i)
                {
                    long uid = 0;
                    units[i].GetField(ref uid, "CHA_UID");
                    unitUids.Add(uid);
                }
                

                // Reward Print 팝업
                //GameCore.Instance.CommonSys.tbUi.SetReturnBtnCB(() => { });
                //EventDelegate confirmCallBackMethod = new EventDelegate(() => { GameCore.Instance.CommonSys.tbUi.SetReturnBtnCB(null); });
                GameCore.Instance.ShowReceiveItemPopup("획득", list, new EventDelegate(new EventDelegate.Callback(()=>
                {
                    GameCore.Instance.CloseMsgWindow();
                    GameCore.Instance.ShowAgree("재파밍", "동일한 조합으로 동일한 파밍을 다시 나가시겠습니까?", 0, () =>
                    {
                        for (int i = 0; i < keys.Count; ++i)
                        {
                            GameCore.Instance.NetMgr.Req_Farming_Create(keys[i], FarmingTypeUnits[keys[i]].ToArray());
                        }
                        GameCore.Instance.CloseMsgWindow();
                    });
                })));
				//var grid = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Farming/content", ui.transform);
				//for (int i = 0; i < list.Length; ++i)
				//{
				//	var sdata = list[i];
				//	if (!(sdata is ItemSData) || ((ItemSData)sdata).count > 0)
				//		CardBase.CreateCard(sdata, null, false, grid.transform, null, (_key) => GameCore.Instance.ShowCardInfo(sdata));
				//}
				//GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "획득",
				//	new MsgAlertBtnData[] { new MsgAlertBtnData("확인", null) }, 0, null, null, grid.gameObject)));
				//grid.transform.localPosition = new Vector3(-32, -68);

				ui.UpdateResource();
				ui.ResetList();

                return true;

			case 1:		GameCore.Instance.ShowNotice("파밍 실패", "쿼리 오류", 0); break;
			case 2:		GameCore.Instance.ShowNotice("파밍 실패", "존재하지 않는 파밍 아이디", 0); break;
			default:	GameCore.Instance.ShowNotice("파밍 실패", "알 수 없는 에러 " + code, 0); break;
		}

		return false;
	}

	internal bool ANS_FARMING_CANSEL(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		var code = -1;
		json.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				int key = 0;
				json.GetField(ref key, "DELETE_FARMING");
				GameCore.Instance.PlayerDataMgr.RemoveFarmingData(key);
				var cha_List = json.GetField("CHA_LIST");
				GameCore.Instance.PlayerDataMgr.SetUnitSData(cha_List);

				ui.ResetList();
                ui.UpdateUnitCard();
                return true;

			case 1:		GameCore.Instance.ShowNotice("파밍 실패", "쿼리 오류", 0); break;
			case 2:		GameCore.Instance.ShowNotice("파밍 실패", "존재하지 않는 파밍 아이디", 0); break;
			default:	GameCore.Instance.ShowNotice("파밍 실패", "알 수 없는 에러 " + code, 0); break;
		}

		return false;
	}

    internal override void ClickBackButton()
    {
        if(GameCore.Instance.CommonSys.tbUi.GetFriendType() == true) GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu()) GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        else GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        switch (tutorialNum)
        {
            case 12:
                break;
            default:
                break;
        }
        return nActionList;
    }
}
