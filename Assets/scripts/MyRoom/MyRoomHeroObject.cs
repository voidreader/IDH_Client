using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;
using IDH.MyRoom;

public class MyRoomHeroObject : MonoBehaviour, IPlaceAbleObject
{
    public bool IsSameClass(Type type)
    {
        return type == this.GetType();
    }


    public const string TYPE_CHARACTER = "Character";

    public int MaxSizeX { get { return SizeData[0].Length; } }
    public int MaxSizeY { get { return SizeData.Length; } }
    public int CenterX { get; private set; }
    public int CenterY { get; private set; }
    public string RawSizeData { get { return rawData; } }
    public string ObjectTypeName { get { return TYPE_CHARACTER; } }
    public string AttachConditionType { get { return "n"; } }
    public int AttachConditionValue { get { return 0; } }

    public TransformTile PlacedTile { get; set; }
    public TransformTile LastPlacedTile { get; set; }

    string rawData = "12";
    public string[] SizeData { get; private set; }

    public MyRoomHeroData HeroData { get; private set; }
    public int UsedRoomID { get; private set; }
    public SkeletonAnimation SkeletonAnim { get; private set; }
    public MeshRenderer SkeletonRenderer { get; private set; }
    public Collider Collider { get; private set; }
    public TransformTile[] UsingTileList { get; set; }

    private SpineCharacterCtrl SpineController { get; set; }
    public bool IsActive = true;
    public bool IsStartMoveRoutine = false;
    private bool IsSelected = false;

    public void Initialize(MyRoomHeroData data, MyRoomSystemRefParameter Parameter)
    {
        SkeletonAnim = GetComponent<SkeletonAnimation>();

        SpineController = gameObject.AddComponent<SpineCharacterCtrl>();
        SpineController.Init(false, 0, null);
        SpineController.SetAnimation(SpineAnimation.MyRoom, false);

        SkeletonRenderer = GetComponent<MeshRenderer>();
        Collider = gameObject.AddComponent<BoxCollider>();

        HeroData = data;
        UsedRoomID = data.ServerData.dormitory;

        SizeData = rawData.Split(',');

        CenterX = SizeData[0].Length / 2;
        CenterY = SizeData.Length / 2;

        Parameter.Observer.OnStartEditMode += () => { IsActive = false; };
        Parameter.Observer.OnEndEditMode += () => { IsActive = true; };
    }

    public void StartMoveRoutine()
    {
        IsStartMoveRoutine = true;
        StartCoroutine(MoveRoutine());
    }

    public void Dispose()
    {
        Detach();
        if (PlacedTile != null) PlacedTile = null;

    }

    public void Destory()
    {
        GameObject.Destroy(gameObject);
    }

    public void Detach()
    {
        //if (UsingTileList != null)
        //{
        //    for (int i = 0; i < UsingTileList.Length; ++i)
        //    {
        //        UsingTileList[i].SetState(TransformTile.TileState.UseAble);
        //        UsingTileList[i].InUseMyRoomObject = null;
        //    }
        //}
    }

    public void Place(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        if (IsSelected) return;
        ChangeLayerValueToOrigin();
    }

    public void Flip()
    {
        SkeletonAnim.Skeleton.FlipX = !SkeletonAnim.Skeleton.FlipX;
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, 10.0f));
            if (IsActive)
            {
                yield return StartCoroutine(MoveToPathListRoutine(MyRoomHeroData.FloorTileMap.GetRandomPathTileList(PlacedTile)));
            }
        }
    }


    private IEnumerator MoveToPathListRoutine(List<TransformTile> pathList)
    {
        SpineController.SetAnimation(SpineAnimation.Run, false);

        for (int i = 0; i < pathList.Count; ++i)
        {
            if (IsActive == false) break;

            yield return MoveToGoalTileRoutine(pathList[i]);
        }

        //MyRoomHeroData.FloorTileMap.SetPlace(PlacedTile, this);
        //MyRoomHeroData.FloorTileMap.ApplyPlacedObject(PlacedTile, this);
        //MyRoomHeroData.FloorTileMap.ControlTileRender(false);
        SpineController.SetAnimation(SpineAnimation.MyRoom, false);
    }
    public void SetEmotionHeroObject()
    {
        IsActive = false;
        SpineController.SetAnimation(SpineAnimation.MyRoom, false);
    }

    public float testValue = 0.5f;

    private IEnumerator MoveToGoalTileRoutine(TransformTile goal)
    {
        if (goal.X > PlacedTile.X) SkeletonAnim.Skeleton.FlipX = false;
        else SkeletonAnim.Skeleton.FlipX = true;

        float time = 0.0f;
        Vector3 startPos = transform.position;
        while (time < testValue)
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(startPos, goal.Trans.position, time / testValue);
        }

        PlacedTile = goal;
        ChangeLayerValueToOrigin();
    }

    public void ChangeLayerValueToMax()
    {
        IsSelected = true;
        int layerValue = 32766;
        SkeletonRenderer.sortingOrder = layerValue;
        SpineController.SetDefaultOrderLayerValue(layerValue);
    }

    public void ChangeLayerValueToOrigin()
    {
        IsSelected = false;
        int layerValue = ((14000 / (PlacedTile.Y + 1))) + PlacedTile.X * 10;
        SkeletonRenderer.sortingOrder = layerValue;
        SpineController.SetDefaultOrderLayerValue(layerValue);
    }
}
