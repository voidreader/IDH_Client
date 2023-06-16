using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatisfactionInfoScript : MonoBehaviour
{
	[Header("Head")]
	public GameObject HeadRoot;
	public UILabel lbOpen;
	public UILabel lbLimitValue;

	[Header("Body")]
	public GameObject bodyRoot;
	public UISlider[] sliders;
	public UISprite[] spLevels;
	public UILabel[] lbLevels;


	int roomIndex;
	MyRoomDataMap data;

	public void SetData(int _index, int _satisfaction)
	{
		SetRoomIndex(_index);
		SetValue(_satisfaction);
	}


	public void SetRoomIndex(int _index)
	{
		roomIndex = _index;
		data = GameCore.Instance.DataMgr.GetMyRoomData(roomIndex);
		for (int i = 0; i < 4; ++i)
			lbLevels[i].text = string.Format("{0}) {1}", i + 1, MyRoomDataMap.GetStrMyRoomEffect(data.satisfactionEffectID[i], data.satisfactionEffectValue[i]));
	}

	public void ShowBody(bool _show)
	{
		bodyRoot.SetActive(_show);
		lbOpen.text = _show ? "▲" : "▼";
	}

	public void SetValue(int _value)
	{
		if (data == null)
			return;

		int prevCost = 0;
		bool notReaching = false;
		for (int i = 0; i < 4; ++i)
		{
			if (data.satisfactionCost[i] <= _value)
			{
				sliders[i].value = 1f;
				spLevels[i].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
				lbLevels[i].color = Color.white;
			}
			else
			{
				if (notReaching == false)
				{
					notReaching = true;
					var max = data.satisfactionCost[i] - prevCost;
					var min = _value - prevCost;
					sliders[i].value = (float)min / max;
				}
				else
				{
					sliders[i].value = 0f;
				}
				spLevels[i].color = new Color32(0x89, 0x89, 0x89, 0xFF);
				lbLevels[i].color = new Color32(0x89, 0x89, 0x89, 0xFF);
			}
			prevCost = data.satisfactionCost[i];
		}
		lbLimitValue.text = string.Format("[F600FFFF]{0}[-] / {1}", _value, prevCost);
	}

	private void Update()
	{
		{
//#if UNITY_EDITOR
			if (Input.GetMouseButtonDown(0))
//#else
//			if(Input.GetTouch(0).phase == TouchPhase.Began)
//#endif
			{
				RaycastHit2D hit;
				if (UnityCommonFunc.GetCameraHitInfo2D(GameCore.Instance.GetUICam(), out hit, "UI"))
				{
					if (bodyRoot.activeSelf)
					{
						var Colls = bodyRoot.transform.GetComponent<Collider2D>();
						if (Colls != hit.collider)
							ShowBody(false);
					}
					else
					{
						var Colls = HeadRoot.transform.GetComponent<Collider2D>();
						if (Colls == hit.collider)
							ShowBody(true);
					}
				}
			}
		}
	}



public void OnPress()
{
	Debug.Log("OnPress");
}
public void OnPress(bool _down)
{
	Debug.Log("OnPress" + _down);
}
public void OnClick()
{
	Debug.Log("OnClick");
}
}
