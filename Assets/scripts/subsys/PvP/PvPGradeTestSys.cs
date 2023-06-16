using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class PvPGradeTestSysPara : ParaBase
{
}

internal class PvPGradeTestSys : SubSysBase
{
    UIPvPGradeTest ui;
    BattleParaBase battlePara;

	public PvPGradeTestSys() : base(SubSysType.PvPGradeTest)
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
		//if( ui == null)
		{
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

            //GameCore.Instance.NetMgr.Req_PvPGradeTestMatchList();

            //GameCore.Instance.NetMgr.Req_PvPRankList();


            ui = UIManager.Instance.GetPage<UIPvPGradeTest>(EUIPage.UIPvPGradeTest);
            //ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("SelectStage/PanelSelectStageUI", GameCore.Instance.ui_root).GetComponent<UIPvPReady>();
            ui.Init(CBClickStage, CBClickBack, userRank, pvpRankList);


        }
        //GameCore.Instance.SndMgr.PlayBGM(BGM.SelectStage);
        GameCore.Instance.SoundMgr.SetMainBGMSound();
        SubSysBase.CheckSuddenQuit.SetSuddenQuitData(CheckSuddenQuit.SuddenQuitSysType.PvPGradeTest, null);

        AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.PvP , 0);
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
            handlerMap.Add(GameEventType.ANS_PVP_GRADETEST, ANS_PVP_GRADETEST);
        }
        

        base.RegisterHandler();
    }

    public stRank userRank;
    public List<stRank> pvpRankList = new List<stRank>();
    public List<stPVPChar> pvpCharList = new List<stPVPChar>();
    public List<stRoomBaseItem> roomBaseItemList = new List<stRoomBaseItem>();


    internal bool ANS_PVP_RANKLIST(ParaBase _para)
    {
        Debug.LogError("ANS_PVP_RANKLIST");

        // clear
        if (pvpRankList != null)
            pvpRankList.Clear();
        else
            pvpRankList = new List<stRank>();

        var json = _para.GetPara<PacketPara>().data.data;

        int code = -1;
        json.GetField(ref code, "result");
        //json = json.GetField("DATA");

        if (code == 0)
        {
            var _json = json;

            JSONObject itemLIstJson = _json.GetField("MY_RANK");

            userRank = new stRank();
            JsonParse.ToParse(itemLIstJson, "USER_LEVEL", out userRank.level);

            JsonParse.ToParse(itemLIstJson, "USER_NAME", out userRank.name);
            JsonParse.ToParse(itemLIstJson, "DELEGATE_ICON", out userRank.icon);
            JsonParse.ToParse(itemLIstJson, "RANK", out userRank.rank);
            JsonParse.ToParse(itemLIstJson, "POINT", out userRank.point);
            JsonParse.ToParse(itemLIstJson, "POWER", out userRank.power);

            /*
            if (itemLIstJson != null)
            {
                for (int i = 0; i < itemLIstJson.Count; ++i)
                {
                    var optionSubJson = itemLIstJson[i];

                    stRank rankInst = new stRank();
                    JsonParse.ToParse(optionSubJson, "USER_LEVEL", out rankInst.level);

                    JsonParse.ToParse(optionSubJson, "USER_NAME", out rankInst.name);
                    JsonParse.ToParse(optionSubJson, "DELEGATE_ICON", out rankInst.icon);
                    JsonParse.ToParse(optionSubJson, "RANK", out rankInst.rank);
                    JsonParse.ToParse(optionSubJson, "POINT", out rankInst.point);
                    JsonParse.ToParse(optionSubJson, "POWER", out rankInst.power);

                    pvpRankList.Add(rankInst);
                }
            }
            */
            itemLIstJson = _json.GetField("PVP_LIST");

            if (itemLIstJson != null)
            {
                for (int i = 0; i < itemLIstJson.Count; ++i)
                {
                    var optionSubJson = itemLIstJson[i];

                    stRank rankInst = new stRank();
                    JsonParse.ToParse(optionSubJson, "USER_LEVEL", out rankInst.level);

                    JsonParse.ToParse(optionSubJson, "USER_NAME", out rankInst.name);
                    JsonParse.ToParse(optionSubJson, "DELEGATE_ICON", out rankInst.icon);
                    JsonParse.ToParse(optionSubJson, "RANK", out rankInst.rank);
                    JsonParse.ToParse(optionSubJson, "POINT", out rankInst.point);
                    JsonParse.ToParse(optionSubJson, "POWER", out rankInst.power);

                    pvpRankList.Add(rankInst);

                }
            }
        }


        ui.Init(CBClickStage, CBClickBack, userRank, pvpRankList);

        return true;
    }

    
    internal bool ANS_PVP_GRADETEST(ParaBase _para)
    {
        /*
        Debug.LogError("ANS_PVP_GRADETEST");

        // clear
        if (pvpCharList != null)
            pvpCharList.Clear();
        else
            pvpCharList = new List<stPVPChar>();

        if (roomBaseItemList != null)
            roomBaseItemList.Clear();
        else
            roomBaseItemList = new List<stRoomBaseItem>();

        var json = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        json.GetField(ref code, "result");
        //json = json.GetField("DATA");

        if (code == 0)
        {
            var _json = json;

            JSONObject pvpListJson = _json.GetField("PVP_LIST");

            if (pvpListJson != null)
            {
                for (int i = 0; i < pvpListJson.Count; ++i)
                {
                    var optionSubJson = pvpListJson[i];

                    stPVPChar pvpCharInst = new stPVPChar();
                    JsonParse.ToParse(optionSubJson, "CHA_UID", out pvpCharInst.charUID);
                    JsonParse.ToParse(optionSubJson, "TEAM", out pvpCharInst.teamIdx);
                    JsonParse.ToParse(optionSubJson, "POSITION", out pvpCharInst.posiion);
                    JsonParse.ToParse(optionSubJson, "USER_UID", out pvpCharInst.userUID);
                    JsonParse.ToParse(optionSubJson, "CHA_ID", out pvpCharInst.charID);
                    JsonParse.ToParse(optionSubJson, "CHA_LVL", out pvpCharInst.charLevel);
                    JsonParse.ToParse(optionSubJson, "DISPATCH", out pvpCharInst.dispatch);
                    JsonParse.ToParse(optionSubJson, "MYROOM_ID", out pvpCharInst.myRoomID);
                    JsonParse.ToParse(optionSubJson, "FARMING_ID", out pvpCharInst.farmingID);
                    JsonParse.ToParse(optionSubJson, "CREATE_DATE", out pvpCharInst.createDate);

                    Debug.LogError("CHA_UID = " + pvpCharInst.charUID);
                    pvpCharList.Add(pvpCharInst);

                }
            }

            JSONObject roomBaseItemListJson = _json.GetField("MYROOM_ITEM");

            if (roomBaseItemListJson != null)
            {
                for (int i = 0; i < roomBaseItemListJson.Count; ++i)
                {
                    var optionSubJson = roomBaseItemListJson[i];

                    stRoomBaseItem roomBaseItem = new stRoomBaseItem();
                    JsonParse.ToParse(optionSubJson, "ITEM_ID", out roomBaseItem.itemID);
                    JsonParse.ToParse(optionSubJson, "TYPE", out roomBaseItem.itemType);

                    ItemDataMap itemDataMap = GameCore.Instance.PlayerDataMgr.GetItemData((long)roomBaseItem.itemID);
                    if (itemDataMap == null)
                        itemDataMap = GameCore.Instance.DataMgr.GetItemData(roomBaseItem.itemID);

                    roomBaseItem.ingameTex = itemDataMap.inGameImageID;

                    roomBaseItemList.Add(roomBaseItem);

                }
            }

        }

        ui.InitializePvP(pvpCharList, roomBaseItemList);
        */
        return true;
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
