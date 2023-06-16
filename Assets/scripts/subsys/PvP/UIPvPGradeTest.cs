using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class UIPvPGradeTest : /*MonoBehaviour*/UIBase
{
    [SerializeField]
    UIGrid gradeTable;

    [SerializeField]
    GameObject gradeComponent;

    [SerializeField]
    GameObject userGradeComponent;

    [SerializeField]
    UISlider gradeSlider;

    [SerializeField]
    UILabel rechallengeCountLbl;

    [SerializeField]
    GameObject gradeSliderEffect;

    GameObject bottomRoot;

    internal void Init(EventDelegate.Callback _cbSelectStage, EventDelegate.Callback _cbBack, stRank userRank, List<stRank> rankList)
    {
        //UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "Stage1Button").onClick.Add(new EventDelegate(_cbSelectStage));
        //UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "BattleStartButton").onClick.Add(new EventDelegate(_cbSelectStage));
        //UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "Back").onClick.Add(new EventDelegate(_cbBack));

        InitializeRankList(userRank, rankList);
        if (GameCore.Instance.PlayerDataMgr.PvPData.grade == 7000008)
        {
            bottomRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "BottomRoot");
            bottomRoot.gameObject.transform.GetChild(1).GetChild(0).GetComponent<UILabel>().text = "완료";
            bottomRoot.gameObject.transform.GetChild(3).gameObject.SetActive(false);
            bottomRoot.gameObject.transform.GetChild(4).GetComponent<UILabel>().text = "배치고사가 종료되었습니다.";
            bottomRoot.gameObject.transform.GetChild(4).transform.localPosition += new Vector3(197, 0, 0);
            bottomRoot.gameObject.transform.GetChild(4).GetComponent<UILabel>().pivot = UIWidget.Pivot.Right;
        }
    }

    //bool bRankListBtn;
    public void InitializeRankList(stRank userRank, List<stRank> rankList)
    {
        //bLodgingBtn = !bLodgingBtn;

        if (gradeTable == null)
        {
            GameObject ItemList = UtilityFunc.Inst.GetChildObj("CenterGrade", gameObject);
            gradeTable = UtilityFunc.Inst.GetChildObj("Left_B", ItemList).GetComponent<UIGrid>();
        }

        Transform[] tms = gradeTable.GetComponentsInChildren<Transform>(true);
        foreach (Transform tm in tms)
        {
            if (tm != gradeTable.transform)
                GameObject.Destroy(tm.gameObject);
        }
        //userGradeComponent.GetComponent<UIGradeComponent>().Init(userRank);
        rechallengeCountLbl.text = "도전 기회 " + "[00ff00]" + GameCore.Instance.PlayerDataMgr.PvPData.rechallengeCount + "[-]" + " 회";

        var gradeIdx = (GameCore.Instance.PlayerDataMgr.PvPData.grade % 100) - 1;
        if (!GameCore.Instance.PlayerDataMgr.PvPData.placement)
        {
            gradeSlider.value = ((gradeIdx + 1) * 0.142857f);
            if (gradeIdx < 7)
            {
                gradeSliderEffect.transform.localPosition = new Vector3(gradeIdx * 165f + 45f, 0f, 0f);
            }
            else
            {
                gradeSliderEffect.SetActive(false);
            }
        }
        else
        {
            gradeSliderEffect.SetActive(false);
            gradeSlider.value = 0;
        }
            

        for (int i = 0; i < /*rankList.Count*/8; i++)
        {
            //if (((ShopObject)baseObject).ObjectInfos.nObjectLevel < levelItemList[i]._bulding_lv || levelItemList[i]._bulding_lv == 0)
            //    continue;

            GameObject goPage = NGUITools.AddChild(gradeTable.gameObject, gradeComponent);
            goPage.transform.localScale = new Vector3(1, 1, 1);

            if (gradeIdx == i)
                goPage.GetComponent<UIGradeComponent>().Init(i, UIGradeComponent.eGradeBalloon.complete);
            else if ((gradeIdx + 1) == i && GameCore.Instance.PlayerDataMgr.PvPData.rechallengeCount > 0)
                goPage.GetComponent<UIGradeComponent>().Init(i, UIGradeComponent.eGradeBalloon.challenge);
            else
                goPage.GetComponent<UIGradeComponent>().Init(i, gradeIdx < i ? UIGradeComponent.eGradeBalloon.none : UIGradeComponent.eGradeBalloon.completed);
        }

        gradeTable.enabled = true;
        gradeTable.Reposition();

        tms = gradeTable.GetComponentsInChildren<Transform>(true);
        foreach (Transform tm in tms)
        {
            if (tm != gradeTable.transform)
            {
                if (tm.GetComponent<UIGradeComponent>())
                    tm.localPosition = new Vector3(tm.localPosition.x, 56, tm.localPosition.z);
            }
        }

    }

    public void OnClickGradeTestReady()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        //GameCore.Instance.ChangeSubSystem(SubSysType.PvPReady, null);
        if (GameCore.Instance.PlayerDataMgr.PvPData.grade == 7000008)
        {// 마스터일떄도 종료
            QuitGradeTest();
        }
        else if (GameCore.Instance.PlayerDataMgr.PvPData.rechallengeCount > 0)
        {

            GameCore.Instance.ChangeSubSystem(SubSysType.PvPMatch, null);
        }

        else
            QuitGradeTest();
    }

    public void OnClickQuitGradeTest()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        if (GameCore.Instance.PlayerDataMgr.PvPData.rechallengeCount >= 0)
        {
            var gradeText = GameCore.Instance.DataMgr.GetPvPRateRewardData(GameCore.Instance.PlayerDataMgr.PvPData.grade);
            GameCore.Instance.ShowAgree("배치 고사 종료", "배치고사를 즉시 종료하고 해당 등급으로 이동합니다.\n계속하시겠습니까?\n\n" + "[C][7200FF]현재등급 : " + gradeText.name + "[-][/C]", 0,
                                                                    new EventDelegate.Callback(() => { GameCore.Instance.CloseMsgWindow(); QuitGradeTest(); }));
        }
        else
            QuitGradeTest();
    }

    public void QuitGradeTest()
    {
        GameCore.Instance.ChangeSubSystem(SubSysType.PvPReady, null);
    }
}
