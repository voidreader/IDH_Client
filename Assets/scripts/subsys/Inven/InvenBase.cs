using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal abstract class InvenBase : MonoBehaviour
{
	[Flags]
	internal enum TypeFlag  // 표시할 리스트 플래그들
	{                       //	다중선택		확장						필터			표시데이터
		Character		= 1 << 0,			//		0						0(인벤만)				0				배치상태(우선순위 - 배치 > 파견 > 숙소)(숙소에서는 숙소만) 
		Equip			= 1 << 1,			//		0						0						0				장착여부
		SetInterior     = 1 << 2,			//		0					
		Furniture		= 1 << 3,			//		0					
		Prop			= 1 << 4,			//		0		
		Resource		= 1 << 5,			//																		미보유
		Interior		= 1 << 6,			//							
		GiftBox			= 1 << 7,			//							
		Wall			= 1 << 8,			//							
		Floor			= 1 << 9,			//							
	}

	// area
	Collider2D area;

	// tab
	UIGrid tabGrid;
    Vector3 tabGridPosition;

    // Head
    UILabel lbHeadName;
	protected UILabel lbCounter;
	protected UIButton btnExpend;
	protected UIButton btnFilter;
    protected UIButton btnAlignment;
	protected UIButton btnResetFilter;

	//body
	Transform tfBodyRoot;

	// bottom
	UISprite multiSelect;

	GameObject emptyLabel;

	List<UISprite> listTabSprite;
	protected List<ItemList> listRoots; // 사용되는 모든 리스트의 루트
	protected Dictionary<TypeFlag, ItemList> listRootTable;

    protected GameObject filterObj_Char;
    protected GameObject filterObj_Item;

    protected GameObject alignmentBox;

    protected ItemList nowListRoot;
	int selectedIdx;      // 현재 선택된 탭 인덱스

	bool bMultiSelect;      // 다중선택 여부
	bool bUseHead;          // 확장 및 필터 기능 사용 여부.(헤더에 있는 버튼들)

    bool bPressing;

	// 아이템이 클릭되었을때
	protected abstract void CBItemClick(TypeFlag _type, long _id);

	// 아이템이 프레스되었을때
	protected abstract void CBItemPress(TypeFlag _type, long _id);

	// 생성되어야하는 아이템의 아이디를 생성해줄 함수
	protected abstract long[] CBCreateItemIds(TypeFlag _type);

	// 카드 생성후 설정을 위해 호출됨. 카드가 숨겨진다면 false 반환
	protected abstract bool CBSetCard(TypeFlag _type, CardBase _card);

	// 슬롯 확장 버튼을 눌렀을때
	protected abstract void CBClickExpendButton();

	// 필터 버튼을 눌렀을때
	protected abstract void CBClickFilterButton();

    //정렬 버튼을 눌렀을 때
    protected abstract void CBClickAlignmentButtom();

	protected abstract void CBChangeTab(TypeFlag _type);

	protected void Init(TypeFlag _flag, bool _useHaed , bool _reset = false)
	{
		bUseHead = _useHaed;

		multiSelect = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/toggleButton", transform).GetComponent<UISprite>();
        multiSelect.GetComponent<UIAnchor>().container = GameCore.Instance.ui_root.gameObject;
        multiSelect.GetComponent<UIButton>().onClick.Add(new EventDelegate(ToggleMultiSelect));

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
        btnAlignment = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_Range");
		btnResetFilter = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btResetFilter");
		//body
		tfBodyRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "body_Root");
		emptyLabel = UnityCommonFunc.GetGameObjectByName(gameObject, "lbEmpty");

        if (listTabSprite == null) listTabSprite = new List<UISprite>();
        if (listRoots == null) listRoots = new List<ItemList>();
        if (listRootTable == null) listRootTable = new Dictionary<TypeFlag, ItemList>();
		CreateTab(_flag);

		selectedIdx = 0;
		bMultiSelect = false;

        if (_reset)
            return;

		btnExpend.onClick.Add(new EventDelegate(CBClickExpendButton));
		btnFilter.onClick.Add(new EventDelegate(CBClickFilterButton));
		btnResetFilter.onClick.Add(new EventDelegate(OnClickResetFilter));
        btnAlignment.onClick.Add(new EventDelegate(CBClickAlignmentButtom));
	}

	internal virtual void Destroy()
	{
		Destroy(multiSelect.gameObject);
		GameObject.Destroy(gameObject);
	}

	internal long[] GetSelectedIds()
	{
		return nowListRoot.GetSelects();
	}

	internal void Unselect()
	{
		nowListRoot.OffSelectAll();
	}

    internal void ResetScrollInBound()
    {
        nowListRoot.Reposition();
        //GameCore.Instance.DoWaitCall(()=>nowListRoot.ResetScrollInBound());
    }

	internal void RemoveCard(long _id)
	{
		nowListRoot.RemoveItem(_id);
		UpdateCount();
	}
	internal CardBase GetCard(long _id)
	{
		return nowListRoot.GetItem(_id);
	}

	internal void ToggleMultiSelect()
	{
		SetMultiSelect(!bMultiSelect);
	}

	protected void SetMultiSelect(bool _multi)
	{
		bMultiSelect = _multi;

		if (_multi) multiSelect.spriteName = "BTN_ON";
		else				multiSelect.spriteName = "BTN_OFF";

		UpdateMutiSelect(_multi);

		nowListRoot.SetMutiSelectable(_multi);
	}

	// 다중선택 버튼을 눌렀을때 
	protected virtual void UpdateMutiSelect(bool _multi)
	{

	}

	protected void SetActiveGrid()
	{
		nowListRoot.gameObject.GetComponentInChildren<UIGrid>().enabled = true;
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

	private void Open(int _idx)
	{
		selectedIdx = _idx;
		var listRoot = listRoots[selectedIdx];
		nowListRoot = listRoot;
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
		area.enabled = listRoot.Type != TypeFlag.SetInterior;

		// Set Tab
		var tab = listTabSprite[_idx];
        tab.transform.GetChild(0).gameObject.SetActive(true);
        tab.transform.localScale = new Vector3(1.1f, 1.1f);
		tab.spriteName = "BTN_06_01_02";

		bool bMultiSelect = listRoot.Type <= TypeFlag.Prop;
		multiSelect.gameObject.SetActive(bMultiSelect);

		// Set Head
		bool headGroupActive =
			(listRoot.itemType == CardType.Character ||
			 listRoot.itemType == CardType.Equipment) &&
			bUseHead;

		if (headGroupActive) UpdateCount();
		btnExpend.gameObject.SetActive(headGroupActive);
		lbCounter.gameObject.SetActive(headGroupActive);
		btnFilter.gameObject.SetActive(headGroupActive);
        btnAlignment.gameObject.SetActive(headGroupActive);

		lbHeadName.text = tab.GetComponentInChildren<UILabel>().text;

      if( emptyLabel != null)
		    emptyLabel.SetActive(nowListRoot.GetActiveCount() == 0);
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
		btnFilter.gameObject.SetActive(false);
        btnAlignment.gameObject.SetActive(false);
	}

	protected void ShowResetFilter(bool _show)
	{
		btnResetFilter.gameObject.SetActive(_show);
	}

	public void OnClickResetFilter()
	{
		if(nowListRoot != null)
			nowListRoot.DoFilter(0);
        if (filterObj_Char != null) filterObj_Char.GetComponent<FilterCtrl>().Reset();
        if (filterObj_Item != null) filterObj_Item.GetComponent<FilterCtrl>().Reset();

        ShowResetFilter(false);
	}

	internal void CreateTab(TypeFlag _flag)
	{
        ClearData();
		// 인벤 순서 대로 생성
		if ((_flag & TypeFlag.Character) != 0)	        CreateTab("영웅", TypeFlag.Character, CardType.Character, "commonRsc/prefab/hero_card_big");
		if ((_flag & TypeFlag.Equip) != 0)			    CreateTab("장비", TypeFlag.Equip, CardType.Equipment, "commonRsc/prefab/item_card_big");
		if ((_flag & TypeFlag.Resource) != 0)		    CreateTab("재료", TypeFlag.Resource, CardType.resource, "commonRsc/prefab/item_card_big");
		if ((_flag & TypeFlag.Interior) != 0)		    CreateTab("인테리어", TypeFlag.Interior, CardType.Interior, "commonRsc/prefab/item_card_big");
		if ((_flag & TypeFlag.GiftBox) != 0) { }
		if ((_flag & TypeFlag.SetInterior) != 0)        CreateTab_SetInertior("세트", "commonRsc/prefab/item_card_big");
		if ((_flag & TypeFlag.Furniture) != 0)	        CreateTab("가구", TypeFlag.Furniture, CardType.Interior, "commonRsc/prefab/item_card_big");
		if ((_flag & TypeFlag.Prop) != 0)				CreateTab("소품", TypeFlag.Prop, CardType.Interior, "commonRsc/prefab/item_card_big");
		if ((_flag & TypeFlag.Wall) != 0)				CreateTab("벽지", TypeFlag.Wall, CardType.Interior, "commonRsc/prefab/item_card_big");
		if ((_flag & TypeFlag.Floor) != 0)			    CreateTab("바닥", TypeFlag.Floor, CardType.Interior, "commonRsc/prefab/item_card_big");

		tabGrid.enabled = true;

		CBSelectTab(0);
	}

    public void ClearData()
    {
        for(int i = 0; i < listTabSprite.Count; ++i)
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
		Close(selectedIdx);
		Open(_idx);
		CBChangeTab(nowListRoot.Type);
	}

    #region 영웅 정렬방식.
    // 랭크 내림차순 정렬
    public static int HeroSortByRankAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        if (data1.rank != data2.rank)
            return data1.rank < data2.rank ? 1 : -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        if (sdata1.enchant != sdata2.enchant)
            return sdata2.enchant.CompareTo(sdata1.enchant);

        return sdata2.GetPower().CompareTo(sdata1.GetPower());
    }

    //랭크 오름차순 정렬 def.
    public static int HeroSortByRankDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        if (sdata1.enchant != sdata2.enchant)
            return sdata2.enchant.CompareTo(sdata1.enchant);

        return sdata2.GetPower().CompareTo(sdata1.GetPower());
    }
    //이름 내림차순 정렬
    public static int HeroSortByNameAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        return data1.name.CompareTo(data2.name);
    }
    //이름 오름차순 정렬
    public static int HeroSortByNameDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        return data2.name.CompareTo(data1.name);
    }

    //획득 내림차순 정렬
    public static int HeroSortByGetAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        var sData1 = c1.SData as CardSData;
        var sData2 = c2.SData as CardSData;

        return sData1.uid.CompareTo(sData2.uid);

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //획득 오름차순 정렬
    public static int HeroSortByGetDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        var sData1 = c1.SData as CardSData;
        var sData2 = c2.SData as CardSData;

        return sData2.uid.CompareTo(sData1.uid);

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //공격력 내림차순 정렬
    public static int HeroSortByPowerAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        return sdata1.GetPower().CompareTo(sdata2.GetPower());
        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //공격력 오름차순 정렬
    public static int HeroSortByPowerDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        return sdata2.GetPower().CompareTo(sdata1.GetPower());
    }
    #endregion

    #region 아이템 정렬방식.

    //아이템 랭크 내림차순 정렬
    public static int ItemSortByRankAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        if (data1.rank != data2.rank)
            return data1.rank < data2.rank ? 1 : -1;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        var p1 = sdata1.GetPower(false);
        var p2 = sdata2.GetPower(false);
        if (p1 != p2)
            return p2.CompareTo(p1);

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }

    //아이템 랭크 오름차순 정렬
    public static int ItemSortByRankDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

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
    //아이템 공격력 내림차순 정렬
    public static int ItemSortByPowerAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        var p1 = sdata1.GetPower(false);
        var p2 = sdata2.GetPower(false);
        if (p1 != p2)
            return p1.CompareTo(p2);

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;


        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 공격력 오름차순 정렬
    public static int ItemSortByPowerDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        var p1 = sdata1.GetPower(false);
        var p2 = sdata2.GetPower(false);
        if (p1 != p2)
            return p2.CompareTo(p1);

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;
        
        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 이름 내림차순 정렬
    public static int ItemSortByNameAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        if (data1.name != data2.name)
            return data1.name.CompareTo(data2.name);

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);

    }
    //아이템 이름 오름차순 정렬
    public static int ItemSortByNameDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        if (data1.name != data2.name)
            return data2.name.CompareTo(data1.name);

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 획득 내림차순 정렬
    public static int ItemSortByGetAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        return sdata1.uid.CompareTo(sdata2.uid);
    }
    //아이템 획득 내림차순 정렬
    public static int ItemSortByGetDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State > c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        return sdata2.uid.CompareTo(sdata1.uid);

    }
    #endregion

    private void CreateTab(string _tabName, TypeFlag _type, CardType _itemType, string _rscName)
	{
		// 탭버튼 생성
		var idx = listRoots.Count;
		var tab = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/tabButton", tabGrid.transform);

        tab.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => {
            CBSelectTab(idx); }));
        tab.GetComponentInChildren<UILabel>().text = _tabName;
        listTabSprite.Add(tab.GetComponent<UISprite>());
        tabGrid.enabled = true;

        // 아이템 리스트 생성
        var body = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/ListBody", tfBodyRoot);
		var itemList = new ItemList(body, _type, _itemType, OnClickItem, OnPressItem,null);
		listRoots.Add(itemList);
		listRootTable.Add(_type, itemList);

		var grid = body.GetComponent<UIGrid>();
		if (_itemType == CardType.Character)	                                grid.cellHeight = 205;
		else																	grid.cellHeight = 160;

		//아이템 리스트 아이디 생성설정
		var ids = CBCreateItemIds(_type);

		// 아이템 항목 생성
		for (int i = 0; i < ids.Length; ++i)
		{
			var tf = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(_rscName, body.transform).transform;
			var card = tf.GetComponent<CardBase>();

			CardSData sdata;
			if (_itemType == CardType.Character)
				sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(ids[i]);
			else
				sdata = GameCore.Instance.PlayerDataMgr.GetItemSData(ids[i]);

            if (sdata.key == 0)
            {
                Debug.LogError("Data Key is 0");
                continue;
            }

			card.Init(sdata, null, null, null);

			// 카드 설정 콜백
			card.gameObject.SetActive(CBSetCard(_type, card));

			itemList.AddItem(tf, card.ID, card);
		}

        

        body.SetActive(false);
	}

	private void CreateTab_SetInertior(string _tabName, string _rscName)
	{
		// 탭버튼 생성
		var idx = listRoots.Count;
		var tab = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/tabButton", tabGrid.transform);
		tab.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => {
            CBSelectTab(idx); }));
		tab.GetComponentInChildren<UILabel>().text = _tabName;
		listTabSprite.Add(tab.GetComponent<UISprite>());
		tabGrid.enabled = true;

		// 아이템 리스트 생성
		var body = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Inven/ListBody_setInterior", tfBodyRoot);
		var itemList = new SubTabItemList(body, TypeFlag.SetInterior, CardType.Interior, OnClickItem, OnPressItem);
		listRoots.Add(itemList);
		listRootTable.Add(TypeFlag.SetInterior, itemList);

		//아이템 리스트 아이디 생성설정
		var ids = CBCreateItemIds(TypeFlag.SetInterior);

		// 소지중인 아이템 키테이블 생성
		Dictionary<int, long> keys = new Dictionary<int, long>();
		for( int i = 0; i < ids.Length; ++i)
			keys.Add(GameCore.Instance.PlayerDataMgr.GetItemSData(ids[i]).key, ids[i]);


		// 세트 테이블 데이터 읽기
		var list = (DataMapCtrl<SetFurnitureDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.SetFurniture);
		var it = list.GetEnumerator();
		while(it.MoveNext())
		{
			var setData = it.Current.Value;
			int haveCount = 0;
			List<CardBase> items = new List<CardBase>();
			var dataList = GameCore.Instance.DataMgr.GetSetFurnitureSetList(setData.id);
			// 카드 생성
			for( int i = 0; i < dataList.Count; ++i)
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
				CBSetCard(TypeFlag.Interior, card);

				items.Add(card);
			}

			itemList.CreateTab(setData.id, setData.name, GameCore.Instance.DataMgr.GetItemStringData(setData.discID), haveCount, items.ToArray());
		}
		itemList.ChangeFirst();
		body.SetActive(false);
	}

    /// <summary>
    /// 인벤토리 내부에서 아이템을 클릭하였을 때 발생하는 이벤트.
    /// 
    /// NOTE : 클릭 관련 이슈로 인하여 bPressing을 삽입하였다.
    /// 기본적으로 false처리 되어있기 때문에 Press 이벤트 발생중이 아니라면 작동한다.
    /// Press 작동중 이슈로 인하여 추가발생될 경우 bPressing을 해제하여
    /// 정상적으로 작동할 수 있는 환경으로 개발해놨음.
    /// 
    /// NOTE : 잘못된 코드가 아닙니다..
    /// </summary>
    /// <param name="_id"> 클릭된 이벤트 객체. </param>
	private void OnClickItem(long _id)
	{
        if(!bPressing)
        {
            if (nowListRoot.SetSelect(_id))
                if (!bMultiSelect)
                    CBItemClick(nowListRoot.Type, _id);
        }
        else
        {
            bPressing = false;
        }
	}

	private void OnPressItem(long _id)
	{
        // [5Star] 에서 작성한 주석
        //CBItemClick(nowListRoot.Type, _id);

#if UNITY_EDITOR
        StartCoroutine(PreventOnClickIsPressing(nowListRoot, 1.5F /* onClick이 인식되는 딜레이 시간*/, _id));
#endif
    }

#if UNITY_EDITOR

    /// <summary>
    /// 아이템 리스트 자체가 Monobehavir를 상속받고있지 않고,
    /// onClick 이벤트가 겹쳐서 발생하는 이슈로 인하여 코루틴으로 잠구는 방식을 채택
    /// OnPress도중 터치나 마우스를 땠을 때 일정 시간이 경과하지 않은 상태면 OnClick 이벤트가 발생하기 때문에
    /// 그 시간동안 Delay를 걸어둔뒤 해제하는 방식으로 한다.
    /// 
    /// OnPress 도중 마우스를 때는 이벤트를 통하여 OnClick이 발생하면 데이터 변화는 없고 bPress 변수만 변경된다.
    /// 
    /// [!] 제작 : 2020-02-19 : 22:45 이현철
    /// </summary>
    /// <param name="_itemList"> 사용될 아이템 리스트 객체</param>
    /// <param name="_delay"> 리턴 딜레이 시간 / 구글링 결과 약 60fps</param>
    /// <param name="_id"> 이벤트 발생에 필요한 ID값 </param>
    /// <returns> 코루틴 반환값. </returns>
    IEnumerator PreventOnClickIsPressing(ItemList _itemList,float _delay, long _id)
    {
        if (bMultiSelect)
        {
            bPressing = true;
            if (_itemList.table.ContainsKey(_id))
            {
                long imageId = _itemList.GetItem(_id).Data.imageID;
                foreach (var data in _itemList.table.Keys)
                {
                    CardBase card = _itemList.GetItem(data);
                    if (card.Data.imageID == imageId)
                    {
                        if (card.Select == CardBase.SelectState.None &&
                            card.State == CardBase.States.Normal)
                        {
                            // NOTE : 데이터를 직접 꺼내서 적용하는 방법을 해보았으나
                            // [5Star]에서 만든 코드와 충돌하여 충돌하지 않는 방법으로 다시 제작하였음.
                            // 클릭된 상태에서 모두 취소가 안되는 버그현상이있었음 ( 데이터상으로는 취소되었으나, 라이트가 꺼지지 않음 )
                            // 그의 해결방법으로 SetSelect로 변경.
                            // GetItem(data).SetSelect를 사용하지 말것 : 내부 충돌로 버그가 발생함.
                            _itemList.SetSelect(data);
                        }
                    }
                }
            }
        }

        // 딜레이 대기중에라도 OnClick이벤트가 발생하면 bPress는 자동으로 false로 변경됨.
        // 이는 Editor상에서만 사용되는 코드이기때문에 추가시 모바일 환경을 테스트 해볼 필요가 있음.
        yield return new WaitForSeconds(_delay);
        bPressing = false;
    }

#endif

    public ItemList GetSelectedInvenItem()
    {
        return listRoots[selectedIdx];
    }
}
