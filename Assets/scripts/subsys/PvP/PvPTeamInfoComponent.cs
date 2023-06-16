using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class PvPTeamInfoComponent : MonoBehaviour
{
	public Transform[] CardRoots;
	public UISprite spSkillIcon;
	public UILabel lbSkillName;
	public UILabel lbUserNameGuide;


	public void SetData(List<PvPOppUnitSData> _unitDatas, string _userName)
	{
        lbUserNameGuide.text = string.Format("{0}의 대표팀 정보입니다.", _userName);

        if ( _unitDatas == null || _unitDatas.Count == 0)
		{
            lbSkillName.text = "스킬없음";
            spSkillIcon.gameObject.SetActive(false);
            Debug.LogError("invalid Data");
			return;
		}

		
		if (_unitDatas[0].skill > 0)
		{
			var skillData = GameCore.Instance.DataMgr.GetTeamSkillData(_unitDatas[0].skill);
			GameCore.Instance.SetUISprite(spSkillIcon, skillData.imageID);
            spSkillIcon.gameObject.SetActive(true);
            lbSkillName.text = skillData.name;
		}
		else
		{
			GameCore.Instance.SetUISprite(spSkillIcon, CommonType.SP_TEAMSKILL_EMPTY);
			lbSkillName.text = "스킬없음";
		}
		

		// Set Unit Card
		for(int i = 0; i < _unitDatas.Count; ++i)
			CardBase.CreateBigCardByKey(_unitDatas[i].charID, CardRoots[i]).SetEnchant(_unitDatas[i].enchant);
	}
}
