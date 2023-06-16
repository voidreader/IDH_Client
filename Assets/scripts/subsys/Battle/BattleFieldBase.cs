using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;




/// <summary>
/// 전투시스템에서 배경 및 유닛 애니메이션를 처리하기위한 클래스
/// </summary>
internal abstract class BattleFieldBase : MonoBehaviour
{
    internal static readonly float FieldWidth = 20.48f;     //필드 하나의 사이즈 (Battle/fieldSet.prefab의 너비와 일치해야 한다.)
    internal static readonly float EnemyDistance = 16f;     //적들이 배치될 간격
    
    protected ObjectPool<Transform> skillHighlight;         // 스킬 대상 하이라이트

    protected Transform[] backGrounds;                      // 배경 오브젝트
    protected Transform[] skys;                             // 하늘 배경 오브젝트

    protected int[] cacheBgIds;                             // 배경 스프라이트 데이터 Id 배열
    bool oldShowBlander = false;                            // 블라인더 출력 상태

    protected UISprite blander;
    UITweener blanderTw;

    protected Transform cachedCamTf;
    protected Vector3 camOrigine;

    protected Transform moveTeamTf;
    protected Vector3 moveTeamOrigin;

    protected Transform friendTeamTf;

    // Only Use PvP
    protected Transform enemyTeamTf;

    BattleParticleMgr ptcMgr;

    protected float tmpAdvance = 0f;
    protected Vector3 cachedNowCamPos;
    protected bool bZooming; // 줌인 또는 줌아웃 중일때(애니메이션 동안만)
    protected bool bZoom;    // 줌인 상태일때

    /// <summary>
    /// 초기화
    /// 사용전에 반드시 호출하여야 한다.
    /// </summary>
    /// <param name="_playerData">플레이어의 유닛 데이터(배치위치와 일치해여야 한다.)</param>
    /// <param name="_stageID">StageDataMap에서 색인으로 사용될 ID값</param>
    /// <return>등장하는 적 팀 총 수</return>
    internal virtual void Init(int[] _bgIds, bool _isPvP = false)
    {
        if (skillHighlight == null)
        {
            var obj = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>("Battle/underHighlight");
            skillHighlight = new ObjectPool<Transform>(obj, transform);
        }

        ptcMgr = UnityCommonFunc.GetComponentByName<BattleParticleMgr>(gameObject, "ParticleMgr");

        cacheBgIds = _bgIds;

        // Create Background
        CreateBackground();

        // camera setting
        cachedCamTf = GameCore.Instance.GetWorldCam().transform;
        cachedCamTf.rotation = Quaternion.Euler(32.5f, 0f, 0f);

        blander = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Blander");
        blanderTw = blander.GetComponent<UITweener>();
        //blander.transform.parent = cachedCamTf;
        blander.transform.position = cachedCamTf.position + cachedCamTf.forward;


        //// Set Advance
        //SetAdvance(0);
    }


    /// <summary>
    /// 배경을 생성한다. 
    /// * 현재는 모두 한번에 생성한다.
    /// </summary>
    protected abstract void CreateBackground();



    /// <summary>
    /// 전투 시스템 종료시 호출 되어야 한다.
    /// 모든 데이터를 삭제 처리한다.
    /// </summary>
    internal void clear()
    {
        if (backGrounds != null)
        {
            for (int i = 0; i < backGrounds.Length; ++i)
                Destroy(backGrounds[i].gameObject);
            backGrounds = null;
        }

        if (skys != null)
        {
            for (int i = 0; i < skys.Length; i++)
                Destroy(skys[i].gameObject);
            skys = null;
        }
    }

    float camSakePower = -1;
    internal void CamShake(float _dmg)
    {
        camSakePower = Mathf.Min(_dmg / 200, 10) * 0.1f;
    }

    internal void SetFriendTeam(Transform _tf)
    {
        friendTeamTf = _tf;
    }
    internal void SetMoveTeam(Transform _tf)
    {
        moveTeamTf = _tf;
    }
    internal void SetActiveTeam(bool isActive)
    {
        moveTeamTf.gameObject.SetActive(isActive);
    }


    // Only Use PvP
    internal void SetEnemyTeam(Transform _tf)
    {
        enemyTeamTf = _tf;
    }

    /// <summary>
    /// 이동시 배경 이동 처리를 한다.
    /// </summary>
    /// <param name="_advance">진행도</param>
    internal virtual void SetAdvance(float _advance)
    {
        tmpAdvance = _advance;

        if (bZooming)
        {
            Debug.Log("Cancel Zoom!");
            bZooming = false;
            StopAllCoroutines();
        }
    }

    internal void ShowBlander(bool _show)
    {
        if (oldShowBlander == _show)
            return;

        oldShowBlander = _show;
        blanderTw.ResetToBeginning();
        if (_show)
        {
            blanderTw.PlayForward();
        }
        else
            blanderTw.PlayReverse();
    }

    internal void ShowSkillHighlight(Transform _tf, bool _blue)
    {
        //skillHighlight.transform.localPosition = _pos;
        //skillHighlight.SetActive(true);

        var tf = skillHighlight.BringObject();
        tf.parent = _tf;
        tf.localPosition = Vector3.zero;

        tf.GetComponent<UISprite>().spriteName = (_blue) ? "TARGET_01_01" : "TARGET_02_01";
    }

    internal void DisableSkillHightlight()
    {
        //skillHighlight.SetActive(false);
        skillHighlight.ReturnObjectAll();
    }


    //internal void ShowParticle(ParticleType _type, Vector3 _uiPos)
    //{
    //    ptcMgr.ShowParticle(_type, _uiPos);
    //}

    internal void ShowParticle(ParticleType _type, BattleUnitData _unit, DamagePower atk = DamagePower.Normal, DamagePower grd = DamagePower.Normal)
    {
        string animationName = null;
        int attackType = 0;
        switch (_type)
        {
            case ParticleType.MagicSkill:
            case ParticleType.NearSkill:
            case ParticleType.GunSkill:
                switch (atk)
                {
                    case DamagePower.Critical: animationName = "Action_3"; attackType = 3; break;
                    case DamagePower.Absolute:
                    case DamagePower.Normal: animationName = "Action_2"; attackType = 2; break;
                    case DamagePower.Fail: animationName = "Action_1"; attackType = 1; break;
                }
                break;
            default:
                animationName = "Action";
                break;
        }
        //GameCore.Instance.SoundMgr.SetCommonBattleSound(_type, attackType);
        var pos = _unit.GetWorldPosition() + _unit.Center;
        ptcMgr.ShowParticle(_type, _unit, animationName);
    }

    internal void SetParticleTimeScale()
    {
        ptcMgr.SetTimeScale(GameCore.timeScale);
    }

    internal Transform GetWallRoot()
    {
        return backGrounds[0].transform.GetChild(3);
    }
    internal Transform GetFloorRoot()
    {
        return backGrounds[0].transform.GetChild(4);
    }


    // Cam Shake
    private void LateUpdate()
    {
        if (tmpAdvance == 0f)
        {
            blander.transform.position = cachedCamTf.position + cachedCamTf.forward;
            return;
        }

        if (0.02f < camSakePower)
        {
            if (bZoom)
            {
                if (bZooming)
                    cachedNowCamPos = cachedCamTf.position;
                cachedCamTf.position = cachedNowCamPos + new Vector3(UnityEngine.Random.Range(-camSakePower, camSakePower),
                                                                     UnityEngine.Random.Range(-camSakePower * 0.5f, camSakePower * 0.5f));
            }
            else
            {
                cachedCamTf.position = camOrigine + new Vector3(UnityEngine.Random.Range(-camSakePower, camSakePower),
                                                                UnityEngine.Random.Range(-camSakePower * 0.5f, camSakePower * 0.5f));
            }

            camSakePower *= Mathf.Pow(0.95f, Mathf.Max(1, GameCore.timeScale/2));
        }
        else
        {
            if (bZoom)
            {
                if (bZooming)
                    cachedNowCamPos = cachedCamTf.position;
            }
            else
                cachedCamTf.position = camOrigine;
        }

        blander.transform.position = cachedCamTf.position + cachedCamTf.forward;
    }

    public void SetZoomIn(Vector3 _tgPos, float _distance, Action _cbPosUpdate, float _delay, float _time)
    {
        if (GameCore.Instance.TimeScaleChange > 2)
            return;

        if (bZooming)
        {
            StopAllCoroutines();
        }
        StartCoroutine(CoZoomIn(_tgPos, _distance, _cbPosUpdate, _delay, _time));
    }

    IEnumerator CoZoomIn(Vector3 _tgPos, float _distance, Action _cbPosUpdate, float _delay, float _time = 0.3f)
    {
        bZoom = true;

        if (0 < _delay)
            yield return new WaitForSeconds(_delay / GameCore.timeScale);

        bZooming = true;
        var angle = cachedCamTf.rotation.eulerAngles.x * Mathf.Deg2Rad;

        var sPos = cachedCamTf.position;
        var dPos = _tgPos + new Vector3(0f, _distance * Mathf.Sin(angle), -_distance * Mathf.Cos(angle));

        float acc = Time.deltaTime * GameCore.timeScale;
        while (acc < _time)
        {
            cachedCamTf.position = Vector3.Lerp(sPos, dPos, easeOutCirc(acc / _time));
            _cbPosUpdate();
            yield return null;
            acc += Time.deltaTime * GameCore.timeScale;
        }

        cachedCamTf.position = dPos;
        _cbPosUpdate();

        bZooming = false;
    }

    public void SetZoomOut(Action _cbPosUpdate, float _delay, float _time)
    {
        if (!bZoom)
            return;

        if (bZooming)
        {
            StopAllCoroutines();
        }

        StartCoroutine(CoZoomOut(_cbPosUpdate, _delay, _time));
    }

    IEnumerator CoZoomOut(Action _cbPosUpdate, float _delay, float _time = 0.3f)
    {

        if (0 < _delay)
            yield return new WaitForSeconds(_delay / GameCore.timeScale);

        bZooming = true;
        var sPos = cachedCamTf.position;
        var dPos = camOrigine;

        float acc = Time.deltaTime * GameCore.timeScale;
        while (acc < _time)
        {
            cachedCamTf.position = Vector3.Lerp(sPos, dPos, easeOutCirc(acc / _time));
            _cbPosUpdate();

            yield return null;
            acc += Time.deltaTime * GameCore.timeScale;
        }

        cachedCamTf.position = dPos;
        _cbPosUpdate();

        bZooming = false;
        bZoom = false;
    }

    private float easeOutCirc(float v)
    {
        v--;
        return Mathf.Sqrt(1 - v * v);
    }
}
