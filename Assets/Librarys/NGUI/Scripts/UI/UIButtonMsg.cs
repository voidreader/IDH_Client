//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>
public delegate void UIMsgDelegate(GameObject senderObj);

[AddComponentMenu("NGUI/Interaction/Button Msg")]
public class UIButtonMsg : MonoBehaviour
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		OnDoubleClick,
	}

  public UIMsgDelegate m_uiMsgDelegate = null;
	public Trigger trigger = Trigger.OnClick;
	public bool includeChildren = false;

	bool mStarted = false;
	bool mHighlighted = false;

	void Start () { mStarted = true; }

	void SetCallback(UIButton _button, MonoBehaviour _target, string _funcName )
	{
		var eventDelegate = new EventDelegate(_target, _funcName);
		EventDelegate.Add(_button.onClick, eventDelegate);
		EventDelegate.Remove(_button.onClick, eventDelegate);


	}
	//void OnEnable() { if (mStarted && mHighlighted) OnHover(UICamera.IsHighlighted(gameObject)); }

	void OnHover (bool isOver)
	{
		if (enabled)
		{
			if (((isOver && trigger == Trigger.OnMouseOver) ||
				(!isOver && trigger == Trigger.OnMouseOut))) Send();
			mHighlighted = isOver;
		}
	}

	void OnPress (bool isPressed)
	{
		if (enabled)
		{
			if (((isPressed && trigger == Trigger.OnPress) ||
				(!isPressed && trigger == Trigger.OnRelease))) Send();
		}
	}

	void OnClick () { if (enabled && trigger == Trigger.OnClick) Send(); }

	void OnDoubleClick () { if (enabled && trigger == Trigger.OnDoubleClick) Send(); }

	void Send ()
	{
        if (m_uiMsgDelegate == null) return;

		if (includeChildren)
		{
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();

			for (int i = 0, imax = transforms.Length; i < imax; ++i)
			{
				Transform t = transforms[i];
                m_uiMsgDelegate(t.gameObject);
			}
		}
		else
		{
						m_uiMsgDelegate(gameObject);
		}
	}
}