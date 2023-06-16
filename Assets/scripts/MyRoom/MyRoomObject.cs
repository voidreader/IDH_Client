using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using IDH.MyRoom;

public class MyRoomObject : IPlaceAbleObject
{
    

    public const string TYPE_FLOOR = "Floor";
    public const string TYPE_FLOORBOARD = "FloorBoard";
    public const string TYPE_FURNITURE = "Furniture";
    public const string TYPE_RUG = "Rug";

    public const string TYPE_WALL = "Wall";
    public const string TYPE_PROP = "Prop";
    public const string TYPE_CEILING = "Ceiling";

    public int MaxSizeX { get { return SizeData[0].Length; } }
    public int MaxSizeY { get { return SizeData.Length; } }
    public string RawSizeData { get { return rawData; } }
    public int CenterX { get; private set; }
    public int CenterY { get; private set; }
    public string ObjectTypeName { get; private set; }
    public string AttachConditionType { get { return LocalData.attachConditionType; } }
    public int AttachConditionValue { get { return LocalData.attachConditionValue - 1; } }

    public bool IsSameClass(Type type)
    {
        return type == this.GetType();
    }

    public TransformTile PlacedTile { get; set; }
    public TransformTile LastPlacedTile { get; set; }
    public Collider Collider { get; set; }

    string rawData = "";
    public string[] SizeData { get; private set; }

    public GameObject AttachedObject { get; private set; }
    public SpriteRenderer Sprite_Renderer { get; private set; }

    public   ItemSData          ServerData { get; private set; }
    internal ItemDataMap        LocalData { get; private set; }
    public   MyRoomObjectData   MyRoomServerData { get; set; }

    public TransformTile[] UsingTileList { get; set; }
    public bool IsAblePlace = false;
    public int UsedRoomID { get; private set; }

    private bool IsSelected = false;

    internal MyRoomObject(GameObject attachedObject, ItemDataMap localData, MyRoomObjectData myRoomData)
    {
        MyRoomServerData = myRoomData;
        UsedRoomID = MyRoomServerData.UsedRoomId;
        LocalData = localData;
        AttachedObject = attachedObject;

        Initialize();
    }

    public MyRoomObject(GameObject attachedObject, ItemSData sData, MyRoomObjectData myRoomData)
    {
        ServerData = sData;

        MyRoomServerData = myRoomData;
        UsedRoomID = MyRoomServerData.UsedRoomId;
        AttachedObject = attachedObject;

        ItemDataMap itemData = GameCore.Instance.PlayerDataMgr.GetItemData(sData.uid);
        LocalData = itemData;

        Initialize();
    }

    private void Initialize()
    {
        StringBuilder sb = new StringBuilder(LocalData.sizeX + 1);

        for (int i = 0; i < LocalData.sizeX; ++i) sb.Append('1');

        rawData = sb.ToString();
        for (int i = 1; i < LocalData.sizeY; ++i)
        {
            rawData = string.Concat(rawData, ",");
            rawData = string.Concat(rawData, sb.ToString());
        }

        SizeData = rawData.Split(',');

        CenterX = SizeData[0].Length / 2;
        CenterY = SizeData.Length / 2;

        Sprite_Renderer = AttachedObject.GetComponent<SpriteRenderer>();

        switch (LocalData.typeName)
        {
            case TYPE_WALL:
                ObjectTypeName = TYPE_WALL;
                break;
            case TYPE_FLOOR:
                ObjectTypeName = TYPE_FLOOR;
                break;
            case TYPE_FLOORBOARD:
                ObjectTypeName = TYPE_FLOORBOARD;
                break;
            case TYPE_CEILING:
                ObjectTypeName = TYPE_CEILING;
                break;
            case TYPE_FURNITURE:
                AttachedObject.transform.localScale = LocalData.scaleValue;
                ObjectTypeName = TYPE_FURNITURE;
                break;
            case TYPE_PROP:
                AttachedObject.transform.localScale = LocalData.scaleValue;
                ObjectTypeName = TYPE_PROP;
                break;
            case TYPE_RUG:
                AttachedObject.transform.localScale = LocalData.scaleValue;
                ObjectTypeName = TYPE_RUG;
                break;
            default:
                ObjectTypeName = TYPE_PROP;
                break;
        }
    }

    public MyRoomObject BuildGameObject(Sprite targetImage)
    {
        Sprite_Renderer.sprite = targetImage;
        Sprite_Renderer.flipX = (MyRoomServerData.vectorList[1].x == 1);
        if (ObjectTypeName == MyRoomObject.TYPE_FURNITURE ||
            ObjectTypeName == MyRoomObject.TYPE_PROP ||
            ObjectTypeName == MyRoomObject.TYPE_RUG)
        {
            Collider = Sprite_Renderer.gameObject.AddComponent<BoxCollider>();
            if (LocalData.fileName == "F_03_2_1")
            {
                BoxCollider box = Collider as BoxCollider;
                box.center = new Vector3(-0.1f, 1.25f, 0);
                box.size = new Vector3(7.75f, 2.25f, 0.2f);
            }
        }

        return this;
    }

    public void Dispose()
    {
        if (ObjectTypeName == TYPE_FLOOR || ObjectTypeName == TYPE_WALL ||
            ObjectTypeName == TYPE_FLOORBOARD || ObjectTypeName == TYPE_CEILING) return;

        Detach();
        if (PlacedTile != null) PlacedTile = null;
    }

    public void Destory()
    {
        if (ObjectTypeName == TYPE_FLOOR || ObjectTypeName == TYPE_WALL) return;
        GameObject.Destroy(AttachedObject.gameObject);
    }

    public void Detach()
    {
        if (UsingTileList != null)
        {
            bool ablePlace = true;

            for (int i = 0; i < UsingTileList.Length; ++i)
            {
                if(UsingTileList[i].InUseMyRoomObject != this)
                {
                    ablePlace = false;
                    break;
                }
            }

            for (int i = 0; i < UsingTileList.Length; ++i)
            {
                if(ablePlace)
                {
                    UsingTileList[i].SetState(TransformTile.TileState.UseAble);
                    UsingTileList[i].InUseMyRoomObject = null;
                }
                else
                {
                    UsingTileList[i].SetState(TransformTile.TileState.UseUnable);
                }
            }
        }
    }

    public void Place(Vector3 pos, Quaternion rot)
    {
        AttachedObject.transform.position = pos;
        if (IsSelected) return;
        Sprite_Renderer.sortingOrder = GetLayerValue();
    }

    public int GetLayerValue()
    {
        switch (ObjectTypeName)
        {
            case TYPE_FLOOR: return -1;
            case TYPE_WALL: return -2;
            case TYPE_PROP: return ((LocalData.optionValue[1] + 1) * 100) + ((PlacedTile.Y + 1) * 34) + PlacedTile.X;
            case TYPE_CEILING: return 1000 + LocalData.optionValue[1];
            case TYPE_FURNITURE: return ((14000 / (PlacedTile.Y + 1))) + PlacedTile.X * 10 + LocalData.optionValue[1];
            case TYPE_FLOORBOARD: return 30000 + LocalData.optionValue[1];
            case TYPE_RUG: return LocalData.optionValue[1];
            default: return LocalData.optionValue[1];
        }
    }

    public void UpdateMyRoomServerData()
    {
        if (ObjectTypeName == MyRoomObject.TYPE_FURNITURE ||
            ObjectTypeName == MyRoomObject.TYPE_PROP ||
            ObjectTypeName == MyRoomObject.TYPE_RUG)
        {
            MyRoomServerData.vectorList[0] = new Vector2(PlacedTile.X, PlacedTile.Y);
            MyRoomServerData.vectorList[1] = new Vector2(Sprite_Renderer.flipX ? 1 : 0, 0);
        }
    }

    public void ChangeLayerValueToMax()
    {
        IsSelected = true;
        Sprite_Renderer.sortingOrder = 32766;
    }

    public void ChangeLayerValueToOrigin()
    {
        IsSelected = false;
        Sprite_Renderer.sortingOrder = GetLayerValue();
    }
}