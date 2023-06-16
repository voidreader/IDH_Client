using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using IDH.MyRoom;

public class MyRoomBuildTest : MonoBehaviour 
{
    private List<MyRoomObjectData> testList = new List<MyRoomObjectData>();
    // Use this for initialization
    private MyRoom target;

    void Initialize()
    {
        testList.Clear();

        MyRoomObjectData floor = new MyRoomObjectData();
        floor.ItemId = 3600101;
        floor.LocalData = GameCore.Instance.DataMgr.GetItemData(floor.ItemId);
        floor.vectorList.Add(new Vector2(0, 0));
        floor.vectorList.Add(new Vector2(0, 0));
        testList.Add(floor);

        MyRoomObjectData wall = new MyRoomObjectData();
        wall.ItemId = 3600109;
        wall.LocalData = GameCore.Instance.DataMgr.GetItemData(wall.ItemId);
        wall.vectorList.Add(new Vector2(0, 0));
        wall.vectorList.Add(new Vector2(0, 0));
        testList.Add(wall);

        for (int i = 0; i < 5; ++i)
        {
            MyRoomObjectData data = new MyRoomObjectData();
            data.ItemId = Random.Range(3600102, 3600107);
            data.LocalData = GameCore.Instance.DataMgr.GetItemData(data.ItemId);
            data.vectorList.Add(new Vector2(Random.Range(5, 25), Random.Range(2, 5)));
            data.vectorList.Add(new Vector2(Random.Range(0, 1), 0));
            testList.Add(data);
        }

        for (int i = 0; i < 3; ++i)
        {
            MyRoomObjectData data = new MyRoomObjectData();
            data.ItemId = Random.Range(3600110, 3600114);
            data.LocalData = GameCore.Instance.DataMgr.GetItemData(data.ItemId);
            data.vectorList.Add(new Vector2(Random.Range(5, 25), Random.Range(2, 5)));
            data.vectorList.Add(new Vector2(Random.Range(0, 1), 0));
            testList.Add(data);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    Initialize();
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    target.Destroy();
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    var atlas = MyRoomSys.GetMyRoomSpriteAtlas();
        //    MyRoomSys.BuildDefualtRoom(ref atlas);
        //}

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    var atlas = MyRoomSys.GetMyRoomSpriteAtlas();
        //    target = MyRoomSys.BuildMyRoom(ref atlas, ref testList);
        //}
	}

    void Example()
    {
        MyRoom buildedRoom = MyRoomSys.BuildMyRoom(ref testList);
        buildedRoom.Destroy();
    }
}
