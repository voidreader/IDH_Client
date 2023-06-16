using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IDH.MyRoom;

public class MyRoomPlacedObjectBox : MonoBehaviour
{
    public const string NameTextFormat = "[24F404FF]{0}";
    public const string PositionTextFormat = "위치: {0}";

    public UISprite Icon;
    public UILabel Name;
    public UILabel Position;
    public UIButton RemoveButton;

    public MyRoomObject TargetObject { get; private set; }
    private MyRoomSystemRefParameter Parameter { get; set; }

    public void Initialize(MyRoomSystemRefParameter parameter, MyRoomObject myRoomObject)
    {
        TargetObject = myRoomObject;
        Parameter = parameter;

        ItemDataMap itemData = myRoomObject.LocalData as ItemDataMap;
        GameCore.Instance.SetUISprite(Icon, itemData.GetCardSpriteKey());
        //string fileName = itemData.fileName;
        //Icon.spriteName = string.Concat("128_", fileName);
        //Icon.atlas = GameCore.Instance.ResourceMgr.GetLocalObject<UIAtlas>(string.Concat("MyRoom/IconAtlas/", itemData.atlasName));

        Name.text = string.Format(NameTextFormat, itemData.name);

        switch (myRoomObject.ObjectTypeName)
        {
            case MyRoomObject.TYPE_FLOOR:
            case MyRoomObject.TYPE_FLOORBOARD:
            case MyRoomObject.TYPE_RUG:
            case MyRoomObject.TYPE_FURNITURE:
                Position.text = string.Format(PositionTextFormat, "바닥");
                break;
            case MyRoomObject.TYPE_WALL:
            case MyRoomObject.TYPE_CEILING:
            case MyRoomObject.TYPE_PROP:
                Position.text = string.Format(PositionTextFormat, "벽");
                break;
        }

        RemoveButton.onClick.Clear();
        RemoveButton.onClick.Add(new EventDelegate(OnClickRemoveButton));
    }

    public void OnClickRemoveButton()
    {
        Parameter.Command.CmdMyRoomObjectRetrunToInventory(TargetObject, true);
    }

    public void OnClickBox()
    {
        switch (TargetObject.ObjectTypeName)
        {
            case MyRoomObject.TYPE_FLOOR:
            case MyRoomObject.TYPE_WALL:
                break;
            default:
                Parameter.Command.CmdSelectObject.Invoke(TargetObject);
                break;
        }
        //Parameter.Command.CmdSelectObject.Invoke(TargetObject);
    }
}
