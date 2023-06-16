using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal abstract class CardBase : ButtonRapper
{
	internal enum States        // 유닛 카드 데이터 테이블이 완성되면 그곳으로 옮길것
	{
		NotHave = 15,            // 미보유
		Normal = 14,             // 일반
		Detachment = 2,         // 파견
		Arrangement = 1,        // 배치
        MainCharacter = 0,      // 메인 캐릭터

        Room = 3,               // 마이룸 배치 0 (+1 ~ +10)
	}

	internal enum SelectState
	{
		None,       // 효과 무
		Select,     // 선택(파랑)
		Highlight,  // 강조(빨강)
	}

    [System.Flags]
	internal enum UnitInfo
	{
		None,   // None
		Power,  // 전투력
		Equip,  // 파츠 작용중
	}

	[System.Flags]
	internal enum ActiveButton
	{
		None = 0,
		Management = 1 << 0,
		Unposition = 1 << 1,
    }


	protected static Color[] colors = new Color[]
	{
		new Color32(246,  0,255,255), // SSS
		new Color32(  0,240,255,255),	// SS
		new Color32(255,234,  0,255),	// S
		new Color32(255,255,255,255),	// A
		new Color32(139, 88, 46,255), // B
	};

	public long ID { get; protected set; }
	public bool isHave { get; protected set; } // true : id is UID, id is Key
	public CardType Type { get; protected set; }
	public CardSData SData { get; protected set; }
	public CardDataMap Data { get; protected set; }
	public bool Active { get; protected set; }          //  활성화 여부
	public SelectState Select { get; protected set; }		//  현재 선택 상태
	public States State { get; protected set; }					//N 0:파견중, 1:None, 2:배치중, 3:메인 캐릭터
	public UnitInfo Info { get; protected set; }				//N 0:None, 1:전투력, 2:파츠착용중
	public ActiveButton Button { get; protected set; }	//F 0 : None, 1:영웅관리, 2:배치해제
	public int Count { get; protected set; }
    public int Upgrade { get; protected set; }
    public bool Lock { get; protected set; }


    bool internalButtonActive = true; // 버튼 활성화 가능 여부
	bool internalCountActive = true; // true일때만 카운트 표현


	private bool bPressed;

	#region Create Card

	internal static CardBase CreateBigCard(CardDataMap _data, Transform _parent, Action<long> _cbClick = null, Action<long> _cbPress = null)
	{		return CreateCard(null, _data, true, _parent, _cbClick, _cbPress);	}
	internal static CardBase CreateSmallCard(CardDataMap _data, Transform _parent, Action<long> _cbClick = null, Action<long> _cbPress = null)
	{ return CreateCard( null, _data, false, _parent, _cbClick, _cbPress); }
	internal static CardBase CreateBigCard(CardSData _sdata, Transform _parent, Action<long> _cbClick = null, Action<long> _cbPress = null)
	{ return CreateCard(_sdata, null, true, _parent, _cbClick, _cbPress); }
	internal static CardBase CreateSmallCard(CardSData _sdata, Transform _parent, Action<long> _cbClick = null, Action<long> _cbPress = null)
	{ return CreateCard(_sdata, null, false, _parent, _cbClick, _cbPress); }

	internal static CardBase CreateBigCardByKey(int _key, Transform _parent, Action<long> _cbClick = null, Action<long> _cbPress = null)
	{
		CardDataMap data;
		if (CardDataMap.IsItemKey(_key))			data = GameCore.Instance.DataMgr.GetItemData(_key);
		else if (CardDataMap.IsUnitKey(_key)) data = GameCore.Instance.DataMgr.GetUnitData(_key);
		else																	data = GameCore.Instance.DataMgr.GetTeamSkillData(_key);
		return CreateCard(null, data, true, _parent, _cbClick, _cbPress);
	}
	internal static CardBase CreateSmallCardByKey(int _key, Transform _parent, Action<long> _cbClick = null, Action<long> _cbPress = null)
	{
		CardDataMap data;
		if (CardDataMap.IsItemKey(_key))			data = GameCore.Instance.DataMgr.GetItemData(_key);
		else if (CardDataMap.IsUnitKey(_key)) data = GameCore.Instance.DataMgr.GetUnitData(_key);
		else																	data = GameCore.Instance.DataMgr.GetTeamSkillData(_key);
		return CreateCard(null, data, false, _parent, _cbClick, _cbPress);
	}

	// Skill Card...

	internal static CardBase CreateCard(CardSData _sdata, CardDataMap _data, bool _big, Transform _parent, Action<long> _cbClick = null, Action<long> _cbPress = null)
	{
		if (_sdata == null && _data == null)
		{
			Debug.LogError("Invalid Data");
			return null;
		}

		CardType type = _data != null ? _data.type : _sdata.type;

		string resourceName = "commonRsc/prefab/";
		switch (type)
		{
			case CardType.TeamSkill:
				if (_data == null)
				{
					Debug.LogError("Invalid Data");
					return null;
				}
				resourceName += "skill_card";
				break;

			case CardType.Character:
				if (_data == null)		_data = GameCore.Instance.DataMgr.GetUnitData(_sdata.key);
				resourceName += "hero_card_" + (_big ? "big" : "small");
                break;

			default:
				if (_data == null) _data = GameCore.Instance.DataMgr.GetItemData(_sdata.key);
				resourceName += "item_card_" + (_big ? "big" : "small");
				break;
		}

		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(resourceName, _parent);
		var card = go.GetComponent<CardBase>();
        if (_cbPress == null)
        {
            if(_sdata != null)
                card.Init(_sdata, _data, _cbClick, (id)=> GameCore.Instance.ShowCardInfo(_sdata));
            else
                card.Init(_sdata, _data, _cbClick, (id) => GameCore.Instance.ShowCardInfoNotHave(_data.id));
        }
        else
    		card.Init(_sdata, _data, _cbClick, _cbPress);

		return card;
	}

	#endregion

	internal virtual void Init(CardSData _sdata, CardDataMap _data, Action<long> _cbClick, Action<long> _cbPress)
	{
		if (_sdata == null && _data == null)
		{
			Debug.LogError("Invalid Data");
			return;
		}

		SData = _sdata;
		Data = _data;
		if (Data == null)
		{
			if (SData.type == CardType.Character)
				Data = GameCore.Instance.DataMgr.GetUnitData(SData.key);
			else
				Data = GameCore.Instance.DataMgr.GetItemData(SData.key);
		}
		if( Data == null)
		{
			Debug.LogError("올바르지 않은 유닛 데이터입니다.");
			return;
		}

		isHave = SData != null;
		ID = isHave ? SData.uid : Data.id;
		Type = Data.type;

		if (_cbClick != null) cbClick = () => _cbClick(ID);
		if (_cbPress != null) cbPress = () => _cbPress(ID);

		Active = true;
		Select = SelectState.None;    //  현재 선택 상태
		State = States.Normal;		  // N 0:파견중, 1:None, 2:배치중
		Info = UnitInfo.None;		  // N 0:None, 1:전투력, 2:파츠착용중
		Button = ActiveButton.None;   // F 0 : None, 1:영웅관리, 2:배치해제
		Count = -1;

		bPressed = false;
	}

	internal void SetCallback(Action<long> _cbClick, Action<long> _cbPress)
	{
		if (_cbClick != null) cbClick = () => _cbClick(ID);
		if (_cbPress != null) cbPress = () => _cbPress(ID);
	}

	internal void InternalButtonActive(bool _Active)
	{
		internalButtonActive = _Active;
	}

	internal void InternalCountActive(bool _active)
	{
		internalCountActive = _active;
	}

	internal virtual void SetManageButtonCallBack(Action<long> _cb)
	{
		//btManage.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => _cb(ID)));
	}

	internal virtual void SetUnposButtonCallBack(Action<long> _cb)
	{
		//btUnpos.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => _cb(ID)));
	}

	protected abstract void UpdateEnable(bool _active);
	protected abstract void UpdateState(States _state);
	protected abstract void UpdateInfo(UnitInfo _info);
	protected abstract void UpdateHighLight(SelectState _state);
	protected abstract void UpdateButton(ActiveButton _active);
	protected abstract void UpdateCount(int _count);
    protected abstract void UpdateEnchant(int _value);
    protected abstract void UpdateLock(bool _lock);
    protected abstract void UpdateCompare(CardSData _target);

    internal void SetEnable(bool _Active)
	{
		if (Active != _Active)
		{
			Active = _Active;
			UpdateEnable(Active);
		}
	}


	internal void SetState(States _state, bool _force = false)
	{
		if (_force || State != _state)
		{
			//Debug.Log(gameObject.name + " : " +  _state);
			State = _state;
			UpdateState(State);
			SetEnable(State == States.Normal);
		}
	}


	internal void SetInfo(UnitInfo _info)
	{
		if (Info != _info)
		{
			Info = _info;
			UpdateInfo(Info);
		}
	}

	internal void SetSelect(SelectState _select)
	{
		if (Select != _select)
		{
			Select = _select;
			UpdateHighLight(Select);
			//if (Select == SelectState.Select)
			//	SetButton(ActiveButton.Management);
			//else
			//	SetButton(ActiveButton.None);
		}
	}

	internal void SetButton(ActiveButton _Active)
	{
		if(internalButtonActive == false)
		{
			UpdateButton(ActiveButton.None);
			return;
		}

		if (Button != _Active)
		{
			Button = _Active;
			UpdateButton(Button);
		}
	}

	internal void AddButton(ActiveButton _Active)
	{
		if (internalButtonActive == false)
		{
			UpdateButton(ActiveButton.None);
			return;
		}

		if ((Button | _Active) != Button)
		{
			Button |= _Active;
			UpdateButton(Button);
		}
	}
	internal void SubButton(ActiveButton _Active)
	{
		if (internalButtonActive == false)
		{
			UpdateButton(ActiveButton.None);
			return;
		}

		if ((Button & _Active) == _Active)
		{
			Button &= ~_Active;
			UpdateButton(Button);
		}
	}

	internal void SetCount(int _count)
	{
		if (internalCountActive)
			Count = _count;
		else
			Count = 0;
		UpdateCount(Count);
	}

    internal void SetEnchant(int _value)
    {
        Upgrade = _value;
        UpdateEnchant(_value);
    }

    internal void SetLock(bool _lock)
    {
        Lock = _lock;
        UpdateLock(_lock);
    }

    internal void SetCompare(CardSData _target = null)
    {
        UpdateCompare(_target);
    }
}