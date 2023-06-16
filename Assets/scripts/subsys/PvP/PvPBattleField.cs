using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Spine.Unity;
using System;
using IDH.MyRoom;




/// <summary>
/// 전투시스템에서 배경 및 유닛 애니메이션를 처리하기위한 클래스
/// </summary>
internal class PvPBattleField : BattleFieldBase
{
    public static PvPBattleField Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/PanelBattlefield", _parent);
        return go.AddComponent<PvPBattleField>();
    }

    internal override void Init(int[] _bgIds, bool _isPvP = false)
    {
        base.Init(_bgIds, _isPvP);

        // Set Camera Transform
        cachedCamTf.rotation = Quaternion.Euler(32.5f, 0f, 0f);
        cachedCamTf.position = new Vector3(0f, 16.14f, -19.21f);
        camOrigine = new Vector3(0f, 11.8f, -19f);//cachedCamTf.position;
    }

    protected override void CreateBackground()
    {
        //SpriteAtlas atlas = MyRoomSys.GetMyRoomSpriteAtlas();
        //MyRoom buildedRoom = MyRoomSys.BuildMyRoom(ref atlas, ref testList);
        //buildedRoom.Destroy();

        /*
        if (backGrounds != null)
        {
            Debug.LogError("backGround이 이미 존재합니다!");

            for (int i = 0; i < backGrounds.Length; ++i)
                Destroy(backGrounds[i]);
            backGrounds = null;
        }
        var parent = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "Backgrounds");

        // Set BGs(without Sky)
        backGrounds = new Transform[1];
        backGrounds[0] = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/fieldSet_PvP", parent).transform;

        for (int i = 1; i < cacheBgIds.Length; ++i)
        {
            int n = i;
            if (cacheBgIds[n] > 0)
                GameCore.Instance.ResourceMgr.GetObjectAsync<Sprite>(ABType.AB_Texture, cacheBgIds[n], (Sprite sp) =>
                {
                    for (int j = 0; j < backGrounds.Length; ++j)
                        backGrounds[j].GetChild(n - 1).GetComponent<UI2DSprite>().sprite2D = sp;
                });
            else
                for (int j = 0; j < backGrounds.Length; ++j)
                    backGrounds[j].GetChild(n - 1).GetComponent<UI2DSprite>().sprite2D = null;
        }
        */
        return;
    }

    internal override void SetAdvance(float _advance)
    {
        base.SetAdvance(_advance);

        var distance = Mathf.Lerp(FieldWidth * 0.5f, 0f, _advance);
        if (moveTeamTf != null)
            moveTeamTf.position = new Vector3(-distance, 0f);
        if (enemyTeamTf != null)
            enemyTeamTf.position = new Vector3(distance, 0f);
        return;
    }
}
