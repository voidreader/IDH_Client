using System;
using System.Collections.Generic;
using UnityEngine;

internal class PlayerInfoScript : MonoBehaviour
{
	UILabel lbLevel;
	UILabel lbName;
	UILabel lbExp;
	UISlider sdExp;

	UISprite typicalIcon;
	UIButton btChangeTypical;

	UILabel lbIntro;
	UIInput ipIntro;
	UIButton btEditIntro;

	Action<long, string> cbChange;
	long uid = -1;
	string comment = "";

	GameObject listUI;
	ItemList list;

	private void Awake()
	{
		lbLevel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "level");
		lbName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "name");
		lbExp = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "expLabel");
		sdExp = UnityCommonFunc.GetComponentByName<UISlider>(gameObject, "Slider");

		typicalIcon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "cardRoot");
		btChangeTypical = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnChangeTypical");
		btChangeTypical.onClick.Add(new EventDelegate(OnClickChangeTypical));

		lbIntro = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "introduction");
		ipIntro = UnityCommonFunc.GetComponentByName<UIInput>(gameObject, "introduction_input");
		btEditIntro = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnEdit");
		btEditIntro.onClick.Add(new EventDelegate(OnClickIntroEdit));
		ipIntro.onSubmit.Add(new EventDelegate(OnSubmitChangeIntro));

		// 추가 사항
		lbIntro.multiLine = false;
		ipIntro.onReturnKey = UIInput.OnReturnKey.Default;
	}

	internal void Init(Action<long, string> _cbChange)
	{
		cbChange = _cbChange;
		ResetData();

		lbLevel.text = "LV." + GameCore.Instance.PlayerDataMgr.Level;
		lbName.text = GameCore.Instance.PlayerDataMgr.Name;
		var maxExp = GameCore.Instance.PlayerDataMgr.MaxExp;
		var exp = GameCore.Instance.PlayerDataMgr.Exp;
        if (maxExp == 0)
        {
            lbExp.text = string.Format("[FFEA00]{0}[-] / {1}", '-', '-');
            sdExp.value = 1f;
        }
        else
        {
            lbExp.text = string.Format("[FFEA00]{0}[-] / {1}", exp, maxExp);
            sdExp.value = (float)exp / maxExp;
        }
		

		lbIntro.text = (comment!="") ? comment : "자기소개를 써주세요";
	}

	internal void ResetData()
	{
		ChangeTypical(GameCore.Instance.PlayerDataMgr.MainCharacterUID);
		ChangeComment(GameCore.Instance.PlayerDataMgr.Comment);
	}

	internal void ChangeTypical(long _uid)
	{
		//if (!_force && uid == _uid)
		//	return;

		uid = _uid;
		var data = GameCore.Instance.PlayerDataMgr.GetUnitData(uid);
		if (data != null)
		{
			GameCore.Instance.SetUISprite(typicalIcon, data.GetBigProfileSpriteKey());
		}
		else
		{
			typicalIcon.atlas = null;
		}
	}

	internal void ChangeComment(string _str)
	{
		//if (!_force && comment == _str)
		//	return;

		comment = _str;
		lbIntro.text = (comment != "") ? comment : "자기소개를 써주세요";
	}

	private void OnClickIntroEdit()
	{
		ipIntro.enabled = true;
		ipIntro.isSelected = true;
	}

	private void OnSubmitChangeIntro()
	{
		comment = ipIntro.value;
		lbIntro.text = (comment != "") ? comment : "자기소개를 써주세요";
		ipIntro.isSelected = false;
		ipIntro.enabled = false;
		cbChange(uid, comment);
	}




	private void OnClickChangeTypical()
	{
		if (listUI == null)
		{
			listUI = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/PanelLobbyInvenUI", GameCore.Instance.ui_root);
			UnityCommonFunc.GetComponentByName<UIButton>(listUI, "btSubmit").onClick.Add(new EventDelegate(CBSelect));
			UnityCommonFunc.GetComponentByName<UIButton>(listUI, "btClose").onClick.Add(new EventDelegate(CBClose));
			var grid = UnityCommonFunc.GetComponentByName<UIGrid>(listUI, "ListBody");
			list = new ItemList(grid.gameObject, InvenBase.TypeFlag.Character, CardType.Character, CBClickItem, null);
			var ids = GameCore.Instance.PlayerDataMgr.GetUnitIds();
			for (int i = 0; i < ids.Length; ++i)
			{
				var sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(ids[i]);
                var data = GameCore.Instance.PlayerDataMgr.GetUnitData(ids[i]);
                if (data.IsExpCard())
                    continue;

                var card = CardBase.CreateBigCard(sdata, grid.transform, null, null);
                //if (sdata.GetEquipCount() != 0) card.SetInfo(CardBase.UnitInfo.Power | CardBase.UnitInfo.Equip);
                //else                            card.SetInfo(CardBase.UnitInfo.Power);

                list.AddItem(card.transform, ids[i], card);
			}
            grid.onCustomSort = InvenBase.HeroSortByRankDescending;
        }
		else
			listUI.SetActive(true);

		GameCore.Instance.CommonSys.HideMsgComfirm();
	}
	private void CBClickItem(long _id)
	{
		list.SetSelect(_id);
	}

	private void CBSelect()
	{
		var selects = list.GetSelects();
		if (selects.Length == 0)
			return;

		uid = selects[0];
		cbChange(uid, comment);

		CBClose();
	}

	private void CBClose()
	{
		listUI.SetActive(false);
		GameCore.Instance.CommonSys.ReShowMsgComfirm();
	}

	internal void Destroy()
	{
		if(list != null)
			GameObject.Destroy(list.gameObject);
		GameObject.Destroy(gameObject);
	}

    internal void GetComment()
    {
        comment = ipIntro.value;
        cbChange(uid, comment);
    }
}
