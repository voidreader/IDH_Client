using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILodgingComponent : UIBase
{
	//[SerializeField] UITweener m_Tween = null;
	[SerializeField] UILabel m_txtMessage = null;
	[SerializeField] UILabel lodgingIdx = null;

    public int nIdx;
		private bool bAvtive;
    public override void Initialize()
	{
		base.Initialize();
	}

    public void Init(bool _active)
    {
		bAvtive = _active;
		GetComponent<UISprite>().alpha = bAvtive ? 1f : 0.2f;

		}

	public override void Open()
	{
		base.Open();

		//m_Tween.ResetToBeginning();
		//m_Tween.PlayForward();
	}

	public override void Close()
	{
		base.Close();
	}

    public void OnClickLodgingComponent()
    {
			
	}

    public void SetMessage(string strMsg)
	{
		m_txtMessage.text = strMsg;

		Open();
	}

    public void SetMessage(int nIdx, object arg)
    {
        Open();
    }
}

