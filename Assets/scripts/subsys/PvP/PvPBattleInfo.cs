using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvPBattleInfo : MonoBehaviour
{
	public UIPanel panel;
	public UILabel lbLeague;
	public UILabel lbFieldInfo;

	[Header("Player Info")]
	public UILabel lbLavel_p;
	public UILabel lbName_p;
	public UISprite spIcon_p;

	[Header("Opponent Info")]
	public UILabel lbLevel_o;
	public UILabel lbName_o;
	public UISprite spIcon_o;

    public Animator animator;

	internal void SetInfo(PvPSData _oppData)
	{
		var data = GameCore.Instance.DataMgr.GetPvPRateRewardData(GameCore.Instance.PlayerDataMgr.PvPData.grade);
		lbLeague.text = string.Format("- {0} 리그 -", data.name);
		lbFieldInfo.text = string.Format("{0} 의 [FF7E00FF]숙소[-]", _oppData.userName);


		lbLavel_p.text = string.Format("LV.{0}", GameCore.Instance.PlayerDataMgr.Level);
		lbName_p.text = GameCore.Instance.PlayerDataMgr.Name;
		var unitData_p = GameCore.Instance.PlayerDataMgr.GetUnitData(GameCore.Instance.PlayerDataMgr.MainCharacterUID);
		GameCore.Instance.SetUISprite(spIcon_p, UnitDataMap.GetBigProfileSpriteKey(unitData_p.prefabId));

		lbLevel_o.text = string.Format("LV.{0}", _oppData.userLevel);
		lbName_o.text = _oppData.userName;
        
		var unitData_o = GameCore.Instance.DataMgr.GetUnitData(_oppData.typicalKey);
		if(unitData_o == null)
			GameCore.Instance.SetUISprite(spIcon_o, UnitDataMap.GetBigProfileSpriteKey(1));
		else
			GameCore.Instance.SetUISprite(spIcon_o, UnitDataMap.GetBigProfileSpriteKey(unitData_o.prefabId));

        animator = GetComponent<Animator>();

    }
    public float GetAnimatorLimitTime()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }
	// 애니메이셔닝은 UI에서 한다.
	public UIPanel GetPanel()
	{
		return panel;
	}
}
