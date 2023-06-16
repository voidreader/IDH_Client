using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//public struct FriendData // 이것을 삭제하고 추가된 데이터맵으로 대체할것
//{
//	internal long uid;
//	internal string name;
//    internal string comment;
//	internal int typicalKey;
//	internal int lv;
//	internal int power;
//	internal int skillKey;
//    internal DateTime sendTime;
//    internal DateTime recentTime;
//    //internal HeroSData[] units;
//    internal List<PvPOppUnitSData> units;
//}

internal class FriendSelectItem : MonoBehaviour
{
	UISprite spBG;
	UISprite spIcon;
	UISprite spSkill;
	UILabel lbLevel;
	UILabel lbName;
	UILabel lbPower;

	FriendSData data;
	TeamSkillDataMap teamSkill;
	Action<long> cbSelect;

	internal static FriendSelectItem Create(Transform _parent)
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Story/FriendSelectItem", _parent);
		var result = go.GetComponent<FriendSelectItem>();
		result.InitLink();
		return result;
	}

	private void InitLink()
	{
		spBG = GetComponent<UISprite>();
		spBG.GetComponent<UIButton>().onClick.Add(new EventDelegate( () => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            cbSelect(data.USER_UID); } ));
		spIcon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "charIcon");
		spSkill = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "skillIcon");
		spSkill.GetComponent<ButtonRapper>().SetClickCallback(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            cbSelect(data.USER_UID);
        });
		spSkill.GetComponent<ButtonRapper>().SetPressCallback(OnPressSkill);
		spSkill.GetComponent<ButtonRapper>().SetStopPressCallback(OnStopPressSkill);
		lbLevel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbLevel");
		lbName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbName");
		lbPower = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbPower");
	}

	internal void Init(FriendSData _data, Action<long> _cbSelect)
	{
		cbSelect = _cbSelect;
		data = _data;
		lbName.text = data.USER_NAME;
		lbLevel.text = "LV." + data.USER_LEVEL;
		lbPower.text = data.DELEGATE_TEAM_POWER.ToString("N0");

        var typicalUnit = GameCore.Instance.DataMgr.GetUnitData(data.DELEGATE_ICON);
		GameCore.Instance.SetUISprite(spIcon, UnitDataMap.GetSmallProfileSpriteKey(typicalUnit.prefabId));

        //GetComponent<UIDragCamera>().draggableCamera = _cam;

        // Set Skill Icon
        //GameCore.Instance.SetUISprite(spSkill, -1);

        // Set Skill Icon Callback
        if (0 < _data.SKILL)
        {
            teamSkill = GameCore.Instance.DataMgr.GetTeamSkillData(_data.SKILL);
            if (teamSkill != null)
                GameCore.Instance.SetUISprite(spSkill, teamSkill.imageID);
        }

		spSkill.GetComponent<UIDragScrollView>().scrollView = transform.parent.parent.parent.GetComponent<UIScrollView>();
	}

	internal void SetSelect(bool _active)
	{
		if(_active)		spBG.spriteName = "BTN_02_01_02";
		else			spBG.spriteName = "BTN_05_01_01";
	}

	internal long GetUID()
	{
		return data.USER_UID;
	}

	private void OnPressSkill()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        if (teamSkill == null)
			return;

		GameCore.Instance.ShowCardInfoNotHave(teamSkill.id);
	}

	private void OnStopPressSkill()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        GameCore.Instance.CloseAlert();
	}
}
