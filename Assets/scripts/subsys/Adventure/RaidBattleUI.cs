using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 레이드모드 전투시스템 UI
/// </summary>
public class RaidBattleUI : BattleUIBase
{
    public static RaidBattleUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/PanelBattleUI", _parent);
        return go.AddComponent<RaidBattleUI>();
    }

    GameObject raidInfo;
    UILabel lbRaidInfo;
    UILabel lbRaidBuff;
    UILabel lbRaidPower;
    UILabel lbRaidTime;

    GameObject friendVERoot;
    UI2DSprite sp2dFirendIllust;
    UILabel lbFriendInfo;

    UnitStatusUI bossUI;

    protected override void InitInternal(PvPSData _oppenetData = null)
    {
        raidInfo = UnityCommonFunc.GetGameObjectByName(gameObject, "RaidInfo");
        lbRaidInfo = UnityCommonFunc.GetComponentByName<UILabel>(raidInfo, "lbMain");
        lbRaidBuff = UnityCommonFunc.GetComponentByName<UILabel>(raidInfo, "lbBuff");
        lbRaidPower = UnityCommonFunc.GetComponentByName<UILabel>(raidInfo, "lbMonPower");
        lbRaidTime = UnityCommonFunc.GetComponentByName<UILabel>(raidInfo, "lbTimer");
        raidInfo.SetActive(true);

        friendVERoot = UnityCommonFunc.GetGameObjectByName(gameObject, "friendTeamVE_Root");
        sp2dFirendIllust = UnityCommonFunc.GetComponentByName<UI2DSprite>(friendVERoot, "sp2dFriendTeamVEIllust");
        lbFriendInfo = UnityCommonFunc.GetComponentByName<UILabel>(friendVERoot, "lbFriendTeamVEInfo");
        UnityCommonFunc.GetComponentByName<UITweener>(friendVERoot, "friendTeamVE").onFinished.Add(new EventDelegate(() => friendVERoot.SetActive(false)));
        friendVERoot.SetActive(false);

        bossUI = UnityCommonFunc.GetComponentByName<UnitStatusUI>(gameObject, "charStatus_boss");
        
        strikeBt_Root.SetActive(false);
    }

    internal void SetBossStatusUI(BattleUnitData _unit, float _damage)
    {
        bossUI.InitForBoss(_unit, _damage);
        //bossUI.gameObject.SetActive(true);
    }

    internal UnitStatusUI GetBossStatusUI()
    {
        return bossUI;
    }

    public void SetFriendVEInfo(int _illustKey, string _name, int _power)
    {
        GameCore.Instance.SetUISprite(sp2dFirendIllust, _illustKey);
        lbFriendInfo.text = string.Format("[b][F600FF]{0}[-]의 팀 / 팀 전투력 : [24FF00]{1:N0}[-][/b]", _name, _power);
    }

    internal override void ShowRoundText(int _round)
    {
        //friendVERoot.SetActive(true);
    }

    protected override void UpdateTime()
    {
        lbRaidTime.text = ((int)timeValue / 60) + ":" + ((int)timeValue % 60).ToString("00");
    }


}
