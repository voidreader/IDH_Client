using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace IDH.MyRoom
{
    internal partial class MyRoomSys : SubSysBase
    {
        public static SpriteAtlas GetMyRoomSpriteAtlas()
        {
            return Resources.Load<SpriteAtlas>("MyRoom/MyRoom");
        }

        public static MyRoom BuildMyRoom(ref List<MyRoomObjectData> myRoomObjectList)
        {
            MyRoom reValRoom = new MyRoom();

            reValRoom.Root = new GameObject("MyRoomRoot");

            GameObject wall = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/MyRoom_Wall"));
            GameObject wallTileMapRoot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/TileMap_Wall"));
            GameObject wallDecoPivot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/WallDecoPivot"));

            reValRoom.WallTileMap = new MyRoomTileMap(MyRoomTileMap.TileMapType.Wall,
                                            wallTileMapRoot,
                                            wall.GetComponent<SpriteRenderer>(),
                                            wallDecoPivot.transform);

            GameObject floor = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/MyRoom_Floor"));
            GameObject floorTileMapRoot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/TileMap_Floor"));
            GameObject floorDecoPivot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/FloorDecoPivot"));

            reValRoom.FloorTileMap = new MyRoomTileMap(MyRoomTileMap.TileMapType.Floor,
                                            floorTileMapRoot,
                                            floor.GetComponent<SpriteRenderer>(),
                                            floorDecoPivot.transform);



            wall.transform.SetParent(reValRoom.Root.transform);
            wallTileMapRoot.transform.SetParent(reValRoom.Root.transform);
            wallDecoPivot.transform.SetParent(reValRoom.Root.transform);

            floor.transform.SetParent(reValRoom.Root.transform);
            floorTileMapRoot.transform.SetParent(reValRoom.Root.transform);
            floorDecoPivot.transform.SetParent(reValRoom.Root.transform);

            wallTileMapRoot.SetActive(false);
            floorTileMapRoot.SetActive(false);

            reValRoom.RoomBaseObjectDic.Add("wall", wall);
            reValRoom.RoomBaseObjectDic.Add("wallTileMapRoot", wallTileMapRoot);
            reValRoom.RoomBaseObjectDic.Add("wallDecoPivot", wallDecoPivot);
            reValRoom.RoomBaseObjectDic.Add("floor", floor);
            reValRoom.RoomBaseObjectDic.Add("floorTileMapRoot", floorTileMapRoot);
            reValRoom.RoomBaseObjectDic.Add("floorDecoPivot", floorDecoPivot);

            bool haveWall = false;
            bool haveFloor = false;

            foreach (var item in myRoomObjectList)
            {
                if (item.LocalData.typeName == MyRoomObject.TYPE_WALL) haveWall = true;
                if (item.LocalData.typeName == MyRoomObject.TYPE_FLOOR) haveFloor = true;

                if (item.LocalData.typeName == MyRoomObject.TYPE_RUG) continue;
                MyRoomObject temp = CreateMyRoomObject(item);
                temp.AttachedObject.transform.SetParent(reValRoom.Root.transform);
                reValRoom.PlaceMyRoomObject(temp);
            }


            if (haveWall) wall.GetComponent<SpriteRenderer>().enabled = false;
            if (haveFloor) floor.GetComponent<SpriteRenderer>().enabled = false;

            return reValRoom;
        }

        public static MyRoom BuildDefualtRoom(ref SpriteAtlas resourceAtlas)
        {
            Debug.LogError("BuildMyRoom Def");
            MyRoom reValRoom = new MyRoom();

            reValRoom.Root = new GameObject("MyRoomRoot");

            GameObject wall = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/MyRoom_Wall"));
            GameObject wallTileMapRoot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/TileMap_Wall"));
            GameObject wallDecoPivot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/WallDecoPivot"));
            reValRoom.WallTileMap = new MyRoomTileMap(MyRoomTileMap.TileMapType.Wall,
                                            wallTileMapRoot,
                                            wall.GetComponent<SpriteRenderer>(),
                                            wallDecoPivot.transform);

            GameObject floor = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/MyRoom_Floor"));
            GameObject floorTileMapRoot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/TileMap_Floor"));
            GameObject floorDecoPivot = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/FloorDecoPivot"));
            reValRoom.FloorTileMap = new MyRoomTileMap(MyRoomTileMap.TileMapType.Floor,
                                            floorTileMapRoot,
                                            floor.GetComponent<SpriteRenderer>(),
                                            floorDecoPivot.transform);

            wall.transform.SetParent(reValRoom.Root.transform);
            wallTileMapRoot.transform.SetParent(reValRoom.Root.transform);
            wallDecoPivot.transform.SetParent(reValRoom.Root.transform);

            floor.transform.SetParent(reValRoom.Root.transform);
            floorTileMapRoot.transform.SetParent(reValRoom.Root.transform);
            floorDecoPivot.transform.SetParent(reValRoom.Root.transform);

            wallTileMapRoot.SetActive(false);
            floorTileMapRoot.SetActive(false);

            reValRoom.RoomBaseObjectDic.Add("wall", wall);
            reValRoom.RoomBaseObjectDic.Add("wallTileMapRoot", wallTileMapRoot);
            reValRoom.RoomBaseObjectDic.Add("wallDecoPivot", wallDecoPivot);
            reValRoom.RoomBaseObjectDic.Add("floor", floor);
            reValRoom.RoomBaseObjectDic.Add("floorTileMapRoot", floorTileMapRoot);
            reValRoom.RoomBaseObjectDic.Add("floorDecoPivot", floorDecoPivot);

            return reValRoom;
        }

        private static MyRoomObject CreateMyRoomObject(MyRoomObjectData myRoomObjData)
        {
            GameObject temp = GameObject.Instantiate(Resources.Load<GameObject>("MyRoom/DummyProp"));
            MyRoomObject reVal = new MyRoomObject(temp, myRoomObjData.LocalData, myRoomObjData);

            GameCore.Instance.ResourceMgr.GetObject<SpriteAtlas>(ABType.AB_UnityAtlas, 1101, (_obj) => { 
                reVal.Sprite_Renderer.sprite = _obj.GetSprite(myRoomObjData.LocalData.fileName);
            });
            
            if (myRoomObjData.vectorList[1] != null)
                reVal.Sprite_Renderer.flipX = (myRoomObjData.vectorList[1].x == 1);

            return reVal;
        }
    }
}

