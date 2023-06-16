using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailBoxComponent : MonoBehaviour
{
    public struct Data // 임시 메일데이터
    {
        internal ItemSData data;
        public System.DateTime endTime;
    }

    public static MailBoxComponent Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/MailBoxRoot", _parent);
        var result = go.GetComponent<MailBoxComponent>();
        return result;
    }


    [SerializeField] UIGrid grid;
    List<MailBoxItemScript> items = new List<MailBoxItemScript>();

    public int ItemCount { get { return items.Count; } }

    private void Start()
    {
        var datas = new List<MailSData>();
        //datas.Add(new MailSData() { MAIL_TYPE = 1, ITEM_ID = 3000001, ITEM_VALUE = 200, MAIL_DESC = "DUMMY", DELETE_DATE = GameCore.nowTime.AddDays(1) });
        //datas.Add(new MailSData() { MAIL_TYPE = 1, ITEM_ID = 3000001, ITEM_VALUE = 200, MAIL_DESC = "DUMMY", DELETE_DATE = GameCore.nowTime.AddDays(1) });
        //atas.Add(new MailSData() { MAIL_TYPE = 1, ITEM_ID = 3000001, ITEM_VALUE = 200, MAIL_DESC = "DUMMY", DELETE_DATE = GameCore.nowTime.AddDays(1) });

        Init(datas);
    }

    internal void Init(List<MailSData> _datas)
    {
        grid.transform.parent.localPosition = Vector3.zero;

        for (int i = 0; i < _datas.Count; ++i)
        {
            var item = MailBoxItemScript.Create(grid.transform);
            item.Init(_datas[i]);
            items.Add(item);
        }

        grid.enabled = true;
    }

    internal void RemoveMail(long _mailUID)
    {
        if (_mailUID == -2)
        {
            for (int i = items.Count-1; i >= 0 ; --i)
            {
                if (items[i].GetItemType() != CardType.Immediate)
                {
                    items[i].transform.parent = GameCore.Instance.Ui_root;
                    Destroy(items[i].gameObject);
                    items.RemoveAt(i);
                }
            }

            grid.Reposition();
        }
        else
        {
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i].GetUID() == _mailUID)
                {
                    Destroy(items[i].gameObject);
                    items.RemoveAt(i);
                    grid.enabled = true;
                    return;
                }
            }
        }
    }

    public MailSData GetSDataByUID(long _uid)
    {
        for (int i = 0; i < items.Count; ++i)
        {
            if (items[i].GetUID() == _uid)
                return items[i].mailSData;
        }

        return null;
    }

    public void CBGetAll()
    {
        MailBoxItemScript.LastClickedUID = -2;
        //GameCore.Instance.CommonSys.CreateEmptyMail(gameObject.transform);
        GameCore.Instance.NetMgr.Req_Mail_Get(0);
    }

    internal void ResetScrollViewOffset()
    {
        var panel = grid.transform.parent.GetComponent<UIPanel>();
        panel.transform.localPosition = Vector3.zero;
        panel.clipOffset = new Vector2(0f, -240f);
    }
}
