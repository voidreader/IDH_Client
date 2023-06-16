using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;


internal class AddToBottomPara : ParaBase
{
	internal bool left;			// false라면 오른쪽에 추가됨.
	internal bool clear;		// 남은 것을 비울지 여부
	internal GameObject go;	// 추가될 오브젝트.(null일시 추가하지 않는다.)

	private AddToBottomPara(bool _left, bool _clear, GameObject _go)
	{
		left = _left;
		clear = _clear;
		go = _go;
	}

	internal static AddToBottomPara Clear(bool _left)
	{
		return new AddToBottomPara(_left, true, null);
	}

	internal static AddToBottomPara Add(bool _left, GameObject _go)
	{
		return new AddToBottomPara(_left, false, _go);
	}
}

internal class SetReturnCBPara : ParaBase
{
	internal Action cb;	// 리턴버튼에 할당될 콜백 함수

	internal SetReturnCBPara(Action _cb)
	{
		cb = _cb;
	}
}

internal class CommonSysPara : ParaBase
{
	internal bool showBlind;
	internal CommonSysPara(bool _showBlind)
	{
		showBlind = _showBlind;
	}
}


internal class CommonSys : SubSysBase
{
    internal MsgAlertUI msgUi;
	public TopBottomUI tbUi { get; private set; }

    MailBoxComponent mailbox;
    
    private Font fontDefault;

    List<NoticeSData> notices = new List<NoticeSData>();


    internal CommonSys() : base(SubSysType.Common)
	{
        fontDefault = Resources.Load("Font/NanumGothicExtraBold") as Font;
        preloadingBundleKeys = null;
    }

	protected override void EnterSysInner(ParaBase _para)
	{
		if (msgUi == null)
		{
			var showBlind = (_para == null) ? true : ((CommonSysPara)_para).showBlind;
			msgUi = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/PanelAlertUI", GameCore.Instance.ui_root).GetComponent<MsgAlertUI>();
			msgUi.Init(showBlind);
		}

		if (tbUi == null)
		{
			tbUi = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/TopBottomUI", GameCore.Instance.ui_root).GetComponent<TopBottomUI>();
			tbUi.Init();
		}

        
        //Timer timer = new System.Timers.Timer();
        //timer.Interval = 60 * 1000;
        //timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
        //timer.Start();

		base.EnterSysInner(_para);
    }

	protected override void RegisterHandler()
	{
		if( handlerMap == null)
		{
			handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
			handlerMap.Add(GameEventType.MsgAlert, ShowMessage);
			handlerMap.Add(GameEventType.InfoAlert, ShowInfo);
			handlerMap.Add(GameEventType.AddToBottom, AddToBottom);
			handlerMap.Add(GameEventType.SetReturnCB, SetReturnBtnCB);
			//handlerMap.Add(GameEventType.ChangeSys, ShowTopBottomUI);
            handlerMap.Add(GameEventType.ANS_MAIL_GET, ANS_MAIL_GET);
            handlerMap.Add(GameEventType.ANS_MAIL_LIST, ANS_MAIL_LIST);
            handlerMap.Add(GameEventType.ANS_ITEM_GEN_CHECK, ANS_ITEM_GEN_CHECK);

            handlerMap.Add(GameEventType.ANS_NOTIFY_FRIEND_MAIL_COUNT, ANS_NOTIFY_FRIEND_MAIL_COUNT);
            handlerMap.Add(GameEventType.ANS_NOTIFY_FRIEND_COUNT, ANS_NOTIFY_FRIEND_COUNT);
            handlerMap.Add(GameEventType.ANS_NOTIFY_MAIL_COUNT, ANS_NOTIFY_MAIL_COUNT);
            handlerMap.Add(GameEventType.ANS_NOTIFY_NOTICE, ANS_NOTIFY_NOTICE);

            handlerMap.Add(GameEventType.ANS_FRIEND_LIST, ANS_FRIEND_LIST);

            handlerMap.Add(GameEventType.NOTIFY_FRIEND, ANS_NOTIFY_FRIEND_COUNT);
            handlerMap.Add(GameEventType.NOTIFY_MAIL, ANS_NOTIFY_MAIL_COUNT);

            handlerMap.Add(GameEventType.ANS_MYROOM_GetInfo, ANS_MYROOM_GetInfo);

            handlerMap.Add(GameEventType.ANS_PUSH_SAVE, ANS_PUSH_SAVE);


            handlerMap.Add(GameEventType.ANS_MYROOM_HISTORY, ANS_MyroomHistoryList);
            handlerMap.Add(GameEventType.ANS_MYROOM_FriendList, ANS_RequestFreindList);

            base.RegisterHandler();
		}
	}

    protected override void UnegisterHandler()
	{
		// Do nothing 핸들러 해제를 하지 않는다.
		//base.UnegisterHandler();
	}

	internal bool ShowMessage(ParaBase _para)
	{
		if (_para != null)			msgUi.ShowMessage(_para);
		else						msgUi.RemoveMsgComfirm();
		return true;
	}

	internal void HideMsgComfirm()
	{
		msgUi.HideMsgComfirm();
	}
	internal void ReShowMsgComfirm()
	{
		msgUi.ReShowMsgComfirm();
	}

	internal int GetMsgComfirmCount()
	{
		return msgUi.GetCountMsgComfirm();
	}

    internal bool CanBack()
    {
        return msgUi.CanBack();
    }


    internal void ShowReceiveItem(CardSData[] _data, Action _cbClose)
	{
		msgUi.ShowReceiveItem(_data, _cbClose);
	}

	internal bool ShowInfo(ParaBase _para)
	{
		var para = _para.GetPara<InfoAlertPara>();
		msgUi.ShowInfo(para);
		return true;
	}

	internal void ShowLoadingIcon(bool _show)
	{
        //Debug.LogError("Loading Icon :" + _show);
		msgUi.ShowLoadingIcon(_show);
	}

	internal bool ShowTopBottomUI(SubSysType _type)//ParaBase _para)
	{
        //var type = _para.GetPara<ChangeSysPara>().nextSysType;
        switch (_type)
		{
			case SubSysType.Common:
			case SubSysType.Loading:
			case SubSysType.Result:
			case SubSysType.Battle:
            case SubSysType.PvPBattle:
            case SubSysType.DailyBattle:
            case SubSysType.RaidBattle:
                if (tbUi != null)
                    tbUi.HideUI();
				break;
			case SubSysType.Lobby:
                if (tbUi)
                {
                    tbUi.ShowSubResource(SubSysType.Lobby);
                    tbUi.ShowOnlyMenu();
                }
                else
                {
                    tbUi = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/TopBottomUI", GameCore.Instance.ui_root).GetComponent<TopBottomUI>();
                    tbUi.Init();
                    tbUi.ShowSubResource(SubSysType.Lobby);
                    tbUi.ShowOnlyMenu();
                }
                break;

			default:
				if (tbUi)
				{
					tbUi.ShowUI(_type);
					tbUi.ShowSubResource(_type);
				}
				break;
		}

        //if (_type != SubSysType.Loading && _type != SubSysType.Common)
        //    GameCore.Instance.NetMgr.Req_Notify_Friend_Mail_Count();
        if (tbUi != null)
            tbUi.UpdateIcon();
		return true;
	}

	internal bool AddToBottom(ParaBase _para)
	{
		var para = _para.GetPara<AddToBottomPara>();
		if (para.left)
		{
			if (para.clear)				tbUi.ClearToBL();
			if (para.go != null)	tbUi.AddToBL(para.go);
		}
		else
		{
			if (para.clear)				tbUi.ClearToBR();
			if (para.go != null)	tbUi.AddToBR(para.go);
		}

		return true;
	}

	internal bool SetReturnBtnCB(ParaBase _para)
	{
		tbUi.SetReturnBtnCB(_para.GetPara<SetReturnCBPara>().cb);
		return true;
	}

	internal void UpdateMoney()
	{
		tbUi.UpdateMoney();
	}

	internal void ResetUI()
	{
		tbUi.Reset();
	}

	internal void SetTitle(string _name)
	{
		tbUi.UpdateTitle(_name);
	}

    internal void ShowFriendWindow()
    {
        tbUi.OnClickFriend();
    }

    internal void ShowLoadingPage(bool _show, EventDelegate.Callback _cb = null)
    {
        msgUi.ShowLoadingPage(_show, _cb);
    }

    internal void ClearNotices()
    {
        notices.Clear();
        msgUi.ClearNotices();
    }

    internal void Update()
    {
        UpdateNotice();
    }

    void UpdateNotice()
    {
        for (int i = 0; i < notices.Count; ++i)
        {
            var notice = notices[i];
            if (!notice.ACTIVE)
                continue;

            if (GameCore.nowTime > notice.NextActiveTime)
            {
                msgUi.EnqNotice(notice);

                //Debug.Log(GameCore.nowTime + "  [" + notice.TYPE + "] Notice : " + notice.CONTENT);

                if (notice.INTERVAL == 0)
                    notice.ACTIVE = false;
                else
                    notice.UpdateNextActiveTime();
            }
        }
    }


    internal bool ANS_MYROOM_GetInfo(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.PlayerDataMgr.SetMyRoomData(para.GetField("MYROOM"));
                GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, null);
                break;

            //case 1: GameCore.Instance.ShowNotice("제조정보 업데이트 실패", "잘못된 데이터", 0); break;
            //case 2: GameCore.Instance.ShowNotice("제조정보 업데이트 실패", "슬롯정보 없음", 0); break;
            default: GameCore.Instance.ShowNotice("숙소 데이터 실패", "알 수 없는 에러 : " + code, 0); break;
        }

        GameCore.Instance.SubsysMgr.DecNeedPreloadDataCnt();
        return true;
    }



    internal bool ANS_NOTIFY_FRIEND_MAIL_COUNT(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;

        int friendCount = -1;
        int mailCount = -1;
        var Notice = data.GetField("NOTIFY");
        Notice.GetField(ref friendCount, "FCT");
        Notice.GetField(ref mailCount, "MCT");

        tbUi.FriendMailCheck(friendCount, mailCount);
        return true;
    }

    internal bool ANS_NOTIFY_FRIEND_COUNT(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;
        bool isGetFriend = false;
        data.GetField(ref isGetFriend, "NOTIFY");

        tbUi.GetNewFriendCheck(isGetFriend);
        //Debug.LogError("Friend Alert is " + isGetFriend);
        return true;
    }

    internal bool ANS_NOTIFY_MAIL_COUNT(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;
        bool isGetMail = false;

        data.GetField(ref isGetMail, "NOTIFY");
        tbUi.GetNewMailCheck(isGetMail);
        //Debug.LogError("Mail Alert is " + isGetMail);
        return true;
    }


    internal bool ANS_NOTIFY_NOTICE(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;

        notices.Clear();
        var json = data.GetField("NOTICE");
        if (json == null)
            return true;

        for(int i = 0; i < json.Count; ++i)
        {
            var sdata = new NoticeSData();
            sdata.SetData(json[i]);
            notices.Add(sdata);
        }

        return true;
    }


    private bool ANS_MAIL_LIST(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");
        switch (code)
        {
            case 0:
                var json = para.GetField("MAIL");
                var list = new List<MailSData>(json.Count);

                for (int i = 0; i < json.Count; ++i)
                {
                    var data = new MailSData();
                    data.SetData(json[i]);
                    list.Add(data);
                }

                mailbox = MailBoxComponent.Create(tbUi.transform);
                mailbox.Init(list);
                ShowMailBox(mailbox, mailbox.ItemCount == 0);
                
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }
        return false;
    }

    void ShowMailBox(MailBoxComponent _mailbox , bool _isEmpty)
    {
        if (!_isEmpty)
        {
            GameCore.Instance.ShowObject("우편함", null, _mailbox.gameObject, 3,
                new MsgAlertBtnData[] {
                            new MsgAlertBtnData("닫기", new EventDelegate(() => { GameCore.Instance.CloseMsgWindow(); tbUi.OnClosedMail(); })),
                            new MsgAlertBtnData("모두 받기", new EventDelegate(_mailbox.CBGetAll))
                });
        }
        else
        {
            GameCore.Instance.ShowObject("우편함", null, _mailbox.gameObject, 3,
                new MsgAlertBtnData[] {
                            new MsgAlertBtnData("닫기", new EventDelegate(() => { GameCore.Instance.CloseMsgWindow(); tbUi.OnClosedMail(); }))
                });
            CreateEmptyMail(_mailbox.transform, -87);
        }
    }

    public void CreateEmptyMail(Transform parent, float _paddingY = 0)
    {
        if (UnityCommonFunc.GetGameObjectByName(parent.parent.gameObject, "MailEmpty") != null)
            return;

        int fontSize = 30;

        GameObject mailEmpty = new GameObject("MailEmpty");
        UILabel uILabel = mailEmpty.AddComponent<UILabel>();
        UIPanel parentPanal = parent.GetComponent<UIPanel>();

        //폰트 설정 및 단어 입력
        mailEmpty.layer = LayerMask.NameToLayer("UI");
        uILabel.trueTypeFont = fontDefault;
        uILabel.fontSize = fontSize;
        uILabel.text = "새로운 우편이 없습니다.";
        uILabel.width = (int)parentPanal.width;
        uILabel.height = fontSize;
        uILabel.color = new Color(89f / 255f, 87f / 255f, 87f / 255f);

        //상속 이후 사이즈 및 위치 조절
        mailEmpty.transform.SetParent(parent.parent);
        mailEmpty.transform.localScale = new Vector3(1, 1, 1);
        //uILabel.transform.localPosition = new Vector3(0f, -parentPanal.height *0.5f, 0f);
        uILabel.transform.position = Vector3.zero;
        uILabel.transform.localPosition += Vector3.up * _paddingY;

        //parentPanal.GetComponent<UIScrollView>().ResetPosition();
    }


    private bool ANS_MAIL_GET(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");
        switch (code)
        {
            case 0:
                var list = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                bool isGacha = list != null && 0 < MailBoxItemScript.LastClickedUID && mailbox.GetSDataByUID(MailBoxItemScript.LastClickedUID).type == CardType.Immediate;

                mailbox.RemoveMail(MailBoxItemScript.LastClickedUID);
                MailBoxItemScript.LastClickedUID = -1;
                mailbox.ResetScrollViewOffset();

                if (GameCore.Instance.SubsysMgr.NowSysType == SubSysType.Inven)
                {
                    var sys = GameCore.Instance.SubsysMgr.GetNowSubSys() as InvenSys;
                    sys.ResetUI();
                }

                if (mailbox.ItemCount == 0)
                {
                    CreateEmptyMail(mailbox.transform);
                    tbUi.GetNewMailCheck(false);
                }

                if (isGacha)
                {
                    mailbox.gameObject.SetActive(false);
                    mailbox.transform.parent = GameCore.Instance.Ui_root;
                    GameCore.Instance.CloseMsgWindow();
                    GameCore.Instance.ShowReceiveItem(list, () =>
                    {
                        mailbox.gameObject.SetActive(true);
                        ShowMailBox(mailbox, mailbox.ItemCount == 0);
                        GameCore.Instance.DoWaitCall(mailbox.ResetScrollViewOffset);
                    });
                }
                else // 뽑기권이 아닐때만 효과음 출력
                {
                    GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Item_Get);
                    if (list == null || list.Length == 0)
                        GameCore.Instance.ShowNotice("우편 수령", "즉시 받을 수 있는 우편이 없습니다.", 0);
                    else
                        GameCore.Instance.ShowReceiveItemPopup("우편 수령", list);
                }

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "해당 우편 없음", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "아이템 슬롯 부족", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "캐릭터 슬롯 부족", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }
        return false;
    }

    

    private bool ANS_ITEM_GEN_CHECK(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");
        switch (code)
        {
            case 0:
                var json = para.GetField("ITEM");
                if (json != null)
                {
                    GameCore.Instance.PlayerDataMgr.SetCardSData(json);
                    GameCore.Instance.CommonSys.UpdateMoney();
                }

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }
        return false;
    }


    bool ANS_FRIEND_LIST(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");
        if (code == 0)
        {
            int del_Friend_Limit = 0;
            para.GetField(ref del_Friend_Limit, "DEL_FRIEND_LIMIT");
            GameCore.Instance.PlayerDataMgr.LocalUserData.DeleteFriendRemainingCount = del_Friend_Limit;
            GameCore.Instance.PlayerDataMgr.SetFriendSData(para.GetField("FRIEND_LIST"));

            GameCore.Instance.SubsysMgr.DecNeedPreloadDataCnt();

            return true;
        }
        else
        {
            GameCore.Instance.ShowNotice("실패", "친구 목록 불러오기 실패 " + code, 0);
            return false;
        }
    }

    public bool ANS_PUSH_SAVE(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        if (code == 0)
        {
            tbUi.SaveOptionAndCloseMsg();
            return true;
        }
        else
        {
            GameCore.Instance.ShowAlert("푸시알람 설정 저장 실패");
        }

        return false;
    }


    public bool ANS_MyroomHistoryList(ParaBase _para)
    {
        if (GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.MyRoom)
        {
            GameCore.Instance.SubsysMgr.DecNeedPreloadDataCnt();
            (GameCore.Instance.SubsysMgr.GetNowSubSys() as IDH.MyRoom.MyRoomSys).DoWaitOnLoad(()=>
                (GameCore.Instance.SubsysMgr.GetNowSubSys() as IDH.MyRoom.MyRoomSys).ANS_MyroomHistoryList(_para));
        }
        return true;
    }


    public bool ANS_RequestFreindList(ParaBase _para)
    {
        if (GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.MyRoom)
        {
            GameCore.Instance.SubsysMgr.DecNeedPreloadDataCnt();
            (GameCore.Instance.SubsysMgr.GetNowSubSys() as IDH.MyRoom.MyRoomSys).DoWaitOnLoad(()=>
                (GameCore.Instance.SubsysMgr.GetNowSubSys() as IDH.MyRoom.MyRoomSys).ANS_RequestFreindList(_para));
        }

        return true;
    }
}
