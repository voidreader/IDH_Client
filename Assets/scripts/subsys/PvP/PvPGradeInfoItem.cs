using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvPGradeInfoItem : MonoBehaviour
{
	public UISprite spGradeIcon;
	public UILabel lbGradeName;
	public UILabel lbPoint;

	public UILabel lbPerlReward;
	public UILabel lbGoldReward;

	public GameObject goNowPosSign;
	public GameObject goNextGradeSign;

	public void SetData(int _grade, float _avg)
	{
		var data = GameCore.Instance.DataMgr.GetPvPRateRewardData(_grade);
		if (data == null)
		{
			Debug.LogError(" Not Exist Data!");
			return;
		}

		spGradeIcon.spriteName = UIPvPMatch.GetGradeSmallSprite(_grade);
        //RankEffectManager.CreatePVP(_grade, spGradeIcon.transform);
		lbGradeName.text = data.name;
		lbPoint.text = _avg.ToString("N0");
		if (_avg % 1 != 0)
			lbPoint.text += "." + ((_avg % 1) * 100).ToString("00");
		lbPerlReward.text = data.perl.ToString("N0");
		lbGoldReward.text = data.gold.ToString("N0");

		goNowPosSign.SetActive(GameCore.Instance.PlayerDataMgr.PvPData.grade == _grade);

        if (_grade + 1 >= 7000010)
        {
            goNextGradeSign.SetActive(false);
        }
        else
        {
            var nextData = GameCore.Instance.DataMgr.GetPvPRateRewardData(_grade + 1);
            goNextGradeSign.SetActive(nextData != null);
        }
	}
}
