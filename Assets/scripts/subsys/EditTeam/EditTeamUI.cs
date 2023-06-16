using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class EditTeamUI : MonoBehaviour, ISequenceTransform
{
	TeamEditor te;

	// 카드
	UILabel lbCardCount;
	int cardMaxCount;
	//UIButton btnFilter;
	GameObject filterObj;

	UISprite[] spTeamNums;

	// 팀스킬 알리미
	UISprite TS_Icon;
	UILabel TS_Name;
	UILabel TS_Disc;

	UILabel totalPower;
	UILabel addedPower;
	UITweener[] twsAddedPower;

	TeamSkillDataMap tsData;

	UIButton btResetFilter;

	long power = 0L;
	bool bPrintAddPower;

    GameObject TeamSkillEffectLoop;
    GameObject TeamSkillEffectHighLight;


    //정렬관련 변수.
    private UIButton btnAlignment;
    private GameObject alignmentBox;

    GameObject[] btAlignmentType;

    UILabel[] alignmentTypeText;                        //정렬 타입 텍스트.
    UILabel[] alignmentIconText;                        //정렬 방식 텍스트.

    private List<Collider2D> ColliderList = new List<Collider2D>();

    UIGrid grTeamSkill;
    UIButton teamSkillButton;
    UIButton teamSkillSubmitButton;

    internal void Init(Action<long> _cbManage, Action _cbUndo, Action _cbClear, Action<int> _cbChangeTeam, Action _cbAutoTeam, Action _cbSubmit)
	{
		cardMaxCount = -1;
		var listGo = UnityCommonFunc.GetGameObjectByName(gameObject, "content");
		//list = new ItemList<long, HeroCardBase>(listGo, CBCardComparer, CBSetFilter);
		//slot = new TeamSlotMgr(gameObject, _cbClickSlot, _cbPressSlot, _cbManage, _cbUnpos, _cbSwap);
		te = new TeamEditor(gameObject, listGo, _cbManage, CBSelectCard, CBChangeSlot);

		lbCardCount = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "card_Counter");
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_filter").onClick.Add(new EventDelegate(()=> {
            CBClickFilter(); }));

		spTeamNums = new UISprite[5];
		for (int i = 0; i < 5; ++i)
		{
			var btn = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "teamBtn" + (i + 1));
			spTeamNums[i] = btn.GetComponent<UISprite>();
			var n = i;
			btn.onClick.Add(new EventDelegate(() => {
                _cbChangeTeam(n); }));
		}

		TS_Icon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "TeamSkill_Icon");
		TS_Icon.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickTeamSkill));
		TS_Name = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "teamskill_Name");
		TS_Disc = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "teamskill_Disc");

        TeamSkillEffectLoop = UnityCommonFunc.GetGameObjectByName(TS_Icon.gameObject, "Eff_Equip_Loop");
        TeamSkillEffectHighLight = UnityCommonFunc.GetGameObjectByName(TS_Icon.gameObject, "Eff_TeamSkill_Equip");

        totalPower = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "total_power");
		addedPower = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "added_power");
		twsAddedPower = UnityCommonFunc.GetComponentsByName<UITweener>(addedPower.gameObject);
		// Button Callbacks
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_Undo").onClick.Add(new EventDelegate(() => {
			GameCore.Instance.ShowAgree("팀 되돌리기","팀을 이전 저장한 상태로 되돌립니다.\n 되돌리시겠습니까?", 0, () => {
				_cbUndo();
                te.SelectedUnitID = -1;
                GameCore.Instance.CloseMsgWindow();
			});
		}));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_Reset").onClick.Add(new EventDelegate(() => {
			GameCore.Instance.ShowAgree("팀 초기화","배치된 상태를 모두 해제합니다.\n초기화 하시겠습니까?", 0, () =>{
				te.ClearSlot();
				_cbClear();
                te.SelectedUnitID = -1;
                te.ListReposition();
                CheckTeamSKill();
                CalcTeamPower();
                GameCore.Instance.CloseMsgWindow();
			});
		}));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_Auto").onClick.Add(new EventDelegate(() => {
			_cbAutoTeam();
		}));
        teamSkillButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_TS");
        teamSkillButton.onClick.Add(new EventDelegate(() => {
            OnClickTeamSkill();
        }));

        teamSkillSubmitButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btSubmit");
        teamSkillSubmitButton.onClick.Add(new EventDelegate(()=>	{
			_cbSubmit();
		}));

		btResetFilter = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btResetFilter");
		btResetFilter.onClick.Add(new EventDelegate(OnClickResetFilter));
		bPrintAddPower = false;
		PrintAddTeamPower(0);


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

    internal void SetTeamNumButton(int _num)
	{
		for( int i = 0; i < 5; ++i)
		{
			if( _num == i)
				spTeamNums[i].spriteName = "BTN_05_01_02";
			else
				spTeamNums[i].spriteName = "BTN_05_01_01";
		}
	}

	internal void UpdateCardMaxCount(int _maxCnt = -1)
	{
		if( _maxCnt != -1)
			cardMaxCount = _maxCnt;

		var cnt = te.GetListCount();
		if(cnt <= _maxCnt )
			lbCardCount.text = cnt + "[c]/" + cardMaxCount;
		else
			lbCardCount.text = "[c][FF0000]" + cnt + "[-]/" + cardMaxCount;
	}

	internal long GetSlotUnitID(int _num)
	{
		return te.GetUnitIDInSlot(_num);
	}

	internal void OffAffectAllOnList()
	{
		te.OffAffectAllOnList();
	}

	internal void SlotClear()
	{
		te.ClearSlot();
	}

	internal void SetTeam(HeroSData[] _unit)
	{
		bPrintAddPower = false;
		te.ClearSlot();
        for (int i = 0; i < _unit.Length; ++i)
		{
			if (_unit[i] != null)
			{
				te.SetArrangement(i, _unit[i], false, false);
			}
			else
			{
				SlotUnpos(i);
				te.SetArrangement(i, null, false, false);
			}
		}
		CheckTeamSKill();
		CalcTeamPower();
        te.ListReposition();
		bPrintAddPower = true;
	}


	internal void SlotUnpos(int _num)
	{
		var id = te.GetUnitIDInSlot(_num);
		if (id == -1)
			return;

		UnitUnPos(id);
	}

	internal void UnitUnPos(long _id)
	{
		var idx = te.GetSlotIndexByUID(_id);
		if (idx == -1)
			return;

		te.RemoveCharacterBySlot(idx);
	}

	internal int GetSlotCount()
	{
		return te.GetSlotCount();
	}
	internal bool GetEmptyTeam()
	{
		for (int i = 0; i < te.GetSlotCount(); i++)
			if (te.GetSlotOnUnitID(i) != -1L)
				return false;
		return true;
	}

	internal void SetSelect(long _uid)
	{
		te.SelectedUnitID = _uid;
	}

	internal void CBClickFilter()
	{
		if(filterObj == null)
			filterObj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("CommonRsc/prefab/filter_root", transform);
		//filterObj.SetActive(true);
		filterObj.GetComponent<FilterCtrl>().SetPrevFilter();
		GameCore.Instance.ShowObject("필터", null, filterObj, 2, new MsgAlertBtnData[] {
			new MsgAlertBtnData("완료", new EventDelegate(() => {
				//CBDoFilter();
				var ctrl = filterObj.GetComponent<FilterCtrl>();
				int filter = ctrl.GetFilter();
				te.DoFilter(filter);
				ShowResetFilter( filter != 0 );
				filterObj.transform.parent = transform;
				filterObj.SetActive(false);
				GameCore.Instance.CloseMsgWindow();
			}))
            //,
			//new MsgAlertBtnData("취소", new EventDelegate(() => {
			//	filterObj.GetComponent<FilterCtrl>().ChangeReverse();
			//	filterObj.transform.parent = transform;
			//	filterObj.SetActive(false);
			//	GameCore.Instance.CloseAlert();
			//}))
		});
	}

	void ShowResetFilter(bool _show)
	{
		btResetFilter.gameObject.SetActive(_show);
	}

	public void OnClickResetFilter()
	{
		te.DoFilter(0);
        filterObj.GetComponent<FilterCtrl>().Reset();
        ShowResetFilter(false);
	}

	internal void OnClickTeamSkill()
	{
		List<TeamSkillDataMap> list = new List<TeamSkillDataMap>();
		List<int> ids = new List<int>();

		for(int i = 0; i < 6; ++i)
		{
			var uid = te.GetSlotOnUnitID(i);
			var data = GameCore.Instance.PlayerDataMgr.GetUnitData(uid);
			if (data == null)
				continue;

			var tmpList = GameCore.Instance.DataMgr.GetTeamSkillDataInFindMap(data.charIdType);
			if (tmpList == null || tmpList.Count == 0)
				continue;
			ids.Add(data.charIdType);
			for( int j = 0; j < tmpList.Count; ++j)
				if (!list.Contains(tmpList[j]))
					list.Add(tmpList[j]);
		}
		
		if(ids.Count == 0)
		{
			GameCore.Instance.ShowNotice("팀 스킬 불가", "가능한 팀스킬이 없습니다.\n더 많은 영웅을 배치해주세요.", 0);
			return;
		}

		CreateSelectSkill(ids.ToArray(), list);
	}

	void CreateSelectSkill(int[] ids, List<TeamSkillDataMap> list)
	{
		var goTSScrollView = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("EditTeam/TS_ScrolllView", transform);
        grTeamSkill = goTSScrollView.GetComponentInChildren<UIGrid>();
        grTeamSkill.onCustomSort = OnSortTeamSkillListItem;
        grTeamSkill.sorting = UIGrid.Sorting.Custom;

		for (int i = 0; i < list.Count; ++i)
		{
			var listItem = TeamSkillListItem.Create(grTeamSkill.transform);
			var teamSkillKey = list[i].id;
			var equiped = (tsData == null) ? false : (tsData.id != teamSkillKey) ? false : true;
			listItem.Init(list[i], ids, equiped, () => {
                ChangeTeamSkill(teamSkillKey);
                GameCore.Instance.CloseMsgWindow();
            });
		}

		GameCore.Instance.ShowObject("팀스킬 선택", null, goTSScrollView, 3, new MsgAlertBtnData[1] { new MsgAlertBtnData("돌아가기", new EventDelegate(() => {
            GameCore.Instance.CloseMsgWindow();
        })) });
	}

	void ChangeTeamSkill(int _tsKey)
	{
        if (_tsKey <= 0 || (tsData != null && tsData.id == _tsKey))
        {
            //GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_TeamSkill_Equip);
            UnequipTeamSkill();
        }
        else
        {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_TeamSkill_Equip);
            EquipTeamSkill(_tsKey);
        }
	}

	private int OnSortTeamSkillListItem(Transform _1, Transform _2)
	{
		var item1 = _1.GetComponent<TeamSkillListItem>();
		var item2 = _2.GetComponent<TeamSkillListItem>();

		var result = item1.NotExistHeroCount.CompareTo(item2.NotExistHeroCount);
		if (result != 0)
			return result;

		return item2.NeedHeroCount.CompareTo(item1.NeedHeroCount);
	}

	internal void EquipTeamSkill(int _tsKey)
	{
		if (_tsKey <= 0)
		{
			UnequipTeamSkill();
			return;
		}

		tsData = GameCore.Instance.DataMgr.GetTeamSkillData(_tsKey);

		CheckTeamSKill();
		if (tsData == null)
		{
			UnequipTeamSkill();
			return;
		}
		GameCore.Instance.SetUISprite(TS_Icon, tsData.imageID);
		TS_Name.text = tsData.name;
		TS_Disc.text = tsData.disc;

        TeamSkillEffectLoop.gameObject.SetActive(true);
        TeamSkillEffectHighLight.gameObject.SetActive(true);
    }

	internal void UnequipTeamSkill()
	{
		te.SetOffTeamSkillIcon();
		tsData = null;
		GameCore.Instance.SetUISprite(TS_Icon, 4370001);// GameCore.Instance.DataMgr.GetTeamSkillData(3370001));
		TS_Name.text = "스킬없음";
		TS_Disc.text = "팀스킬에 관련된\n영웅을 배치하세요.";

        TeamSkillEffectLoop.gameObject.SetActive(false);
        TeamSkillEffectHighLight.gameObject.SetActive(false);
	}

	void CheckTeamSKill()
	{
		if (tsData == null)
			return;

		te.SetOffTeamSkillIcon();
		int count = te.GetSlotCount();
		for (int i = 0; i < tsData.needChar.Length; ++i)
		{
			bool find = false;
			for (int j = 0; j < count; j++)
			{
				var data = GameCore.Instance.PlayerDataMgr.GetUnitData(te.GetSlotOnUnitID(j));
				if(data != null && data.charIdType == tsData.needChar[i])
				{
					te.SetTeamKillIcon(j, true);
					find = true;
					break;
				}
			}

			if (find == false)
			{
				te.SetOffTeamSkillIcon();
				UnequipTeamSkill();
				break;
			}
		}
	}

	int CalcTeamPower()
	{
		int count = te.GetSlotCount();
		int power = 0;
		for (int i = 0; i < count; i++)
		{
			long uid = te.GetSlotOnUnitID(i);
			if (uid <= 0)
				continue;

			power += GameCore.Instance.PlayerDataMgr.GetUnitSData(uid).GetPower();
		}

		totalPower.text = power.ToString("N0");
		return power;
	}

	void PrintAddTeamPower(int _power)
	{
		if (!bPrintAddPower)
			return;

		if (_power == 0)
			addedPower.text = "";
		else
		{
			if(_power > 0)
			{
				addedPower.text = "+" + _power.ToString("N0");
				addedPower.color = new Color32(0x00, 0xFF, 0xEA, 0xFF);
			}
			else
			{
				addedPower.text = _power.ToString("N0");
				addedPower.color = new Color32(0xC0, 0xC0, 0xC0, 0xFF);
			}

			for(int i = 0; i < twsAddedPower.Length; ++i)
			{
				twsAddedPower[i].ResetToBeginning();
				twsAddedPower[i].PlayForward();
			}
		}
	}

	internal int GetTeamSkillKey()
	{
		if (tsData == null)
			return 0;
		return tsData.id;
	}

	void CBSelectCard(long _uid)
	{
		int power = 0;
		if (0 < _uid)
		{
			power = GameCore.Instance.PlayerDataMgr.GetUnitSData(_uid).GetPower();
			if (0 < te.GetSlotIndexByUID(_uid))
				power *= -1;
		}
	}

	int prevPower = 0;

	void CBChangeSlot()
	{
		var power = CalcTeamPower();
		PrintAddTeamPower(power - prevPower);
		prevPower = power;

		// Check TeamSkill Able
		CheckTeamSKill();
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
        UpdateAlignmentButton(0);
        te.SetAlignment(InvenBase.HeroSortByRankDescending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //랭크기준 오름차순 정렬
    public void OnClickSortByRankAscending()
    {
        UpdateAlignmentButton(1);
        te.SetAlignment(InvenBase.HeroSortByRankAscending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 내림차순 정렬
    public void OnClickSortByNameDescending()
    {
        UpdateAlignmentButton(2);
        te.SetAlignment(InvenBase.HeroSortByNameDescending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 오름차순 정렬
    public void OnClickSortByNameAscending()
    {
        UpdateAlignmentButton(3);
        te.SetAlignment(InvenBase.HeroSortByNameAscending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 내림차순 정렬
    public void OnClickSortByGetDescending()
    {
        UpdateAlignmentButton(4);
        te.SetAlignment(InvenBase.HeroSortByGetDescending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 오름차순 정렬
    public void OnClickSortByGetAscending()
    {
        UpdateAlignmentButton(5);
        te.SetAlignment(InvenBase.HeroSortByGetAscending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 내림차순 정렬
    public void OnClickSortByPowerDescending()
    {
        UpdateAlignmentButton(6);
        te.SetAlignment(InvenBase.HeroSortByPowerDescending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 오름차순 정렬
    public void OnClickSortByPowerAscending()
    {
        UpdateAlignmentButton(7);
        te.SetAlignment(InvenBase.HeroSortByPowerAscending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

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
    //// 랭크 내림차순 정렬
    //private int HeroSortByRankDescending(Transform _1, Transform _2)
    //{
    //    var c1 = _1.GetComponent<CardBase>();
    //    var c2 = _2.GetComponent<CardBase>();

    //    if (c1.State != c2.State)
    //        return c1.State > c2.State ? 1 : -1;

    //    var data1 = c1.Data as UnitDataMap;
    //    var data2 = c2.Data as UnitDataMap;

    //    if (data1 == null) return 1;
    //    if (data2 == null) return -1;

    //    if (data1.rank != data2.rank)
    //        return data1.rank > data2.rank ? 1 : -1;

    //    if ((data1.id - 1) % 10 != (data2.id - 1) % 10)
    //        return (data1.id - 1) % 10 < (data2.id - 1) % 10 ? 1 : -1;

    //    if (data1.charIdType != data2.charIdType)
    //        return data1.charIdType < data2.charIdType ? 1 : -1;

    //    var sdata1 = (HeroSData)c1.SData;
    //    var sdata2 = (HeroSData)c2.SData;

    //    return sdata1.GetPower() < sdata2.GetPower() ? -1 : 1;

    //    // 강화 -- 데이터 테이블이 없어서 패스
    //}
    ////랭크 오름차순 정렬
    //private int HeroSortByRankAscending(Transform _1, Transform _2)
    //{
    //    var c1 = _1.GetComponent<CardBase>();
    //    var c2 = _2.GetComponent<CardBase>();

    //    if (c1.State != c2.State)
    //        return c1.State > c2.State ? 1 : -1;

    //    var data1 = c1.Data as UnitDataMap;
    //    var data2 = c2.Data as UnitDataMap;

    //    if (data1 == null) return 1;
    //    if (data2 == null) return -1;

    //    if (data1.rank != data2.rank)
    //        return data1.rank < data2.rank ? 1 : -1;

    //    if ((data1.id - 1) % 10 != (data2.id - 1) % 10)
    //        return (data1.id - 1) % 10 > (data2.id - 1) % 10 ? 1 : -1;

    //    if (data1.charIdType != data2.charIdType)
    //        return data1.charIdType > data2.charIdType ? 1 : -1;

    //    var sdata1 = (HeroSData)c1.SData;
    //    var sdata2 = (HeroSData)c2.SData;

    //    return sdata1.GetPower() < sdata2.GetPower() ? -1 : 1;

    //    // 강화 -- 데이터 테이블이 없어서 패스
    //}
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

    internal void SetOnClickAction(Action returnTutorialData)
    {
        te.list.saveAction.onPressAction += returnTutorialData;
    }
    internal void SetOnPressAction(Action returnTutorialData)
    {
        te.list.saveAction.onPressAction += returnTutorialData;
    }


    internal Transform GetCardTransform()
    {
        // 카드 리스트에서 uid 3인 카드(녹턴)의 트랜스폼 
        var list = te.list;
        int i = 0;
        foreach (var trData in list.tfTable)
        {
            if (trData.Value.SData.uid == 5)
                return trData.Key;
            i++;
        }

        Debug.LogError("Can't Find CardData Transform in Tutorial");
        return null;
    }

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 3:
                nTutorialList.Add(new ReturnTutorialData(transform, 2));
                nTutorialList.Add(new ReturnTutorialData(te.GetTeamSlotTransform(4), 3));
                nTutorialList.Add(new ReturnTutorialData(te.GetTeamSlotTransform(2), 3));
                nTutorialList.Add(new ReturnTutorialData(te.GetTeamSlotTransform(1), 3));
                nTutorialList.Add(new ReturnTutorialData(teamSkillButton.transform, 0));
                nTutorialList.Add(new ReturnTutorialData(teamSkillSubmitButton.transform, 0));
                nTutorialList.Add(new ReturnTutorialData(GameCore.Instance.CommonSys.tbUi.GetBackButtonTransform, 0));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
