using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

internal class MyRoomInventory : MonoBehaviour
{
    public Action<long> OnSelectHeroCallBack { get; set; }
    public Action<long> OnSelectItemCallBack { get; set; }
    public Action<long[]> OnSelectHeroListCallBack { get; set; }
    public Action<long[]> OnSelectItemListCallBack { get; set; }

    GameObject filterObj;

    // area
    private Collider2D area;
    // tab
    private UIGrid tabGrid;

    // Head (TopBar)
    private UILabel lbHeadName;
    protected UILabel lbCounter;
    protected UIButton btnExpend;
    protected UIButton btnFilter;
    protected UIButton btnResetFilter;
    protected UIButton btnClose;
    // body (Grid)
    private Transform tfBodyRoot;

    // bottom (BottomBar)
    private UISprite multiSelect;
    private GameObject emptyLabel;

    private List<UISprite> listTabSprite;
    private List<ItemList> listRoots;   // 사용되는 모든 리스트의 루트
    protected Dictionary<InvenBase.TypeFlag, ItemList> listRootTable;

    protected ItemList nowListRoot;


    private int selectedIdx;        // 현재 선택된 탭 인덱스

    private bool bMultiSelect;      // 다중선택 여부
    private bool bUseHead;          // 확장 및 필터 기능 사용 여부.(헤더에 있는 버튼들)

    private List<HeroSData> heroList;
    private List<ItemSData> itemList;
    private Dictionary<InvenBase.TypeFlag, List<CardBase>> cardListDic = new Dictionary<InvenBase.TypeFlag, List<CardBase>>();

    //정렬관련 변수.
    private UIButton btnAlignment;
    private GameObject alignmentBox;

    GameObject[] btAlignmentType;

    UILabel[] alignmentTypeText;                        //정렬 타입 텍스트.
    UILabel[] alignmentIconText;                        //정렬 방식 텍스트.

    private List<Collider2D> ColliderList = new List<Collider2D>();


    internal void Init(InvenBase.TypeFlag _flag, bool _useHaed)
    {
        bUseHead = _useHaed;

        multiSelect = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/toggleButton", transform).GetComponent<UISprite>();
        multiSelect.GetComponent<UIAnchor>().container = GameCore.Instance.ui_root.gameObject;
        multiSelect.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { SetMultiSelect(!bMultiSelect); }));
        multiSelect.gameObject.SetActive(false);

        // area
        area = UnityCommonFunc.GetComponentByName<Collider2D>(gameObject, "ListBody");

        // tab
        tabGrid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "tabBtn_Root");

        //Head
        lbHeadName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "HeadName");
        // 상황별 사용
        lbCounter = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Counter");
        btnExpend = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_Expend");
        btnFilter = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_Filter");
        btnResetFilter = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btResetFilter");
        btnClose = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btClose");

        //body
        tfBodyRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "body_Root");
        emptyLabel = UnityCommonFunc.GetGameObjectByName(gameObject, "lbEmpty");

        if (listTabSprite == null) listTabSprite = new List<UISprite>();
        if (listRoots == null) listRoots = new List<ItemList>();
        if (listRootTable == null) listRootTable = new Dictionary<InvenBase.TypeFlag, ItemList>();
        

        selectedIdx = 0;
        bMultiSelect = false;

        btnExpend.onClick.Add(new EventDelegate(OnExpendButtonClick));
        btnFilter.onClick.Add(new EventDelegate(OnFilterButtonClick));
        btnResetFilter.onClick.Add(new EventDelegate(OnResetFilterClick));
        btnClose.onClick.Add(new EventDelegate(() => OnClickCloseButton()));



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

        var Colls = alignmentBox.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < Colls.Length; ++i)
        {
            ColliderList.Add(Colls[i]);
        }
        var btnAlignmentColls = btnAlignment.GetComponent<Collider2D>();
        ColliderList.Add(btnAlignmentColls);

        SetTabList(_flag);
    }

    internal void SetList(List<HeroSData> heroSList, List<ItemSData> itemSList)
    {
        heroList = heroSList;
        itemList = itemSList;
    }

    internal UIButton GetArrangeButton()
    {
        return UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btArrange");
    }

    // 모든 선택 취소
    internal UIButton GetUnselectButton()
    {
        return UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btUnselect");
    }
    // 배치해제  
    internal UIButton GetUnArrangeButton()
    {
        return UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btUnarrange");
    }
    // 자동선택       
    internal UIButton GetAutoArrangeButton()
    {
        return UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btAutoArrange");
    }

    internal UIButton GetCloseButton() { return btnClose; }

    internal void SetTabList(InvenBase.TypeFlag _flag)
    {
        ClearData();

        // 인벤 순서 대로 생성
        if ((_flag & InvenBase.TypeFlag.Character) != 0) CreateTab("영웅", InvenBase.TypeFlag.Character, CardType.Character, "commonRsc/prefab/hero_card_big");
        if ((_flag & InvenBase.TypeFlag.Equip) != 0) CreateTab("장비", InvenBase.TypeFlag.Equip, CardType.Equipment, "commonRsc/prefab/item_card_big");
        if ((_flag & InvenBase.TypeFlag.Resource) != 0) CreateTab("재료", InvenBase.TypeFlag.Resource, CardType.resource, "commonRsc/prefab/item_card_big");
        if ((_flag & InvenBase.TypeFlag.Interior) != 0) CreateTab("인테리어", InvenBase.TypeFlag.Interior, CardType.Interior, "commonRsc/prefab/item_card_big");
        if ((_flag & InvenBase.TypeFlag.GiftBox) != 0) { }
        if ((_flag & InvenBase.TypeFlag.SetInterior) != 0) CreateTab_SetInertior("세트", "commonRsc/prefab/item_card_big");
        if ((_flag & InvenBase.TypeFlag.Furniture) != 0) CreateTab("가구", InvenBase.TypeFlag.Furniture, CardType.Interior, "commonRsc/prefab/item_card_big");
        if ((_flag & InvenBase.TypeFlag.Prop) != 0) CreateTab("소품", InvenBase.TypeFlag.Prop, CardType.Interior, "commonRsc/prefab/item_card_big");
        if ((_flag & InvenBase.TypeFlag.Wall) != 0) CreateTab("벽지", InvenBase.TypeFlag.Wall, CardType.Interior, "commonRsc/prefab/item_card_big");
        if ((_flag & InvenBase.TypeFlag.Floor) != 0) CreateTab("바닥", InvenBase.TypeFlag.Floor, CardType.Interior, "commonRsc/prefab/item_card_big");

        tabGrid.enabled = true;

        CBSelectTab(0);
    }

    private void CreateTab(string _tabName, InvenBase.TypeFlag _type, CardType _itemType, string _rscName)
    {
        // 탭버튼 생성
        var idx = listRoots.Count;
        var tab = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/tabButton", tabGrid.transform);

        tab.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => CBSelectTab(idx)));
        tab.GetComponentInChildren<UILabel>().text = _tabName;
        listTabSprite.Add(tab.GetComponent<UISprite>());
        tabGrid.enabled = true;

        // 아이템 리스트 생성
        var body = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/ListBody", tfBodyRoot);
        ItemList itemListView = null;
        if ((_itemType == CardType.Character))
            itemListView = new ItemList(body, _type, _itemType, OnClickItem, OnPressItem,  InvenBase.HeroSortByRankDescending, CBDoFilter);
        else
            itemListView = new ItemList(body, _type, _itemType, OnClickItem, OnPressItem, InvenBase.ItemSortByRankDescending, CBDoFilter);
        UpdateAlignmentButton(0);
        listRoots.Add(itemListView);
        listRootTable.Add(_type, itemListView);
        var grid = body.GetComponent<UIGrid>();
        if (_itemType == CardType.Character) grid.cellHeight = 205;
        else grid.cellHeight = 160;

        if (_itemType == CardType.Character)
        {
            foreach(var hero in heroList)
            {
                var data = GameCore.Instance.DataMgr.GetUnitData(hero.key);
                if (data.IsExpCard())
                    continue;

                Transform trans = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(_rscName, body.transform).transform;
                CardBase card = trans.GetComponent<CardBase>();
                card.Init(hero, null, null, null);
                card.gameObject.SetActive(CBSetCard(_type, card));
                itemListView.AddItem(trans, card.ID, card);
                if (cardListDic.ContainsKey(_type) == false) cardListDic.Add(_type, new List<CardBase>());
                cardListDic[_type].Add(card);
            }
        }
        else
        {
            var listByType = from item in itemList
                             where (GameCore.Instance.PlayerDataMgr.GetItemData(item.uid).subType == ConvertTypeFlagToItemSubType(_type))
                             select item;

            var list = listByType.ToList();

            foreach (var item in list)
            {
                Transform trans = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(_rscName, body.transform).transform;
                CardBase card = trans.GetComponent<CardBase>();
                card.Init(item, null, null, null);
                card.gameObject.SetActive(CBSetCard(_type, card));
                itemListView.AddItem(trans, card.ID, card);
                if (cardListDic.ContainsKey(_type) == false) cardListDic.Add(_type, new List<CardBase>());
                cardListDic[_type].Add(card);
            }
        }

        body.SetActive(false);
    }

    private void CreateTab_SetInertior(string _tabName, string _rscName)
    {
        // 탭버튼 생성
        var idx = listRoots.Count;
        var tab = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/tabButton", tabGrid.transform);
        tab.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => CBSelectTab(idx)));
        tab.GetComponentInChildren<UILabel>().text = _tabName;
        listTabSprite.Add(tab.GetComponent<UISprite>());
        tabGrid.enabled = true;

        // 아이템 리스트 생성
        var body = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/ListBody_setInterior", tfBodyRoot);
        var itemList = new SubTabItemList(body, InvenBase.TypeFlag.SetInterior, CardType.Interior, OnClickItem, OnPressItem);
        listRoots.Add(itemList);
        listRootTable.Add(InvenBase.TypeFlag.SetInterior, itemList);

        //아이템 리스트 아이디 생성설정
        var ids = CBCreateItemIds(InvenBase.TypeFlag.SetInterior);

        // 소지중인 아이템 키테이블 생성
        Dictionary<int, long> keys = new Dictionary<int, long>();
        for (int i = 0; i < ids.Length; ++i)
            keys.Add(GameCore.Instance.PlayerDataMgr.GetItemSData(ids[i]).key, ids[i]);

        cardListDic.Add(InvenBase.TypeFlag.SetInterior, new List<CardBase>());

        // 세트 테이블 데이터 읽기
        var list = (DataMapCtrl<SetFurnitureDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.SetFurniture);
        var it = list.GetEnumerator();
        while (it.MoveNext())
        {
            var setData = it.Current.Value;
            int haveCount = 0;
            List<CardBase> items = new List<CardBase>();
            var dataList = GameCore.Instance.DataMgr.GetSetFurnitureSetList(setData.id);
            // 카드 생성
            for (int i = 0; i < dataList.Count; ++i)
            {
                var key = dataList[i].itemId;
                if (key <= 0)
                    break;

                // 소지중인지 검사
                var tf = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(_rscName, body.transform).transform;
                var card = tf.GetComponent<CardBase>();

                if (keys.ContainsKey(key))
                {
                    var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(keys[key]);
                    card.Init(sdata, null, null, null);
                    card.SetCount(sdata.count - sdata.myRoomCount);
                    ++haveCount;
                }
                else
                {
                    var data = GameCore.Instance.DataMgr.GetItemData(key);
                    card.Init(null, data, null, null);
                    card.SetState(CardBase.States.NotHave);
                }

                // 카드 설정 콜백
                CBSetCard(InvenBase.TypeFlag.Interior, card);
                items.Add(card);
                cardListDic[InvenBase.TypeFlag.Interior].Add(card);
            }

            itemList.CreateTab(setData.id, setData.name, GameCore.Instance.DataMgr.GetItemStringData(setData.discID), haveCount, items.ToArray());
            
        }

        itemList.ChangeFirst();
        body.SetActive(false);
    }

    private void ClearData()
    {
        for (int i = 0; i < listTabSprite.Count; ++i)
            Destroy(listTabSprite[i].gameObject);

        for (int i = 0; i < listRoots.Count; ++i)
            Destroy(listRoots[i].gameObject);

        listTabSprite.Clear();
        listRoots.Clear();
        listRootTable.Clear();
    }

    /// <summary>
    /// 해당 탭을 open 한다.
    /// 이전에 open된 탭을 close 한다.
    /// </summary>
    /// <param name="_idx"></param>
    public void CBSelectTab(int _idx)
    {
        //GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        Close(selectedIdx);
        Open(_idx);
        CBChangeTab(nowListRoot.Type);
    }

    protected void SetMultiSelect(bool _multi)
    {
        bMultiSelect = _multi;

        if (_multi) multiSelect.spriteName = "BTN_ON";
        else multiSelect.spriteName = "BTN_OFF";

        UpdateMutiSelect(_multi);

        nowListRoot.SetMutiSelectable(_multi);
    }

    public void OnClickCloseButton()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        gameObject.SetActive(false);
        OnSelectHeroCallBack = null;
        OnSelectItemCallBack = null;
        OnSelectHeroListCallBack = null;
        OnSelectItemListCallBack = null;
    }

    private void OnClickItem(long _id)
    {
        //GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        if (nowListRoot.SetSelect(_id))
            if (!bMultiSelect)
                CBItemClick(nowListRoot.Type, _id);
    }

    private void OnPressItem(long _id)
    {
        OnClickItem(_id);
        var card = listRoots[selectedIdx].GetItem(_id);
        GameCore.Instance.ShowCardInfoNotHave(card.Data.id);
    }

    private void OnExpendButtonClick()
    {

    }

    private void OnFilterButtonClick()
    {
        if (filterObj == null)
            filterObj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("CommonRsc/prefab/filter_root", transform);
        //filterObj.SetActive(true);
        filterObj.GetComponent<FilterCtrl>().SetPrevFilter();
        GameCore.Instance.ShowObject("필터", null, filterObj, 2, new MsgAlertBtnData[] {
            new MsgAlertBtnData("완료", new EventDelegate(() => {
                var ctrl = filterObj.GetComponent<FilterCtrl>();
                int filter = ctrl.GetFilter();
                nowListRoot.DoFilter(filter);
                ShowResetFilter( filter != 0 );
                filterObj.transform.parent = transform;
                filterObj.SetActive(false);
                GameCore.Instance.CloseMsgWindow();
            }), true, null, SFX.Sfx_UI_Confirm)
		});
    }

    private void OnResetFilterClick()
    {
        nowListRoot.DoFilter(0);
        filterObj.GetComponent<FilterCtrl>().Reset();
        ShowResetFilter(false);
    }

    public void OnClickResetFilter()
    {
        if (nowListRoot != null) nowListRoot.DoFilter(0);
        ShowResetFilter(false);
    }

    private long[] CBCreateItemIds(InvenBase.TypeFlag _type)
    {
        List<long> ids = new List<long>();
        switch (_type)
        {
            case InvenBase.TypeFlag.Character: return GameCore.Instance.PlayerDataMgr.GetUnitIds();
            case InvenBase.TypeFlag.Equip: return GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.Equipment);
            case InvenBase.TypeFlag.Resource: return GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.resource);
            case InvenBase.TypeFlag.SetInterior:
            case InvenBase.TypeFlag.Interior: return GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.Interior);
            case InvenBase.TypeFlag.GiftBox: return GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.GiftBox);
            default: return null;
        }
    }

    private CardType ConvertTypeFlagToCardType(InvenBase.TypeFlag type)
    {
        switch (type)
        {
            case InvenBase.TypeFlag.Character: return CardType.Character;
            case InvenBase.TypeFlag.Equip: return CardType.Equipment;
            case InvenBase.TypeFlag.Resource: return CardType.resource;
            case InvenBase.TypeFlag.SetInterior:
            case InvenBase.TypeFlag.Furniture:
            case InvenBase.TypeFlag.Prop:
            case InvenBase.TypeFlag.Interior: return CardType.Interior;
            case InvenBase.TypeFlag.GiftBox: return CardType.GiftBox;

            default: return CardType.Dummy;
        }
    }

    private ItemSubType ConvertTypeFlagToItemSubType(InvenBase.TypeFlag type)
    {
        switch (type)
        {
            case InvenBase.TypeFlag.Furniture: return ItemSubType.Furniture;
            case InvenBase.TypeFlag.Prop: return ItemSubType.Prop;
            case InvenBase.TypeFlag.Wall: return ItemSubType.Wall;
            case InvenBase.TypeFlag.Floor: return ItemSubType.Floor;

            default: return ItemSubType.None;
        }
    }

    protected bool CBSetCard(InvenBase.TypeFlag _type, CardBase _card)
    {
        if (_card.Data == null)
            return false;

        switch (_type)
        {
            case InvenBase.TypeFlag.Character:
                var sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(_card.ID);
                if (sdata.GetEquipCount() != 0) _card.SetInfo(CardBase.UnitInfo.Power | CardBase.UnitInfo.Equip);
                else                            _card.SetInfo(CardBase.UnitInfo.Power);

                if (GameCore.Instance.PlayerDataMgr.IsArrange(sdata.uid))
                    _card.SetState(CardBase.States.Arrangement);
                else if (sdata.dispatch != false)
                    _card.SetState(CardBase.States.Detachment);
                else if (sdata.dormitory != 0)
                    _card.SetState(CardBase.States.Room + sdata.dormitory);
                else
                    _card.SetState(CardBase.States.Normal);
                break;

            case InvenBase.TypeFlag.Equip:
                var itemSData = (ItemSData)GameCore.Instance.PlayerDataMgr.GetItemSData(_card.ID);
                if (itemSData.equipHeroUID > 0)
                    _card.SetState(CardBase.States.Arrangement);

                break;
            case InvenBase.TypeFlag.Resource:
                break;
            case InvenBase.TypeFlag.Interior:
                break;
            case InvenBase.TypeFlag.GiftBox:
                break;
            default:
                var interior = GameCore.Instance.PlayerDataMgr.GetItemSData(_card.ID);
                if (interior != null)
                {
                    var cnt = interior.count - interior.myRoomCount;
                    _card.SetCount(cnt);

                }
                else // 미보유
                {
                    // 가구배치직후 간헐적 영웅들이 비보유로 표기. 원인 불명
                    //_card.SetState(CardBase.States.NotHave);
                }
                break;
        }

        return true;
    }

    internal void UpdateAllCard()
    {
        foreach (var targetList in cardListDic)
            for (int i = 0; i < targetList.Value.Count; ++i)
                CBSetCard(targetList.Key, targetList.Value[i]);
    }

    internal void UpdateCard()
    {
        var targetList = cardListDic[nowListRoot.Type];

        for(int i = 0; i < targetList.Count; ++i)
        {
            CBSetCard(nowListRoot.Type, targetList[i]);
        }
    }

    internal void UpdateCard(InvenBase.TypeFlag type)
    {
        var targetList = cardListDic[type];

        for (int i = 0; i < targetList.Count; ++i)
        {
            CBSetCard(nowListRoot.Type, targetList[i]);
        }
    }

    private void Close(int _idx)
    {
        OnClickResetFilter();

        selectedIdx = _idx;
        var listRoot = listRoots[selectedIdx];
        nowListRoot = null;

        listRoot.SetActive(false);

        // Set Tab
        var tab = listTabSprite[_idx];
        tab.transform.localScale = new Vector3(1f, 1f);
        tab.transform.GetChild(0).gameObject.SetActive(false);
        tab.spriteName = "BTN_06_01_01";

        // Set Head
        btnExpend.gameObject.SetActive(false);
        lbCounter.gameObject.SetActive(false);
        btnAlignment.gameObject.SetActive(false);
        GetUnselectButton().gameObject.SetActive(false);
        GetUnArrangeButton().gameObject.SetActive(false);
        GetAutoArrangeButton().gameObject.SetActive(false);

    }

    private void Open(int _idx)
    {
        selectedIdx = _idx;
        var listRoot = listRoots[selectedIdx];
        nowListRoot = listRoot;
        //UpdateCard();

        SetMultiSelect(false);

        listRoot.SetActive(true);

        // Reset Scrool Position
        if (tfBodyRoot != null)
        {
            //tfBodyRoot.transform.localPosition = Vector3.zero;
            SpringPanel.Begin(tfBodyRoot.gameObject, Vector3.zero, 15);
            //if (tfBodyRoot.GetComponent<SpringPanel>() != null)
            //    tfBodyRoot.GetComponent<SpringPanel>().enabled = false;
        }

        // Set Area
        area.enabled = listRoot.Type != InvenBase.TypeFlag.SetInterior;

        // Set Tab
        var tab = listTabSprite[_idx];
        tab.transform.localScale = new Vector3(1.1f, 1.1f);
        tab.transform.GetChild(0).gameObject.SetActive(true);
        tab.spriteName = "BTN_06_01_02";

        //bool bMultiSelect = listRoot.Type <= InvenBase.TypeFlag.Prop;
        //multiSelect.gameObject.SetActive(bMultiSelect);

        // Set Head
        bool headGroupActive =
            (listRoot.itemType == CardType.Character ||
             listRoot.itemType == CardType.Equipment) &&
            bUseHead;

        if (headGroupActive) UpdateCount();
        //btnExpend.gameObject.SetActive(headGroupActive);
        //lbCounter.gameObject.SetActive(headGroupActive);
        btnFilter.gameObject.SetActive(headGroupActive);
        btnAlignment.gameObject.SetActive(headGroupActive);

        //Set Bottom
        if (listRoot.itemType == CardType.Character)
        {
            GetUnArrangeButton().gameObject.SetActive(true);
        }
        else if (listRoot.Type == InvenBase.TypeFlag.SetInterior)
        {
            GetAutoArrangeButton().gameObject.SetActive(true);
        }

        lbHeadName.text = tab.GetComponentInChildren<UILabel>().text;

        if (emptyLabel != null && _idx != 1)
            emptyLabel.SetActive(nowListRoot.GetActiveCount() == 0);
        else
            emptyLabel.SetActive(false);
    }


    internal void UpdateCount()
    {
        int count = nowListRoot.GetCount();
        int maxCount;

        switch (nowListRoot.itemType)
        {
            case CardType.Character: maxCount = GameCore.Instance.PlayerDataMgr.HeroSlotLimitCount; break;
            case CardType.Equipment: maxCount = GameCore.Instance.PlayerDataMgr.EquipItemSlotLimitCount; break;
            default: return;
        }

        if (maxCount < count)
            lbCounter.text = "[FF0000FF]" + count + "[-]/" + maxCount;
        else
            lbCounter.text = count + "[-]/" + maxCount;
    }

    protected void CBChangeTab(InvenBase.TypeFlag _type)
    {
    }

    protected void ShowResetFilter(bool _show)
    {
        btnResetFilter.gameObject.SetActive(_show);
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

        var data = _card.Data as UnitDataMap;
        if (data != null)
            return (rank == 0 || (1 << (4 - data.rank) & rank) != 0) &&
                   (type == 0 || (1 << (data.charType - 1) & type) != 0) &&
                   (rare == 0 || (1 << (data.evolLvl - 1) & rare) != 0);
        else return true;
    }

    protected void UpdateMutiSelect(bool _multi)
    {
        //btSell.SetActive(_multi);
        //btUnselect.SetActive(_multi);
    }

    protected void CBItemClick(InvenBase.TypeFlag _type, long _id)
    {
        if (multiSelect)
        {

        }
        else
        {
            if (_type == InvenBase.TypeFlag.Character)
            {
                var hero = heroList.Find(x => x.uid == _id);
                if (OnSelectHeroCallBack != null) OnSelectHeroCallBack.Invoke(hero.uid);
                return;
            }

            var Item = itemList.Find(x => x.uid == _id);
            if (OnSelectItemCallBack != null) OnSelectItemCallBack.Invoke(Item.uid);
        }
    }

    public ItemList GetSelectedInvenItem()
    {
        return nowListRoot;
    }


    internal void SelectClear()
    {
        nowListRoot.OffSelectAll();
    }

    #region 정렬 메서드
    private void CBClickAlignmentButtom()
    {
        if (!alignmentBox.activeSelf) alignmentBox.SetActive(true);
        else alignmentBox.SetActive(false);
    }
    //랭크기준 내림차순 정렬
    public void OnClickSortByRankDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(0);
        if (nowListRoot.Type == InvenBase.TypeFlag.Character)
        {
            nowListRoot.SetSortCallBack(InvenBase.HeroSortByRankDescending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        else
        {
            nowListRoot.SetSortCallBack(InvenBase.ItemSortByRankDescending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        alignmentBox.gameObject.SetActive(false);
    }
    //랭크기준 오름차순 정렬
    public void OnClickSortByRankAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(1);
        if (nowListRoot.Type == InvenBase.TypeFlag.Character)
        {
            nowListRoot.SetSortCallBack(InvenBase.HeroSortByRankAscending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        else
        {
            nowListRoot.SetSortCallBack(InvenBase.ItemSortByRankAscending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 내림차순 정렬
    public void OnClickSortByNameDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(2);
        if (nowListRoot.Type == InvenBase.TypeFlag.Character)
        {
            nowListRoot.SetSortCallBack(InvenBase.HeroSortByNameDescending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        else
        {
            nowListRoot.SetSortCallBack(InvenBase.ItemSortByNameDescending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 오름차순 정렬
    public void OnClickSortByNameAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(3);
        if (nowListRoot.Type == InvenBase.TypeFlag.Character)
        {
            nowListRoot.SetSortCallBack(InvenBase.HeroSortByNameAscending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        else
        {
            nowListRoot.SetSortCallBack(InvenBase.ItemSortByNameAscending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 내림차순 정렬
    public void OnClickSortByGetDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(4);
        if (nowListRoot.Type == InvenBase.TypeFlag.Character)
        {
            nowListRoot.SetSortCallBack(InvenBase.HeroSortByGetDescending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        else
        {
            nowListRoot.SetSortCallBack(InvenBase.ItemSortByGetDescending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 오름차순 정렬
    public void OnClickSortByGetAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(5);
        if (nowListRoot.Type == InvenBase.TypeFlag.Character)
        {
            nowListRoot.SetSortCallBack(InvenBase.HeroSortByGetAscending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        else
        {
            nowListRoot.SetSortCallBack(InvenBase.ItemSortByGetAscending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 내림차순 정렬
    public void OnClickSortByPowerDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(6);
        if (nowListRoot.Type == InvenBase.TypeFlag.Character)
        {
            nowListRoot.SetSortCallBack(InvenBase.HeroSortByPowerDescending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        else
        {
            nowListRoot.SetSortCallBack(InvenBase.ItemSortByPowerDescending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 오름차순 정렬
    public void OnClickSortByPowerAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(7);
        if (nowListRoot.Type == InvenBase.TypeFlag.Character)
        {
            nowListRoot.SetSortCallBack(InvenBase.HeroSortByPowerAscending);
        if (filterObj == null) nowListRoot.DoFilter(31);
        else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
        else
        {
            nowListRoot.SetSortCallBack(InvenBase.ItemSortByPowerAscending);
            if (filterObj == null) nowListRoot.DoFilter(31);
            else nowListRoot.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());
        }
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


    private void Update()
    {
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
    #endregion
}
