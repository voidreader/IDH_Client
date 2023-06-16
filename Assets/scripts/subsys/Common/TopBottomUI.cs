using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePotUnity;

public class TopBottomUI : MonoBehaviour
{
    public class IObserverTopBottomUI
    {
        public Action<bool> OnClickFriendUIIcon { get; set; }
        public Action<bool> OnClickMailIcon { get; set; }
    }

    public IObserverTopBottomUI Observer = new IObserverTopBottomUI();


    UITable[] aligins;
    UIAnchor[] anchors;

    UISprite BG;
    //top
    UISprite Top_BG;
    GameObject TL, TR;
    UIButton backButton;

    //bottom
    UISprite Bottom_BG;
    GameObject BL, BR;

    UILabel title_Label;
    UILabel gold_Label;
    UILabel cash_Label;
    UIButton btSupportRsc;
    UILabel lbSupportRsc;
    GameObject srTimer;
    UILabel lbSRTimer;

    Action cbRetrunBtn;

    UIButton btFriend;
    UIButton btMail;

    internal GameObject btnMenu;
    internal UIButton btnMainMenu;
    GameObject comboBoxRoot;

    SubSysType nowType;

    FriendUI friendUI;

    MainOption option;

    ResourceType resourceType;


    internal void Init()
    {
        aligins = new UITable[] {// UnityCommonFunc.GetComponentsByName<UITable>(gameObject);
			//UnityCommonFunc.GetComponentByName<UITable>(gameObject, "Left_T"),
			//UnityCommonFunc.GetComponentByName<UITable>(gameObject, "Right_T"),
			UnityCommonFunc.GetComponentByName<UITable>(gameObject, "Left_B"),
            UnityCommonFunc.GetComponentByName<UITable>(gameObject, "Right_B")
        };
        anchors = new UIAnchor[4];
        anchors[0] = UnityCommonFunc.GetComponentByName<UIAnchor>(gameObject, "Left_T");
        anchors[1] = UnityCommonFunc.GetComponentByName<UIAnchor>(gameObject, "Right_T");
        anchors[2] = UnityCommonFunc.GetComponentByName<UIAnchor>(gameObject, "Left_B");
        anchors[3] = UnityCommonFunc.GetComponentByName<UIAnchor>(gameObject, "Right_B");

        BG = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "background");

        Top_BG = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "BG_T");
        TL = UnityCommonFunc.GetGameObjectByName(gameObject, "Left_T");
        TR = UnityCommonFunc.GetGameObjectByName(gameObject, "Right_T");
        backButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "backButton");

        btnMenu = UnityCommonFunc.GetGameObjectByName(gameObject, "5_Menu_TR");
        title_Label = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Title");
        btSupportRsc = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "0_SupportRsc_TR");
        lbSupportRsc = UnityCommonFunc.GetComponentByName<UILabel>(btSupportRsc.gameObject, "text");
        srTimer = UnityCommonFunc.GetGameObjectByName(btSupportRsc.gameObject, "timer");
        lbSRTimer = srTimer.GetComponentInChildren<UILabel>();
        gold_Label = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "1_GoldLabel_TR");
        cash_Label = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "2_CashLabel_TR");


        Bottom_BG = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "BG_B");
        BL = UnityCommonFunc.GetGameObjectByName(gameObject, "Left_B");
        BR = UnityCommonFunc.GetGameObjectByName(gameObject, "Right_B");


        btFriend = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "3_Friend_TR");
        btMail = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "4_Mail_TR");
        comboBoxRoot = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("common/ComboRoot", transform);
        comboBoxRoot.SetActive(false);

        UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "1_Gold_TR").onClick.Add(new EventDelegate(OnClickGold));
        UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "2_Cash_TR").onClick.Add(new EventDelegate(OnClickCash));
        UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "3_Friend_TR").onClick.Add(new EventDelegate(OnClickFriend));
        UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "4_Mail_TR").onClick.Add(new EventDelegate(OnClickMail));
        btnMenu.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickMenu));
        UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "6_Option_TR").onClick.Add(new EventDelegate(OnClickOption));

        btnMainMenu = UnityCommonFunc.GetComponentByName<UIButton>(comboBoxRoot, "btLobby");
        btnMainMenu.onClick.Add(new EventDelegate(OnClickLobby));
        UnityCommonFunc.GetComponentByName<UIButton>(comboBoxRoot, "btStory").onClick.Add(new EventDelegate(OnClickStory));
        UnityCommonFunc.GetComponentByName<UIButton>(comboBoxRoot, "btAdventure").onClick.Add(new EventDelegate(OnClickAdventrue));
        UnityCommonFunc.GetComponentByName<UIButton>(comboBoxRoot, "btPVP").onClick.Add(new EventDelegate(OnClickPVP));
        UnityCommonFunc.GetComponentByName<UIButton>(comboBoxRoot, "btMyRoom").onClick.Add(new EventDelegate(OnClickMyRoom));
        UnityCommonFunc.GetComponentByName<UIButton>(comboBoxRoot, "btTeam").onClick.Add(new EventDelegate(OnClickTeam));
        UnityCommonFunc.GetComponentByName<UIButton>(comboBoxRoot, "btBag").onClick.Add(new EventDelegate(OnClickBag));
    }

    IEnumerator CoVigorTime(DateTime _createTime, float _genTime,  bool _vigor) // _vigor - true:vigor, false-pvpTocket
    {
        var sec = _genTime - (float)(GameCore.nowTime - _createTime).TotalSeconds;

        lbSRTimer.text = string.Format("{0:00}:{1:00}", (int)sec / 60, (int)sec % 60);
        yield return new WaitForSeconds(sec % 1);


        var delay = new WaitForSeconds(1f);
        do
        {
            sec = Mathf.Max(0f, _genTime - (float)(GameCore.nowTime - _createTime).TotalSeconds);
            lbSRTimer.text = string.Format("{0:00}:{1:00}", (int)sec / 60, (int)sec % 60);
            yield return delay;
        } while (sec > 0);


        if (_vigor) GameCore.Instance.NetMgr.Req_Check_GenItem(CommonType.ITEMKEY_VIGOR);
        else        GameCore.Instance.NetMgr.Req_Check_GenItem(CommonType.ITEMKEY_TICKET_PVP);
        srTimer.SetActive(false);
    }


    internal void Reposition()
    {
        for (int i = 0; i < aligins.Length; ++i)
        {
            aligins[i].Reposition();
            anchors[i].enabled = true;
        }
    }
    internal void Reposition(int _num)
    {
        if (0 <= _num && _num < aligins.Length)
        {
            aligins[_num].Reposition();
            anchors[_num].enabled = true;
        }
    }
    internal void SetActiveAnchor(bool _value, int _pos)
    {
        anchors[_pos].gameObject.SetActive(_value);
    }


    internal void HideUI()
    {
        gameObject.SetActive(false);
    }

    internal void ShowOnlyMenu()
    {
        gameObject.SetActive(true);
        BG.gameObject.SetActive(false);
        Top_BG.gameObject.SetActive(false);
        TR.SetActive(true);
        TL.SetActive(false);
        Bottom_BG.gameObject.SetActive(false);
        BL.SetActive(false);
        BR.SetActive(false);

        btSupportRsc.gameObject.SetActive(false);

        UpdateMoney();

        Reposition();
    }

    internal void FriendMailCheck(int friendCount, int mailCount)
    {
        btFriend.transform.GetChild(1).gameObject.SetActive(friendCount > 0);
        btMail.transform.GetChild(1).gameObject.SetActive(mailCount > 0);
    }

    internal void GetNewFriendCheck(bool isGetFriend)
    {
        btFriend.transform.GetChild(1).gameObject.SetActive(isGetFriend);
    }
    internal void GetNewMailCheck(bool isGetMail)
    {
        btMail.transform.GetChild(1).gameObject.SetActive(isGetMail);
    }

    /// <summary>
    /// Top, Bottom UI가 모두 보일때 호출된다.
    /// </summary>
    /// <param name="_para"></param>
    internal void ShowUI(SubSysType _type)
    {
        if (gameObject == null)
            return;

        switch (_type)
        {
            case SubSysType.Inven:
            case SubSysType.EditTeam:
            case SubSysType.PvPReady:
            case SubSysType.PvPMatch:
            case SubSysType.PvPGradeTest:
            case SubSysType.HeroInfo:
            case SubSysType.HeroUp:
            case SubSysType.ItemUp:
            case SubSysType.Farming:
                BG.spriteName = "BG_N_S_00_01"; break;

            case SubSysType.Story:
            case SubSysType.DailyPrepare:
            case SubSysType.RaidPrepare:
            case SubSysType.RaidBattle:
                BG.spriteName = "BG_N_S_02_01"; break;

            case SubSysType.Gacha:
            case SubSysType.Make:
                BG.spriteName = "BG_N_S_01_01"; break;
        }
        gameObject.SetActive(true);
        BG.gameObject.SetActive(true);
        Top_BG.gameObject.SetActive(true);
        TL.SetActive(true);
        TR.SetActive(true);
        Bottom_BG.gameObject.SetActive(true);
        BL.SetActive(true);
        BR.SetActive(true);

        ClearToBL();
        ClearToBR();

        // Set title
        //UpdateTitle(_type);

        // Set TR
        UpdateMoney();
        ShowSubResource(_type);


        // Set Bottom
        switch (_type)
        {
            case SubSysType.MyRoom:
                BG.gameObject.SetActive(false);
                break;
        }

        Reposition();
    }

    internal void ShowSubResource(SubSysType _type)
    {
        nowType = _type;
        StopAllCoroutines();
        switch (nowType)
        {
            case SubSysType.Story: ShowSubResouce_Vigor(); break;
            case SubSysType.Gacha: ShowSubResouce_Friendship(); break;
            case SubSysType.MyRoom: ShowSubResouce_Mileage(); break;
            case SubSysType.PvPGradeTest:
            case SubSysType.PvPMatch:
            case SubSysType.PvPReady:
            case SubSysType.PvPResult: ShowSubResouce_PVPTicket(); break;
            default: ShowSubResouce_None(); break;
        }
        //GameCore.Instance.NetMgr.Req_Notify_Friend_Mail_Count();
    }

    private void ShowSubResouce_None()
    {
        btSupportRsc.gameObject.SetActive(false);
    }

    private void ShowSubResouce_Vigor()
    {
        btSupportRsc.gameObject.SetActive(true);
        btSupportRsc.normalSprite = "ACT_01";
        btSupportRsc.pressedSprite = "ACT_02";

        var item = GameCore.Instance.PlayerDataMgr.GetItemByKey(CommonType.ITEMKEY_VIGOR);

        resourceType = ResourceType.Vigor;
        // Set Value
        lbSupportRsc.text = string.Format("{0}/{1}", item.count, GameCore.Instance.PlayerDataMgr.MaxVigor);

        // Set Timer
        if (gameObject.activeInHierarchy &&
            item.count < GameCore.Instance.PlayerDataMgr.MaxVigor)
        {
            srTimer.SetActive(true);
            var consts = GameCore.Instance.DataMgr.GetStaminaConstData();
            StartCoroutine(CoVigorTime(item.createDate, consts.createVigorTime, true));
        }
        else
        {
            srTimer.SetActive(false);
        }
    }

    private void ShowSubResouce_Mileage()
    {
        btSupportRsc.gameObject.SetActive(true);
        btSupportRsc.normalSprite = "M_01";
        btSupportRsc.pressedSprite = "M_01";

        // Set Value
        lbSupportRsc.text = GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Mailage).ToString("N0");
        resourceType = ResourceType.Mailage;
        srTimer.SetActive(false);
    }

    private void ShowSubResouce_Friendship()
    {
        btSupportRsc.gameObject.SetActive(true);
        btSupportRsc.normalSprite = "HEART_01";
        btSupportRsc.pressedSprite = "HEART_01";

        // Set Value
        lbSupportRsc.text = GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Friendship).ToString("N0");
        resourceType = ResourceType.Friendship;
        // ToDo : Show Timer
        srTimer.SetActive(false);
    }

    private void ShowSubResouce_PVPTicket()
    {
        btSupportRsc.gameObject.SetActive(true);
        btSupportRsc.normalSprite = "PVP_01";
        btSupportRsc.pressedSprite = "PVP_02";

        var item = GameCore.Instance.PlayerDataMgr.GetItemByKey(CommonType.ITEMKEY_TICKET_PVP);
        resourceType = ResourceType.PVPTicket;

        // Set Value
        var consts = GameCore.Instance.DataMgr.GetPvPConstData();
        lbSupportRsc.text = string.Format("{0}/{1}", item.count, consts.ticketMaxGenCount);

        // Set Timer
        if (gameObject.activeInHierarchy &&
            item.count < consts.ticketMaxGenCount)
        {
            srTimer.SetActive(true);
            StartCoroutine(CoVigorTime(item.createDate, consts.ticketGenTime, false));
        }
        else
        {
            srTimer.SetActive(false);
        }
    }


    public void SetActiveFriendUI(bool _show)
    {
        if (_show)
        {
            if (friendUI == null)
                friendUI = FriendUI.Create(transform, OnClickCloseFriend);

            friendUI.Show(true);
            if (Observer.OnClickFriendUIIcon != null) Observer.OnClickFriendUIIcon.Invoke(true);
        }
        else
        {
            if (friendUI != null)
                friendUI.Show(false);
            
            if (Observer.OnClickFriendUIIcon != null) Observer.OnClickFriendUIIcon.Invoke(false);
        }
    }



    internal void Reset()
    {
        comboBoxRoot.SetActive(false);
        UpdateMoney();
        UpdateIcon();
    }

    internal void UpdateMoney()
    {
        if (GameCore.Instance != null && GameCore.Instance.PlayerDataMgr != null)
        {
            gold_Label.text = GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Gold).ToString("N0");
            cash_Label.text = GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Cash).ToString("N0");
            ShowSubResource(nowType);
        }
        else
        {
            Debug.LogError("GameCore.Instance = " + GameCore.Instance);
            Debug.LogError("GameCore.Instance.PlayerDataMgr = " + GameCore.Instance.PlayerDataMgr);
        }
    }

    internal void UpdateIcon()
    {
        //UnityCommonFunc.GetGameObjectByName(btFriend.gameObject, "highlight").SetActive(false);
        //UnityCommonFunc.GetGameObjectByName(btMail.gameObject, "highlight").SetActive(false);
    }

    internal void UpdateTitle(string _name)
    {
        title_Label.text = _name;
    }

    internal void AddToBL(GameObject _go)
    {
        _go.transform.parent = BL.transform;
        UnityCommonFunc.ResetTransform(_go.transform);
        Reposition(2);
    }

    internal void AddToBR(GameObject _go)
    {
        _go.transform.parent = BR.transform;
        UnityCommonFunc.ResetTransform(_go.transform);
        Reposition(3);
    }

    internal void ClearToBL()
    {
        var cnt = BL.transform.childCount;
        for (int i = cnt - 1; i >= 0; --i)
            Destroy(BL.transform.GetChild(i).gameObject);
    }

    internal void ClearToBR()
    {
        var cnt = BR.transform.childCount;
        for (int i = cnt - 1; i >= 0; --i)
            Destroy(BR.transform.GetChild(i).gameObject);
    }

    internal void CloseBottomBR()
    {
        gameObject.transform.GetChild(2).gameObject.SetActive(false);
    }
    internal void UncloseBottomBR()
    {
        gameObject.transform.GetChild(2).gameObject.SetActive(true);
    }


    internal void SetReturnBtnCB(Action _cbReturnBtn)
    {
        cbRetrunBtn = _cbReturnBtn;
    }

    public void OnclickBackButton()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Back);

        if (cbRetrunBtn != null)
        {
            cbRetrunBtn();
        }
        else
        {
            GameCore.Instance.ChangeBack();
        }
    }
    public void OnClickNoticeTestButton()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Back);

        GamePot.showNotice();
    }
    public void OnClickFAQTestButton()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Back);
        GamePot.showCSWebView();
    }


    public void OnClickResource()
    {
        switch (resourceType)
        {
            case ResourceType.Vigor:
            case ResourceType.PVPTicket:
                if (GameCore.Instance.SubsysMgr.GetNowSysType() != SubSysType.Shop)
                    GameCore.Instance.ChangeSubSystem(SubSysType.Shop, new ShopPara() { openPos = 2 });
                break;

            case ResourceType.Friendship:
            case ResourceType.Mailage:
            case ResourceType.Honor:
            case ResourceType.Ticket_daily:
            case ResourceType.Ticket_Raid:
                break;
        }
    }

    public void OnClickGold()
    {
        if (GameCore.Instance.SubsysMgr.GetNowSysType() != SubSysType.Shop)
            GameCore.Instance.ChangeSubSystem(SubSysType.Shop, new ShopPara() { openPos = 1 });
    }


    public void OnClickCash()
    {
        if (GameCore.Instance.SubsysMgr.GetNowSysType() != SubSysType.Shop)
            GameCore.Instance.ChangeSubSystem(SubSysType.Shop, new ShopPara() { openPos = 0 });
    }


    public void OnClickFriend()
    {
        SetActiveFriendUI(true);
    }

    public void OnClickCloseFriend()
    {
        SetActiveFriendUI(false);
    }

    public bool GetFriendType()
    {
        if (friendUI != null) return friendUI.gameObject.activeSelf;
        else return false;
    }

    private void OnClickMail()
    {
        GameCore.Instance.NetMgr.Req_Mail_List();
        if (Observer.OnClickMailIcon != null) Observer.OnClickMailIcon.Invoke(true);
    }

    public void OnClosedMail()
    {
        if (Observer.OnClickMailIcon != null) Observer.OnClickMailIcon.Invoke(false);
    }

    private void OnClickMenu()
    {
        comboBoxRoot.SetActive(!comboBoxRoot.activeSelf);
        if (comboBoxRoot.activeSelf)
        {
            comboBoxRoot.transform.position = btnMenu.transform.position;
            comboBoxRoot.transform.localPosition += new Vector3(-60, -11);
        }
    }

    public void OnCloseMenu()
    {
        if (comboBoxRoot.activeSelf)
        {
            comboBoxRoot.SetActive(false);
        }
    }

    public bool GetActiveMenu()
    {
        return comboBoxRoot.activeSelf;
    }

    private void OnClickOption()
    {
        var mainOption = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Option/MainOption", this.transform);
        //GameObject soundOption = mainOption.GetComponent<MainOption>().SoundOption;
        //GameCore.Instance.SoundMgr.SetVolumeSlider(ref soundOption);
        option = mainOption.GetComponent<MainOption>();
        option.Init();
        GameCore.Instance.ShowObject("게임 설정", null, mainOption, 3, new MsgAlertBtnData[] {
            new MsgAlertBtnData("취소", new EventDelegate(()=> {
                GameCore.Instance.CloseMsgWindow();
            }), true, null, SFX.Sfx_UI_Cancel),
            new MsgAlertBtnData("저장", new EventDelegate(() => {
                if(option.IsChangePushSetting())
                    GameCore.Instance.NetMgr.Req_Push_Save(option.pushSetting);
                else
                    SaveOptionAndCloseMsg();
            }), true, null, SFX.Sfx_UI_Confirm) });
        //GameCore.Instance.ShowAlert("옵션 미구현");
    }

    public void SaveOptionAndCloseMsg()
    {
        if (option == null)
            return;

        option.Save();
        option = null;
        GameCore.Instance.CloseMsgWindow();
    }

    private void OnClickLobby()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        if (GameCore.Instance.SubsysMgr.NowSysType != SubSysType.Lobby)
        {
            GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
        }
        Reset();
    }

    private void OnClickStory()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        if (GameCore.Instance.SubsysMgr.NowSysType != SubSysType.Story)
        {
            GameCore.Instance.ChangeSubSystem(SubSysType.Story, null);
        }
        Reset();
    }

    private void OnClickAdventrue()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.Instance.ShowObject("", null, AdventureUI.Create(transform).gameObject, 4, new MsgAlertBtnData[] { new MsgAlertBtnData("닫기", null) });
        Reset();
    }

    private void OnClickPVP()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        if (GameCore.Instance.SubsysMgr.NowSysType != SubSysType.PvPReady &&
             GameCore.Instance.PlayerDataMgr.PvPData.placement)
        {
            GameCore.Instance.ChangeSubSystem(SubSysType.PvPReady, null);
        }
        else if (GameCore.Instance.SubsysMgr.NowSysType != SubSysType.PvPReady &&
             !GameCore.Instance.PlayerDataMgr.PvPData.placement)
            GameCore.Instance.ChangeSubSystem(SubSysType.PvPGradeTest, null);
        //Debug.Log(GameCore.Instance.PlayerDataMgr.PvPData.placement);
        Reset();
    }

    private void OnClickMyRoom()
    {
        if (GameCore.Instance.SubsysMgr.NowSysType != SubSysType.MyRoom)
            GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, null);
        Reset();
    }

    private void OnClickTeam()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        if (GameCore.Instance.SubsysMgr.NowSysType != SubSysType.EditTeam)
        {
            GameCore.Instance.ChangeSubSystem(SubSysType.EditTeam, null);
        }
        Reset();
    }

    private void OnClickBag()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        if (GameCore.Instance.SubsysMgr.NowSysType != SubSysType.Inven)
        {
            GameCore.Instance.ChangeSubSystem(SubSysType.Inven, null);
        }
        Reset();
    }

    private void Update()
    {
        if (comboBoxRoot.activeInHierarchy)
        {
            //#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0) && (GameCore.Instance.lobbyTutorial == null || GameCore.Instance.lobbyTutorial.IsRunning == false))
            //#else
            //			if(Input.GetTouch(0).phase == TouchPhase.Began)
            //#endif
            {
                RaycastHit2D hit;
                if (UnityCommonFunc.GetCameraHitInfo2D(GameCore.Instance.GetUICam(), out hit, "UI"))
                {
                    var Colls = comboBoxRoot.GetComponentsInChildren<Collider2D>();
                    var btnColls = btnMenu.GetComponent<Collider2D>();
                    for (int i = 0; i < Colls.Length; ++i)
                    {
                        if (Colls[i] == hit.collider)
                            return;
                    }
                    if (btnColls == hit.collider) return;
                }
                comboBoxRoot.SetActive(false);
            }
        }
    }

    public Transform GetBackButtonTransform { get { return backButton.transform; } }
}
