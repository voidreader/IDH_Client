using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class UIRankComponent : UIBase
{
    PvPGroupRankSData rankInfo;

	public static string printTeamUserName;

    [SerializeField] UITweener m_Tween = null;
	[SerializeField] UILabel m_txtMessage = null;

    public GameObject foreObject;
    public GameObject timer;
    public float fUpgradeTime;
    public float fStartTime;
    public bool IsUpgradeComplete;

    static int static_Depth = 200;

    public UISprite whiteBgSprite = null;
    public UISprite blackBgSprite = null;
    public UISprite gageSprite = null;
    public UILabel timerLbl;

    /*
        public int level;
    public string name;
    public int icon;
    public int rank;
    public int point;
    public int power;
    */

    public UILabel nameLbl;
    public UISprite charIcon;
    public UISprite rankIcon;
    public UILabel rankLbl;
    public UILabel pointLbl;
    public UILabel powerLbl;
	public UISprite spBG;
	public GameObject playerSigin;
	public GameObject teamIcon;
	bool enableClick;

		public override void Initialize()
	{
		base.Initialize();
	}

 //   public void Init(stRank rankInfo)
 //   {
 //       this.rankInfo = rankInfo;

 //       nameLbl.text = string.Format("LV.{0} {1}", rankInfo.level, rankInfo.name);
 //       charIcon.spriteName = GetCharIcon(rankInfo.icon);
 //       rankIcon.spriteName = GetRank(rankInfo.rank);
 //       rankLbl.text = string.Format("{0:N0}위", rankInfo.rank.ToString());
 //       pointLbl.text = rankInfo.point.ToString();
	//			powerLbl.text = string.Format("팀 전투력: {0:N0}", rankInfo.power);
	//}

	internal void Init(PvPGroupRankSData rankInfo, bool _enableClick = true)
	{
		this.rankInfo = rankInfo;
		enableClick = _enableClick;
		nameLbl.text = string.Format("LV.{0} {1}", rankInfo.USER_LEVEL, rankInfo.USER_NAME);
		var unitData = GameCore.Instance.DataMgr.GetUnitData(rankInfo.DELEGATE_ICON);
		if (unitData != null)
			GameCore.Instance.SetUISprite(charIcon, UnitDataMap.GetSmallProfileSpriteKey(unitData.prefabId));
		else
			charIcon.spriteName = "";

		rankIcon.spriteName = UIPvPMatch.GetGradeBigSprite(rankInfo.GRADE);
		rankLbl.text = string.Format("{0:N0}위", rankInfo.RANK);
		pointLbl.text = string.Format("{0:N0}", rankInfo.POINT);
		powerLbl.text = string.Format("팀 전투력: {0:N0}", rankInfo.POWER);

		if(enableClick == false)
			teamIcon.SetActive(false);

		if (rankInfo.USER_UID.Equals(GameCore.Instance.PlayerDataMgr.PvPData.userUID))
		{
			spBG.spriteName = "BTN_02_01_01";
			playerSigin.SetActive(true);
		}
	}

	public void OnClickComponent()
	{
		if (enableClick)
		{
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Enemy_Info);
            GameCore.Instance.NetMgr.Req_PvPTeamInfo(rankInfo.USER_UID);
			printTeamUserName = rankInfo.USER_NAME;
        }
	}

	public override void Open()
	{
		base.Open();

		m_Tween.ResetToBeginning();
		m_Tween.PlayForward();
	}

	public override void Close()
	{
		base.Close();
	}

    public void SetMessage(string strMsg)
	{
		m_txtMessage.text = strMsg;

		Open();
	}

    public void SetMessage(int nIdx, object arg)
    {
        //m_txtMessage.text = string.Format(TableManager.GetString(nIdx), arg);
        Open();
    }

    public void LateUpdate()
    {
        if (timer && timer.activeInHierarchy)
        {
            // 게이지 뒤로감
            //timeSlider.value = (parentShop.fStartTime - (parentShop.fStartTime - parentShop.fUpgradeTime ))/ parentShop.fStartTime;
            if (fUpgradeTime > 0)
            {
                fUpgradeTime -= Time.deltaTime;
                if (fUpgradeTime <= 0)
                {
                    // 시간 strremainedtime에 넣기
                    fUpgradeTime = 0;
                    IsUpgradeComplete = true;
                    timer.SetActive(false);
                }
                else
                {
                    float fVal = 1 - ((fStartTime - (fStartTime - fUpgradeTime)) / fStartTime);
                    timer.GetComponent<UISlider>().value = fVal;
                    timerLbl.text = UtilityFunc.Inst.ConvertToTime(fUpgradeTime);
                }

            }
        }
    }
}

