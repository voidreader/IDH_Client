using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class EquipItemUI : MonoBehaviour, IComparer<ItemCardBase>, ISequenceTransform
{
    public static EquipItemUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("EquipItem/EquipItemUI", _parent);
        return go.GetComponent<EquipItemUI>();
    }


    [SerializeField] EquipItemSlotCtrl slotCtrl;
    [SerializeField] EquipItemDesc itemDesc;

    [SerializeField] UIButton[] btTabButtons;
    [SerializeField] UIGrid[] grListRoots;

    [Space()]
    [SerializeField] GameObject goEmptyGuide;
    [SerializeField] CardInfoLabel infoLabel;
    [SerializeField] UI2DSprite sp2dIllust;
    [SerializeField] UILabel lbPower;
    [SerializeField] UILabel lbAddPower;
    [SerializeField] GameObject addPower;
    
    HeroSData unitSData;
    UnitDataMap unitData;
    ItemList[] lists;
    int selectedTab = -1;

    CardBase selectedCard;
    Coroutine runningCorutine = null;

    public bool bAutoEquip { get; private set; } // 아이템 해제후 변경시 자동 선택일 경우 true
    public bool bRunningAutoEquip { get; set; } //아이템 자동 선택 동작중일때 true
    public long equipItemUID { get; private set; } // 아이템 해제후 변경시 장착할 아이템 UID

    public int cachedPower;

    private void Awake()
    {
        // Create List Instance 
        lists = new ItemList[grListRoots.Length];
        for (int i = 0; i < lists.Length; ++i)
            lists[i] = new ItemList(grListRoots[i].gameObject, 0, 0, CBClickListCard, CBPressListCard, CBCardComparer, null);

        // Set Tab Button Click Callback
        for (int i = 0; i < btTabButtons.Length; ++i)
        {
            var n = i;
            btTabButtons[i].onClick.Add(new EventDelegate(() => ToggleTab(n)));
        }
    }

    public void Init(HeroSData _sdata)
    {
        unitSData = _sdata;
        unitData = GameCore.Instance.DataMgr.GetUnitData(_sdata.key);
        InitList();
        ToggleTab(0);

        var unit = GameCore.Instance.PlayerDataMgr.GetUnitData(_sdata.uid);

        GameCore.Instance.SetUISprite(sp2dIllust, unit.GetIllustSpeiteKey());

        lbPower.text = string.Format("{0:N0}", unitSData.GetPower());

        infoLabel.Init(_sdata);
        itemDesc.Clear();
        slotCtrl.Init(_sdata, CBSetEquipItem, CBClickSlotInCard, CBClickUpgrade, CBClickUnequip, ToggleTab);

        UpdateCardList(selectedTab);
        lbAddPower.text = "";
    }
    
    void InitList()
    {
        // Create Cards
        var ids = GameCore.Instance.PlayerDataMgr.GetItemIds(CardType.Equipment);
        for (int i = 0; i < ids.Length; ++i)
        {
            var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(ids[i]);
            var data = GameCore.Instance.DataMgr.GetItemData(sdata.key);

            var typeIdx = GetEquipTypeIdx(data);
            if (0 <= typeIdx && typeIdx < lists.Length && data.equipLimit == unitData.charType)
            {
                var card = CardBase.CreateBigCard(sdata, grListRoots[typeIdx].transform);
                card.SetManageButtonCallBack(CBClickUpgrade);
                card.SetUnposButtonCallBack(CBClickUnequip);
                if (0 < sdata.equipHeroUID)
                {
                    if (sdata.equipHeroUID == unitSData.uid)
                    {
                        card.SetState(CardBase.States.Arrangement); // 착용중
                        slotCtrl.SetItem(card.SData);
                    }
                    else
                        card.SetState(CardBase.States.Detachment); // 타영웅 장착중
                }
                lists[typeIdx].AddItem(card.transform, ids[i], card);
            }
        }

        // Update 비교 아이콘
        for (int i = 0; i < lists.Length; ++i)
        {
            if (slotCtrl.GetCardByIndex(i) == null)
                continue;

            var it = lists[i].GetEnumerator();
            var card = lists[i].GetItem(slotCtrl.GetCardByIndex(i).SData.uid);
            while (it.MoveNext())
            {
                if (card == null || it.Current.Value == card)
                    it.Current.Value.SetCompare(null);
                else
                    it.Current.Value.SetCompare(card.SData);
            }
        }

        // on Grid
        for (int i = 0; i < grListRoots.Length; ++i)
        {
            grListRoots[i].enabled = true;
        }
    }

    void UpdateCardList(int _idx)
    {
        if (_idx < 0 || lists.Length <= _idx)
            return;

        var iter = lists[_idx].GetEnumerator();
        while (iter.MoveNext())
        {
            var card = iter.Current.Value;
            var data = (ItemSData)card.SData;
            if (0 < data.equipHeroUID)
            {
                if (data.equipHeroUID == unitSData.uid)
                    card.SetState(CardBase.States.Arrangement); // 장착중
                else
                    card.SetState(CardBase.States.Detachment); // 타영웅 장착중
            }
        }
    }

    void ToggleTab(int _idx)
    {
        if (selectedTab == _idx)
            return;

        if (selectedTab != -1)
        {
            btTabButtons[selectedTab].GetComponent<UISprite>().spriteName = CommonType.BTN_1_NORMAL;
            grListRoots[selectedTab].gameObject.SetActive(false);
        }
        if (_idx != -1)
        {
            btTabButtons[_idx].GetComponent<UISprite>().spriteName = CommonType.BTN_1_ACTIVE;
            grListRoots[_idx].gameObject.SetActive(true);
        }
        selectedTab = _idx;
        UpdateCardList(_idx);
        goEmptyGuide.SetActive(lists[_idx].GetCount() == 0);

        var sp = grListRoots[_idx].transform.parent.GetComponent<SpringPanel>();
        if (sp != null)
        {
            sp.target = Vector3.zero;
            sp.strength = 8f;
            //sp.onFinished = null;
            sp.enabled = true;
        }

        UnselectCard();

    }

    public void SelectCard(ItemSData _sdata)
    {
        Debug.Log("Select");
        int idx = GetEquipTypeIdx(_sdata);
        var item = lists[idx].GetItem(_sdata.uid);
        var baseStat = unitSData.GetBaseStat();

        itemDesc.SetData(_sdata);
        lists[idx].SetSelect(_sdata.uid);
        slotCtrl.SetSelect(_sdata);
        if (_sdata.equipHeroUID != unitSData.uid)
        {
            item.SetButton(CardBase.ActiveButton.Management);
            var equipedCard = slotCtrl.GetCardByIndex(idx);
            if (equipedCard != null)
            {
                ItemSData equipItemSData = (ItemSData)equipedCard.SData;
                Debug.Log("기존장비 빠지는 값 : " + unitSData.GetPower(false, equipItemSData));
                Debug.Log("들어오는 값 : " + unitSData.GetPower(true, _sdata));
                UpdatePower(unitSData.GetPower(true, _sdata) + unitSData.GetPower(false, equipItemSData));
                Debug.Log("총 계산된 값 : " + unitSData.GetPower(true, _sdata) + unitSData.GetPower(false, equipItemSData));
            }
            else
            {
                UpdatePower(unitSData.GetPower(true, _sdata));
                Debug.Log("총 계산된 값 : " + unitSData.GetPower(true, _sdata));
            }
            //item.SetButton(CardBase.ActiveButton.Management);
            //var equipedCard = slotCtrl.GetCardByIndex(idx);
            //if (equipedCard != null)
            //    UpdatePower(_sdata.GetPower(true, baseStat) - ((ItemSData)equipedCard.SData).GetPower(true, baseStat));
            //else
            //    UpdatePower(_sdata.GetPower(true, baseStat));
        }
        else
        {
            item.SetButton(CardBase.ActiveButton.Management | CardBase.ActiveButton.Unposition);
            UpdatePower(unitSData.GetPower(false, _sdata));
            Debug.Log("총 계산된 값 : " + unitSData.GetPower(false, _sdata));
        }
        selectedCard = item;
        
    }

    public void UnselectCard()
    {
        if (selectedCard == null)
            return;

        itemDesc.Clear();
        slotCtrl.ClearSelect();
        selectedCard.SetSelect(CardBase.SelectState.None);
        selectedCard.SetButton(CardBase.ActiveButton.None);
        lists[GetEquipTypeIdxByUID(selectedCard.ID)].OffSelectAll();
        selectedCard = null;
        
    }


    public void UpdatePower(int _add = 0, bool _updatePower = true)
    {
        if (0 <= _add)       lbAddPower.text = string.Format("[c][00F0FF]+{0:N0}",  _add);
        else if(0 > _add)   lbAddPower.text = string.Format("[c][FE0000]-{0:N0}", -_add);
        if (_updatePower)
            lbPower.text = string.Format("{0:N0}", unitSData.GetPower());

        addPower.GetComponent<TweenAlpha>().ResetToBeginning();
        addPower.GetComponent<TweenAlpha>().enabled = false;

        addPower.GetComponent<TweenPosition>().ResetToBeginning();
        addPower.GetComponent<TweenPosition>().enabled = false;

    }

    int CBCardComparer(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        return data1.GetDefPower() < data2.GetDefPower() ? -1 : 1;

        // 강화 -- 데이터 테이블이 없어서 패스
    }

    bool IsDispatch()
    {
        if (unitSData.dispatch)
        {
            GameCore.Instance.ShowAlert("파밍에 나간 영웅은 장비를 변경할 수 없습니다.");
            return true;
        }

        return false;
    }


    void CBClickListCard(long _uid)
    {
        if(selectedCard == null || selectedCard.SData.uid != _uid)
        {
            SelectCard(GameCore.Instance.PlayerDataMgr.GetItemSData(_uid));
        }
        else
        {
            UnselectCard();
            lbAddPower.text = "";
        }
    }

    void CBPressListCard(long _uid)
    {
    }

    void CBClickUpgrade(long _uid)
    {
        GameCore.Instance.ChangeSubSystem(SubSysType.ItemUp, new HeroInfoPara(_uid, SubSysType.EquipItem));
    }

    void CBClickUnequip(long _uid)
    {
        var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(_uid);
        if (sdata.equipHeroUID != unitSData.uid)
            return;

        if (IsDispatch())
            return;

        var data = GameCore.Instance.PlayerDataMgr.GetItemData(_uid);
        var idx = GetEquipTypeIdx(data);

        equipItemUID = -1;
        GameCore.Instance.NetMgr.Req_Item_Unequip(unitSData.uid, _uid);

        UnselectCard();
    }

    public void SetUnequip(int _slotIdx)
    {
        var oldCard = slotCtrl.UnposSlot(_slotIdx);
        var card = lists[_slotIdx].GetItem(oldCard.uid);
        card.SetState(CardBase.States.Normal);
        //UpdatePower();
        ShowPowerTextAnimation();

        // Update 비교 아이콘
        var it = lists[_slotIdx].GetEnumerator();
        while (it.MoveNext())
        {
            it.Current.Value.SetCompare(null);
        }
    }

    public void CBClickSlotInCard(int _slotIdx)
    {
        if (selectedCard == null)
        {
            SelectCard((ItemSData)slotCtrl.GetCardByIndex(_slotIdx).SData);
        }
        else
        {
            CBSetEquipItem(_slotIdx);
        }
    }

    void CBSetEquipItem(int _slotIdx)
    {
        if (selectedCard == null) 
            return; // 선택된 카드 없음

        var idx = GetEquipTypeIdx((ItemDataMap)selectedCard.Data);
        if (idx != _slotIdx) 
            return; // 다른 슬롯

        TryEquipItem(selectedCard.SData.uid);
        UnselectCard();
    }


    /// <summary>
    /// 장비아이템 변경을 하기위해 서버로 데이터를 보낸다.(이때 이미 장착중인 아이템이 있다면 해제을 먼저 처리해야하기때문에 장착할 아이템의 UID를 equipItemUID에 저장한다.)
    /// </summary>
    /// <param name="_itemUID"></param>
    public void TryEquipItem(long _itemUID)
    {
        var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(_itemUID);
        var idx = GetEquipTypeIdx(sdata);

        // 이미 착용 중이라면
        if (sdata.equipHeroUID == unitSData.uid)
        {
            lbAddPower.text = "";
            return;
        }

        if (IsDispatch())
            return;

        // 같은 타입의 장비를 착용중이라면 해제 요청
        if (unitSData.equipItems[idx] > 0)
        {
            equipItemUID = sdata.uid;
            GameCore.Instance.NetMgr.Req_Item_Unequip(unitSData.uid, unitSData.equipItems[idx]);
        }
        else
        {
            equipItemUID = -1;
            if (sdata.equipHeroUID > 0) // 타 영웅이 장착 중이라면
            {
                //GameCore.Instance.ShowAgree("장비 변경", "다른 영웅이 착용중인 아이템입니다. \n 착용자를 변경하시겠습니까?", 0, () =>
                //    {
                //        GameCore.Instance.NetMgr.Req_Item_Equip_Change(unitSData.uid, sdata.uid);
                //        GameCore.Instance.CloseMsgWindow();
                //    });
                var unit = GameCore.Instance.PlayerDataMgr.GetUnitSData(sdata.equipHeroUID);
                if (unit.dispatch)
                {
                    GameCore.Instance.ShowNotice("장비 변경", "파밍중인 영웅이 장착중인 장비는 변경 할 수 없습니다.", 0);
                }
                else
                {
                    GameCore.Instance.ShowReceiveHeroPopup( "장비 변경", "\n\n\n\n\n\n\n\n\n다른 영웅이 장착중인 아이템입니다. \n착용자를 변경하시겠습니까?", 24, 
                                                            "\n\n\n\n\n현재 착용 영웅", 20, unit, () =>
                    {
                        GameCore.Instance.NetMgr.Req_Item_Equip_Change(unitSData.uid, sdata.uid);
                        GameCore.Instance.CloseMsgWindow();
                    });
                }
                
            }
            else // 장착 요청을 한다.
            {
                cachedPower = GameCore.Instance.PlayerDataMgr.GetUnitSData(unitSData.uid).GetPower();
                GameCore.Instance.NetMgr.Req_Item_Equip(unitSData.uid, sdata.uid);
            }
        }
    }

    /// <summary>
    /// 현재 씬의 UI상에서 장비 아이템을 변경한다.
    /// </summary>
    /// <param name="_itemUID"></param>
    public void SetEquipItem(long _itemUID, bool _updatePower = true)
    {
        var sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(_itemUID);
        var idx = GetEquipTypeIdx(sdata);

        //GameCore.Instance.PlayerDataMgr.SetEquip(unitSData.uid, sdata.uid);
        var oldCard = slotCtrl.SetItem(sdata);
        if (oldCard != null)
        {
            //((ItemSData)oldCard).equipHeroUID = -1;
            lists[idx].GetItem(oldCard.uid).SetState(CardBase.States.Normal);
        }
        lists[idx].GetItem(sdata.uid).SetState(CardBase.States.Arrangement);

        //UpdatePower();
        if (_updatePower)
            ShowPowerTextAnimation();

        // Update 비교 아이콘
        var it = lists[idx].GetEnumerator();
        var card = lists[idx].GetItem(sdata.uid);
        while (it.MoveNext())
        {
            if (card == null || it.Current.Value == card)
                it.Current.Value.SetCompare(null);
            else
                it.Current.Value.SetCompare(card.SData);
        }
    }

    public void ShowPowerTextAnimation()
    {
        if (runningCorutine != null) StopCoroutine(runningCorutine);
        runningCorutine = StartCoroutine(UpdateLbPowerText());
    }

    private IEnumerator UpdateLbPowerText(){
        int power = Int32.Parse(lbPower.text.Replace(",", ""));
        int endPower = unitSData.GetPower();

        int centerPoint = ((endPower - power) / 2) + power;
        int speed = 5;
        
        float time = 0f;


        addPower.GetComponent<TweenAlpha>().ResetToBeginning();
        addPower.GetComponent<TweenAlpha>().enabled = true;

        addPower.GetComponent<TweenPosition>().ResetToBeginning();
        addPower.GetComponent<TweenPosition>().enabled = true;

        while (power != endPower){
            time += Time.deltaTime * speed;
            power = (int) Mathf.Lerp(power, endPower, time);
            lbPower.text = string.Format("{0:N0}", power);
            yield return null;
        }
    }

    public void SetAutoEquip()
    {
        if (IsDispatch())
            return;

        bAutoEquip = false;
        PriorityQueue<ItemCardBase> queue = new PriorityQueue<ItemCardBase>(this);
        List<long> equipItemUIDs = new List<long>();
        int cntEmptySlot = 0;
        int cntWorking = 0;
        for (int i = 0; i < lists.Length; ++i)
        {
            if (unitSData.equipItems[i] > 0)
                continue;

            cntEmptySlot++;
            var it = lists[i].GetEnumerator();
            while (it.MoveNext())
                queue.Push((ItemCardBase)it.Current.Value);

            while (true)
            {
                var card = queue.Pop();
                if (card == null)
                    break;
                else if ((card.SData as ItemSData).equipHeroUID <= 0)
                {
                    equipItemUIDs.Add(card.SData.uid);
                    cntWorking++;
                    break;
                }
            }
            queue.Clear();
            lbAddPower.text = "";
        }

        if (cntEmptySlot != 0 && cntWorking == 0)
            GameCore.Instance.ShowAlert("착용 가능한 장비가 없습니다.");
        else if (equipItemUIDs.Count != 0)
        {
            bRunningAutoEquip = true;
            cachedPower = GameCore.Instance.PlayerDataMgr.GetUnitSData(unitSData.uid).GetPower();
            GameCore.Instance.NetMgr.Req_Item_Equip(unitSData.uid, equipItemUIDs.ToArray());
        }
    }

    public void OnClickUnequipAll()
    {
        if (IsDispatch())
            return;

        var equipItems = new List<long>(unitSData.equipItems);
        for(int i = equipItems.Count-1; i >= 0 ; --i)
            if (equipItems[i] <= 0)
                equipItems.RemoveAt(i);

        equipItemUID = -1;
        if(equipItems.Count > 0){
            bRunningAutoEquip = true;
            GameCore.Instance.NetMgr.Req_Item_Unequip(unitSData.uid, equipItems.ToArray());
            UnselectCard();
        }
        lbAddPower.text = "";
    }

    public void OnClickAutoEquip()
    {
        if (bRunningAutoEquip | bAutoEquip)
            return;

        UnselectCard();

        for (int i = 0; i < unitSData.equipItems.Length; ++i)
        {
            if (0 < unitSData.equipItems[i])
            {
                bAutoEquip = true;
                OnClickUnequipAll();
                return;
            }
        }

        SetAutoEquip();
    }

    public int Compare(ItemCardBase _1, ItemCardBase _2)
    {
        if (((ItemSData)_1.SData).equipHeroUID > 0) return -1;
        if (((ItemSData)_2.SData).equipHeroUID > 0) return 1;

        if (_1.Data.rank != _2.Data.rank)
            return _1.Data.rank.CompareTo(_2.Data.rank);

        return ((ItemSData)_2.SData).GetPower(false).CompareTo(((ItemSData)_1.SData).GetPower(false));
    }

    public static int GetEquipTypeIdxByUID(long _uid)
    {
        return GameCore.Instance.PlayerDataMgr.GetItemData(_uid).subType - (ItemSubType.EquipItem + 1);
    }

    public static int GetEquipTypeIdx(ItemSData _sdata)
    {
        return GameCore.Instance.DataMgr.GetItemData(_sdata.key).subType - (ItemSubType.EquipItem + 1);
    }

    public static int GetEquipTypeIdx(ItemDataMap _sdata)
    {
        return _sdata.subType - (ItemSubType.EquipItem + 1);
    }

    public static int GetEquipTypeIdx(int _subType)
    {
        return _subType - ((int)ItemSubType.EquipItem + 1);
    }

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 6:
                var listData = lists[0];
                Transform equipData = null;
                foreach (var trData in listData.tfTable)
                {
                    equipData = trData.Key;
                    break;
                }
                nTutorialList.Add(new ReturnTutorialData(equipData, 5));
                nTutorialList.Add(new ReturnTutorialData(slotCtrl.GetSlotTransform(0), 6));
                nTutorialList.Add(new ReturnTutorialData(GameCore.Instance.CommonSys.tbUi.btnMenu.transform, 0));
                nTutorialList.Add(new ReturnTutorialData(GameCore.Instance.CommonSys.tbUi.btnMainMenu.transform, 0));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
