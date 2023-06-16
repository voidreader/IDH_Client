using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;


public class ChatUI : MonoBehaviour
{
	//public int MaxChatCount = ;

	[SerializeField] UITable content;
    [SerializeField] UIInput input;
    [SerializeField] UILabel TextCount;
	UIWidget area;
	Queue<ChatItemScript> queue = new Queue<ChatItemScript>();

	Vector3 camOriginPos;
	Vector3 camPrevPos;

     float contentHeight;

	internal void Init()
	{
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "blind").onClick.Add(new EventDelegate(() => gameObject.SetActive(false)));

		content = UnityCommonFunc.GetComponentByName<UITable>(gameObject, "content");
		input = UnityCommonFunc.GetComponentByName<UIInput>(gameObject, "Input");
		area = UnityCommonFunc.GetComponentByName<UIWidget>(gameObject, "bg");
        TextCount = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "TextCount");
        //	view = UnityCommonFunc.GetComponentByName<UIDraggableCamera>(gameObject, "camChatList");
        input.defaultText = CSTR.GUIDE_InputHere;
        input.onSubmit.Clear();
		input.onSubmit.Add(new EventDelegate(SendChat));
        TextCount.text = input.value.Length.ToString() + "/" + input.characterLimit.ToString();
        //	camOriginPos = camPrevPos = view.transform.position;
        // GameCore.Instance.NetMgr.Req_Chat_Filter();

        content.Reposition();
    }

    private void Update()
    {
        TextCount.text = input.value.Length.ToString() + "/" + input.characterLimit.ToString();
    }


    private void SendChat()
	{
		var text = input.value;
		if (text == "")
			return;

        // 특수문자 파싱 %(16진수)
        GameCore.Instance.NetMgr.Req_Chat(JsonTextParse.ToJsonText(text));
		input.value = "";
	}

	internal void AddChat(string name, string text, int key)
	{
        ChatItemScript chatItemScript = new ChatItemScript(key, name, text, content.transform);

        queue.Enqueue(chatItemScript);
		if(queue.Count > ChatMgr.MaxChatCount)
		{
			var item = queue.Dequeue();
			item.Destroy();
		}

		content.Reposition();

		//contentHeight = content.cellHeight * content.transform.childCount + (chatItemScript.LineCount-1) * fontsize;
	}



    internal void AddChat(int charId, string userName, string itemName, string Gread)
    {
        queue.Enqueue(new ChatItemScript(charId, userName, itemName, Gread, content.transform));
        if(queue.Count > ChatMgr.MaxChatCount)
        {
            var item = queue.Dequeue();
            item.Destroy();
        }
        content.Reposition();

        //contentHeight = content.cellHeight * content.transform.childCount;
        //Debug.Log(area.localSize + "  " + contentHeight);
    }



    internal void ClearChat()
    {
        foreach(var item in queue)
            item.Destroy();
        queue.Clear();
    }
}
