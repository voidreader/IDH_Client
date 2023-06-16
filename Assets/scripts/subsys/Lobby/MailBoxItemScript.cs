using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailBoxItemScript : MonoBehaviour
{
    public static long LastClickedUID = -1;

    public static MailBoxItemScript Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/MailBoxItem", _parent);
        return go.GetComponent<MailBoxItemScript>();
    }

    [SerializeField] Transform _cardRoot;

    [SerializeField] UILabel _name;
    [SerializeField] UILabel _info;
    [SerializeField] UILabel _desc;

    [SerializeField] UISprite _timerIcon;
    [SerializeField] UILabel _timer;

    [SerializeField] GameObject _takeButton;
    [SerializeField] GameObject _useButton;

    public MailSData mailSData;
    CardSData sdata;
    CardBase card;
    bool CloseTime = false;
    bool noUpdate = false;

    internal void Init(MailSData _mailSData)
    {
        mailSData = _mailSData;
        if (mailSData.type == CardType.Character)
            sdata = new HeroSData(_mailSData.ITEM_ID) { enchant = _mailSData.ITEM_VALUE };
        else if (mailSData.type == CardType.Equipment)
            sdata = new ItemSData(_mailSData.ITEM_ID, 1, _mailSData.type) { enchant = _mailSData.ITEM_ENCHANT };// { enchant = _mailSData.ITEM_VALUE };
        else
            sdata = new ItemSData(_mailSData.ITEM_ID, _mailSData.ITEM_VALUE, _mailSData.type);

        CloseTime = false;
        noUpdate = false;
        _timerIcon.color = Color.white;
        _timer.color = Color.white;
        _info.text = GameCore.Instance.DataMgr.GetMailStringData(_mailSData.MAIL_TYPE);
        _desc.text = _mailSData.MAIL_DESC;
        if (card != null)
        {
            Destroy(card.gameObject);
            card = null;
        }

        switch (_mailSData.type)
        {
            case CardType.Interior:     InitInterior(); break;
            case CardType.Character:    InitCharter();  break;
            default:                    InitItem();     break;
        }

        _takeButton.SetActive(_mailSData.type != CardType.Immediate);
        _useButton.SetActive(_mailSData.type == CardType.Immediate);
    }

    private void Update()
    {
        if (noUpdate)
            return;

        var time = mailSData.DELETE_DATE - GameCore.nowTime;
        if(time.TotalHours < 24)
        {
            if(!CloseTime)
            {
                CloseTime = true;
                _timer.color = Color.red;
                _timerIcon.color = Color.red;
            }

            _timer.text = string.Format("{0:00}:{1:00}:{2:00} 남음", time.Hours, time.Minutes, time.Seconds);
        }
        else
        {
            _timer.text = string.Format("{0}일 남음", (int)time.TotalDays);
            noUpdate = true;
        }
    }

    void InitCharter()
    {
        UnitDataMap data = GameCore.Instance.DataMgr.GetUnitData(sdata.key);
        card = CardBase.CreateCard(sdata, data, true, _cardRoot);
        card.transform.localScale = new Vector3(0.75f, 0.75f);
        card.transform.localPosition = new Vector3(10, 0, 0);

        _name.text = data.name;

        //lbInfo.text = CardDataMap.GetStrRank(data.rank) + "등급 " + CardDataMap.GetStrType(data.charType) + " 영웅";
        //lbDesc.text = GameCore.Instance.DataMgr.GetCharacterStringData(data.discId);
    }

    void InitItem()
    {
        ItemDataMap data = GameCore.Instance.DataMgr.GetItemData(sdata.key);
        card = CardBase.CreateCard(sdata, data, true, _cardRoot);

        _name.text = data.name;
        //lbInfo.text = "";

        switch (data.type)
        {
            case CardType.Equipment:
                //lbInfo.text = "랜덤 능력치";

                //lbDesc.text = ItemSData.GetAllDefOptionString(data.id);
                break;

            case CardType.GiftBox:
                //lbDesc.text = GameCore.Instance.DataMgr.GetItemStringData(data.discID);
                break;

            case CardType.resource:
            default:
                //lbDesc.text = GameCore.Instance.DataMgr.GetItemStringData(data.discID);
                break;
        }
    }

    void InitInterior()
    {
        ItemDataMap data = GameCore.Instance.DataMgr.GetItemData(sdata.key);
        card = CardBase.CreateCard(sdata, data, true, _cardRoot);

        _name.text = data.name;
        //lbInfo.text = "만족도 " + data.optionValue[0];
        switch (data.subType)
        {
            case ItemSubType.Furniture: //lbDesc.text = "가구"; break;
            case ItemSubType.Prop:      //lbDesc.text = "소품"; break;
            case ItemSubType.Wall:      //lbDesc.text = "벽지"; break;
            case ItemSubType.Floor:     //lbDesc.text = "바닥"; break;
                break;
        }
        // Todo : 세트명가져오기
        //lbDesc.text += "\n" + "";
    }

    internal long GetUID()
    {
        return mailSData.MAIL_UID;
    }

    internal CardType GetItemType()
    {
        return mailSData.type;
    }

    public void OnClickTake()
    {
        GameCore.Instance.NetMgr.Req_Mail_Get(mailSData.MAIL_UID);
        LastClickedUID = mailSData.MAIL_UID;
    }

    public void OnClickUse()
    {
        GameCore.Instance.NetMgr.Req_Mail_Get(mailSData.MAIL_UID);
        LastClickedUID = mailSData.MAIL_UID;
    }
}
