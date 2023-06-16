using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

internal class ItemUpUI : MonoBehaviour, IComparer<CardBase>
{
    public class FocusedItemData
    {
        public ItemSData ServerData;
        public ItemDataMap LocalData;
    }

    [Serializable]
    public class Slot
    {
        public Transform uiSlotTransform;
        public GameObject goEquipEffect;
        public CardBase targetCard { get; private set; }

        public void PutInSlot(CardBase card) { targetCard = card; }
        public void EmptySlot() { targetCard = null; }

        public void UpdateSlot()
        {
            if (targetCard = null) return;
            targetCard.transform.SetParent(uiSlotTransform);
            targetCard.transform.localPosition = Vector3.zero;
        }
    }

    public static ItemUpUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("ItemUp/ItemUpUI", _parent);
        return go.GetComponent<ItemUpUI>();
    }

    [Header("Effect")]
    [SerializeField] GameObject goLevelUpEffect;
    [SerializeField] GameObject goMaxGaugeEffect;
    [SerializeField] GameObject goButtonEffect;

    [SerializeField] Transform[] slots;

    [Header("Slot")] public Slot[] SlotList;

    [Header("Info")]
    [SerializeField] UISprite spIllust;
    [SerializeField] CardInfoLabel cardInfo;

    [Header("Card List")]
    [SerializeField] GameObject goHaedGuide;
    [SerializeField] GameObject goFilterButton;
    [SerializeField] GameObject goFilterReset;
    [SerializeField] GameObject listRoot;

    [Header("Bottom")]
    [SerializeField] UILabel lbPower;
    [SerializeField] UILabel lbAddPower;

    [SerializeField] UISlider sdMiddle;
    [SerializeField] UISlider sdFore;
    [SerializeField] UILabel lbSlider;

    [SerializeField] GameObject goPrevEvol;
    [SerializeField] UILabel lbPrevEvol;
    [SerializeField] UILabel lbPrevUpgrade;
    [SerializeField] GameObject goNextEvol;
    [SerializeField] UILabel lbNextEvol;
    [SerializeField] UILabel lbNextUpgrade;

    [SerializeField] UILabel lbUpButton;
    [SerializeField] GameObject goPriceRoot;
    [SerializeField] UILabel lbPrice;
    [SerializeField] UISprite spPriceIcon;


    GameObject filterObj;
    ItemList list;
    public FocusedItemData FocusedItem { get; private set; }
    List<CardBase> userUpgradeSlotList = new List<CardBase>();

    //정렬관련 변수.
    private UIButton btnAlignment;
    private GameObject alignmentBox;

    GameObject[] btAlignmentType;

    UILabel[] alignmentTypeText;                        //정렬 타입 텍스트.
    UILabel[] alignmentIconText;                        //정렬 방식 텍스트.

    private List<Collider2D> ColliderList = new List<Collider2D>();


    float subValue = 0f;
    float addValue = 0f;
    float maxValue = 0f;
    int maxEnchant = -1;

    float prevForeGauge;
    float tgForeGauge;
    float accFore = 0f;

    internal void Init(ItemSData _data)
    {
        FocusedItem = new FocusedItemData();
        FocusedItem.ServerData = _data;
        FocusedItem.LocalData = GameCore.Instance.PlayerDataMgr.GetItemData(FocusedItem.ServerData.uid);

        UpdateItemInfo(false, true);
        GameCore.Instance.SetUISprite(spIllust, FocusedItem.LocalData.GetIliustSpriteKey());

        UpdatePower();

        // Set CardList
        list = new ItemList(listRoot, InvenUI.TypeFlag.Character, CardType.Character, OnClickList, OnPressList, CBCardComparer, CBDoFilter);
        var ids = GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.Equipment);

        // Create Cards
        for (int i = 0; i < ids.Length; ++i)
        {
            if (ids[i] == -1)
                continue;

            var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(ids[i]);
            // 목록 제외
            if (sdata.equipHeroUID > 0 ||     // 누군가 장착중
                _data.uid == sdata.uid ||     // 현재 카드
                _data.locked)                 // 잠긴 카드
                continue;

            var card = CardBase.CreateBigCard(sdata, listRoot.transform);// callback은 ItemList에서 설정한다.
            card.SetState(CardBase.States.Normal, true);
            
            list.AddItem(card.transform, ids[i], card);
        }

        btnAlignment = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_Range");
        btnAlignment.onClick.Add(new EventDelegate(CBClickAlignmentButtom));
        alignmentBox = UnityCommonFunc.GetGameObjectByName(gameObject, "AlignmentBox");
        btAlignmentType = new GameObject[8];

        btAlignmentType[0] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardRankDown");
        btAlignmentType[1] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardRankUp");
        btAlignmentType[2] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardNameDown");
        btAlignmentType[3] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardNameUp");
        btAlignmentType[4] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "GetCardDown");
        btAlignmentType[5] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "GetCardUp");
        btAlignmentType[6] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardPowerDown");
        btAlignmentType[7] = UnityCommonFunc.GetGameObjectByName(alignmentBox, "CardPowerUp");

        btAlignmentType[0].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByRankDescending));
        btAlignmentType[1].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByRankAscending));
        btAlignmentType[2].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByNameDescending));
        btAlignmentType[3].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByNameAscending));
        btAlignmentType[4].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByGetDescending));
        btAlignmentType[5].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByGetAscending));
        btAlignmentType[6].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByPowerDescending));
        btAlignmentType[7].GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSortByPowerAscending));

        alignmentTypeText = new UILabel[8];
        alignmentIconText = new UILabel[8];
        for (int i = 0; i < btAlignmentType.Length; ++i)
        {
            alignmentTypeText[i] = btAlignmentType[i].GetComponentsInChildren<UILabel>()[0];
            alignmentIconText[i] = btAlignmentType[i].GetComponentsInChildren<UILabel>()[1];
        }
        alignmentTypeText[0].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        alignmentIconText[0].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        var Colls = alignmentBox.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < Colls.Length; ++i)
        {
            ColliderList.Add(Colls[i]);
        }
        var btnAlignmentColls = btnAlignment.GetComponent<Collider2D>();
        ColliderList.Add(btnAlignmentColls);
    }

    internal CardBase[] GetOnSlotCards()
    {
        return userUpgradeSlotList.ToArray();
    }

    internal void RemoveAllCardInSlot()
    {
        SetAddableValue(-addValue);
        for (int i = 0; i < userUpgradeSlotList.Count; ++i)
        {
            list.RemoveItem(userUpgradeSlotList[i].SData.uid);
            Destroy(userUpgradeSlotList[i].gameObject);
        }
        userUpgradeSlotList.Clear();
        //list.Reposition(false);
    }

    public void UpdateCardListHeight()
    {
        if (gameObject.transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<SpringPanel>() != null &&
           list.ItemCardListHeight() > 130 &&
           (gameObject.transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<UIPanel>().clipOffset.y * (-1)) > list.ItemCardListHeight())
        {
            list.Reposition(false, false, true);
            gameObject.transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<SpringPanel>().enabled = true;
        }
    }


    public void UpdateItemInfo(bool _strengthen, bool _force = false)
    {
        FocusedItem.LocalData = GameCore.Instance.PlayerDataMgr.GetItemData(FocusedItem.ServerData.uid);
        maxEnchant = GameCore.Instance.DataMgr.GetMaxItemStrengthenLevel(FocusedItem.LocalData.rank+10);

        subValue = GameCore.Instance.DataMgr.GetItemStrengthenCostData(FocusedItem.LocalData.rank + 10, FocusedItem.ServerData.enchant).costExp;
        maxValue = (FocusedItem.ServerData.enchant == maxEnchant) ? subValue : GameCore.Instance.DataMgr.GetItemStrengthenCostData(FocusedItem.LocalData.rank + 10, FocusedItem.ServerData.enchant+1).costExp;
        addValue = 0f;
        UpdateSlider(_force);

        cardInfo.Init(FocusedItem.ServerData);

        goLevelUpEffect.SetActive(_strengthen);
    }

    private void UpdateSlider(bool _force = false)
    {
        var now = (FocusedItem.ServerData.exp - subValue) / (maxValue - subValue);
        var add = addValue / (maxValue - subValue);

        if ((maxValue - subValue) == 0)
        {
            now = 1f;
            add = 0f;
        }

        // 게이지 애니메이션 용 변수 설정
        if (_force) prevForeGauge = sdFore.value = now;
        else        prevForeGauge = sdFore.value;
        accFore = 0f;
        tgForeGauge = now;
        sdMiddle.value = now + add;


        if (now >= 1f)          lbSlider.text = "MAX";
        else if (addValue == 0) lbSlider.text = string.Format("{0:0.#}%", now * 100f);
        else                    lbSlider.text = string.Format("{0:0.#}%(+{1:0.#}%)", (now + add) * 100f, add * 100f);

        goMaxGaugeEffect.SetActive(now >= 1f);
        SetNextUpgradeLabel(FocusedItem.LocalData.rank, FocusedItem.ServerData.enchant);
    }

    private bool CheckExeption(long uid, bool useNotify = true)
    {

        long upItemUID = FocusedItem.ServerData.uid;
        int upItemRank = GameCore.Instance.DataMgr.GetItemData((GameCore.Instance.PlayerDataMgr.GetItemData(upItemUID).id)).rank;

        long selectItemUID = uid;
        int selectItemRank = GameCore.Instance.DataMgr.GetItemData((GameCore.Instance.PlayerDataMgr.GetItemData(selectItemUID).id)).rank;

        int selectItemEnchant = GameCore.Instance.PlayerDataMgr.GetItemSData(selectItemUID).enchant;
        int selectItemExp = GameCore.Instance.PlayerDataMgr.GetItemSData(selectItemUID).exp;

        var card = list.GetItem(uid);

        var nowExp = FocusedItem.ServerData.exp + addValue;

        if(FocusedItem.ServerData.exp > maxValue)
        {
            if(useNotify) GameCore.Instance.ShowNotice("경험치 변환", "강화/진화/각성을 할 때에는\n재료가 필요하지 않습니다.", 0);
            return false;
        }
        if(nowExp > maxValue)
        {
            if(useNotify) GameCore.Instance.ShowNotice("경험치 초과", "더 이상 경험치를 획득할 수 없습니다.", 0);
            return false;
        }

        if(upItemRank > selectItemRank) { if (useNotify) ShowUsingWarning(card); return false; }
        if (0 < selectItemEnchant) { if (useNotify) ShowUsingWarning(card); return false; }
        if (0 < selectItemExp) { if (useNotify) ShowUsingWarning(card); return false; }
        return true;
    }


    private void ShowUsingWarning(CardBase card)
    {
        GameCore.Instance.ShowAgree("사용 주의", "현재 카드보다 높은 등급이거나 성장의 흔적이 있는\n 영웅을 재료로 사용합니다. 계속하시겠습니까?", 0, () =>
        {
            GameCore.Instance.CloseMsgWindow();

            card.SetSelect(CardBase.SelectState.Select);
            var item = CardBase.CreateSmallCard(card.SData, SlotList[userUpgradeSlotList.Count].uiSlotTransform, 
                                                SelectCard, (uid) => GameCore.Instance.ShowCardInfo(GameCore.Instance.PlayerDataMgr.GetItemSData(uid)));
            userUpgradeSlotList.Add(item);
            SetAddableValue(GetCardValue(item));

            SlotList[userUpgradeSlotList.Count - 1].goEquipEffect.SetActive(false);
            SlotList[userUpgradeSlotList.Count - 1].goEquipEffect.SetActive(true);
        });
    }

    private void SetNextUpgradeLabel(int _evolLvl, int _enchant)
    {
        bool bNext = tgForeGauge >= 1f;

        if (maxEnchant > _enchant) // 강화단계가 남은 경우
        {
            goPrevEvol.SetActive(false);
            lbPrevUpgrade.text = string.Format("+{0}", _enchant);
            goNextEvol.SetActive(false);
            lbNextUpgrade.text = string.Format("+{0}", _enchant + ((bNext) ? 1 : 0));

            var gap = 0;
            if (FocusedItem.ServerData.exp >= maxValue)
                //gap = itemSData.GetPower(false, null, itemSData.enchant + 1) - itemSData.GetPower(false);
                gap = FocusedItem.ServerData.GetPower(false, null, 1) - FocusedItem.ServerData.GetPower(false);

            UpdatePower(gap);

            SetUpgradeButton(bNext, _enchant + 1);
        }
        else // 최종 강화가 모두 완료되었을 경우
        {
            goPrevEvol.SetActive(false);
            lbPrevEvol.text = _evolLvl.ToString();
            lbPrevUpgrade.text = string.Format("+{0}", _enchant);

            goNextEvol.SetActive(false);
            lbNextEvol.text = _evolLvl.ToString();
            lbNextUpgrade.text = string.Format("+{0}", _enchant);

            lbUpButton.text = "강화불가";
            lbUpButton.alignment = NGUIText.Alignment.Center;
            goPriceRoot.SetActive(false);
            goButtonEffect.SetActive(false);
            goMaxGaugeEffect.SetActive(false);
        }

        lbNextUpgrade.color = bNext ? (Color)CommonType.COLOR_01 : Color.white;
        lbNextEvol.color = bNext ? (Color)CommonType.COLOR_01 : Color.white;
    }

    private void SetUpgradeButton(bool _next, int _enchant)
    {
        if (!_next)
        {
            lbUpButton.text = "경험치 변환";
            lbUpButton.alignment = NGUIText.Alignment.Center;
            goPriceRoot.SetActive(false);
            goButtonEffect.SetActive(userUpgradeSlotList.Count != 0);
        }
        else
        {
            lbUpButton.text = "강화";
            lbUpButton.alignment = NGUIText.Alignment.Left;
            goPriceRoot.SetActive(true);

            var data = GameCore.Instance.DataMgr.GetItemStrengthenCostData(FocusedItem.LocalData.rank+10, _enchant);
            lbPrice.text = string.Format("{0:N0}", data.costGold);
            spPriceIcon.spriteName = "ICON_MONEY_02";

            goButtonEffect.SetActive(true);
        }
    }

    private void SelectCard(long _uid)
    {

        long upEquipUID = FocusedItem.ServerData.uid;
        int upEquipRank = GameCore.Instance.DataMgr.GetItemData((GameCore.Instance.PlayerDataMgr.GetItemData(upEquipUID).id)).rank;

        long selectEquipUID = _uid;
        int selectEquipRank = GameCore.Instance.DataMgr.GetItemData((GameCore.Instance.PlayerDataMgr.GetItemData(selectEquipUID).id)).rank;
        
        int selsctEquipEnchant = GameCore.Instance.PlayerDataMgr.GetItemSData(selectEquipUID).enchant;
        int selectEquipExp = GameCore.Instance.PlayerDataMgr.GetItemSData(selectEquipUID).exp;

        if (sdFore.value >= 1f)
        {
            GameCore.Instance.ShowNotice("경험치 변환", "강화를 할 때에는\n재료가 필요하지 않습니다.", 0);
            return;
        }
            
        var card = list.GetItem(_uid);
       
        for(int i = 0; i < userUpgradeSlotList.Count; ++i)
        {
            if (userUpgradeSlotList[i].ID == _uid)  // Unselect
            {
                card.SetSelect(CardBase.SelectState.None);
                RemoveSlot(i);
                return;
            }
        }

        float nowExp = FocusedItem.ServerData.exp + addValue;

        if (nowExp >= maxValue)
        {
            GameCore.Instance.ShowNotice("경험치 초과", "더 이상 경험치를 획득할 수 없습니다.", 0);
            return;
        }
        if (userUpgradeSlotList.Count == 10) { GameCore.Instance.ShowNotice("자동 선택 불가", "재료 슬롯이 가득 찼습니다.", 0); return; }

        if (!(upEquipRank > selectEquipRank || 0 < selsctEquipEnchant || 0 < selectEquipExp))
        {
  
            if (userUpgradeSlotList.Count >= SlotList.Length) // Max Select Count
                return;

            // Select
            card.SetSelect(CardBase.SelectState.Select);
            var item = CardBase.CreateSmallCard(card.SData, SlotList[userUpgradeSlotList.Count].uiSlotTransform, SelectCard, (uid) => GameCore.Instance.ShowCardInfo(GameCore.Instance.PlayerDataMgr.GetItemSData(uid)));
            userUpgradeSlotList.Add(item);
            SetAddableValue(GetCardValue(item));
            SlotList[userUpgradeSlotList.Count - 1].goEquipEffect.SetActive(false);
            SlotList[userUpgradeSlotList.Count - 1].goEquipEffect.SetActive(true);
        }
        else { CheckExeption(_uid); }
    }

    private void RemoveSlot(int _idx)
    {
        SetAddableValue(-GetCardValue(userUpgradeSlotList[_idx]));
        Destroy(userUpgradeSlotList[_idx].gameObject);

        for (; _idx < userUpgradeSlotList.Count-1; ++_idx)
        {
            userUpgradeSlotList[_idx] = userUpgradeSlotList[_idx + 1];

            var tf = userUpgradeSlotList[_idx].transform;
            tf.parent = SlotList[_idx].uiSlotTransform;
            tf.localPosition = Vector3.zero;
        }

        userUpgradeSlotList.RemoveAt(userUpgradeSlotList.Count - 1);
    }

    private void UpdatePower(int _add = 0)
    {
        if (0 < _add)       lbAddPower.text = string.Format("[c][00F0FF]+{0:N0}",  _add);
        else if(0 > _add)   lbAddPower.text = string.Format("[c][FE0000]-{0:N0}", -_add);
        else                lbAddPower.text = string.Empty;
        lbPower.text = string.Format("{0:N0}", FocusedItem.ServerData.GetPower(false));
    }

    private void ShowResetFilter(bool _show)
    {
        goFilterReset.gameObject.SetActive(_show);
    }

    #region UnityUIDekegate

    public void OnClickResetFilter()
    {
        list.DoFilter(0);
        filterObj.GetComponent<FilterCtrl>().Reset();
        ShowResetFilter(false);
    }

    public void OnClickFilter()
    {
        if (filterObj == null)
            filterObj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("CommonRsc/prefab/filter_root_item", transform);
        
        filterObj.GetComponent<FilterCtrl>().SetPrevFilter();
        GameCore.Instance.ShowObject("필터", null, filterObj, 2, new MsgAlertBtnData[] {
            new MsgAlertBtnData("완료", new EventDelegate(() => {
                var ctrl = filterObj.GetComponent<FilterCtrl>();
                int filter = ctrl.GetFilter();
                list.DoFilter(filter);
                ShowResetFilter( filter != 0 );
                filterObj.transform.parent = transform;
                filterObj.SetActive(false);
                GameCore.Instance.CloseMsgWindow();
            }), true, null, SFX.Sfx_UI_Confirm)
		});
    }

    public void OnClickAutoSelect()
    {
        if(FocusedItem.ServerData.exp >= maxValue)
        {
            GameCore.Instance.ShowNotice("경험치 변환", "강화를 할 때에는\n 재료가 필요하지 않습니다.", 0);
            return;
        }
        var queue = new PriorityQueue<CardBase>(this);

        var iter = list.GetEnumerator();
        while (iter.MoveNext())
        {
            var card = iter.Current.Value;
            if (card.gameObject.activeInHierarchy == true &&                        // 필터링 안된 카드
                CheckExeption(card.SData.uid, false) &&
                card.Select == CardBase.SelectState.None &&                         // 선택 안된 카드
                ((ItemSData)card.SData).equipHeroUID <= 0 &&                        // 영웅인 장착하고 있지 않은 카드
                GameCore.Instance.DataMgr.GetItemData(card.Data.id).rank > 1)    // 랭크가 SS 미만인 카드
                queue.Push(iter.Current.Value);
        }
        if (FocusedItem.ServerData.exp + addValue >= maxValue) { GameCore.Instance.ShowNotice("경험치 초과", "더 이상 경험치를 획득할 수 없습니다.", 0); return; }
        if (queue.Count < 1) { GameCore.Instance.ShowNotice("자동 선택 불가", "선택 가능한 카드가 없습니다.", 0); return; }
        if (userUpgradeSlotList.Count == 10) { GameCore.Instance.ShowNotice("자동 선택 불가", "재료 슬롯이 가득 찼습니다.", 0); return; }
        while (queue.Count != 0 && userUpgradeSlotList.Count < 10)
        {
            if (FocusedItem.ServerData.exp + addValue >= maxValue) break;

            var card = queue.Pop();
            SelectCard(card.ID);
            
        }
    }

    public void OnClickUnselectAll()
    {
        while (userUpgradeSlotList.Count != 0)
        {
            var card = list.GetItem(userUpgradeSlotList[userUpgradeSlotList.Count - 1].ID);
            card.SetSelect(CardBase.SelectState.None);
            RemoveSlot(userUpgradeSlotList.Count - 1);
        }
    }

    public void OnClickUpgrade()
    {
        if(sdFore.value < 1f) // 강화하기 중
        {
            if (userUpgradeSlotList.Count == 0)
            {
                GameCore.Instance.ShowAlert("재료가 되는 카드들을 선택해주세요.");
            }
            else if ((FocusedItem.ServerData.exp + addValue) > maxValue)
            {
                GameCore.Instance.ShowAgree("경험치 초과", "100%를 넘은 경험치는 모두 사라집니다.\n그래도 강화하시겠습니까?", 0, () =>
                {
                    GameCore.Instance.CloseMsgWindow();
                    GameCore.Instance.NetMgr.Req_Item_Strengthen_Exp(FocusedItem.ServerData.uid, userUpgradeSlotList.ToArray());
                });
            }
            else
            {
                GameCore.Instance.NetMgr.Req_Item_Strengthen_Exp(FocusedItem.ServerData.uid, userUpgradeSlotList.ToArray());
            }
        }
        else 
        {
            if (CheckCost(FocusedItem.ServerData.enchant + 1, true))
                GameCore.Instance.NetMgr.Req_Item_Strengthen(FocusedItem.ServerData.uid);// Do Upgrade
        }
    }

    public bool CheckCost(int _enchant, bool _showPopup = false)
    {
        var data = GameCore.Instance.DataMgr.GetItemStrengthenCostData(FocusedItem.LocalData.originalRank, _enchant);
        if (data == null)
            return false;

        ResourceType type = ResourceType.Gold;
        int cost = data.costGold;

        var result = cost <= GameCore.Instance.PlayerDataMgr.GetReousrceCount(type);

        if (!result && _showPopup)
            GameCore.Instance.ShowReduceResource(type);

        return result;
    }


    private void OnClickList(long _uid)
    {
        SelectCard(_uid);
    }

    private void OnPressList(long _uid)
    {
        GameCore.Instance.ShowCardInfo(GameCore.Instance.PlayerDataMgr.GetItemSData(_uid));
    }

    #endregion

    #region "정체불명의 메서드"

    private float GetCardValue(CardBase _card)
    {
        var data = (ItemDataMap)_card.Data;
        return data.exp;
    }

    private void SetAddableValue(float _addedValue)
    {
        addValue += _addedValue;
        //sdMiddle.value = sdFore.value + addValue;
        UpdateSlider();
    }

    private bool CBDoFilter(CardBase _card, int _filter)
    {
        // Filtering
        int rank = FilterCtrl.GetFilterRank(_filter);
        int type = FilterCtrl.GetFilterType(_filter);
        int rare = FilterCtrl.GetFilterRare(_filter);

        var data = (ItemDataMap)_card.Data;

        return (rank == 0 || (1 << (4 - data.rank) & rank) != 0) &&
               (type == 0 || (1 << (data.equipLimit - 1) & type) != 0);//&&
                                                                       //(rare == 0 || (1 << (data.evolLvl - 1) & rare) != 0);
    }

    private int CBCardComparer(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;
        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }

    public int Compare(CardBase _1, CardBase _2)
    {
        var x = GameCore.Instance.PlayerDataMgr.GetItemData(_1.ID);
        var y = GameCore.Instance.PlayerDataMgr.GetItemData(_2.ID);

        if (x.rank != y.rank)
            return x.rank < y.rank ? 1 : -1;
        var sx = GameCore.Instance.PlayerDataMgr.GetItemSData(_1.ID);
        var sy = GameCore.Instance.PlayerDataMgr.GetItemSData(_2.ID);

        if (sx.enchant != sy.enchant)
            return sx.enchant > sy.enchant ? 1 : -1;

        return ((ItemSData)_1.SData).GetPower(false).CompareTo(((ItemSData)_2.SData).GetPower(false));
    }

    #endregion


    #region 정렬 메서드
    //정렬 버튼 활성화 함수.
    private void CBClickAlignmentButtom()
    {
        if (!alignmentBox.activeSelf) alignmentBox.SetActive(true);
        else alignmentBox.SetActive(false);
    }
    //랭크기준 내림차순 정렬
    public void OnClickSortByRankDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(0);
        list.SetSortCallBack(ItemSortByRankDescending);
        if (filterObj == null) list.DoFilter(31);
        else list.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //랭크기준 오름차순 정렬
    public void OnClickSortByRankAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(1);
        list.SetSortCallBack(ItemSortByRankAscending);
        if (filterObj == null) list.DoFilter(31);
        else list.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 내림차순 정렬
    public void OnClickSortByNameDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(2);
        list.SetSortCallBack(ItemSortByNameDescending);
        if (filterObj == null) list.DoFilter(31);
        else list.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 오름차순 정렬
    public void OnClickSortByNameAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(3);
        list.SetSortCallBack(ItemSortByNameAscending);
        if (filterObj == null) list.DoFilter(31);
        else list.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 내림차순 정렬
    public void OnClickSortByGetDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(4);
        list.SetSortCallBack(ItemSortByGetDescending);
        if (filterObj == null) list.DoFilter(31);
        else list.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 오름차순 정렬
    public void OnClickSortByGetAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(5);
        list.SetSortCallBack(ItemSortByGetAscending);
        if (filterObj == null) list.DoFilter(31);
        else list.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 내림차순 정렬
    public void OnClickSortByPowerDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(6);
        list.SetSortCallBack(ItemSortByPowerDescending);
        if (filterObj == null) list.DoFilter(31);
        else list.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 오름차순 정렬
    public void OnClickSortByPowerAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(7);
        list.SetSortCallBack(ItemSortByNameAscending);
        if (filterObj == null) list.DoFilter(31);
        else list.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }

    internal void UpdateAlignmentButton(int selectAlignmentType)
    {
        btnAlignment.GetComponentsInChildren<UILabel>()[0].text = alignmentTypeText[selectAlignmentType].text;
        btnAlignment.GetComponentsInChildren<UILabel>()[1].text = alignmentIconText[selectAlignmentType].text;

        for (int i = 0; i < btAlignmentType.Length; ++i)
        {
            if (i == selectAlignmentType)
            {
                alignmentTypeText[i].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
                alignmentIconText[i].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
            }
            else
            {
                alignmentIconText[i].color = Color.white;
                alignmentTypeText[i].color = Color.white;
            }
        }
    }

    #region 아이템 정렬방식.
    //아이템 랭크 내림차순 정렬
    private int ItemSortByRankDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;
        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 랭크 오름차순 정렬
    private int ItemSortByRankAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank < data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;
        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 공격력 내림차순 정렬
    private int ItemSortByPowerDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        if (sdata1.GetPower(false) != sdata2.GetPower(false))
        {
            return sdata1.GetPower(false) < sdata2.GetPower(false) ? 1 : -1;
        }

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 공격력 오름차순 정렬
    private int ItemSortByPowerAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        if (sdata1.GetPower(false) != sdata2.GetPower(false))
        {
            return sdata1.GetPower(false) > sdata2.GetPower(false) ? 1 : -1;
        }

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 이름 내림차순 정렬
    private int ItemSortByNameDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        List<string> testItemNameSort = new List<string>();
        testItemNameSort.Add(data1.name);
        testItemNameSort.Add(data2.name);
        testItemNameSort.Sort();
        if (testItemNameSort[0] != testItemNameSort[1])
            return testItemNameSort[0] == data1.name ? 1 : -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);

    }
    //아이템 이름 오름차순 정렬
    private int ItemSortByNameAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        List<string> testItemNameSort = new List<string>();
        testItemNameSort.Add(data1.name);
        testItemNameSort.Add(data2.name);
        testItemNameSort.Sort();
        testItemNameSort.Reverse();

        if (testItemNameSort[0] != testItemNameSort[1])
            return testItemNameSort[0] == data1.name ? 1 : -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 획득 내림차순 정렬
    private int ItemSortByGetDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        return sdata1.uid < sdata2.uid ? 1 : -1;
    }
    //아이템 획득 오름차순 정렬
    private int ItemSortByGetAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        return sdata1.uid > sdata2.uid ? 1 : -1;

    }
    #endregion
    #endregion
    private void Update()
    {
        UpdateGauge();

#if UNITY_EDITOR

        if (Input.GetMouseButtonDown(0))
        {
            if (alignmentBox.activeInHierarchy)
            {

                Ray ray = GameCore.Instance.GetUICam().ScreenPointToRay(Input.mousePosition);
                RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 99999, LayerMask.GetMask("UI"));



                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; ++i)
                    {
                        if (ColliderList.Contains(hits[i].collider)) return;
                    }
                }
                alignmentBox.SetActive(false);
            }
        }
#endif
        if (Input.touchCount == 0) return;

        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (alignmentBox.activeInHierarchy)
            {

                Ray ray = GameCore.Instance.GetUICam().ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 99999, LayerMask.GetMask("UI"));



                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; ++i)
                    {
                        if (ColliderList.Contains(hits[i].collider)) return;
                    }
                }
                alignmentBox.SetActive(false);
            }
        }
    }


    void UpdateGauge()
    {
        accFore = Mathf.Min(1f, accFore + Time.deltaTime);
        sdFore.value = Mathf.Lerp(sdFore.value, tgForeGauge, accFore);
    }
}
