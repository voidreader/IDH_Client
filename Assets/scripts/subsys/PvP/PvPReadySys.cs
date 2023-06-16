using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class PvPReadySysPara : ParaBase
{
}

internal class PvPReadySys : SubSysBase
{
    UIPvPReady ui;

	List<PvPGroupRankSData> pvpGroupRnaks = new List<PvPGroupRankSData>();
	List<PvPGroupRankSData> pvpTopRanks = new List<PvPGroupRankSData>();
	List<PvPGroupRankSData> seasonList = new List<PvPGroupRankSData>();
	Dictionary<int, float> pvpGradeAvg = new Dictionary<int, float>();

	public PvPReadySys() : base(SubSysType.PvPReady)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Team,
            InitDataType.PvP,
        };
    }

	protected override void EnterSysInner(ParaBase _para)
	{
		base.EnterSysInner(_para);

		Name = "PVP";
		if (!UIManager.Instance.GetParentPage())
		{
			GameObject uiRoot = GameObject.Find("UI Root");
			GameObject pageRoot = UtilityFunc.Inst.GetChildObj(uiRoot, "Page");
			if (!pageRoot)
				pageRoot = new GameObject("Page");
			pageRoot.transform.parent = uiRoot.transform;
			NGUITools.SetLayer(pageRoot, LayerMask.NameToLayer("UI"));

			UIManager.Instance.SetParentPage(pageRoot);
			pageRoot.transform.localScale = new Vector3(1, 1, 1);
		}
		ui = UIManager.Instance.GetPage<UIPvPReady>(EUIPage.UIPvPReady);

        GameCore.Instance.SoundMgr.SetMainBGMSound();

        SubSysBase.CheckSuddenQuit.SetSuddenQuitData(CheckSuddenQuit.SuddenQuitSysType.PvPReady, null);
        //GameCore.Instance.SndMgr.PlayBGM(BGM.SelectStage);

        // 배치고사를 마친 상태일 경우
        if (!GameCore.Instance.PlayerDataMgr.PvPData.placement)
			GameCore.Instance.NetMgr.Req_PvPQuitGradeTest();
		else
			GameCore.Instance.NetMgr.Req_PvPRankList();


        //var startTime = GetRemainPvPSeasonStart();
        //var endTime = GetRemainPvPSeasonEnd();

        //if (startTime < endTime)
        //{
        //    GameCore.Instance.ShowNotice("시즌 종료", "시즌이 종료되었습니다.\n다음 시즌까지 기다려주세요.\n" + startTime/*.ToString("yyyy/MM/dd HH:mm")*/, () => {
        //        GameCore.Instance.CloseMsgWindow();
        //        GameCore.Instance.ChangeBack();
        //    }, 0);
        //}

    }

	protected override void ExitSysInner(ParaBase _para)
	{
		base.ExitSysInner(_para);
		if (ui != null )
		{
			GameObject.Destroy(ui.gameObject);
			ui = null;
		}
	}

    protected override void RegisterHandler()
    {
        if (handlerMap == null)
        {
            handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();

            handlerMap.Add(GameEventType.ANS_PVP_RANKLIST, ANS_PVP_RANKLIST);
            //handlerMap.Add(GameEventType.ANS_PVP_MATCHLIST, ANS_PVP_MATCHLIST);
						handlerMap.Add(GameEventType.ANS_PVP_TEAMINFO, ANS_PVP_TEAMINFO);
						handlerMap.Add(GameEventType.ANS_PVP_QUITGRADETEST, ANS_PVP_QUITGRADETEST); 
						handlerMap.Add(GameEventType.ANS_PVP_CONFIRMLASTSEASON, ANS_PVP_CONFIRMLASTSEASON);
				}

        base.RegisterHandler();
    }

    public static DateTime GetRemainPvPSeasonEnd()
    {
        int minCount = int.MaxValue;
        int minCountIdx = -1;

        //var time = GameCore.nowTime;
        var endDatas = GameCore.Instance.DataMgr.GetPvPConstData().resetWeek;
        var endTime = GameCore.Instance.DataMgr.GetPvPConstData().pvpEndTime;

        for (int i = 0; i < endDatas.Length; ++i)
        {
            var time = GameCore.nowTime - new TimeSpan(0, endTime, 0, 0);
            var dayCount = (6 + endDatas[i] - time.DayOfWeek) % 7;

            var hourCount = 24 - time.Hour;

            var count = dayCount * 24 + hourCount;
            if (count < minCount)
            {
                minCount = count;
                minCountIdx = i;
            }
        }

        var t = GameCore.nowTime;
        var seasonEndTime = t.Date.AddHours(t.Hour);
        seasonEndTime = seasonEndTime.AddHours(minCount);

        Debug.Log("Season End Time : " + seasonEndTime);

        return seasonEndTime;
    }

    public static DateTime GetRemainPvPSeasonStart()
    {
        int minCount = int.MaxValue;
        int minCountIdx = -1;

        //var time = GameCore.nowTime;
        var endDatas = GameCore.Instance.DataMgr.GetPvPConstData().resetWeek;
        var startTime = GameCore.Instance.DataMgr.GetPvPConstData().pvpStartTime;

        for (int i = 0; i < endDatas.Length; ++i)
        {
            var time = GameCore.nowTime - new TimeSpan(0, startTime, 0, 0);
            var dayCount = (6 + endDatas[i] - time.DayOfWeek) % 7;

            var hourCount = 24 - time.Hour;

            var count = dayCount * 24 + hourCount;
            if (count < minCount)
            {
                minCount = count;
                minCountIdx = i;
            }
        }

        var t = GameCore.nowTime;
        var seasonStartTime = t.Date.AddHours(t.Hour);
        seasonStartTime = seasonStartTime.AddHours(minCount);

        Debug.Log("Season End Time : " + seasonStartTime);

        return seasonStartTime;
    }

    public bool CheckPvPSeasonEnd()
    {
        var startTime = GetRemainPvPSeasonStart();
        var endTime = GetRemainPvPSeasonEnd();

        return startTime < endTime;
    }




    public stRank userRank;
    public List<stRank> pvpRankList = new List<stRank>();
    public List<PvPOppUnitSData> pvpCharList = new List<PvPOppUnitSData>();
    public List<stRoomBaseItem> roomBaseItemList = new List<stRoomBaseItem>();


    internal bool ANS_PVP_RANKLIST(ParaBase _para)
    {
		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				var rankJson = json.GetField("RANK");
				var myRankJson = rankJson.GetField("MYRANK");
				{// Set Rank
					int rank = -9999;
					myRankJson[0].GetField(ref rank, "RANK");
					GameCore.Instance.PlayerDataMgr.PvPRank = rank;

					int groupRank = -9999;
					myRankJson[0].GetField(ref groupRank, "GROUP_RANK");
					GameCore.Instance.PlayerDataMgr.PvPGroupRank = groupRank;
				}

				// 그룹 PVP 순위 정보
				pvpGroupRnaks.Clear();
				var groupRankJson = rankJson.GetField("GROUP_RANK");
				for (int i = 0; i < groupRankJson.Count; ++i)
				{
					PvPGroupRankSData data = new PvPGroupRankSData();
					data.SetData(groupRankJson[i]);
					pvpGroupRnaks.Add(data);
				}


				// TOP50 PVP 순위 정보
				pvpTopRanks.Clear();
				var topRankJson = rankJson.GetField("TOP_RANK");
				for (int i = 0; i < topRankJson.Count; ++i)
				{
					PvPGroupRankSData data = new PvPGroupRankSData();
					data.SetData(topRankJson[i]);
					pvpTopRanks.Add(data);
				}

				// Grade Avg;
				var positionJson = rankJson.GetField("MYPOSITION");
				for (int i = 0; i < positionJson.Count; i++)
				{
					int grade = 0;
					float avg = 0f;
					positionJson[i].GetField(ref grade, "GRADE");
					positionJson[i].GetField(ref avg, "AVERAGE");

					if (!pvpGradeAvg.ContainsKey(grade))
						pvpGradeAvg.Add(grade, avg);
					else
						pvpGradeAvg[grade] = avg;
				}

				ui.CreateGroupRank(pvpGroupRnaks);
				ui.CreateTop50Rank(pvpTopRanks);
				ui.CreateRankInfo(pvpGradeAvg);
				ui.UpdateNowPage();

                ui.UpdatePlayerRankInfo(GameCore.Instance.PlayerDataMgr.PvPData.grade, GameCore.Instance.PlayerDataMgr.PvPGroupRank);
                ui.UpdatePlayerInfo(GameCore.Instance.PlayerDataMgr.PvPData, GameCore.Instance.PlayerDataMgr.PvPRank, GameCore.Instance.PlayerDataMgr.PvPGroupRank);

                // 이전 시즌이 끝났다면...
                var seasonJson = json.GetField("LAST_SEASON");
                /*
                {
                    var Result = new JSONObject();
                    Result.AddField("USER_UID", "2I");
                    Result.AddField("USER_NAME", "JinHyeok");
                    Result.AddField("USER_LEVEL", 1);
                    Result.AddField("CHA_ID", 1);
                    Result.AddField("POWER", 1);
                    Result.AddField("RANK", 1);
                    Result.AddField("GROUP", 1);
                    Result.AddField("GRADE", 7000006);
                    Result.AddField("BFGRADE", 7000007);
                    Result.AddField("POINT", 1);
                    Result.AddField("WIN", 1);
                    Result.AddField("DEFEAT", 1);
                    Result.AddField("CONSECUTIVE", 1);

                    Result.AddField("PLACEMENT", 1);
                    Result.AddField("RECHALLENGE_COUNT", 1);

                    var Reward = new JSONObject();
                    Reward.AddField("USER_UID", 30);
                    Reward.AddField("REWARD_ITEM_ID", 3000001);
                    Reward.AddField("REWARD_ITEM_COUNT", 30);
                    JSONObject d = new JSONObject(JSONObject.Type.ARRAY);
                    
                    d.Add(Result);
                    seasonJson.AddField("RESULT", d);
                }
                */
                if (seasonJson != null)
				{
					var resultJson = seasonJson.GetField("RESULT");
					if (resultJson != null)
					{
						seasonList.Clear();
						for (int i = 0; i < resultJson.Count; ++i)
						{
							PvPGroupRankSData data = new PvPGroupRankSData();
							data.SetData(resultJson[i]);
							seasonList.Add(data);
						}
                        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_PVP_Season_End);
						var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("UI/Components/SeasonReward", ui.transform);
						var seasonReward = go.GetComponent<SeasonRewardComponent>();
						seasonReward.SetData(seasonList, GameCore.Instance.PlayerDataMgr.PvPData.userUID);

						GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "",
							new MsgAlertBtnData[]
							{
							new MsgAlertBtnData() { text = "순위 보기", enable = true, ed = new EventDelegate(() => { if(!seasonReward.bAnimationing)OpenSeasonRankList(); }), clickSound = SFX.Sfx_UI_Button },
							new MsgAlertBtnData() { text = "확인", enable = true, ed = new EventDelegate(() => { if(!seasonReward.bAnimationing)ConfirmLastReason(); }), clickSound = SFX.Sfx_UI_Button }
							},
							4, null, null, go)));
					}
				}
				

				return true;

			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			case 2:
                SubSysBase.CheckSuddenQuit.GoToLobby();
				break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
		}

		return false;
	}

	public void OpenSeasonRankList()
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("UI/Components/SeasonRankLsit", ui.transform);
		var grid = go.GetComponentInChildren<UIGrid>();

		for(int i = 0; i < seasonList.Count; ++i)
		{
			var item = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("UI/Components/RankComponent", grid.transform);
			item.GetComponent<UIRankComponent>().Init(seasonList[i], false);
		}
		grid.Reposition();

		GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "",
		new MsgAlertBtnData[] { new MsgAlertBtnData() { text = "이전", enable = true, ed = new EventDelegate(() => { GameCore.Instance.CloseMsgWindow(); }), clickSound = SFX.Sfx_UI_Button } },
		4, null, null, go)));
	}

	public void ConfirmLastReason()
	{
//		GameCore.Instance.CloseMsgWindow();
		GameCore.Instance.NetMgr.Req_PvPConfirmLastSeason();
	}

	//internal bool ANS_PVP_MATCHLIST(ParaBase _para)
 //   {
 //       Debug.LogError("ANS_PVP_MATCHLIST");

 //       // clear
 //       if (pvpCharList != null)
 //           pvpCharList.Clear();
 //       else
 //           pvpCharList = new List<PvPOppUnitSData>();

 //       if (roomBaseItemList != null)
 //           roomBaseItemList.Clear();
 //       else
 //           roomBaseItemList = new List<stRoomBaseItem>();

 //       var json = _para.GetPara<PacketPara>().data.data;
 //       int code = -1;
 //       json.GetField(ref code, "result");
 //       //json = json.GetField("DATA");

 //       if (code == 0)
 //       {
 //           var _json = json;

 //           JSONObject pvpListJson = _json.GetField("PVP_LIST");

 //           if (pvpListJson != null)
 //           {
 //               for (int i = 0; i < pvpListJson.Count; ++i)
 //               {
 //                   var optionSubJson = pvpListJson[i];

	//									PvPOppUnitSData pvpCharInst = new PvPOppUnitSData();
 //                   JsonParse.ToParse(optionSubJson, "CHA_UID", out pvpCharInst.charUID);
 //                   JsonParse.ToParse(optionSubJson, "TEAM", out pvpCharInst.teamIdx);
 //                   JsonParse.ToParse(optionSubJson, "POSITION", out pvpCharInst.position);
 //                   JsonParse.ToParse(optionSubJson, "USER_UID", out pvpCharInst.userUID);
 //                   JsonParse.ToParse(optionSubJson, "CHA_ID", out pvpCharInst.charID);
 //                   JsonParse.ToParse(optionSubJson, "CHA_LVL", out pvpCharInst.charLevel);
 //                   JsonParse.ToParse(optionSubJson, "DISPATCH", out pvpCharInst.dispatch);
 //                   JsonParse.ToParse(optionSubJson, "MYROOM_ID", out pvpCharInst.myRoomID);
 //                   JsonParse.ToParse(optionSubJson, "FARMING_ID", out pvpCharInst.farmingID);
 //                   JsonParse.ToParse(optionSubJson, "CREATE_DATE", out pvpCharInst.createDate);

 //                   Debug.LogError("CHA_UID = " + pvpCharInst.charUID);
 //                   pvpCharList.Add(pvpCharInst);

 //               }
 //           }

 //           JSONObject roomBaseItemListJson = _json.GetField("MYROOM_ITEM");

 //           if (roomBaseItemListJson != null)
 //           {
 //               for (int i = 0; i < roomBaseItemListJson.Count; ++i)
 //               {
 //                   var optionSubJson = roomBaseItemListJson[i];

 //                   stRoomBaseItem roomBaseItem = new stRoomBaseItem();
 //                   JsonParse.ToParse(optionSubJson, "ITEM_ID", out roomBaseItem.itemID);
 //                   JsonParse.ToParse(optionSubJson, "TYPE", out roomBaseItem.itemType);

 //                   ItemDataMap itemDataMap = GameCore.Instance.PlayerDataMgr.GetItemData((long)roomBaseItem.itemID);
 //                   if (itemDataMap == null)
 //                       itemDataMap = GameCore.Instance.DataMgr.GetItemData(roomBaseItem.itemID);

 //                   roomBaseItem.ingameTex = itemDataMap.inGameImageID;

 //                   roomBaseItemList.Add(roomBaseItem);

 //               }
 //           }

 //       }

 //       //ui.InitializePvP(pvpCharList, roomBaseItemList);

 //       return true;
 //   }


	private bool ANS_PVP_QUITGRADETEST(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				var pvpJson = json.GetField("PVP");
				if(pvpJson.type == JSONObject.Type.ARRAY)
					GameCore.Instance.PlayerDataMgr.PvPData.SetData(pvpJson[0]);
				else
					GameCore.Instance.PlayerDataMgr.PvPData.SetData(pvpJson);
				var rankJson = json.GetField("RANK");
				var myRankJson = rankJson.GetField("MYRANK");
				{// Set Rank
					int rank = 0;
					myRankJson[0].GetField(ref rank, "RANK");
					GameCore.Instance.PlayerDataMgr.PvPRank = rank;

					int groupRank = 0;
					myRankJson[0].GetField(ref groupRank, "GROUP_RANK");
					GameCore.Instance.PlayerDataMgr.PvPGroupRank = groupRank;
				}

				// 그룹 PVP 순위 정보
				pvpGroupRnaks.Clear();
				var groupRankJson = rankJson.GetField("GROUP_RANK");
				for( int i = 0; i < groupRankJson.Count; ++i)
				{
					PvPGroupRankSData data = new PvPGroupRankSData();
					data.SetData(groupRankJson[i]);
					pvpGroupRnaks.Add(data);
				}


				// TOP50 PVP 순위 정보
				pvpTopRanks.Clear();
				var topRankJson = rankJson.GetField("TOP_RANK");
				for (int i = 0; i < topRankJson.Count; ++i)
				{
					PvPGroupRankSData data = new PvPGroupRankSData();
					data.SetData(topRankJson[i]);
					pvpTopRanks.Add(data);
				}

				// Grade Avg;
				var positionJson = rankJson.GetField("MYPOSITION");
				for (int i = 0; i < positionJson.Count; i++)
				{
					int grade = 0;
					float avg = 0f;
					positionJson[i].GetField(ref grade, "GRADE");
					positionJson[i].GetField(ref avg, "AVERAGE");

					if (!pvpGradeAvg.ContainsKey(grade))
						pvpGradeAvg.Add(grade, avg);
					else
						pvpGradeAvg[grade] = avg;
				}

				ui.UpdatePlayerRankInfo(GameCore.Instance.PlayerDataMgr.PvPData.grade, GameCore.Instance.PlayerDataMgr.PvPGroupRank);
				ui.UpdatePlayerInfo(GameCore.Instance.PlayerDataMgr.PvPData, GameCore.Instance.PlayerDataMgr.PvPRank, GameCore.Instance.PlayerDataMgr.PvPGroupRank);

				ui.CreateGroupRank(pvpGroupRnaks);
				ui.CreateTop50Rank(pvpTopRanks);
				ui.CreateRankInfo(pvpGradeAvg);
				ui.UpdateNowPage();

				// Show Grade Test result
				var rankName = GameCore.Instance.DataMgr.GetPvPRateRewardData(GameCore.Instance.PlayerDataMgr.PvPData.grade).name;
				var icon = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("UI/Components/rankIcon", ui.transform);
                var iconSprite = icon.GetComponentInChildren<UISprite>();
                iconSprite.spriteName = UIPvPMatch.GetGradeBigSprite(GameCore.Instance.PlayerDataMgr.PvPData.grade);
                RankEffectManager.CreatePVP(GameCore.Instance.PlayerDataMgr.PvPData.grade, iconSprite.transform);

                for (int i = 0; i < icon.transform.childCount; ++i)
				{
					var child = icon.transform.GetChild(i);
					if (child.name == "rankName")
						child.GetComponent<UILabel>().text = rankName;
				}
				GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "배치고사 종료",
					new MsgAlertBtnData[] { new MsgAlertBtnData() { text = "확인", enable = true, ed = new EventDelegate(() => { GameCore.Instance.CloseMsgWindow(); }), clickSound = SFX.Sfx_UI_Button } },
					1, null, null, icon)));

				return true;

			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			case 2:

                SubSysBase.CheckSuddenQuit.GoToLobby();
                break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
		}

		return false;
	}

	public bool ANS_PVP_TEAMINFO(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");

		switch(code)
		{
			case 0:
				var listJson = json.GetField("LIST");
				List<PvPOppUnitSData> list = new List<PvPOppUnitSData>();
				for (int i = 0; i < listJson.Count; ++i)
				{
					PvPOppUnitSData unit = new PvPOppUnitSData();
					unit.SetData(listJson[i]);
					list.Add(unit);
				}

				var teamInfo = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("UI/Components/TeamInfo", ui.transform);
				teamInfo.GetComponent<PvPTeamInfoComponent>().SetData(list, UIRankComponent.printTeamUserName);
				GameCore.Instance.ShowObject("", null, teamInfo, 4, new MsgAlertBtnData[] {
                       new MsgAlertBtnData("확인", new EventDelegate(() => GameCore.Instance.CloseMsgWindow()))
                });

				return true;

			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
		}

		return false;
	}


	private bool ANS_PVP_CONFIRMLASTSEASON(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");


		switch (code)
		{
			case 0:
				GameCore.Instance.CloseMsgWindow();
				return true;
			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
		}
		return false;
	}

	private void CBClickStage()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.Instance.ChangeSubSystem(SubSysType.Battle, BattlePara.CreateStoryPara(0, 1));// { playerTeam = 0, stageId = 1 });
	}

	private void CBClickBack()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Cancel);
        GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, new LobbyPara() { });
	}
    internal override void UpdateUI()
    {
        base.UpdateUI();
        SubSysBase.CheckSuddenQuit.SuddenQuit();
    }
}