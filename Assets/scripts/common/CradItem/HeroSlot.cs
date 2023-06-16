using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSlot : SlotBase<SpineCharacterCtrl>
{
	UISprite bg;
	UISprite emptyImg;
	GameObject icon_Chemi;
    GameObject icon_TeamSkill;
	UISprite spSelected;
	Transform char_Root;
	GameObject btn_Root;

    UISprite spRank;
    UISprite spType;

    int chemiCount;

	float acc = 0f;
	float floatTime = 0.6f;
	float fadeoutTime = 0.6f;

	UILabel lbChemi;
	Transform tfChemi;
	Queue<string> chemiQueue;


	internal void Init(int _num, Action<int> _cbClick, Action<int> _cbPress, Action<long> _cbManage, Action<long> _cbUnpos, Action<int> _cbSwap)
	{
		bg = GetComponent<UISprite>();
		emptyImg = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "icon");
		icon_Chemi = UnityCommonFunc.GetGameObjectByName(gameObject, "Icon_chemi");
		icon_TeamSkill = UnityCommonFunc.GetGameObjectByName(gameObject, "Icon_team");
		char_Root = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "spine_root");
		btn_Root = UnityCommonFunc.GetGameObjectByName(gameObject, "buttons");
		spSelected = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "highlight");
        spRank = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "spRank");
        spType = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "spType");
        spRank.gameObject.SetActive(false);
        spType.gameObject.SetActive(false);

        var btManage = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "Btn_manage");
		var btUnpos = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "Btn_unpos");

		tfChemi = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "chemiEffect");
		lbChemi = tfChemi.GetComponent<UILabel>();
		tfChemi.gameObject.SetActive(false);

		btManage.onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            _cbManage(Id); }));
		btUnpos.onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            _cbUnpos(Id); }));

		chemiCount = 0;
		bg.color = Color.white;
		emptyImg.enabled = true;
        icon_Chemi.SetActive(false);
		icon_TeamSkill.SetActive(false);
        spSelected.enabled = false;
		btn_Root.SetActive(false);
		chemiQueue = new Queue<string>();
		base.Init(_num, _cbClick, _cbPress, _cbSwap);
	}



	// 존재하던 스파인캐릭터를 반환한다.
	internal SpineCharacterCtrl SetCharacter(long _id)
	{
		SpineCharacterCtrl result = RemoveItem();
		UpdateSlotOn(true);
		SetID(_id);

		AcumulateTimer ty = new AcumulateTimer();
		ty.Begin();
		var dt = GameCore.Instance.PlayerDataMgr.GetUnitData(_id);

        spRank.gameObject.SetActive(true);
        spType.gameObject.SetActive(true);
        spRank.spriteName = UnitDataMap.GetRankSpriteName(dt.rank);
        spType.spriteName = UnitDataMap.GetTypeSpriteName(dt.charType);

        GameCore.Instance.ResourceMgr.GetInstanceObject(ABType.AB_Prefab, dt.prefabId, (_obj) =>
		{
            if (_obj == null)
            {
                Debug.LogError("Load Fail!!");
                return;
            }
			AcumulateTimer timer = new AcumulateTimer();
			var tf = _obj.transform;
			tf.gameObject.layer = LayerMask.NameToLayer("UI");
			tf.parent = char_Root;
			UnityCommonFunc.ResetTransform(tf);
			timer.Begin();

			// 스파인 캐릭터
			var ctrl = tf.gameObject.AddComponent<SpineCharacterCtrl>();
			ctrl.Init(false, 0, null, false);
			timer.End();

			// 그림자
			var shadow = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/shadow", tf);
			shadow.layer = LayerMask.NameToLayer("UI");
			shadow = shadow.transform.GetChild(0).gameObject;
			shadow.layer = LayerMask.NameToLayer("UI");
			shadow.transform.localScale = Vector3.one;

			SetItem(_id, ctrl);
			ty.End();
			//Debug.Log("Spine Prefab Create Delay : " + timer.Delay + "   " + ty.Delay);
		});

		return result;
	}

	internal SpineCharacterCtrl SetCharacter(long _id, SpineCharacterCtrl _ctrl)
	{
        var dt = GameCore.Instance.PlayerDataMgr.GetUnitData(_id);

        spRank.gameObject.SetActive(true);
        spType.gameObject.SetActive(true);
        spRank.spriteName = UnitDataMap.GetRankSpriteName(dt.rank);
        spType.spriteName = UnitDataMap.GetTypeSpriteName(dt.charType);

        SpineCharacterCtrl result = SetItem(_id, _ctrl);

		var tf = _ctrl.transform;
		tf.parent = char_Root;
		UnityCommonFunc.ResetTransform(tf);
		//SetChemistry(false);
		tf.gameObject.SetActive(true);

		return result;
	}

	internal SpineCharacterCtrl RemoveCharacter()
	{
        spRank.gameObject.SetActive(false);
        spType.gameObject.SetActive(false);
        SetTeamSkillIcon(false);
		chemiQueue.Clear();
		acc = 999f;
		return RemoveItem();
	}


	internal void AddPrintChemistry(string _str)
	{
		if (_str == null)
			return;

		var strs = _str.Split('\n');
		for (int i = 0; i < strs.Length ; i++)
			if (strs[i] != "")
				chemiQueue.Enqueue(strs[i]);
	}

	internal void IncChemistry()
	{
		++chemiCount;
		icon_Chemi.SetActive(0 < chemiCount);
	}
	
	internal void DecChemistry()
	{
		--chemiCount;
		icon_Chemi.SetActive(0 < chemiCount);
    }

	internal void ResetChemistry()
	{
		chemiCount = 0;
		icon_Chemi.SetActive(false);
    }

	internal void SetTeamSkillIcon(bool _active)
	{
		icon_TeamSkill.SetActive(_active);
    }

	private void Update()
	{
		if (lbChemi.gameObject.activeSelf == false)
		{
			if (chemiQueue.Count == 0)
				return;

			lbChemi.text = chemiQueue.Dequeue();
			acc = 0f;
			tfChemi.localPosition = new Vector3();
			lbChemi.gameObject.SetActive(true);
		}

		if(lbChemi.gameObject.activeSelf == true)
		{ 
			acc = Mathf.Min(acc + Time.deltaTime, floatTime + fadeoutTime);

			if (acc < floatTime + fadeoutTime)
			{
				tfChemi.localPosition = Vector3.Lerp(new Vector3(0f, -50f, 0f), new Vector3(0f, 70f, 0f), acc / (floatTime + fadeoutTime));
                if (tfChemi.localPosition.y <= 30f)
                    lbChemi.alpha = 1f;
                else
                    lbChemi.alpha = Mathf.Lerp(1f, 0f, (acc - floatTime) / fadeoutTime);
			}
			else
			{
				lbChemi.gameObject.SetActive(false);
			}
		}
	}

	protected override void UpdateAffect(bool _active)
	{
		spSelected.enabled = _active;
		spSelected.spriteName = "SELECT_02_01_01";
	}

	protected override void UpdateSelect(bool _active)
	{
		spSelected.enabled = _active;
		spSelected.spriteName = "SELECT_01_01_01";
	}

	protected override void UpdateSlotOn(bool _on)
	{
		//bg.color = _on ? Color.gray : Color.white;
		bg.spriteName = _on ? "SLOT_01_03" : "SLOT_02_01";
		emptyImg.enabled = !_on;
        if (!_on)
            icon_Chemi.SetActive(false);
		//icon_TeamSkill.enabled = false;
		//if (_on == false)
		//	UpdateButton(false);
	}

	protected override void UpdateButton(bool _active)
	{
		btn_Root.SetActive(_active);
	}

	internal bool IsEmpty()
	{
		return Id == -1;
	}

}
