using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 스토리모드 전투시스템 UI
/// </summary>
public class BattleUI : BattleUIBase , ISequenceTransform
{
    public static BattleUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/PanelBattleUI", _parent);
        return go.GetComponent<BattleUI>();
    }


    GameObject progressBar;
    UISlider battleProgress;
    UILabel timeLabel;

    
    protected override void InitInternal(PvPSData _oppenetData = null)
    {
        progressBar = UnityCommonFunc.GetGameObjectByName(gameObject, "progressBar");
        battleProgress = UnityCommonFunc.GetComponentByName<UISlider>(progressBar, "progressBar");
        timeLabel = UnityCommonFunc.GetComponentByName<UILabel>(progressBar, "timer");
        var stageNum = UnityCommonFunc.GetComponentByName<UILabel>(progressBar, "stageNum");

        if (GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.DailyBattle)
        {
            stageNum.text = "DUNGEON";
        }
        else if (GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.Battle)
        {
            var sys = GameCore.Instance.SubsysMgr.GetNowSubSys() as BattleSysBase;
            var data = GameCore.Instance.DataMgr.GetStoryData(sys.Para.stageId);
            stageNum.text = data.chapter + "-" + data.stage;
            switch (data.difficult)
            {
                case 1: stageNum.color = Color.white; break;
                case 2: stageNum.color = new Color32(255, 192, 0, 255); break;
                case 3: stageNum.color = new Color32(255, 126, 0, 255); break;
            }
        }

        progressBar.SetActive(true);

        if (GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning)
        {
            SetAutoButton(false);
            btAuto.gameObject.SetActive(false);
        }

        //roundText = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "RoundText");
        //roundTextTweens = roundText.gameObject.GetComponentsInChildren<UITweener>();
        //roundTextTweens[0].onFinished.Add(new EventDelegate(() => { roundText.gameObject.SetActive(false); isAnimationing = false; }));
        roundText.gameObject.SetActive(false);
    }

    internal override void SetProgress(float _value)
    {
        if (battleProgress != null)
            battleProgress.value = _value;
    }

    internal override void ShowRoundText(int _round)
    {
        isAnimationing = true;
        roundNumTextSprite[_round - 1].SetActive(true);
        UnityCommonFunc.GetGameObjectByName(roundText, "Round").transform.localScale = Vector3.one * 0.01f;
        roundText.gameObject.SetActive(true);
        float length = roundText.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length + 1f;
        StartCoroutine(GameCore.WaitForTime(length, () =>
        {
            roundNumTextSprite[_round - 1].SetActive(false);
            roundText.gameObject.SetActive(false);
            isAnimationing = false;
        }));
    }

    protected override void UpdateTime()
    {
        /*lbRaidTime.text = */timeLabel.text = ((int)timeValue / 60) + ":" + ((int)timeValue % 60).ToString("00");
    }

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 1:
                {
                    ReturnTutorialData returnTutorialData = new ReturnTutorialData(unitCards[2].gameObject.transform, 0);
                    nTutorialList.Add(returnTutorialData);
                    break;
                }
            case 4:
                {
                    nTutorialList.Add(new ReturnTutorialData(teamBt_Root.transform, 0));
                    break;
                }
            case 5:
                {
                    nTutorialList.Add(new ReturnTutorialData(strikeBt_Root.transform, 0));
                    break;
                }
            default:
                break;
        }
        return nTutorialList;
    }
}
