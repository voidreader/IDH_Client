using System;
using UnityEngine;
using System.Collections.Generic;

class TeamSkillCard : CardBase
{										
	UISprite spImage;   
	UISprite spCover;    
	UISprite spBlind;   
	UILabel lbName;

	UISprite spSelect;

	internal void InitLink()
	{
		spImage = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "image");
		spCover = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "cover");
		spBlind = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "blind");
		lbName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "name");
		spSelect = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "select");
	}

	internal override void Init(CardSData _sdata, CardDataMap _data, Action<long> _cbClick, Action<long> _cbPress)
	{
		if(_data == null)
		{
			Debug.LogError("invalid Data");
			return;
		}
		InitLink();
		base.Init(_sdata, _data, _cbClick, _cbPress);

		spSelect.gameObject.SetActive(false);
		GameCore.Instance.SetUISprite(spImage, ((TeamSkillDataMap)_data).imageID);
		lbName.text = ((TeamSkillDataMap)_data).name;
	}

	protected override void UpdateButton(ActiveButton _active)
	{
	}

	protected override void UpdateCount(int _count)
	{
	}

	protected override void UpdateEnable(bool _active)
	{
	}

	protected override void UpdateHighLight(SelectState _state)
	{
	}

	protected override void UpdateInfo(UnitInfo _info)
	{
	}

	protected override void UpdateState(States _state)
	{
	}

    protected override void UpdateEnchant(int _value)
    {
    }

    protected override void UpdateCompare(CardSData _target)
    {
    }

    protected override void UpdateLock(bool _lock)
    {
    }
}
