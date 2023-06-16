using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChatData
{
    public int CHA_ID;
    public string USER_NAME;
    public string MESSAGE;
    public int GRADE;

    public ChatData(int _chaID, string _userName, string _msg, int _grade = -1)
    {
        CHA_ID = _chaID;
        USER_NAME = _userName;
        MESSAGE = _msg;
        GRADE = _grade;
    }
}

public class ChatMgr : MonoBehaviour, IEventHandler
{
    Queue<ChatData> chatList = new Queue<ChatData>();

    WordFilterTree filterTree;

    // 최대 채팅 개수
    public static readonly int MaxChatCount = 40;

    // 채팅이 들어왔을때 콜백 
    public event Action<ChatData> onNewChat;

    /// <summary>
    /// 채팅 필터 문자열이 초기화 되었는지 여부
    /// </summary>
    public bool InitedFilterTexts {  get { return filterTree != null; } }

    public void Init()
    {
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_CHAT,
                                                         GameEventType.ANS_CHAT_NOTIFICATION,
                                                         GameEventType.ANS_CHAT_FILTER);
    }

    public IEnumerator<ChatData> GetChatListEnumerator()
    {
        return chatList.GetEnumerator();
    }


    public int GetChatCount()
    {
        return chatList.Count;
    }


    internal string Filtering(string _text)
    {
        if (filterTree == null)
            return _text;

        return filterTree.Filtering(_text);
    }


    public bool HandleMessage(GameEvent _evt)
    {
        switch (_evt.EvtType)
        {
            case GameEventType.ANS_CHAT_FILTER:         return ANS_CHAT_FILTER(_evt.Para.GetPara<PacketPara>().data.data);
            case GameEventType.ANS_CHAT:                return ANS_CHAT(_evt.Para.GetPara<PacketPara>().data.data);
            case GameEventType.ANS_CHAT_NOTIFICATION:   return ANS_CHAT_NOTIFICATION(_evt.Para.GetPara<PacketPara>().data.data);
        }

        return false;
    }

    internal bool ANS_CHAT_FILTER(JSONObject _json)
    {
        var FilterList = _json.GetField("FILTER");

        if (filterTree == null)
            filterTree = new WordFilterTree();

        filterTree.AddFilterTexts(FilterList.str.Split(','));
        chatList.Clear();
        return true;
    }


    internal bool ANS_CHAT(JSONObject _json)
    {
        string name = null;
        string text = null;
        int key = -1;
        _json.GetField(ref name, "USER_NAME");
        _json.GetField(ref text, "MESSAGE");
        _json.GetField(ref key, "CHA_ID");

        text = Filtering(text);// 채팅은 자주 리드로우 되므로 필터링 된 문자열을 저장한다.
        var data = new ChatData(key, name, text);

        // 리스트에 넣기
        AddChat(data);

        return true;
    }

    public void AddChat(ChatData _data)
    {
        chatList.Enqueue(_data);
        if (chatList.Count >= MaxChatCount)
            chatList.Dequeue();

        // 구독한 콜백 호출
        if (onNewChat != null)
            onNewChat.Invoke(_data);
    }

    internal bool ANS_CHAT_NOTIFICATION(JSONObject _json)
    {
        string userName = null;        //유저 이름
        string itemName = null;        //아이탬/영웅 이름

        int itemGreade = -1;     //등급.
        int charId = -1;

        var msgList = _json.GetField("MSG");
        if (msgList.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < msgList.Count; ++i)
            {
                msgList[i].GetField(ref charId, "UDI");
                msgList[i].GetField(ref userName, "UN");
                msgList[i].GetField(ref itemName, "NAME");
                msgList[i].GetField(ref itemGreade, "GRADE");

                var data = new ChatData(charId, userName, itemName, itemGreade);

                AddChat(data);
            }
        }
        return true;
    }



    public void OnApplicationPause(bool _pause)
    {
        if (!_pause)
        {
            //chatList.Clear();
            //if (GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.Lobby)
            //{
            //    var sys = GameCore.Instance.SubsysMgr.GetNowSubSys() as LobbySys;
            //    sys.ClearChat();
            //}
        }
    }
}
