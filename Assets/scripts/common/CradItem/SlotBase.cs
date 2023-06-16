using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlotBase<T> : MonoBehaviour where T : Component
{
	long id;
	int slotNum;
	Action<int> cbClick;
	Action<int> cbPress;
	Action<int> cbSwap;

    public SaveAction saveAction = new SaveAction();

	GameObject item;

	bool bSwapable;

	internal long Id { get { return id; } }
	internal int SlotNum { get { return slotNum; } }
	internal T Item {  get { return item != null ? item.GetComponent<T>() : null; ; } }

	protected bool selected;  // 파란 강조효과
	protected bool affected;  // 빨간 강조효과
	protected bool bButton;		// 버튼 활성화여부


	protected abstract void UpdateAffect(bool _active);
	protected abstract void UpdateSelect(bool _active);
	protected abstract void UpdateSlotOn(bool _on);
	protected abstract void UpdateButton(bool _active);

	protected void Init(int _num, Action<int> _cbClick, Action<int> _cbPress, Action<int> _cbSwap)
	{
		slotNum = _num;
		cbClick = _cbClick;
		cbPress = _cbPress;
		cbSwap = _cbSwap;
		id = -1L;
		bSwapable = false;
		UpdateSelect(false);
		UpdateSlotOn(false);
		UpdateButton(false);
	}


	protected void SetID(long _id)
	{
		id = _id;
	}

	protected T SetItem(long _id, T _item)
	{
		T result = Item;
		id = _id;
		item = _item.gameObject;

		UpdateSlotOn(true);
		return result;
	}

	protected T RemoveItem()
	{
		T result = Item;
		id = -1;
		item = null;

		UpdateSlotOn(false);
		return result;
	}

	internal void SetSelect(bool _select)
	{
		selected = _select;
		UpdateSelectnAffect();
	}
	internal void SetAffect(bool _affect)
	{
		affected = _affect;
		UpdateSelectnAffect();
	}
	internal void SetButton(bool _active)
	{
		bButton = _active;
		UpdateButton(bButton);
	}


	private void UpdateSelectnAffect()
	{
		if (affected)
			UpdateAffect(true);
		else if (selected)
			UpdateSelect(true);
		else
			UpdateSelect(false);
	}


	public void OnClick()
	{
        //if (bPressed == true)
        //{
        //	bPressed = false;
        //	return;
        //}
        if (Id <= 0)
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Pre);
        else
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);

        if (cbClick != null)
			cbClick(slotNum);
        saveAction.GetOnClickAction();
	}

	public void OnPress(bool _press)
	{
		if (_press)
		{
			StopAllCoroutines();
			bSwapable = true;
			StartCoroutine(GameCore.CoWaitCall(2f, () => {
                if (cbPress != null)
                    cbPress(slotNum);
                bSwapable = false;
            }));
		}
		else
		{
			StopAllCoroutines();
			if (bSwapable && cbSwap != null)
				cbSwap(slotNum);
		}
	}
}
