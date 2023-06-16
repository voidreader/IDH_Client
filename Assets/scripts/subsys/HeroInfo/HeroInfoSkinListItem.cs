using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoSkinListItem : MonoBehaviour
{
    public enum State
    {
        bought  = 0,   // 소지중
        Equipped =1,   // 착용중
        NotBuy = 2,    // 미소지
        Special = 3,   // 이벤트
    }

    internal struct Data
    {
        public long UID;
        public int charID;
        public int spineKey;
        public int illustKey;

        public State state;
        public int priceItemKey;
        public int priceValue;

        public string name;
        internal ItemEffectType stat1;
        public float stat1Value;
        internal ItemEffectType stat2;
        public float stat2Value;

        internal Data(long _uid, int _charID, int _spineKey, int _illustKey, State _state, int _priceKey, int _priceValue, string _name, ItemEffectType _stat1, float _stat1Value, ItemEffectType _stat2, float _stat2Value)
        {
            UID = _uid;
            charID = _charID;
            spineKey = _spineKey;
            illustKey = _illustKey;
            state = _state;
            priceItemKey = _priceKey;
            priceValue = _priceValue;
            name = _name;
            stat1 = _stat1;
            stat2 = _stat2;
            stat1Value = _stat1Value;
            stat2Value = _stat2Value;
        }
    }


    public static HeroInfoSkinListItem Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("HeroInfo/SkinListItem", _parent);
        return go.GetComponent<HeroInfoSkinListItem>();
    }

    [SerializeField] UISprite spBackGround;

    [SerializeField] UILabel lbName;

    [SerializeField] GameObject goEquipped;
    [SerializeField] GameObject goTimer;
    [SerializeField] UILabel lbTimer;

    [SerializeField] Transform spineRoot;

    [SerializeField] UILabel lbStat1Name;
    [SerializeField] UILabel lbStat1Value;

    [SerializeField] UILabel lbStat2Name;
    [SerializeField] UILabel lbStat2Value;

    [SerializeField] UILabel lbPreviewBt;
    [SerializeField] UIButton btPreview;
    [SerializeField] GameObject goPreview;

    [SerializeField] UILabel lbEquipBt;
    [SerializeField] UIButton btEquip;
    [SerializeField] GameObject goPrice;
    [SerializeField] UISprite spPriceIcon;
    [SerializeField] UILabel lbPriceValue;

    Data data;
    SpineCharacterCtrl spine;

    System.DateTime timeLimit;
    System.Action cbTimeOut;
    System.Action<HeroInfoSkinListItem> cbPreview;
    System.Action<HeroInfoSkinListItem> cbPrice;
    List<Spine.Animation> animationList = new List<Spine.Animation>();

    public bool bPreview { get; set; }//get { return goPreview.activeSelf; } set { goPreview.SetActive(value); } }

    internal void Init(Data _data, 
                       UIDraggableCamera _draggableCam,
                       System.Action<HeroInfoSkinListItem> _cbPreview, 
                       System.Action<HeroInfoSkinListItem> _cbPrice, 
                       System.DateTime _TimeLimited = default(System.DateTime), 
                       System.Action _cbTimeOut = null)
    {
        data = _data;
        timeLimit = _TimeLimited;
        cbTimeOut = _cbTimeOut;
        cbPreview = _cbPreview;
        cbPrice = _cbPrice;

        spBackGround.GetComponent<UIDragCamera>().draggableCamera = _draggableCam;
        lbName.text = _data.name;
        lbStat1Name.text = ItemSData.GetItemEffectString(_data.stat1);
        lbStat1Value.text = "+" + _data.stat1Value;
        lbStat2Name.text = ItemSData.GetItemEffectString(_data.stat2);
        lbStat2Value.text = "+" + _data.stat2Value;
        lbPriceValue.text = _data.priceValue.ToString("N0");
        spPriceIcon.spriteName = GetPriceItemSpriteName(_data.priceItemKey);


        GameCore.Instance.ResourceMgr.GetInstanceObject(ABType.AB_Prefab, _data.spineKey, (_obj) =>
        {
            if (_obj == null)
            {
                Debug.LogError("Load Fail!!");
                return;
            }

            AcumulateTimer timer = new AcumulateTimer();
            _obj.layer = LayerMask.NameToLayer("UI");
            var tf = _obj.transform;
            tf.parent = spineRoot;
            UnityCommonFunc.ResetTransform(tf);

            // 스파인 캐릭터
            spine = _obj.AddComponent<SpineCharacterCtrl>();
            spine.Init(false, 0, null, false);
            
            // 그림자
            var shadow = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/shadow", tf);
            shadow = shadow.transform.GetChild(0).gameObject;
            shadow.layer = LayerMask.NameToLayer("UI");
            shadow.transform.localScale = Vector3.one;

            // 애니메이션
            animationList.Clear();
            animationList.AddRange(spine.skelAnim.Skeleton.data.animations);
        });

        SetState(_data.state);
    }

    public void SetState(State _state)
    {
        data.state = _state;

        StopAllCoroutines();
        switch (_state)
        {
            case State.bought:
                goEquipped.SetActive(false);
                goTimer.SetActive(false);
                spBackGround.spriteName = "SK_BOARD_02";
                spBackGround.GrayScale(false);
                btEquip.normalSprite = btPreview.normalSprite = CommonType.BTN_2_NORMAL;
                btEquip.pressedSprite = btPreview.pressedSprite = CommonType.BTN_2_ACTIVE;
                btPreview.tweenTarget = null;
                lbPreviewBt.text = "미리 보기";
                lbEquipBt.text = "적용";
                lbEquipBt.pivot = UIWidget.Pivot.Center;
                goPrice.SetActive(false);
                break;

            case State.Equipped:
                goEquipped.SetActive(true);
                goTimer.SetActive(false);
                spBackGround.spriteName = "SK_BOARD_02";
                spBackGround.GrayScale(false);
                btPreview.tweenTarget = btPreview.gameObject;
                btEquip.normalSprite = btPreview.normalSprite = CommonType.BTN_1_NORMAL;
                btEquip.pressedSprite = btPreview.pressedSprite = CommonType.BTN_1_ACTIVE;
                lbPreviewBt.text = "동작 보기";
                lbEquipBt.text = "해제";
                lbEquipBt.pivot = UIWidget.Pivot.Center;
                goPrice.SetActive(false);
                break;

            case State.NotBuy:
                goEquipped.SetActive(false);
                goTimer.SetActive(false);
                spBackGround.spriteName = "SK_BOARD_02";
                spBackGround.GrayScale(true);
                btEquip.normalSprite = btPreview.normalSprite = CommonType.BTN_5_NORMAL;
                btEquip.pressedSprite = btPreview.pressedSprite = CommonType.BTN_5_ACTIVE;
                btPreview.tweenTarget = null;
                lbPreviewBt.text = "미리 보기";
                lbEquipBt.text = "구매";
                lbEquipBt.pivot = UIWidget.Pivot.Left;
                goPrice.SetActive(true);
                break;

            case State.Special:
                goEquipped.SetActive(false);
                goTimer.SetActive(true);
                spBackGround.spriteName = "SK_BOARD_01";
                spBackGround.GrayScale(false);
                btEquip.normalSprite = btPreview.normalSprite = CommonType.BTN_3_NORMAL;
                btEquip.pressedSprite = btPreview.pressedSprite = CommonType.BTN_3_ACTIVE;
                btPreview.tweenTarget = null;
                lbPreviewBt.text = "미리 보기";
                lbEquipBt.text = "구매";
                lbEquipBt.pivot = UIWidget.Pivot.Left;
                goPrice.SetActive(true);
                StartCoroutine(CoTimer(timeLimit));
                break;
        }
    }

    internal State GetState()
    {
        return data.state;
    }

    internal Data GetData()
    {
        return data;
    }

    private void OnEnable()
    {
        if(data.state == State.Special)
            StartCoroutine(CoTimer(timeLimit));
    }

    string GetPriceItemSpriteName(int _key)
    {
        switch(_key)
        {
            case 3000001: return "ICON_MONEY_02";
            case 3000002: return "ICON_MONEY_03";
        }
        return "";
    }

    IEnumerator CoTimer(System.DateTime _timeLimit)
    {
        var totalSec = (_timeLimit - GameCore.nowTime).TotalSeconds;
        var h = totalSec / 3600;
        var m = (totalSec / 60) % 60;
        var s = totalSec % 60;

        while (h + m + s < 0)
        {
            lbTimer.text = string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
            yield return null;

            if(--s < 0)
            {
                s = 59;
                if(--m < 0)
                {
                    m = 59;
                    if (--h < 0)
                        break;
                }
            }
        }

        if (cbTimeOut != null)
            cbTimeOut();
    }

    public void OnClickPreView()
    {
        var sp = btPreview.GetComponent<UISprite>();
        switch (data.state)
        {
            case State.bought:
                // Toggle Illust
                cbPreview(this);
                break;

            case State.Equipped:
                // PlayAnimation
                spine.skelAnim.AnimationState.SetAnimation(0, animationList[Random.Range(0, animationList.Count)], false);
                spine.skelAnim.AnimationState.AddAnimation(0, "Idle", true, 0);
                break;

            case State.NotBuy:
                // Toggle Illust
                cbPreview(this);
                break;

            case State.Special:
                //  Toggle Illust
                cbPreview(this);
                break;
        }

        UpdatePreviewButton();
    }

    public void UpdatePreviewButton()
    {
        var sp = btPreview.GetComponent<UISprite>();
        switch (data.state)
        {
            case State.bought: sp.spriteName = bPreview ? CommonType.BTN_2_ACTIVE : CommonType.BTN_2_NORMAL; break;
            case State.Equipped: break;
            case State.NotBuy: sp.spriteName = bPreview ? CommonType.BTN_5_ACTIVE : CommonType.BTN_5_NORMAL; break;
            case State.Special: sp.spriteName = bPreview ? CommonType.BTN_3_ACTIVE : CommonType.BTN_3_NORMAL; break;
        }
    }

    public void OnClickPrice()
    {
        switch (data.state)
        {
            case State.bought:
                break;

            case State.Equipped:
                break;

            case State.NotBuy:
                break;

            case State.Special:
                break;
        }
    }


}
