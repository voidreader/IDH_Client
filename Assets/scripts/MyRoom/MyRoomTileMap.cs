using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IDH.MyRoom;

public class MyRoomTileMap
{
    public enum TileMapType
    {
        Wall,
        Floor,
        Rug,
    }
    public Vector2 Center { get { return new Vector2(LengthX / 2, LengthY / 2); } }
    public int LengthX { get; private set; }
    public int LengthY { get; private set; }
    public TransformTile[,] TileMap { get; private set; }
    public SpriteRenderer TileMapBackGround { get; private set; }
    public List<MyRoomObject> MapDecorationList { get; private set; }
    public TileMapType MapType { get; private set; }
    private Transform DecoPivotTransform { get; set; }

    private List<TransformTile> ColoredTileList = new List<TransformTile>();

    public MyRoomTileMap(TileMapType type, GameObject tileMapRoot, SpriteRenderer tileMapBackGround, Transform decoPivot)
    {
        MapType = type;
        LengthX = tileMapRoot.transform.GetChild(0).childCount;
        LengthY = tileMapRoot.transform.childCount;

        TileMap = new TransformTile[LengthX, LengthY];

        for (int y = 0; y < LengthY; ++y)
        {
            for (int x = 0; x < LengthX; ++x)
            {
                GameObject go = tileMapRoot.transform.GetChild(y).GetChild(x).gameObject;
                TransformTile temp = new TransformTile(x, y, go.transform, go.GetComponent<MeshRenderer>());
                TileMap[x, y] = temp;
                temp.Mesh_Renderer.enabled = false;
            }
        }

        TileMapBackGround = tileMapBackGround;
        DecoPivotTransform = decoPivot;

        switch (type)
        {
            case TileMapType.Floor: TileMapBackGround.sortingOrder = -9999;     return;
            case TileMapType.Wall:  TileMapBackGround.sortingOrder = -10000;    return;
            case TileMapType.Rug:   TileMapBackGround.sortingOrder = 1;         return;
        }
       
    }

    /// <summary>
    /// 예외시 센터 타일 반환
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public TransformTile GetTile(int x, int y)
    {
        if (x < 0 || LengthX <= x) return TileMap[(int)Center.x, (int)Center.y];
        if (y < 0 || LengthY <= y) return TileMap[(int)Center.x, (int)Center.y];

        return TileMap[x, y];
    }

    public TransformTile GetTileOrNull(int x, int y)
    {
        if (x < 0 || LengthX <= x) return null;
        if (y < 0 || LengthY <= y) return null;

        return TileMap[x, y];
    }

    public List<TransformTile> GetUseAbleTileList(int paddingX = 0, int paddingY = 0)
    {
        List<TransformTile> reValList = new List<TransformTile>();

        for (int y = 0; y < LengthY - paddingY; ++y)
        {
            for (int x = 0; x < LengthX - paddingX; ++x)
            {
                if (TileMap[x, y].State == TransformTile.TileState.UseAble)
                {
                    reValList.Add(TileMap[x, y]);
                }
            }
        }

        return reValList;
    }

    public List<TransformTile> GetRandomPathTileList(TransformTile startTile)
    {
        List<TransformTile> reValList = new List<TransformTile>();

        TransformTile cursorNode = startTile;

        while (true)
        {
            if (reValList.Count > 10) break;
            TransformTile selectNode = null;
            TransformTile[] tempList = new TransformTile[2];

            tempList[0] = GetTileOrNull(cursorNode.X + 1, cursorNode.Y);
            tempList[1] = GetTileOrNull(cursorNode.X - 1, cursorNode.Y);

            for (int i = 0; i < tempList.Length; ++i)
            {
                if (tempList[i] == null) continue;
                if (tempList[i].State != TransformTile.TileState.UseAble) continue;
                if (reValList.Find(node => node == tempList[i]) != null) continue;
                if (selectNode == null)
                {
                    selectNode = tempList[i];
                }
                else
                {
                    int randomNumber = UnityEngine.Random.Range(0, 99);
                    selectNode = (randomNumber % 2 == 0) ? selectNode : tempList[i];
                }
            }

            if (selectNode == null)
            {
                tempList[0] = GetTile(cursorNode.X, cursorNode.Y + 1);
                tempList[1] = GetTile(cursorNode.X, cursorNode.Y - 1);

                for (int i = 0; i < tempList.Length; ++i)
                {
                    if (tempList[i] == null) continue;
                    if (tempList[i].State != TransformTile.TileState.UseAble) continue;
                    if (reValList.Find(node => node == tempList[i]) != null) continue;
                    if (selectNode == null)
                    {
                        selectNode = tempList[i];
                    }
                    else
                    {
                        int randomNumber = UnityEngine.Random.Range(0, 99);
                        selectNode = (randomNumber % 2 == 0) ? selectNode : tempList[i];
                    }
                }
            }

            if (selectNode == null) break;

            cursorNode = selectNode;
            reValList.Add(selectNode);
        }

        return reValList;
    }

    #region Tile RenderControl Method

    public void ControlTileRender(bool isRender)
    {
        foreach (var tile in TileMap) tile.Mesh_Renderer.enabled = isRender;
    }

    private void ControlTileRenderVertical(int x, bool isRender)
    {
        for (int i = 0; i < LengthY; ++i) TileMap[x, i].Mesh_Renderer.enabled = isRender;
    }

    private void ControlTileRenderHorizontal(int y, bool isRender)
    {
        for (int i = 0; i < LengthX; ++i) TileMap[i, y].Mesh_Renderer.enabled = isRender;
    }

    public void ClearTileColor()
    {
        foreach (var tile in ColoredTileList) tile.Mesh_Renderer.enabled = false;
        ColoredTileList.Clear();
    }

    #endregion

    public bool SetPlace(TransformTile targetTile, IPlaceAbleObject target)
    {
        if (target.IsSameClass(typeof(MyRoomObject)))
        {
            MyRoomObject myRoomObject = target as MyRoomObject;
            return SetPlaceMyRoomObject(targetTile, myRoomObject);
        }
        else
        {
            MyRoomHeroObject myRoomHero = target as MyRoomHeroObject;
            return SetPlaceMyRoomHero(targetTile, myRoomHero);
        }
    }

    private bool SetPlaceMyRoomObject(TransformTile targetTile, MyRoomObject target)
    {
        if (target.ObjectTypeName == MyRoomObject.TYPE_FLOOR || target.ObjectTypeName == MyRoomObject.TYPE_WALL)
        {
            target.AttachedObject.transform.rotation = TileMapBackGround.transform.rotation;
            target.AttachedObject.transform.position = TileMapBackGround.transform.position;
            target.Sprite_Renderer.sortingOrder = target.GetLayerValue();
            return true;
        }
        else if (target.ObjectTypeName == MyRoomObject.TYPE_FLOORBOARD || target.ObjectTypeName == MyRoomObject.TYPE_CEILING)
        {
            if (MapDecorationList == null) MapDecorationList = new List<MyRoomObject>();
            target.AttachedObject.transform.rotation = DecoPivotTransform.transform.rotation;
            target.AttachedObject.transform.position = DecoPivotTransform.transform.position;
            target.AttachedObject.transform.SetParent(DecoPivotTransform);
            target.AttachedObject.transform.localScale = Vector3.one;
            target.Sprite_Renderer.sortingOrder = target.GetLayerValue();
            MapDecorationList.Add(target);

            return true;
        }

        if (target.AttachConditionType != "n")
        {
            if (CheckToAttachCondition(targetTile, target) == false)
            {
                if (target.PlacedTile == null) targetTile = GetDefaultPlacedTile(target);
                else targetTile = target.PlacedTile;
            }
        }

        bool reVal = CheckPlaceAndChangeTileColor(targetTile, target);

        int serchStartX = targetTile.X - ((target.MaxSizeX - 1) - target.CenterX);
        int serchStartY = targetTile.Y - ((target.MaxSizeY - 1) - target.CenterY);

        switch (target.LocalData.typeName)
        {
            case MyRoomObject.TYPE_PROP:
                PlaceProp(targetTile, target);
                break;
            case MyRoomObject.TYPE_RUG:
            case MyRoomObject.TYPE_FURNITURE:
                if (targetTile.X + 1 >= LengthX || serchStartX < 0) return false;
                if (targetTile.Y + 1 >= LengthY || serchStartY < 0) return false;
                PlaceFloorObject(targetTile, TileMap[targetTile.X, serchStartY], target);
                break;
            default:
                break;
        }

        target.PlacedTile = targetTile;
        target.IsAblePlace = reVal;
        return reVal;
    }

    private bool SetPlaceMyRoomHero(TransformTile targetTile, MyRoomHeroObject target)
    {
        bool reVal = CheckPlaceAndChangeTileColor(targetTile, target);

        UnitDataMap dataMap = target.HeroData.LocalData as UnitDataMap;

        int serchStartX = targetTile.X - ((target.MaxSizeX - 1) - target.CenterX);
        int serchStartY = targetTile.Y - ((target.MaxSizeY - 1) - target.CenterY);

        if (targetTile.X + 1 >= LengthX || serchStartY < 0) return false;
        PlaceMyRoomHero(targetTile, TileMap[targetTile.X, serchStartY], target);

        target.PlacedTile = targetTile;
        return reVal;
    }

    public bool SetPlaceByNoneConditionCheck(TransformTile targetTile, MyRoomObject target)
    {
        if (target.ObjectTypeName == MyRoomObject.TYPE_FLOOR || target.ObjectTypeName == MyRoomObject.TYPE_WALL)
        {
            target.AttachedObject.transform.rotation = TileMapBackGround.transform.rotation;
            target.AttachedObject.transform.position = TileMapBackGround.transform.position;
            target.Sprite_Renderer.sortingOrder = target.GetLayerValue();
            return true;
        }

        if (target.ObjectTypeName == MyRoomObject.TYPE_FLOORBOARD || target.ObjectTypeName == MyRoomObject.TYPE_CEILING)
        {
            if (MapDecorationList == null) MapDecorationList = new List<MyRoomObject>();
            target.AttachedObject.transform.rotation = DecoPivotTransform.transform.rotation;
            target.AttachedObject.transform.position = DecoPivotTransform.transform.position;
            target.AttachedObject.transform.SetParent(DecoPivotTransform);
            target.AttachedObject.transform.localScale = Vector3.one;
            target.Sprite_Renderer.sortingOrder = target.GetLayerValue();
            MapDecorationList.Add(target);

            return true;
        }

        int serchStartX = targetTile.X - ((target.MaxSizeX - 1) - target.CenterX);
        int serchStartY = targetTile.Y - ((target.MaxSizeY - 1) - target.CenterY);

        if (serchStartX < 0) serchStartX = 0;
        if (serchStartY < 0) serchStartY = 0;

        switch (target.LocalData.typeName)
        {
            case MyRoomObject.TYPE_PROP:
                PlaceProp(targetTile, target);
                break;
            case MyRoomObject.TYPE_RUG:
            case MyRoomObject.TYPE_FURNITURE:
                PlaceFloorObject(targetTile, TileMap[targetTile.X, serchStartY], target);
                break;
            default:
                break;
        }

        target.PlacedTile = targetTile;
        target.IsAblePlace = true;
        return true;
    }

    private bool CheckToAttachCondition(TransformTile targetTile, MyRoomObject target)
    {
        if (target.AttachConditionType == "n") return true;

        switch (target.AttachConditionType)
        {
            case "u":
                return LengthY - 1 - (targetTile.Y - target.MaxSizeY / 2) <= target.AttachConditionValue;
            case "d":
                return targetTile.Y <= target.AttachConditionValue;
            case "l":
                return targetTile.X <= target.AttachConditionValue;
            case "r":
                return LengthX - 1 - (targetTile.X - target.MaxSizeX / 2) <= target.AttachConditionValue;
            default:
                return false;
        }
    }

    private bool CheckPlaceAndChangeTileColor(TransformTile targetTile, IPlaceAbleObject target)
    {
        bool reVal = true;

        foreach (var tile in ColoredTileList)
        {
            if (tile.InUseMyRoomObject != null) continue;
            tile.SetState(TransformTile.TileState.UseAble);
        }

        ColoredTileList.Clear();

        int serchStartX = targetTile.X - ((target.MaxSizeX - 1) - target.CenterX);
        int serchStartY = targetTile.Y - ((target.MaxSizeY - 1) - target.CenterY);

        for (int x = 0; x < target.MaxSizeX; ++x)
        {
            for (int y = 0; y < target.MaxSizeY; ++y)
            {
                int serchX = serchStartX + x;
                int serchY = serchStartY + y;

                if (serchX < 0 || serchX >= LengthX) { reVal = false; continue; }
                if (serchY < 0 || serchY >= LengthY) { reVal = false; continue; }

                if (TileMap[serchX, serchY].State == TransformTile.TileState.UseUnable) reVal = false;

                ColoredTileList.Add(TileMap[serchX, serchY]);
            }
        }

        foreach (var tile in ColoredTileList)
        {
            if (reVal == false) tile.SetState(TransformTile.TileState.UseUnable);
            else tile.SetState(TransformTile.TileState.Using);
        }

        return reVal;
    }

    private TransformTile GetDefaultPlacedTile(MyRoomObject target)
    {
        switch (target.AttachConditionType)
        {
            case "u": return TileMap[LengthX / 2, LengthY - 1 - target.AttachConditionValue];
            case "d": return TileMap[LengthX / 2, target.AttachConditionValue];
            case "l": return TileMap[LengthX - 1 - target.AttachConditionValue, LengthY / 2];
            case "r": return TileMap[target.AttachConditionValue, LengthY / 2];
            default: return TileMap[LengthX / 2, LengthY / 2];
        }
    }

    private void CheckAndSetStateToUsingTileList(TransformTile[] usingTileList)
    {
        bool isPlaceAble = true;

        for (int i = 0; i < usingTileList.Length; ++i)
        {
            if (usingTileList[i].State == TransformTile.TileState.UseUnable)
            {
                isPlaceAble = false;
                break;
            }
        }

        for (int i = 0; i < usingTileList.Length; ++i)
        {
            usingTileList[i].SetState(isPlaceAble ? TransformTile.TileState.Using : TransformTile.TileState.UseUnable);
        }
    }

    public void SetActiveTileRenderByAttachCondition(IPlaceAbleObject target)
    {
        ControlTileRender(false);

        int x = 0;
        int y = 0;

        switch (target.AttachConditionType)
        {
            case "u":
                y = LengthY - 1 - target.AttachConditionValue;
                for (int i = LengthY - 1; i >= y; --i) ControlTileRenderHorizontal(i, true);
                break;
            case "d":
                y = target.AttachConditionValue;
                for (int i = 0; i <= y; ++i) ControlTileRenderHorizontal(i, true);
                break;
            case "l":
                x = target.AttachConditionValue;
                for (int i = 0; 0 <= x; ++i) ControlTileRenderVertical(i, true);
                break;
            case "r":
                x = LengthX - 1 - target.AttachConditionValue;
                for (int i = LengthX - 1; i >= x; --i) ControlTileRenderVertical(i, true);
                break;
            default:
                return;
        }
    }

    /// <summary>
    /// Prop 배치시 호출
    /// </summary>
    /// <param name="targetTile"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool PlaceProp(TransformTile targetTile, MyRoomObject target)
    {
        if (targetTile.X + 1 > LengthX && target.MaxSizeX % 2 != 0) return false;
        if (targetTile.Y + 1 > LengthY && target.MaxSizeY % 2 != 0) return false;

        // 범위 체크 알고리즘이 추가로 필요할듯

        Vector3 centerVector = targetTile.Trans.position;

        if (target.MaxSizeX % 2 == 0 && targetTile.X + 1 < LengthX) // 짝수 일때 처리
        {
            centerVector.x = 0.0f;
            centerVector.x = targetTile.Trans.position.x + TileMap[targetTile.X + 1, targetTile.Y].Trans.position.x;
            centerVector.x /= 2;
        }

        if (target.MaxSizeY % 2 == 0 && targetTile.Y + 1 < LengthY) // 짝수 일때 처리
        {
            centerVector.y = 0.0f;
            centerVector.y = targetTile.Trans.position.y + TileMap[targetTile.X, targetTile.Y + 1].Trans.position.y;
            centerVector.y /= 2;
        }

        target.UsingTileList = ColoredTileList.ToArray();
        CheckAndSetStateToUsingTileList(target.UsingTileList);

        target.PlacedTile = targetTile;
        target.Place(centerVector, targetTile.Trans.rotation);

        return true;
    }

    /// <summary>
    /// Furniture, Rug 배치시 호출
    /// </summary>
    /// <param name="targetTile"></param>
    /// <param name="tempTile"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool PlaceFloorObject(TransformTile targetTile, TransformTile tempTile, MyRoomObject target)
    {
        Vector3 centerVector = tempTile.Trans.position;

        // Y는 계산이 필요 없음
        if (target.MaxSizeX % 2 == 0 && targetTile.X + 1 < LengthX) // 짝수 일때 처리
        {
            centerVector.x = 0.0f;
            centerVector.x = targetTile.Trans.position.x + TileMap[targetTile.X + 1, targetTile.Y].Trans.position.x;
            centerVector.x /= 2;
        }

        target.UsingTileList = ColoredTileList.ToArray();
        CheckAndSetStateToUsingTileList(target.UsingTileList);

        target.PlacedTile = targetTile;
        target.Place(centerVector, targetTile.Trans.rotation);

        return true;
    }

    private bool PlaceMyRoomHero(TransformTile targetTile, TransformTile tempTile, MyRoomHeroObject target)
    {
        Vector3 centerVector = tempTile.Trans.position;

        centerVector.x = 0.0f;
        centerVector.x = targetTile.Trans.position.x + TileMap[targetTile.X + 1, targetTile.Y].Trans.position.x;
        centerVector.x /= 2;

        target.UsingTileList = ColoredTileList.ToArray();

        CheckAndSetStateToUsingTileList(target.UsingTileList);

        target.PlacedTile = targetTile;
        target.Place(centerVector, targetTile.Trans.rotation);

        return true;
    }

    public void ApplyPlacedObject(TransformTile targetTile, IPlaceAbleObject target)
    {
        if (target.IsSameClass(typeof(MyRoomObject)))
        {
            MyRoomObject myRoomObject = target as MyRoomObject;
            ApplyPlacedMyRoomObject(targetTile, myRoomObject);
        }
    }

    private void ApplyPlacedMyRoomObject(TransformTile targetTile, MyRoomObject target)
    {
        if (target.ObjectTypeName == MyRoomObject.TYPE_FLOOR || target.ObjectTypeName == MyRoomObject.TYPE_WALL)
        {
            TileMapBackGround.sprite = target.Sprite_Renderer.sprite;
            target = null;
            return;
        }
        else if(target.ObjectTypeName == MyRoomObject.TYPE_FLOORBOARD || target.ObjectTypeName == MyRoomObject.TYPE_CEILING)
        {
            return;
        }

        foreach (var tile in ColoredTileList)
        {
            tile.SetState(TransformTile.TileState.UseUnable);
            tile.InUseMyRoomObject = target;
        }

        ColoredTileList.Clear();

        target.PlacedTile = targetTile;
        target.LastPlacedTile = targetTile;
        target.Place(target.AttachedObject.transform.position, targetTile.Trans.rotation);
    }

    private void ApplyPlacedMyRoomHeroObject(TransformTile targetTile, MyRoomHeroObject target)
    {
        ColoredTileList.Clear();

        int serchStartX = targetTile.X - ((target.MaxSizeX - 1) - target.CenterX);
        int serchStartY = targetTile.Y - ((target.MaxSizeY - 1) - target.CenterY);

        for (int x = 0; x < target.MaxSizeX; ++x)
        {
            for (int y = 0; y < target.MaxSizeY; ++y)
            {
                int serchX = serchStartX + x;
                int serchY = serchStartY + y;

                ColoredTileList.Add(TileMap[serchX, serchY]);
            }
        }

        foreach (var tile in ColoredTileList)
        {
            tile.SetState(TransformTile.TileState.UseUnable);
            tile.InUseMyRoomObject = target;
        }

        target.PlacedTile = targetTile;
        target.LastPlacedTile = targetTile;
        target.Place(target.transform.position, targetTile.Trans.rotation);
    }
}