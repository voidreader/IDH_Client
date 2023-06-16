using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class MyRoomUI : MonoBehaviour
{
    public Dictionary<Collider, GameObject> gameObjectList;

    public bool GetGameObject(ref Collider collider, out GameObject reVal)
    {
        reVal = null;
        if(gameObjectList.TryGetValue(collider, out reVal)) return true;
        return false;
    }

    public static MyRoomUI instance;


	internal void Init( EventDelegate.Callback _cbSelectStage, EventDelegate.Callback _cbBack)
	{
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "Stage1Button").onClick.Add(new EventDelegate(_cbSelectStage));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "BattleStartButton").onClick.Add(new EventDelegate(_cbSelectStage));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "Back").onClick.Add(new EventDelegate(_cbBack));
	}
}
