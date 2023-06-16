using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePotUnity;

internal class LobbyPara : ParaBase
{

}

internal class LobbySys : SubSysBase, ISequenceAction
{
	LobbyUI ui;

    float waitBGMTime;

    internal LobbySys() : base(SubSysType.Lobby)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Team,
            InitDataType.Gacha,
            InitDataType.Item,
            InitDataType.Farming,
            InitDataType.PvP,
        };

    }


    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);

        ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_PanelLobbyUI, GameCore.Instance.ui_root).GetComponent<LobbyUI>();
        ui.Init();
        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, ui.GetTutorialTransformList);

        //GameCore.Instance.NetMgr.Req_Make_GetList();
        //GameCore.Instance.NetMgr.Req_Farming_GetList();
        GameCore.Instance.NetMgr.Req_Notify_MyRoom_Count();
        //GameCore.Instance.NetMgr.Req_Notify_Friend_Mail_Count();
        GameCore.Instance.NetMgr.Req_Mission_List();

        if (!GameCore.Instance.ChatMgr.InitedFilterTexts)
            GameCore.Instance.NetMgr.Req_Chat_Filter();

        // 이미 파밍 데이터가 수신되었기때문제 바로 업데이트
        ui.FarmingReward();

        // 제조 데이터 최초 업데이트
        if (!GameCore.Instance.PlayerDataMgr.InitedMakeData)
            GameCore.Instance.NetMgr.Req_Make_GetList();
        else
            ui.MakeReward();

        // 출첵 데이터 최초 업데이트
        if (!GameCore.Instance.PlayerDataMgr.InitedAttendanceData)
            GameCore.Instance.NetMgr.Req_Attendance_Lookup();
        else
            CheckAttendanceHighlight();

        GameCore.Instance.SoundMgr.SetMainBGMSound();
        GameCore.Instance.ShowReceiveItemPopup(CSTR.TEXT_LeveUpReward, GameCore.Instance.PlayerDataMgr.GetExpRewards().ToArray());
        //PrintExpReward(GameCore.Instance.PlayerDataMgr.GetExpRewards());
        GameCore.Instance.PlayerDataMgr.ClearExpReward();
        SubSysBase.CheckSuddenQuit.SetSuddenQuitData(CheckSuddenQuit.SuddenQuitSysType.Lobby, null);

        AddAllPrevChat();
        GameCore.Instance.ChatMgr.onNewChat += CBAddNewChat;
    }


    static bool bFirst = true;
    internal override void FuncAfterShowLoadingEnd()
    {
        if (bFirst)
        {
            bFirst = false;
#if !UNITY_EDITOR
            GamePot.showNotice();
#endif
        }
    }


    private void PrintExpReward(List<CardSData> _list)
	{
		if (_list.Count == 0)
			return;

		var grid = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Farming/content", ui.transform);
		for (int i = 0; i < _list.Count; ++i)
		{
			var sdata = _list[i];
			if (!(sdata is ItemSData) || ((ItemSData)sdata).count > 0)
				CardBase.CreateCard(sdata, null, false, grid.transform, null, (_key) => GameCore.Instance.ShowCardInfo(sdata));
		}
		GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, CSTR.TEXT_LeveUpReward,
			new MsgAlertBtnData[] { new MsgAlertBtnData(CSTR.TEXT_Accept, null) }, 0, null, null, grid.gameObject)));
		grid.transform.localPosition = new Vector3(-32, -68);
	}

	protected override void RegisterHandler()
	{
		handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
		handlerMap.Add(GameEventType.ANS_ACCOUNT_SETCOMDELE, ANS_ACCOUNT_SETCOMDELE);
		//handlerMap.Add(GameEventType.ANS_CHAT, ANS_CHAT);
        //handlerMap.Add(GameEventType.ANS_CHAT_NOTIFICATION, ANS_CHAT_NOTIFICATION);
		handlerMap.Add(GameEventType.ANS_MAKE_GETLIST, ANS_MAKE_GETLIST);
		handlerMap.Add(GameEventType.ANS_FARMING_GETLIST, ANS_FARMING_GETLIST);
        handlerMap.Add(GameEventType.ANS_NOTIFY_MYROOM_MISSION_COUNT, ANS_NOTIFY_MYROOM_COUNT);
        handlerMap.Add(GameEventType.ANS_ATTENDANCE_LOOKUP, ANS_ATTENDANCE_LOOKUP);
        //handlerMap.Add(GameEventType.ANS_MYROOM_GetInfo, ANS_MYROOM_GetInfo);

        base.RegisterHandler();
	}

	protected override void ExitSysInner(ParaBase _para)
	{
		base.ExitSysInner(_para);

		if (ui != null)
		{
			GameObject.Destroy(ui.gameObject);
			ui = null;
		}

        GameCore.Instance.ChatMgr.onNewChat -= CBAddNewChat;
    }

	internal override void ClickBackButton()
	{
        if (GameCore.Instance.CommonSys.tbUi.GetFriendType() == true)   GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu())      GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        else if (GameCore.Instance.MissionMgr.MissionUIActive())        GameCore.Instance.MissionMgr.Close();
        else                                                            GameCore.Instance.ShowAgree(CSTR.MSG_HEAD_Quit, CSTR.MSG_AskQuit, 0, GameCore.Instance.QuitApplication);
	}


    public void StartMissionReward()
    {
        ui.MissionRewardStart();
    }


    internal void UpdateHotTime()
    {
        ui.UpdateHotTime();
    }


    /// <summary>
    /// ???? 뭐하는 거지 이거
    /// </summary>
    /// <param name="_para"></param>
    /// <returns></returns>
	private bool ANS_ACCOUNT_SETCOMDELE(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = 0;
		para.GetField(ref code, "result");

		if (code == 0)
		{
			GameCore.Instance.PlayerDataMgr.SetMainCharacterUID(ui.tmpUID);
			GameCore.Instance.PlayerDataMgr.SetComment(ui.tmpComment);
			ui.InfoReset();

			GameCore.Instance.ShowAlert(CSTR.MSG_Change);
		}
		else
		{
			ui.InfoReset();
			GameCore.Instance.ShowAlert(CSTR.MSG_Change_Fail);
		}

		return true;
	}

    internal bool ANS_NOTIFY_MYROOM_COUNT(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;
        int myRoomCount = -1;
        var Notice = data.GetField("NOTIFY");
        Notice.GetField(ref myRoomCount, "MRC");

        ui.MyRoomReward(myRoomCount);

        StartMissionReward();
        return true;
    }

    public void AddAllPrevChat()
    {
        var iter = GameCore.Instance.ChatMgr.GetChatListEnumerator();
        while(iter.MoveNext())
        {
            var data = iter.Current;
            if (data.GRADE < 0) ui.AddChat(data.USER_NAME, data.MESSAGE, data.CHA_ID);
            else                ui.AddChat(data.CHA_ID, data.USER_NAME, data.MESSAGE, data.GRADE);
        }
    }

    public void CBAddNewChat(ChatData _data)
    {
        if (_data.GRADE < 0) ui.AddChat(_data.USER_NAME, _data.MESSAGE, _data.CHA_ID);
        else                 ui.AddChat(_data.CHA_ID, _data.USER_NAME, _data.MESSAGE, _data.GRADE);
    }

    internal void ClearChat()
    {
        if (ui != null)
            ui.ClearChat();
    }

    void CheckAttendanceHighlight()
    {
        bool highlight = false;
        var dic = GameCore.Instance.PlayerDataMgr.attendanceDic;

        var iter = GameCore.Instance.DataMgr.GetACheckEnumertor();
        while(iter.MoveNext())
        {
            var id = iter.Current.Value.id;

            var data = GameCore.Instance.DataMgr.GetACheckData(id);
            if (data == null)
                continue;

            if (GameCore.nowTime < data.startDate.Date || data.endDate < GameCore.nowTime)
                continue;

            if (!dic.ContainsKey(id))
            {
                highlight = true;
                break;
            }
            else
            {
                if (GameCore.Instance.DataMgr.GetLastACheckReward(id) != dic[id].AI &&
                    dic[id].RD.Date < GameCore.nowTime.Date)
                {
                    highlight = true;
                    break;
                }
            }
        }

        ui.SetButtonHighlight_Attendance(highlight);
    }


    //   internal bool ANS_CHAT(ParaBase _para)
    //{
    //	var para = _para.GetPara<PacketPara>().data.data;
    //	//int code = -1;
    //	//para.GetField(ref code, "result");

    //	//if (code == 0)
    //	//{
    //		string name = null;
    //		string text = null;
    //		int key = -1;
    //		para.GetField(ref name, "USER_NAME");
    //		para.GetField(ref text, "MESSAGE");
    //		para.GetField(ref key, "CHA_ID");
    //		ui.AddChat(name, text, key);
    //	//}

    //	return true;
    //}
    //internal bool ANS_CHAT_NOTIFICATION(ParaBase para)
    //{
    //    var paraData = para.GetPara<PacketPara>().data.data;
    //    string userName = null;        //유저 이름
    //    string itemName = null;        //아이탬/영웅 이름

    //    int itemGreade = -1;     //등급.
    //    int charId = -1;

    //    var msgList = paraData.GetField("MSG");
    //    if (msgList.type == JSONObject.Type.ARRAY)
    //    {
    //        for (int i = 0; i < msgList.Count; ++i)
    //        {
    //            msgList[i].GetField(ref charId, "UDI");
    //            msgList[i].GetField(ref userName, "UN");
    //            msgList[i].GetField(ref itemName, "NAME");
    //            msgList[i].GetField(ref itemGreade, "GRADE");

    //            ui.AddChat(charId, userName, itemName, itemGreade);
    //        }
    //    }
    //    return true;
    //}


    internal bool ANS_MAKE_GETLIST(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch(code)
		{
			case 0:
				GameCore.Instance.PlayerDataMgr.SetMakeData(para.GetField("MAKING"));
                ui.MakeReward();
                return true;
			case 2: GameCore.Instance.ShowNotice(CSTR.TEXT_MakeUpdate, CSTR.MSG_EmptySlot, 0); break;
			default: GameCore.Instance.ShowNotice(CSTR.TEXT_MakeUpdate, string.Format(CSTR.MSG_WrongCode, code), 0); break;
        }
       
        return false;
	}

	internal bool ANS_FARMING_GETLIST(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				GameCore.Instance.PlayerDataMgr.SetFarmingData(para.GetField("FARMING"));
                ui.FarmingReward();
                return true;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_FarmingUpdate, string.Format(CSTR.MSG_WrongCode, code), 0); break;
        }

        return false;
	}

    //internal bool ANS_MYROOM_GetInfo(ParaBase _para)
    //{
    //    var para = _para.GetPara<PacketPara>().data.data;
    //    int code = -1;
    //    para.GetField(ref code, "result");

    //    switch (code)
    //    {
    //        case 0:
    //            GameCore.Instance.PlayerDataMgr.SetMyRoomData(para.GetField("MYROOM"));
    //            GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, null);
    //            return true;
    //        //case 1: GameCore.Instance.ShowNotice("제조정보 업데이트 실패", "잘못된 데이터", 0); break;
    //        //case 2: GameCore.Instance.ShowNotice("제조정보 업데이트 실패", "슬롯정보 없음", 0); break;
    //        default: GameCore.Instance.ShowNotice("마이룸 데이터 실패", "알 수 없는 에러 : " + code, 0); break;
    //    }

    //    return false;
    //}

    
    internal bool ANS_ATTENDANCE_LOOKUP(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.PlayerDataMgr.SetAttendanceData(para.GetField("ATTENDANCE"));
                CheckAttendanceHighlight();
                return true;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_AttendaceUpdate, string.Format(CSTR.MSG_WrongCode, code), 0); break;
        }

        return false;
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        return nActionList;
    }
}
