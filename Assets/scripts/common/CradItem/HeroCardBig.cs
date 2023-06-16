using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class HeroCardBig : CardBase
{
    [SerializeField] UISprite _illust;
    [SerializeField] UISprite _backGround;
    [SerializeField] UIGrid _stars;
    [SerializeField] UISprite _cover;
    [SerializeField] UISprite _typeMark;
    [SerializeField] UILabel _name;    
    [SerializeField] UILabel _reinforce;
    [SerializeField] UISprite _blind;
    [SerializeField] UISprite _rank;
    [SerializeField] UISprite _selected;
    [SerializeField] UISprite _state;
    [SerializeField] GameObject _info;
    [SerializeField] GameObject _infoEquip;
    [SerializeField] UISprite _infoIcon;
    [SerializeField] UILabel _stateLabel;
    [SerializeField] UILabel _infoLabel;
    [SerializeField] internal GameObject _manageButton; 
    [SerializeField] GameObject _unposButton;  
    [SerializeField] GameObject _isDead;
    public GameObject _newCard;
    public GameObject _enchantCard;
    protected string rank_str;//  랭크 문자열 캐시

    private void InitLink()
    {
        if (_illust != null)
            return;

        #region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
        // [NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.

        //_illust = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Illust");
        //_backGround = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "bg");
        //_stars = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "Stars");
        //_cover = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Cover");
        //_typeMark = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "TypeMark");
        //_name = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Name");
        //_reinforce = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Reinforce");
        //_blind = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Blind");
        //_rank = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Rank");
        //_selected = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Selected");
        //_state = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "State");
        //_info = UnityCommonFunc.GetGameObjectByName(gameObject, "Info");
        //_infoEquip = UnityCommonFunc.GetGameObjectByName(gameObject, "EquipInfo");
        //_infoIcon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Info_icon");
        //_stateLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "State_text");
        //_infoLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Info_text");
        //_manageButton = UnityCommonFunc.GetGameObjectByName(gameObject, "Btn_manage");
        //_unposButton = UnityCommonFunc.GetGameObjectByName(gameObject, "Btn_unpos");
        //_isDead = UnityCommonFunc.GetGameObjectByName(gameObject, "isDead");
        //_newCard = UnityCommonFunc.GetGameObjectByName(gameObject, "NewCard");
        //_enchantCard = UnityCommonFunc.GetGameObjectByName(gameObject, "enchantCard");
        #endregion
    }

    internal override void Init(CardSData _sdata, CardDataMap _data, Action<long> _cbClick, Action<long> _cbPress)
    {
        InitLink();
        base.Init(_sdata, _data, _cbClick, _cbPress);

        var data = (UnitDataMap)Data;
        if (data == null)
            return;

        // 일러스트 및 이름 설정
        GameCore.Instance.SetUISprite(_illust, data.GetBigCardSpriteKey());
        _name.text = data.name;


        // 별 설정 // 각성막음
        ///////////////////////////////////////////////////////////////////////////
        /// Open spec : 각성 불가이기 때문에 등급만 5성으로 고정                ///
        ///////////////////////////////////////////////////////////////////////////
        //var cnt = data.evolLvl;                                               ///
        var cnt = Mathf.Min(data.evolLvl, 5);                                   ///
        ///////////////////////////////////////////////////////////////////////////


        // 커버 설정
        rank_str = "ICON_LV_" + (5 - data.rank).ToString("00");
        _cover.spriteName = "CARD_LV_" + (5 - data.rank).ToString("00");
        _rank.spriteName = rank_str + "_S";

        // 경험치 카드 분기
        if (((UnitDataMap)Data).IsExpCard())
        {
            // Set Star
            for (int i = 0; i < 5; ++i)
                _stars.transform.GetChild(i).gameObject.SetActive(false);

            // Set Type
            _typeMark.spriteName = "";
            
            // [ NOTE ] 2020-02-20 이현철
            // 기존 버전 NGUI는 spriteName을 지울경우 연산오류로 인하여 그래픽이 출력되지 않았으나,
            // 2019.03버전 기준으로 그 현상이 패치되었기 때문에 신버전에서 
            // 출력되면 안되는것이 출력되는 버그가 발생하여
            // Alpha값을 조절하여 문제해결
            _typeMark.alpha = 0;

            // Set Enchant
            _reinforce.text = "";
        }
        else
        {
            SetStar(cnt);
            _typeMark.spriteName = CardDataMap.GetTypeSpriteName(data.charType);// "ICON_TYPE_01_" + udata.charType.ToString("00") + "_S";

            // alpha값 원상복귀
            _typeMark.alpha = 1; 

            if (_sdata != null) UpdateEnchant(((HeroSData)_sdata).enchant);
            else                UpdateEnchant(0);
        }


        
        


        // 상태 설정
        _state.gameObject.SetActive(false);
        _stateLabel.text = "";

        // 정보 설정
        _info.SetActive(false);
        _infoIcon.gameObject.SetActive(false);
        _infoLabel.text = "";

        // 기본 상태 설정
        _blind.enabled = false;
        _selected.enabled = false;
        _manageButton.SetActive(false);
        _unposButton.SetActive(false);
        _isDead.SetActive(false);
        _newCard.SetActive(false);
        _enchantCard.SetActive(false);

        if (GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.Inven)
        {
            if (GameCore.Instance.PlayerDataMgr.GetNewHeroCardUidList() != null)
            {
                for (int i = 0; i < GameCore.Instance.PlayerDataMgr.GetNewHeroCardUidList().Count; ++i)
                {
                    if (GameCore.Instance.PlayerDataMgr.GetNewHeroCardUidList()[i] == _sdata.uid) _newCard.SetActive(true);
                }
            }

            if (isHave)
                _enchantCard.SetActive(IsEnchantAlert(_sdata.uid));
        }
    }



    public static bool IsEnchantAlert(long _uid)
    {
        if (_uid <= 0)
            return false;

        var evolLvl = GameCore.Instance.PlayerDataMgr.GetUnitData(_uid).evolLvl;
        var enchant = GameCore.Instance.PlayerDataMgr.GetUnitSData(_uid).enchant;
        var exp = GameCore.Instance.PlayerDataMgr.GetUnitSData(_uid).exp;

        var maxEnchant = GameCore.Instance.DataMgr.GetMaxStrengthenLevel(evolLvl);

        // 현재 진화단계에서 강화단계가 남은 경우 // 각성막음
        if (maxEnchant > enchant)   return GameCore.Instance.DataMgr.GetStrengthenCostData(evolLvl, enchant + 1).costExp <= exp;
        else if (evolLvl < 5/*10*/) return GameCore.Instance.DataMgr.GetStrengthenCostData(evolLvl + 1, 0).costExp <= exp;
        else                        return false;
    }



    public void SetStar(int _cnt)
    {
        var starName = (_cnt <= 5) ? "ICON_STAR_01_S" : "ICON_STAR_02_S";
        var emptyStarName = (_cnt <= 5) ? "ICON_STAR_00_S" : "ICON_STAR_01_S";
        _cnt = ((_cnt - 1) % 5) + 1;
        var starCnt = _stars.transform.childCount;
        for (int i = 0; i < starCnt; ++i)
        {
            if (i < _cnt)
            {
                _stars.transform.GetChild(i).GetComponent<UISprite>().spriteName = starName;
                _stars.transform.GetChild(i).GetComponent<UISprite>().color = Color.white;
                _stars.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                _stars.transform.GetChild(i).GetComponent<UISprite>().spriteName = emptyStarName;
                _stars.transform.GetChild(i).GetComponent<UISprite>().color = (_cnt <= 5) ? new Color(1f, 1f, 1f) : Color.white;
                _stars.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        _stars.Reposition();
    }


	internal override void SetManageButtonCallBack(Action<long> _cb)
	{
		_manageButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { if (_cb != null) _cb(ID); }));
	}

	internal override void SetUnposButtonCallBack(Action<long> _cb)
	{
		_unposButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { if (_cb != null) _cb(ID); }));
	}

	protected override void UpdateEnable(bool _active)
	{
		_rank.spriteName = rank_str + ((_active) ? "_S" : "_B");
		_blind.enabled = !_active;
	}

	protected override void UpdateState(States _state)
	{
		switch (_state)
		{
			case States.Normal:
				this._state.gameObject.SetActive(false);
				break;
			case States.Detachment:
				this._state.gameObject.SetActive(true);
				_stateLabel.text = "파밍중";
				_stateLabel.color = new Color32(0xFF, 0x7E, 0x00, 0xFF);
				break;
			case States.Arrangement:
				this._state.gameObject.SetActive(true);
				_stateLabel.text = "팀배치"; 
				_stateLabel.color = new Color32(0x00, 0xF0, 0xFF, 0xFF);
				break;
            case States.MainCharacter:
                this._state.gameObject.SetActive(true);
                _stateLabel.text = "대표";
                _stateLabel.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
                break;
			case States.NotHave:
				this._state.gameObject.SetActive(true);
				_stateLabel.text = "미보유";
				_stateLabel.color = new Color32(0xFF, 0x00, 0x00, 0xFF);
				break;
			default: // myroom
				this._state.gameObject.SetActive(true);
				_stateLabel.text = "숙소"+((int)_state - (int)States.Room);
				_stateLabel.color = new Color32(0xFF, 0xEA, 0x00, 0xFF);
				break;
		}
	}


	protected override void UpdateInfo(UnitInfo _info)
	{
		Info = _info;

        this._info.SetActive((Info & UnitInfo.Power) != UnitInfo.None);
        _infoEquip.SetActive((Info & UnitInfo.Equip) != UnitInfo.None);

        if (this._info.activeSelf)
        {
            this._info.SetActive(true);
            if (isHave)
            {
                _infoLabel.text = GameCore.Instance.PlayerDataMgr.GetUnitSData(ID).GetPower().ToString("N0");
                _infoLabel.pivot = UIWidget.Pivot.Right;
            }
            else
                return;

            _infoIcon.gameObject.SetActive(true);
        }
	}

	protected override void UpdateHighLight(SelectState _state)
	{
        if (_state == SelectState.Highlight)
		{
			_selected.enabled = true;
			_selected.spriteName = "SELECT_02_01_02";
			UpdateEnable(false);
		}
		else if (_state == SelectState.Select)
		{
			_selected.enabled = true;
			_selected.spriteName = "SELECT_01_01_02";
			UpdateEnable(false);
		}
		else
		{
			_selected.enabled = false;
			UpdateEnable(State == States.Normal);
		}
	}

	protected override void UpdateButton(ActiveButton _active)
	{
		_manageButton.SetActive((_active & ActiveButton.Management) != 0);
		_unposButton.SetActive((_active & ActiveButton.Unposition) != 0);
	}

	IEnumerator Resize()
	{
		yield return null;

		//if (tbInfo.gameObject.activeInHierarchy)
		//	tbInfo.Reposition();

		if (_stars.gameObject.activeInHierarchy)
			_stars.Reposition();
	}

	protected override void UpdateCount(int _count)
	{
		// None
	}

    protected override void UpdateEnchant(int _value)
    {
        _reinforce.text = string.Format("{0}", _value);
    }

    protected override void UpdateCompare(CardSData _target)
    {
        // None
    }

    public void SetDeadSprite()
    {
        float colorVaule = 100f / 255f;
        Color colorGray = new Color(colorVaule, colorVaule, colorVaule);
        _illust.color = colorGray;
        _backGround.color = colorGray;
        _cover.color = colorGray;
        _typeMark.color = colorGray;
        _name.color = colorGray;
        _name.transform.parent.GetComponent<UISprite>().color = colorGray;
        _reinforce.color = colorGray;
        _blind.color = colorGray;
        _rank.color = colorGray;

        int starCnt = _stars.transform.childCount;
        for (int i = 0; i < starCnt; ++i)
        {
            _stars.transform.GetChild(i).GetComponent<UISprite>().color = colorGray;
        }
        _isDead.SetActive(true);
    }

    protected override void UpdateLock(bool _lock)
    {
        // Todo : UpdateLock HeroCardBig
    }
}
