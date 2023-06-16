using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class UIPvPMatch : /*MonoBehaviour*/UIBase
{
	public enum PVPInfoTarget
	{
		Player,
		Opponent
	}

		
  //  [SerializeField]
  //  UISprite userSprite;
  //  [SerializeField]
  //  UILabel userLevel;
  //  [SerializeField]
  //  UILabel userName;
  //  [SerializeField]
  //  UILabel userPower;

		
		[SerializeField]PvPPlayerInfoScript myPvPInfo;
		[SerializeField]PvPPlayerInfoScript oppPvPInfo;

		[SerializeField]
    GameObject charSlotComponent;

		[SerializeField]
		UIGrid myGrid;

    [SerializeField]
    UIGrid oppGrid;

    [SerializeField]
    UISprite unionRankIcon;

    [SerializeField]
    UILabel researchBtnCoinLbl;

	[SerializeField]
	GameObject researchButton;

	[SerializeField]
	UILabel lbCost;

	Action cbStartPvPBattle;

    [SerializeField]
    Animator animator;

    [SerializeField]
    RuntimeAnimatorController runtimeAnimatorController;

    internal void Init(Action _cbStartPvPBattle)
    {
        cbStartPvPBattle = _cbStartPvPBattle;
        // Create Team Card
        if (myGrid == null)
        {
            GameObject ItemList = UtilityFunc.Inst.GetChildObj("CenterGrade", gameObject);
            myGrid = UtilityFunc.Inst.GetChildObj("Left_B", ItemList).GetComponent<UIGrid>();
        }

        if (myGrid.transform.childCount != 0)
            for (int i = myGrid.transform.childCount - 1; i >= 0; --i)
            {
                var tg = myGrid.transform.GetChild(i);
                tg.parent = null;
                Destroy(tg.gameObject);
            }

        for (int i = 0; i < 6; ++i)
        {
            long uid = GameCore.Instance.PlayerDataMgr.GetTeamIds(CommonType.TEAM_PVP_IDX, i);
            if (uid == -1)
                continue;

            CardSData sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(uid);
            CardBase card = CardBase.CreateBigCard(sdata, myGrid.transform);
            card.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            card.Init(sdata, null, null, null);
            // 카드 설정 콜백
            //card.gameObject.SetActive(CBSetCard(_type, card));
            //NGUITools.AddChild(myGrid.gameObject, tm);
        }

        myGrid.enabled = true;
        myGrid.Reposition();

        // 플레이어 정보 출력
        var pvpData = GameCore.Instance.PlayerDataMgr.PvPData;
        var rank = GameCore.Instance.PlayerDataMgr.PvPRank;
        var groupRank = GameCore.Instance.PlayerDataMgr.PvPGroupRank;
        var teamskillKey = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(CommonType.TEAM_PVP_IDX);
        SetPvPInfo(PVPInfoTarget.Player, pvpData, rank, groupRank);
        SetPvPTeamSkill(PVPInfoTarget.Player, teamskillKey);

        // 재탐색 관련 정보 출력
        UpdateDiscoverCost(pvpData.rediscover);

        // uionranksprite (중앙 랭크 아이콘)
        var grade = pvpData.grade + (!pvpData.placement ? 1 : 0);
        unionRankIcon.spriteName = GetGradeBigSprite(grade);
        RankEffectManager.CreatePVP(grade, unionRankIcon.transform);

        if (!pvpData.placement)
        {
            lbCost.text = "0";
        }

    }

	public void SetPvPInfo(PVPInfoTarget _opp, PvPSData _data, int _rank, int _groupRank)
	{
		var tg = _opp == PVPInfoTarget.Player ? myPvPInfo : oppPvPInfo;
		tg.SetData(_opp, _data, _rank, _groupRank);
	}

	public void SetPvPTeamSkill(PVPInfoTarget _opp, int _tskillKey)
	{
		var tg = _opp == PVPInfoTarget.Player ? myPvPInfo : oppPvPInfo;
		tg.SetSkillData(_tskillKey);
	}


	public void CreateOpponentTeamCard(List<PvPOppUnitSData> pvpCharList)
	{
		// Remove Prev Card
		if (oppGrid.transform.childCount != 0)
			for (int i = oppGrid.transform.childCount - 1; i >= 0; --i)
			{
				var tg = oppGrid.transform.GetChild(i);
				tg.parent = null;
				Destroy(tg.gameObject);
			}

		for (int i = 0; i < pvpCharList.Count; ++i)
		{
			int key = pvpCharList[i].charID;

			CardBase card = CardBase.CreateCard(null, GameCore.Instance.DataMgr.GetUnitData(key), true, oppGrid.transform);
            card.SetEnchant(pvpCharList[i].enchant);

            card.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		}

		oppGrid.enabled = true;
		oppGrid.Reposition();
	}

	public void UpdateDiscoverCost(int _rediscoverCount)
	{
		int initVal = GameCore.Instance.DataMgr.GetPvPConstData().researchInitVal;
		researchBtnCoinLbl.text = (initVal + (_rediscoverCount * GameCore.Instance.DataMgr.GetPvPConstData().researchIncVal)).ToString();
	}

    public static string GetGradeBigSprite(int nGrade)
    {
        string strRank = null;

        if (nGrade == 7000001)
        {
            strRank = "LV_01_BRONS_B";
        }
        else if (nGrade == 7000002)
        {
            strRank = "LV_02_BRONS_B";
        }
        else if (nGrade == 7000003)
        {
            strRank = "LV_03_SILVER_B";
        }
        else if (nGrade == 7000004)
        {
            strRank = "LV_04_SILVER_B";
        }
        else if (nGrade == 7000005)
        {
            strRank = "LV_05_GOLD_B";
        }
        else if (nGrade == 7000006)
        {
            strRank = "LV_06_GOLD_B";
        }
        else if (nGrade == 7000007)
        {
            strRank = "LV_07_PLATINUM_B";
        }
        else if (nGrade == 7000008)
        {
            strRank = "LV_08_MASTER_B";
        }
        else if (nGrade == 7000009)
        {
            strRank = "LV_09_CHAMPION_B";
        }

        return strRank;
    }

	public static string GetGradeSmallSprite(int nGrade)
	{
		string strRank = null;

		if (nGrade == 7000001)
		{
			strRank = "LV_01_BRONS_S";
		}
		else if (nGrade == 7000002)
		{
			strRank = "LV_02_BRONS_S";
		}
		else if (nGrade == 7000003)
		{
			strRank = "LV_03_SILVER_S";
		}
		else if (nGrade == 7000004)
		{
			strRank = "LV_04_SILVER_S";
		}
		else if (nGrade == 7000005)
		{
			strRank = "LV_05_GOLD_S";
		}
		else if (nGrade == 7000006)
		{
			strRank = "LV_06_GOLD_S";
		}
		else if (nGrade == 7000007)
		{
			strRank = "LV_07_PLATINUM_S";
		}
		else if (nGrade == 7000008)
		{
			strRank = "LV_08_MASTER_S";
		}
		else if (nGrade == 7000009)
		{
			strRank = "LV_09_CHAMPION_S";
		}

		return strRank;
	}

	public void DisibleResearchButton()
	{
		researchButton.SetActive(false);
		lbCost.text = "0";
	}

	public void OnClickResearchBtn()
    {
        ++GameCore.Instance.PlayerDataMgr.ResearchCount;
        GameCore.Instance.NetMgr.Req_PvPReDiscover();
    }

    public void OnClickEditTeam()
    {
        var sys = GameCore.Instance.SubsysMgr.GetNowSubSys() as PvPMatchSys;
        GameCore.Instance.ChangeSubSystem(SubSysType.EditTeam, new ReturnSysPara(SubSysType.PvPMatch, 4, sys.para));
    }

    public void OnClickStartBattle()
    {
        if (researchButton.activeSelf == true && GameCore.Instance.PlayerDataMgr.PvPData.placement)
            GameCore.Instance.NetMgr.Req_PvPStartBattle();
        else
            cbStartPvPBattle(); // 배치고사 중일땐 REQ_PVP.203(PVP_STARTBATTLE)를 보내지 않는다.
	}

    public void SetAnimator()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_PVP_Ready);

        if (!animator.enabled)  animator.enabled = true;
        else                    animator.SetTrigger("ShowVS");
    }
}
