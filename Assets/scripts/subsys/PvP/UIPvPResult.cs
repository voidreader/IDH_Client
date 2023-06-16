using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPvPResult : UIBase
{
    public enum From
    {
        PvP,
        Placement,
        Revenge,
        Raid
    }

	[Header("Main")]
	[SerializeField]	UISprite resultSprite;
	[SerializeField]    GameObject[] arrChar;
   
	[Space]
	[Header("Result")]
	[SerializeField]	UILabel lbLeagueGuide;
	[SerializeField]	UILabel lbPointGuide;
	[SerializeField]	GameObject resultRoot;

	[Header(" - Not PlaceMent")]
	[SerializeField]	GameObject noPlacementRoot;
	[SerializeField]	UILabel lbRank;
	[SerializeField]	UISprite spRankUpDown;
	[SerializeField]	UILabel lbAddedPoint;
	[SerializeField]	UILabel lbPoint;
	int cachedGrade;

	[Header(" - PlaceMent")]
	[SerializeField]	GameObject placementRoot;
	[SerializeField]	UISprite spRankIcon;
	[SerializeField]	UISprite spNextRankIcon;
	[SerializeField]	UILabel lbNoNextGuide;
	int cachedRank;
	int cachedPoint;

    [Header(" - Raid")]
    [SerializeField] GameObject raidRoot;
    [SerializeField] RaidRewardGradeComponent raidGrade;
    [SerializeField] UILabel lbTotalDmg;
    [SerializeField] UILabel lbAddedDmg;

    [Space]
    [Header("Bottom")]
    [SerializeField] UIAnchor uIAnchor;
	[SerializeField]  GameObject nextBattleBtn;
	[SerializeField]  UILabel lbBackbutton;


	From from;
	

	internal static UIPvPResult Create()
	{/*
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Result/PanelResultUI", GameCore.Instance.ui_root);
		var result = go.GetComponent<UIPvPResult>();
		result.InitLink();
		return result;
        */
        if (!UIManager.Instance.GetParentPage())
        {
            GameObject uiRoot = GameObject.Find("UI Root");
            GameObject pageRoot = UtilityFunc.Inst.GetChildObj(uiRoot, "Page");
            if (!pageRoot)
                pageRoot = new GameObject("Page");
            pageRoot.transform.parent = uiRoot.transform;
            NGUITools.SetLayer(pageRoot, LayerMask.NameToLayer("UI"));

            UIManager.Instance.SetParentPage(pageRoot);
            pageRoot.transform.localScale = new Vector3(1, 1, 1);

        }

        UIPvPResult result = UIManager.Instance.GetPage<UIPvPResult>(EUIPage.UIPvPResult);

        return result;

    }

	internal void Init(From _revenge, bool _win, int _playerTeamIdx)
	{
		// Set Player Info
		SetPlayerUnitByTeamIndex(_playerTeamIdx);

		// Set Win or Lose Text Icon
		if (_win)		resultSprite.spriteName = "P_TEXT_VICTORY";
		else			resultSprite.spriteName = "P_TEXT_DEFEATED";

        // Set Result Data
        cachedGrade = GameCore.Instance.PlayerDataMgr.PvPData.grade;

        from = _revenge;
        switch (_revenge)
        {
            case From.Placement:
                resultRoot.SetActive(true);
                noPlacementRoot.SetActive(false);
                placementRoot.SetActive(true);
                raidRoot.SetActive(false);
                lbLeagueGuide.text = "달성 등급";
                lbPointGuide.text = "도전 등급";
                lbBackbutton.text = "로비";

                spRankIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(cachedGrade);
                if (cachedGrade + 1 < 7000009)
                    spNextRankIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(cachedGrade + 1);
                else
                {
                    spNextRankIcon.gameObject.SetActive(false);
                    lbNoNextGuide.gameObject.SetActive(true);
                }
                nextBattleBtn.SetActive(false);
                break;

            case From.PvP:
                resultRoot.SetActive(true);
                noPlacementRoot.SetActive(true);
                placementRoot.SetActive(false);
                raidRoot.SetActive(false);
                var data = GameCore.Instance.DataMgr.GetPvPRateRewardData(cachedGrade);
                lbLeagueGuide.text = string.Format("{0} 리그", data.name);
                lbPointGuide.text = "포인트";
                lbBackbutton.text = "로비";

                cachedRank = GameCore.Instance.PlayerDataMgr.PvPGroupRank;
                cachedPoint = GameCore.Instance.PlayerDataMgr.PvPData.point;
                lbRank.text = string.Format("{0:N0}위", cachedRank);
                lbPoint.text = string.Format("{0:N0}", cachedPoint);
                break;

            case From.Revenge:
                resultRoot.SetActive(false);
                nextBattleBtn.SetActive(false);
                raidRoot.SetActive(false);
                lbBackbutton.text = "숙소";
                return;

            case From.Raid:
                resultRoot.SetActive(false);
                raidRoot.SetActive(true);
                nextBattleBtn.SetActive(false);
                lbBackbutton.text = "로비";

                break;

            default:
                break;
        }
        uIAnchor.relativeOffset = Vector2.up * 0.025f;
        uIAnchor.enabled = true;
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

		for(; arrIdx < 5; ++arrIdx)
		{
			//UtilityFunc.Inst.GetChildObj("bg", arrChar[arrIdx]).GetComponent<UISprite>().spriteName = "";
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

	public void UpdateResult(int _addedPoint, int _addedRank)
	{
		StartCoroutine(CoNoPlacementAnim(_addedPoint, _addedRank));
	}

	public void UpdatePlacement(int _nowGrade)
	{
		StartCoroutine(CoPlacementAnim(_nowGrade));
	}

	IEnumerator CoNoPlacementAnim(int _addedPoint, int _addedRank)
	{
		// Unit Card Anim
		var unitCardTws = new UITweener[arrChar.Length];
		for(int i = 0; i < unitCardTws.Length; ++i)
		{
			unitCardTws[i] = arrChar[i].GetComponent<UITweener>();
			unitCardTws[i].ResetToBeginning();
			unitCardTws[i].PlayForward();
		}

		while (true)
		{
			yield return null;

			for (int i = 0; i < unitCardTws.Length; ++i)
				if (unitCardTws[i].enabled == true)
					continue;

			break;
		}

		// Point Anim
		if (_addedPoint < 0)
			lbAddedPoint.color = Color.red;

		float time = 1f;
		float acc = Time.deltaTime;
		while (acc < time)
		{
			var value = (int)(_addedPoint * (acc / time));
			lbPoint.text = (cachedPoint + value).ToString("N0");
			lbAddedPoint.text = string.Format("{0} {1:N0}", _addedPoint < 0 ? '-' : '+', Mathf.Abs(value));

			yield return null;
			acc += Time.deltaTime;
		}

		lbPoint.text = (cachedPoint + _addedPoint).ToString("N0");
		lbAddedPoint.text = string.Format("{0} {1:N0}", _addedPoint < 0 ? '-': '+', Mathf.Abs(_addedPoint));


		// rank Anim
		if( _addedRank > 0 )
		{
			spRankUpDown.color = Color.red;
			spRankUpDown.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		}
		else if( _addedRank == 0)
		{
			spRankUpDown.gameObject.SetActive(false);
		}

		time = 1f;
		acc = Time.deltaTime;
		while(acc < time)
		{
			var value = (int)(_addedRank * (acc / time));
			lbRank.text = string.Format("{0:N0}위", cachedRank + value);
			spRankUpDown.alpha = acc / time;

			yield return null;
			acc += Time.deltaTime;
		}

		lbRank.text = string.Format("{0:N0}위", cachedRank + _addedRank);
		spRankUpDown.alpha = 1f;
	}


	IEnumerator CoPlacementAnim(int _nowGrade)
	{
		// Unit Card Anim
		var unitCardTws = new UITweener[arrChar.Length];
		for (int i = 0; i < unitCardTws.Length; ++i)
		{
			unitCardTws[i] = arrChar[i].GetComponent<UITweener>();
			unitCardTws[i].ResetToBeginning();
			unitCardTws[i].PlayForward();
		}

		while (true)
		{
			yield return null;

			for (int i = 0; i < unitCardTws.Length; ++i)
				if (unitCardTws[i].enabled == true)
					continue;

			break;
		}

		if (_nowGrade == cachedGrade)
			yield break;


		float time = 1f;
		float acc = Time.deltaTime;
		while (acc < time)
		{
			var value = 1f - acc / time;
			spRankIcon.alpha = value;
			//spNextRankIcon.alpha = value;
			lbNoNextGuide.alpha = value;

			yield return null;
			acc += Time.deltaTime;
		}

		spRankIcon.alpha = 0f;
		//spNextRankIcon.alpha = 0f;
		lbNoNextGuide.alpha = 0f;
		//spRankIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(_nowGrade);
		//if (_nowGrade + 1 < 7000009)
		//	spNextRankIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(_nowGrade + 1);
		//else
		//{
		//	spNextRankIcon.gameObject.SetActive(false);
		//	lbNoNextGuide.gameObject.SetActive(true);
		//}

		if (_nowGrade + 1 < 7000009)
			spRankIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(_nowGrade + 1);
		else
		{
			spRankIcon.gameObject.SetActive(false);
			lbNoNextGuide.gameObject.SetActive(true);
		}
		Vector3 sPos = spNextRankIcon.transform.position;
		Vector3 dPos = spRankIcon.transform.position;
		spRankIcon.transform.position = spNextRankIcon.transform.position;

		time = 0.5f;
		acc = Time.deltaTime;
		while (acc < time)
		{
			var value = acc / time;
			spNextRankIcon.transform.position = Vector3.Lerp(sPos, dPos, value * value);
			yield return null;
			acc += Time.deltaTime;
		}
		spNextRankIcon.transform.position = dPos;

		time = 0.5f;
		acc = Time.deltaTime;
		while (acc < time)
		{
			var value = acc / time;
			spRankIcon.alpha = value * value;
			lbNoNextGuide.alpha = value * value;
			yield return null;
			acc += Time.deltaTime;
		}
		spRankIcon.alpha = 1f;


		//time = 0.5f;
		//acc = Time.deltaTime;
		//while (acc < time)
		//{
		//	spRankIcon.alpha = acc / time;
		//	yield return null;
		//	acc += Time.deltaTime;
		//}
		//spRankIcon.alpha = 1f;

		//time = 0.5f;
		//acc = Time.deltaTime;
		//while (acc < time)
		//{
		//	spNextRankIcon.alpha = acc / time;
		//	lbNoNextGuide.alpha = acc / time;
		//	yield return null;
		//	acc += Time.deltaTime;
		//}
		//spNextRankIcon.alpha = 1f;
		//lbNoNextGuide.alpha = 1f;
	}



	public void OnClickPvPLobby()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

		if(from == From.Revenge)
			GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, null);
		else if (from == From.Placement)
			GameCore.Instance.ChangeSubSystem(SubSysType.PvPGradeTest, null);
		else
			GameCore.Instance.ChangeSubSystem(SubSysType.PvPReady, null);
	}

	public void OnClickNextPvPBattle()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        if (from == From.Placement)
            GameCore.Instance.ChangeSubSystem(SubSysType.PvPGradeTest, null);
        else
            GameCore.Instance.ChangeSubSystem(SubSysType.PvPMatch, null);
	}

	public void OnClickReturnHome()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
	}
}
