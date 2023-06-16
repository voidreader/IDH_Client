using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetManager : SingletonInst<NetManager>
{
	public struct SMinigameClearData
	{
		public long nKey;
		public int nNormalCount;
		public int nGoodCount;
		public int nGreatCount;
	}

    [HideInInspector] public bool IsWaiting = false;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Init()
    {
        base.Init();
    }

    public override void Clear()
    {
        base.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void RequestTownDeleteAll()
    {

    }

    public void RequestTownBalanceAccounts(bool bLogin = false)
    {

    }

    public void RequestTownUpgradeBuilding(int objectSID, int complete_flag = 0)
    {
        
    }

    public void RequestTownOpenArea(int areaIdx)
    {
        
    }
}


