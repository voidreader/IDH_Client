using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class UIGradeComponent : UIBase
{
    public stPvPGrade gradeInfo;
    public enum eGradeBalloon { none = 0, challenge, completed, complete }

    static int static_Depth = 200;


    public UISprite rankIcon;
    public UILabel rankLbl;
    public GameObject balloon_complete;
    public GameObject balloon_challenge;

    public override void Initialize()
	{
		base.Initialize();
	}

    public void Init(stPvPGrade gradeInfo)
    {
        this.gradeInfo = gradeInfo;

        rankIcon.spriteName = GetGrade(gradeInfo.icon);
        RankEffectManager.CreateGradeTest(gradeInfo.icon, rankIcon.transform);
        rankLbl.text = PvPAdvancementDataMap.GetStrRank(gradeInfo.icon);
	}

    public void Init(int idx, eGradeBalloon balloonType = eGradeBalloon.none)
    {
        rankIcon.spriteName = GetGrade(idx);
        if (balloonType != eGradeBalloon.none)
            RankEffectManager.CreateGradeTest(idx, rankIcon.transform);
        rankLbl.text = PvPAdvancementDataMap.GetStrRank(idx);

				balloon_complete.SetActive(balloonType == eGradeBalloon.complete);
				balloon_challenge.SetActive(balloonType == eGradeBalloon.challenge);

				if(balloonType < eGradeBalloon.completed)
					rankLbl.transform.parent.GetComponent<UISprite>().color = new Color32(0x30, 0x30, 0x30, 0xFF);
				if( balloonType == eGradeBalloon.none)
					rankIcon.color = Color.gray;


	}

	public string GetCharIcon( int nIcon)
    {
        string strCharIcon = null;
        if (nIcon == 1)
        {
            strCharIcon = "icon_s_naga";
        }
        else if (nIcon == 2)
        {
            strCharIcon = "icon_s_hyena";
        }
        else if (nIcon == 3)
        {
            strCharIcon = "icon_s_guinueng";
        }
        else if (nIcon == 4)
        {
            strCharIcon = "icon_s_dana";
        }
        else if (nIcon == 5)
        {
            strCharIcon = "icon_s_lady";
        }
        else if (nIcon == 6)
        {
            strCharIcon = "icon_s_eunbidan";
        }
        else
            strCharIcon = "icon_s_sasa";


        return strCharIcon;
    }

    public string GetGrade(int nGrade)
    {
        string strRank = null;

        if (nGrade == 0)
        {
            strRank = "LV_01_BRONS_S";
        }
        else if (nGrade == 1)
        {
            strRank = "LV_02_BRONS_S";
        }
        else if (nGrade == 2)
        {
            strRank = "LV_03_SILVER_S";
        }
        else if (nGrade == 3)
        {
            strRank = "LV_04_SILVER_S";
        }
        else if (nGrade == 4)
        {
            strRank = "LV_05_GOLD_S";
        }
        else if (nGrade == 5)
        {
            strRank = "LV_06_GOLD_S";
        }
        else if (nGrade == 6)
        {
            strRank = "LV_07_PLATINUM_S";
        }
        else if (nGrade == 7)
        {
            strRank = "LV_08_MASTER_S";
        }
        else if (nGrade == 8)
        {
            strRank = "LV_09_CHAMPION_S";
        }

        return strRank;
    }

    public override void Open()
	{
		base.Open();
	}

	public override void Close()
	{
		base.Close();
	}

    public void OnClickStainComponent()
    {
        Debug.LogError("OnClickStainComponent()");
        /*
        if (string.IsNullOrEmpty(stainInfo.startTime) && string.IsNullOrEmpty(stainInfo.endTime))
            GameCore.Instance.NetMgr.Req_MyRoomStartCleanStain(stainInfo);
        else
        {
            if( IsUpgradeComplete)
                GameCore.Instance.NetMgr.Req_MyRoomEndCleanStain(stainInfo);
        }
        */
        //SceneMyRoom.Inst.OpenMyRoom(nIdx);
    }

    public void SetMessage(string strMsg)
	{
		Open();
	}

    public void SetMessage(int nIdx, object arg)
    {
        //m_txtMessage.text = string.Format(TableManager.GetString(nIdx), arg);
        Open();
    }

}

