using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IDH.MyRoom;


internal class PvPMatchSysPara : ParaBase
{
	public bool isRevenge;
	public int historyUID;
    public Action returnButtonDelegate;

}

internal class PvPMatchSys : SubSysBase
{
	UIPvPMatch ui;
    long historyUID = -1;

    public PvPMatchSysPara para { get; private set; }

	public PvPMatchSys() : base(SubSysType.PvPMatch)
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
        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.SelectStage);

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
		ui = UIManager.Instance.GetPage<UIPvPMatch>(EUIPage.UIPvPMatch);
		ui.Init(StartPvPBattle);

        if (_para != null)
		{
            para = _para.GetPara<PvPMatchSysPara>();
            if (para.isRevenge)
            {
                Name = "REVENGE";
                historyUID = para.historyUID;
                GameCore.Instance.NetMgr.Req_MyRoomRevenge(para.historyUID);
                ui.DisibleResearchButton();
                GameCore.Instance.CommonSys.tbUi.SetReturnBtnCB(para.returnButtonDelegate);
                return;
            }
            GameCore.Instance.CommonSys.tbUi.SetReturnBtnCB(para.returnButtonDelegate);
        }

        GameCore.Instance.NetMgr.Req_PvPMatchList();
        historyUID = -1;

        ui.gameObject.SetActive(false);
        SubSysBase.CheckSuddenQuit.SetSuddenQuitData(CheckSuddenQuit.SuddenQuitSysType.PvpMatch, null);
    }

	protected override void ExitSysInner(ParaBase _para)
	{
		base.ExitSysInner(_para);
		if (ui != null)
		{
			GameObject.Destroy(ui.gameObject);
			ui = null;
		}

        if (para != null && para.isRevenge)
        {
            GameCore.Instance.CommonSys.tbUi.SetReturnBtnCB(null);
        }
	}

	protected override void RegisterHandler()
	{
		if (handlerMap == null)
		{
			handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();

			//handlerMap.Add(GameEventType.ANS_PVP_RANKLIST, ANS_PVP_RANKLIST);
			handlerMap.Add(GameEventType.ANS_PVP_MATCHLIST, ANS_PVP_MATCHLIST); 
			handlerMap.Add(GameEventType.ANS_MYROON_REVENGE, ANS_MYROON_REVENGE);
			handlerMap.Add(GameEventType.ANS_PVP_STARTBATTLE, ANS_PVP_STARTBATTLE);
		}

		base.RegisterHandler();
	}

	public PvPSData oppPvPData = new PvPSData();
	public List<PvPOppUnitSData> pvpCharList = new List<PvPOppUnitSData>();
	public List<PvPRoomSData> roomItemList = new List<PvPRoomSData>();
    public List<MyRoomObjectData> myRoomObjectDataList = new List<MyRoomObjectData>();
    public List<StatInfos> statInfoList = new List<StatInfos>();

    internal bool ANS_PVP_MATCHLIST(ParaBase _para)
	{
		//Debug.LogError("ANS_PVP_MATCHLIST");

		// clear
		if (roomItemList != null)
			roomItemList.Clear();
		else
			roomItemList = new List<PvPRoomSData>();

		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				// Update ReDiscover Cost;
				GameCore.Instance.PlayerDataMgr.SetRewardItems(json.GetField("REWARD"));

				// Player PvP Data
				var playerPvP = json.GetField("PVP");
				if (playerPvP.type == JSONObject.Type.ARRAY)
					GameCore.Instance.PlayerDataMgr.PvPData.SetData(playerPvP[0]);
				else
					GameCore.Instance.PlayerDataMgr.PvPData.SetData(playerPvP);

				// Set My Team Power
				json.GetField(ref GameCore.Instance.PlayerDataMgr.PvPData.power, "MY_COMBAT");

				// Update Rediscover Cost
				ui.UpdateDiscoverCost(GameCore.Instance.PlayerDataMgr.PvPData.rediscover);

				// opponent PVP Data
				var oppJson = json.GetField("ENEMY");
				var oppPvP = oppJson.GetField("PVP");
                if (oppPvP.IsNull)
                {
                    GameCore.Instance.ShowNotice("매칭 실패", "상대를 찾지 못했습니다.", ()=>{
                        GameCore.Instance.CloseMsgWindow();
                        GameCore.Instance.ChangeBack();
                    }, 0);
                }
                else
                {
                    if (oppPvP.type == JSONObject.Type.ARRAY)
                    {
                        if (oppPvP != null)
                            oppPvPData.SetData(oppPvP[0]);
                        else
                        {
                            oppPvPData.userName = "";
                            oppPvPData.userLevel = 0;
                            oppPvPData.placement = false; // 더미일경우( PVP 데이터가 비인 상태)
                            oppPvPData.grade = GameCore.Instance.PlayerDataMgr.PvPData.grade;
                            oppPvPData.typicalKey = GameCore.Instance.PlayerDataMgr.PvPData.typicalKey;
                        }
                    }
                    else
                        oppPvPData.SetData(oppPvP);
                }
				// Set opponent Team Power
				oppJson.GetField(ref oppPvPData.power, "COMBAT");

				// opponent Unit Data
				pvpCharList.Clear();
				var oppUnitJson = oppJson.GetField("LIST");
				if (oppPvPData.placement)
				{
					for (int i = 0; i < oppUnitJson.Count; ++i)
					{
						PvPOppUnitSData data = new PvPOppUnitSData();
						data.SetData(oppUnitJson[i]);
						pvpCharList.Add(data);
					}
				}
				else
				{
					for (int i = 0; i < oppUnitJson.Count; ++i)
					{
						PvPOppUnitSData data = new PvPOppUnitSData();
						oppUnitJson[i].GetField(ref data.charID, "CHA_ID");
						data.position = i + 1;
						pvpCharList.Add(data);
					}
				}

				// opponent Room Base Item Info Data
				JSONObject roomBaseItemListJson = oppJson.GetField("MYROOM_ITEM");
				roomItemList.Clear();


                myRoomObjectDataList.Clear();
                if (roomBaseItemListJson.type == JSONObject.Type.ARRAY)
				{
                    for(int i = 0; i < roomBaseItemListJson.Count; ++i)
                    {
                        MyRoomObjectData temp = new MyRoomObjectData();

                        roomBaseItemListJson[i].GetField(ref temp.ItemId, "ITEM_ID");
                        temp.LocalData = GameCore.Instance.DataMgr.GetItemData(temp.ItemId);


                        //JSONObject vectorArr = new JSONObject(roomBaseItemListJson[i].GetField("POSITION").str);
                        JSONObject vectorArr = roomBaseItemListJson[i].GetField("POSITION");
                        Vector2 pos = Vector2.zero;
                        Vector2 flip = Vector2.zero;

                        if (vectorArr != null)
                        {
                            vectorArr[0].GetField(ref pos.x, "x");
                            vectorArr[0].GetField(ref pos.y, "y");
                            vectorArr[1].GetField(ref flip.x, "x");
                        }

                        temp.vectorList.Add(pos);
                        temp.vectorList.Add(flip);

                        myRoomObjectDataList.Add(temp);
                    }

                    //:[{"MYROOM_ID":1,"ANCHOR":{"x":0,"y":0},"ANGLE":0,"POSITION":[{"x":13,"y":8},{"x":1,"y":0}],"ITEM_ID":3600106,"TYPE":21}
                    //               for (int i = 0; i < roomBaseItemListJson.Count; ++i)
                    //{
                    //	var optionSubJson = roomBaseItemListJson[i];

                    //	PvPRoomSData myRoomItem = new PvPRoomSData();
                    //                   myRoomItem.SetData(optionSubJson);
                    //                   roomItemList.Add(myRoomItem);
                    //                   MyRoomObjectData myRoomObjectData = new MyRoomObjectData();
                    //                   myRoomObjectData.ItemId = myRoomItem.ITEM_ID;
                    //                   myRoomObjectData.LocalData = GameCore.Instance.DataMgr.GetItemData(myRoomObjectData.ItemId);
                    //                   if(myRoomItem.POSITION)
                    //                   myRoomObjectData.vectorList = myRoomItem.POSITION.ToList<Vector2>();
                    //                   myRoomObjectDataList.Add(myRoomObjectData);

                    //}
                }
				int oppTeamSkillKey = pvpCharList[0].skill;
				int oppRank = 0;
				oppJson.GetField(ref oppRank, "RANK");
				int oppGroupRank = 0;
				oppJson.GetField(ref oppGroupRank, "GROUP_RANK");

				var pvpData = GameCore.Instance.PlayerDataMgr.PvPData;
				var rank = GameCore.Instance.PlayerDataMgr.PvPRank;
				var groupRank = GameCore.Instance.PlayerDataMgr.PvPGroupRank;

				ui.SetPvPInfo(UIPvPMatch.PVPInfoTarget.Player, pvpData, rank, groupRank);
				ui.SetPvPInfo(UIPvPMatch.PVPInfoTarget.Opponent, oppPvPData, oppRank, oppGroupRank);
				ui.SetPvPTeamSkill(UIPvPMatch.PVPInfoTarget.Opponent, oppTeamSkillKey);
				ui.CreateOpponentTeamCard(pvpCharList);

                ui.SetAnimator();
                //ui.InitializePvP(pvpCharList, roomBaseItemList);

                var statInfos = json.GetField("STAT");
                for (int i = 0; i < statInfos.Count; i++)
                {
                    StatInfos statInfoStruct = new StatInfos(statInfos[i], false);
                    statInfoList.Add(statInfoStruct);
                }

                break;

			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			case 2: GameCore.Instance.ShowNotice("실패", "재도전횟수 소진", 0); break;
			case 3:
                //GameCore.Instance.ShowNotice("실패", "재화 부족", 0);
                GameCore.Instance.ShowReduceResource(ResourceType.Gold);
                break;
			case 4: GameCore.Instance.ShowNotice("실패", "아이템 업데이트 오류", 0); break;
			case 5: GameCore.Instance.ShowNotice("실패", "재탐색 불가", 0); break;
			case 6:
                SubSysBase.CheckSuddenQuit.GoToLobby();
                //GameCore.Instance.ShowNotice("실패", "시즌이 종료되었습니다.", 0);
                //GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
                break;

			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 " + code, 0); break;
		}

		return true;
	}

	public bool ANS_MYROON_REVENGE(ParaBase _para)
	{
		if (roomItemList != null)
			roomItemList.Clear();
		else
			roomItemList = new List<PvPRoomSData>();

		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				// Update ReDiscover Cost;
				GameCore.Instance.PlayerDataMgr.SetRewardItems(json.GetField("REWARD"));
				int playerRank = 0;
				int playerGroupRank = 0;
				json.GetField(ref playerRank, "RANK");
				json.GetField(ref playerGroupRank, "GROUP_RANK");
				GameCore.Instance.PlayerDataMgr.PvPRank = playerRank;
				GameCore.Instance.PlayerDataMgr.PvPGroupRank = playerGroupRank;

				// Player PvP Data
				var playerPvP = json.GetField("PVP");
				if (playerPvP.type == JSONObject.Type.ARRAY)
					GameCore.Instance.PlayerDataMgr.PvPData.SetData(playerPvP[0]);
				else
					GameCore.Instance.PlayerDataMgr.PvPData.SetData(playerPvP);

				// Set My Team Power
				json.GetField(ref GameCore.Instance.PlayerDataMgr.PvPData.power, "MY_COMBAT");

				// Update Rediscover Cost
				ui.UpdateDiscoverCost(GameCore.Instance.PlayerDataMgr.PvPData.rediscover);

				// opponent PVP Data
				var oppJson = json.GetField("ENEMY");
				var oppPvP = oppJson.GetField("PVP");
				if (oppPvP.type == JSONObject.Type.ARRAY)
				{
					if (oppPvP[0] != null)
						oppPvPData.SetData(oppPvP[0]);
					else
					{
						oppPvPData.placement = false; // 더미일경우( PVP 데이터가 비인 상태)
						oppPvPData.grade = GameCore.Instance.PlayerDataMgr.PvPData.grade;
						oppPvPData.typicalKey = GameCore.Instance.PlayerDataMgr.PvPData.typicalKey;
					}
				}
				else
					oppPvPData.SetData(oppPvP);

				// Set opponent Team Power
				oppJson.GetField(ref oppPvPData.power, "COMBAT");

				// opponent Unit Data
				pvpCharList.Clear();
				var oppUnitJson = oppJson.GetField("LIST");
				if (oppPvPData.placement)
				{
					for (int i = 0; i < oppUnitJson.Count; ++i)
					{
						PvPOppUnitSData data = new PvPOppUnitSData();
						data.SetData(oppUnitJson[i]);
						pvpCharList.Add(data);
					}
				}
				else
				{
					for (int i = 0; i < oppUnitJson.Count; ++i)
					{
						PvPOppUnitSData data = new PvPOppUnitSData();
						oppUnitJson[i].GetField(ref data.charID, "CHA_ID");
						data.position = i + 1;
						pvpCharList.Add(data);
					}
				}

				// opponent Room Base Item Info Data
				JSONObject roomBaseItemListJson = oppJson.GetField("MYROOM_ITEM");
				roomItemList.Clear();
				if (roomBaseItemListJson != null)
				{
					for (int i = 0; i < roomBaseItemListJson.Count; ++i)
					{
						var optionSubJson = roomBaseItemListJson[i];

						PvPRoomSData myRoomItem = new PvPRoomSData();
						myRoomItem.SetData(optionSubJson);
						roomItemList.Add(myRoomItem);
					}
				}
				int oppTeamSkillKey = pvpCharList[0].skill;
				int oppRank = 0;
				oppJson.GetField(ref oppRank, "RANK");
				int oppGroupRank = 0;
				oppJson.GetField(ref oppGroupRank, "GROUP_RANK");

				var pvpData = GameCore.Instance.PlayerDataMgr.PvPData;
				var rank = GameCore.Instance.PlayerDataMgr.PvPRank;
				var groupRank = GameCore.Instance.PlayerDataMgr.PvPGroupRank;

				ui.SetPvPInfo(UIPvPMatch.PVPInfoTarget.Player, pvpData, rank, groupRank);
				ui.SetPvPInfo(UIPvPMatch.PVPInfoTarget.Opponent, oppPvPData, oppRank, oppGroupRank);
				ui.SetPvPTeamSkill(UIPvPMatch.PVPInfoTarget.Opponent, oppTeamSkillKey);
				ui.CreateOpponentTeamCard(pvpCharList);
                //ui.InitializePvP(pvpCharList, roomBaseItemList);

                var statInfos = json.GetField("STAT");
                if (statInfos != null)
                {
                    for (int i = 0; i < statInfos.Count; i++)
                    {
                        StatInfos statInfoStruct = new StatInfos(statInfos[i], false);
                        statInfoList.Add(statInfoStruct);
                    }
                }

                break;

			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 " + code, 0); break;
		}

		return true;
	}


	public bool ANS_PVP_STARTBATTLE(ParaBase _para)
	{
		var json = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		json.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				GameCore.Instance.PlayerDataMgr.SetRewardItems(json.GetField("REWARD"));

				StartPvPBattle();
				return true;

			case 1: GameCore.Instance.ShowNotice("실패", "티켓 부족", 0); break;
            case 2: SubSysBase.CheckSuddenQuit.GoToLobby(); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 " + code, 0); break;
		}

		return false;
	}

	public void StartPvPBattle()
	{
        ResultPvPUI.From from = ResultPvPUI.From.PvP;
        if (!GameCore.Instance.PlayerDataMgr.PvPData.placement)
            from = ResultPvPUI.From.Placement;
        else if(historyUID != -1)
            from = ResultPvPUI.From.Revenge;

        //GameCore.Instance.ChangeSubSystem(SubSysType.PvPBattle, new PvPBattlePara()
        //{
        //  from = from,
        //	charList = pvpCharList,
        //	plyPower = GameCore.Instance.PlayerDataMgr.PvPData.power,
        //	oppPower = oppPvPData.power,
        //	interierList = roomItemList,
        //	opponentData = oppPvPData,
        //	historyUID = historyUID
        //});

        GameCore.Instance.ChangeSubSystem(SubSysType.PvPBattle, PvPBattlePara.CreatePvPPara(
            from,
            oppPvPData,
            pvpCharList,
            myRoomObjectDataList,
            GameCore.Instance.PlayerDataMgr.PvPData.power,
            oppPvPData.power,
            historyUID,
            statInfoList
        ));
    }

    internal override void ClickBackButton()
    {
        if (GameCore.Instance.CommonSys.tbUi.GetFriendType() == true) GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu()) GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        else GameCore.Instance.ChangeSubSystem((!GameCore.Instance.PlayerDataMgr.PvPData.placement) ? SubSysType.PvPGradeTest : SubSysType.PvPReady, null);
    }

    internal override void UpdateUI()
    {
        base.UpdateUI();
        SubSysBase.CheckSuddenQuit.SuddenQuit();
    }

    internal override void FuncAfterShowLoadingEnd()
    {
        ui.gameObject.SetActive(true);
        //ui.SetAnimator();
    }
}
