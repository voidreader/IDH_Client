using System;
using System.Collections.Generic;
using UnityEngine;



class EffectAlert : MonoBehaviour
{
	UILabel text;
	UISprite sprite;

	Transform tf;
	Vector3 pos;
	float acc;
	float tgTime = 1f;
	float x = 0.1f;

	Vector3 worldPos;

	internal void Init(Vector3 _pos, EffectType _type, int _value)
	{
		if (tf == null)
			tf = transform;

		if (text == null)
			text = GetComponent<UILabel>();

		if (sprite == null)
			sprite = GetComponentInChildren<UISprite>();

		sprite.gameObject.SetActive(false);
		text.fontSize = 40;

		worldPos = _pos;
		pos = GameCore.Instance.WorldPosToUIPos(worldPos);
		tf.localPosition = pos;
		acc = 0;
		text.alpha = 1f;

		switch (_type)
		{
			case EffectType.CounterAtk:			text.text = "반격!"; break;
			case EffectType.FollowAtk:			text.text = "협공!"; break;
			case EffectType.SetGuard:			text.text = "가드!"; break;
			case EffectType.ProportionDmg:	    text.text = "정확한 한방"; break;

			case EffectType.Sleep:				text.text = "수면" + _value + "턴"; break;
			case EffectType.Stun:				text.text = "기절" + _value + "턴"; break;
			case EffectType.paralyze:			text.text = "마비" + _value + "턴"; break;
		}

		gameObject.SetActive(true);
	}

	internal void Init(Vector3 _pos, UnitStat _type, bool _inc, int _value)
	{
		if (tf == null)
			tf = transform;

		if (text == null)
			text = GetComponent<UILabel>();

		if (sprite == null)
			sprite = GetComponentInChildren<UISprite>();

		sprite.gameObject.SetActive(false);
		text.fontSize = 26;

		worldPos = _pos;
		pos = GameCore.Instance.WorldPosToUIPos(worldPos);
		tf.localPosition = pos;
		acc = 0;
		text.alpha = 1f;

		string inc = "";
		if (_inc) inc = "증가";
		else inc = "감소";

		switch (_type)
		{
			case UnitStat.Hp:		text.text = "최대체력" + inc + _value + "턴"; break;
			case UnitStat.Attack:	text.text = "공격력" + inc + _value + "턴"; break;
			case UnitStat.Armor:	text.text = "방어력" + inc + _value + "턴"; break;
			case UnitStat.Vigor:	text.text = "행동력" + inc + _value + "턴"; break;
			case UnitStat.Agility:	text.text = "민첩성" + inc + _value + "턴"; break;
			case UnitStat.Concent:	text.text = "집중력" + inc + _value + "턴"; break;
			case UnitStat.Mental:	text.text = "정신력" + inc + _value + "턴"; break;
			case UnitStat.Recovery: text.text = "회복력" + inc + _value + "턴"; break;
			case UnitStat.Aggro:    text.text = "어그로" + inc + _value + "턴"; break;
			default: text.text = ""; break;
		}

		gameObject.SetActive(true);
	}

	private void Update()
	{
		acc += Time.deltaTime * GameCore.timeScale;
		var value = acc / tgTime;

		if (value >= 1)
			gameObject.SetActive(false);
		else
		{
			tf.localPosition = GameCore.Instance.WorldPosToUIPos(worldPos) + (new Vector3(0f, value * 30f, 0f));

			value = value < 0.8f ? 1f : (0.2f - (value - 0.8f)) / 0.2f;
			text.alpha = value;
		}
	}
}
