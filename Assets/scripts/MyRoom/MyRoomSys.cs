using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System;

namespace IDH.MyRoom
{
    internal class MyRoomPara : ParaBase { }

    public class MyRoomInventoryData
    {
        public Dictionary<long, HeroSData> HeroSdataDic;
        public Dictionary<long, ItemSData> ItemSdataDic;
    }

    public class MyRoomSysParameter : ParaBase
    {
        public bool IsVisit = false;
        public MyRoomInfo TargetUserMyRoomInfo;
        public MyRoomData TargetMyRoom;
        public long TargetUserID;

        public bool showFriendList = false;
    }

    public class MyRoomSystemRefParameter
    {
        /// <summary>
        /// 현재유저의 친구 정보(PlayerDataMgr.GetFriendList()의 리턴값)
        /// </summary>
        public List<MyRoomFriendData> FriendList { get; set; }
        /// <summary>
        /// 히스토리 로그 리스트
        /// </summary>
        public List<MyRoomHistoryLogData> HistoryLogDataList { get; set; }
        /// <summary>
        /// 영웅 및 아이템 인벤토리
        /// </summary>
        public MyRoomInventoryData InventoryData { get; set; }

        public Stack<Action> NegativeButtonStatck { get; set; }
        public MyRoomSystemCommand Command { get; set; }
        public MyRoomSystemObserver Observer { get; set; }

        /// <summary>
        /// 현재 마이룸에 생성된 모든 오브젝트의 리스트
        /// </summary>
        public List<MyRoomObject> PlacedObjectList { get; set; }
        public List<MyRoomHeroObject> PlacedHeroList { get; set; }

        public MyRoomTileMap FloorTileMap { get; set; }
        public MyRoomTileMap WallTileMap { get; set; }

        public int CurrentRoomID;
        public int UserMainRoomID;
        //public int UserMyRoomCount;
        public bool[] OpendUserMyRoom;
        public bool IsVisit { get; set; }

        public int SatisfactionValue
        {
            get
            {
                int value = 0;
                for (int i = 0; i < PlacedObjectList.Count; ++i)
                {
                    value += (PlacedObjectList[i].LocalData as ItemDataMap).optionValue[0];
                }

                return value;
            }
        }
    }

    internal partial class MyRoomSys : SubSysBase
    {
        public enum MyRoomControlMode
        {
            SelectMyRoomObject,
            ControlMyRoomObject,
            ControlMyRoomDefault,
            ControlUI
        }

        /// <summary>
        /// ObjectName, GameObject
        /// </summary>
        private Dictionary<string, GameObject> RoomBaseObjectDic = new Dictionary<string, GameObject>();

        private Vector3 cameraOriginPos;
        private Vector3 cameraOriginAngles;

        private Camera              worldCam;
        private CameraRayController rayController;

        private SpriteAtlas resourceAtlas;
        private StainObject stainPrefab;

        /// <summary>
        /// 캐시
        /// </summary>
        /// 
        public MyRoomInventoryData  ServerInventory { get; private set; }
        private MyRoomInfo          UserMyRoomInfo { get; set; }
        /// <summary>
        /// 임시로 퍼블릭해놓음
        /// </summary>
        public List<MyRoomFriendData>       UserFriendDataList { get; set; }
        private List<MyRoomHistoryLogData>  HistoryLogDataList = new List<MyRoomHistoryLogData>();

        private MyRoomSysParameter          InnerParameter { get; set; }
        private MyRoomSystemRefParameter    RefParameter { get; set; }
        private MyRoomSystemCommand        Command { get; set; }
        private MyRoomSystemObserver       Observer { get; set; }

        private MyRoomTileMap WallTileMap;
        private MyRoomTileMap FloorTileMap;
        private MyRoomTileMap RugTileMap;

        private List<StainObject>       StainObjectList = new List<StainObject>();
        private List<MyRoomObject>      PlacedObjectList = new List<MyRoomObject>();
        private List<MyRoomHeroObject>  PlacedHeroList = new List<MyRoomHeroObject>();

        private IPlaceAbleObject    SelectedObject { get; set; }
        private MyRoomData          MyRoomCursor { get; set; }
        private MyRoomTileMap       MapCursor { get; set; }
        private TransformTile       TileCursor { get; set; }

        private MyRoomControlMode CurrentControlMode { get; set; }
        private MyRoomControlMode PrevControlMode { get; set; }

        private Stack<Action> NegativeButtonStatck = new Stack<Action>();

        private CharacterDialogueScript characterDialogueScript;

        Queue<MyRoomStainData> cleanAllQ = new Queue<MyRoomStainData>(); // 청소를 일괄로 하기위해 청소할 먼지들을 임시로 보관하는 큐리스트
        List<CardSData> rewardsByCleaning = new List<CardSData>(); // 친구 먼지 청소보상을 일괄로 받기위해 보상을 임시 저장하는 리스트



        public MyRoomSys() : base(SubSysType.MyRoom)
        {
            needInitDataTypes = new InitDataType[] {
                InitDataType.MyRoom,
                InitDataType.Character,
                InitDataType.Item,
            };

            preloadingBundleKeys = new int[]
            {
                CommonType.DEF_KEY_BGM_MYROOM,
                CommonType.DEF_KEY_SFX_UI
            };
        }


        MyRoomUIController MyRoomUI;



        public override int PreLoading()
        {
            var cnt = base.PreLoading();

            GameCore.Instance.NetMgr.Req_MyRoom_GetInfo();
            GameCore.Instance.NetMgr.Req_Friend_List();
            GameCore.Instance.NetMgr.Req_MyRoomFriendList();
            GameCore.Instance.NetMgr.Req_MyRoomHistory();

            return cnt + 4;
        }


        protected override void EnterSysInner(ParaBase _para)
        {
            base.EnterSysInner(_para);

            System.GC.Collect();

            BattleSysBase.SetCamViewportBySreen();

            //UserData 가져와 캐싱
            ServerInventory = new MyRoomInventoryData();
            ServerInventory.HeroSdataDic = GameCore.Instance.PlayerDataMgr.HeroSdataDic;
            ServerInventory.ItemSdataDic = GameCore.Instance.PlayerDataMgr.ItemSdataDic;
            UserFriendDataList = new List<MyRoomFriendData>();

            UserMyRoomInfo = GameCore.Instance.PlayerDataMgr.UserMyRoom;
            //GameCore.Instance.NetMgr.Req_MyRoomFriendList();
            //GameCore.Instance.NetMgr.Req_MyRoomHistory();
            GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_MyRoom, true, false);
            //GameCore.Instance.SndMgr.PlayBGM(BGM.MyRoomScene);

            //들어온 데이터 해석
            InnerParameter = _para as MyRoomSysParameter;

            if (InnerParameter == null)
            {
                InnerParameter = new MyRoomSysParameter();
                InnerParameter.IsVisit = false;
                InnerParameter.TargetMyRoom = UserMyRoomInfo.MainRoom;
                InnerParameter.TargetUserID = GameCore.Instance.PlayerDataMgr.LocalUserData.UserID;
            }
            else if (InnerParameter.IsVisit)
            {
                InnerParameter.showFriendList = false;
            }
            else if (InnerParameter.showFriendList)
            {
                InnerParameter.IsVisit = false;
                InnerParameter.TargetMyRoom = UserMyRoomInfo.MainRoom;
                InnerParameter.TargetUserID = GameCore.Instance.PlayerDataMgr.LocalUserData.UserID;
            }

            NegativeButtonStatck.Clear();

            if (InnerParameter.IsVisit)
            {
                NegativeButtonStatck.Push(() =>
                {
                    InnerParameter.IsVisit = false;
                    InnerParameter.TargetUserMyRoomInfo = UserMyRoomInfo;
                    InnerParameter.TargetMyRoom = UserMyRoomInfo.MainRoom;

                    GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, InnerParameter);
                });
            }
            else
            {
                NegativeButtonStatck.Push(() => { GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null); });
            }

            GameCore.Instance.CommonSys.tbUi.Observer.OnClickFriendUIIcon += OnClickFriendIcon;
            GameCore.Instance.CommonSys.tbUi.Observer.OnClickMailIcon += OnClickMailIcon;

            MyRoomCursor = InnerParameter.TargetMyRoom;

            //MyRoomSystem 에서 쓰이는 클래스 및 데이터 생성 및 캐싱
            Observer = new MyRoomSystemObserver();

            Command = new MyRoomSystemCommand();
            Command.CmdChangeMyRoom = ChangeMyRoom;
            Command.CmdMoveToRevengeMatch = MoveToRevengeMatch;
            Command.CmdSpawnObjectByInventory = RequestSpawnObjectByInventory;
            Command.CmdSpawnHeroByInventory = RequestSpawnHeroByInventory;
            Command.CmdVisitFriendRoom = VisitFriendMyRoom;
            Command.CmdStartEditMode = StartEditMode;
            Command.CmdEndEditMode += EndEditMode;
            Command.CmdEndEditMode += ApplyEdit;
            Command.CmdMyRoomObjectEditEnd = EndObjectEditMode;
            Command.CmdSelectObject = SelectObject;
            Command.CmdSelectedObjectFlip = FlipObject;
            Command.CmdSelectedObjectRetrunToInventory = ReturnSelectedObjectToInventory;
            Command.CmdMyRoomObjectRetrunToInventory = ReturnMyRoomObjectToInventory;
            Command.CmdHeroReturnToInventory = Send_ReturnToInventoryHero;
            Command.CmdSeletedObjectRetrunToOriginPosition = ReturnToOriginPosition;
            Command.CmdClearRoom = ClearRoom;
            Command.CmdCleanDirtyDust = CleanDirtyDust;
            Command.CmdReceiveReward = ReceiveReward;
            Command.CmdChangeControlMode = ChangeControlMode;
            Command.CmdStainListActiveControl = StainRenderControl;
            Command.CmdSelectMainRoom = SelectMainRoom;

            RefParameter = new MyRoomSystemRefParameter();
            RefParameter.FriendList = UserFriendDataList;
            RefParameter.HistoryLogDataList = HistoryLogDataList;
            RefParameter.InventoryData = ServerInventory;
            RefParameter.PlacedObjectList = PlacedObjectList;
            RefParameter.PlacedHeroList = PlacedHeroList;
            RefParameter.NegativeButtonStatck = NegativeButtonStatck;
            RefParameter.Command = Command;
            RefParameter.Observer = Observer;
            //RefParameter.UserMyRoomCount = UserMyRoomInfo.MyRoomDataList.Count;
            RefParameter.OpendUserMyRoom = new bool[10];
            for (int i = 0; i < UserMyRoomInfo.MyRoomDataList.Count; ++i)
                RefParameter.OpendUserMyRoom[UserMyRoomInfo.MyRoomDataList[i].ID-1] = true;

            RefParameter.UserMainRoomID = UserMyRoomInfo.MainRoomID;
            RefParameter.CurrentRoomID = MyRoomCursor.ID;

            if (TransformTile.TileColorGreen == null)
            {
                TransformTile.TileColorRed = Resources.Load<Material>("MyRoom/MyRoomTileMaterialRed");
                TransformTile.TileColorGreen = Resources.Load<Material>("MyRoom/MyRoomTileMaterialGreen");
                TransformTile.TileColorNormal = Resources.Load<Material>("MyRoom/MyRoomTileMaterialNormal");
            }

            stainPrefab = Resources.Load<StainObject>("MyRoom/Stain");
            GameCore.Instance.ResourceMgr.GetObject<SpriteAtlas>(ABType.AB_UnityAtlas, 1101, (obj)=> {
                resourceAtlas = obj;
            }); // Resources.Load<SpriteAtlas>("MyRoom/MyRoom");
            

            worldCam = GameCore.Instance.GetWorldCam();
            cameraOriginPos = worldCam.transform.position;
            cameraOriginAngles = worldCam.transform.eulerAngles;
            worldCam.transform.position = new Vector3(0.0f, 14.72f, -15.3f);
            worldCam.transform.eulerAngles = new Vector3(35.0f, 0.0f, 0.0f);

            rayController = worldCam.GetComponent<CameraRayController>();
            if (rayController == null)
            {
                rayController = worldCam.gameObject.AddComponent<CameraRayController>();
                rayController.Initialize();
                rayController.MouseIntervalX = 40;
                rayController.MouseIntervalY = 10;
            }
            rayController.enabled = true;

            RefParameter.IsVisit = InnerParameter.IsVisit;
            if (InnerParameter.IsVisit)
            {
                CreateVisitMyRoomBaseObject();

                var name = "";
                foreach (var data in GameCore.Instance.PlayerDataMgr.GetFriendList())
                    if (InnerParameter.TargetUserID == data.USER_UID)
                    {
                        name = data.USER_NAME;
                        break;
                    }
                GameCore.Instance.CommonSys.SetTitle(string.Format("{1}의 숙소 {0}", MyRoomCursor.ID, name));
            }
            else
            {
                CreateUserMyRoomBaseObject();
                GameCore.Instance.CommonSys.SetTitle(string.Format("숙소 {0}", MyRoomCursor.ID));
            }
            

            //실제 마이룸 오브젝트 배치
            BuildMyRoom(MyRoomCursor);

            if (Observer.OnInitializedMyRoomSystem != null) Observer.OnInitializedMyRoomSystem.Invoke();

            ChangeControlMode(MyRoomControlMode.ControlMyRoomDefault);
            //ChangeControlMode(MyRoomControlMode.ControlUI);

            characterDialogueScript = CharacterDialogueScript.Create(GameCore.Instance.ui_root);

            cbEnterSys();
            cbEnterSys = null;

            if (InnerParameter.showFriendList && !InnerParameter.IsVisit)
                MyRoomUI.OnClickVisitFriendList();



            AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.MyRoom, 0, 1);
        }

        protected override void ExitSysInner(ParaBase _para)
        {
            base.ExitSysInner(_para);

            foreach (var myRoomObj in PlacedObjectList) myRoomObj.Destory();
            foreach (var hero in PlacedHeroList) hero.Destory();
            foreach (var stainObj in StainObjectList)
            {
                stainObj.StopAllCoroutines();
                GameObject.Destroy(stainObj.gameObject);
            }

            DestroyRoomBaseObject();

            worldCam.transform.position = cameraOriginPos;
            worldCam.transform.eulerAngles = cameraOriginAngles;
            rayController.enabled = false;
            rayController.OnMouseMovedInterval = null;
            rayController.OnRayCastHit = null;
            rayController.OnRayCastHits = null;

            Resources.UnloadUnusedAssets();

            PlacedObjectList.Clear();
            StainObjectList.Clear();
            HistoryLogDataList.Clear();
            PlacedHeroList.Clear();
            NegativeButtonStatck.Clear();

            GameCore.Instance.CommonSys.tbUi.Observer.OnClickFriendUIIcon -= OnClickFriendIcon;
            GameCore.Instance.CommonSys.tbUi.Observer.OnClickMailIcon -= OnClickMailIcon;

            characterDialogueScript.FreeDialogue();

            //아래떄문에 pvp 되돌아가기 널뜸
            //Parameter = null;

            System.GC.Collect();
        }

        protected override void RegisterHandler()
        {
            handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
            handlerMap.Add(GameEventType.ANS_MYROOM_Buy, ANS_MYROOM_Buy);
            handlerMap.Add(GameEventType.ANS_MYROOM_BUILD, ANS_MYROOM_BUILD);
            //handlerMap.Add(GameEventType.ANS_MYROOM_FriendList, ANS_RequestFreindList);
            //handlerMap.Add(GameEventType.ANS_MYROOM_HISTORY, ANS_MyroomHistoryList);
            handlerMap.Add(GameEventType.ANS_MYROOM_FriendRoomDataList, ANS_RequestFreindRoomDataList);
            handlerMap.Add(GameEventType.ANS_CHARACTER_ALLOCATE, ANS_CHARACTER_ALLOCATE);
            handlerMap.Add(GameEventType.ANS_CHARACTER_UNALLOCATE, ANS_CHARACTER_UNALLOCATE);
            handlerMap.Add(GameEventType.ANS_MYROOM_START_CLEAN, ANS_MYROOM_START_CLEAN);
            handlerMap.Add(GameEventType.ANS_MYROOM_END_CLEAN, ANS_MYROOM_END_CLEAN);
            handlerMap.Add(GameEventType.ANS_MYROOM_END_CLEAN_ALL, ANS_MYROOM_END_CLEAN_ALL);
            handlerMap.Add(GameEventType.ANS_MYROOM_ReturnToInventoryObject, ANS_MYROOM_ReturnToInventoryObject);
            handlerMap.Add(GameEventType.ANS_ACCOUNT_SETMAINMYROOM, ANS_SelectMainRoom);

            base.RegisterHandler();
        }

        internal override void ClickBackButton()
        {
            if (NegativeButtonStatck.Count != 0)
                NegativeButtonStatck.Pop().Invoke();
            else
                GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
        }

        #region Room InitializeMethod

        private void BuildMyRoom(MyRoomData roomData)
        {
            foreach (var item in roomData.PlacedObjectList)
            {
                if (item == null || item.LocalData == null) continue;
                var spawnedObject = SpawnObject(item);
                SelectMapCursor(item.LocalData.typeName).ApplyPlacedObject(spawnedObject.PlacedTile, spawnedObject);
            }

            foreach (var hero in roomData.PlacedHeroList)
            {
                SpawnHero(hero, true);
            }

            var stainPositionList = CreateStainPosition(roomData.StainDataList.Count);

            for (int i = 0; i < roomData.StainDataList.Count; ++i)
            {
                StainObject stain = GameObject.Instantiate(stainPrefab, GameCore.Instance.ui_root);
                stain.transform.position = GameCore.Instance.WorldPositionToUiPosition(stainPositionList[i]);
                stain.Initialize(roomData.StainDataList[i], RefParameter);
                stain.SetLayerDepth(i * 100 + 100);
                if (InnerParameter.IsVisit && stain.CurrentState == StainObject.State.Present) stain.ChangeRemovedState();
                StainObjectList.Add(stain);
            }

            //오브젝트 배치후
            MyRoomCursor = roomData;
            SelectedObject = null;
            TileCursor = null;
            WallTileMap.ControlTileRender(false);
            FloorTileMap.ControlTileRender(false);
            RugTileMap.ControlTileRender(false);
        }

        private void CreateUserMyRoomBaseObject()
        {
            CreateMyRoomBaseObject();

            GameObject uiObject = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/NewMyRoomUiPanel"), GameCore.Instance.ui_root);
            MyRoomUI = uiObject.GetComponentInChildren<MyRoomUIController>();
            MyRoomUI.Initialize(RefParameter);

            RoomBaseObjectDic.Add("UiObject", uiObject);

            // 편집, 숙소이동, 방문하기, 기록보기 등등의 UI 오브젝트들
        }

        private void CreateVisitMyRoomBaseObject()
        {
            CreateMyRoomBaseObject();

            GameObject uiObject = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/NewMyRoomUiPanel_visit"), GameCore.Instance.ui_root);
            MyRoomUI = uiObject.GetComponentInChildren<MyRoomUIController>();
            MyRoomUI.Initialize(RefParameter);

            RoomBaseObjectDic.Add("UiObject", uiObject);
        }

        private void CreateMyRoomBaseObject()
        {
            GameObject wall = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/MyRoom_Wall"));
            GameObject wallTileMapRoot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/TileMap_Wall"));
            GameObject wallDecoPivot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/WallDecoPivot"));
            WallTileMap = new MyRoomTileMap(MyRoomTileMap.TileMapType.Wall,
                                            wallTileMapRoot,
                                            wall.GetComponent<SpriteRenderer>(),
                                            wallDecoPivot.transform);

            GameObject floor = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/MyRoom_Floor"));
            GameObject floorTileMapRoot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/TileMap_Floor"));
            GameObject floorDecoPivot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/FloorDecoPivot"));
            FloorTileMap = new MyRoomTileMap(MyRoomTileMap.TileMapType.Floor,
                                            floorTileMapRoot,
                                            floor.GetComponent<SpriteRenderer>(),
                                            floorDecoPivot.transform);

            MyRoomHeroData.FloorTileMap = FloorTileMap;

            GameObject rugFloor = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/MyRoom_Floor"));
            GameObject rugFloorTileMapRoot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/TileMap_Floor"));
            GameObject rugFloorDecoPivot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/FloorDecoPivot"));
            RugTileMap = new MyRoomTileMap(MyRoomTileMap.TileMapType.Floor,
                                            rugFloorTileMapRoot,
                                            rugFloor.GetComponent<SpriteRenderer>(),
                                            rugFloorDecoPivot.transform);
            rugFloor.name = "rugFloor";
            rugFloor.SetActive(false);
            rugFloorTileMapRoot.name = "rugFloorTileMapRoot";
            rugFloorDecoPivot.name = "rugFloorDecoPivot";

            //공통 UI
            //청소 관련 아이콘들

            RoomBaseObjectDic.Add("wall", wall);
            RoomBaseObjectDic.Add("wallTileMapRoot", wallTileMapRoot);
            RoomBaseObjectDic.Add("wallDecoPivot", wallDecoPivot);
            RoomBaseObjectDic.Add("floor", floor);
            RoomBaseObjectDic.Add("floorTileMapRoot", floorTileMapRoot);
            RoomBaseObjectDic.Add("floorDecoPivot", floorDecoPivot);
            RoomBaseObjectDic.Add("rugFloor", rugFloor);
            RoomBaseObjectDic.Add("rugFloorTileMapRoot", rugFloorTileMapRoot);
            RoomBaseObjectDic.Add("rugFloorDecoPivot", rugFloorDecoPivot);
        }

        private void DestroyRoomBaseObject()
        {
            foreach (var targetObject in RoomBaseObjectDic) GameObject.Destroy(targetObject.Value);

            RoomBaseObjectDic.Clear();
        }

        private List<Vector3> CreateStainPosition(int maxCount)
        {
            List<Vector3> reValList = new List<Vector3>();
            if (maxCount <= 0) return reValList;

            int wallTileCount = UnityEngine.Random.Range(0, maxCount);
            int floorTileCount = maxCount - wallTileCount;

            List<TransformTile> selectTileList = new List<TransformTile>();

            while (wallTileCount > 0)
            {
                var tile = WallTileMap.TileMap[UnityEngine.Random.Range(3, WallTileMap.LengthX - 4), UnityEngine.Random.Range(3, WallTileMap.LengthY - 4)];
                if (selectTileList.Contains(tile)) continue;
                selectTileList.Add(tile);
                wallTileCount -= 1;
            }

            while (floorTileCount > 0)
            {
                var tile = FloorTileMap.TileMap[UnityEngine.Random.Range(3, FloorTileMap.LengthX - 4), UnityEngine.Random.Range(3, FloorTileMap.LengthY - 4)];
                if (selectTileList.Contains(tile)) continue;
                selectTileList.Add(tile);
                floorTileCount -= 1;
            }

            foreach (var tile in selectTileList)
            {
                reValList.Add(tile.Trans.position);
            }

            return reValList;
        }

        #endregion

        private MyRoomObject SpawnObject(MyRoomObjectData myRoomObjData)
        {
            GameObject temp = null;

            if (myRoomObjData.LocalData.typeName == MyRoomObject.TYPE_WALL)
            {
                MapCursor = WallTileMap;
                TileCursor = WallTileMap.TileMap[0, 0];
                temp = RoomBaseObjectDic["wall"];
                MyRoomObject wall = PlacedObjectList.Find(x => x.AttachedObject == temp);
                if (wall != null) ReturnMyRoomObjectToInventory(wall, false);
            }
            else if (myRoomObjData.LocalData.typeName == MyRoomObject.TYPE_FLOOR)
            {
                MapCursor = FloorTileMap;
                TileCursor = FloorTileMap.TileMap[0, 0];
                temp = RoomBaseObjectDic["floor"];
                MyRoomObject floor = PlacedObjectList.Find(x => x.AttachedObject == temp);
                if (floor != null) ReturnMyRoomObjectToInventory(floor, false);
            }
            else
            {
                SelectMapCursor(myRoomObjData.LocalData.typeName);
                TileCursor = MapCursor.GetTile((int)myRoomObjData.vectorList[0].x, (int)myRoomObjData.vectorList[0].y);
                temp = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/DummyProp"));
            }

            MyRoomObject spawnedObject = null;
            if (InnerParameter.IsVisit) spawnedObject = new MyRoomObject(temp, myRoomObjData.LocalData, myRoomObjData);
            else spawnedObject = new MyRoomObject(temp, myRoomObjData.SeverData, myRoomObjData);
            Sprite image = resourceAtlas.GetSprite(myRoomObjData.LocalData.fileName);
            spawnedObject.BuildGameObject(image);

            if (myRoomObjData.LocalData.typeName == MyRoomObject.TYPE_FLOORBOARD ||
                myRoomObjData.LocalData.typeName == MyRoomObject.TYPE_CEILING)
            {
                MyRoomObject sameLayerObject = PlacedObjectList.Find(x => x.GetLayerValue() == spawnedObject.GetLayerValue());
                if (sameLayerObject != null) ReturnMyRoomObjectToInventory(sameLayerObject, false);
            }

            MapCursor.SetPlace(TileCursor, spawnedObject);
            PlacedObjectList.Add(spawnedObject);

            return spawnedObject;
        }

        private void SpawnHero(MyRoomHeroData heroData, bool isStartMoveRoutine)
        {
            GameCore.Instance.ResourceMgr.GetInstanceObject(ABType.AB_Prefab, heroData.LocalData.prefabId, (_obj) =>
            {
                if (_obj == null)
                {
                    Debug.LogError("Load Fail!!");
                    return;
                }

                _obj.transform.rotation = Quaternion.Euler(35, 0, 0);
                _obj.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                SpineCharacterCtrl spineCtrl = _obj.gameObject.AddComponent<SpineCharacterCtrl>();
                spineCtrl.Init(false, 0, null);
                spineCtrl.SetAnimation(SpineAnimation.Idle, false);

                MyRoomHeroData data = new MyRoomHeroData();
                data.ServerData = heroData.ServerData;
                data.LocalData = heroData.LocalData;

                MyRoomHeroObject temp = _obj.AddComponent<MyRoomHeroObject>();
                temp.Initialize(data, RefParameter);
                PlacedHeroList.Add(temp);

                MapCursor = FloorTileMap;

                var useAbleTileList = MapCursor.GetUseAbleTileList(1,0);
                if (useAbleTileList.Count == 0) return;

                TransformTile randomSelectedTile = null;

                for (int i = 0; i < useAbleTileList.Count; ++i)
                {
                    TransformTile tempTile = useAbleTileList[UnityEngine.Random.Range(0, useAbleTileList.Count - 1)];
                    TransformTile rightTIle = MapCursor.GetTile(tempTile.X + 1, tempTile.Y);
                    TransformTile leftTIle = MapCursor.GetTile(tempTile.X, tempTile.Y);

                    if (rightTIle != null && leftTIle != null)
                    {
                        randomSelectedTile = tempTile;
                        break;
                    }
                }

                MapCursor.SetPlace(randomSelectedTile, temp);
                MapCursor.ApplyPlacedObject(randomSelectedTile, temp);
                if (Observer.OnPlacedMyRoomHero != null) Observer.OnPlacedMyRoomHero.Invoke(temp);//비동기로 인해서 호출해 줘야함
                MapCursor.ControlTileRender(false);
                if (isStartMoveRoutine) temp.StartMoveRoutine();
                else
                {
                    if (RefParameter.IsVisit == false) SelectObject(temp);
                }
            });
        }

        private MyRoomTileMap SelectMapCursor(string objectTypeName)
        {
            switch (objectTypeName)
            {
                case MyRoomObject.TYPE_RUG:
                    MapCursor = RugTileMap;
                    break;
                case MyRoomObject.TYPE_WALL:
                case MyRoomObject.TYPE_PROP:
                case MyRoomObject.TYPE_CEILING:
                    MapCursor = WallTileMap;
                    return WallTileMap;
                case MyRoomObject.TYPE_FLOOR:
                case MyRoomObject.TYPE_FURNITURE:
                case MyRoomObject.TYPE_FLOORBOARD:
                case MyRoomHeroObject.TYPE_CHARACTER:
                    MapCursor = FloorTileMap;
                    break;
            }

            return FloorTileMap;
        }

        private void StainRenderControl(bool active)
        {
            for (int i = 0; i < StainObjectList.Count; ++i)
            {
                if (StainObjectList[i].CurrentState == StainObject.State.Removed) continue;
                StainObjectList[i].gameObject.SetActive(active);
            }
        }

        private void CleanDirtyDust(long stainUID)
        {
            var target = MyRoomCursor.StainDataList.Find(stain => stain.UniqueId.Equals(stainUID));
            if (target == null) return;
            if (InnerParameter.IsVisit) target.HelpUserId = InnerParameter.TargetUserID;
            else                        target.HelpUserId = 0;// "0I";

            Send_CleanStain(target);
        }


        /// <summary>
        /// 일괄 청소 시작 리스트를 만들어 회신이 오면 순차적으로 송신한다.
        /// </summary>
        /// <returns></returns>
        public int CleanAllDirtyDust()
        {
            if (cleanAllQ.Count != 0)
                return cleanAllQ.Count;

            foreach(var target in MyRoomCursor.StainDataList)
            {
                if (target == null || target.CleanStartTime != default(DateTime))
                    continue;

                if (InnerParameter.IsVisit) target.HelpUserId = InnerParameter.TargetUserID;
                else                        target.HelpUserId = 0;

                if (!RefParameter.IsVisit && GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Mailage) <= cleanAllQ.Count)
                {
                    Debug.LogError("마일리지 부족");
                    break;
                }

                //Send_CleanStain(target);
                cleanAllQ.Enqueue(target);
            }

            SendNextStartCleanStain();

            return cleanAllQ.Count;
        }

        public static void MergeRewardItemSdata(ref List<CardSData> _list)
        {
            for (int i = _list.Count-1; 0 <= i; --i)
            {
                if (_list[i].type == CardType.Character)
                    continue;

                for (int j = 0; j < i; ++j)
                {
                    if (_list[i].key != _list[j].key)
                        continue;

                    ((ItemSData)_list[j]).count += ((ItemSData)_list[i]).count;
                    _list.RemoveAt(i);
                    break;
                }
            }
        }

        bool SendNextStartCleanStain()
        {
            if (cleanAllQ.Count == 0)
                return false;

            Send_CleanStain(cleanAllQ.Peek());
            return true;
        }

        /// <summary>
        /// 청소를 시작할 수 있는 먼지 개수
        /// </summary>
        /// <returns></returns>
        public int GetCleanableDirtyDustCount()
        {
            int count = 0;
            foreach(var target in MyRoomCursor.StainDataList)
            {
                if (target == null || target.CleanStartTime != default(DateTime))
                    continue;

                count++;
            }

            return count;
        }


        private void ReceiveReward(long stainUID)
        {
            var target = MyRoomCursor.StainDataList.Find(stain => stain.UniqueId.Equals(stainUID));
            if (target == null) return;
            Send_TakePresent(target);
        }


        public int ReceiveAllReward(long stainUID) // Not used
        {
            int count = 0;
            foreach (var target in MyRoomCursor.StainDataList)
            {
                if (target == null || target.CleanStartTime == default(DateTime) || target.CleanEndTime < GameCore.nowTime)
                    continue;

                Send_TakePresent(target);
                count++;
            }

            return count;
        }

        /// <summary>
        /// 청소 완료된 먼지 개수
        /// </summary>
        /// <returns></returns>
        public int GetCleanedDirtyDustCount()
        {
            int count = 0;
            foreach (var target in MyRoomCursor.StainDataList)
            {
                if (target == null || target.CleanEndTime > GameCore.nowTime)
                    continue;

                count++;
            }

            return count;
        }

        private void SelectMainRoom()
        {
            if (InnerParameter.IsVisit) return;
            Send_SelectMainRoom(MyRoomCursor.ID);
        }

        /// <summary>
        /// ButtonIndex(0~9)일경우 MyRoomData[i]로 변경되게끔 작동합니다.
        /// </summary>
        /// <param name="index"></param>
        private void ChangeMyRoom(int index)
        {
            if (index < 0 || 9 < index) return;

            MyRoomSysParameter parameter = new MyRoomSysParameter();
            parameter.IsVisit = false;
            index += 1;
            parameter.TargetMyRoom = UserMyRoomInfo.MyRoomDataList.Find(room => room.ID == index);
            parameter.TargetUserID = GameCore.Instance.PlayerDataMgr.LocalUserData.UserID;
            if (parameter.TargetMyRoom == null) return;
            GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, parameter);
        }

        private void MoveToRevengeMatch(long historyUid)
        {
            MyRoomHistoryLogData targetData = HistoryLogDataList.Find(target => target.HistoryUID == historyUid);
            if (targetData == null) return;

            PvPMatchSysPara parameter = new PvPMatchSysPara();
            parameter.isRevenge = true; //??? 뭐지 이거
            parameter.historyUID = (int)targetData.HistoryUID;
            parameter.returnButtonDelegate = () =>
            {
                GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, null);
            };

            GameCore.Instance.ChangeSubSystem(SubSysType.PvPMatch, parameter);
        }

        private void VisitFriendMyRoom(long userUID)
        {
            InnerParameter.TargetUserID = userUID;
            GameCore.Instance.NetMgr.Req_MyRoomFriendRoomDataList(userUID);
        }

        #region MyRoomObject EditMode Method

        private void StartEditMode()
        {
            ChangeControlMode(MyRoomControlMode.SelectMyRoomObject);
            SelectedObject = null;
            TileCursor = null;
            if (Observer.OnStartEditMode != null) Observer.OnStartEditMode.Invoke();
            for (int i = 0; i < PlacedHeroList.Count; ++i)
            {
                FloorTileMap.ApplyPlacedObject(PlacedHeroList[i].PlacedTile, PlacedHeroList[i]);
            }
            FloorTileMap.ControlTileRender(false);

            AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.MyRoom, 1, 1);
        }

        private void EndEditMode()
        {
            //ChangeControlMode(MyRoomControlMode.ControlUI);
            ChangeControlMode(MyRoomControlMode.ControlMyRoomDefault);
            if (Observer.OnEndEditMode != null) Observer.OnEndEditMode.Invoke();
            for (int i = 0; i < PlacedHeroList.Count; ++i)
            {
                if (PlacedHeroList[i].IsStartMoveRoutine == false)
                    PlacedHeroList[i].StartMoveRoutine();
            }
        }

        private void ApplyEdit()
        {
            EndEditMode();
        }

        /// <summary>
        /// 오브젝트편집UI 진입시 콜(isSpawned ? 인벤토리 선택으로 진입 : 일반 진입)
        /// </summary>
        private void StartObjectEditMode(bool isSpawned)
        {
            if (SelectedObject.AttachConditionType != "n") MapCursor.SetActiveTileRenderByAttachCondition(SelectedObject);
            else MapCursor.ControlTileRender(true);
            ChangeControlMode(MyRoomControlMode.ControlMyRoomObject);

            ///쫌 줄이자
            for (int i = 0; i < PlacedObjectList.Count; ++i)
            {
                if (SelectedObject == PlacedObjectList[i]) continue;
                Color temp = PlacedObjectList[i].Sprite_Renderer.material.color;
                temp.a = 0.5f;
                PlacedObjectList[i].Sprite_Renderer.material.color = temp;
            }

            for (int i = 0; i < PlacedHeroList.Count; ++i)
            {
                if (SelectedObject as object == PlacedHeroList[i] as object) continue;


                PlacedHeroList[i].SkeletonAnim.Skeleton.a = 0.5f;
            }

            if (Observer.OnStartMyRoomObjectEditMode != null) Observer.OnStartMyRoomObjectEditMode.Invoke(SelectedObject);

            AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.MyRoom, 3, 1);
        }

        private bool EndObjectEditMode(bool useSetPlaceCheck)
        {
            if (useSetPlaceCheck)
            {
                if (MapCursor.SetPlace(SelectedObject.PlacedTile, SelectedObject) == false) return false;
                MapCursor.ApplyPlacedObject(SelectedObject.PlacedTile, SelectedObject);
                if (SelectedObject.IsSameClass(typeof(MyRoomObject)))
                {
                    MyRoomObject sendTarget = SelectedObject as MyRoomObject;
                    sendTarget.UpdateMyRoomServerData();
                    Send_PlaceMyRoomObjectData(sendTarget.MyRoomServerData);
                }
            }

            MapCursor.ControlTileRender(false);

            for (int i = 0; i < PlacedObjectList.Count; ++i)
            {
                Color temp = PlacedObjectList[i].Sprite_Renderer.material.color;
                temp.a = 1.0f;
                PlacedObjectList[i].Sprite_Renderer.material.color = temp;
            }

            for (int i = 0; i < PlacedHeroList.Count; ++i)
            {
                if (SelectedObject as object == PlacedHeroList[i] as object) continue;
                PlacedHeroList[i].SkeletonAnim.Skeleton.a = 1.0f;
            }

            if (Observer.OnEndMyRoomObjectEditMode != null) Observer.OnEndMyRoomObjectEditMode.Invoke(SelectedObject);


            if (SelectedObject != null) DeSelectObject();
            StartEditMode();
            rayController.MoveCamera(CameraRayController.CameraMoveDir.Center);
            return true;
        }

        private void UpdateObjectData(MyRoomObjectData myRoomObjData)
        {
            MyRoomObject target = PlacedObjectList.Find(obj => obj.MyRoomServerData.MyRoomUniqueid == myRoomObjData.MyRoomUniqueid);
            if (target == null)
            {
                string richText1 = "숙소 배치된 아이템 오류";
                GameCore.Instance.ShowAlert(richText1);
            }
            target.MyRoomServerData = myRoomObjData;
        }

        private void RequestSpawnObjectByInventory(long heroUid)
        {
            MyRoomObjectData sendData = MyRoomObjectData.ConvertItemServerDataUidToPacketData(heroUid, RefParameter);

            if (sendData == null)
            {
                string richText1 = "숙소 아이템 오류";
                GameCore.Instance.ShowAlert(richText1);
                return;
            }

            Send_PlaceMyRoomObjectData(sendData);
        }

        private void RequestSpawnHeroByInventory(long heroUid)
        {
            Send_PlaceMyRoomHero((int)heroUid);
        }

        /// <summary>
        /// 인벤토리로 되돌리기(현재 커서)
        /// </summary>
        private void ReturnSelectedObjectToInventory()
        {
            IPlaceAbleObject target = SelectedObject;

            ReturnMyRoomObjectToInventory(target, true);
        }

        /// <summary>
        /// 인벤토리로 되돌리기(MyRoomObject)
        /// </summary>
        private void ReturnMyRoomObjectToInventory(IPlaceAbleObject target, bool autoActiveEditMode)
        {
            //버그로 인한 임시 코드 방문시 이게 호출되는일이 뭐가 있지?
            if (InnerParameter.IsVisit) return;

            if (target.IsSameClass(typeof(MyRoomObject)))
            {
                MyRoomObject myRoomObject = target as MyRoomObject;

                if (PlacedObjectList.Contains(myRoomObject))
                {
                    PlacedObjectList.Remove(myRoomObject);
                    MyRoomCursor.PlacedObjectList.Remove(myRoomObject.MyRoomServerData);
                }
                else
                {
                    return;
                }

                //(myRoomObject.ServerData as ItemSData).myRoomCount -= 1;
                //(myRoomObject.ServerData as ItemSData).count += 1;

                Send_ReturnToInventoryObject(myRoomObject);
                MyRoomCursor.PlacedObjectList.Remove(myRoomObject.MyRoomServerData);
            }
            else
            {
                MyRoomHeroObject selectedObject = target as MyRoomHeroObject;

                List<MyRoomHeroObject> heroList = new List<MyRoomHeroObject>();
                heroList.Add(selectedObject);

                Send_ReturnToInventoryHero(heroList);
                PlacedHeroList.Remove(selectedObject);
            }

            SelectMapCursor(target.ObjectTypeName);

            target.Dispose();
            target.Destory();

            switch (target.ObjectTypeName)
            {
                case MyRoomObject.TYPE_WALL:
                    WallTileMap.TileMapBackGround.sprite = resourceAtlas.GetSprite("W_00_0_1");
                    break;
                case MyRoomObject.TYPE_FLOOR:
                    FloorTileMap.TileMapBackGround.sprite = resourceAtlas.GetSprite("F_00_0_1");
                    break;
            }

            if (Observer.OnDeletedMyRoomObject != null) Observer.OnDeletedMyRoomObject.Invoke(target);

            MapCursor.ControlTileRender(false);

            for (int i = 0; i < PlacedObjectList.Count; ++i)
            {
                Color temp = PlacedObjectList[i].Sprite_Renderer.material.color;
                temp.a = 1.0f;
                PlacedObjectList[i].Sprite_Renderer.material.color = temp;
            }

            for (int i = 0; i < PlacedHeroList.Count; ++i)
            {
                if (SelectedObject as object == PlacedHeroList[i] as object) continue;
                PlacedHeroList[i].SkeletonAnim.Skeleton.a = 1.0f;
            }

            if (Observer.OnEndMyRoomObjectEditMode != null) Observer.OnEndMyRoomObjectEditMode.Invoke(SelectedObject);

            SelectedObject = null;
            if (autoActiveEditMode) StartEditMode();
        }

        private void SelectObject(IPlaceAbleObject target)
        {
            if (SelectedObject != null) DeSelectObject();
            SelectedObject = target;
            SelectedObject.Detach();
            SelectedObject.ChangeLayerValueToMax();
            TileCursor = SelectedObject.PlacedTile;
            SelectMapCursor(target.ObjectTypeName);
            MapCursor.SetPlace(TileCursor, SelectedObject);
            StartObjectEditMode(false);
        }

        private void DeSelectObject()
        {
            if (SelectedObject != null) SelectedObject.ChangeLayerValueToOrigin();
            SelectedObject = null;
        }

        private void FlipObject()
        {
            if (SelectedObject.IsSameClass(typeof(MyRoomObject)))
            {
                MyRoomObject selectedObject = SelectedObject as MyRoomObject;
                selectedObject.Sprite_Renderer.flipX = !selectedObject.Sprite_Renderer.flipX;
            }
            else
            {
                MyRoomHeroObject selectedObject = SelectedObject as MyRoomHeroObject;
                selectedObject.Flip();
            }
        }

        private void ReturnToOriginPosition()
        {
            TileCursor = SelectedObject.LastPlacedTile;
            MapCursor.SetPlace(TileCursor, SelectedObject);
        }

        private void ClearRoom()
        {
            foreach (var placedObject in PlacedObjectList)
            {
                MyRoomCursor.PlacedObjectList.Remove(placedObject.MyRoomServerData);

                //(placedObject.ServerData as ItemSData).myRoomCount -= 1;
                //(placedObject.ServerData as ItemSData).count += 1;

                Send_ReturnToInventoryObject(placedObject);

                placedObject.Dispose();
                placedObject.Destory();

                if (Observer.OnDeletedMyRoomObject != null) Observer.OnDeletedMyRoomObject.Invoke(placedObject);
            }

            PlacedObjectList.Clear();

            foreach (var hero in PlacedHeroList)
            {
                hero.Dispose();
                hero.Destory();

                if (Observer.OnDeletedMyRoomObject != null) Observer.OnDeletedMyRoomObject.Invoke(hero);
            }

            Send_ReturnToInventoryHero(PlacedHeroList);
            PlacedHeroList.Clear();

            MyRoomCursor.PlacedHeroList.Clear();
            MyRoomCursor.PlacedObjectList.Clear();

            WallTileMap.ControlTileRender(false);
            FloorTileMap.ControlTileRender(false);
            RugTileMap.ControlTileRender(false);

            var wall = RoomBaseObjectDic["wall"];
            var wallTileMapRoot = RoomBaseObjectDic["wallTileMapRoot"];
            var wallDecoPivot = RoomBaseObjectDic["wallDecoPivot"];
            var floor = RoomBaseObjectDic["floor"];
            var floorTileMapRoot = RoomBaseObjectDic["floorTileMapRoot"];
            var floorDecoPivot = RoomBaseObjectDic["floorDecoPivot"];
            var rugFloor = RoomBaseObjectDic["rugFloor"];
            var rugFloorTileMapRoot = RoomBaseObjectDic["rugFloorTileMapRoot"];
            var rugFloorDecoPivot = RoomBaseObjectDic["rugFloorDecoPivot"];

            RoomBaseObjectDic.Remove("wall");
            RoomBaseObjectDic.Remove("wallTileMapRoot");
            RoomBaseObjectDic.Remove("wallDecoPivot");
            RoomBaseObjectDic.Remove("floor");
            RoomBaseObjectDic.Remove("floorTileMapRoot");
            RoomBaseObjectDic.Remove("floorDecoPivot");
            RoomBaseObjectDic.Remove("rugFloor");
            RoomBaseObjectDic.Remove("rugFloorTileMapRoot");
            RoomBaseObjectDic.Remove("rugFloorDecoPivot");

            GameObject.Destroy(wall.gameObject);
            GameObject.Destroy(wallTileMapRoot.gameObject);
            GameObject.Destroy(wallDecoPivot.gameObject);
            GameObject.Destroy(floor.gameObject);
            GameObject.Destroy(floorTileMapRoot.gameObject);
            GameObject.Destroy(floorDecoPivot.gameObject);
            GameObject.Destroy(rugFloor.gameObject);
            GameObject.Destroy(rugFloorTileMapRoot.gameObject);
            GameObject.Destroy(rugFloorDecoPivot.gameObject);

            CreateMyRoomBaseObject();

            if (Observer.OnClearRoom != null) Observer.OnClearRoom.Invoke();
        }

        #endregion

        #region Input Control Method

        public void OnRayCastHit(Collider collider)
        {
            if (SelectedObject != null && SelectedObject.Collider == collider) return;

            MyRoomHeroObject hitHeroTarget = null;
            hitHeroTarget = PlacedHeroList.Find(hero => hero.Collider == collider);
            if (hitHeroTarget != null)
            {
                SelectObject(hitHeroTarget);
                return;
            }

            if (SelectedObject != null && SelectedObject.Collider == collider) return;

            MyRoomObject hitObjectTarget = null;
            hitObjectTarget = PlacedObjectList.Find(obj => obj.Collider == collider);

            SelectObject(hitObjectTarget);
        }
        public void OnRayCastChracterDialogue(Collider collider)
        {
            if (SelectedObject != null && SelectedObject.Collider == collider) return;

            MyRoomHeroObject hitHeroTarget = null;
            
            //hitHeroTarget.SkeletonAnim.AnimationState.SetAnimation(SpineAnimation.MyRoom, false);
            //skelAnim.AnimationState
            hitHeroTarget = PlacedHeroList.Find(hero => hero.Collider == collider);
            if (hitHeroTarget != null)
            {
                if (MyRoomUI.IsActiveUI())
                    return;

                //Debug.Log("와 들어온다~!");
                UnitDataMap unitData = hitHeroTarget.HeroData.LocalData;
                if(characterDialogueScript.SetDialogue(hitHeroTarget.transform, unitData, DialogueType.MyRoom, ()=>
                {
                    hitHeroTarget.IsActive = true;
                }) == true)
                    hitHeroTarget.SetEmotionHeroObject();
                //SelectObject(hitHeroTarget);
                return;
            }
        }

        public void OnMouseMoved(Vector3 interval)
        {
            if (AutonomyTutorial.IsRunning || MyRoomUI.IsActiveUI())
                return;

            if (SelectedObject == null) return;

            int xValue = (int)Mathf.Clamp(interval.x, -1, 1);
            int yValue = (int)Mathf.Clamp(interval.y, -1, 1);

            TransformTile moveTargetTile = MapCursor.GetTileOrNull(SelectedObject.PlacedTile.X + xValue,
                                                                    SelectedObject.PlacedTile.Y + yValue);

            if (moveTargetTile == null) return;

            TileCursor = moveTargetTile;

            if (TileCursor.X < 7)                          rayController.MoveCamera(CameraRayController.CameraMoveDir.Left);
            else if (TileCursor.X > MapCursor.LengthX - 7) rayController.MoveCamera(CameraRayController.CameraMoveDir.Right);
            else if (TileCursor.Y < 7)                     rayController.MoveCamera(CameraRayController.CameraMoveDir.Down);
            else if (TileCursor.Y > MapCursor.LengthY - 7) rayController.MoveCamera(CameraRayController.CameraMoveDir.Up);
            else                                           rayController.MoveCamera(CameraRayController.CameraMoveDir.Center);

            MapCursor.SetPlace(TileCursor, SelectedObject);
        }

        private void ChangeControlMode(MyRoomControlMode mode)
        {
            rayController.OnRayCastHit = null;
            rayController.OnMouseMovedInterval = null;
            rayController.OnRayCastNull = null;

            switch (mode)
            {
                case MyRoomControlMode.SelectMyRoomObject:
                    rayController.enabled = false;
                    GameCore.Instance.StartCoroutine(GameCore.WaitForTime(0.25f, () => { rayController.enabled = true; }));
                    rayController.OnRayCastHit += OnRayCastHit;
                    break;
                case MyRoomControlMode.ControlMyRoomObject:
                    rayController.ClearPoint();
                    rayController.OnMouseMovedInterval += OnMouseMoved;
                    break;
                case MyRoomControlMode.ControlMyRoomDefault:
                    rayController.enabled = false;
                    GameCore.Instance.StartCoroutine(GameCore.WaitForTime(0.25f, () => { rayController.enabled = true; }));
                    rayController.OnRayCastHit += OnRayCastChracterDialogue;
                    break;
                case MyRoomControlMode.ControlUI:
                    rayController.OnRayCastNull += OnRayCastNull;
                    break;
            }

            PrevControlMode = CurrentControlMode;
            CurrentControlMode = mode;

        }

        private void OnRayCastNull()
        {
            if (Observer.OnRayCastCheckNull != null) Observer.OnRayCastCheckNull.Invoke();
        }

        #endregion

        #region TopBottomUI Observer

        private void OnClickFriendIcon(bool IsOpen)
        {
            if (IsOpen)
            {
                NegativeButtonStatck.Push(() => { GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend(); });
                ChangeControlMode(MyRoomControlMode.ControlUI);
            }
            else
            {
                ChangeControlMode(PrevControlMode);
                if (NegativeButtonStatck.Count > 1)
                {
                    NegativeButtonStatck.Pop();
                }
            }

        }

        private void OnClickMailIcon(bool IsOpen)
        {
            if (IsOpen)
            {
                NegativeButtonStatck.Push(() => { GameCore.Instance.CommonSys.tbUi.OnClosedMail(); });
                ChangeControlMode(MyRoomControlMode.ControlUI);
            }
            else
            {
                ChangeControlMode(PrevControlMode);
                if (NegativeButtonStatck.Count > 1)
                {
                    NegativeButtonStatck.Pop();
                }
            }

        }

        #endregion


        event Action cbEnterSys;
        public void DoWaitOnLoad(Action _cb)
        {
            cbEnterSys += _cb;
        }
    }

}

