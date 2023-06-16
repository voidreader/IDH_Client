using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class SelectStageUI : MonoBehaviour
{
	internal void Init( EventDelegate.Callback _cbSelectStage, EventDelegate.Callback _cbBack)
	{
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "Stage1Button").onClick.Add(new EventDelegate(_cbSelectStage));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "BattleStartButton").onClick.Add(new EventDelegate(_cbSelectStage));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "Back").onClick.Add(new EventDelegate(_cbBack));
	}
}
