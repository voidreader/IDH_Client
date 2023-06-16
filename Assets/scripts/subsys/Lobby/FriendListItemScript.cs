using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendListItemScript : MonoBehaviour
{
    public enum Type
    {
        Friend,             // 친구 상태
        NotFriend,          // 아무것도 아닌 상태
        RequestedFriend,    // 요청한 상태
        AcceptableFriend    // 요청 받은 상태
    }

    public enum Data
    {
        Satisfaction,
        Pollution,
        OpenRoom,
        Power,
        Resent,
    }

    public static FriendListItemScript Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_FriendListItem, _parent);
        var result = go.GetComponent<FriendListItemScript>();
        return result;
    }

    [Header("Head")]
    [SerializeField] UILabel lbLevel;
    [SerializeField] UILabel lbName;

    [Header("Icon")]
    [SerializeField] UISprite spTypicalIcon;
    [SerializeField] UILabel lbComment;
    [SerializeField] GameObject goRemoveBtn;

    [Header("Datas")]
    [SerializeField] UIGrid grData;
    [SerializeField] GameObject[] goDatas;
    [SerializeField] UILabel[] lbDatas;
    //[SerializeField] UILabel lbPowerValue;
    //[SerializeField] UILabel lbRecent;

    [Header("Buttons")]
    [SerializeField] GameObject btSend;
    [SerializeField] GameObject btReceive;

    [SerializeField] UIButton btRequest;
    [SerializeField] GameObject goAcceptBtn;

    [SerializeField] GameObject goTeamBtn;
    [SerializeField] GameObject goRoomBtn;

    [SerializeField] UISprite spSendIcon;
    [SerializeField] UILabel lbSendableTime;
    [SerializeField] UILabel lbAcceptableTime;

    [SerializeField] UITweener[] twSends;
    [SerializeField] UITweener[] twReceives;

    Type type;
    public FriendSData data { get; private set; }
    public int acceptableDay { get; private set; }
    System.Func<FriendListItemScript, bool> cbLastActiveItem;

    System.TimeSpan sendTime = System.TimeSpan.Zero;
    internal void Init(System.Func<FriendListItemScript, bool> _cbLastActiveItem, Type _type, FriendSData _data, int _acceptableDay = 0)
    {
        type = _type;
        data = _data;
        acceptableDay = _acceptableDay;
        cbLastActiveItem = _cbLastActiveItem;

        // Set Data
        goDatas[(int)Data.Satisfaction].SetActive(false);
        goDatas[(int)Data.Pollution].SetActive(false);
        goDatas[(int)Data.OpenRoom].SetActive(false);
        goDatas[(int)Data.Power].SetActive(true);
        goDatas[(int)Data.Resent].SetActive(true);
        goTeamBtn.SetActive(true);
        goRoomBtn.SetActive(false);
        grData.enabled = true;

        lbLevel.text = string.Format(CSTR.Level, _data.USER_LEVEL);
        lbName.text = _data.USER_NAME;
        lbComment.text = _data.COMM;
        if (_data.DELEGATE_ICON > 0)
        {
            var unit = GameCore.Instance.DataMgr.GetUnitData(_data.DELEGATE_ICON);
            GameCore.Instance.SetUISprite(spTypicalIcon, unit.GetBigProfileSpriteKey());
        }
        lbDatas[(int)Data.Power].text = _data.DELEGATE_TEAM_POWER.ToString("N0");
        var h = (int)(GameCore.nowTime - _data.LOGIN_DATE).TotalHours;
        h = Mathf.Abs(h);

        if (h == 0)      lbDatas[(int)Data.Resent].text = CSTR.LastConnetion_Moment;
        else if (h < 24) lbDatas[(int)Data.Resent].text = string.Format(CSTR.LastConnetion_Hour, h);
        else             lbDatas[(int)Data.Resent].text = string.Format(CSTR.LastConnetion_Day, h / 24);
        lbAcceptableTime.text = string.Format(CSTR.Remain_Day, _acceptableDay);

        UpdateSendTime();

        // 타입별 출력 데이터
        SetState(type);
    }

    internal void InitMyRoom(FriendSData _data)
    {
        data = _data;
        cbLastActiveItem = null;

        goRemoveBtn.SetActive(false);
        goAcceptBtn.SetActive(false);
        btRequest.gameObject.SetActive(false);

        btSend.GetComponent<BoxCollider2D>().gameObject.SetActive(false);
        btReceive.GetComponent<BoxCollider2D>().gameObject.SetActive(false);
        btSend.GetComponent<UIButton>().gameObject.SetActive(false);
        btReceive.GetComponent<UIButton>().gameObject.SetActive(false);

        goDatas[(int)Data.Satisfaction].SetActive(true);
        goDatas[(int)Data.Pollution].SetActive(true);
        goDatas[(int)Data.OpenRoom].SetActive(true);
        goDatas[(int)Data.Power].SetActive(false);
        goDatas[(int)Data.Resent].SetActive(true);

        goTeamBtn.SetActive(false);
        goRoomBtn.SetActive(true);
        grData.enabled = true;

        lbLevel.text = string.Format(CSTR.Level, _data.USER_LEVEL);
        lbName.text = _data.USER_NAME;
        lbComment.text = _data.COMM;
        if (_data.DELEGATE_ICON > 0)
        {
            var unit = GameCore.Instance.DataMgr.GetUnitData(_data.DELEGATE_ICON);
            GameCore.Instance.SetUISprite(spTypicalIcon, unit.GetBigProfileSpriteKey());
        }

        lbDatas[(int)Data.Satisfaction].text = "124 [-]/ 400";
        lbDatas[(int)Data.Pollution].text = "14 [-]/ 400";
        lbDatas[(int)Data.Resent].text = "4 [-]/ 400";

        var h = (int)(GameCore.nowTime - _data.LOGIN_DATE).TotalHours;
        if (h < 24) lbDatas[(int)Data.Resent].text = string.Format(CSTR.LastConnetion_Hour, h);
        else        lbDatas[(int)Data.Resent].text = string.Format(CSTR.LastConnetion_Day, h / 24);
    }

    internal void SetState(Type _type)
    {
        type = _type;

        goRemoveBtn.SetActive(false);
        goAcceptBtn.SetActive(false);
        btRequest.gameObject.SetActive(false);

        btSend.GetComponent<BoxCollider2D>().gameObject.SetActive(false);
        btReceive.GetComponent<BoxCollider2D>().gameObject.SetActive(false);
        btSend.GetComponent<UIButton>().gameObject.SetActive(false);
        btReceive.GetComponent<UIButton>().gameObject.SetActive(false);

        switch (_type)
        {
            case Type.Friend:
                goRemoveBtn.SetActive(true);

                btSend.GetComponent<BoxCollider2D>().gameObject.SetActive(true);
                btSend.GetComponent<UIButton>().gameObject.SetActive(true);

                btReceive.GetComponent<BoxCollider2D>().gameObject.SetActive(true);
                btReceive.GetComponent<UIButton>().gameObject.SetActive(true);
                btReceive.GetComponent<BoxCollider2D>().enabled = IsActiveFriendshipReceiveBtn() ? true : false;
                btReceive.GetComponent<UIButton>().enabled = IsActiveFriendshipReceiveBtn() ? true : false;
                btSend.GetComponent<UIButton>().defaultColor = new Color(1, 1, 1, IsActiveFriendshipSendBtn() ? 1f : 0.5f);
                btReceive.GetComponent<UIButton>().defaultColor = new Color(1, 1, 1, IsActiveFriendshipReceiveBtn() ? 1f : 0.5f);
                break;

            case Type.NotFriend:
                btRequest.gameObject.SetActive(true);
                btRequest.defaultColor = Color.white;
                btRequest.GetComponentInChildren<UILabel>().text = CSTR.TEXT_ReqFriend;
                break;

            case Type.RequestedFriend:
                btRequest.gameObject.SetActive(true);
                btRequest.GetComponent<UIButton>().enabled = false;
                btRequest.GetComponent<BoxCollider2D>().enabled = false;
                btRequest.defaultColor = new Color(1, 1, 1, 0.4f);
                btRequest.GetComponentInChildren<UILabel>().text = CSTR.TEXT_Req_Done;
                break;

            case Type.AcceptableFriend:
                goRemoveBtn.SetActive(true);
                goAcceptBtn.SetActive(true);
                break;
        }
    }

    public void SetGive(System.DateTime _time)
    {
        data.FRIENDSHIP_DATE = _time;
        UpdateFriendShipSendTime();
        btSend.GetComponent<UIButton>().defaultColor = new Color(1, 1, 1, 0.5f);
        btSend.GetComponent<BoxCollider2D>().enabled = false;
        btSend.GetComponent<UIButton>().enabled = false;
        // Play Tween
        for (int i = 0; i < twSends.Length; ++i)
        {
            twSends[i].ResetToBeginning();
            twSends[i].PlayForward();
        }
    }

    public void SetTake()
    {
        data.FRIENDSHIP_FLAG = 0;
        btReceive.GetComponent<UIButton>().defaultColor = new Color(1, 1, 1, 0.5f);
        btReceive.GetComponent<BoxCollider2D>().enabled = false;
        btReceive.GetComponent<UIButton>().enabled = false;
        // Play Tween
        for (int i = 0; i < twReceives.Length; ++i)
        {
            twReceives[i].ResetToBeginning();
            twReceives[i].PlayForward();
        }
    }

    public void UpdateFriendShipSendTime()
    {
        //sendTime = data.FRIENDSHIP_DATE.AddDays(1) - GameCore.nowTime;
        var consts = GameCore.Instance.DataMgr.GetFriendConstData();
        sendTime = data.FRIENDSHIP_DATE.AddSeconds(consts.friendshipCooltime) - GameCore.nowTime;
    }

    public bool IsActiveFriendshipSendBtn()
    {
        return sendTime <= System.TimeSpan.Zero;
    }

    public bool IsActiveFriendshipReceiveBtn()
    {
        return data.FRIENDSHIP_FLAG != 0;
    }

    private void Update()
    {
        if (type == Type.Friend)
            UpdateSendTime();
    }

    void UpdateSendTime()
    {
        if (sendTime < System.TimeSpan.Zero)
            return;

        UpdateFriendShipSendTime();
        if (sendTime > System.TimeSpan.Zero)
        {
            lbSendableTime.text = string.Format(CSTR.Time_HHmmss, sendTime.Hours, sendTime.Minutes, sendTime.Seconds);
            spSendIcon.spriteName = "WHITE_ICON_17";
        }
        else
        {
            lbSendableTime.text = CSTR.TEXT_Send;
            spSendIcon.spriteName = "ICON_MONEY_04";
            btSend.GetComponent<UIButton>().defaultColor = Color.white;
            if (type == Type.Friend)
            {
                btSend.GetComponent<BoxCollider2D>().enabled = true;
                btSend.GetComponent<UIButton>().enabled = true;
            }
        }
    }

    public void OnClickRemoveFriend()
    {
        if (type == Type.AcceptableFriend)
        {
            if (cbLastActiveItem(this))
                GameCore.Instance.NetMgr.Req_Friend_Accept_NO(data.USER_UID);
        }
        else
        {
            var count = GameCore.Instance.DataMgr.GetFriendConstData().maxDeleteCount;
            GameCore.Instance.ShowAgree(CSTR.MSG_HEAD_RemoveFriend, string.Format(CSTR.MSG_AskDeleteFriend, count), 0, CSTR.TEXT_Delete, () =>
            {
                GameCore.Instance.CloseMsgWindow();
                if (cbLastActiveItem(this))
                    GameCore.Instance.NetMgr.Req_Friend_Remove(data.USER_UID);
                else
                    GameCore.Instance.ShowNotice(CSTR.TEXT_Req_Fail, CSTR.MSG_PleaseWait, 0);
            });
        }
    }


    public void OnClickSend()
    {
        if (IsActiveFriendshipSendBtn())
            GameCore.Instance.NetMgr.Req_Friend_Send(data.USER_UID);
    }

    public void OnClickReceive()
    {
        if (IsActiveFriendshipReceiveBtn())
            GameCore.Instance.NetMgr.Req_Friend_Receive(data.USER_UID);
    }

    public void OnClickRequest()
    {
        if (type == Type.NotFriend && cbLastActiveItem(this))
            GameCore.Instance.NetMgr.Req_Friend_Request(data.USER_UID);
    }

    public void OnClickAccept()
    {
        if (cbLastActiveItem(this))
            GameCore.Instance.NetMgr.Req_Friend_Accept_OK(data.USER_UID);
    }

    public void OnClickTeam()
    {
        if (cbLastActiveItem(this))
            GameCore.Instance.NetMgr.Req_Friend_TeamInfo(data.USER_UID, 1);
    }

    public void OnClickRoom()
    {
        GameCore.Instance.ShowAlert(CSTR.TEXT_Unembodiment);
    }
}
