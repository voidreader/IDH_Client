using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using IDH.MyRoom;

public class MyRoomUIController : MonoBehaviour
{
    public class WorldToScreenUiObject
    {
        public GameObject UiObject;
        public Transform WorldTarget;

        public void Update()
        {
            UiObject.transform.position = GameCore.Instance.WorldPositionToUiPosition(WorldTarget.position);
        }
    }

    public class ChangeMyRoomButton
    {
        public static int lastBuyRoomIndex;

        public Action<int> OnClickButtonDelegate;

        public GameObject Notification { get; set; }
        public UISprite ButtonSprite { get; set; }
        public UIButton Button { get; set; }

        public UIButton btBuy { get; set; }
        public BoxCollider2D Collider { get; set; }
        private Color ActiveColor { get; set; }
        private Color DeActiveColor { get; set; }
        private int Index { get; set; }

        public ChangeMyRoomButton(int index, UISprite buttonSprite, UIButton button, BoxCollider2D collider)
        {
            Index = index;
            ButtonSprite = buttonSprite;
            Button = button;
            Button.onClick.Add(new EventDelegate(() => { GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button); if (OnClickButtonDelegate != null) OnClickButtonDelegate.Invoke(index); }));
            btBuy = UnityCommonFunc.GetComponentByName<UIButton>(Button.gameObject, "btBuy");
            btBuy.onClick.Add(new EventDelegate(ShowBuyRoom));
            btBuy.transform.parent = Button.transform.parent;
            Collider = collider;
            ActiveColor = ButtonSprite.color;
            DeActiveColor = new Color(ActiveColor.r, ActiveColor.g, ActiveColor.b, 0.5f);
            Notification = UnityCommonFunc.GetGameObjectByName(button.gameObject, "highlight");
        }

        public void SetActive(bool isActive)
        {
            ButtonSprite.color = isActive ? ActiveColor : DeActiveColor;
            Button.enabled = isActive;
            Collider.enabled = isActive;
            btBuy.gameObject.SetActive(!isActive);
        }

        public void ShowBuyRoom()
        {
            var script = MyRoomBuyScript.Create(GameCore.Instance.ui_root);
            script.Init(GameCore.Instance.DataMgr.GetMyRoomData(Index+1));
            lastBuyRoomIndex = Index;

            GameCore.Instance.ShowObject("숙소 해제", null, script.gameObject, 2, new MsgAlertBtnData[] {
                new MsgAlertBtnData("취소", new EventDelegate(GameCore.Instance.CloseMsgWindow)),
                new MsgAlertBtnData("확인", new EventDelegate(()=>{
                    GameCore.Instance.CloseMsgWindow();
                    GameCore.Instance.NetMgr.Req_MyRoom_Buy(Index+1);
                }))
            });

            script.transform.localPosition = Vector3.zero;
        }
    }

    public class EditToolController
    {
        public bool IsActive { get; private set; }
        public WorldToScreenUiObject PositiveButton { get; set; }
        //public WorldToScreenUiObject NegativeButton { get; set; }
        public WorldToScreenUiObject FlippingButton { get; set; }
        public WorldToScreenUiObject RecallButton { get; set; }

        public void SetActive(bool trueFalse, bool isActiveNegativeButton)
        {
            PositiveButton.UiObject.SetActive(trueFalse);
            FlippingButton.UiObject.SetActive(trueFalse);
            RecallButton.UiObject.SetActive(trueFalse);

            //if (isActiveNegativeButton) NegativeButton.UiObject.SetActive(trueFalse);
            //else
            //{
            //    if(NegativeButton.UiObject.activeInHierarchy)
            //        NegativeButton.UiObject.SetActive(false);
            //}

            IsActive = trueFalse;
        }

        public void Update()
        {
            PositiveButton.Update();
            //NegativeButton.Update();
            FlippingButton.Update();
            RecallButton.Update();
        }
    }

    [Header("Using Prefab")]
    public MyRoomFrindInfo MyRoomFriendPrefab;
    public MyRoomHistoryLog MyRoomHistoryLogPrefab;
    public MyRoomPlacedObjectBox MyRoomPlacedObjectBoxPrefab;
    public MyRoomHeroBox MyRoomHeroBoxPrefab;

    [Header("Ref Object")]
    public GameObject MainPanel;
    public GameObject EditPanel;
    public GameObject MyRoomList;
    public GameObject HistoryList;
    public GameObject MyRoomInventory;
    public GameObject VisitFriendList;
    public GameObject PlacedHeroListObject;
    public GameObject PlacedItemListObject;
    public MyRoomSatisfactionInfo SatisfactionInfo;
    public MyRoomSatisfactionInfo SatisfactionInfoEdit;

    [Header("Ref Object[EditTools]")]
    public GameObject EditToolPositive;
    public GameObject EditToolFlipping;
    public GameObject EditToolRecall;
    public GameObject EditToolNegative;

    public UIButton MainMyRoomButtol;
    public GameObject goCleaningAllButton;
    public GameObject goCleaningRewardAllButton;


    private ChangeMyRoomButton[] ChangeMyRoomButtonList;


    private List<MyRoomHistoryLogData> HistoryDataList { get; set; }
    private List<MyRoomFriendData> FriendList { get; set; }
    private MyRoomSystemCommand Command { get; set; }
    private MyRoomSystemObserver Observer { get; set; }
    private List<MyRoomObject> PlacedObjectList { get; set; }
    private List<MyRoomHeroObject> PlacedHeroList { get; set; }
    /// <summary>
    /// 다른쪽에 전달용으로만 씀(임시)
    /// </summary>
    protected MyRoomSystemRefParameter RefParameter { get; set; }
    /// <summary>
    /// 데이터상의 인벤토리(MyRoomInventoryData)
    /// </summary>
    private MyRoomInventoryData Inventory { get; set; }

    /// <summary>
    ///  UI 및 기존 IDH인벤토리(MyRoomInventory)
    /// </summary>
    private MyRoomInventory UIInventory { get; set; }

    private EditToolController EditTool;

    private List<MyRoomPlacedObjectBox> placedObjectBoxList = new List<MyRoomPlacedObjectBox>();
    private UIGrid placedItemListBoxGrid;
    private List<MyRoomHistoryLog> logObjectList = new List<MyRoomHistoryLog>();

    private List<MyRoomHeroBox> placedHeroboxList = new List<MyRoomHeroBox>();
    private UIGrid placedHeroListBoxGrid;

    protected virtual void Awake()
    {
        for (int i = 0; i < transform.childCount; ++i) transform.GetChild(i).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);

        EditTool = new EditToolController();

        EditTool.PositiveButton = new WorldToScreenUiObject();
        EditTool.PositiveButton.UiObject = EditToolPositive;
        EditTool.PositiveButton.WorldTarget = new GameObject("EditToolPositivePivot").transform;

        EditTool.FlippingButton = new WorldToScreenUiObject();
        EditTool.FlippingButton.UiObject = EditToolFlipping;
        EditTool.FlippingButton.WorldTarget = new GameObject("EditToolFlippingPivot").transform;

        EditTool.RecallButton = new WorldToScreenUiObject();
        EditTool.RecallButton.UiObject = EditToolRecall;
        EditTool.RecallButton.WorldTarget = new GameObject("EditToolRecallPivot").transform;

        //EditTool.NegativeButton = new WorldToScreenUiObject();
        //EditTool.NegativeButton.UiObject = EditToolNegative;
        //EditTool.NegativeButton.WorldTarget = new GameObject("EditToolNegativePivot").transform;

        EditTool.SetActive(false, true);

        ChangeMyRoomButtonList = new ChangeMyRoomButton[MyRoomList.transform.childCount];

        for (int i = 0; i < ChangeMyRoomButtonList.Length; ++i)
        {
            UISprite buttonSpriteTemp = MyRoomList.transform.GetChild(i).GetComponent<UISprite>();
            UIButton buttonTemp = buttonSpriteTemp.GetComponent<UIButton>();
            BoxCollider2D colliderTemp = buttonSpriteTemp.GetComponent<BoxCollider2D>();
            ChangeMyRoomButtonList[i] = new ChangeMyRoomButton(i,buttonSpriteTemp, buttonTemp,colliderTemp);
            ChangeMyRoomButtonList[i].OnClickButtonDelegate = OnClickChangeRoomButton;
            ChangeMyRoomButtonList[i].SetActive(false);

            UIButton btBuy = UnityCommonFunc.GetComponentByName<UIButton>(buttonSpriteTemp.gameObject, "btBuy");
        }
    }

    public virtual void Initialize(MyRoomSystemRefParameter parameter)
    {
        InitParams(parameter);

        InitObserver();

        InitInventory();
        
        for (int i = 0; i < parameter.OpendUserMyRoom.Length; ++i)
            if (parameter.OpendUserMyRoom[i])
                ActiveRoomButton(i);

        SatisfactionInfo.Initialize(parameter);
        SatisfactionInfo.gameObject.SetActive(true);
        SatisfactionInfoEdit.Initialize(parameter);
        SatisfactionInfoEdit.gameObject.SetActive(false);
        goCleaningAllButton.SetActive(false);
        goCleaningRewardAllButton.SetActive(!parameter.IsVisit);
    }


    protected void InitParams(MyRoomSystemRefParameter parameter)
    {
        RefParameter = parameter;

        HistoryDataList = parameter.HistoryLogDataList;
        FriendList = parameter.FriendList;
        Inventory = parameter.InventoryData;
        Command = parameter.Command;
        Observer = parameter.Observer;
        PlacedObjectList = parameter.PlacedObjectList;
        PlacedHeroList = parameter.PlacedHeroList;
    }

    protected void InitObserver()
    {
        Observer.OnUpdatedHistoryData += UpdateHistoryList;
        Observer.OnStartMyRoomObjectEditMode += TurnOnEditTool;
        Observer.OnStartMyRoomObjectEditMode += OnStartMyRoomObjectEditMode;
        Observer.OnEndMyRoomObjectEditMode += OnEndMyRoomObjectEditMode;
        Observer.OnEndMyRoomObjectEditMode += TurnOffEditTools;
        Observer.OnPlacedMyRoomObject += AddMyRoomPlacedObjectBox;
        Observer.OnPlacedMyRoomHero += AddMyRoomHeroBox;
        Observer.OnDeletedMyRoomObject += DeleteMyRoomPlacedObjectBox;
        Observer.OnDeletedMyRoomObject += DeleteMyRoomHeroBox;
        Observer.OnInitializedMyRoomSystem += InitializeGridObjects;
        Observer.OnInitializedMyRoomSystem += InitializeRoomButton;
        Observer.OnChangedMainRoom += OnChangedMainMyRoom;
        Observer.OnRayCastCheckNull += CloseList;
    }

    protected void InitInventory()
    {
        UIInventory = MyRoomInventory.GetComponent<MyRoomInventory>();

        InvenBase.TypeFlag flags = InvenBase.TypeFlag.Character |
                        //InvenBase.TypeFlag.SetInterior |
                        InvenBase.TypeFlag.Furniture |
                        InvenBase.TypeFlag.Prop |
                        InvenBase.TypeFlag.Wall |
                        InvenBase.TypeFlag.Floor;


        //새로바뀐거는 영웅과 아이템 리스트를 파라미터로 받음 근데 마이룸 init을 수정할수없어서 셋을 먼저 하는것으로 대체
        UIInventory.SetList(Inventory.HeroSdataDic.Values.ToList(),
                            Inventory.ItemSdataDic.Values.ToList());
        UIInventory.Init(flags, true);
        UIInventory.GetArrangeButton().onClick.Add(new EventDelegate(() => OnClickPlaceButtonAtInventory()));

        UIInventory.GetCloseButton().onClick.Add(new EventDelegate(() => Command.CmdStartEditMode()));
        UIInventory.GetCloseButton().onClick.Add(new EventDelegate(() => RefParameter.NegativeButtonStatck.Pop()));

        UIInventory.GetUnArrangeButton().onClick.Add(new EventDelegate(() =>
        {
            ItemList temp = UIInventory.GetSelectedInvenItem();
            long[] selectList = temp.GetSelects();

            if (selectList.Length == 0) return;
            // 아몰랑 일단 한개만
            HeroSData heroData;
            if (GameCore.Instance.PlayerDataMgr.HeroSdataDic.TryGetValue(selectList[0], out heroData) == false) return;
            if (heroData.dormitory == RefParameter.CurrentRoomID)
            {
                MyRoomHeroObject heroObject = PlacedHeroList.Find(hero => hero.HeroData.ServerData.uid == heroData.uid);
                if (heroObject == null) return;
                heroData.dormitory = 0;
                Command.CmdMyRoomObjectRetrunToInventory(heroObject, false);
            }
            else
            {
                heroData.dormitory = 0;
                List<HeroSData> list = new List<HeroSData>();
                list.Add(heroData);
                Command.CmdHeroReturnToInventory(list);
            }

            UIInventory.UpdateCard();
        }
        ));
    }

    private void InitializeGridObjects()
    {
        placedItemListBoxGrid = PlacedItemListObject.GetComponentInChildren<UIGrid>();

        for (int i = 0; i < PlacedObjectList.Count; ++i)
        {
            //if (PlacedObjectList[i].ObjectTypeName == MyRoomObject.TYPE_FLOOR ||
            //    PlacedObjectList[i].ObjectTypeName == MyRoomObject.TYPE_WALL) continue;

            AddMyRoomPlacedObjectBox(PlacedObjectList[i]);
        }

        placedHeroListBoxGrid = PlacedHeroListObject.GetComponentInChildren<UIGrid>();

        for (int i = 0; i < PlacedHeroList.Count; ++i)
        {
            AddMyRoomHeroBox(PlacedHeroList[i]);
        }

        
    }

    private void InitializeRoomButton()
    {
        if(RefParameter.CurrentRoomID == RefParameter.UserMainRoomID)
        {
            MainMyRoomButtol.defaultColor = Color.yellow;
            MainMyRoomButtol.hover = Color.yellow;
            MainMyRoomButtol.pressed = Color.yellow;
            MainMyRoomButtol.UpdateColor(true);
        }
        else
        {
            MainMyRoomButtol.defaultColor = Color.white;
            MainMyRoomButtol.hover = Color.white;
            MainMyRoomButtol.pressed = Color.white;
            MainMyRoomButtol.UpdateColor(true);
        }
    }

    public bool IsActiveUI()
    {
        return  (MyRoomList != null && MyRoomList.activeSelf) || 
                (HistoryList != null && HistoryList.activeSelf) || 
                (MyRoomInventory != null && MyRoomInventory.activeSelf) ||
                (VisitFriendList != null && VisitFriendList.activeSelf) ||
               GameCore.Instance.CommonSys.GetMsgComfirmCount() != 0;
    }


    private void AddMyRoomPlacedObjectBox(IPlaceAbleObject target)
    {
        if (target.IsSameClass(typeof(MyRoomObject)) == false) return;
        MyRoomObject myRoomObject = target as MyRoomObject;

        MyRoomPlacedObjectBox box = Instantiate(MyRoomPlacedObjectBoxPrefab, placedItemListBoxGrid.transform);
        box.Initialize(RefParameter, myRoomObject);
        box.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { OnClickPlaceItemList(); }));
        placedObjectBoxList.Add(box);
        placedItemListBoxGrid.Reposition();
    }

    private void DeleteMyRoomPlacedObjectBox(IPlaceAbleObject target)
    {
        if (target.IsSameClass(typeof(MyRoomObject)) == false) return;
        //if (target.ObjectTypeName == MyRoomObject.TYPE_FLOOR ||
        //    target.ObjectTypeName == MyRoomObject.TYPE_WALL) return;

        MyRoomObject myRoomObject = target as MyRoomObject;

        MyRoomPlacedObjectBox targetBox = placedObjectBoxList.Find( box => box.TargetObject == myRoomObject);

        if (targetBox == null)
        {
            Debug.Log(string.Format("MyRoomUIController.DeleteMyRoomPlacedObjectBox ::: {0} is Null", myRoomObject.LocalData.name));
            return;
        }

        placedObjectBoxList.Remove(targetBox);
        targetBox.gameObject.SetActive(false);
        targetBox.transform.SetParent(null);
        Destroy(targetBox.gameObject);
        placedItemListBoxGrid.Reposition();
    }

    private void AddMyRoomHeroBox(IPlaceAbleObject target)
    {
        if (target.IsSameClass(typeof(MyRoomHeroObject)) == false) return;
        MyRoomHeroObject heroObject = target as MyRoomHeroObject;

        if (placedHeroboxList.Find(hero => hero.TargetHero == heroObject) != null)//내려놓음
        {
            foreach (var heroBox in placedHeroboxList) heroBox.Selected.enabled = false;
            return;
        }
        //비동기로 로드하는것이 평소보다 일찍 콜 될경우 간혹 없는 경우가 있음
        if(placedHeroListBoxGrid == null) placedHeroListBoxGrid = PlacedHeroListObject.GetComponentInChildren<UIGrid>();

        MyRoomHeroBox box = Instantiate(MyRoomHeroBoxPrefab, placedHeroListBoxGrid.transform);
        box.Initialize(RefParameter, heroObject, ()=> 
        {
            foreach (var heroBox in placedHeroboxList) heroBox.Selected.enabled = false;
        });

        placedHeroboxList.Add(box);
        placedHeroListBoxGrid.Reposition();
    }

    private void DeleteMyRoomHeroBox(IPlaceAbleObject target)
    {
        if (target.IsSameClass(typeof(MyRoomHeroObject)) == false) return;
        MyRoomHeroObject targetHero = target as MyRoomHeroObject;

        MyRoomHeroBox targetBox = placedHeroboxList.Find(box => box.TargetHero == targetHero);

        if (targetBox == null)
        {
            Debug.Log(string.Format("MyRoomUIController.DeleteMyRoomPlacedObjectBox ::: {0} is Null", targetHero.HeroData.LocalData.name));
            return;
        }

        placedHeroboxList.Remove(targetBox);
        Destroy(targetBox.gameObject);

        //placedItemListBoxGrid.Reposition();
        placedHeroListBoxGrid.enabled = true;
    }

    protected virtual void Update()
    {
        //if (Input.GetMouseButtonUp(0))
        //{
        //    RaycastHit2D hit;
        //    if (UnityCommonFunc.GetCameraHitInfo2D(GameCore.Instance.GetUICam(), out hit, "UI") == false)
        //    {
        //        CloseList();
        //    }
        //}


        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            RaycastHit2D hit;

            Ray ray = GameCore.Instance.GetUICam().ScreenPointToRay(Input.GetTouch(0).position);
            hit = Physics2D.Raycast(ray.origin, ray.direction, 99999, LayerMask.GetMask("UI"));

            if (hit.collider == null) CloseList();
        }

        if (EditTool.IsActive) EditTool.Update();
        UnityCommonFunc.GetGameObjectByName(MainPanel, "ChangeRoomButton").transform.GetChild(2).gameObject.SetActive(false);
        for (int i = 0; i < ChangeMyRoomButtonList.Length/*GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList.Count*/; ++i)
        {
            if (CheckRewardNotification(i))
            {
                
                ChangeMyRoomButtonList[i].Notification.SetActive(true);
                UnityCommonFunc.GetGameObjectByName(MainPanel, "ChangeRoomButton").transform.GetChild(2).gameObject.SetActive(true);
            }
            else ChangeMyRoomButtonList[i].Notification.SetActive(false);
        }
    }

    protected virtual void OnDestroy()
    {
        //이거 어떤순간에 null 레퍼런스 뜸 해결요망
        if(EditTool.PositiveButton.WorldTarget != null) Destroy(EditTool.PositiveButton.WorldTarget.gameObject);
        if (EditTool.FlippingButton.WorldTarget != null) Destroy(EditTool.FlippingButton.WorldTarget.gameObject);
        if (EditTool.RecallButton.WorldTarget != null) Destroy(EditTool.RecallButton.WorldTarget.gameObject);
        //if (EditTool.NegativeButton.WorldTarget != null) Destroy(EditTool.NegativeButton.WorldTarget.gameObject);
    }

    public void ActiveRoomButton(int index)
    {
        if (index < 0 || ChangeMyRoomButtonList.Length <= index) return;

        if (CheckRewardNotification(index)) ChangeMyRoomButtonList[index].Notification.SetActive(true);
        else ChangeMyRoomButtonList[index].Notification.SetActive(false);

        ChangeMyRoomButtonList[index].SetActive(true);
    }

    public void OnClickChangeRoomButton(int index)
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        Command.CmdChangeMyRoom(index);
    }

    private void OnStartMyRoomObjectEditMode(IPlaceAbleObject target)
    {
        GameCore.Instance.CommonSys.tbUi.gameObject.SetActive(false);
        MainMyRoomButtol.gameObject.SetActive(false);
        EditPanel.SetActive(false);
    }

    private void OnEndMyRoomObjectEditMode(IPlaceAbleObject target)
    {
        GameCore.Instance.CommonSys.tbUi.gameObject.SetActive(true);
        MainMyRoomButtol.gameObject.SetActive(true);
        EditPanel.SetActive(true);
    }

    private void TurnOffEditTools(IPlaceAbleObject target = null)
    {
        EditTool.PositiveButton.WorldTarget.SetParent(transform);
        EditTool.FlippingButton.WorldTarget.SetParent(transform);
        EditTool.RecallButton.WorldTarget.SetParent(transform);
        //EditTool.NegativeButton.WorldTarget.SetParent(transform);
        EditTool.SetActive(false, true);
    }

    private void TurnOnEditTool(IPlaceAbleObject target)
    {
        if (target == null)
            return;

        BoxCollider t = target.Collider as BoxCollider;

        Vector3 rd = t.center;
        rd.x += (t.size.x / 2);
        rd.y -= (t.size.y / 2);
        rd.z -= (t.size.z / 2);

        EditTool.PositiveButton.WorldTarget.position = t.transform.TransformPoint(rd);
        EditTool.PositiveButton.WorldTarget.SetParent(t.transform);

        Vector3 lt = t.center;
        lt.x -= (t.size.x / 2);
        lt.y += (t.size.y / 2);
        lt.z -= (t.size.z / 2);

        EditTool.FlippingButton.WorldTarget.position = t.transform.TransformPoint(lt);
        EditTool.FlippingButton.WorldTarget.SetParent(t.transform);

        Vector3 rt = t.center;
        rt.x += (t.size.x / 2);
        rt.y += (t.size.y / 2);
        rt.z -= (t.size.z / 2);

        EditTool.RecallButton.WorldTarget.position = t.transform.TransformPoint(rt);
        EditTool.RecallButton.WorldTarget.SetParent(t.transform);

        if (target.LastPlacedTile == null)
        {
            lt.y = t.center.y;
            EditTool.FlippingButton.WorldTarget.position = t.transform.TransformPoint(lt);
            EditTool.SetActive(true, false);
        }
        else
        {
            Vector3 ld = t.center;
            ld.x -= (t.size.x / 2);
            ld.y -= (t.size.y / 2);
            ld.z -= (t.size.z / 2);

            //EditTool.NegativeButton.WorldTarget.position = t.transform.TransformPoint(ld);
            //EditTool.NegativeButton.WorldTarget.SetParent(t.transform);

            EditTool.SetActive(true,true);
        }
    }

    private void UpdateHistoryList()
    {
        Transform historyListGridTransform = HistoryList.GetComponentInChildren<UIGrid>().transform;

        for (int i = 0; i < HistoryDataList.Count; ++i)
        {
            MyRoomHistoryLog log = Instantiate(MyRoomHistoryLogPrefab, historyListGridTransform);
            log.Initialize(RefParameter, HistoryDataList[i]);
            logObjectList.Add(log);
        }
    }

    private void UpateHeroList()
    {

    }

    public void UpdateInventory()
    {
        UIInventory.UpdateCard();
    }

    public void UpdateAllInventory()
    {
        UIInventory.UpdateAllCard();
    }

    internal void UpdateInventory(InvenBase.TypeFlag _flag)
    {
        UIInventory.UpdateCard(_flag);
    }

    public void OnClickEditButton()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        CloseList();
        bool boolValue = EditPanel.activeInHierarchy;
        if (boolValue)
        {
            Command.CmdEndEditMode.Invoke();
            Command.CmdStainListActiveControl(true);
        }
        else
        {
            Command.CmdStartEditMode.Invoke();
            Command.CmdStainListActiveControl(false);
        }
        SatisfactionInfoEdit.TurnOffSatisfactionPanelButton();
        SatisfactionInfoEdit.gameObject.SetActive(!boolValue);
        SatisfactionInfo.gameObject.SetActive(boolValue);
        GameCore.Instance.CommonSys.tbUi.SetActiveAnchor(boolValue, 1);

        MainPanel.SetActive(!MainPanel.activeInHierarchy);
        EditPanel.SetActive(!EditPanel.activeInHierarchy);
    }

    private void CloseList()
    {
        if (MyRoomList != null && MyRoomList.activeInHierarchy) OnClickMyRoomListButton();
        if (HistoryList != null && HistoryList.activeInHierarchy) OnClickHistoryListButton();
        if (PlacedItemListObject != null && PlacedItemListObject.activeInHierarchy) OnClickPlaceItemList();
    }

    public void OnClickMyRoomListButton()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        bool isActive = MyRoomList.activeInHierarchy;
        MyRoomList.SetActive(!isActive);
    }

    public void OnClickHistoryListButton()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        HistoryList.SetActive(!HistoryList.activeInHierarchy);
    }

    public void OnClickPlaceItemList()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        bool isActive = PlacedItemListObject.activeInHierarchy;
        PlacedItemListObject.SetActive(!isActive);
        Command.CmdChangeControlMode.Invoke(isActive ? MyRoomSys.MyRoomControlMode.SelectMyRoomObject : MyRoomSys.MyRoomControlMode.ControlUI);
    }

    public void OnClickMyRoomInventory()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        CloseList();
        UIInventory.UpdateCard(InvenBase.TypeFlag.Character);
        UIInventory.UpdateCount();
        UIInventory.OnClickSortByRankDescending();
        UIInventory.SelectClear();
        MyRoomInventory.SetActive(true);
        Command.CmdChangeControlMode(MyRoomSys.MyRoomControlMode.ControlUI);

        RefParameter.NegativeButtonStatck.Push(() =>
        {
            MyRoomInventory.SetActive(false);
            Command.CmdStartEditMode();
        });

        AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.MyRoom, 2, 1);
    }

    private void OnClickPlaceButtonAtInventory()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        ItemList temp = UIInventory.GetSelectedInvenItem();
        long[] selectList = temp.GetSelects();

        if (selectList.Length <= 0) return;

        Action<long> targetMethod = (temp.Type == InvenBase.TypeFlag.Character) ?
                                    Command.CmdSpawnHeroByInventory : Command.CmdSpawnObjectByInventory;

        for (int i = 0; i < selectList.Length; ++i) targetMethod.Invoke(selectList[i]);

        RefParameter.NegativeButtonStatck.Pop();
        MyRoomInventory.SetActive(false);
    }

    public void OnClickVisitFriendList()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        if (VisitFriendList.activeInHierarchy == false)
        {
            CloseList();
            Transform visitFriendListGridTransform = VisitFriendList.GetComponentInChildren<UIGrid>().transform;

            if (visitFriendListGridTransform.childCount == 0)
            {
                if (FriendList == null) FriendList = (GameCore.Instance.SubsysMgr.GetNowSubSys() as MyRoomSys).UserFriendDataList;

                if (FriendList.Count == 0)
                {
                    var defaultLabel = VisitFriendList.transform.Find("DefaultLabel");
                    defaultLabel.gameObject.SetActive(true);
                }
                else
                {
                    for (int i = 0; i < FriendList.Count; ++i)
                    {
                        if (GameCore.Instance.PlayerDataMgr.GetFriendOrNull(FriendList[i].FriendUID) == null) continue;
                        MyRoomFrindInfo info = Instantiate(MyRoomFriendPrefab, visitFriendListGridTransform);
                        info.Initialize(FriendList[i], Command.CmdVisitFriendRoom);
                    }
                }
            }
            else
            {
                for (int i = 0; i < FriendList.Count; ++i)
                {
                    if (GameCore.Instance.PlayerDataMgr.GetFriendOrNull(FriendList[i].FriendUID) == null) continue;
                    var info = visitFriendListGridTransform.GetChild(i).GetComponent<MyRoomFrindInfo>();
                    info.Initialize(FriendList[i], Command.CmdVisitFriendRoom);
                }
            }

            RefParameter.NegativeButtonStatck.Push(() =>
            {
                OnClickVisitFriendList();
            });
        }
        else
        {
            if(RefParameter.NegativeButtonStatck.Count > 1) RefParameter.NegativeButtonStatck.Pop();
        }

        Command.CmdStainListActiveControl(VisitFriendList.activeInHierarchy);
        VisitFriendList.SetActive(!VisitFriendList.activeInHierarchy);
    }

    public void OnClickMainMyRoomButton()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        Command.CmdSelectMainRoom();
    }

    public void OnChangedMainMyRoom()
    {
        Color applyColor;

        if (RefParameter.CurrentRoomID == RefParameter.UserMainRoomID) applyColor = Color.yellow;
        else applyColor = Color.white;

        MainMyRoomButtol.defaultColor = applyColor;
        MainMyRoomButtol.hover = applyColor;
        MainMyRoomButtol.pressed = applyColor;
        MainMyRoomButtol.UpdateColor(true);
    }

    public void OnClickEditToolPositive()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        Debug.Log("OnClickEditToolPositive");
        
        if(Command.CmdMyRoomObjectEditEnd.Invoke(true)) TurnOffEditTools();
    }

    public void OnClickEditToolNegative()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        Debug.Log("OnClickEditToolNegative");
        //GameCore.Instance.ShowAlert("Negative 버튼 이슈로인해 잠금");
        Command.CmdSeletedObjectRetrunToOriginPosition.Invoke();
    }

    public void OnClickEditToolFlipping()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        Debug.Log("OnClickEditToolFlipping");
        Command.CmdSelectedObjectFlip.Invoke();
    }

    public void OnClickEditToolRecall()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        Debug.Log("OnClickEditToolRecall");
        TurnOffEditTools();
        Command.CmdSelectedObjectRetrunToInventory.Invoke();
    }

    public void OnClickClearRoomButton()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        if (Command.CmdClearRoom != null) Command.CmdClearRoom.Invoke();
        OnClickEditButton();
    }


    private bool CheckRewardNotification(int index)
    {
        var list = GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList;
        for (int i = 0; i < list.Count; ++i)
            if (list[i].ID == index + 1)
            {
                for (int j = 0; j < list[i].StainDataList.Count; ++j)
                {
                    if (list[i].StainDataList[j].RewardItemCount > 0) return true;
                }
            }

        return false;
    }



    public void OnClickCleaningAll()
    {
        var sys = (GameCore.Instance.SubsysMgr.GetNowSubSys() as MyRoomSys);
        if (sys == null)
        {
            GameCore.Instance.ShowNotice("청소", "알 수 없는 에러", 0);
            return;
        }

        if (sys.GetCleanableDirtyDustCount() == 0)
        {
            GameCore.Instance.ShowNotice("청소", "청소할 먼지가 없습니다.", 0);
            return;
        }

        if (!RefParameter.IsVisit && GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Mailage) == 0)
        {
            GameCore.Instance.ShowNotice("청소", "마일리지가 부족합니다.", 0);
            return;
        }

        sys.CleanAllDirtyDust();
    }



    public void OnClickCleaningRewardAll()
    {
        var sys = (GameCore.Instance.SubsysMgr.GetNowSubSys() as MyRoomSys);
        if (sys == null)
        {
            GameCore.Instance.ShowNotice("청소", "알 수 없는 에러", 0);
            return;
        }

        if (sys.GetCleanedDirtyDustCount() == 0)
        {
            GameCore.Instance.ShowNotice("청소", "완료된 청소 없습니다.", 0);
            return;
        }

        GameCore.Instance.NetMgr.Req_MyRoomTakePresentAll(RefParameter.CurrentRoomID);
    }


}
