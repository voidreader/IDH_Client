using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryUI : MonoBehaviour, ISequenceTransform
{
    // Left
    UI2DSprite spIllust;
    UI2DSprite spIllustTmp;
    [SerializeField] UI2DSprite spIllustBlind_1;
    [SerializeField] UI2DSprite spIllustBlind_2;
    GameObject lockIcon;
    UILabel lbLock;
    TouchSlidScript slid;

    UIButton btDisc;

    UILabel lbDisc;
    private int lbSettingHeight;

    private UIDragScrollView lbDiscDragScrollView;
    private float ibDiscDragScrollViewHeight;

    Vector2 lbDiscPosition;
    [SerializeField] UIPanel panelDisc;
    [SerializeField] float lbDiscScrollSpeed;

    UIButton btChapterLeft;
    UIButton btChapterRight;
    UILabel lbName;

    // Right
    UILabel lbChapter;
    UILabel lbCounter;
    UIGrid contentRoot;
    StoryListItem[] items;
    private UIScrollView scrollview;
    private float cellHeight;

    // Bottom
    UILabel lbRewardGuide;
    UIButton btChangeDiff;
    GameObject diifComboBox;
    UILabel[] diffComboTexts;

    UILabel lbStarCount;
    GameObject[] rewardRoots;
    CardBase[] rewardCards;


    Action<int> cbPrepare;

    StoryChapterDataMap chapterData;

    // variables
    int starCount;
    static int chapter = 1;
    static int diffcult = 1;
    int openCount;
    bool bChapterAnim; // 챕터 전환 애니메이션 중일 때 true
    bool bUnlockAnim; // 챈터 열림 애니메이션 중일때 true
    int chapterCharacterImgId;
    bool bLock;
    bool isFirst = false;   //처음 스테이지 선택창으로 들어왔을 경우
    [SerializeField] Animator effectUnlock;
    int checkLastOpen = 0;
    internal static StoryUI Create()
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Story/PanelStoryUI", GameCore.Instance.ui_root);
        var result = go.GetComponent<StoryUI>();
        result.InitLink();
        return result;
    }

    private void InitLink()
    {
        effectUnlock.enabled = false;
        //if (CheckUnLock() == true) spIllustBlind_1.gameObject.SetActive(true);
        //if (CheckUnLock() == true) spIllustBlind_2.gameObject.SetActive(true);

        // Left
        slid = UnityCommonFunc.GetComponentByName<TouchSlidScript>(gameObject, "PanelIllustRoot");
        slid.SetCallbackEndDrag(CBIllustEndDrag);
        spIllust = UnityCommonFunc.GetComponentByName<UI2DSprite>(gameObject, "Illust");
        var tw = spIllust.GetComponent<TweenPosition>();
        tw.onFinished.Add(new EventDelegate(() => {
            GameCore.Instance.SetUISprite(spIllust, chapterCharacterImgId);
            GameCore.Instance.SetUISprite(spIllustBlind_1, chapterCharacterImgId);
            spIllust.color = bLock ? Color.black : Color.white;
            spIllust.transform.localPosition = Vector3.zero;
            bChapterAnim = false;
        }));
        spIllustTmp = UnityCommonFunc.GetComponentByName<UI2DSprite>(gameObject, "Illust2");
        tw = spIllustTmp.GetComponent<TweenPosition>();
        tw.onFinished.Add(new EventDelegate(() => {
            //spIllustTmp.alpha = 0f;
            //새로운 챕터가 처음 열릴 경우

        }));
        //spIllustBlind = UnityCommonFunc.GetComponentByName<UI2DSprite>(gameObject, "IllustBlind");
        spIllustBlind_1.gameObject.SetActive(false);
        spIllustBlind_2.gameObject.SetActive(false);

        lockIcon = UnityCommonFunc.GetGameObjectByName(gameObject, "LockIcon");
        lbLock = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "LockLabel");

        //======여기부터=======
        lbDiscDragScrollView = UnityCommonFunc.GetComponentByName<UIDragScrollView>(gameObject, "lbDiscScroll");
        ibDiscDragScrollViewHeight = lbDiscDragScrollView.GetComponent<UIWidget>().height;
        lbDisc = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbDisc");
        lbDiscPosition.x = 17f;
        lbSettingHeight = 25;

        btDisc = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btDisc");
        btDisc.onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            lbDiscDragScrollView.gameObject.SetActive(!lbDiscDragScrollView.gameObject.activeSelf);
            SetLBDiscScroll();
        }));

        lbName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "StoryName");
        btChapterLeft = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "left");
        btChapterRight = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "right");
        btChapterLeft.onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            SetChapter(chapter - 1);
        }));
        btChapterRight.onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            SetChapter(chapter + 1);
        }));

        // Right
        lbChapter = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbChapter");
        lbCounter = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "openStoryCount");
        contentRoot = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "content");
        items = new StoryListItem[10];
        for (int i = 0; i < items.Length; ++i)
            items[i] = StoryListItem.Create(contentRoot.transform);
        scrollview = contentRoot.transform.parent.GetComponent<UIScrollView>();
        cellHeight = contentRoot.cellHeight;

        //bottom
        lbRewardGuide = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "rewardGuide");
        diifComboBox = UnityCommonFunc.GetGameObjectByName(gameObject, "diffComboRoot");
        diffComboTexts = new UILabel[3];
        for (int i = 0; i < diffComboTexts.Length; ++i)
        {
            int n = i + 1;
            var button = UnityCommonFunc.GetComponentByName<UIButton>(diifComboBox, "comboItem" + n);
            button.onClick.Add(new EventDelegate(() => SetDiffcult(n)));
            diffComboTexts[i] = button.GetComponentInChildren<UILabel>();
        }
        btChangeDiff = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "DiffButton");
        btChangeDiff.onClick.Add(new EventDelegate(() => {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            diifComboBox.gameObject.SetActive(!diifComboBox.gameObject.activeSelf);
        }));

        lbStarCount = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "srCntLabel");
        rewardRoots = new GameObject[3] {
            UnityCommonFunc.GetGameObjectByName(gameObject, "rwitem1Root"),
            UnityCommonFunc.GetGameObjectByName(gameObject, "rwitem2Root"),
            UnityCommonFunc.GetGameObjectByName(gameObject, "rwitem3Root")
        };
        rewardCards = new CardBase[3];
    }

    internal void SetActive(bool _active)
    {
        gameObject.SetActive(_active);
        if (_active == false)
            effectUnlock.enabled = false;
    }

    internal void Init(Action<int> _cbPrepare)
    {
        cbPrepare = _cbPrepare;
        diffcult = GameCore.difficult == -1 ? (GameCore.Instance.PlayerDataMgr.GetStorySDataDic - 7000000) / 100000 + 1 : GameCore.difficult;
        int chapterSaveInfo = GameCore.chapterSave == -1 ?
                                GameCore.Instance.DataMgr.GetStoryData(GameCore.Instance.PlayerDataMgr.GetStorySDataDic).chapter :
                                GameCore.chapterSave;

        if (GameCore.Instance.PlayerDataMgr.CheckUnlockStory() == true)
        {
            var prev = GameCore.Instance.DataMgr.GetStoryData(GameCore.Instance.PlayerDataMgr.GetUnlockStoryData().openCondition);
            SetUI(prev.chapter, prev.difficult, true);

            if (gameObject.activeSelf)
                StartUnlockAnim();
        }
        else
        {
            SetUI(chapterSaveInfo, diffcult, true, false, GameCore.stageSave);
        }
    }

    public void StartUnlockAnim()
    {
        var now = GameCore.Instance.PlayerDataMgr.GetUnlockStoryData();

        GameCore.Instance.SetActiveBlockPanelInvisable(true);
        GameCore.Instance.DoWaitCall(1f, () =>
        {
            bUnlockAnim = false;
            SetUI(now.chapter, now.difficult, true, true);
            SetChapterLeft(true, true, 0);
            bUnlockAnim = true;
        });
        GameCore.Instance.DoWaitCall(2f, () =>
        {
            UnlockNextStage();
            GameCore.Instance.SetActiveBlockPanelInvisable(false);
        });

        bUnlockAnim = true;
    }


    internal void Destroy()
    {
        if (gameObject != null)
            GameObject.Destroy(gameObject);
    }


    internal void SetChapter(int _chapter) { SetUI(_chapter, diffcult); }
    internal void SetDiffcult(int _diff)
    {
        //StoryDataMap storyDataMap = GameCore.Instance.DataMgr.GetStoryData(GameCore.Instance.PlayerDataMgr.GetStorySDataDic);
        //chapter = (_diff >= storyDataMap.difficult) ? storyDataMap.chapter : GameCore.Instance.DataMgr.GetStoryConstData().maxChapter;
        SetUI(chapter, _diff);
    }


    internal void SetUI(int _chapter, int _diff, bool _force = false, bool _NoLeftAnim = false, int _stageNum = -1)
    {
        if (bChapterAnim || bUnlockAnim)
            return;

        bool bChangeChapter = _chapter != chapter;
        bool bChangeDiff = _diff != diffcult;
        bool _left = chapter != _chapter ? (chapter < _chapter) : (diffcult > _diff);
        if (_force)
            bChangeChapter = bChangeDiff = true;

        var maxChapter = GameCore.Instance.DataMgr.GetStoryConstData().maxChapter;
        _chapter = ((maxChapter + (_chapter - 1)) % maxChapter) + 1;

        var chapterKey = StoryChapterDataMap.GenerateChapterKey(_chapter, _diff);

        chapterData = GameCore.Instance.DataMgr.GetStoryChapterData(chapterKey);
        if (chapterData == null)
            return;

        if (bChangeChapter)
        {
            chapter = _chapter;
            lbRewardGuide.text = string.Format("스토리{0} 별 보상", chapter);
        }

        if (bChangeDiff)
        {
            diffcult = _diff;
            UpdateDiffcultButton();
        }

        if (bChangeChapter | bChangeDiff)
        {
            //lbDisc.gameObject.SetActive(true);
            diifComboBox.SetActive(false);
            UpdateListItem(_stageNum);
            UpdateReward();
            if (!_NoLeftAnim)
                SetChapterLeft(!_force, _left, openCount);
        }
    }

    internal int GetChapterStarCount(int _chap, int _diff)
    {
        int result = 0;
        for (int i = 0; i < 10; ++i)
        {
            int key = StoryDataMap.GenerateStoryKey(_chap, _diff, i + 1);
            var sdata = GameCore.Instance.PlayerDataMgr.GetStorySData(key);
            if (sdata != null)
                result += sdata.GetMossionClearCount();
        }
        return result;
    }

    internal void UpdateReward()
    {
        // Star Count Update;
        starCount = GetChapterStarCount(chapter, diffcult);
        lbStarCount.text = starCount.ToString();

        // Set Slider
        for (int i = 0; i < 3; ++i)
            rewardRoots[i].GetComponentInChildren<UISlider>().value = (float)(starCount - (i * 10)) / 10;

        // Reward Item Setting
        for (int i = 0; i < rewardCards.Length; ++i)
        {
            CardDataMap data;
            if (CardDataMap.IsItemKey(chapterData.rewardID[i]))
                data = GameCore.Instance.DataMgr.GetItemData(chapterData.rewardID[i]);
            else
                data = GameCore.Instance.DataMgr.GetUnitData(chapterData.rewardID[i]);

            var type = data.type;
            if (rewardCards[i] == null)
            {
                int n = i;
                rewardCards[i] = CardBase.CreateSmallCardByKey(chapterData.rewardID[i], rewardRoots[i].transform,
                                                                                                                (key) => OnClickRewardItem(n),
                                                                                                                (key) => GameCore.Instance.ShowCardInfoNotHave((int)key));
            }
            else
            {
                rewardCards[i].Init(null, data, null, (key) => GameCore.Instance.ShowCardInfoNotHave((int)key));
            }
            rewardCards[i].SetCount(chapterData.rewardCount[i]);

            // Reward Item Set State

            var sdata = GameCore.Instance.PlayerDataMgr.GetStorychapterSData(chapterData.chapter, chapterData.difficult, i + 1);
            if (sdata == null)
                SetRewardItemState(i, 2);
            else
            {
                if (sdata.reward == false)
                    SetRewardItemState(i, 1);
                else
                    SetRewardItemState(i, 0);
            }
        }

    }

    //----현재 내가 건드려야될 함수------
    internal void UpdateListItem(int _stageNum = -1)
    {
        // Update List Item
        openCount = 0;
        checkLastOpen = 0;
        for (int i = 0; i < 10; ++i)
        {
            int missionflag = 0;
            bool bOpen = false;
            bool bClear = false;

            
            int key = StoryDataMap.GenerateStoryKey(chapter, diffcult, i + 1);
            var sdata = GameCore.Instance.PlayerDataMgr.GetStorySData(key);
            if (sdata == null)
            {
                //if (data.openCondition == 0 || GameCore.Instance.PlayerDataMgr.GetStorySData(data.openCondition) != null)
                //	bOpen = true;
            }
            else
            {
                missionflag = sdata.GetMissionClearFlag();
                bOpen = true;
                bClear = sdata.clear;

                if (0 < _stageNum && (GameCore.Instance.lobbyTutorial == null || !GameCore.Instance.lobbyTutorial.IsRunning))
                    checkLastOpen = _stageNum - 1;
                else
                    checkLastOpen = i;


            }
            items[i].Init(key, bOpen, bClear, missionflag, () => cbPrepare(key));

            if (bOpen)
                ++openCount;
        }

        //시작 지점의 높이를 구하는 부분
        float startHeight;
        //GameCore에 저장된 스테이지 정보가 없을 경우, 가장 최신의 스테이지 정보로 이동
        int stageSaveInfo = GameCore.stageSave;
        int limitPos = items.Length - 3;
        if (stageSaveInfo != -1 && isFirst == false)
        {
            isFirst = true;
            int stageSavePos = stageSaveInfo;
            //startHeight = stageSavePos >= limitPos ? cellHeight * (limitPos - 1) + cellHeight * 0.4f : cellHeight * (stageSavePos);
            startHeight = checkLastOpen >= limitPos ? cellHeight * (limitPos - 1) + cellHeight * 0.4f : cellHeight * (checkLastOpen);
        }
        else
        {
            startHeight = checkLastOpen >= limitPos ? cellHeight * (limitPos - 1) + cellHeight * 0.4f : cellHeight * (checkLastOpen);
        }

        if( GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning)
            contentRoot.transform.localPosition = new Vector3(-337, -9, 0) + new Vector3(0, Mathf.Max(0, startHeight), 0);
        else
            SpringPanel.Begin(scrollview.panel.cachedGameObject, new Vector3(0, startHeight, 0), 6f);

        // Chapter Number Print
        lbChapter.text = "STORY " + chapter;

        // Open Story Count Print
        lbCounter.text = openCount + "[c] / 10";

    }
    Coroutine coroutineUnLock;
    private void UnlockNextStage()
    {
        GameCore.Instance.PlayerDataMgr.ResetUnlockStoryKey();

        spIllustBlind_1.gameObject.SetActive(false);
        spIllustBlind_2.gameObject.SetActive(false);
        effectUnlock.enabled = true;
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Chapter_Release);
        if (coroutineUnLock != null)
            StopCoroutine(coroutineUnLock);
        coroutineUnLock = StartCoroutine(UnLockAnimation());
    }
    private IEnumerator UnLockAnimation()
    {
        yield return new WaitForSeconds(1.667f);
        if (effectUnlock != null)
            effectUnlock.enabled = false;

        coroutineUnLock = null;
        bUnlockAnim = false;
    }

    internal void UpdateDiffcultButton()
    {
        var lb = btChangeDiff.GetComponentInChildren<UILabel>();
        lb.text = StoryDataMap.GetStrDiffcult(diffcult);

        for (int i = 0; i < diffComboTexts.Length; ++i)
        {
            if (i + 1 == diffcult)
                diffComboTexts[i].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
            else
                diffComboTexts[i].color = Color.white;
        }
    }
    public void CheckLabelDiscPosition()
    {
        if (isLableDiscMove == false)
            return;

        ResetLabelDiscPosition();
        float discPanelPosY = panelDisc.clipOffset.y;
        if (lbDiscDragScrollView.scrollView.isDragging == true)
            return;
        SetPanelDisc(discPanelPosY -= GameCore.timeScale * lbDiscScrollSpeed);

    }
    private void ResetLabelDiscPosition()
    {
        float discPanelPosY = panelDisc.clipOffset.y;

        if (discPanelPosY >= 0)
        {
            SetPanelDisc(-(lbDisc.height + ibDiscDragScrollViewHeight + 0.1f));
        }
        else if (discPanelPosY <= -(lbDisc.height + ibDiscDragScrollViewHeight))
        {
            SetPanelDisc(0 - 0.1f);
        }

    }
    private void SetPanelDisc(float value)
    {
        lbDiscPosition.y = value;
        panelDisc.clipOffset = lbDiscPosition;
        lbDiscPosition.y *= -1f;
        panelDisc.transform.localPosition = lbDiscPosition;
    }
    private bool isLableDiscMove = false;
    private void SetLBDiscScroll()
    {
        lbDisc.text = GameCore.Instance.DataMgr.GetStoryStringData(chapterData.discID);
        int labelLength = (int)(lbDisc.printedSize.y / lbDisc.fontSize);
        if (labelLength >= 3)
        {
            isLableDiscMove = true;
            lbDisc.height = lbSettingHeight * (labelLength + 1);
            lbDiscDragScrollView.enabled = true;
            SetPanelDisc(-ibDiscDragScrollViewHeight * 0.6f);
        }
        else
        {
            isLableDiscMove = false;
            lbDisc.height = lbSettingHeight * 2;
            lbDiscDragScrollView.enabled = false;
            SetPanelDisc(-ibDiscDragScrollViewHeight * 0.9f);
        }
    }


    internal void SetChapterLeft(bool _animation, bool _left, int _openCount)
    {
        bLock = _openCount == 0;

        spIllustBlind_1.gameObject.SetActive(spIllustBlind_2.gameObject.activeSelf);
        spIllustBlind_2.gameObject.SetActive(bLock);
        spIllustBlind_2.alpha = 1f;

        lbName.text = chapterData.name;
        SetLBDiscScroll();
        //lbDisc.text = GameCore.Instance.DataMgr.GetStoryStringData(chapterData.discID);
        if (_animation)
        {
            // Set Illust
            {
                var twAlpha = spIllust.GetComponent<TweenAlpha>();
                twAlpha.from = 1f;
                twAlpha.to = 0f;
                twAlpha.ResetToBeginning();
                twAlpha.PlayForward();

                var twPosition = spIllust.GetComponent<TweenPosition>();
                twPosition.from = new Vector3(0f, 0f, 0f);
                twPosition.to = new Vector3(_left ? -260f : 260f, 0f, 0f);
                twPosition.ResetToBeginning();
                twPosition.PlayForward();

                chapterCharacterImgId = chapterData.imgID;
                bChapterAnim = true;
            }

            {
                GameCore.Instance.SetUISprite(spIllustTmp, chapterData.imgID);
                GameCore.Instance.SetUISprite(spIllustBlind_2, chapterData.imgID);

                if (bLock && !GameCore.Instance.PlayerDataMgr.CheckUnlockStory())
                {
                    spIllustTmp.isGray = true;
                    spIllustTmp.color = Color.gray;
                }
                else
                {
                    spIllustTmp.isGray = false;
                    spIllustTmp.color = Color.white;
                }
                //spIllustTmp.color = bLock ? Color.black : Color.white;
                var twAlpha = spIllustTmp.GetComponent<TweenAlpha>();
                twAlpha.from = 0f;
                twAlpha.to = 1f;
                twAlpha.ResetToBeginning();
                twAlpha.PlayForward();

                var twPosition = spIllustTmp.GetComponent<TweenPosition>();
                twPosition.from = new Vector3(_left ? 260f : -260f, 0f, 0f);
                twPosition.to = new Vector3(0f, 0f, 0f);
                twPosition.ResetToBeginning();
                twPosition.PlayForward();
            }
        }
        else
        {
            GameCore.Instance.SetUISprite(spIllust, chapterData.imgID);
            spIllust.color = bLock ? Color.black : Color.white;
            spIllustTmp.alpha = 0f;
        }


        lockIcon.SetActive(bLock);
        lbLock.gameObject.SetActive(bLock && !GameCore.Instance.PlayerDataMgr.CheckUnlockStory());
        if (bLock)
        {
            var storyData = GameCore.Instance.DataMgr.GetStoryDataByIndex(chapter, 1, diffcult);
            var data = GameCore.Instance.DataMgr.GetStoryData(storyData.openCondition);

            lbLock.text = string.Format("스토리 {0}-{1} {2}을 클리어 하세요.", data.chapter, data.stage, StoryDataMap.GetStrDiffcult(data.difficult));
        }
    }

    private void OnClickRewardItem(int _idx)
    {
        if (starCount < (_idx + 1) * 10)
            return;

        var data = GameCore.Instance.PlayerDataMgr.GetStorychapterSData(chapter, diffcult, _idx + 1);
        if (data == null || data.reward == true)
            return;

        GameCore.Instance.NetMgr.Req_Story_Reward(chapterData.id, _idx + 1);
        //GameCore.Instance.ShowAlert("받는척함");
        //GameCore.Instance.ShowReceiveItem(new int[] { chapterData.rewardID[_idx] });
        //SetRewardItemState(_idx, 0);

        // Todo : Take Item

        return;
    }

    internal void SetRewardItemState(int _idx, int _state)
    {
        var effect = UnityCommonFunc.GetGameObjectByName(rewardRoots[_idx], "effect");
        var obtain = UnityCommonFunc.GetGameObjectByName(rewardRoots[_idx], "spObtained");

        switch (_state)
        {
        case 0: // 획득됨
            rewardCards[_idx].SetEnable(false);
            effect.SetActive(false);
            obtain.SetActive(true);
            break;

        case 1: // 획득 할 수 있음
            rewardCards[_idx].SetEnable(true);
            effect.SetActive(true);
            obtain.SetActive(false);
            break;

        case 2: // 획득 불가
            rewardCards[_idx].SetEnable(true);
            effect.SetActive(false);
            obtain.SetActive(false);
            break;
        }

    }

    private void CBIllustEndDrag(Vector2 _vec)
    {
        if (_vec.x < 0)
            SetChapter(chapter + 1);
        else
            SetChapter(chapter - 1);
    }

    public bool GetNowGachaPlaying()
    {
        if (GameObject.Find("ReceiveItemRoot") != null)
        {
            return GameObject.Find("ReceiveItemRoot").GetComponent<ReceiveEffectUI>().GetNowGachaPlaying();
        }
        else
            return false;
    }

    private void Update()
    {
        if (diifComboBox.activeInHierarchy)
        {
            //#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            //#else
            //			if(Input.GetTouch(0).phase == TouchPhase.Began)
            //#endif
            {
                RaycastHit2D hit;
                if (UnityCommonFunc.GetCameraHitInfo2D(GameCore.Instance.GetUICam(), out hit, "UI"))
                {
                    var Colls = diifComboBox.transform.parent.GetComponentsInChildren<Collider2D>();

                    for (int i = 0; i < Colls.Length; ++i)
                        if (Colls[i] == hit.collider)
                            return;
                }
                diifComboBox.SetActive(false);
            }
        }
    }

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
        case 1:
            nTutorialList.Add(new ReturnTutorialData(items[checkLastOpen].GetRewardTransform(0), 1));
            nTutorialList.Add(new ReturnTutorialData(items[checkLastOpen].GetPrepareButton(), 0));
            break;
        case 4:
            nTutorialList.Add(new ReturnTutorialData(items[checkLastOpen].GetPrepareButton(), 0));
            break;
        case 5:
            nTutorialList.Add(new ReturnTutorialData(items[checkLastOpen].GetPrepareButton(), 0));
            break;
        default:
            break;
        }
        return nTutorialList;
    }
}