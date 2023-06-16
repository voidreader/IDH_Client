using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatItemScript
{
	int charID;
	string text;
	string name;
	int _lineCount = 1;

	GameObject obj;

	UISprite icon;
	UnitDataMap data;
	UILabel nameLabel;

	// UITable을 이용한 정렬을 위한 변수
	static int createCount = 100000000;

	public int LineCount { get => _lineCount; }

	// 채팅 출력
	internal ChatItemScript(int _charId, string _name, string _text, Transform _rootTf)
	{
		charID = _charId;
		name = _name;
		text = _text;
		//rootTf = _rootTf;
		bool mine = name == GameCore.Instance.PlayerDataMgr.Name;

		if (mine)
		{
			obj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_ChatItem_Mine, _rootTf);
		}
		else
		{
			obj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_ChatItem_Other, _rootTf);
		}

		icon = UnityCommonFunc.GetComponentByName<UISprite>(obj, "iconRoot");
		data = GameCore.Instance.DataMgr.GetUnitData(_charId);
		if(data != null)
			GameCore.Instance.SetUISprite(icon, data.GetSmallProfileSpriteKey());

		//UnityCommonFunc.GetComponentByName<UILabel>(go, "text").text = _text;

		TextUpdate(_text);
		obj.name = (createCount++).ToString();

		if(nameLabel == null)
		{
			nameLabel = UnityCommonFunc.GetComponentByName<UILabel>(obj, "textName");
		}
		

		if (!mine) nameLabel.text = _name;
		else nameLabel.text = ""; 
	}

	// 아이템 출력
    internal ChatItemScript(int _charId,string username, string itemName, string itemGread, Transform rootTransform)
    {
        charID = _charId;

        obj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_ChatItem_Notification, rootTransform);
		obj.name = (createCount++).ToString();

		UnityCommonFunc.GetComponentByName<UILabel>(obj, "text").text = string.Format(CSTR.ChatUniqueNotification, itemGread, itemName);
        var icon = UnityCommonFunc.GetComponentByName<UISprite>(obj, "iconRoot");
        var data = GameCore.Instance.DataMgr.GetUnitData(_charId);
        if (data != null)
            GameCore.Instance.SetUISprite(icon, data.GetSmallProfileSpriteKey());
        UnityCommonFunc.GetComponentByName<UILabel>(obj, "textName").text = username;
    }

	internal void Destroy()
	{
		obj.transform.parent = null;
		GameObject.Destroy(obj);
	}

	internal void TextUpdate(string _text)
	{
		string splitText = _text;

		UILabel chat = UnityCommonFunc.GetComponentByName<UILabel>(obj, "text");

		if(splitText.Length != 0)
		{
			chat.text = "";
		}

		
		int wordCount = 0;                          // 한줄에 들어가는 글자 최대수 ( 30 )을 카운트하기 위한 변수
		const int lineWordCountMax = 30;			// 한줄에 들어가는 글자 최대수
		for (int i = 0; i < splitText.Length; i++)
		{
			if (splitText[i] == '.')
			{
				int saveIndex = i;
				for (/* upper for ( i ) */; i < splitText.Length && splitText[i] == '.'; i++)
				{
					chat.text += splitText[i];
				}

				if(saveIndex == i - 1 /* '.' 이 단 1개일 경우 */ && i != splitText.Length)
				{
					chat.text += "\n";
					wordCount = 0;
					_lineCount++;
				}
			}
			else if (wordCount == lineWordCountMax)
			{
				chat.text += "\n";
				wordCount = 0;
				_lineCount++;
			}
			
			// NOTE : 상위 '.' 검사와 관련하여 
			// ....... 등으로 끝났을때 발생하는 IndexOutOfRange Exception 방지용.
			if(i < splitText.Length)
			{
				chat.text += splitText[i];
				wordCount++;
			}
		}


		// 이름 위치 조정용
		if(nameLabel == null)
		{
			nameLabel = UnityCommonFunc.GetComponentByName<UILabel>(obj, "textName");
		}
		nameLabel.transform.localPosition += new Vector3(0, 7.4F * (LineCount - 1), 0);
	}
}
