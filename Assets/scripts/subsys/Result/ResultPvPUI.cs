using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ResultPvPUI : ResultUIBase
{
    public enum From
    {
        PvP,
        Placement,
        Revenge
    }

    internal static ResultPvPUI Create(Transform _parent)
    {
        //var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Result/PanelResultPvPUI", _parent);
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Result/UIPvPResult", _parent);
        var result = go.GetComponent<ResultPvPUI>();
        return result;
    }
    internal override void TurnOffNextButton()
    {
    }

    [Header("Main")]
    [SerializeField]
    UISprite resultSprite;
    [SerializeField]
    GameObject resultSpriteVictory;
    [SerializeField]
    GameObject resultSpriteDefeated;
    [SerializeField]
    GameObject[] arrChar;

    [Space]
    [Header("Result")]
    [SerializeField]
    UILabel lbLeagueGuide;
    [SerializeField]
    UILabel lbPointGuide;
    [SerializeField]
    GameObject resultRoot;

    [Header(" - Not PlaceMent")]
    [SerializeField]
    GameObject noPlacementRoot;
    [SerializeField]
    UILabel lbRank;
    //[SerializeField]
    //UISprite spRankUpDown;
    [SerializeField]
    GameObject spRankUp;
    [SerializeField]
    GameObject spRankDown;
    [SerializeField]
    UILabel lbAddedPoint;
    [SerializeField]
    UILabel lbPoint;
    int cachedGrade;

    [Header(" - PlaceMent")]
    [SerializeField]
    GameObject placementRoot;
    [SerializeField]
    UISprite spRankIcon;
    [SerializeField]
    UISprite spNextRankIcon;
    [SerializeField]
    UILabel lbNoNextGuide;
    int cachedRank;
    int cachedPoint;


    [SerializeField]
    GameObject btNextBabblt;

    From from;
    //[Header(" - Raid")]
    //[SerializeField] GameObject raidRoot;
    //[SerializeField] RaidRewardGradeComponent raidGrade;
    //[SerializeField] UILabel lbTotalDmg;
    //[SerializeField] UILabel lbAddedDmg;

    internal override void Init(ResultBasePara _para)
    {
        base.Init(_para);
        var pr = _para.GetPara<ResultPvPPara>();
        var revenge = pr.historyUID > 0;

        // Set Player Info
        SetPlayerUnitByTeamIndex(para.playerTeamIdx);

        //// Set Win or Lose Text Icon
        //if (para.clear) resultSprite.spriteName = "P_TEXT_VICTORY";
        //else resultSprite.spriteName = "P_TEXT_DEFEATED";

        if (para.clear) resultSpriteVictory.SetActive(true);
        else resultSpriteDefeated.SetActive(true);

        // Set Result Data
        cachedGrade = GameCore.Instance.PlayerDataMgr.PvPData.grade;

        from = pr.from;
        switch (from)
        {
            case From.Placement:
                resultRoot.SetActive(true);
                noPlacementRoot.SetActive(false);
                placementRoot.SetActive(true);
                lbLeagueGuide.text = "달성 등급";
                lbPointGuide.text = "도전 등급";
                goCheckOutBtn.GetComponentInChildren<UILabel>().text = "로비";

                spRankIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(cachedGrade);
                if (cachedGrade + 1 < 7000009)
                {
                    spNextRankIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(cachedGrade + 1);
                    RankEffectManager.CreatePVP(cachedGrade + 1, spNextRankIcon.transform);
                }
                    
                else
                {
                    spNextRankIcon.gameObject.SetActive(false);
                    lbNoNextGuide.gameObject.SetActive(true);
                }
                //goNextBtn.SetActive(pr.clear);
                break;

            case From.PvP:
                resultRoot.SetActive(true);
                noPlacementRoot.SetActive(true);
                placementRoot.SetActive(false);
                var data = GameCore.Instance.DataMgr.GetPvPRateRewardData(cachedGrade);
                lbLeagueGuide.text = string.Format("{0} 리그", data.name);
                lbPointGuide.text = "포인트";
                goCheckOutBtn.GetComponentInChildren<UILabel>().text = "로비";
                //goNextBtn.SetActive(pr.clear);
                cachedRank = GameCore.Instance.PlayerDataMgr.PvPGroupRank;
                cachedPoint = GameCore.Instance.PlayerDataMgr.PvPData.point;
                lbRank.text = string.Format("{0:N0}위", cachedRank);
                lbPoint.text = string.Format("{0:N0}", cachedPoint);

                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_PVP_Result);
                break;

            case From.Revenge:
                resultRoot.SetActive(false);
                goNextBtn.SetActive(false);
                goCheckOutBtn.GetComponentInChildren<UILabel>().text = "숙소";
                return;

            default:
                break;
        }

    }


    internal override void UpdateData(params object[] _params)
    {
        var pr = para.GetPara<ResultPvPPara>();
        switch (pr.from)
        {
            case From.PvP:
                UpdateResult((int)_params[0], (int)_params[1]);
                break;

            case From.Placement:
                UpdatePlacement(GameCore.Instance.PlayerDataMgr.PvPData.grade);
                break;

            case From.Revenge:
                //PlayerUnitPSirtes();
                break;

            default:
                break;
        }
    }

    internal override void SetRewardItem(CardSData[] _sdatas)
    {
        // Never call
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

                UtilityFunc.Inst.GetChildObj("bg", arrChar[arrIdx]).GetComponent<UISprite>().GrayScale(!para.clear);
                UtilityFunc.Inst.GetChildObj("fore", arrChar[arrIdx]).GetComponent<UI2DSprite>().GrayScale(!para.clear);
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
        //var unitCardTws = new UITweener[arrChar.Length];
        //for (int i = 0; i < unitCardTws.Length; ++i)
        //{
        //    unitCardTws[i] = arrChar[i].GetComponent<UITweener>();
        //    unitCardTws[i].ResetToBeginning();
        //    unitCardTws[i].PlayForward();
        //}

        //while (true)
        //{
        //    yield return null;

        //    for (int i = 0; i < unitCardTws.Length; ++i)
        //        if (unitCardTws[i].enabled == true)
        //            continue;

        //    break;
        //}

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
        lbAddedPoint.text = string.Format("{0} {1:N0}", _addedPoint < 0 ? '-' : '+', Mathf.Abs(_addedPoint));
        
        // rank Anim
        if (_addedRank < 0) spRankUp.SetActive(true);

        else if (_addedRank > 0) spRankDown.SetActive(true);
        //{
        //    spRankUpDown.color = Color.red;
        //    spRankUpDown.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        //}
        //else if (_addedRank == 0)
        //{
        //    spRankUpDown.gameObject.SetActive(false);
        //}

        time = 1f;
        acc = Time.deltaTime;
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_PVP_Rank_UP);
        while (acc < time)
        {
            var value = (int)(_addedRank * (acc / time));
            lbRank.text = string.Format("{0:N0}위", cachedRank + value);
            //spRankUpDown.alpha = acc / time;

            yield return null;
            acc += Time.deltaTime;
        }

        lbRank.text = string.Format("{0:N0}위", cachedRank + _addedRank);
        //spRankUpDown.alpha = 1f;
    }

    IEnumerator CoPlacementAnim(int _nowGrade)
    {
        // Unit Card Anim
        //var unitCardTws = new UITweener[arrChar.Length];
        //for (int i = 0; i < unitCardTws.Length; ++i)
        //{
        //    unitCardTws[i] = arrChar[i].GetComponent<UITweener>();
        //    unitCardTws[i].ResetToBeginning();
        //    unitCardTws[i].PlayForward();
        //}

        //while (true)
        //{
        //    yield return null;

        //    for (int i = 0; i < unitCardTws.Length; ++i)
        //        if (unitCardTws[i].enabled == true)
        //            continue;

        //    break;
        //}

        if (_nowGrade == cachedGrade)
            yield break;


        float time = 1f;
        float acc = Time.deltaTime;
        while (acc < time)
        {
            var value = 1f - acc / time;
            spRankIcon.alpha = value;
            lbNoNextGuide.alpha = value;

            yield return null;
            acc += Time.deltaTime;
        }

        spRankIcon.alpha = 0f;
        lbNoNextGuide.alpha = 0f;

        if (_nowGrade + 1 < 7000009)
        {
            spRankIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(_nowGrade + 1);
            RankEffectManager.CreatePVP(_nowGrade + 1, spRankIcon.transform);
        }
            
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
    }

    public void OnClickPvPLobby()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        var pr = para.GetPara<ResultPvPPara>();
        switch (pr.from)
        {
            case From.PvP:
                GameCore.Instance.ChangeSubSystem(SubSysType.PvPReady, null);
                break;

            case From.Placement:
                GameCore.Instance.ChangeSubSystem(SubSysType.PvPGradeTest, null);
                break;

            case From.Revenge:
                GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, null);
                break;

            default:
                break;
        }
    }

    public void OnClickNextPvPBattle()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        if (GameCore.Instance.PlayerDataMgr.PvPData.rechallengeCount == 0 &&
            !GameCore.Instance.PlayerDataMgr.PvPData.placement)
        {
            GameCore.Instance.ChangeSubSystem(SubSysType.PvPReady, null);
        }
        else
            GameCore.Instance.ChangeSubSystem(SubSysType.PvPMatch, null);
    }

    public void OnClickReturnHome()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);

    }

    private void Update()
    {
        if (GameCore.Instance.PlayerDataMgr.PvPData.grade == 7000008 &&
            !GameCore.Instance.PlayerDataMgr.PvPData.placement)
            btNextBabblt.transform.GetChild(0).GetComponent<UILabel>().text = "종료";
        else
            btNextBabblt.transform.GetChild(0).GetComponent<UILabel>().text = "다음 전투";
    }
}
