using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendUI : MonoBehaviour , IEventHandler
{
    public enum Page
    {
        Friend,
        Search,
        Recommend,
        Acceptable,
        //Request,    // UI상 보이진 않음
        Count
    }

    public static FriendUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/PanelFriendWindow", _parent);
        var result = go.GetComponent<FriendUI>();
        result.Init();
        return result;
    }

    public static FriendUI Create(Transform _parent, Action closeCallBack)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/PanelFriendWindow", _parent);
        var result = go.GetComponent<FriendUI>();
        result.Init();
        result.OnCloseFriendUI = closeCallBack;
        return result;
    }

    static string[][] strInfo = new string[][] {
        new string[] { "우정포인트는 친구에게 보낼 때도 받을 수 있습니다.", "친구 삭제는 하루 다섯 명까지만 가능합니다." },
        new string[] { "친구 이름은 정확하게 입력해주세요(대소문자구분)" },
        new string[] { "추천 친구는 유저 레벨을 기준으로 선정됩니다." },
        new string[] { "친구 신청은 일주일간 유지 됩니다." }
    };

    [SerializeField] UILabel lbHead;
    [SerializeField] UIButton[] tabButtons;
    [SerializeField] UIGrid[] listRoot;
    [SerializeField] Transform tfScroolView;
    [SerializeField] UILabel lbMessage;
    [SerializeField] GameObject FriendshipButtonRoot;
    [SerializeField] GameObject searchField;
    [SerializeField] GameObject refreshButton;
    [SerializeField] UIInput ipSearch;
    [SerializeField] GameObject btnSearchCancel;
    [SerializeField] UILabel SearchWarnMessage;

    List<FriendListItemScript>[] items;

    int tabIdx = 0;
    int nowRecommendPage = 0;
    FriendListItemScript lastActiveItem;
    int strInfoIdx = 0;
    Coroutine coUpdateInfoMessage;

    bool bSendAll = false;
    bool bReceiveAll = false;

    public Action OnCloseFriendUI { get; set; }
    bool searchFlag = false;

    public void Init()
    {
        for (int i = 0; i < tabButtons.Length; ++i)
        {
            var n = i;
            tabButtons[i].onClick.Add(new EventDelegate(() =>
            {
                SwitchingTab(n);
            }));
        }

        listRoot[(int)Page.Friend].onCustomSort = CBFriendListItemCompare;
        
        items = new List<FriendListItemScript>[(int)Page.Count] {
        new List<FriendListItemScript>(),
        new List<FriendListItemScript>(),
        new List<FriendListItemScript>(),
        new List<FriendListItemScript>(),
        //new List<FriendListItemScript>(),
        };

        

        ipSearch.onSelect.Add(new EventDelegate(() => ipSearch.value = ""));
    }

    public bool HandleMessage(GameEvent _evt)
    {
        var code = -1;
        var json = _evt.Para.GetPara<PacketPara>().data.data;
        json.GetField(ref code, "result");
        switch (_evt.EvtType)
        {
            case GameEventType.ANS_FRIEND_LIST:             ANS_FRIEND_LIST(json, code); break;
            case GameEventType.ANS_FRIEND_SEARCH:           ANS_FRIEND_SEARCH(json, code); break;
            case GameEventType.ANS_FRIEND_REQUEST:          ANS_FRIEND_REQUEST(json, code); break;
            case GameEventType.ANS_FRIEND_RECOMMENDLIST:    ANS_FRIEND_RECOMMENDLIST(json, code); break;
            case GameEventType.ANS_FRIEND_REQUESTEDLIST:    ANS_FRIEND_REQUESTEDLIST(json, code); break;
            case GameEventType.ANS_FRIEND_ACCEPTABLELIST:   ANS_FRIEND_ACCEPTABLELIST(json, code); break;
            case GameEventType.ANS_FRIEND_ACCEPT_OK:        ANS_FRIEND_ACCEPT_OK(json, code); break;
            case GameEventType.ANS_FRIEND_ACCEPT_NO:        ANS_FRIEND_ACCEPT_NO(json, code); break;
            case GameEventType.ANS_FRIEND_REMOVE:           ANS_FRIEND_REMOVE(json, code); break;
            case GameEventType.ANS_FRIEND_SEND:             ANS_FRIEND_SEND(json, code); break;
            case GameEventType.ANS_FRIEND_RECEIVE:          ANS_FRIEND_RECEIVE(json, code); break;
            case GameEventType.ANS_FRIEND_TEAMINFO:         ANS_FRIEND_TEAMINFO(json, code); break;

            default:return false;
        }
        return true;
    }

    void ResetScroll()
    {
        tfScroolView.transform.localPosition = Vector3.zero;
        var sp = tfScroolView.GetComponent<SpringPanel>();
        if (sp != null)
            sp.enabled = false;
    }

    void AddItem(Page _page, FriendListItemScript _item)
    {
        items[(int)_page].Add(_item);
        listRoot[(int)_page].GetComponent<UIGrid>().enabled = true;
    }

    bool RemoveItem(Page _page, long _uid)
    {
        var list = items[(int)_page];
        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].data.USER_UID == _uid)
            {
                Destroy(list[i].gameObject);
                list.RemoveAt(i);
                listRoot[(int)_page].GetComponent<UIGrid>().enabled = true;
                if (_page == Page.Friend)
                    SetFriendCount(list.Count);

                if (list.Count == 0)
                    SetActiveHighlight(3, false);

                return true;
            }
        }

        return false;
    }

    void SetFriendCount(int _count)
    {
        var maxCount = GameCore.Instance.DataMgr.GetFriendConstData().CalcMaxFrinedCount(GameCore.Instance.PlayerDataMgr.Level);
        lbHead.text = string.Format(CSTR.FriendCount, _count, maxCount);
    }

    void ClearList(Page _page)
    {
        var list = items[(int)_page];
        for (int i = 0; i < list.Count; ++i)
        {
            list[i].transform.parent = GameCore.Instance.Ui_root;
            Destroy(list[i].gameObject);
        }

        list.Clear();
    }

    public void Show(bool _show)
    {
        gameObject.SetActive(_show);

        if (_show)
        {
            // Regster Envet Handler
            GameCore.Instance.EventMgr.RegisterHandler(this,
                GameEventType.ANS_FRIEND_LIST,
                GameEventType.ANS_FRIEND_SEARCH,
                GameEventType.ANS_FRIEND_REQUEST,
                GameEventType.ANS_FRIEND_RECOMMENDLIST,
                GameEventType.ANS_FRIEND_REQUESTEDLIST,
                GameEventType.ANS_FRIEND_ACCEPTABLELIST,
                GameEventType.ANS_FRIEND_ACCEPT_OK,
                GameEventType.ANS_FRIEND_ACCEPT_NO,
                GameEventType.ANS_FRIEND_REMOVE,
                GameEventType.ANS_FRIEND_SEND,
                GameEventType.ANS_FRIEND_RECEIVE,
                GameEventType.ANS_FRIEND_TEAMINFO);

            SwitchingTab(0);

            GameCore.Instance.NetMgr.Req_Friend_List(); // 친구 목록
            nowRecommendPage = 1;
            GameCore.Instance.NetMgr.Req_Friend_RecommendList(nowRecommendPage);// 친구 추천
            //GameCore.Instance.NetMgr.Req_Friend_RequestedList();// 친구 요청 목록
            GameCore.Instance.NetMgr.Req_Friend_AcceptableList();// 친구 수락 목록
        }
        else
        {
            // Unregster Event Handler
            GameCore.Instance.EventMgr.UnregisterHandler(this);
            ClearList(Page.Friend);
            ClearList(Page.Recommend);
            ClearList(Page.Search);
            ClearList(Page.Acceptable);
        }
    }

    private void SwitchingTab(int _n)
    {
        ResetScroll();

        tabButtons[tabIdx].GetComponent<UISprite>().spriteName = "BTN_06_01_01";
        tabButtons[tabIdx].transform.localScale = new Vector3(1f, 1f);
        tabButtons[tabIdx].transform.GetChild(0).gameObject.SetActive(false);
        listRoot[tabIdx].gameObject.SetActive(false);

        tabIdx = _n;

        tabButtons[tabIdx].GetComponent<UISprite>().spriteName = "BTN_06_01_02";
        tabButtons[tabIdx].transform.localScale = new Vector3(1.1f, 1.1f);
        tabButtons[tabIdx].transform.GetChild(0).gameObject.SetActive(true);
        listRoot[tabIdx].gameObject.SetActive(true);

        FriendshipButtonRoot.SetActive(tabIdx == 0);
        searchField.SetActive(tabIdx == 1);
        refreshButton.SetActive(tabIdx == 2);

        if (tabIdx == 1) {
            ipSearch.value = "";
            btnSearchCancel.SetActive(false);
            searchFlag = false;
        }

        // Info Message  Set
        if (coUpdateInfoMessage != null)
            StopCoroutine(coUpdateInfoMessage);

        var infoCount = strInfo[tabIdx].Length;
        strInfoIdx = UnityEngine.Random.Range(0, infoCount);

        if (infoCount > 1)
            coUpdateInfoMessage = StartCoroutine(CoUpdateInfoMessage());

        lbMessage.text = strInfo[tabIdx][strInfoIdx];
        var tw = lbMessage.GetComponent<UITweener>();
        tw.ResetToBeginning();
        tw.PlayForward();
        
        if(tabIdx == 1) GameCore.Instance.NetMgr.Req_Friend_Search("");
        else if(tabIdx == 2) GameCore.Instance.NetMgr.Req_Friend_RecommendList(1);
        else if(tabIdx == 3) GameCore.Instance.NetMgr.Req_Friend_RequestedList();
    }

    IEnumerator CoUpdateInfoMessage()
    {
        var tw = lbMessage.GetComponent<UITweener>();

        var waitTime = new WaitForSeconds(3f);
        var waitDuration = new WaitForSeconds(tw.duration);

        while (true)
        {
            yield return waitTime;

            tw.PlayReverse();

            yield return waitDuration;

            var infoCount = strInfo[tabIdx].Length;
            strInfoIdx = (strInfoIdx + 1) % infoCount;
            lbMessage.text = strInfo[tabIdx][strInfoIdx];

            tw.PlayForward();

            yield return waitDuration;
        }
    }

    public void OnClickSendAll()
    {
        int count = 0;
        var list = items[(int)Page.Friend];
        for (int i = 0; i < list.Count; ++i)
            if (list[i].IsActiveFriendshipSendBtn())
                ++count;

        if (count == 0)
            GameCore.Instance.ShowAlert(CSTR.MSG_CantSendFriendship);
        else
        {
            GameCore.Instance.NetMgr.Req_Friend_Send(0);
            bSendAll = true;
        }

    }


    public void OnClickReciveAll()
    {
        int count = 0;
        var list = items[(int)Page.Friend];
        for (int i = 0; i < list.Count; ++i)
            if (list[i].IsActiveFriendshipReceiveBtn())
                ++count;

        if (count == 0)
            GameCore.Instance.ShowAlert(CSTR.MSG_CantReceiveFriendship);
        else
        {
            GameCore.Instance.NetMgr.Req_Friend_Receive(0);
            bReceiveAll = true;
        }
    }

    public void OnClickReFresh()
    {
        nowRecommendPage = (nowRecommendPage % 5) + 1;
        GameCore.Instance.NetMgr.Req_Friend_RecommendList(nowRecommendPage);// 친구 추천
    }
    public void OnCancelSearch()
    {
        searchFlag = false;
        ipSearch.RemoveFocus();
        ipSearch.value = "";
        btnSearchCancel.SetActive(false);
        SearchWarnMessage.gameObject.SetActive(false);
        GameCore.Instance.NetMgr.Req_Friend_Search(ipSearch.value);
    }

    public void OnSubmitSearch()
    {
        if(ipSearch.value == "") return;
        searchFlag = true;
        ipSearch.RemoveFocus();
        btnSearchCancel.SetActive(true);
        GameCore.Instance.NetMgr.Req_Friend_Search(ipSearch.value);
    }

    public void OnClickClose()
    {
        Show(false);
        if (OnCloseFriendUI != null) OnCloseFriendUI.Invoke();
    }

    bool CBOnLastActiveItem(FriendListItemScript _item)
    {
        if (lastActiveItem == null)
        {
            lastActiveItem = _item;
            return true;
        }

        return false;
    }

    int CBFriendListItemCompare(Transform _1, Transform _2)
    {
        var data1 = _1.GetComponent<FriendListItemScript>().data;
        var data2 = _2.GetComponent<FriendListItemScript>().data;

        if (data1.DELEGATE_TEAM_POWER != data2.DELEGATE_TEAM_POWER)
            return data1.DELEGATE_TEAM_POWER < data2.DELEGATE_TEAM_POWER ? 1 : -1; 

        if(data1.USER_LEVEL != data2.USER_LEVEL)
            return data1.USER_LEVEL < data2.USER_LEVEL ? 1 : -1;

        return data1.USER_NAME.CompareTo(data2.USER_NAME);
    }

    void SetActiveHighlight(int idx, bool _active)
    {
        tabButtons[idx].transform.GetChild(2).gameObject.SetActive(_active);

        // CheckTotal highlight off?
        bool on = false;
        foreach (var tab in tabButtons)
            if (tabButtons[idx].transform.GetChild(2).gameObject.activeSelf)
            {
                on = true;
                break;
            }

        GameCore.Instance.CommonSys.tbUi.GetNewFriendCheck(on);
    }

    void ANS_FRIEND_LIST(JSONObject _json, int _code)
    {
        if (_code == 0)
        {
            /////////////// CommonSys에서 저장함./////////////////
            //int del_Friend_Limit = 0;
            //_json.GetField(ref del_Friend_Limit, "DEL_FRIEND_LIMIT");
            //GameCore.Instance.PlayerDataMgr.LocalUserData.DeleteFriendRemainingCount = del_Friend_Limit;
            //GameCore.Instance.PlayerDataMgr.SetFriendSData(_json.GetField("FRIEND_LIST"));
            ///////////////////////////////////////////////////////
            
            var list = GameCore.Instance.PlayerDataMgr.GetFriendList();
            for (int i = 0; i < list.Length; ++i)
            {
                var item = FriendListItemScript.Create(listRoot[(int)Page.Friend].transform);
                item.Init(CBOnLastActiveItem, FriendListItemScript.Type.Friend, list[i]);
                items[(int)Page.Friend].Add(item);
                listRoot[(int)Page.Friend].GetComponent<UIGrid>().enabled = true;
            }

            SetFriendCount(list.Length);

            // SetHighlight
            SetActiveHighlight(0, false);
            foreach (var data in list)
                if (data.FRIENDSHIP_FLAG == 1)
                {
                    SetActiveHighlight(0, true);
                    break;
                }
        }
    }

    void ANS_FRIEND_SEARCH(JSONObject _json, int _code)
    {
        if (_code == 1)
        {
            GameCore.Instance.ShowNotice(CSTR.MSG_HEAD_FindFriend, CSTR.MSG_QuiryErr, 0);
            return;
        }

        ClearList(Page.Search);
        SearchWarnMessage.gameObject.SetActive(false);
        var listJson = _json.GetField("FRIEND_LIST");
        if (listJson.Count == 0){
            SearchWarnMessage.gameObject.SetActive(true);
            if(searchFlag == false) SearchWarnMessage.text = CSTR.MSG_EmptyRewFriend;
            else SearchWarnMessage.text = CSTR.MSG_EmptySeurchFriend;
            return;
        }

        for (int i = 0; i < listJson.Count; ++i)
        {
            int acceptableDay = 0;
            var data = new FriendSData();
            data.SetData(listJson[i]);

            /* int ACTIVE = 0;
            int ACCEPT = 0;
            bool contain = listJson[i].ToDictionary().ContainsKey("ACTIVE");
            if (contain)
            {
               listJson[i].GetField(ref ACTIVE, "ACTIVE");
               listJson[i].GetField(ref ACCEPT, "ACCEPT");
            }*/

            long uid = data.USER_UID;

            // 친구 상태 결정(친구인가? 아무것도 아닌가? 요청한 상태인가? 요청 받은 상태인가?)
            FriendListItemScript.Type state;

            switch (_code)
            {
                case 2:
                    state = FriendListItemScript.Type.RequestedFriend;
                    break;
                case 3:
                    {
                        state = FriendListItemScript.Type.Friend;
                        var list = items[(int)Page.Friend];
                        for (int j = 0; j < list.Count; ++j)
                            if (list[i].data.USER_UID == uid)
                            {
                                data = list[i].data;
                                break;
                            }
                        break;
                    }
                case 4:
                    {
                        state = FriendListItemScript.Type.AcceptableFriend;
                        var list = items[(int)Page.Acceptable];
                        for (int j = 0; j < list.Count; ++j)
                            if (list[i].data.USER_UID == uid)
                            {
                                data = list[i].data;
                                acceptableDay = list[i].acceptableDay;
                                break;
                            }
                        break;
                    }
                default:
                    state = FriendListItemScript.Type.NotFriend;
                    break;
            }

            var item = FriendListItemScript.Create(listRoot[(int)Page.Search].transform);
            item.Init(CBOnLastActiveItem, state, data, acceptableDay);
            AddItem(Page.Search, item);
            ResetScroll();
        }
    }

    void ANS_FRIEND_REQUEST(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                if (lastActiveItem != null)
                    lastActiveItem.SetState(FriendListItemScript.Type.RequestedFriend);
                break;

            case 1: GameCore.Instance.ShowNotice(CSTR.TEXT_ReqFriend, CSTR.MSG_QuiryErr, 0); break;
            case 2: GameCore.Instance.ShowNotice(CSTR.TEXT_ReqFriend, CSTR.MSG_AlreadyReq, 0); break;
            case 3: GameCore.Instance.ShowNotice(CSTR.TEXT_ReqFriend, CSTR.MSG_AlreadyFriend, 0); break;
            case 4: GameCore.Instance.ShowNotice(CSTR.TEXT_ReqFriend, CSTR.MSG_MaxumFriendCount, 0); break;
            default: GameCore.Instance.ShowNotice(CSTR.TEXT_ReqFriend, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }

        lastActiveItem = null;
    }

    void ANS_FRIEND_RECOMMENDLIST(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                ClearList(Page.Recommend);

                var listJson = _json.GetField("FRIEND_LIST");
                if (listJson.Count == 0)
                    return;

                for (int i = 0; i < listJson.Count; ++i)
                {
                    var data = new FriendSData();
                    data.SetData(listJson[i]);

                    var item = FriendListItemScript.Create(listRoot[(int)Page.Recommend].transform);
                    item.Init(CBOnLastActiveItem, FriendListItemScript.Type.NotFriend, data);
                    AddItem(Page.Recommend, item);
                    ResetScroll();
                }
                break;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_RcmFriend, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }
    }

    void ANS_FRIEND_REQUESTEDLIST(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                //ClearList(Page.Request);

                //var listJson = _json.GetField("FRIEND_LIST");
                //if (listJson.Count == 0)
                //    return;

                //for (int i = 0; i < listJson.Count; ++i)
                //{
                //    var data = new FriendSData();
                //    data.SetData(listJson[i]);

                //    var item = FriendListItemScript.Create(listRoot[(int)Page.Request].transform);
                //    item.Init(CBOnLastActiveItem, FriendListItemScript.Type.RequestedFriend, data);
                //    items[(int)Page.Request].Add(item);
                //    AddItem(Page.Request, item);
                //    ResetScroll();
                //}
                break;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_ReqFriend, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }
    }

    void ANS_FRIEND_ACCEPTABLELIST(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                ClearList(Page.Acceptable);

                var listJson = _json.GetField("FRIEND_LIST");
                if (listJson.Count == 0)
                {
                    SetActiveHighlight(3, false);
                    GameCore.Instance.CommonSys.tbUi.GetNewFriendCheck(false);
                    return;
                }

                SetActiveHighlight(3, true);
                for (int i = 0; i < listJson.Count; ++i)
                {
                    var data = new FriendSData();
                    data.SetData(listJson[i]);

                    DateTime reqDate;
                    JsonParse.ToParse(listJson[i], "REQUEST_DATE", out reqDate);
                    var day = 7 - (int)(GameCore.nowTime - reqDate).TotalDays;
                    var item = FriendListItemScript.Create(listRoot[(int)Page.Acceptable].transform);
                    item.Init(CBOnLastActiveItem, FriendListItemScript.Type.AcceptableFriend, data, day);
                    AddItem(Page.Acceptable, item);
                    ResetScroll();
                }
                break;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_ActFriend, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }
    }

    void ANS_FRIEND_ACCEPT_OK(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                if (lastActiveItem != null)
                {
                    // 친구 추가 및 리스트 업데이트
                    GameCore.Instance.PlayerDataMgr.AddFriend(lastActiveItem.data);
                    ClearList(Page.Friend);
                    var friends = GameCore.Instance.PlayerDataMgr.GetFriendList();
                    for (int i = 0; i < friends.Length; ++i)
                    {
                        var item = FriendListItemScript.Create(listRoot[(int)Page.Friend].transform);
                        item.Init(CBOnLastActiveItem, FriendListItemScript.Type.Friend, friends[i]);
                        AddItem(Page.Friend, item);
                    }

                    SetFriendCount(items[(int)Page.Friend].Count);

                    // 요청 목록에서 제거
                    RemoveItem(Page.Acceptable, lastActiveItem.data.USER_UID);
                }
                break;

            case 2: GameCore.Instance.ShowNotice(CSTR.TEXT_ActFriend, CSTR.MSG_AlreadyFriend, 0); break;
            case 3: GameCore.Instance.ShowNotice(CSTR.TEXT_ActFriend, CSTR.MSG_MaxumFriendCount, 0); break;
            case 4: GameCore.Instance.ShowNotice(CSTR.TEXT_ActFriend, CSTR.MSG_MaxumFriendCount_oppo, 0); break;
            default: GameCore.Instance.ShowNotice(CSTR.TEXT_ActFriend, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }

        lastActiveItem = null;
    }

    void ANS_FRIEND_ACCEPT_NO(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                if (lastActiveItem != null)
                {
                    RemoveItem(Page.Acceptable, lastActiveItem.data.USER_UID);
                    if (items[(int)Page.Acceptable].Count == 0)
                        GameCore.Instance.CommonSys.tbUi.GetNewFriendCheck(false);
                }
                break;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_ReqFriend_Refuse, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }

        lastActiveItem = null;
    }

    void ANS_FRIEND_REMOVE(JSONObject _json, int _code)
    {
        var code = -1;
        _json.GetField(ref code, "result");
        switch (code)
        {
            case 0:
                if (lastActiveItem != null)
                {
                    // 친구 추가 및 리스트 업데이트
                    GameCore.Instance.PlayerDataMgr.RemoveFriend(lastActiveItem.data);

                    // 친구 목록에서 제거
                    RemoveItem(Page.Friend, lastActiveItem.data.USER_UID);
                }
                break;

            case 2: GameCore.Instance.ShowNotice(CSTR.TEXT_DelFriend, CSTR.MSG_MaxumDelFriendCount, 0); break;
            default: GameCore.Instance.ShowNotice(CSTR.TEXT_DelFriend, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }

        lastActiveItem = null;
    }

    void ANS_FRIEND_SEND(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                var reward = _json.GetField("REWARD");
                GameCore.Instance.PlayerDataMgr.SetRewardItems(reward);

                var uids = _json.GetField("FRIEND_UID_LIST");
                for (int i = 0; i < uids.list.Count; ++i)
                {
                    long uid = uids.list[i].custom_l;
                    var list = items[(int)Page.Friend];
                    for (int j = 0; j < list.Count; ++j)
                        if (list[j].data.USER_UID.Equals(uid))
                            list[j].SetGive(GameCore.nowTime);
                }

                if (bSendAll)
                {
                    GameCore.Instance.ShowAlert(string.Format(CSTR.SendFriendship, uids.Count));
                    bSendAll = false;
                }
                break;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_SnedFriend, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }
    }

    void ANS_FRIEND_RECEIVE(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                var reward = _json.GetField("REWARD");
                GameCore.Instance.PlayerDataMgr.SetRewardItems(reward);

                var uids = _json.GetField("FRIEND_UID_LIST");
                for (int i = 0; i < uids.Count; ++i)
                {
                    long uid = uids.list[i].custom_l;
                    var list = items[(int)Page.Friend];
                    for (int j = 0; j < list.Count; ++j)
                        if (list[j].data.USER_UID.Equals(uid))
                            list[j].SetTake();

                    SetActiveHighlight(0, false);
                    foreach (var data in list)
                    {
                        if (data.data.FRIENDSHIP_FLAG == 1)
                        {
                            SetActiveHighlight(0, true);
                            break;
                        }
                    }
                }

                if (bReceiveAll)
                {
                    GameCore.Instance.ShowAlert(string.Format(CSTR.ReceiveFriendship, uids.Count));
                    bReceiveAll = false;
                    //SetActiveHighlight(0, false);
                }
                break;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_RecvFriend, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }
    }

    void ANS_FRIEND_TEAMINFO(JSONObject _json, int _code)
    {
        switch (_code)
        {
            case 0:
                var list = _json.GetField("FRIEND_LIST");

                List<PvPOppUnitSData> units = new List<PvPOppUnitSData>(list.Count);
                for (int i = 0; i < list.Count; ++i)
                {
                    var unit = new PvPOppUnitSData();
                    unit.SetData(list[i]);
                    units.Add(unit);
                }

                var teamInfo = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_TeamInfo, transform);
                teamInfo.GetComponent<PvPTeamInfoComponent>().SetData(units, lastActiveItem.data.USER_NAME);
                GameCore.Instance.ShowObject(string.Empty, null, teamInfo, 4, new MsgAlertBtnData[] {
                    new MsgAlertBtnData(CSTR.TEXT_Accept, new EventDelegate(() => GameCore.Instance.CloseMsgWindow()))
                });
                var widget = teamInfo.GetComponent<UIWidget>();
                if (widget != null) widget.alpha = 0f;
                break;

            default: GameCore.Instance.ShowNotice(CSTR.TEXT_TeamInfo, string.Format(CSTR.MSG_WrongCode, _code), 0); break;
        }

        lastActiveItem = null;
    }
}
