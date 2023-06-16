using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvPPlayerRankInfo : MonoBehaviour
{
	static string[] strState = new string[]
	{	"승급", "잔류", "강등" };

	public UISprite spRankIcon;
	public UILabel lbRank;
	public UILabel lbLeagueHead;
	public UILabel lbRewardHead;
	public UILabel lbRewardPerl;
	public UILabel lbRewardGold;

	public UIGrid grLeagueAdvancement;
	public GameObject pfLeagueAdvancement;

	public void SetData(int _grade, int _groupRank)
	{
		spRankIcon.spriteName = UIPvPMatch.GetGradeBigSprite(_grade);
        RankEffectManager.CreatePVP(_grade, spRankIcon.transform);

		var strRank = GameCore.Instance.DataMgr.GetPvPRateRewardData(_grade);
		lbRank.text = strRank.name;
		lbRewardHead.text = strRank.name + " 보상";
		lbRewardPerl.text = strRank.perl.ToString("N0");
		lbRewardGold.text = strRank.gold.ToString("N0");

		if (_grade == 7000009)
		{
			_grade = 7000008;
			strRank = GameCore.Instance.DataMgr.GetPvPRateRewardData(_grade);
		}
		lbLeagueHead.text = strRank.name + " 리그";
		SetAdvancement(_grade, _groupRank);
	}


	private void SetAdvancement(int _grade, int _groupRank)
	{
		var advancementList = GameCore.Instance.DataMgr.GetPvPAdvancementFindMapData(_grade);
		int lastLimit = 1;

		for (int i = 0; i < advancementList.Count; ++i)
		{
			var data = advancementList[i];
			var tf = Instantiate(pfLeagueAdvancement, grLeagueAdvancement.transform).transform;
			for (int j = 0; j < tf.childCount; j++)
			{
				var child = tf.GetChild(j);
				switch (child.name)
				{
					case "rank":
						child.GetComponent<UILabel>().text =
							string.Format("{0:00} ~ {1:00}위", lastLimit, data.rankLimit);
						break;

					case "state":
						var stateIdx = data.rateType.CompareTo(data.rateAdvance) + 1;
						if (stateIdx == 1)
						{
							child.GetComponent<UILabel>().text =
								string.Format("{0:00}", strState[stateIdx]);
						}
						else
						{
							var strRank = GameCore.Instance.DataMgr.GetPvPRateRewardData(data.rateAdvance);
							child.GetComponent<UILabel>().text =
								string.Format("{0:00} {1:00}", strRank.name, strState[stateIdx]);
						}
						break;

					case "arrow":
						if (lastLimit <= _groupRank && _groupRank <= data.rankLimit)
							child.gameObject.SetActive(true);
						else
							child.gameObject.SetActive(false);
						break;

					default:
						break;
				}
			}
			lastLimit = data.rankLimit + 1;
		}

		grLeagueAdvancement.enabled = true;
	}

}
