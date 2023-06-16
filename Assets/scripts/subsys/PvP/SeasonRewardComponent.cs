using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonRewardComponent : MonoBehaviour
{
	public UISprite spRankIcon;
	public UILabel lbRank;
	public UILabel lbRankLeague;
	public UILabel lbRate;
	public UILabel lbRewardPerl;
	public UILabel lbRewardGold;
    public UILabel lbGuide;

    public bool bAnimationing;

	internal void SetData(List<PvPGroupRankSData> _seasonList, long _userUID)
	{
		for (int i = 0; i < _seasonList.Count; ++i)
		{
			if(_seasonList[i].USER_UID.Equals(_userUID))
			{
				var data = GameCore.Instance.DataMgr.GetPvPRateRewardData(_seasonList[i].GRADE);
				spRankIcon.spriteName = UIPvPMatch.GetGradeBigSprite(_seasonList[i].GRADE);
                RankEffectManager.CreatePVP(_seasonList[i].GRADE, spRankIcon.transform);
				lbRank.text = data.name;
				lbRewardPerl.text = data.perl.ToString("N0");
				lbRewardGold.text = data.gold.ToString("N0");

				var prevData = GameCore.Instance.DataMgr.GetPvPRateRewardData(_seasonList[i].BFGRADE);
				lbRankLeague.text = prevData.name + " 리그";

				lbRate.text = _seasonList[i].RANK + "위";

                //StartCoroutine(CoChangeGradeAnim(_seasonList[i].BFGRADE, _seasonList[i].GRADE));
                var date = PvPReadySys.GetRemainPvPSeasonEnd();
                lbGuide.text = string.Format("보상은 우편함으로 이동합니다.\n다음 시즌 종료는[F600FF] {0}요일 {1:00}:00[-] 입니다.", DailyDungeonUI.GetWeekStr((int)date.DayOfWeek), date.Hour);
			}
		}
	}

	IEnumerator CoChangeGradeAnim(int _bfGrade, int _grade)
	{
		bAnimationing = true;
		spRankIcon.spriteName = UIPvPMatch.GetGradeBigSprite(_bfGrade);
		yield return new WaitForSeconds(0.5f);

		float time = 1f;
		float acc = Time.deltaTime;
		while(acc < time)
		{
			var v = acc / time;
			spRankIcon.alpha = EaseLinear(1f, 0f, v);
			var scale = EaseLinear(1f, 0.3f, v);
			spRankIcon.transform.localScale = new Vector3(scale, scale, 1);
			yield return null;
			acc += Time.deltaTime;
		}
		spRankIcon.spriteName = UIPvPMatch.GetGradeBigSprite(_grade);
        RankEffectManager.CreatePVP(_grade, spRankIcon.transform);
        acc = Time.deltaTime;
		while (acc < time)
		{
			var v = acc / time;
			spRankIcon.alpha = v;

			var scale = EaseOutBack(0.3f, 1f, v*v*v);
			spRankIcon.transform.localScale = new Vector3(scale, scale, 1);

			yield return null;
			acc += Time.deltaTime;
		}

		yield return new WaitForSeconds(0.3f);

		switch(_bfGrade.CompareTo(_grade) )
		{
			case -1: // 강등
				time = 0.1f;
				acc = Time.deltaTime;
				while (acc < time)
				{
					var rot = EaseOutQuart(0f, -15f, acc / time);
					spRankIcon.transform.localRotation = Quaternion.Euler(0f, 0f, rot);

					yield return null;
					acc += Time.deltaTime;
				}
				break;

			case 0: // 잔류
							// Do Nothing
				break;

			case 1: // 승급
				time = 0.1f;
				acc = Time.deltaTime;
				while (acc < time)
				{
					var scale = EaseLinear(1f, 1.1f, acc / time);
					spRankIcon.transform.localScale = new Vector3(scale, scale, 1f);

					yield return null;
					acc += Time.deltaTime;
				}
				time = 0.5f;
				acc = Time.deltaTime;
				while (acc < time)
				{
					var scale = EaseLinear(1.1f, 1f, acc / time);
					spRankIcon.transform.localScale = new Vector3(scale, scale, 1f);

					yield return null;
					acc += Time.deltaTime;
				}
				spRankIcon.transform.localScale = Vector3.one;
				break;
		}
		bAnimationing = false;
	}

	float EaseLinear(float _s, float _e, float _v)
	{
		_e -= _s;
		return _s + (_e * _v);
	}
	private float EaseOutQuart(float _s, float _e, float _v)
	{
		_v--;
		_e -= _s;
		return -_e * (_v * _v  - 1) + _s;
	}

	float EaseOutBack(float _s, float _e, float _v)
	{
		float s = 1.70158f;
		_e -= _s;
		_v = (_v) - 1;
		return _e * ((_v) * _v * ((s + 1) * _v + s) + 1) + _s;
	}
}
