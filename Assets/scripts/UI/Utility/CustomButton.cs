using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomButton : MonoBehaviour
{
	public enum EventTrigger { OnPress = 0, OnClick, OnRelease }

	[System.Serializable] public class ButtonEvent : UnityEvent<int> { }

	[SerializeField] ButtonEvent OnButtonEvent = null;
	[SerializeField] EventTrigger eTrigger = EventTrigger.OnClick;
	[SerializeField] int nBtnIndex = 0;

	private BoxCollider2D m_Collider = null;
	private UISprite[] m_Sprite = null;
	private UILabel[] m_Label = null;
	private Color[] m_LabelColor;
	private bool m_bDisabled = false;

	public ESoundID eSoundID = ESoundID.sd_ef_001;

	void Awake()
	{
		m_Collider = GetComponent<BoxCollider2D>();
	}

	public bool IsDisable()
	{
		return m_bDisabled;
	}

	public void SetEnable(bool bEnable)
	{
		SetEnable(bEnable, bEnable);
	}

	public void SetEnable(bool bEnableCollider, bool bEnableBtn)
	{
		if (m_Collider) m_Collider.enabled = bEnableCollider;

		m_bDisabled = !bEnableBtn;

		if (null == m_Sprite)
			m_Sprite = GetComponentsInChildren<UISprite>();

		if (null == m_Label)
		{
			m_Label = GetComponentsInChildren<UILabel>();

			if (null != m_Label)
			{
				m_LabelColor = new Color[m_Label.Length];
				for (int i = 0; i < m_Label.Length; ++i)
					m_LabelColor[i] = m_Label[i].color;
			}
		}

		if (null != m_Sprite)
		{
			for (int i = 0; i < m_Sprite.Length; ++i)
				m_Sprite[i].color = (bEnableBtn) ? Color.white : Color.gray;
		}

		if (null != m_Label)
		{
			for (int i = 0; i < m_Label.Length; ++i)
				m_Label[i].color = (bEnableBtn) ? m_LabelColor[i] : Color.gray;
		}
	}

	public void SetBtnText(string strBtnText)
	{
		if (null == m_Label)
			m_Label = GetComponentsInChildren<UILabel>();

		if (null != m_Label)
		{
			if (0 != m_Label.Length)
				m_Label[0].text = strBtnText;
		}
	}

	void OnPress(bool bPress)
	{
		if (EventTrigger.OnClick == eTrigger)
			return;

		if (!enabled || !m_Collider.enabled)
			return;

		if (bPress)
		{
			if (EventTrigger.OnPress == eTrigger)
				OnButtonEvent.Invoke(nBtnIndex);
		}
		else
		{
			if (EventTrigger.OnRelease == eTrigger)
				OnButtonEvent.Invoke(nBtnIndex);
		}
	}

	void OnClick()
	{
		if (EventTrigger.OnClick != eTrigger)
			return;

		if (!enabled || !m_Collider.enabled)
			return;

		OnButtonEvent.Invoke(nBtnIndex);

		//if (eSoundID != ESoundID.None)
        //SoundManager.Instance.MakeSFX(eID);
        //UIManager.Instance.PlaySFX(eSoundID);
    }

	void OnDestroy()
	{
		OnButtonEvent.RemoveAllListeners();
	}
}
