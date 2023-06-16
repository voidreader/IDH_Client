using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

internal class ItemList
{
	public GameObject gameObject { get; protected set; }
	public CardType itemType { get; protected set; }
	public InvenBase.TypeFlag Type { get; protected set; }


	UIGrid grid;
    internal Dictionary<long, CardBase> table;
	internal Dictionary<Transform, CardBase> tfTable;

	Func<CardBase, int, bool> cbFilter;

	bool bMultiSelect;
	List<long> selectList;
	List<long> highlightList;

	Action<long> cbClick;       // 아이템 클릭시 호출되는 콜백 함수
	Action<long> cbPress;       // 아이템 클릭홀드시 호출되는 콜백 함수

    public SaveAction saveAction = new SaveAction();

    Coroutine coResizeScrolling = null;

	internal ItemList(GameObject _go, InvenBase.TypeFlag _typeFlag, CardType _type, Action<long> _cbClick, Action<long> _cbPress, Comparison<Transform> _cbSort = null, Func<CardBase, int, bool> _cbFilter = null)
	{
		bMultiSelect = false;
		selectList = new List<long>();
		highlightList = new List<long>();
		Type = _typeFlag;
		itemType = _type;
		gameObject = _go;
		cbClick = _cbClick;
		cbPress = _cbPress;

		table = new Dictionary<long, CardBase>();
		tfTable = new Dictionary<Transform, CardBase>();
		grid = gameObject.GetComponent<UIGrid>();

		SetSortCallBack(_cbSort);
		SetFilterCallBack(_cbFilter);
	}

    internal List<CardBase> GetItemCardList()
    {
        List<CardBase> reValList = new List<CardBase>();

        foreach(var item in table)
        {
            reValList.Add(item.Value);
        }

        return reValList;
    }

	internal bool ContainsKey(long _id)
	{	return table.ContainsKey(_id); }

	internal bool ContainsKey(Transform _tf)
	{ return tfTable.ContainsKey(_tf);	}

	internal int GetCount()
	{	return table.Count;	}

	internal int GetActiveCount()
	{
		int count = 0;
		var it = table.GetEnumerator();
		while(it.MoveNext())
		{
			if (it.Current.Value.gameObject.activeSelf)
				++count;
		}

		return count;
	}

	internal void SetMutiSelectable(bool _muiti)
	{
		bMultiSelect = _muiti;
		if (bMultiSelect == false)
			OffSelectAll();
	}

	internal void SetActive(bool _active)
	{
		gameObject.SetActive(_active);
	}

	internal bool SetSelect(long _id)
	{
		OffHightLightAll();

		if (!bMultiSelect) // 멀티 셀렉트가 아닐때
		{
			if (selectList.Contains(_id)) // 이미 선택되었다면  off
			{
				OffSelect(_id);
				return false;
			}

			OffSelectAll();
			if (table.ContainsKey(_id))
			{
				GetItem(_id).SetSelect(CardBase.SelectState.Select);
				selectList.Add(_id);
			}
		}
		else // 멀티 셀렉트일때
		{
            if ((itemType == CardType.Equipment && GameCore.Instance.PlayerDataMgr.GetItemSData(_id).locked) ||
                (itemType == CardType.Character && GameCore.Instance.PlayerDataMgr.GetUnitSData(_id).locked))
            {
                GameCore.Instance.ShowAlert("잠긴 카드는 다중 선택 할 수 없습니다.");
                return false;
            }

			if (selectList.Contains(_id)) // 이미 선택되었다면  off
			{
				OffSelect(_id);
				return false;
			}
			else if (table.ContainsKey(_id))  // 아니라면 on
			{
				GetItem(_id).SetSelect(CardBase.SelectState.Select);
				selectList.Add(_id);
			}
		}
		return true;
	}



	internal void OffSelectAll()
	{
        for (int i = 0; i < selectList.Count; ++i)
        {
            GetItem(selectList[i]).SetSelect(CardBase.SelectState.None);
            GetItem(selectList[i]).SetButton(CardBase.ActiveButton.None);
        }
        selectList.Clear();
	}

	internal void OffSelect(long _id)
	{
		for( int i = 0; i < selectList.Count;++i)
		{
			if(selectList[i] == _id)
			{
				GetItem(selectList[i]).SetSelect(CardBase.SelectState.None);
                GetItem(selectList[i]).SetButton(CardBase.ActiveButton.None);
                selectList.RemoveAt(i);
				break;
			}
		}
	}

	internal long[] GetSelects()
	{
		return selectList.ToArray();
	}

    internal bool IsSelected(long uid)
    {
        return selectList.Contains(uid);
    }

	internal void SetHighlight(long _id)
	{
		if (!table.ContainsKey(_id))
			return;

		GetItem(_id).SetSelect(CardBase.SelectState.Highlight);
		highlightList.Add(_id);
	}

	internal void OffHightLightAll()
	{
		for( int i = 0; i < highlightList.Count; ++i)
			GetItem(highlightList[i]).SetSelect(CardBase.SelectState.None);
		highlightList.Clear();
	}

	internal virtual void AddItem(Transform _item, long _key, CardBase _value)
	{
		_value.SetCallback(OnClickItem, OnPressItem);
		if(table.ContainsKey(_key))
		{
			GameObject.Destroy(_item.gameObject);
			Debug.LogError("Exist same Key. " + _key);
			return;
		}
		table.Add(_key, _value);
		tfTable.Add(_item, _value);
		_item.parent = gameObject.transform;
		_item.localScale = Vector3.one;

		if (grid !=null)
			grid.enabled = true;
	}

    internal void ResetScrollInBound()
    {
        grid.transform.parent.GetComponent<UIScrollView>().RestrictWithinBounds(false);
    }

    internal Dictionary<long,CardBase>.Enumerator GetEnumerator()
	{
		return table.GetEnumerator();
	}

	internal CardBase GetItem(long _key)
	{
		if (table.ContainsKey(_key))
			return table[_key];
		return null;
	}

	internal CardBase GetItem(Transform _tf)
	{
		if (tfTable.ContainsKey(_tf))
			return tfTable[_tf];
		return null;
	}

	internal CardBase RemoveItem(long _key, bool _destroy = true)
	{
		CardBase result = null;
		if( table.ContainsKey(_key))
		{
			// 테이블에서 삭제
			result = table[_key];
			table.Remove(_key);

			// TF 기반 테이블에서도 삭제
			var it = tfTable.GetEnumerator();
			Transform _tf = null;
			while (it.MoveNext())
			{
				if (it.Current.Value == result)
				{
					_tf = it.Current.Key;
                    _tf.gameObject.SetActive(false);
					tfTable.Remove(_tf);
					break;
				}
			}

			// 재정렬
			if (grid != null)
				grid.enabled = true;

			// 게임 오브젝트 삭제 여부
			if ( _destroy)
			{
				GameObject.Destroy(_tf.gameObject);
				return null;
			}
			else
			{// 해당 하이템 반환
				return result;
			}
		}
		else // 테이블에 존재하지 않는다면 종료
		{
			return null;
		}
	}

    internal void RemoveAllItem()
    {
        var removeList = table.Keys.ToArray();

        for(int i = 0; i < removeList.Length; ++i)
        {
            RemoveItem(removeList[i]);
        }
    }

	internal CardBase RemoveItem(Transform _tf, bool _destroy = true)
	{
		CardBase result = null;
		if (tfTable.ContainsKey(_tf))
		{
			// TF 기반 테이블에서 삭제
			result = tfTable[_tf];
			tfTable.Remove(_tf);

			// 테이블에서도 삭제
			long _key = default(long);
			var it = table.GetEnumerator();
			while (it.MoveNext())
			{
				if (it.Current.Value == result)
				{
					_key = it.Current.Key;
					table.Remove(_key);
					break;
				}
			}
			// 재정렬
			if (grid != null)
				grid.enabled = true;

			// 게임 오브젝트 삭제 여부
			if (_destroy)
			{
				GameObject.Destroy(_tf);
				return null;
			}
			else
			{// 해당 아이템 반환
				return result;
			}
		}
		else // 테이블에 존재하지 않는다면 종료
		{
			return null;
		}
	}

    internal void Reposition(bool _Immediately = true, bool isCardListHeightPositionCheck = false, bool isItemCardListHeightPositionCheck = false)
	{
		if (grid != null)
		{
			if (_Immediately)
				grid.Reposition();
			else
				grid.enabled = true;

            if (coResizeScrolling != null)
                GameCore.Instance.StopCoroutine(coResizeScrolling);

            coResizeScrolling = GameCore.Instance.StartCoroutine(CoWaitResizeScrolling());
        }
	}

    IEnumerator CoWaitResizeScrolling()
    {
        yield return null;

        if (grid == null || grid.transform.parent.GetComponent<UIPanel>() == null)
            yield break;

        var panel = grid.transform.parent.GetComponent<UIPanel>();
        var panelSize = panel.height;
        var gridSize = grid.height;
        var panelOffset = panel.clipOffset.y;
        var gridOffset = grid.transform.localPosition.y;

        if (gridSize <= panelSize)
            yield break;

        //Debug.Log("panelSize : " + panelSize + ", PanelOffset : " + panelOffset);
        //Debug.Log("grid Size : " + gridSize + ", grid Offset : " + gridOffset + "   (" + grid.transform.childCount + ")");

        if (-panelOffset > gridSize - panelSize)
        {
            SpringPanel.Begin(grid.transform.parent.gameObject, Vector3.up * (gridSize - panelSize - gridOffset), 15);
        //    Debug.Log("Scrolling!!! " + (Vector3.down * (gridSize - panelSize)));
        }


        coResizeScrolling = null;
    }

    public int CardListHeight()
    {
        int cardListHeight;

        if (GetCount() % 4 != 0) cardListHeight = (((GetCount() / 4) + 1) - 3) * 205 + 88;
        else cardListHeight = ((GetCount() / 4) - 3) * 205 + 88;

        return cardListHeight;
    }
    public int ItemCardListHeight()
    {
        int cardListHeight;

        if (GetCount() % 4 != 0) cardListHeight = (((GetCount() / 4) + 1) - 4) * 168 + 136;
        else cardListHeight = ((GetCount() / 4) - 4) * 168 + 136;

        return cardListHeight;
    }

    internal void SetSortCallBack(Comparison<Transform> _cb)
	{
		if (grid != null)
			grid.onCustomSort = _cb;

    }

	internal void SetFilterCallBack(Func<CardBase, int, bool> _cb)
	{
		cbFilter = _cb;
	}

	internal void DoFilter(int _filter)
	{
		var it = tfTable.GetEnumerator();

		// UnFiltering
		if (cbFilter == null || _filter == 0)
		{
			while (it.MoveNext())
				it.Current.Key.gameObject.SetActive(true);
		}
		// Filtering 
		else
		{
			while (it.MoveNext())
			{
				var go = it.Current.Key.gameObject;
				var active = cbFilter(it.Current.Value, _filter);

				go.SetActive(active);
			}
		}

		Reposition(false);
        
        //grid.ConstrainWithinPanel();
    }

	protected void OnClickItem(long _uid)
	{
		if (cbClick != null && saveAction.onPressAction == null)
			cbClick(_uid);
        saveAction.GetOnClickAction();
    }

	protected void OnPressItem(long _uid)
	{
		if (cbPress != null && saveAction.onClickAction == null)
			cbPress(_uid);

        saveAction.GetOnPressAction();
    }
}
