using System;
using System.Collections.Generic;
using UnityEngine;
class ButtonRapper : MonoBehaviour
{

	protected Action cbClick;
	protected Action cbPress;
	protected Action cbStopPress;

    UIButton btn;

    SaveAction saveAction = new SaveAction();

    public float pressDelay = 0.5f;
    public SFX ClickSound = SFX.Sfx_UI_Button;
    internal bool Pressed { get; private set; }

    public void Awake()
    {
        btn = GetComponent<UIButton>();
    }


    internal void SetClickCallback(Action _cb)
	{
        if (btn != null)
        {
            cbClick = null;
            btn.onClick.Clear();
            btn.onClick.Add(new EventDelegate(new EventDelegate.Callback(_cb)));
        }
        else
        {
            cbClick = _cb;
        }
    }
	internal void SetPressCallback(Action _cb)
	{
        cbPress = _cb;
    }
	internal void SetStopPressCallback(Action _cb)
	{
        cbStopPress = _cb;
    }

    internal void SetClickAddCallback(Action _cb)
    {
        saveAction.onClickAction = _cb;
        //cbClickAdd += _cb;
    }
    internal void SetPressAddCallback(Action _cb)
    {
        saveAction.onPressAction = _cb;
        //cbPressAdd += _cb;
    }
    internal void SetStopPressAddCallback(Action _cb)
    {
        saveAction.stopPressAction = _cb;
        //cbStopPressAdd += _cb;
    }


    private void OnClick()
	{
        if (ClickSound != SFX.None && (btn == null || btn.enabled == true))
            GameCore.Instance.SoundMgr.SetCommonBattleSound(ClickSound);

		if (cbClick != null)
        {
            cbClick();
        }
        saveAction.GetOnClickAction();

    }

	private void OnPress(bool _press)
	{
		if (_press)
		{
			StopAllCoroutines();
			if(cbPress != null)
				StartCoroutine(GameCore.CoWaitCall(pressDelay, () => {
                    cbPress();

                    saveAction.GetOnPressAction();
                    Pressed = true; }));
		}
		else
		{
			Pressed = false;
			if (cbStopPress != null) cbStopPress();

            saveAction.GetStopPressAction();
            StopAllCoroutines();
		}
	}
}
