using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderScript : MonoBehaviour {

	public int defCount = 100;
	public int minCount = 0;
	public int maxCount = 900;

	internal UISlider slider;
	internal UILabel label;
	internal int count;
	internal int idx; // 식별자

	internal UISprite spInc;
	internal UISprite spDec;

	private Action<int> cbChange;

	private void InitIlnk()
	{
		slider = UnityCommonFunc.GetComponentByName<UISlider>(gameObject, "slider");
		label = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "count");

		slider.onChange.Clear();
		slider.onChange.Add(new EventDelegate(OnChangeSlider));
		slider.numberOfSteps = maxCount;

		var inc = UnityCommonFunc.GetComponentByName<ButtonRapper>(gameObject, "Inc");
		spInc = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Inc");
		inc.SetClickCallback(IncCount);
		inc.SetPressCallback(() => StartCoroutine(CoCount_Loop(true)));
		inc.SetStopPressCallback(() => StopAllCoroutines());
		

		var dec = UnityCommonFunc.GetComponentByName<ButtonRapper>(gameObject, "Dec");
		spDec = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Dec");
		dec.SetClickCallback(DecCount);
		dec.SetPressCallback(() => StartCoroutine(CoCount_Loop(false)));
		dec.SetStopPressCallback(() => StopAllCoroutines());

		SetCount(0);
	}

	internal void Init(int _idx, Action<int> _cb)
	{
		InitIlnk();
		idx = _idx;
		cbChange = _cb;
	}

	private void OnChangeSlider()
	{
		var count = Mathf.Clamp((slider.value * maxCount), minCount, maxCount);
		SetCount(Mathf.RoundToInt(count) + defCount);
	}

	internal void IncCount()
	{
		var count = Mathf.Clamp((slider.value * maxCount) + 1, minCount, maxCount);
		SetCount(Mathf.RoundToInt(count) + defCount);
	}

	internal void DecCount()
	{
		var count = Mathf.Clamp((slider.value * maxCount) - 1, minCount, maxCount);
		SetCount(Mathf.RoundToInt(count) + defCount);
	}

	IEnumerator CoCount_Loop(bool _inc)
	{
		float interval = 0.1f;
		float cnt = 1;
		while (true)
		{
			var count = Mathf.Clamp((slider.value * maxCount) + (int)((_inc) ? cnt : (-cnt)), minCount, maxCount);
			SetCount(Mathf.RoundToInt(count) + defCount);

			if (interval > 0.05f)
				yield return new WaitForSeconds(interval);
			else
				yield return null;

			interval *= 0.6f;
			cnt *= 1.005f;
		}
	}



	internal virtual void SetCount(int _count, bool _notify = false)
	{
		count = Mathf.Clamp(_count, defCount, maxCount + defCount);
		label.text = count.ToString();
		slider.Set((float)(count - defCount) / maxCount, _notify);

		if (count <= defCount)
		{
			spInc.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			spDec.color = new Color(0.1f, 0.1f, 0.1f, 1f);
		}
		else if (count >= maxCount + defCount)
		{
			spInc.color = new Color(0.1f, 0.1f, 0.1f, 1f);
			spDec.color = new Color(0.5f, 0.5f, 0.5f, 1f);
		}
		else
		{
			spInc.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			spDec.color = new Color(0.5f, 0.5f, 0.5f, 1f);
		}

		if (cbChange != null)
			cbChange(idx);
	}

	internal int GetCount()
	{
		return count;
	}

}
