using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 전투시 하단에 표시되는 영웅 카드 UI
/// </summary>
public class UnitCardUI : MonoBehaviour
{
	internal int fieldId = -1;
    BattleUnitData data;


    UILabel hpText;
	UISprite heroFace;
	UISprite cardCover;
	UISprite fillSprite;
	UILabel fillTimerText;
	GameObject deathMark;

    //TweenAlpha twAlpha;
    GameObject heroSkillEffect;

	float cacheMaxHp;

	bool isPvP;

    GameObject goStateBuffIcon;
    UISprite spStateBuffIcon;
    bool unuseable = false;

	public void Awake()
	{
		hpText = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "hp_text");
		heroFace = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "hero");
		cardCover = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "cover");
		//twAlpha = UnityCommonFunc.GetComponentByName<TweenAlpha>(gameObject, "coverEffect");
        heroSkillEffect = UnityCommonFunc.GetGameObjectByName(gameObject, "sss_Hero_Select");
        fillSprite = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "fill");
		fillTimerText = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "fill_text");
		deathMark = UnityCommonFunc.GetGameObjectByName(gameObject, "death");

        goStateBuffIcon = UnityCommonFunc.GetGameObjectByName(gameObject, "goStateBuffIcon");
        spStateBuffIcon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "spStateBuffIcon");

        SetCoolTime(0f, 0f);
	}

	internal void SetInfo(BattleUnitData _data)
	{
		if( _data == null )
		{
			SetActive(false);
			return;
		}
        data = _data;
		fieldId = _data.FieldId;
		SetActive(true);
		cacheMaxHp = _data.GetStat(UnitStat.Hp);
		SetHp(_data.NowHp);

		cardCover.spriteName = "INCHARAC_LINE_" + _data.Data.rank;

		GameCore.Instance.SetUISprite(heroFace, _data.Data.GetBattleCardSpriteKey());
		gameObject.SetActive(true);
	}

	public void SetFill()
	{
		fillSprite.fillAmount = 1f;
        heroSkillEffect.SetActive(false);
        //twAlpha.to = 0f;
    }

	public void SetCoolTime(float _acc, float _timer)
	{
		//Debug.Log("[" +fieldId+ "]" + _acc + "  " + _timer);
		if (deathMark.activeSelf)
			return;
        
		if (_acc <= 0)
		{
			fillSprite.fillAmount = 0f;
			fillTimerText.text = "";
            heroSkillEffect.SetActive(!unuseable);
            //twAlpha.to = 1f;
        }
		else
		{
			fillSprite.fillAmount = _acc / _timer;
			fillTimerText.text = _acc.ToString("0.0")+"s";
            heroSkillEffect.SetActive(false);
            //twAlpha.to = 0f;
		}

		if(isPvP)
		{
			SetFill();
		}
	}

	internal void AddEventdelegate(EventDelegate _ed, int i)
	{
		if (isPvP)
			return;

		//var ed = new EventDelegate(_target, _funcName);
		_ed.parameters[0].value = i;
		_ed.parameters[0].expectedType = i.GetType();

		EventDelegate.Add(GetComponent<UIButton>().onClick, _ed);
	}

	internal void SetPvPMode(bool _enable)
	{
		isPvP = _enable;
		if (isPvP)
			SetFill();
	}

	internal void SetHp(float _nowHp)
	{
		hpText.text = "[24FF00FF]" + ((int)_nowHp) + "[-]/" + (int)cacheMaxHp;
		SetAlive(_nowHp > 0);
	}

	internal void ResetHp(float _nowHp, float _maxHp)
	{
		cacheMaxHp = _maxHp;
		SetHp(_nowHp);
	}

	internal void SetActive(bool _active)
	{
		cardCover.transform.parent.gameObject.SetActive(_active);
	}

	internal void SetAlive( bool _live )
	{
		if (deathMark.activeSelf != _live)
			return;

        if (_live == false)
            ShowStateBuff(0);
        heroSkillEffect.SetActive(_live);
        //twAlpha.gameObject.SetActive(_live);
		fillSprite.fillAmount = (_live) ? 0f: 1f;
		deathMark.SetActive(!_live);
		fillTimerText.text = "";
		fillSprite.color = new Color(0f, 0f, 0f, (_live) ? 0.45f : 0.8f);
	}

    internal void UpdateState()
    {
        if (data.HaveStateBuff(EffectType.paralyze))
        {
            ShowStateBuff(1);
        }
        else if (data.HaveStateBuff(EffectType.Sleep))
        {
            ShowStateBuff(2);
        }
        else if(data.HaveStateBuff(EffectType.Stun))
        {
            ShowStateBuff(3);
        }
        else
        {
            // no Show
            ShowStateBuff(0);
        }
    }

    void ShowStateBuff(int num)
    {
        //Debug.Log("[" + data.FieldId + "] " + num);
        unuseable = 0 < num;
        if (deathMark.activeSelf)
            unuseable = false;

        goStateBuffIcon.SetActive(unuseable);
        if (unuseable)
        {
            switch (num)
            {
                case 1: spStateBuffIcon.spriteName = "paralyze_1"; break;
                case 2: spStateBuffIcon.spriteName = "sleep_1"; break;
                case 3: spStateBuffIcon.spriteName = "stun_1"; break;
            }
        }

        SetCoolTime(data.GetCoolTimeAcc(), data.GetCoolTime());
    }
}
