using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class FilterItem : MonoBehaviour
{
	UISprite sprite;
	int num;
	Action<int> cb;

	internal void Init(int _num, Action<int> _cb)
	{
		sprite = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "toggle");
		GetComponentInChildren<UIButton>().onClick.Add(new EventDelegate(() => {
			if (cb != null)
				cb(num);
		}));

		num = _num;
		cb = _cb;

	}

	internal void SetToggle(bool _set)
	{
		if (_set)
			sprite.color = Color.magenta;
		else
			sprite.color = Color.gray;
	}
}
