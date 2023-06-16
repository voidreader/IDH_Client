using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

internal class HeroUpUI : MonoBehaviour, ISequenceTransform//, IComparer<CardBase>
{
    public class FocusedHeroData
    {
        public  HeroSData ServerData;
        public  UnitDataMap LocalData;
    }

    [Serializable]
    public class Slot
    {
        public Transform uiSlotTransform;
        public GameObject goEquipEffect;
        public CardBase targetCard { get; private set; }

        public void PutInSlot(CardBase card) { targetCard = card; }
        public void EmptySlot() { targetCard = null;  }

        public void UpdateSlot()
        {
            if (targetCard == null) return;
            targetCard.transform.SetParent(uiSlotTransform);
            targetCard.transform.localPosition = Vector3.zero;
        }
    }


    public static HeroUpUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("HeroUp/HeroUpUI", _parent);
        return go.GetComponent<HeroUpUI>();
    }

    //HeroUpUI 상단 텍스트 출력 text
    static string[] strInfo = new string[]
    {
        "100%를 초과한 경험치는 버려집니다.",
        "경험치 변환 시 장비는 해제됩니다."
    };

    static int strInfoIdx = 0;

    [Header("Effect")]
    [SerializeField] GameObject goLevelUpEffect;
    [SerializeField] GameObject goEvolutionEffect;
    [SerializeField] GameObject goMaxGaugeEffect;
    [SerializeField] GameObject goButtonEffect;
    [SerializeField] GameObject goStarEffect;
    [SerializeField] UISprite spStarEffect;

    [Header("Slot")]    public Slot[] SlotList;

    [Header("Info")]
    [SerializeField]    UI2DSprite sp2dIllust;
    [SerializeField]    CardInfoLabel cardInfo;

    [Header("Card List")]
    [SerializeField]    UILabel lbGuideHead;
    [SerializeField]    GameObject goFilterButton;
    [SerializeField]    GameObject goFilterReset;
    [SerializeField]    GameObject listRoot;

    [Header("Bottom")]
    [SerializeField]    UILabel lbPower;
    [SerializeField]    UILabel lbAddPower;

    [SerializeField]    UISlider sdMiddle;
    [SerializeField]    UISlider sdFore;
    [SerializeField]    UILabel lbSlider;

    [SerializeField]    GameObject goPrevEvol;
    [SerializeField]    UILabel lbPrevEvol;
    [SerializeField]    UILabel lbPrevUpgrade;
    [SerializeField]    GameObject goNextEvol;
    [SerializeField]    UILabel lbNextEvol;
    [SerializeField]    UILabel lbNextUpgrade;

    [SerializeField]    UILabel lbUpButton;
    [SerializeField]    GameObject goPriceRoot;
    [SerializeField]    UILabel lbPrice;
    [SerializeField]    UISprite spPriceIcon;

    [SerializeField] UIButton btUp;

    private Coroutine coUpdateInfoMessege;
    private GameObject filterObj;

    public FocusedHeroData FocusedHero { get; private set; }
    private ItemList inventoryItemList;
    
    private  List<CardBase> userUpgradeSlotList = new List<CardBase>();

    //정렬관련 변수.
    private UIButton btnAlignment;
    private GameObject alignmentBox;

    GameObject[] btAlignmentType;

    UILabel[] alignmentTypeText;                        //정렬 타입 텍스트.
    UILabel[] alignmentIconText;                        //정렬 방식 텍스트.

    private List<Collider2D> ColliderList = new List<Collider2D>();

    bool maxEvol = false;

    int filterFlag = 0;

    float subValue = 0f;
    float addValue = 0f;
    float maxValue = 0f;

    float prevForeGauge;
    float tgForeGauge;
    float accFore; // 누적 계산

    const int LastLevel = 5;
    const int AwakeningLevel = 5;
    public CardBase[] GetOnSlotCards() { return userUpgradeSlotList.ToArray(); }

    public Transform GetInventoryItem(int pos)
    {
        var iter = inventoryItemList.tfTable.GetEnumerator();
        iter.MoveNext();
        var best = iter.Current.Key;
        while (iter.MoveNext())
        {
            var now = iter.Current.Key;
            if (CBCardComparer(best, now) > 0)
                best = now;
        }

        return best;

        //Transform inventoryTransform;
        //foreach(var inventoryItem in inventoryItemList.tfTable)
        //{
        //    inventoryTransform = inventoryItem.Key;
        //    return inventoryTransform;
        //}
        //return null;
    }

    public void Init(HeroSData _data)
    {
        FocusedHero = new FocusedHeroData();
        FocusedHero.ServerData = _data;
        FocusedHero.LocalData = GameCore.Instance.PlayerDataMgr.GetUnitData(FocusedHero.ServerData.uid);

        UpdateUnitInfo(false, false, true);
        GameCore.Instance.SetUISprite(sp2dIllust, FocusedHero.LocalData.GetIllustSpeiteKey());

        //상단 텍스트 바꾸는 출력을 위한 함수.
        SwichingText();

        UpdateInventoryCardList();

        DataMapCtrl<StrengthenCostDataMap> StrengthenCostDataMap = GameCore.Instance.DataMgr.GetDataList(DataMapType.StrengthenCost) as DataMapCtrl<StrengthenCostDataMap>;

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

    public void UpdateUnitInfo(bool _strengthen = false, bool _evolution = false, bool _force = false)
    {
        FocusedHero.LocalData = GameCore.Instance.PlayerDataMgr.GetUnitData(FocusedHero.ServerData.uid);

        var nextStrengthenData = GetNextStrengthenCostData(FocusedHero.LocalData.evolLvl, FocusedHero.ServerData.enchant);

        subValue = (nextStrengthenData != null) ? GameCore.Instance.DataMgr.GetStrengthenCostData(FocusedHero.LocalData.evolLvl, FocusedHero.ServerData.enchant).costExp : 0;
        maxValue = (nextStrengthenData != null) ? nextStrengthenData.costExp : 0;
        addValue = 0f;
        UpdateSlider(_force);

        goLevelUpEffect.SetActive(_strengthen);
        goEvolutionEffect.SetActive(_evolution);

        if (_evolution)
        {
            var sdata = FocusedHero.ServerData;
            var evol = GameCore.Instance.DataMgr.GetUnitData(sdata.key).evolLvl;
            GameCore.Instance.DoWaitCall(1f, () => ShowEvolutionStarEffect(evol));
            GameCore.Instance.DoWaitCall(2f, () => {
                cardInfo.Init(sdata);
            });
        }
        else
        {
            cardInfo.Init(FocusedHero.ServerData);
        }
    }

    #region "Slot Control Mehtod"

    private void AddCardAtSlot(CardBase card)
    {
        if (userUpgradeSlotList.Count > SlotList.Length)
            return;

        float nowExp = FocusedHero.ServerData.exp + addValue;

        if (nowExp >= maxValue)
            return;

        var list = GameCore.Instance.PlayerDataMgr.GetUserHeroSDataList();
        card.SetSelect(CardBase.SelectState.Select);
        
        var item = CardBase.CreateSmallCard(card.SData, SlotList[userUpgradeSlotList.Count].uiSlotTransform, OnClickSlotCard, null);
        SlotList[userUpgradeSlotList.Count].goEquipEffect.SetActive(false);
        SlotList[userUpgradeSlotList.Count].goEquipEffect.SetActive(true);

        foreach (var hero in list)
        {
            if (hero.IsExpCard())
                continue;

            if (hero.uid == item.SData.uid)
            {
                if (hero.GetEquipCount() != 0) item.SetInfo(CardBase.UnitInfo.Power | CardBase.UnitInfo.Equip);
                else                           item.SetInfo(CardBase.UnitInfo.Power);
                break;
            }
        }
        userUpgradeSlotList.Add(item); // userUpgradeSlotList.Count ++
        UpdateSingleSlot(userUpgradeSlotList.Count - 1);
        SetAddableValue(GetCardValue(card));

        goButtonEffect.SetActive(true);
    }

    public void ClearSlot()
    {
        for (int i = 0; i < userUpgradeSlotList.Count; ++i)
        {
            inventoryItemList.GetItem(userUpgradeSlotList[i].SData.uid).SetSelect(CardBase.SelectState.None);
            Destroy(userUpgradeSlotList[i].gameObject);
        }
        userUpgradeSlotList.Clear();
        UpdateAllSlot();
        inventoryItemList.OffSelectAll();
        //inventoryItemList.Reposition(false);

        SetAddableValue(-addValue);
    }

    public void UpdateCardListHeight()
    {
        if (gameObject.transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<SpringPanel>() != null &&
           inventoryItemList.CardListHeight() > 100 &&
           (gameObject.transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<UIPanel>().clipOffset.y * (-1)) > inventoryItemList.CardListHeight())
        {
            inventoryItemList.Reposition(false, true);
            gameObject.transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<SpringPanel>().enabled = true;
        }
    }

    public void RemoveAtSlot(long itemUid)
    {
        var smallCard = userUpgradeSlotList.Find(card => card.SData.uid == itemUid);
        if (smallCard == null) return;

        RemoveCardAtSlot(userUpgradeSlotList.IndexOf(smallCard));
    }

    private void RemoveCardAtSlot(int slotItemIdx)
    {
        if (userUpgradeSlotList.Count <= slotItemIdx) return;
        //var data = userUpgradeSlotList[slotItemIdx].Data as UnitDataMap;
        SetAddableValue(-(GetCardValue(userUpgradeSlotList[slotItemIdx])));// data.exp);

        Destroy(userUpgradeSlotList[slotItemIdx].gameObject);

        userUpgradeSlotList.RemoveAt(slotItemIdx);
        UpdateAllSlot();
    }

    private void UpdateSingleSlot(int idx)
    {
        SlotList[idx].EmptySlot();
        SlotList[idx].PutInSlot(userUpgradeSlotList[idx]);
        SlotList[idx].UpdateSlot();
    }

    private void UpdateAllSlot()
    {
        for (int i = 0; i < SlotList.Length; ++i) SlotList[i].EmptySlot();
        for (int i = 0; i < userUpgradeSlotList.Count; ++i) SlotList[i].PutInSlot(userUpgradeSlotList[i]);
        foreach (var slot in SlotList) slot.UpdateSlot();
    }

    #endregion

    #region "Inventory Control Method"

    private void SelectInventoryCard(long uid)
    {
        var card = inventoryItemList.GetItem(uid);
        //float nowExp = FocusedHero.ServerData.exp + addValue;
        inventoryItemList.SetSelect(uid);
        AddCardAtSlot(card);
           
    }

    private void UnSelectAtInventoryCard(long uid)
    {
        var card = inventoryItemList.GetItem(uid);
        RemoveAtSlot(uid);
        card.SetSelect(CardBase.SelectState.None);
    }

    private void RemoveInventoryItem(long uid)
    {
        inventoryItemList.RemoveItem(uid);
        Debug.Log(string.Format("Removed {0}", uid));
    }

    /// <summary>
    /// PlayerDataManager의 데이터를 활용하여 인벤토리 정보를 갱신
    /// </summary>
    public void UpdateInventoryCardList()
    {
        // Set CardList
        TeamSData userTeam = GameCore.Instance.PlayerDataMgr.UserTeam;
        long mainHeroUid = GameCore.Instance.PlayerDataMgr.MainCharacterUID;


        List<HeroSData> userHeroSDataList = GameCore.Instance.PlayerDataMgr.GetUserHeroSDataList();

        var selectedList = from hero in userHeroSDataList
                           where (!userTeam.IsHaveUnit(hero.uid)) &&
                                 (hero.dispatch == false) &&
                                 (hero.dormitory == 0) &&
                                 (hero.uid != FocusedHero.ServerData.uid) &&
                                 (hero.uid != mainHeroUid) &&
                                 (hero.locked == false)

                           select hero;

        if (inventoryItemList == null) inventoryItemList = new ItemList(listRoot, InvenUI.TypeFlag.Character, CardType.Character, OnClickInventoryCard, null, CBCardComparer, CBDoFilter);
        else inventoryItemList.RemoveAllItem();

        inventoryItemList.SetMutiSelectable(true);

        foreach (var hero in selectedList)
        {
            var card = CardBase.CreateBigCard(hero, listRoot.transform);// callback은 ItemList에서 설정한다.

            if (!hero.IsExpCard())
            { 
                if (hero.GetEquipCount() != 0) card.SetInfo(CardBase.UnitInfo.Power | CardBase.UnitInfo.Equip);
                else                           card.SetInfo(CardBase.UnitInfo.Power);
                card.SetState(CardBase.States.Normal, true);
            }

            inventoryItemList.AddItem(card.transform, hero.uid, card);
        }

        inventoryItemList.DoFilter(filterFlag);
    }

    #endregion


    private bool CheckExeption(long uid, bool useNotify = true)
    {
        long upUnitUID = FocusedHero.ServerData.uid;
        int upUnitRank = GameCore.Instance.DataMgr.GetUnitData((GameCore.Instance.PlayerDataMgr.GetUnitData(upUnitUID).id)).rank;
        long selectUnitUID = uid;
        int selectUnitRank = GameCore.Instance.DataMgr.GetUnitData((GameCore.Instance.PlayerDataMgr.GetUnitData(selectUnitUID).id)).rank;
        int selectUnitExp = GameCore.Instance.PlayerDataMgr.GetUnitSData(selectUnitUID).exp;
        int selectUnitEnchant = GameCore.Instance.PlayerDataMgr.GetUnitSData(selectUnitUID).enchant;
        var card = inventoryItemList.GetItem(uid);
        float nowExp = FocusedHero.ServerData.exp + addValue;

        int selectEvol = GameCore.Instance.PlayerDataMgr.GetUnitData(selectUnitUID).evolLvl;
        int defaultExp = GameCore.Instance.DataMgr.GetStrengthenCostData(selectEvol, 0).costExp;

        if (FocusedHero.ServerData.exp >= maxValue)
        {
            if (useNotify) GameCore.Instance.ShowNotice("경험치 변환", "강화/진화/각성을 할 때에는\n재료가 필요하지 않습니다.", 0);
            return false;
        }
        if (nowExp >= maxValue)
        {
            if (useNotify) GameCore.Instance.ShowNotice("경험치 초과", "더 이상 경험치를 획득할 수 없습니다.", 0);
            return false;
        }

        if (GameCore.Instance.PlayerDataMgr.GetUnitData(selectUnitUID).IsExpCard())
            return true;

        // Rank = 작은 수 일경우 더 큼
        if ((upUnitRank > selectUnitRank))          { if (useNotify) ShowUsingWarning(card); return false; }
        if(DefaultEvol(selectUnitUID) == 0)         { if (useNotify) ShowUsingWarning(card); return false; }
        if(DefaultEvol(selectUnitUID) != selectEvol){ if (useNotify) ShowUsingWarning(card); return false; }
        if (selectUnitExp - defaultExp > 0)         { if (useNotify) ShowUsingWarning(card); return false; }
        if (0 < selectUnitEnchant)                  { if (useNotify) ShowUsingWarning(card); return false; }
        
        return true;
    }

    public static int DefaultEvol(long uid)
    {
        int selectRank = GameCore.Instance.PlayerDataMgr.GetUnitData(uid).rank;

        if (selectRank == 0) { return 5; }
        else if (selectRank == 1) { return 4; }
        else if (selectRank == 2) { return 3; }
        else if (selectRank == 3) { return 2; }
        else if (selectRank == 4) { return 1; }

        return 0;
    }

    private void ShowUsingWarning(CardBase card)
    {
        if (GameCore.Instance.lobbyTutorial == null || GameCore.Instance.lobbyTutorial.IsRunning == false)
        {
            GameCore.Instance.ShowAgree("사용 주의", "현재 카드보다 높은 등급이거나 성장의 흔적이 있는\n 영웅을 재료로 사용합니다. 계속하시겠습니까?", 0, () =>
            {
                GameCore.Instance.CloseMsgWindow();
                AddCardAtSlot(card);
            });
        }
        else
            AddCardAtSlot(card);
    }

    private void SwichingText()
    {
        if (coUpdateInfoMessege != null) StopCoroutine(coUpdateInfoMessege);

        var infoCount = strInfo.Length;
        strInfoIdx = UnityEngine.Random.Range(0, infoCount);

        coUpdateInfoMessege = StartCoroutine(CoUpdateInfoMessege());

        lbGuideHead.text = strInfo[strInfoIdx];
        var tw = lbGuideHead.GetComponent<UITweener>();
        tw.ResetToBeginning();
        tw.PlayForward();
    }

    private IEnumerator CoUpdateInfoMessege()
    {
        var tw = lbGuideHead.GetComponent<UITweener>();

        var waitTime = new WaitForSeconds(3f);
        var waitDuration = new WaitForSeconds(tw.duration);

        while (true)
        {
            yield return waitTime;

            tw.PlayReverse();

            yield return waitDuration;

            var infoCount = strInfo.Length;
            strInfoIdx = (strInfoIdx == 1) ? 0 : 1;
            lbGuideHead.text = strInfo[strInfoIdx];

            tw.PlayForward();

            yield return waitDuration;
        }
    }

    private StrengthenCostDataMap GetNextStrengthenCostData(int _evolLvl, int _enchant)
    {
        var maxStrengthenLevel = GameCore.Instance.DataMgr.GetMaxStrengthenLevel(_evolLvl);

        // 현재 진화단계에서 강화단계가 남은 경우
        if (maxStrengthenLevel > _enchant) return GameCore.Instance.DataMgr.GetStrengthenCostData(_evolLvl, _enchant + 1);
        // 최종 강화가 모두 완료되었을 경우
        else if (_evolLvl == LastLevel) return null; // 각성막음
        // 현재 진화 단계에서 강화단계를 모두 완료 했을 경우 && 마지막 진화단계가 아닌 경우
        else return GameCore.Instance.DataMgr.GetStrengthenCostData(_evolLvl + 1, 0);
    }

    private void SetNextUpgradeLabel(int _evolLvl, int _enchant)
    {
        var maxStrengthenLevel = GameCore.Instance.DataMgr.GetMaxStrengthenLevel(_evolLvl);
        bool bNext = tgForeGauge >= 1f;

        if (maxStrengthenLevel > _enchant) // 현재 진화단계에서 강화단계가 남은 경우
        {
            goPrevEvol.SetActive(false);
            lbPrevUpgrade.text = string.Format("+{0}", _enchant);
            goNextEvol.SetActive(false);
            lbNextUpgrade.text = string.Format("+{0}", _enchant + ((bNext) ? 1 : 0));

            var gap = 0;
            if (FocusedHero.ServerData.exp >= maxValue)
                gap = FocusedHero.ServerData.GetPower(FocusedHero.ServerData.enchant + 1) - FocusedHero.ServerData.GetPower();

            UpdatePower(gap);

            SetUpgradeButton(bNext, _evolLvl, _enchant + 1);
        }
        else if (_evolLvl == LastLevel) // 최종 강화가 모두 완료되었을 경우 // 각성을 막음 
        {
            maxEvol = true;
            goPrevEvol.SetActive(true);
            var prevIcon = goPrevEvol.GetComponentInChildren<UISprite>();
            //prevIcon.spriteName = "ICON_STAR_02_S";
            lbPrevEvol.text = _evolLvl.ToString();
            lbPrevUpgrade.text = string.Empty;

            goNextEvol.SetActive(true);
            var nextIcon = goNextEvol.GetComponentInChildren<UISprite>();
            //nextIcon.spriteName = "ICON_STAR_02_S";
            lbNextEvol.text = _evolLvl.ToString();
            lbNextUpgrade.text = string.Empty;

            UpdatePower(0);

            lbUpButton.text = "강화불가";
            lbUpButton.alignment = NGUIText.Alignment.Center;
            goPriceRoot.SetActive(false);
        }
        else // 현재 진화 단계에서 강화단계를 모두 완료 했을 경우 && 마지막 진화단계가 아닌 경우
        {
            goPrevEvol.SetActive(true);
            var prevIcon = goPrevEvol.GetComponentInChildren<UISprite>();
            prevIcon.spriteName = string.Format("ICON_STAR_{0:00}_S", _evolLvl <= LastLevel ? 1 : 2);
            lbPrevEvol.text = _evolLvl.ToString();
            lbPrevUpgrade.text = string.Empty;

            goNextEvol.SetActive(true);
            var nextIcon = goNextEvol.GetComponentInChildren<UISprite>();
            nextIcon.spriteName = string.Format("ICON_STAR_{0:00}_S", _evolLvl + ((bNext) ? 1 : 0) <= LastLevel ? 1 : 2);
            lbNextEvol.text = (_evolLvl + ((bNext) ? 1 : 0)).ToString();
            lbNextUpgrade.text = string.Empty;

            var evolUnitData = GameCore.Instance.DataMgr.GetUnitData(FocusedHero.LocalData.id + 1);
            var gap = evolUnitData.GetDefPower() - FocusedHero.LocalData.GetDefPower();
            UpdatePower(gap);

            SetUpgradeButton(bNext, _evolLvl + 1, 0);
        }

        lbNextUpgrade.color = bNext ? (Color)CommonType.COLOR_01 : Color.white;
        lbNextEvol.color = bNext ? (Color)CommonType.COLOR_01 : Color.white;
    }
    
    private void UpdateSlider(bool _force = false)
    {
        // 변화량 계산
        var now = (FocusedHero.ServerData.exp - subValue) / (maxValue - subValue);
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

        // 게이지 텍스트 출력
        if (now >= 1f)          lbSlider.text = "MAX";
        else if (addValue == 0) lbSlider.text = string.Format("{0:0.#}%", now * 100f);
        else                    lbSlider.text = string.Format("{0:0.#}%(+{1:0.#}%)", (now + add) * 100f, add * 100f);

        SetNextUpgradeLabel(FocusedHero.LocalData.evolLvl, FocusedHero.ServerData.enchant);
        goMaxGaugeEffect.SetActive(!maxEvol && now >= 1f);
    }

    private void SetUpgradeButton(bool _next, int _evolLvl, int _enchant)
    {
        if (!_next)
        {
            lbUpButton.text = "경험치 변환";
            lbUpButton.alignment = NGUIText.Alignment.Center;
            goPriceRoot.SetActive(false);
            goButtonEffect.SetActive(false);
        }
        else
        {
            if (_enchant == 0 && _evolLvl > AwakeningLevel) lbUpButton.text = "각성";
            else if (_enchant == 0)                         lbUpButton.text = "진화";
            else                                            lbUpButton.text = "강화";
            lbUpButton.alignment = NGUIText.Alignment.Left;
            goPriceRoot.SetActive(true);
            goButtonEffect.SetActive(true);

            var data = GameCore.Instance.DataMgr.GetStrengthenCostData(_evolLvl, _enchant);
            if (data.costGold > 0)
            {
                lbPrice.text = string.Format("{0:N0}", data.costGold);
                spPriceIcon.spriteName = "ICON_MONEY_02";
            }
            else
            {
                lbPrice.text = string.Format("{0:N0}", data.costCoin);
                spPriceIcon.spriteName = string.Format("ICON_TYPE_02_{0:00}_S", FocusedHero.LocalData.charType);
            }
        }
    }

    private void UpdatePower(int _add = 0)
    {
        if (0 < _add) lbAddPower.text = string.Format("[c][00F0FF]+{0:N0}", _add);
        else if (0 > _add) lbAddPower.text = string.Format("[c][FE0000]-{0:N0}", -_add);
        else lbAddPower.text = string.Empty;
        lbPower.text = string.Format("{0:N0}", FocusedHero.LocalData.GetDefPower(FocusedHero.ServerData.enchant));
    }

    #region UnityUIDelegate

    public void OnClickResetFilter()
    {
        filterFlag = 0;
        inventoryItemList.DoFilter(0);
        filterObj.GetComponent<FilterCtrl>().Reset();
        goFilterReset.gameObject.SetActive(false);
    }
    
    public void OnClickFilter()
    {
        if (filterObj == null)
            filterObj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("CommonRsc/prefab/filter_root", transform);

        filterObj.GetComponent<FilterCtrl>().SetPrevFilter();
        GameCore.Instance.ShowObject("필터", null, filterObj, 2, new MsgAlertBtnData[] {
            new MsgAlertBtnData("완료", new EventDelegate(() => {
                var ctrl = filterObj.GetComponent<FilterCtrl>();
                filterFlag = ctrl.GetFilter();
                inventoryItemList.DoFilter(filterFlag);
                goFilterReset.gameObject.SetActive(filterFlag != 0);
                filterObj.transform.parent = transform;
                filterObj.SetActive(false);
                GameCore.Instance.CloseMsgWindow();
            }), true, null, SFX.Sfx_UI_Confirm)
        });
    }

    public void OnClickAutoSelect()
    {
        //ClearSlot();
        if (FocusedHero.ServerData.exp >= maxValue)
        {
            GameCore.Instance.ShowNotice("경험치 변환", "강화/진화/각성을 할 때에는\n 재료가 필요하지 않습니다.", 0);
            return;
        }

        var list = from bigCard in inventoryItemList.GetItemCardList()
                   where CheckExeption(bigCard.SData.uid, false) &&
                         bigCard.Select != CardBase.SelectState.Select &&
                         GameCore.Instance.DataMgr.GetUnitData((GameCore.Instance.PlayerDataMgr.GetUnitData(bigCard.SData.uid).id)).rank > 1
                   orderby bigCard.Data.rank descending
                   select bigCard;

        if (FocusedHero.ServerData.exp + addValue >= maxValue)  {   GameCore.Instance.ShowNotice("경험치 초과", "더 이상 경험치를 획득할 수 없습니다.", 0); return; }

        if (list.Count() < 1)   {   GameCore.Instance.ShowNotice("자동 선택 불가", "선택 가능한 카드가 없습니다.", 0);    return; }

        if(userUpgradeSlotList.Count == 10) { GameCore.Instance.ShowNotice("자동 선택 불가", "재료 슬롯이 가득 찼습니다.", 0); return; }
        int index = 0;

        foreach (var selectCard in list)
        {
            if (userUpgradeSlotList.Count == SlotList.Length) break;
            if (FocusedHero.ServerData.exp + addValue >= maxValue) break;
            AddCardAtSlot(selectCard);
            index += 1;
        }
        //inventoryItemList.Reposition(false);
    }

    public void OnClickUnselectAll()
    {
        ClearSlot();

        

        //while (slotInCards.Count != 0)
        //{
        //    var card = list.GetItem(slotInCards[slotInCards.Count - 1].ID);
        //    card.SetSelect(CardBase.SelectState.None);
        //    RemoveSlot(slotInCards.Count - 1);
        //}
    }

    public void OnClickUpgrade()
    {
        bool heroEqiupCheck = false;
        if (FocusedHero.ServerData.exp < maxValue) // 재료 넣기 중일 때
        {
            if (userUpgradeSlotList.Count == 0)
                GameCore.Instance.ShowAlert("재료가 되는 카드들을 선택해주세요.");
            else if (((FocusedHero.ServerData.exp + addValue) / maxValue) > 1f) // + Check Money
                GameCore.Instance.ShowAgree("경험치 초과", "100%를 넘은 경험치는 모두 사라집니다.\n그래도 강화하시겠습니까?", 0, () =>
                {
                    GameCore.Instance.CloseMsgWindow();
                    for (int i = 0; i < userUpgradeSlotList.Count; ++i)
                    {
                        if (userUpgradeSlotList[i].Info == CardBase.UnitInfo.Equip) heroEqiupCheck = true;
                    }
                    GameCore.Instance.NetMgr.Req_Enchant_Strengthen_Exp(FocusedHero.ServerData.uid, userUpgradeSlotList.ToArray()); // 재료 넣기
                    
                    if (heroEqiupCheck) GameCore.Instance.ShowAlert("재료로 사용된 캐릭터가 착용하고 있던 장비는 가방으로 이동합니다.");
                });
            else
            {
                for (int i = 0; i < userUpgradeSlotList.Count; ++i)
                {
                    if (userUpgradeSlotList[i].Info == CardBase.UnitInfo.Equip) heroEqiupCheck = true;
                }
                GameCore.Instance.NetMgr.Req_Enchant_Strengthen_Exp(FocusedHero.ServerData.uid, userUpgradeSlotList.ToArray()); // 재료 넣기
              
                if (heroEqiupCheck) GameCore.Instance.ShowAlert("재료로 사용된 캐릭터가 착용하고 있던 장비는 가방으로 이동합니다.");
            }
        }
        else // 진화 또는 강화
        {
            var maxStrengthenLevel = GameCore.Instance.DataMgr.GetMaxStrengthenLevel(FocusedHero.LocalData.evolLvl);
            if (maxStrengthenLevel > FocusedHero.ServerData.enchant)
            {
                if (CheckCost(FocusedHero.LocalData.evolLvl, FocusedHero.ServerData.enchant + 1, true))
                    GameCore.Instance.NetMgr.Req_Enchant_Strengthen(FocusedHero.ServerData.uid); // 강화 하기
            }
            else
            {
                if (FocusedHero.LocalData.evolLvl >= LastLevel) // 최대 진화 // 각성막음
                    GameCore.Instance.ShowNotice("진화", "최대 진화 단계입니다.", 0);
                else if (CheckCost(FocusedHero.LocalData.evolLvl + 1, 0, true))
                    GameCore.Instance.NetMgr.Req_Enchant_Evolution(FocusedHero.ServerData.uid); // 진화 하기
            }
        }
    }


    public bool CheckCost(int _evol, int _enchant, bool _showPopup = false)
    {
        var data = GameCore.Instance.DataMgr.GetStrengthenCostData(_evol, _enchant);
        ResourceType type = ResourceType.Gold;
        int cost = data.costGold;
        if (data.costGold <= 0)
        {
            type = ResourceType.Cash;
            cost = data.costCoin;
        }

        var result = cost <= GameCore.Instance.PlayerDataMgr.GetReousrceCount(type);

        if (!result && _showPopup)
            GameCore.Instance.ShowReduceResource(type);

        return result;
    }


    private void OnClickInventoryCard(long uid)
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        var card = inventoryItemList.GetItem(uid);
        if (card.Select == CardBase.SelectState.Select) UnSelectAtInventoryCard(uid);
        else
        {
            if (userUpgradeSlotList.Count == 10) { GameCore.Instance.ShowNotice("자동 선택 불가", "재료 슬롯이 가득 찼습니다.", 0); return; }
            else
            {
                if (CheckExeption(uid)) SelectInventoryCard(uid);
            }
        }
        
    }

    private void OnClickSlotCard(long uid)
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        var selectCard = userUpgradeSlotList.Find(data => data.ID == uid);
        UnSelectAtInventoryCard(uid);
        RemoveAtSlot(userUpgradeSlotList.IndexOf(selectCard));
    }

    #endregion

    #region "정체불명의 메서드"

    private float GetCardValue(CardBase _card)
    {
        var data = _card.Data as UnitDataMap;
        if (data != null)
        {
            var heroData = GameCore.Instance.DataMgr.GetUnitData(FocusedHero.ServerData.key);
            if (heroData.charIdType == data.charIdType)
                return data.exp * 2;
            else 
                return data.exp;
        }
        return 0f;
    }

    private void SetAddableValue(float _addedValue)
    {
        addValue += _addedValue;
        UpdateSlider();
    }

    /// <summary>
    /// 필터링 할때 호출됨
    /// </summary>
    /// <param name="_card"></param>
    /// <param name="_filter"></param>
    /// <returns></returns>
    private bool CBDoFilter(CardBase _card, int _filter)
    {
        int rank = FilterCtrl.GetFilterRank(_filter);
        int type = FilterCtrl.GetFilterType(_filter);
        int rare = FilterCtrl.GetFilterRare(_filter);

        var data = (UnitDataMap)_card.Data;

        return (rank == 0 || (1 << (4 - data.rank) & rank) != 0) &&
               (type == 0 || (1 << (data.charType - 1) & type) != 0) &&
               (rare == 0 || (1 << (data.evolLvl - 1) & rare) != 0);
    }

    int CBCardComparer(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        if (data2.IsExpCard())
        {
            if (!data1.IsExpCard()) return 1;
            else                    return data1.rank.CompareTo(data2.rank);
        }
        else if (data1.IsExpCard())
            return -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        if ((data1.id - 1) % 10 != (data2.id - 1) % 10)
            return (data1.id - 1) % 10 < (data2.id - 1) % 10 ? 1 : -1;

        if (data1.charIdType != data2.charIdType)
            return data1.charIdType < data2.charIdType ? 1 : -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        return sdata1.GetPower().CompareTo(sdata2.GetPower());

        // 강화 -- 데이터 테이블이 없어서 패스
    }

    /// <summary> 자동 선택시 우선 순위 </summary>
    //public int Compare(CardBase _1, CardBase _2)
    //{
    //    var x = GameCore.Instance.PlayerDataMgr.GetUnitData(_1.ID);
    //    var y = GameCore.Instance.PlayerDataMgr.GetUnitData(_2.ID);

    //    if (x.rank != y.rank)
    //        return y.rank.CompareTo(x.rank);

    //    if (x.evolLvl != y.evolLvl)
    //        return x.evolLvl.CompareTo(y.evolLvl);

    //    var sx = GameCore.Instance.PlayerDataMgr.GetUnitSData(_1.ID);
    //    var sy = GameCore.Instance.PlayerDataMgr.GetUnitSData(_2.ID);

    //    return sx.enchant.CompareTo(sy.enchant);

    //    //return x.stats.GetStat(UnitStat.Attack) > y.stats.GetStat(UnitStat.Attack) ? 1 : -1;
    //}

    #endregion


    #region 정렬 메서드
    //정렬 버튼 활성화 함수.
    private void CBClickAlignmentButtom()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        if (!alignmentBox.activeSelf) alignmentBox.SetActive(true);
        else alignmentBox.SetActive(false);
    }
    //랭크기준 내림차순 정렬
    public void OnClickSortByRankDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(0);
        inventoryItemList.SetSortCallBack(HeroSortByRankDescending);
        if (filterObj == null) inventoryItemList.DoFilter(31);
        else inventoryItemList.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //랭크기준 오름차순 정렬
    public void OnClickSortByRankAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(1);
        inventoryItemList.SetSortCallBack(HeroSortByRankAscending);
        if (filterObj == null) inventoryItemList.DoFilter(31);
        else inventoryItemList.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 내림차순 정렬
    public void OnClickSortByNameDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(2);
        inventoryItemList.SetSortCallBack(InvenBase.HeroSortByNameDescending);
        if (filterObj == null) inventoryItemList.DoFilter(31);
        else inventoryItemList.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 오름차순 정렬
    public void OnClickSortByNameAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(3);
        inventoryItemList.SetSortCallBack(InvenBase.HeroSortByNameAscending);
        if (filterObj == null) inventoryItemList.DoFilter(31);
        else inventoryItemList.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 내림차순 정렬
    public void OnClickSortByGetDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(4);
        inventoryItemList.SetSortCallBack(InvenBase.HeroSortByGetDescending);
        if (filterObj == null) inventoryItemList.DoFilter(31);
        else inventoryItemList.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 오름차순 정렬
    public void OnClickSortByGetAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(5);
        inventoryItemList.SetSortCallBack(InvenBase.HeroSortByGetAscending);
        if (filterObj == null) inventoryItemList.DoFilter(31);
        else inventoryItemList.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 내림차순 정렬
    public void OnClickSortByPowerDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(6);
        inventoryItemList.SetSortCallBack(InvenBase.HeroSortByPowerDescending);
        if (filterObj == null) inventoryItemList.DoFilter(31);
        else inventoryItemList.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 오름차순 정렬
    public void OnClickSortByPowerAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(7);
        inventoryItemList.SetSortCallBack(InvenBase.HeroSortByPowerAscending);
        if (filterObj == null) inventoryItemList.DoFilter(31);
        else inventoryItemList.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

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
    #region 영웅 정렬방식.
    // 랭크 내림차순 정렬
    private int HeroSortByRankDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        if (data2.IsExpCard())
        {
            if (!data1.IsExpCard()) return 1;
            else                    return data1.rank.CompareTo(data2.rank);
        }
        else if (data1.IsExpCard())
            return -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        if (sdata1.enchant != sdata2.enchant)
            return sdata2.enchant.CompareTo(sdata1.enchant);

        return sdata2.GetPower().CompareTo(sdata1.GetPower());

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //랭크 오름차순 정렬
    private int HeroSortByRankAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        if (data2.IsExpCard())
        {
            if (!data1.IsExpCard()) return 1;
            else return data1.rank.CompareTo(data2.rank);
        }
        else if (data1.IsExpCard())
            return -1;

        if (data1.rank != data2.rank)
            return data1.rank < data2.rank ? 1 : -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        if (sdata1.enchant != sdata2.enchant)
            return sdata2.enchant.CompareTo(sdata1.enchant);

        return sdata2.GetPower().CompareTo(sdata1.GetPower());

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    ////이름 내림차순 정렬
    //private int HeroSortByNameDescending(Transform _1, Transform _2)
    //{
    //    var c1 = _1.GetComponent<CardBase>();
    //    var c2 = _2.GetComponent<CardBase>();

    //    if (c1.State != c2.State)
    //        return c1.State > c2.State ? 1 : -1;

    //    var data1 = c1.Data as UnitDataMap;
    //    var data2 = c2.Data as UnitDataMap;

    //    if (data1 == null) return 1;
    //    if (data2 == null) return -1;

    //    List<string> nameSort = new List<string>();
    //    nameSort.Add(data1.name);
    //    nameSort.Add(data2.name);
    //    nameSort.Sort();
    //    nameSort.Reverse();
    //    return nameSort[0] == data1.name ? 1 : -1;

    //}
    ////이름 오름차순 정렬
    //private int HeroSortByNameAscending(Transform _1, Transform _2)
    //{
    //    var c1 = _1.GetComponent<CardBase>();
    //    var c2 = _2.GetComponent<CardBase>();

    //    if (c1.State != c2.State)
    //        return c1.State > c2.State ? 1 : -1;

    //    var data1 = c1.Data as UnitDataMap;
    //    var data2 = c2.Data as UnitDataMap;

    //    if (data1 == null) return 1;
    //    if (data2 == null) return -1;

    //    List<string> nameSort = new List<string>();
    //    nameSort.Add(data1.name);
    //    nameSort.Add(data2.name);
    //    nameSort.Sort();
    //    return nameSort[0] == data1.name ? 1 : -1;

    //}
    ////획득 내림차순 정렬
    //private int HeroSortByGetDescending(Transform _1, Transform _2)
    //{
    //    var c1 = _1.GetComponent<CardBase>();
    //    var c2 = _2.GetComponent<CardBase>();

    //    if (c1.State != c2.State)
    //        return c1.State > c2.State ? 1 : -1;

    //    var data1 = c1.Data as UnitDataMap;
    //    var data2 = c2.Data as UnitDataMap;

    //    if (data1 == null) return 1;
    //    if (data2 == null) return -1;

    //    var sData1 = c1.SData as CardSData;
    //    var sData2 = c2.SData as CardSData;

    //    return sData1.uid < sData2.uid ? -1 : 1;

    //    // 강화 -- 데이터 테이블이 없어서 패스
    //}
    ////획득 오름차순 정렬
    //private int HeroSortByGetAscending(Transform _1, Transform _2)
    //{
    //    var c1 = _1.GetComponent<CardBase>();
    //    var c2 = _2.GetComponent<CardBase>();

    //    if (c1.State != c2.State)
    //        return c1.State > c2.State ? 1 : -1;

    //    var data1 = c1.Data as UnitDataMap;
    //    var data2 = c2.Data as UnitDataMap;

    //    if (data1 == null) return 1;
    //    if (data2 == null) return -1;

    //    var sData1 = c1.SData as CardSData;
    //    var sData2 = c2.SData as CardSData;

    //    return sData1.uid > sData2.uid ? -1 : 1;

    //    // 강화 -- 데이터 테이블이 없어서 패스
    //}
    ////공격력 내림차순 정렬
    //private int HeroSortByPowerDescending(Transform _1, Transform _2)
    //{
    //    var c1 = _1.GetComponent<CardBase>();
    //    var c2 = _2.GetComponent<CardBase>();

    //    if (c1.State != c2.State)
    //        return c1.State > c2.State ? 1 : -1;

    //    var data1 = c1.Data as UnitDataMap;
    //    var data2 = c2.Data as UnitDataMap;

    //    if (data1 == null) return 1;
    //    if (data2 == null) return -1;


    //    var sData1 = c1.SData as HeroSData;
    //    var sData2 = c2.SData as HeroSData;


    //    return sData1.GetPower() > sData2.GetPower() ? -1 : 1;

    //}
    ////공격력 오름차순 정렬
    //private int HeroSortByPowerAscending(Transform _1, Transform _2)
    //{
    //    var c1 = _1.GetComponent<CardBase>();
    //    var c2 = _2.GetComponent<CardBase>();

    //    if (c1.State != c2.State)
    //        return c1.State > c2.State ? 1 : -1;

    //    var data1 = c1.Data as UnitDataMap;
    //    var data2 = c2.Data as UnitDataMap;

    //    if (data1 == null) return 1;
    //    if (data2 == null) return -1;

    //    var sData1 = c1.SData as HeroSData;
    //    var sData2 = c2.SData as HeroSData;

    //    return sData1.GetPower() < sData2.GetPower() ? -1 : 1;

    //}
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
#else
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
#endif
    }

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 7:
                nTutorialList.Add(new ReturnTutorialData(GetInventoryItem(0), 5));
                nTutorialList.Add(new ReturnTutorialData(btUp.transform, 0));
                nTutorialList.Add(new ReturnTutorialData(GameCore.Instance.CommonSys.tbUi.btnMenu.transform, 0));
                nTutorialList.Add(new ReturnTutorialData(GameCore.Instance.CommonSys.tbUi.btnMainMenu.transform, 0));
                break;
            default:
                break;
        }
        return nTutorialList;
    }



    void UpdateGauge()
    {
        accFore = Mathf.Min(1f, accFore + Time.deltaTime);
        sdFore.value = Mathf.Lerp(sdFore.value, tgForeGauge, accFore);
    }

    public void ShowEvolutionStarEffect(int _starNum)
    {
        var pos = (_starNum-1) % 5;
        spStarEffect.spriteName = string.Format("ICON_STAR_{0:00}", _starNum <= 5 ? 1 : 2);

        goStarEffect.transform.localPosition = new Vector3(-42, 17, 0) + (Vector3.right * 34 * (pos));
        goStarEffect.SetActive(false);
        goStarEffect.SetActive(true);
    }
}
