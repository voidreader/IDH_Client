using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSkillListItem : MonoBehaviour
{
	Transform tfCardRoot;
	UILabel lbName;
	UILabel lbDisc1;
	UILabel lbDisc2;

	UIGrid grCardRoot;
	GameObject[] cardStates;

	UIButton button;
    public Transform ButtonTransform { get { return button.transform; } }
	internal int NeedHeroCount { get; private set; } // 스킬이 필요로 하는 영웅의 수
	internal int NotExistHeroCount { get; private set; }// 존재하지 않는(추가로 필요한) 영웅의 수

	internal static TeamSkillListItem Create(Transform _parent)
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("EditTeam/TS_Item", _parent);
		var result = go.GetComponent<TeamSkillListItem>();
		result.LinkInit();
		return result;
	}

	private void LinkInit()
	{
		tfCardRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "TS_Icon_root");
		lbName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "TS_Name");
		lbDisc1 = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "TS_Disc1");
		lbDisc2 = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "TS_Disc2");

		grCardRoot = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "unit_icon_root");
		cardStates = new GameObject[5];
		for (int i = 0; i < cardStates.Length; ++i)
			cardStates[i] = UnityCommonFunc.GetGameObjectByName(gameObject, "state" + (i + 1));

		button = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button");
	}

	internal void Init(TeamSkillDataMap _data, int[] _charIDsOnSlot, bool _equiped, EventDelegate.Callback _onClick)
	{
		CardBase.CreateBigCardByKey(_data.id, tfCardRoot);
		lbName.text = _data.name;
		lbDisc1.text = _data.info;
        lbDisc2.text = _data.disc;

		NeedHeroCount = _data.needChar.Length;
		List<int> list = new List<int>();
		int idx = 0;

		// equiped
		for(int i = 0; i < _data.needChar.Length; ++i)
		{
			bool exist = false;
			for(int j = 0; j < _charIDsOnSlot.Length; ++j)
			{
				if( _data.needChar[i] == _charIDsOnSlot[j])
				{
					var data = GameCore.Instance.DataMgr.GetUnitDataByCharID(_data.needChar[i]);
					CardBase.CreateSmallCard(data, grCardRoot.transform, null, (_key)=>GameCore.Instance.ShowCardInfoNotHave((int)_key));
					cardStates[idx++].SetActive(false);
					exist = true;
					break;
				}
			}
			if(!exist)
				list.Add(_data.needChar[i]);
		}

		for(int i = 0; i < list.Count; ++i)
		{
			var data = GameCore.Instance.DataMgr.GetUnitDataByCharID(list[i]);
			var card = CardBase.CreateSmallCard(data, grCardRoot.transform, null, (_key) => GameCore.Instance.ShowCardInfoNotHave((int)_key));
			card.SetEnable(false);
			var label = cardStates[idx++];
			label.SetActive(true);
			var lb = label.GetComponentInChildren<UILabel>();

			// equipable
			if (GameCore.Instance.PlayerDataMgr.HasUnitSDataByCharID(list[i]))
			{
				lb.color = new Color32(0xFF, 0xEA, 0x00, 0xFF);
				lb.text = "미배치";
			}
			// not have
			else
			{
				lb.color = Color.white;
				lb.text = "미보유";
			}
		}

		// empty Remove
		for (; idx < cardStates.Length; ++idx)
			cardStates[idx].SetActive(false);

		// Set button
		if(_equiped)					button.GetComponentInChildren<UILabel>().text = "해제";
		else									button.GetComponentInChildren<UILabel>().text = "장착 가능";
		if (list.Count != 0)	button.GetComponent<UISprite>().alpha = 0.2f;
		else									button.GetComponent<UISprite>().alpha = 1f;
		button.enabled = list.Count == 0;
		NotExistHeroCount = list.Count;


		button.onClick.Clear();
		if(list.Count == 0)
			button.onClick.Add(new EventDelegate(_onClick));
	}
}
