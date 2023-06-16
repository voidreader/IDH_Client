using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class SubTabItemList : ItemList
{
	internal class SetInteriorData
	{
		internal int groupIdx;
		internal string name;
		internal string disc;

		internal int setCount;
		internal int haveCount;

		internal SetInteriorData(int _idx, string _name, string _disc, int _maxCnt, int _haveCnt )
		{
			groupIdx = _idx;
			name = _name;
			disc = _disc;
			setCount = _maxCnt;
			haveCount = _haveCnt;
		}
	}


	UIScrollView tabScrollView;
	UIGrid tabGrid;

	UIScrollView itemScrollView;
	UIGrid itemGrid;

	UILabel lbCount;


	Dictionary<int, List<CardBase>> cardsOfSet;	// 세트별 아이템 테이블
	Dictionary<int, SetInteriorData> setData;   // 세트의 데이터
	Dictionary<int, UISprite> tabSprites;       // 세트 탭버튼 스프라이트

	Dictionary<Transform, SetInteriorData> setDataByTransform; // 세트 데이터를 트랜스폼으로 찾기 위함
	int nowTabNum;


	public SubTabItemList(GameObject _go, InvenBase.TypeFlag _typeFlag, CardType _type, Action<long> _cbClick, Action<long> _cbPress, Comparison<Transform> _cbSort = null, Func<CardBase, int, bool> _cbFilter = null)
		: base(_go, _typeFlag, _type, _cbClick, _cbPress, _cbSort, _cbFilter)
	{
		tabScrollView = UnityCommonFunc.GetComponentByName<UIScrollView>(gameObject, "svTab");
		tabGrid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "gridTab");
		itemScrollView = UnityCommonFunc.GetComponentByName<UIScrollView>(gameObject, "svItem");
		itemGrid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "gridItem");
		lbCount = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Count");

		cardsOfSet = new Dictionary<int, List<CardBase>>();
		setData = new Dictionary<int, SetInteriorData>();
		tabSprites = new Dictionary<int, UISprite>();
		setDataByTransform = new Dictionary<Transform, SetInteriorData>();

		tabGrid.onCustomSort = CBTabSort;
	}

	internal override void AddItem(Transform _item, long _key, CardBase _value)
	{
		// base.AddItem(_item, _key, _value);
		_value.SetCallback(OnClickItem, OnPressItem);
		table.Add(_key, _value);
		tfTable.Add(_item, _value);
		_item.parent = itemGrid.transform;
		_item.localScale = Vector3.one;

		if (itemGrid != null)
			itemGrid.enabled = true;
	}

	internal void CreateTab(int _groupIdx, string _name, string _disc, int _haveCnt, CardBase[] _items)
	{
		// Create Tab
		var tab = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/setTabButton", tabGrid.transform);
		tab.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            Change(_groupIdx); }));

		tabGrid.enabled = true;
		UnityCommonFunc.GetComponentByName<UILabel>(tab, "name").text = _name;
		UnityCommonFunc.GetComponentByName<UISprite>(tab, "gauge").fillAmount = (float)_haveCnt / _items.Length;
		UnityCommonFunc.GetComponentByName<UILabel>(tab, "count").text = "[FFC000]" + _haveCnt + " [-]/ " + _items.Length;
		tabSprites.Add(_groupIdx, UnityCommonFunc.GetComponentByName<UISprite>(tab, "bg"));

		// Set Data
		var data = new SetInteriorData(_groupIdx, _name, _disc, _items.Length, _haveCnt);
		setData.Add(_groupIdx, data);
		setDataByTransform.Add(tab.transform, data);
		// Add Item
		cardsOfSet.Add(_groupIdx, new List<CardBase>());
		for ( int i = 0; i < _items.Length; ++i)
		{
			cardsOfSet[_groupIdx].Add(_items[i]);
			AddItem(_items[i].transform, _items[i].ID, _items[i]);
			_items[i].gameObject.SetActive(false);
		}
	}

	internal void ChangeFirst()
	{
		if (itemGrid.transform.childCount == 0)
			return;

		var idx = setDataByTransform[tabGrid.GetChild(0)].groupIdx;
		Change(idx);
	}

	internal void Change(int _num)
	{
		if (nowTabNum == _num)
			return;

		Close();
		Open(_num);
	}

	internal void Open(int _num)
	{
		if (cardsOfSet.ContainsKey(_num))
		{
			nowTabNum = _num;

			var list = cardsOfSet[nowTabNum];
			for(int i = 0; i < list.Count; ++i)
				list[i].gameObject.SetActive(true);

			var sp = tabSprites[nowTabNum];
			sp.color = new Color32(0x7E, 0x00, 0xFF, 0x60);

			var data = setData[nowTabNum];
			lbCount.text = "[FFC000](" + data.haveCount + "[-]/" + data.setCount + ")";

			itemScrollView.transform.localPosition = Vector3.zero;

			itemGrid.enabled = true;
		}
	}

	internal void Close()
	{
		if (cardsOfSet.ContainsKey(nowTabNum))
		{
			var list = cardsOfSet[nowTabNum];
			for (int i = 0; i < list.Count; ++i)
				list[i].gameObject.SetActive(false);

			var sp = tabSprites[nowTabNum];
			sp.color = new Color32(0x9F,0x9F,0x9F,0x60);

			OffSelectAll();
		}
	}


	int CBTabSort(Transform _1, Transform _2)
	{
		var data1 = setDataByTransform[_1];
		var data2 = setDataByTransform[_2];

		var cnt1 = (float)data1.haveCount / data1.setCount;
		var cnt2 = (float)data2.haveCount / data2.setCount;

		if (cnt1 < cnt2)
			return 1;
		else if (cnt1 > cnt2)
			return -1;
		else
			return string.Compare(data1.name, data2.name);
	}


}
