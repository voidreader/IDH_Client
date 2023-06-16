using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class FarmingUI : MonoBehaviour, ISequenceTransform
{
    [SerializeField] UI2DSprite spIllustBlindIllust1;
    [SerializeField] UI2DSprite spIllustBlindIllust2;
    [SerializeField] Animator effectUnlock;

    static int farmingGroupId = 0;

	// 좌측 관련
	UI2DSprite spIllust;
	UI2DSprite spIllustTmp;
	UILabel lbFarmingName;
	TouchSlidScript slid;

	// 우측 관련
	UILabel lbMissionNum;
	UILabel lbMissionCnt;
	GameObject goLockIcon;
	UILabel lbLock;

	UIGrid listGrid;
	List<FarmingInfoListItem> list;
	int selectedIndex = -1;

	// 하단 관련
	BottomRscScript botRsc;

	// 팀 편집
	GameObject editTeamRoot;
	TeamEditor te;
	UILabel lbCondition;
	UILabel lbPower;
    UILabel lbNowPower;
    GameObject addPower;
    UIButton btResetFilter;
    UIButton btInvenArray;          //인벤 정렬 버튼.
    int power;
    
	GameObject filterObj;

	bool bLock; // 현 챕터가 열렸는지 안열렸는지.

	int farmingCount; // 현재 파밍중인 팀 카운터(임시용)

	bool bChapterAnim; // 챕터 전환 애니메이션 중일 때 true
	int chapterCharacterImgId;

    int getNowSlotPower;

    //정렬관련 변수.
    private UIButton btnAlignment;
    private GameObject alignmentBox;

    GameObject[] btAlignmentType;

    UILabel[] alignmentTypeText;                        //정렬 타입 텍스트.
    UILabel[] alignmentIconText;                        //정렬 방식 텍스트.

    private List<Collider2D> ColliderList = new List<Collider2D>();


    internal void Init(int farmingGroupId, Action<long> _cbManage)
	{
		// 좌측 관련
		slid = UnityCommonFunc.GetComponentByName<TouchSlidScript>(gameObject, "PanelIllustRoot");
		slid.SetCallbackEndDrag(CBIllustEndDrag);
		spIllust = UnityCommonFunc.GetComponentByName<UI2DSprite>(gameObject, "Illust");
		var tw = spIllust.GetComponent<TweenPosition>();
		tw.onFinished.Add(new EventDelegate(() =>
		{
			GameCore.Instance.SetUISprite(spIllust, chapterCharacterImgId);
			spIllust.color = bLock ? Color.black : Color.white;
			spIllust.transform.localPosition = Vector3.zero;
			bChapterAnim = false;
		}));
		spIllustTmp = UnityCommonFunc.GetComponentByName<UI2DSprite>(gameObject, "Illust2");
		tw = spIllustTmp.GetComponent<TweenPosition>();
		tw.onFinished.Add(new EventDelegate(() => spIllustTmp.alpha = 0f));

		goLockIcon = UnityCommonFunc.GetGameObjectByName(gameObject, "LockIcon");
		lbLock = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "LockLabel");
		lbFarmingName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "FarmingNameLabel");
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "left").onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            ChangeFarming(true); }));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "right").onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            ChangeFarming(false); }));


		// 우측 관련
		lbMissionNum = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "missionLabel");
		lbMissionCnt = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "RunMissionCount");
		listGrid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "content");
		list = new List<FarmingInfoListItem>();

		UpdateChapter(farmingGroupId, true);

		// 하단 관련
		botRsc = BottomRscScript.Create(transform);
		botRsc.UpdateCount();

		// 팀편집
		editTeamRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "TeamEditor");
		var slot_root = UnityCommonFunc.GetGameObjectByName(gameObject, "slot_Root");
		var list_root = UnityCommonFunc.GetGameObjectByName(gameObject, "list_Root");
		te = new TeamEditor(slot_root, list_root, _cbManage, CBSelectCard, CBChangedSlot, false, true);
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "filterBtn").onClick.Add(new EventDelegate(CBClickFilter));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "closeBtn").onClick.Add(new EventDelegate(CloseEditTeam));
		UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "submitBtn").onClick.Add(new EventDelegate(CBDoFarming));
		btResetFilter = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btResetFilter");
		btResetFilter.onClick.Add(new EventDelegate(OnClickResetFilter));
		lbCondition = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbCondition");
		lbPower = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbPower");
        lbNowPower = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbNowPower");
        addPower = UnityCommonFunc.GetGameObjectByName(gameObject, "addPower");
        btInvenArray = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btInvenArray");



        btnAlignment = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button_Range_Farming");
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

        var alignmentBtnCollider = btnAlignment.GetComponent<Collider2D>();
        var Colls = alignmentBox.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < Colls.Length; ++i)
        {
            ColliderList.Add(Colls[i]);
        }
        var btnAlignmentColls = btnAlignment.GetComponent<Collider2D>();
        ColliderList.Add(btnAlignmentColls);

        getNowSlotPower = 0;
        lbNowPower.text = string.Format("[전투력] {0:N0}", getNowSlotPower);

    }

    internal void CBClickFilter()
	{
		if (filterObj == null)
			filterObj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("CommonRsc/prefab/filter_root", transform);
		filterObj.GetComponent<FilterCtrl>().SetPrevFilter();
		GameCore.Instance.ShowObject("필터", null, filterObj, 2, new MsgAlertBtnData[] {
			new MsgAlertBtnData("완료", new EventDelegate(() => {
				//CBDoFilter();
				var ctrl = filterObj.GetComponent<FilterCtrl>();
				int filter = ctrl.GetFilter();
				ShowResetFilter(filter != 0);
				te.DoFilter(filter);
                te.list.Reposition(false,true);

                filterObj.transform.parent = transform;
				filterObj.SetActive(false);
				GameCore.Instance.CloseMsgWindow();
			}))});
	}

    private void ShowInvenArrayResouce_RankDown()
    {
        List<HeroSData> userHeroSDataList = GameCore.Instance.PlayerDataMgr.GetUserHeroSDataList();

        //list = from hero in userHeroSDataList
               
    }

    private void ShowResetFilter(bool _show)
	{
		btResetFilter.gameObject.SetActive(_show);
	}

	private void OnClickResetFilter()
	{
		te.DoFilter(0);
        filterObj.GetComponent<FilterCtrl>().Reset();
        ShowResetFilter(false);
	}

    

	internal void UpdateTime()
	{
		for (int i = 0; i < list.Count; i++)
			list[i].UpdateTime();
	}

	internal void SetCount(int _count)
	{
		farmingCount = _count;
		lbMissionCnt.text = string.Format("{0}[c] / {1}", _count, GameCore.Instance.DataMgr.GetFarmingConstData().MaxCount);
	}

	internal void ResetList()
	{
		ResetList(farmingGroupId);
    }

	// 출력할 리스트 변경
	internal void ResetList(int _farmingGroupId)
	{
		var defId = 6000001 + _farmingGroupId * 5;
		var defData = GameCore.Instance.DataMgr.GetFarmingData(defId);

		//lbFarmingName.text = ;

		lbMissionNum.text = "FARMING " + (_farmingGroupId + 1);
		SetCount(GameCore.Instance.PlayerDataMgr.GetFarmingCount());

		// Set Illust 
	//	GameCore.Instance.SetUISprite(spIllust, defData.imgID);

		// Set ListItems
		if (list.Count == 0)
		{
			for (int i = 0; i < 5; i++)
			{
				var item = FarmingInfoListItem.Create(listGrid.transform);
				list.Add(item);
			}
		}

		for (int i = 0; i < 5; i++)
		{
			FarmingInfoListItem.State state = FarmingInfoListItem.State.None;
			DateTime time = default(DateTime);

			var id = defId + i;
			var sdata = GameCore.Instance.PlayerDataMgr.GetFarmingDataByKey(id);

			if (bLock) state = FarmingInfoListItem.State.Unlock;

			if (sdata != null)
			{
				if (sdata.endTime < GameCore.nowTime)
				{
					state = FarmingInfoListItem.State.Done;
				}
				else
				{
					time = sdata.endTime;
					state = FarmingInfoListItem.State.Active_UsedQuick;
				}
			}

			var data = GameCore.Instance.DataMgr.GetFarmingData(id);
			list[i].Init(data, state, time, ShowEditTeam, CBFarmingDone, CBFarmingCansel);
        }

		listGrid.enabled = true;
	}


	internal void UpdateChapter(int _farmingGroupId, bool _forced, bool _left = true)
	{
		if (bChapterAnim)
			return;

		farmingGroupId = _farmingGroupId;

		var defId = 6000001 + _farmingGroupId * 5;
		var defData = GameCore.Instance.DataMgr.GetFarmingData(defId);

        int chapter = _farmingGroupId + 1;
		int difficult = 1;// 어려움
		int stage = 10;

		lbFarmingName.text = GameCore.Instance.DataMgr.GetStoryChapterData(StoryChapterDataMap.GenerateChapterKey(chapter, difficult)).name;
        Debug.Log("chapter : " + chapter);
		var sdata = GameCore.Instance.PlayerDataMgr.GetStorySData(StoryDataMap.GenerateStoryKey(chapter, difficult, stage));
        if(sdata != null)
            Debug.Log("UpdateChapter : " + sdata.key);
		bLock = sdata == null || !sdata.clear;

        effectUnlock.enabled = false;
        spIllustBlindIllust1.gameObject.SetActive(false);
        spIllustBlindIllust2.gameObject.SetActive(false);
        if (bLock == false)
        {
            int isFirst = PlayerPrefs.GetInt("FarmingChapterLock" + chapter, 1);
            if(isFirst == 0)
            {
                PlayerPrefs.SetInt("FarmingChapterLock" + chapter, 1);

                GameCore.Instance.SetUISprite(spIllustBlindIllust1, defData.imgID);
                GameCore.Instance.SetUISprite(spIllustBlindIllust2, defData.imgID);
                //spIllustBlindIllust1.gameObject.SetActive(true);
                //spIllustBlindIllust2.gameObject.SetActive(true);
                UnlockNextStage();
            }
        }
		if(!_forced)
		{
			{
				var twAlpha = spIllust.GetComponent<TweenAlpha>();
				twAlpha.from = 1f;
				twAlpha.to = 0f;
				twAlpha.ResetToBeginning();
				twAlpha.PlayForward();

				var twPosition = spIllust.GetComponent<TweenPosition>();
				twPosition.from = new Vector3(0f, 0f, 0f);
				twPosition.to = new Vector3(_left ? 260f : -260f, 0f, 0f);
				twPosition.ResetToBeginning();
				twPosition.PlayForward();

				chapterCharacterImgId = defData.imgID;
				bChapterAnim = true;
			}

			{
				GameCore.Instance.SetUISprite(spIllustTmp, defData.imgID);
                spIllustTmp.color = bLock ? Color.gray : Color.white;
                spIllustTmp.isGray = bLock ? true : false;
				var twAlpha = spIllustTmp.GetComponent<TweenAlpha>();
				twAlpha.from = 0f;
				twAlpha.to = 1f;
				twAlpha.ResetToBeginning();
				twAlpha.PlayForward();

				var twPosition = spIllustTmp.GetComponent<TweenPosition>();
				twPosition.from = new Vector3(_left ? -260f : 260f, 0f, 0f);
				twPosition.to = new Vector3(0f, 0f, 0f);
				twPosition.ResetToBeginning();
				twPosition.PlayForward();
			}
		}
		else
		{
			GameCore.Instance.SetUISprite(spIllust, defData.imgID);
			spIllust.color = bLock ? Color.gray : Color.white;
            spIllust.isGray = bLock ? true : false;
			spIllustTmp.alpha = 0f;
		}


		
		//bLock = false;

		goLockIcon.SetActive(bLock);
		if(bLock)
		{
			lbLock.text = string.Format("스토리 {0}-{1} {2}을 클리어 하세요.", chapter, stage, StoryDataMap.GetStrDiffcult(difficult));
		}

		ResetList();
	}

	internal void UpdateUnitCard()
	{
		te.UpdateCardStates();
	}

	internal void UpdateResource()
	{
		botRsc.UpdateCount();
	}

	internal void ShowEditTeam(int _farmingId)
	{
		if(farmingCount >= GameCore.Instance.DataMgr.GetFarmingConstData().MaxCount)
		{
			GameCore.Instance.ShowNotice("파밍 실패", "실행 가능 수 초과", 0);
			return;
		}

		selectedIndex = _farmingId;
		editTeamRoot.SetActive(true);
		te.InitCard();

        CBChangedSlot();
        PrintAddTeamPower(0);

        AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.Farming, 1, 1);
    }

	private void CBIllustEndDrag(Vector2 _vec)
	{
		if (_vec.x < 0)
			ChangeFarming(false);
		else
			ChangeFarming(true);
	}

	internal void CBFarmingDone(int _farmingId)
	{
		GameCore.Instance.NetMgr.Req_Farming_Done(new int[] { _farmingId });
	}

	internal void CBFarmingCansel(int _farmingId)
	{
        GameCore.Instance.ShowAgree("파밍 취소", "정말로 파밍을 취소하시겠습니까?", 0, () => {
            GameCore.Instance.CloseMsgWindow();
            GameCore.Instance.NetMgr.Req_Farming_Cansel(_farmingId);
        });
	}

	internal void CloseEditTeam()
	{
		editTeamRoot.SetActive(false);
		te.Clear();
		selectedIndex = -1;

        OnClickSortByRankDescending();
        if (btResetFilter.gameObject.activeSelf) OnClickResetFilter();


        StartCoroutine(UpdateNowPowerText(0));
    }

	internal void CBSelectCard(long _uid)
	{
		if(_uid <= 0)
		{
		//	GameCore.Instance.ShowAlert("UnselectedCard");
		}
		else
		{
		//	GameCore.Instance.ShowAlert("SelectedCard");
		}
	}

	internal void CBChangedSlot()
	{
        Debug.Log("Change Slot");
        bool checkConditionFlag = true;
        string colorStr = "[00FF00]";
        FarmingDataMap farmingData = GameCore.Instance.DataMgr.GetFarmingData(selectedIndex);
        int conditionValue = farmingData.conditionValue;
        switch(farmingData.conditionType){
            case 1: 
                int power = 0;
                for (int i = 0; i < te.GetSlotCount(); i++)
                {
                    var uid = te.GetSlotOnUnitID(i);
                    if (uid > 0) power += GameCore.Instance.PlayerDataMgr.GetUnitSData(uid).GetPower();
                }
                if(power < conditionValue) checkConditionFlag = false;
            break;
            case 2: 
            case 4: 
            case 5: 
                for (int i = 0; i < te.GetSlotCount(); i++)
                {
                    var uid = te.GetSlotOnUnitID(i);
                    if (uid > 0) {
                        UnitDataMap unitData = GameCore.Instance.DataMgr.GetUnitData(GameCore.Instance.PlayerDataMgr.GetUnitSData(uid).key);
                        if(farmingData.conditionType == 2){
                            if(conditionValue != unitData.charType){
                                checkConditionFlag = false;
                                break;
                            }
                        }
                        if(farmingData.conditionType == 4){
                            if(conditionValue > unitData.evolLvl){
                                checkConditionFlag = false;
                                break;
                            }
                        }
                        if(farmingData.conditionType == 5){
                            if(conditionValue < unitData.rank){
                                checkConditionFlag = false;
                                break;
                            }
                        }
                    } 
                }
            break;
            case 3: 
                ArrayList charTypeList = new ArrayList();
                for (int i = 0; i < te.GetSlotCount(); i++)
                {
                    var uid = te.GetSlotOnUnitID(i);
                    if (uid > 0) {
                        if(charTypeList.IndexOf(GameCore.Instance.DataMgr.GetUnitData(GameCore.Instance.PlayerDataMgr.GetUnitSData(uid).key).charType) < 0){
                            charTypeList.Add(GameCore.Instance.DataMgr.GetUnitData(GameCore.Instance.PlayerDataMgr.GetUnitSData(uid).key).charType);
                        } else {
                            checkConditionFlag = false;
                            break;
                        }
                    } 
                }
            break;
        }
        bool emptySlot = true;
        for (int i = 0; i < te.GetSlotCount(); i++)
        {
            var uid = te.GetSlotOnUnitID(i);
            if (uid > 0) {
                emptySlot = false;
                break;
            } 
        }
        if(!checkConditionFlag) colorStr = "[FF0000FF]";



        string ConditionColor = "[00FF00]";
        if (farmingData.powerCondition > TeamPower()) ConditionColor = "[FF0000FF]";

        lbPower.text = FarmingDefineMap.GetQualify2FarmingDescString(farmingData, colorStr);
        lbCondition.text = FarmingDefineMap.GetQualify1FarmingDescString(farmingData, ConditionColor);
        
        StartCoroutine(UpdateNowPowerText(TeamPower()));
        ChangeSlot();
    }

	internal void CBDoFarming()
	{
		if (selectedIndex == -1)
		{
			Debug.LogError("Not Selected Farming Item");
			return;
		}

		int power = 0;
		var ids = new List<long>();
		for (int i = 0; i < te.GetSlotCount(); i++)
		{
			var uid = te.GetSlotOnUnitID(i);
			if (uid > 0)
			{
				ids.Add(uid);
				power += GameCore.Instance.PlayerDataMgr.GetUnitSData(uid).GetPower();
			}
		}


		if (ids.Count == 0)
		{
			GameCore.Instance.ShowNotice("파밍 실패", "최소 한명의 영웅이 필요합니다.", 0);
			return;
		}

		if (power < GameCore.Instance.DataMgr.GetFarmingData(selectedIndex).powerCondition)
		{
			GameCore.Instance.ShowNotice("파밍 실패", "전투력이 부족합니다.", 0);
			return;
		}

		// Todo : Farming Condition

		GameCore.Instance.NetMgr.Req_Farming_Create(selectedIndex, ids.ToArray());
	}

	private void ChangeFarming(bool _left)
	{
		var maxChapterCount = GameCore.Instance.DataMgr.GetStoryConstData().maxChapter;
		var idx = (farmingGroupId + maxChapterCount + (_left? -1:1)) % maxChapterCount;

		UpdateChapter(idx, false, _left);
    }

    private int TeamPower()
    {
        int power = 0;
        for (int i = 0; i < te.GetSlotCount(); i++)
        {
            var uid = te.GetSlotOnUnitID(i);
            if (uid > 0)
            {
                power += GameCore.Instance.PlayerDataMgr.GetUnitSData(uid).GetPower();
            }
        }
        return power;
    }

    private IEnumerator UpdateNowPowerText(int teamPower)
    {
        int endPower = teamPower;
        
        int speed = 3;

        float time = 0f;

        while (getNowSlotPower != endPower)
        {
            time += Time.deltaTime * speed;
            getNowSlotPower = (int)Mathf.Lerp(getNowSlotPower, endPower, time);
            lbNowPower.text = string.Format("[전투력] {0:N0}", getNowSlotPower);
            yield return null;
        }
    }

    void PrintAddTeamPower(int _power)
    {
        if (_power == 0)
            addPower.GetComponent<UILabel>().text = "";
        else
        {
            if (_power > 0)
            {
                addPower.GetComponent<UILabel>().text = "+" + _power.ToString("N0");
                addPower.GetComponent<UILabel>().color = new Color32(0x00, 0xFF, 0xFF, 0xFF);
            }
            else
            {
                addPower.GetComponent<UILabel>().text = _power.ToString("N0");
                addPower.GetComponent<UILabel>().color = new Color32(0xFF, 0x00, 0x00, 0xFF);
            }
        }
    }


    int prevPower = 0;

    void ChangeSlot()
    {
        var power = TeamPower();
        PrintAddTeamPower(power - prevPower);
        prevPower = power;

        addPower.GetComponent<TweenAlpha>().from = 1f;
        addPower.GetComponent<TweenAlpha>().to = 0f;
        addPower.GetComponent<TweenAlpha>().ResetToBeginning();
        addPower.GetComponent<TweenAlpha>().enabled = true;
    }

    // 모두 받기 버튼 클릭시 동작
    public void OnClickTakeAll()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		List<int> keys = new List<int>();
		int count = GameCore.Instance.PlayerDataMgr.GetFarmingCount();
		for(int i = 0; i < count; ++i)
		{
			var data = GameCore.Instance.PlayerDataMgr.GetFarmingData(i);
			if (data.endTime < GameCore.nowTime)
				keys.Add(data.id);
		}
		if (keys.Count == 0)
			GameCore.Instance.ShowAlert("완료된 파밍이 없습니다.");
		else
			GameCore.Instance.NetMgr.Req_Farming_Done(keys.ToArray());
	}


    Coroutine coroutineUnLock;
    private void UnlockNextStage()
    {
        effectUnlock.enabled = true;
        if (coroutineUnLock != null)
            StopCoroutine(coroutineUnLock);
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Chapter_Release);
        coroutineUnLock = StartCoroutine(UnLockAnimation());
    }
    private IEnumerator UnLockAnimation()
    {
        yield return new WaitForSeconds(1.667f);
        if (effectUnlock != null)
            effectUnlock.enabled = false;
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
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(0);
        te.SetAlignment(InvenBase.HeroSortByRankDescending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //랭크기준 오름차순 정렬
    public void OnClickSortByRankAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(1);
        te.SetAlignment(InvenBase.HeroSortByRankAscending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 내림차순 정렬
    public void OnClickSortByNameDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(2);
        te.SetAlignment(InvenBase.HeroSortByNameDescending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //이름기준 오름차순 정렬
    public void OnClickSortByNameAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(3);
        te.SetAlignment(InvenBase.HeroSortByNameAscending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 내림차순 정렬
    public void OnClickSortByGetDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(4);
        te.SetAlignment(InvenBase.HeroSortByGetDescending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //획득기준 오름차순 정렬
    public void OnClickSortByGetAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(5);
        te.SetAlignment(InvenBase.HeroSortByGetAscending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 내림차순 정렬
    public void OnClickSortByPowerDescending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        UpdateAlignmentButton(6);
        te.SetAlignment(InvenBase.HeroSortByPowerDescending);
        if (filterObj == null) te.DoFilter(31);
        else te.DoFilter(filterObj.GetComponent<FilterCtrl>().GetFilter());

        alignmentBox.gameObject.SetActive(false);
    }
    //공격력 오름차순 정렬
    public void OnClickSortByPowerAscending()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
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

    private void Update()
    {
#if UNITY_EDITOR

        if (Input.GetMouseButtonDown(0))
        {
            if (alignmentBox != null)
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
        }
#endif
        if (Input.touchCount == 0) return;

        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (alignmentBox != null)
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
    }
    #endregion

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 12:
                nTutorialList.Add(new ReturnTutorialData(list[0].GetTransformButton1(), 0));
                var tfTable = te.list.tfTable;
                Transform tfTableData = null;
                foreach(var transformData in tfTable)
                {
                    tfTableData = transformData.Key;
                    break;
                }
                nTutorialList.Add(new ReturnTutorialData(tfTableData, 5));
                //nTutorialList.Add(new ReturnTutorialData(te.Slot.GetHeroSlot(0), 3));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
