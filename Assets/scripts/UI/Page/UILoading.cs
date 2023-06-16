using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UILoading : UIBase
{
	[SerializeField] UITexture m_Image = null;

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Open()
	{
		m_Image.mainTexture = ResourceManager.Instance.GetTexture("loading_card_001");

		base.Open();
	}

	public override void Close()
	{
		base.Close();
	}
}
