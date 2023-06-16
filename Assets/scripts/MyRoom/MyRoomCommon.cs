using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace IDH.MyRoom
{
    public class MyRoomInfo
    {
        public List<MyRoomData> MyRoomDataList { get; set; }
        public MyRoomData MainRoom
        {
            get
            {
                MyRoomData reVal = MyRoomDataList.Find(room => room.Delegate == 1);
                if (reVal == null) reVal = MyRoomDataList[0];
                return reVal;
            }
        }
        public int MainRoomID
        {
            get
            {
                MyRoomData reVal = MyRoomDataList.Find(room => room.Delegate == 1);
                if (reVal == null) return -1;
                return reVal.ID;
            }
        }

        public MyRoomInfo()
        {
            MyRoomDataList = new List<MyRoomData>();
        }

        public void TestPrintLog()
        {
            foreach (var roomData in MyRoomDataList)
            {
                Debug.Log(string.Format("RoomData ID : {0}, RoomData Delegate : {1}", roomData.ID, roomData.Delegate));

                foreach (var roomItem in roomData.PlacedObjectList)
                {
                    Debug.Log(string.Format("ItemId : {0},  ItemUniqueid : {1}, UniqueId : {2}, UsedRoomId : {3} VectorCount : {4}",
                        roomItem.ItemId, roomItem.ItemUniqueid, roomItem.ItemUniqueid, roomItem.UsedRoomId, roomItem.vectorList.Count));
                }

                //foreach (var roomHero in roomData.PlacedHeroList)
                //{
                //    Debug.Log(string.Format("heroid : {0},  heroUid : {1}, name : {2}",
                //        roomHero.SeverData.key, roomHero.SeverData.uid, roomHero.LocalData.name));
                //}

                foreach (var roomStain in roomData.StainDataList)
                {
                    Debug.Log(string.Format("PlacedRoomId : {0},  UniqueId : {1}, HelpUserId : {2}, RewardItemId : {3} RewardItemCount : {4}, CleanStartTime : {5}, CleanEndTime : {6}",
                        roomStain.PlacedRoomId, roomStain.UniqueId, roomStain.HelpUserId, roomStain.RewardItemId, roomStain.RewardItemCount, roomStain.CleanStartTime, roomStain.CleanStartTime));
                }
            }
        }
    }

    public class MyRoomData
    {
        public int ID;                                  //MYROOM_ID
        public int Delegate;                           //대표룸(?)
        public List<MyRoomObjectData> PlacedObjectList { get; set; }
        public List<MyRoomHeroData> PlacedHeroList { get; set; }
        public List<MyRoomStainData> StainDataList { get; set; }

        public int satisfactionValue; // 만족도 수치

        public MyRoomData() { }
    }

    public class MyRoomObjectData
    {
        /// <summary>
        /// Local Data Map 서치용 ItemID
        /// </summary>
        public int ItemId;
        /// /// <summary>
        /// 서버에서의 아이템 UID
        /// </summary>
        public long ItemUniqueid;
        /// /// <summary>
        /// 배치된 룸 ID
        /// </summary>
        public int UsedRoomId;
        /// <summary>
        /// 마이룸 UID
        /// </summary>
        public int MyRoomUniqueid;
        /// <summary>
        /// x,y, 플립여부
        /// </summary>
        public Vector3 Postion;

        public ItemSData SeverData;
        internal ItemDataMap LocalData;

        public List<Vector2> vectorList = new List<Vector2>();

        public static MyRoomObjectData ConvertItemServerDataUidToPacketData(long sDataUid, MyRoomSystemRefParameter refParameter)
        {
            MyRoomObjectData reVal = new MyRoomObjectData();

            reVal.SeverData = GameCore.Instance.PlayerDataMgr.GetItemSData(sDataUid);
            reVal.LocalData = GameCore.Instance.PlayerDataMgr.GetItemData(sDataUid);

            reVal.ItemId = reVal.LocalData.id;
            reVal.ItemUniqueid = reVal.SeverData.uid;
            reVal.UsedRoomId = refParameter.CurrentRoomID;
            reVal.MyRoomUniqueid = 0;
            reVal.vectorList = new List<Vector2>();
            reVal.vectorList.Add(Vector2.one * 999);
            reVal.vectorList.Add(Vector2.zero);

            return reVal;
        }
    }

    public class MyRoomHeroData
    {
        public static MyRoomTileMap FloorTileMap { get; set; }

        //캐릭터 고유
        public HeroSData ServerData;
        internal UnitDataMap LocalData;
    }

    public class MyRoomStainData
    {
        public int PlacedRoomId;
        public long UniqueId;

        /// <summary>
        /// 도와준 사람 없을시 null
        /// </summary>
        public long HelpUserId;
        public string HelpUserName;
        /// <summary>
        /// 청소 보상 관련 
        /// </summary>
        public int RewardItemId;
        public int RewardItemCount;

        public DateTime CleanStartTime;
        public DateTime CleanEndTime;
    }

    public class MyRoomFriendData
    {
        public long FriendUID;
        public long DelegateIconID;
        public string UserName;
        public int UserLevel;
        public DateTime LastLoginTime;
        public int OpenRoomCount;
        public int MyRoomItemCount; //?
        public int StainCount;
        public int SatisfactionCount;
    }

    public class MyRoomHistoryLogData
    {
        public long HistoryUID;
        public int HistoryType;
        public long TargetUserUID;
        public string AttackUserName;
        public int SUCCESS;
        public int REVENGE;
        public DateTime CreateTime;
    }

    public class MyRoomSystemObserver
    {
        public Action OnRayCastCheckNull { get; set; }
        public Action OnChangedMainRoom { get; set; }
        public Action OnInitializedMyRoomSystem { get; set; }
        public Action OnUpdatedHistoryData { get; set; }
        public Action OnStartEditMode { get; set; }
        public Action OnEndEditMode { get; set; }
        public Action OnClearRoom { get; set; }
        public Action<IPlaceAbleObject> OnStartMyRoomObjectEditMode { get; set; }
        public Action<IPlaceAbleObject> OnPlacedMyRoomObject { get; set; }
        public Action<IPlaceAbleObject> OnEndMyRoomObjectEditMode { get; set; }
        public Action<IPlaceAbleObject> OnPlacedMyRoomHero { get; set; }
        public Action<IPlaceAbleObject> OnDeletedMyRoomObject { get; set; }
    }

    public class MyRoomSystemCommand
    {
        public Action<long> CmdVisitFriendRoom;
        public Action<long> CmdMoveToRevengeMatch;
        public Action<long> CmdSpawnObjectByInventory;
        public Action<long> CmdSpawnHeroByInventory;
        public Action<int> CmdChangeMyRoom;
        /// <summary>
        /// 파라미터 bool은 오브젝트 배치 체크를 할것인지 안할것인지
        /// </summary>
        public Func<bool, bool> CmdMyRoomObjectEditEnd;
        public Action<IPlaceAbleObject> CmdSelectObject;
        public Action CmdSelectedObjectFlip;
        public Action<IPlaceAbleObject, bool> CmdMyRoomObjectRetrunToInventory;
        public Action<List<HeroSData>> CmdHeroReturnToInventory;
        public Action CmdSelectedObjectRetrunToInventory;
        public Action CmdSeletedObjectRetrunToOriginPosition;
        public Action CmdStartEditMode;
        public Action CmdEndEditMode;
        public Action<long> CmdCleanDirtyDust;
        public Action CmdSelectMainRoom;
        public Action<long> CmdReceiveReward;
        public Action<bool> CmdStainListActiveControl;
        public Action CmdClearRoom;
        internal Action<MyRoomSys.MyRoomControlMode> CmdChangeControlMode;

    }

    public interface IPlaceAbleObject
    {
        int MaxSizeX { get; }
        int MaxSizeY { get; }

        int CenterX { get; }
        int CenterY { get; }

        string ObjectTypeName { get; }
        string AttachConditionType { get; }
        int AttachConditionValue { get; }

        TransformTile PlacedTile { get; }
        TransformTile LastPlacedTile { get; }
        Collider Collider { get; }

        bool IsSameClass(Type type);

        void ChangeLayerValueToMax();
        void ChangeLayerValueToOrigin();
        void Dispose();
        void Destory();
        void Detach();
        void Place(Vector3 pos, Quaternion rot);
    }

    public class MyRoom
    {
        public GameObject Root { get; set; }
        public MyRoomTileMap FloorTileMap { get; set; }
        public MyRoomTileMap WallTileMap { get; set; }

        public Dictionary<string, GameObject> RoomBaseObjectDic { get; private set; }
        public List<MyRoomObject> MyRoomObjectList { get; private set; }

        public MyRoom()
        {
            RoomBaseObjectDic = new Dictionary<string, GameObject>();
            MyRoomObjectList = new List<MyRoomObject>();
        }

        public void PlaceMyRoomObject(MyRoomObject myRoomObj)
        {
            MyRoomTileMap MapCursor = null;

            switch (myRoomObj.LocalData.typeName)
            {
                case MyRoomObject.TYPE_FLOOR:
                case MyRoomObject.TYPE_FURNITURE:
                case MyRoomObject.TYPE_RUG:
                case MyRoomObject.TYPE_FLOORBOARD:
                case MyRoomHeroObject.TYPE_CHARACTER:
                    MapCursor = FloorTileMap;
                    break;
                case MyRoomObject.TYPE_WALL:
                case MyRoomObject.TYPE_PROP:
                case MyRoomObject.TYPE_CEILING:
                    MapCursor = WallTileMap;
                    break;
            }

            TransformTile TileCursor = null;

            if (myRoomObj.LocalData.typeName == MyRoomObject.TYPE_WALL)
            {
                TileCursor = MapCursor.GetTile(0, 0);
                MapCursor.SetPlace(TileCursor, myRoomObj);

            }
            else if (myRoomObj.LocalData.typeName == MyRoomObject.TYPE_FLOOR)
            {
                TileCursor = MapCursor.GetTile(0, 0);

            }
            else
            {
                TileCursor = MapCursor.GetTile((int)myRoomObj.MyRoomServerData.vectorList[0].x,
                                                    (int)myRoomObj.MyRoomServerData.vectorList[0].y);
            }

            MapCursor.SetPlaceByNoneConditionCheck(TileCursor, myRoomObj);
            MyRoomObjectList.Add(myRoomObj);
        }

        public void Destroy()
        {
            GameObject.Destroy(Root);
        }
    }

}

