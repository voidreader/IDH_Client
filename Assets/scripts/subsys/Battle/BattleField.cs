using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;




/// <summary>
/// 전투시스템에서 배경 및 유닛 애니메이션를 처리하기위한 클래스
/// </summary>
internal class BattleField : BattleFieldBase
{
    protected int prevBgDist = int.MaxValue;                // 배경의 이동 거리
    protected int prevSkyDist = int.MaxValue;               // Sky 배경의 이동거리

    protected Vector3 advance;                              // 앞으로 나아간 거리; 나아갈수록 작아진다. (이것으로 몬스터 및 배경을 움직인다.) 


    public static BattleField Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/PanelBattlefield", _parent);
        return go.AddComponent<BattleField>();
    }

    internal override void Init(int[] _bgIds, bool _isPvP = false)
    {
        base.Init(_bgIds, _isPvP);

        // Set Camera Transform
        cachedCamTf.position = new Vector3(0f, 11.8f, -19f);
        cachedNowCamPos = camOrigine = cachedCamTf.position;
    }

    protected override void CreateBackground()
    {
        if (backGrounds != null)
        {
            Debug.LogError("backGround이 이미 존재합니다!");

            for (int i = 0; i < backGrounds.Length; ++i)
                Destroy(backGrounds[i]);
            backGrounds = null;
        }

        var parent = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "Backgrounds");

        //Set Sky
        skys = new Transform[3];
        for (int i = 0; i < skys.Length; ++i)
            skys[i] = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/fieldSet_sky", parent).transform;

        if (cacheBgIds[0] != -1)
            GameCore.Instance.ResourceMgr.GetObject<Sprite>(ABType.AB_Texture, cacheBgIds[0], (Sprite sp) =>
            {
                for (int j = 0; j < skys.Length; ++j)
                    skys[j].GetChild(0).GetComponent<UI2DSprite>().sprite2D = sp;
            });
        else
            for (int j = 0; j < skys.Length; ++j)
                skys[j].GetChild(0).GetComponent<UI2DSprite>().sprite2D = null;

        // Set BGs(without Sky)
        backGrounds = new Transform[3];
        for (int i = 0; i < backGrounds.Length; ++i)
            backGrounds[i] = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/fieldSet", parent).transform;

        // Create
        for (int i = 1; i < cacheBgIds.Length; ++i)
        {
            int n = i;
            if (cacheBgIds[n] > 0)
                GameCore.Instance.ResourceMgr.GetObject<Sprite>(ABType.AB_Texture, cacheBgIds[n], (Sprite sp) =>
                {
                    for (int j = 0; j < backGrounds.Length; ++j)
                        backGrounds[j].GetChild(n - 1).GetComponent<UI2DSprite>().sprite2D = sp;
                });
            else
                for (int j = 0; j < backGrounds.Length; ++j)
                    backGrounds[j].GetChild(n - 1).GetComponent<UI2DSprite>().sprite2D = null;
        }
        return;
    }

    internal override void SetAdvance(float _advance)
    {
        base.SetAdvance(_advance);

        advance = new Vector3(_advance, 0f, 0f);
                                                
        camOrigine.x = _advance * EnemyDistance;
        cachedCamTf.position = cachedNowCamPos = camOrigine;
        blander.transform.position = cachedCamTf.position + cachedCamTf.forward;

        if (moveTeamTf != null)
            moveTeamTf.position = moveTeamOrigin + advance * EnemyDistance;
        if (friendTeamTf != null)
            friendTeamTf.localPosition = moveTeamTf.localPosition;
        int dist = Mathf.RoundToInt(_advance * (EnemyDistance / FieldWidth));
        if (prevBgDist != dist && backGrounds != null)
        {
            prevBgDist = dist;

            for (int i = 0; i < backGrounds.Length; ++i)
            {
                var pos = dist - (backGrounds.Length / 2) + i;
                backGrounds[i].localPosition = new Vector3(FieldWidth * pos, 0, 0);
            }
        }

        int skyDist = Mathf.RoundToInt(_advance * (EnemyDistance / (FieldWidth * 4)));
        if (prevSkyDist != skyDist && skys != null)
        {
            prevSkyDist = skyDist;
            for (int i = 0; i < skys.Length; ++i)
            {
                var pos = prevSkyDist - (skys.Length / 2) + i;
                skys[i].localPosition = new Vector3(FieldWidth * 4 * pos, 0, 0);
            }
        }
    }

}
