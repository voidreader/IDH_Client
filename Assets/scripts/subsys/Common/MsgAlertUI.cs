using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal enum MsgAlertType
{
    Alert,
    Notice,
    Ask,

    TeamSkillSelect,
    Filtering,
    ItemInfo,
    Reward,
}
internal enum MoneyType
{
    Gold = 0,
    Pearl = 1,
    HeroTicket = 7,
    ItemTicket = 8,
    FriendshipPoint = 9,
    Cash = 5,
    None = 6,
}
internal struct MsgAlertBtnData
{
    internal string text;
    internal EventDelegate ed;
    internal bool enable;
    internal string btnName;
    internal SFX clickSound;

    internal MsgAlertBtnData(string _txt, EventDelegate _ed, bool _enable = true, string _btnName = null, SFX _clickSound = SFX.Sfx_UI_Button)
    {
        text = _txt;
        ed = (_ed != null) ? _ed : new EventDelegate(() => {
            GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, null));
            //GameCore.Instance.SoundMgr.SetCommonBattleSound(_txt == "취소" ? SFX.Sfx_UI_Cancel : _txt == "확인" ? SFX.Sfx_UI_Confirm : SFX.Sfx_UI_Button);
        });
        enable = _enable;
        btnName = _btnName ?? "BTN_01_01_{0:d2}"; // _color ?? new Color(126f / 255f, 0f, 1f);
        clickSound = _clickSound;
    }
}

internal class MsgAlertPara : ParaBase
{
    internal MsgAlertType type;
    internal MoneyType moneyType;
    internal string title;
    internal string message;
    internal string highlight;
    internal GameObject obj;
    internal int size;
    internal MsgAlertBtnData[] btns;
    internal bool isVertical;

    internal int fontSize;
    internal string message2;
    internal int fontSize2;

    internal bool isCanBack = true;

    internal MsgAlertPara(MsgAlertType _type, string _title, MsgAlertBtnData[] _btns, int _size, string _msg, string _hl = null, GameObject _obj = null, bool _isVertical = false, int _fontSize = 22, string _msg2 = "", int _fontSize2 = 0, MoneyType _moneyType = MoneyType.None, bool _canBack = true)
    {
        type = _type;
        title = _title;
        message = _msg;
        highlight = _hl;
        btns = _btns;
        size = _size;
        obj = _obj;
        isVertical = _isVertical;
        fontSize = _fontSize;
        message2 = _msg2;
        fontSize2 = _fontSize2;
        moneyType = _moneyType;
        isCanBack = _canBack;
    }

    internal static MsgAlertPara CreateAlertMsg(string _msg)
    {
        return new MsgAlertPara(MsgAlertType.Alert, null, null, -1, _msg, null);
    }

    internal static MsgAlertPara CreateNoticeMsg(string _title, string _msg, int _size = -1, bool _canBack = true)
    {
        return new MsgAlertPara(MsgAlertType.Notice, _title, new MsgAlertBtnData[] {
            new MsgAlertBtnData("확인", null) }, 
            _size, _msg, null, null, false, 20, "", 0, MoneyType.None, _canBack);
    }
    internal static MsgAlertPara CreateNoticeMsg(string _title, string _msg, EventDelegate.Callback _cbCorrect, int _size = -1, bool _canBack = true)
    {
        return new MsgAlertPara(MsgAlertType.Notice, _title, new MsgAlertBtnData[] {
            new MsgAlertBtnData("확인", new EventDelegate(_cbCorrect)) }, 
            _size, _msg, null, null, false, 20, "", 0, MoneyType.None, _canBack);
    }
    internal static MsgAlertPara CreateNoticeMsg(string _title, string _msg, string _highlight, int _size = -1, bool _canBack = true)
    {
        return new MsgAlertPara(MsgAlertType.Notice, _title, new MsgAlertBtnData[] {
            new MsgAlertBtnData("확인", null) }, 
            _size, _msg, _highlight, null, false, 20, string.Empty, 0, MoneyType.None, _canBack);
    }
    internal static MsgAlertPara CreateAgreeMsg(string _title, string _msg, string _highlight, MoneyType moneyType, EventDelegate.Callback _cbCorrect, int _size = -1, bool _canBack = true)
    {
        return new MsgAlertPara(MsgAlertType.Notice, _title, new MsgAlertBtnData[] {
            new MsgAlertBtnData("취소", null), new MsgAlertBtnData("확인", new EventDelegate(_cbCorrect)) },
            _size, _msg, _highlight, null, false, 20, "", 0, moneyType, _canBack);
    }
    internal static MsgAlertPara CreateAgreeMsg(string _title, string _msg, string _highlight, MoneyType moneyType, EventDelegate.Callback _cbOKCorrect, EventDelegate.Callback _cbCancelCorrect, int _size = -1, bool _canBack = true)
    {
        return new MsgAlertPara(MsgAlertType.Notice, _title, new MsgAlertBtnData[] {
            new MsgAlertBtnData("취소", new EventDelegate(_cbCancelCorrect)),
            new MsgAlertBtnData("확인", new EventDelegate(_cbOKCorrect)) }, 
            _size, _msg, _highlight, null, false, 20, "", 0, moneyType, _canBack);
    }

    internal static MsgAlertPara CreateAgreeMsg(string _title, string _msg, string _highlight, MoneyType moneyType, string _strCorrectBtn, EventDelegate.Callback _cbCorrect, int _size = -1, bool _canBack = true)
    {
        return new MsgAlertPara(MsgAlertType.Notice, _title, new MsgAlertBtnData[] {
            new MsgAlertBtnData("취소", null), new MsgAlertBtnData(_strCorrectBtn, new EventDelegate(_cbCorrect)) },
            _size, _msg, _highlight, null, false, 20, "", 0, moneyType, _canBack);
    }

}


internal class InfoAlertPara : ParaBase
{
    internal CardSData sdata;
    internal bool bHave;
    internal int texture;

    internal ShopPackageSData packetSData;

    internal InfoAlertPara(CardSData _sdata, bool _have)
    {
        sdata = _sdata;
        bHave = _have;
    }


    // 상점용
    internal InfoAlertPara(ShopPackageSData _packetSData)
    {
        sdata = null;
        bHave = false;
        packetSData = _packetSData;
    }
}


internal class MsgAlertUI : MonoBehaviour
{
    Stack<MsgAlertWindow> ConfirmStack;

    [SerializeField] NoticeCtrl _noticeCtrl;
    [SerializeField] GameObject _blandPanel;
    [SerializeField] UITweener _alertTweener;
    [SerializeField] UILabel _alertLabel;
    [SerializeField] GameObject _loadingIcon;
    [SerializeField] GameObject _loading;
    [SerializeField] UITweener[] _loadingTweeners;
    [SerializeField] private UI2DSprite[] _spriteLoadings;
    [SerializeField] private Sprite[] _spriteLoadingResources;
    [SerializeField] internal ReceiveEffectUI _receiveEffectUI;

    UITweener infoTw;
    CardInfoScript info;

    bool isClosable; // alert ui 마우스 클릭으로 끌 수 있는지 여부.(완전히 나온 후에야 가능해짐)


    internal void Init(bool _showBlind)
    {
        ConfirmStack = new Stack<MsgAlertWindow>();

        #region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
        // [NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.

        //_alertTweener = UnityCommonFunc.GetComponentByName<UITweener>(gameObject, "AlertBox");
        //_alertLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Msg");
        //_receiveEffectUI = UnityCommonFunc.GetComponentByName<ReceiveEffectUI>(gameObject, "ReceiveItemRoot");
        //_loadingIcon = UnityCommonFunc.GetGameObjectByName(gameObject, "LoadingIcon");
        //_loading = UnityCommonFunc.GetGameObjectByName(gameObject, "PanelLoading");
        //_loadingTweeners = _loading.GetComponentsInChildren<UITweener>();
        //_spriteLoadings = _loading.GetComponentsInChildren<UI2DSprite>();
        //_noticeCtrl = UnityCommonFunc.GetComponentByName<NoticeCtrl>(gameObject, "PanelNotice");
        //_blandPanel = UnityCommonFunc.GetGameObjectByName(gameObject, "Background");

        #endregion

        var tw = _blandPanel.GetComponent<UITweener>();
        if (_showBlind)
        {
            tw.onFinished.Clear();
            tw.onFinished.Add(new EventDelegate(() =>
            {
                _blandPanel.SetActive(false);
                _blandPanel.GetComponent<UISprite>().color = new Color(0f, 0f, 0f, 0.9f);
            }));
        }
        else
        {
            tw.enabled = false;
            _blandPanel.SetActive(false);
            _blandPanel.GetComponent<UISprite>().color = new Color(0f, 0f, 0f, 0.9f);
        }
    }

    internal void ShowMessage(ParaBase _para)
    {
        var para = _para.GetPara<MsgAlertPara>();
        if (para.type == MsgAlertType.Alert)
            ShowAlert(para.message);
        else
            ShowMsgComfirm(para);
    }

    internal void ShowMsgComfirm(MsgAlertPara _para)
    {
        if (ConfirmStack.Count != 0)
        {
            ConfirmStack.Peek().HideMsgAlert();
        }

        var msgAlert = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/ConfirmWindow", transform).GetComponent<MsgAlertWindow>();
        msgAlert.ShowMsgAlert(_para);
        ConfirmStack.Push(msgAlert);

        _blandPanel.SetActive(true);
    }

    internal void RemoveMsgComfirm()
    {
        if (ConfirmStack.Count != 0)
        {
            var msgAlert = ConfirmStack.Pop();
            GameObject.Destroy(msgAlert.gameObject);

            ReShowMsgComfirm();
        }


        if (ConfirmStack.Count == 0)
            _blandPanel.SetActive(false);
    }

    internal void HideMsgComfirm()
    {
        if (ConfirmStack.Count != 0)
            ConfirmStack.Peek().HideMsgAlert();
        _blandPanel.SetActive(false);
    }
    internal void ReShowMsgComfirm()
    {
        if (ConfirmStack.Count != 0)
            ConfirmStack.Peek().ShowMsgAlert();

        _blandPanel.SetActive(true);
    }

    internal int GetCountMsgComfirm()
    {
        return ConfirmStack.Count;
    }

    internal bool CanBack()
    {
        return ConfirmStack.Peek().canBack;
    }

    internal void ShowAlert(string _msg)
    {
        if (_msg == null)
        {
            HideAlert();
            return;
        }

        if (_alertTweener)
        {
            isClosable = false;
            _alertTweener.gameObject.SetActive(true);
            CancelInvoke("HideAlert");

            _alertLabel.text = _msg;
            //resize
            _alertLabel.GetComponent<UIAnchor>().enabled = true;

            _alertTweener.onFinished.Clear();
            _alertTweener.onFinished.Add(new EventDelegate(() =>
            {
                isClosable = true;
                Invoke("HideAlert", 5f);
            }));
            _alertTweener.PlayForward();
            _alertTweener.ResetToBeginning();
        }
    }

    internal void HideAlert()
    {
        CancelInvoke("HideAlert");
        isClosable = false;
        _alertTweener.onFinished.Clear();
        _alertTweener.onFinished.Add(new EventDelegate(() => _alertTweener.gameObject.SetActive(false)));
        _alertTweener.PlayReverse();
    }

    public void OnClickAlertBox()
    {
        HideAlert();
    }


    public void EnqNotice(NoticeSData _notice)
    {
        _noticeCtrl.EnqNotice(_notice);
    }

    public void ClearNotices()
    {
        _noticeCtrl.ClearData();
    }

    internal void ShowInfo(InfoAlertPara _para)
    {
        if (info == null)
        {
            var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/Card_Info_Popup", _alertTweener.transform.parent);
            info = go.GetComponent<CardInfoScript>();
            infoTw = go.GetComponent<UITweener>();
        }

        if(_para.packetSData != null)
            info.InitByStore(_para.packetSData);
        else
            info.Init(_para.sdata, _para.bHave, true, _para.texture);
    
        //GameCore.Instance.ReturnTutorialData();
        infoTw.PlayForward();

        StartCoroutine(CoDisableInfo());
    }

    IEnumerator CoDisableInfo()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        while (!Input.GetMouseButtonUp(0))
            yield return null;
#else
		while (Input.touchCount != 0 && !(Input.GetTouch(0).phase == TouchPhase.Ended))
			yield return null;
#endif
        
        infoTw.PlayReverse();
    }

    internal void ShowReceiveItem(CardSData[] _data, Action _cbClose)
    {
        _receiveEffectUI.ShowReceiveItem(_data, _cbClose);
    }

    internal void ShowLoadingIcon(bool _show)
    {
        _loadingIcon.SetActive(_show);
    }

    internal void ShowLoadingPage(bool _show, EventDelegate.Callback _cb = null)
    {
        if (_loading.activeSelf == _show)
            return;

        //Debug.Log("Show Loading Page : " + _show + "\t\t" + Time.time);
        _loadingTweeners[0].onFinished.Clear();

        if (_show)
        {
            int randNum = UnityEngine.Random.Range(0, _spriteLoadingResources.Length);
            for (int i = 0; i < _spriteLoadings.Length; i++)
            {
                _spriteLoadings[i].sprite2D = _spriteLoadingResources[randNum];
            }
            _loading.SetActive(true);
            if (_cb != null)
                _loadingTweeners[0].SetOnFinished(_cb);

            for (int i = 0; i < _loadingTweeners.Length; ++i)
                _loadingTweeners[i].PlayForward();
        }
        else
        {
            _loadingTweeners[0].SetOnFinished(() => {
                if (_cb != null) _cb();
                _loading.SetActive(false);
                for (int i = 0; i < _loadingTweeners.Length; ++i)
                {
                    _loadingTweeners[i].PlayForward();
                    _loadingTweeners[i].ResetToBeginning();
                }
                GameCore.Instance.ActionAfterLoading();
            });

            for (int i = 0; i < _loadingTweeners.Length; ++i)
                _loadingTweeners[i].PlayReverse();
        }
    }

    private void Update()
    {
        if (isClosable)
        {
            //#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonUp(0))
            //#elif UNITY_ANDROID
            //			if (Input.GetTouch(0).phase == TouchPhase.Ended)
            //#endif
            {
                HideAlert();
            }
        }
    }
}
