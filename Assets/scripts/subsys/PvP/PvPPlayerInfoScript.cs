using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvPPlayerInfoScript : MonoBehaviour
{
	public UILabel lbLevel;
	public UILabel lbName;
	public UISprite spIcon;
	public UILabel lbTierPoint;
	public UILabel lbGroupRank;
	public UILabel lbPower;
	public UILabel lbScore;

	public UISprite spSkillIcon;
	public UILabel lbSkillName;

	internal void SetData(UIPvPMatch.PVPInfoTarget _opp, PvPSData _data, int _rank, int _groupRank)
	{
		lbLevel.text = "LV." + _data.userLevel;

		lbName.text = _data.userName;
        if(_data.userName == null) lbName.text = "악의 조직 나이프";

		if(_data.typicalKey <= 0)
		{
			GameCore.Instance.SetUISprite(spIcon, UnitDataMap.GetBigProfileSpriteKey(1));
		}
		else
		{
			var unitData = GameCore.Instance.DataMgr.GetUnitData(_data.typicalKey);
			GameCore.Instance.SetUISprite(spIcon, UnitDataMap.GetBigProfileSpriteKey(unitData.prefabId));
		}
		

		string rakeName = "-";
		if (_data.grade != 0)
			rakeName = GameCore.Instance.DataMgr.GetPvPRateRewardData(_data.grade).name;

		if (!_data.placement)
		{
			lbTierPoint.text = rakeName + " / - 점";
			lbGroupRank.text = "- 위 / - 위";
			lbPower.text = _data.power.ToString("N0");
			if (_opp == UIPvPMatch.PVPInfoTarget.Player)
				lbScore.text = "배치중";
			else
				lbScore.text = "- 승 - 패/ - 연승";
		}
		else
		{
			lbTierPoint.text = rakeName + " /" + _data.point.ToString("N0") + "점";
			lbGroupRank.text = _groupRank.ToString("N0") + "위 /" + _rank.ToString("N0") + "위" ;
			lbPower.text = _data.power.ToString("N0");
			lbScore.text = _data.win + "승 " + _data.defeat+"패/" + _data.consecutive+"연승";
		}
	}

	internal void SetSkillData(int _skillKey)
	{
		TeamSkillDataMap data = null;
		if (0 < _skillKey)
			data = GameCore.Instance.DataMgr.GetTeamSkillData(_skillKey);

		if (data != null)
		{
			GameCore.Instance.SetUISprite(spSkillIcon, data.imageID);
			lbSkillName.text = data.name;
		}
		else
		{
			GameCore.Instance.SetUISprite(spSkillIcon, CommonType.SP_TEAMSKILL_EMPTY);
			lbSkillName.text = "";
		}
	}
}
