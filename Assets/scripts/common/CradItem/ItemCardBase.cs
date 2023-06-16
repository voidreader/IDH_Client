using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

internal class ItemCardBase : CardBase
{                                               //	Item	Furniture	Equip      [small o : big 0 ]
    [SerializeField] UISprite _image;           //	 o0			o0		  o0

    [SerializeField] UILabel _countLabel;       //	 o0			o0

    [SerializeField] UISprite _icon;            //				0		  0
    [SerializeField] UILabel _infoLabel;        //				0		  0
    [SerializeField] UILabel _rankLabel;        //				0						

    [SerializeField] UISprite _cover;           //						  o
    [SerializeField] UISprite _blind;           //						  0
    [SerializeField] UILabel _state;            //						  0
    [SerializeField] UISprite _rank;            //						  0
    [SerializeField] UISprite _type;            //						  o0
    [SerializeField] UILabel _reinforce;        //					      o0

    [SerializeField] UISprite _select;
    [SerializeField] UISprite _compareIcon;

    [SerializeField] UIGrid _gridButton;
    [SerializeField] GameObject _manageButton;
    [SerializeField] GameObject _unposButton;

    [SerializeField] GameObject _newCard;
    [SerializeField] public GameObject _enchantCard;
    public bool isBig = false; //Set in inspecter

    private string rank_str;


    private void InitLink()
    {
        #region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
        // [NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.
        // Small card는 일부만, Big card는 모든 컴퍼넌트를 참조함.

        //_image = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "image");
        //_countLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "count");
        //_icon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Info_icon");
        //_infoLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "info");
        //_rankLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "itr_rank");
        //_cover = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "cover");
        //_blind = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "blind");
        //_state = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "state");
        //_rank = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "rank");
        //_type = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "type");
        //_reinforce = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "reinforce");
        //_select = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "select");
        //_compareIcon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "compareIcon");
        //_gridButton = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "buttons");
        //_manageButton = UnityCommonFunc.GetGameObjectByName(gameObject, "Btn_manage");
        //_unposButton = UnityCommonFunc.GetGameObjectByName(gameObject, "Btn_unpos");
        //_newCard = UnityCommonFunc.GetGameObjectByName(gameObject, "NewCard");
        //_enchantCard = UnityCommonFunc.GetGameObjectByName(gameObject, "enchantCard");

        #endregion
    }

    internal override void Init(CardSData _sdata, CardDataMap _data, Action<long> _cbClick, Action<long> _cbPress)
    {
        InitLink();
        base.Init(_sdata, _data, _cbClick, _cbPress);

        if (Data == null)
            return;

        _image.gameObject.SetActive(true);
        if (_blind != null) _blind.enabled = false;
        if (_state != null) _state.gameObject.SetActive(false);

        // Type Cast
        switch (Type)
        {
            case CardType.Character:
                //Debug.LogError("Can't processing this type data." + _data.type);
                break;
            case CardType.Equipment:
                SetEquipData((ItemDataMap)Data);
                break;
            case CardType.Interior:
                SetFurnitureData((ItemDataMap)Data);
                break;
            case CardType.resource:
            default:
                SetItemData((ItemDataMap)Data);
                //Debug.LogError("Can't processing this type data." + Type);
                break;
        }

        // image Set
        if (Type == CardType.Character)
        {
            if (isBig)
                GameCore.Instance.SetUISprite(_image, ((UnitDataMap)Data).GetBigCardSpriteKey());
            else
                GameCore.Instance.SetUISprite(_image, ((UnitDataMap)Data).GetSmallCardSpriteKey());
        }
        //else if(Type == CardType.Interior)
        //{
        //    ItemDataMap itemData = Data as ItemDataMap;
        //    string fileName = itemData.fileName;
        //    spImage.spriteName = string.Concat("128_", fileName);
        //    spImage.atlas = GameCore.Instance.ResourceMgr.GetLocalObject<UIAtlas>(string.Concat("MyRoom/IconAtlas/", itemData.atlasName));
        //}
        else
            GameCore.Instance.SetUISprite(_image, ((ItemDataMap)Data).GetCardSpriteKey());

        _select.enabled = false;


        if (_sdata != null && GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.Inven)
        {
            if (_newCard != null && GameCore.Instance.PlayerDataMgr.GetNewItemCardUidList() != null)
            {
                for (int i = 0; i < GameCore.Instance.PlayerDataMgr.GetNewItemCardUidList().Count; ++i)
                {
                    if (_sdata != null)
                    {
                        if (GameCore.Instance.PlayerDataMgr.GetNewItemCardUidList()[i] == _sdata.uid) _newCard.SetActive(true);
                    }
                }
            }

            if (isHave && _enchantCard != null)
                _enchantCard.SetActive(IsEnchantAlert(_sdata.uid));
        }
    }

    bool IsEnchantAlert(long _uid)
    {
        if (_uid <= 0)
            return false;

        var data = GameCore.Instance.PlayerDataMgr.GetItemData(_uid);
        var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(_uid);

        if (!GameCore.Instance.DataMgr.IsHaveItemStrengthenCostData(data.rank + 10, sdata.enchant + 1))
            return false;

        var nextCostData = GameCore.Instance.DataMgr.GetItemStrengthenCostData(data.rank + 10, sdata.enchant + 1);
        return nextCostData.costExp <= sdata.exp;
    }

    public UISprite GetSprite()
    {
        return _image;
    }

    public UILabel GetCountLabel()
    {
        return _countLabel;
    }

    private void SetEquipData(ItemDataMap _data)
    {
        if (_countLabel != null) _countLabel.gameObject.SetActive(true);
        if (_icon != null) _icon.gameObject.SetActive(true);
        if (_infoLabel != null) _infoLabel.gameObject.SetActive(true);
        if (_rankLabel != null) _rankLabel.gameObject.SetActive(false);
        if (_cover != null) _cover.gameObject.SetActive(!isBig);

        if (_rank != null) _rank.gameObject.SetActive(true);
        if (_type != null) _type.gameObject.SetActive(true);
        if (_reinforce != null) _reinforce.gameObject.SetActive(true);
        if (_compareIcon != null) _compareIcon.gameObject.SetActive(false);

        // Rank
        if (_rank != null)
        {
            rank_str = "ICON_LV_" + (5 - _data.rank).ToString("00");
            _rank.spriteName = rank_str + "_S";
        }
        // type
        _type.spriteName = CardDataMap.GetTypeSpriteName(_data.equipLimit); //"ICON_TYPE_01_" + _data.equipLimit.ToString("00") + "_S";

        // reinforce
        if (isHave)
            _reinforce.text = "+" + ((ItemSData)SData).enchant;
        else
            _reinforce.text = "";

        // Prefix Option
        if (isHave & isBig)
        {
            var prefixIdx = ((ItemSData)SData).prefixIdx;
            var prefixValue = ((ItemSData)SData).prefixValue;
            if (prefixIdx > 0)
            {
                var ruleData = GameCore.Instance.DataMgr.GetItemEffectData(prefixIdx);
                _countLabel.color = new Color32(0x01, 0x9E, 0x59, 0xFF);
                _countLabel.text = string.Format("{0} {1:N0}{2}", ruleData.disc, prefixValue, ruleData.type == 0 ? "" : "%");//((ItemSData)SData).GetPrefixOptString();

            }
            else
            {
                //UpdateCount(((ItemSData)SData).count);
                _countLabel.text = "";
            }
        }
        else
            _countLabel.text = "";


        // info
        if (_infoLabel != null && isBig)
        {
            if (SData != null)
                _infoLabel.text = ((ItemSData)SData).GetPower(false).ToString("N0");
            else
                _infoLabel.text = (_data.GetDefPower().ToString("N0"));
            _infoLabel.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        }
        if (_icon != null && isBig)
        {
            _icon.spriteName = "WHITE_ICON_18";
            _icon.color = new Color32(0xB0, 0xB0, 0xB0, 0xFF);
        }
        // cover
        if (_cover != null)
            _cover.color = colors[_data.rank];
    }

    private void SetFurnitureData(ItemDataMap _data)
    {
        if (_countLabel != null) _countLabel.gameObject.SetActive(true);
        if (_infoLabel != null) _infoLabel.gameObject.SetActive(true);
        if (_rankLabel != null) _rankLabel.gameObject.SetActive(isBig);
        if (_cover != null) _cover.gameObject.SetActive(false);
        if (_icon != null) _icon.gameObject.SetActive(true);

        if (_rank != null) _rank.gameObject.SetActive(false);
        if (_type != null) _type.gameObject.SetActive(false);
        if (_reinforce != null) _reinforce.gameObject.SetActive(false);
        if (_compareIcon != null) _compareIcon.gameObject.SetActive(false);


        // info
        if (_infoLabel != null && isBig)
        {
            _infoLabel.text = _data.optionValue[0].ToString();// "만족도";// _data.satisfaction.ToString("N0");
            _infoLabel.color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        }
        if (_icon != null && isBig)
        {
            _icon.spriteName = "WHITE_ICON_20";
            _icon.color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        }
        if (_rankLabel != null && isBig)
        {
            if (Data.rank != 99)
                _rankLabel.text = "★" + (5 - Data.rank);
            else
                _rankLabel.text = "★1";
        }
        if (_countLabel != null)
        {
            if (isHave)
            {
                if (SData == null)
                    SData = GameCore.Instance.PlayerDataMgr.GetItemSData(ID);
                var count = ((ItemSData)SData).count - ((ItemSData)SData).myRoomCount;
                UpdateCount(count);
                _countLabel.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            }
            else
            {
                UpdateCount(0);
                //SetState(States.NotHave);
                SetState(States.Normal);
            }
        }
    }

    private void SetItemData(ItemDataMap _data)
    {
        if (_countLabel != null) _countLabel.gameObject.SetActive(true);
        if (_infoLabel != null) _infoLabel.gameObject.SetActive(false);
        if (_rankLabel != null) _rankLabel.gameObject.SetActive(false);
        if (_cover != null) _cover.gameObject.SetActive(false);
        if (_icon != null) _icon.gameObject.SetActive(false);

        if (_rank != null) _rank.gameObject.SetActive(false);
        if (_type != null) _type.gameObject.SetActive(false);
        if (_reinforce != null) _reinforce.gameObject.SetActive(false);
        if (_compareIcon != null) _compareIcon.gameObject.SetActive(false);

        if (_countLabel != null)
        {
            if (isHave)
            {
                if (SData == null)
                    SData = GameCore.Instance.PlayerDataMgr.GetItemSData(ID);
                var count = ((ItemSData)SData).count;
                UpdateCount(count);
                _countLabel.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            }
            else
            {
                UpdateCount(0);
                //SetState(States.NotHave);
            }
        }
    }

    internal override void SetManageButtonCallBack(Action<long> _cb)
    {
        if (_manageButton != null)
            _manageButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { if (_cb != null) _cb(ID); }));
    }

    internal override void SetUnposButtonCallBack(Action<long> _cb)
    {
        if (_unposButton != null)
            _unposButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { if (_cb != null) _cb(ID); }));
    }


    internal void SetStateEquip(bool _equip)
    {
        if (Data.type != CardType.Equipment)
            return;

        if (_state != null)
            _state.gameObject.SetActive(_equip);
        _blind.enabled = _equip;
    }

    protected override void UpdateEnable(bool _active)
    {
        _blind.enabled = !_active;
        if (_rank != null)
            _rank.spriteName = rank_str + (_active ? "_S" : "_B");
    }

    protected override void UpdateState(States _state)
    {
        switch (_state)
        {
            case States.Normal:
            case States.Room:
                if (this._state != null)
                    this._state.gameObject.SetActive(false);
                break;

            case States.Detachment: // 타영웅 장착중
                this._state.gameObject.SetActive(true);
                this._state.text = "타영웅";
                this._state.color = Color.white;
                break;

            case States.Arrangement:
                if (this._state != null)
                {
                    this._state.gameObject.SetActive(true);
                    this._state.text = "장착중";
                    this._state.color = new Color32(0x24, 0xFF, 0x00, 0xFF);
                }
                break;

            case States.NotHave:
                if (this._state != null)
                {
                    this._state.gameObject.SetActive(true);
                    this._state.text = "미보유";
                    this._state.color = new Color(1f, 0f, 0f);
                }
                break;
        }
    }


    protected override void UpdateHighLight(SelectState _state)
    {
        if (_state == SelectState.Highlight)
        {
            _select.enabled = true;
            UpdateEnable(false);
            if (Type == CardType.Equipment && isBig)
            {
                _select.spriteName = "SELECT_02_01_02";
                _select.width = 172;
                _select.height = 192;
            }
            else
            {
                _select.spriteName = "SELECT_02_01_01";
                if (isBig)
                {
                    _select.width = 164;
                    _select.height = 184;
                }
            }
        }
        else if (_state == SelectState.Select)
        {
            _select.enabled = true;
            UpdateEnable(false);
            if (Type == CardType.Equipment && isBig)
            {
                _select.spriteName = "SELECT_01_01_02";
                _select.width = 172;
                _select.height = 192;
            }
            else
            {
                _select.spriteName = "SELECT_01_01_01";
                if (isBig)
                {
                    _select.width = 164;
                    _select.height = 184;
                }
            }
        }
        else
        {
            _select.enabled = false;
            UpdateEnable(State == States.Normal);
        }
    }

    protected override void UpdateButton(ActiveButton _active)
    {
        _manageButton.SetActive((_active & ActiveButton.Management) != 0);
        _unposButton.SetActive((_active & ActiveButton.Unposition) != 0);
        _gridButton.enabled = true;
    }


    protected override void UpdateInfo(UnitInfo _info)
    {
    }

    protected override void UpdateCount(int _count)
    {
        if (Type == CardType.Equipment)
            return;

        if (_countLabel == null)
            return;

        if (!isHave && _count <= 1)
        {
            _countLabel.text = "";
        }
        else
        {
            if (isBig) _countLabel.text = "x " + _count.ToString("N0");
            else _countLabel.text = _count.ToString("N0");
        }
    }

    protected override void UpdateEnchant(int _value)
    {
        if (_reinforce != null)
            _reinforce.text = string.Format("+{0}", _value);
    }

    protected override void UpdateCompare(CardSData _target = null)
    {
        if (_compareIcon == null)
            return;

        if (_target != null)
        {
            var compare = ((ItemSData)_target).GetPower(false).CompareTo(((ItemSData)SData).GetPower(false));

            if (compare < 0)
            {
                _compareIcon.gameObject.SetActive(true);
                _compareIcon.color = CommonType.COLOR_01;
                _compareIcon.flip = UIBasicSprite.Flip.Nothing;
            }
            else if (compare > 0)
            {
                _compareIcon.gameObject.SetActive(true);
                _compareIcon.color = CommonType.COLOR_04;
                _compareIcon.flip = UIBasicSprite.Flip.Horizontally;
            }
            else
            {
                _compareIcon.gameObject.SetActive(false);
            }
        }
        else
            _compareIcon.gameObject.SetActive(false);

    }

    protected override void UpdateLock(bool _lock)
    {
        // Todo : UpdateLock ItemCard
    }
}
