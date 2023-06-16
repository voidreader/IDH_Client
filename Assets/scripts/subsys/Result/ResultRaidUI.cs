using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ResultRaidUI : ResultUIBase
{
    internal static ResultRaidUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Result/PanelResultRaidUI", _parent);
        var result = go.GetComponent<ResultRaidUI>();
        return result;
    }

    [Space]
	[SerializeField] GameObject[] arrChar;

    [Space]
    [SerializeField] GameObject goMoneyRoot;
    [SerializeField] UILabel lbGoldCount;
    [SerializeField] UILabel lbPurlCount;

    [Space]
    [SerializeField] RaidRewardGradeComponent raidGrade;
    [SerializeField] UILabel lbTotalDmg;
    [SerializeField] UILabel lbAddedDmg;

    internal override void Init(ResultBasePara _para)
    {
        base.Init(_para);

        var pr = _para.GetPara<ResultRaidPara>();

        // Set Player Info
		SetPlayerUnitByTeamIndex(para.playerTeamIdx);
        goMoneyRoot.SetActive(false);

        // Raid Info
        raidGrade.Init(pr.raidKey);

        lbAddedDmg.text = string.Empty;
        lbTotalDmg.text = string.Format("{0:N0}", GameCore.Instance.PlayerDataMgr.GetRaidSData(pr.raidKey).damage);
    }

    internal override void TurnOffNextButton()
    {
    }
    internal override void UpdateData(params object[] _params)
    {
        var addedDmg = (int)_params[0];

        var pr = para.GetPara<ResultRaidPara>();
        //var totalDmg = GameCore.Instance.PlayerDataMgr.GetRaidSData(pr.raidKey).damage;

        lbAddedDmg.text = string.Format("+ {0:N0}", addedDmg);
        lbTotalDmg.text = string.Format("{0:N0}", GameCore.Instance.PlayerDataMgr.GetRaidSData(pr.raidKey).damage);
        raidGrade.AddDamage(addedDmg);

        PlayerUnitPSirtes();
    }

    internal override void SetRewardItem(CardSData[] _sdatas)
    {
        var gold = 0;
        var purl = 0;

        for(int i = 0; i < _sdatas.Length; ++i)
        {
            var key = _sdatas[i].key;

            if (key == (int)ResourceType.Gold)
                gold += ((ItemSData)_sdatas[i]).count;
            else if (key == (int)ResourceType.Cash)
                purl += ((ItemSData)_sdatas[i]).count;
        }

        if (gold + purl != 0)
        {
            lbGoldCount.text = gold.ToString("N0");
            lbPurlCount.text = purl.ToString("N0");
            goMoneyRoot.SetActive(true);
        }
    }


    internal void SetPlayerUnitByTeamIndex(int _teamIndex)
    {
        int arrIdx = 0;
        for (int i = 0; i < 6; i++)
        {
            var unitData = GameCore.Instance.PlayerDataMgr.GetUnitDataByTeam(_teamIndex, i);
            if (unitData != null)
            {
                int id = unitData.charIdType;

                GameCore.Instance.SetUISprite(UtilityFunc.Inst.GetChildObj("fore", arrChar[arrIdx]).GetComponent<UI2DSprite>(), unitData.GetSkillSpriteKey());
                UtilityFunc.Inst.GetChildObj("label", arrChar[arrIdx]).GetComponent<UILabel>().text = unitData.name;
                arrIdx++;
            }
        }

        for (; arrIdx < 5; ++arrIdx)
        {
            UtilityFunc.Inst.GetChildObj("bg", arrChar[arrIdx]).GetComponent<UISprite>().spriteName = "";
            //UtilityFunc.Inst.GetChildObj("fore", arrChar[arrIdx]).GetComponent<UI2DSprite>().spriteName = "";
            UtilityFunc.Inst.GetChildObj("label", arrChar[arrIdx]).GetComponent<UILabel>().text = "";
            arrChar[arrIdx].SetActive(false);
        }
    }

    public void PlayerUnitPSirtes()
    {
        var unitCardTws = new UITweener[arrChar.Length];
        for (int i = 0; i < unitCardTws.Length; ++i)
        {
            unitCardTws[i] = arrChar[i].GetComponent<UITweener>();
            unitCardTws[i].ResetToBeginning();
            unitCardTws[i].PlayForward();
        }
    }

    public void OnClickRaidPrepare()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        var data = GameCore.Instance.DataMgr.GetRaidData(para.GetPara<ResultRaidPara>().raidKey);
        GameCore.Instance.ChangeSubSystem(SubSysType.RaidPrepare, new StoryPara(data.difficult-1, false, para.playerTeamIdx));
    }

    public void OnClickReturnHome()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
    }
}
