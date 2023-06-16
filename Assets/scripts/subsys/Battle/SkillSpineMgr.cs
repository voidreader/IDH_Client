using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class SkillSpineMgr : MonoBehaviour {

    public class SkillSpineDataMap
    {
        float durationTime;
        float presentTime;
        bool isWork;
        SkeletonAnimation skeletonAnimation;
        Spine.Animation animation;
        public SkillSpineDataMap(SkeletonAnimation _skeletonAnimation)
        {
            skeletonAnimation = _skeletonAnimation;
            Debug.Log(_skeletonAnimation.name);
            presentTime = 0f;
            var skelData = skeletonAnimation.SkeletonDataAsset.GetSkeletonData(false);
            //skeletonAnimation.gameObject.layer = LayerMask.NameToLayer("Default");
            animation = skelData.FindAnimation("Action");
            durationTime = animation.duration;
            isWork = false;
        }

        /// <summary>
        /// 상태에 따라 오브젝트의 위치를 조정.
        /// </summary>
        /// <param name="atkUnit"> 공격(스킬) 사용 객체 </param>
        /// <param name="tgUnit"> 피격(효과적용) 대상 객체</param>
        /// <param name="targetType"> 공격(스킬)의 적용 타입 </param>
        private void SetSpinePosition(BattleUnitData atkUnit, BattleUnitData tgUnit, TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.AllEnemy:
                    {
                        Vector3 vecForDistance = tgUnit.TeamRootTf.GetChild(1).position - tgUnit.TeamRootTf.GetChild(4).position;
                        float distance = vecForDistance.magnitude;
                        skeletonAnimation.transform.position = tgUnit.TeamRootTf.GetChild(1).position + new Vector3(distance * 0.5f, 0, 0) * (atkUnit.IsPlayerTeam? 1: -1) + Vector3.back * 0.1f;
                    }
                    break;
                case TargetType.TeamAllCenter:
                    {
                        Vector3 vecForDistance = atkUnit.TeamRootTf.GetChild(1).position - atkUnit.TeamRootTf.GetChild(4).position;
                        float distance = vecForDistance.magnitude;
                        skeletonAnimation.transform.position = atkUnit.TeamRootTf.GetChild(1).position + new Vector3(distance * 0.5f, 0, 0) * (atkUnit.IsPlayerTeam ? -1 : 1) + Vector3.back * 0.1f;
                    }
                    break;
                case TargetType.All:
                    {
                        Vector3 vecForDistance = atkUnit.TeamRootTf.GetChild(1).position - tgUnit.TeamRootTf.GetChild(1).position;
                        float distance = vecForDistance.magnitude;
                        skeletonAnimation.transform.position = atkUnit.TeamRootTf.GetChild(1).position + new Vector3(distance * 0.5f, 0, 0) * (atkUnit.IsPlayerTeam ? 1 : -1) + Vector3.back * 0.1f;
                    }
                    break;
                default:
                    skeletonAnimation.transform.position = tgUnit.GetWorldPosition() + Vector3.back * 0.1f;
                    break;
            }
        }

        /// <summary>
        /// 베지어 커브를 구현하였지만 사용되지 않음.
        /// 
        /// PS. 베지어 커브 관련 자료 : https://www.hooni.net/xe/study/1630
        /// </summary>
        /// <returns> 곡선 좌표값. </returns>
        Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float t2 = t * t;
            float u2 = u * u;
            float u3 = u2 * u;
            float t3 = t2 * t;

            Vector3 result =
                (u3) * p0 +
                (3f * u2 * t) * p1 +
                (3f * u * t2) * p2 +
                (t3) * p3;

            return result;
        }

        /// <summary>
        /// 투사체가 대상에게 도착하는 애니메이션의 경우 좌표이동 계산.
        /// </summary>
        /// <param name="_time"> 이동까지 걸리는 시간. </param>
        /// <param name="from"> 시작 좌표 </param>
        /// <param name="to"> 도착 좌표 </param>
        /// <returns> 코루틴 반환값.</returns>
        IEnumerator MoveSkillAnimation(float _time, Vector3 from, Vector3 to)
        {
            //Vector3 p1 = from;
            //p1.y += 2f;
            //Vector3 p2 = to;
            //p2.y = p1.y;

            skeletonAnimation.transform.position = from;
            float acc = 0f;
            while (acc < _time)
            {
                acc += Time.deltaTime * GameCore.timeScale;
                var value = acc / _time;

                // 포물선Y 계산
                var v = value * 2;

                if (v < 1f) v = v - 1;
                else v = 1 - v;

                var posY = (v * v * v + 1) * 2;
                skeletonAnimation.transform.position = Vector3.Lerp(from, to/* - _unit.Transform.parent.localPosition*/, value) + Vector3.up * posY;

                yield return null;
                //acc += Time.deltaTime * GameCore.timeScale;
                //var value = acc / _time;
                //skeletonAnimation.transform.position = GetPointOnBezierCurve(from, p1, p2, to, value);
                //Debug.DrawLine(prePos, skeletonAnimation.transform.position,Color.red);
                //prePos = skeletonAnimation.transform.position;
                //yield return null;
            }
            skeletonAnimation.transform.position = to;
        }


        /// <summary>
        /// 생성된 스킬 오브젝트의 Depth값을 설정해주기위해 표준화된 Depth값을 반환.
        /// </summary>
        /// <param name="tgUnit"> 적용이 될 객체값 </param>
        /// <param name="targetType"> 공격 타입. </param>
        /// <returns> Depth값 </returns>
        private int GetSkillDepth(BattleUnitData tgUnit, TargetType targetType)
        {
            switch(targetType)
            {
                case TargetType.AllEnemy:
                case TargetType.All:
                case TargetType.TeamAllCenter:
                    return 5 /* 그래픽 출력을 원활하게 하기위하여 지정된 Deapth값. */;

                default:
                    return tgUnit.SpineCtrl.GetSortingOrder() + 1;
            }
        }


        /// <summary>
        /// 대상이 이동하여 공격하는 경우가 아닌경우,
        /// 공격(스킬)을 사용할 경우 스파인을 설정하는 함수
        /// 
        /// ex ) 검사처럼 전진하여 공격하는 객체는 해당함수가 호출되지 않음
        /// 원거리 딜러의 경우는 공격할때마다 호출 ( 투사체 )
        /// 서포터의 경우 회복 스킬 사용할때 마다 호출 ( 회복 오브젝트 )
        /// </summary>
        /// <param name="atkUnit"> 공격(스킬) 사용 객체</param>
        /// <param name="tgUnit"> 대상이 되는 객체</param>
        /// <param name="isPlayerSkill"> 플레이어의 스킬인지 확인 ( 좌표 수정을 위함 )</param>
        /// <param name="targetType"> 적용 형태. </param>
        internal void StartSkillSpine(BattleUnitData atkUnit, BattleUnitData tgUnit, bool isPlayerSkill, TargetType targetType)
        {
            presentTime = 0f;
            isWork = true;
            //skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = 10;
            skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = GetSkillDepth(tgUnit, targetType);
            skeletonAnimation.transform.rotation = Quaternion.Euler((isPlayerSkill) ? 31f : -31f, (isPlayerSkill) ? 0f : 180f, 0f);
            SetSpinePosition(atkUnit, tgUnit, targetType);
            //if (targetType == TargetType.AllEnemy)
            //{
            //    Vector3 vecForDistance = tgUnit.TeamRootTf.GetChild(1).position - tgUnit.TeamRootTf.GetChild(4).position;
            //    float distance = vecForDistance.magnitude;
            //    skeletonAnimation.transform.position = tgUnit.TeamRootTf.GetChild(1).position + new Vector3(distance * 0.5f, 0, 0);
            //}
            //else
            //{
            //    skeletonAnimation.transform.position = tgUnit.GetWorldPosition();
            //}
            skeletonAnimation.AnimationState.ClearTrack(0);
            skeletonAnimation.AnimationState.SetAnimation(0, animation, false);
            skeletonAnimation.gameObject.SetActive(true);
        }

        /// <summary>
        /// 대상 객체의 공격이 포물선 이동을 하는경우 공격 스킬의 좌표값을 수정
        /// </summary>
        /// <param name="atkUnit"> 공격 객체 </param>
        /// <param name="tgUnit"> 적용 대상 객체 </param>
        /// <param name="isPlayerSkill"> 플레이어가 사용하는 스킬인지 확인 ( 좌표 문제 )</param>
        /// <param name="targetType"> 공격 타입</param>
        internal void StartMoveSpine(BattleUnitData atkUnit, BattleUnitData tgUnit, bool isPlayerSkill, TargetType targetType)
        {
            StartSkillSpine(atkUnit, tgUnit, isPlayerSkill, targetType);
            Vector3 atkUnitWorldPos = atkUnit.GetWorldPosition();
            atkUnitWorldPos.y += atkUnit.SpineCtrl.GetboundHeight() / 2;
            atkUnitWorldPos.x += (atkUnit.IsPlayerTeam)? atkUnit.SpineCtrl.GetBoundWidth() / 2 : atkUnit.SpineCtrl.GetBoundWidth() / 2 * -1;
            GameCore.Instance.StartCoroutine(MoveSkillAnimation(0.5f, atkUnitWorldPos, tgUnit.GetWorldPosition()));
        }

        /// <summary>
        /// 공격(스킬)의 오브젝트 지속시간이 종료된 경우 비활성화 처리.
        /// </summary>
        public void CheckSpineDuration()
        {
            if (isWork == false) return;
            presentTime += Time.deltaTime * GameCore.timeScale;
            if (presentTime >= durationTime)
            {
                isWork = false;
                skeletonAnimation.gameObject.SetActive(false);
                return;
            }
            skeletonAnimation.state.Update(Time.deltaTime * GameCore.timeScale / 20f);
        }
    }

    public Dictionary<int, SkillSpineDataMap> skillTable;

    /// <summary>
    /// 리소스 매니저로부터 스킬 스파인 매니저를 추출하여 BattleUI아래 추가.
    /// </summary>
    /// <param name="_parent"> 부모 객체( BattileUI )</param>
    /// <returns></returns>
    public static SkillSpineMgr Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/SkillSpineMgr", _parent);
        return go.GetComponent<SkillSpineMgr>();
    }
    public void Init()
    {
        skillTable = new Dictionary<int, SkillSpineDataMap>();
    }
    
    /// <summary>
    /// 스킬 데이터 실시간 갱신
    /// </summary>
	void Update () {
		foreach(KeyValuePair<int, SkillSpineDataMap> skillSpineData in skillTable)
        {
            skillSpineData.Value.CheckSpineDuration();
        }
        
	}
    /// <summary>
    /// 스킬의 ID값에 맞춰 오브젝트를 추출한뒤,
    /// 즉각, 실행시킴. ( 생성 -> 적용 -> 소멸 과정 )
    /// </summary>
    /// <param name="atkUnit"> 공격을 실행하는 객체</param>
    /// <param name="tgUnit"> 타겟이 되는 객체 </param>
    /// <param name="isPlayerSkill"> 플레이어의 스킬인지 검사하는 변수</param>
    /// <param name="targetType">스킬의 적용 형태</param>
    /// <param name="isGroup"></param>
    /// <param name="many"> [5Star] 다수의 동일한 스파인 애니메이션을 위하여 키값을 나누기 위한 변수값 </param>
    internal void SetSkillSpine(BattleUnitData atkUnit, BattleUnitData tgUnit, bool isPlayerSkill, TargetType targetType, bool isGroup, int many)
    {
#if UNITY_EDITOR
        Debug.Log(atkUnit.Data.name + " 대상 객체 " + tgUnit.Data.name + "타겟 분류 : " + targetType + " 그룹?" + isGroup);

#endif

        SkillSpineDataMap skillSpineDataMap = null;
        int skillID = (isGroup) ? atkUnit.TeamSkillData.Data.skillID : atkUnit.SkillData.Data.skillID;
        int checkSkillID = many != -1 ? skillID * 10 + many : skillID;
        if (skillTable.ContainsKey(checkSkillID))
        {
            skillSpineDataMap = skillTable[checkSkillID];
        }
        else
        {
            GameCore.Instance.ResourceMgr.GetInstanceObject(ABType.AB_Prefab, skillID, (_obj) =>
            {
                if (_obj == null)
                {
                    Debug.LogError("Load Fail!!");
                    return;
                }

                skillSpineDataMap = new SkillSpineDataMap(_obj.GetComponent<SkeletonAnimation>());
                skillTable.Add(checkSkillID, skillSpineDataMap);
                _obj.transform.parent = this.transform;
            });
        }
        skillSpineDataMap.StartSkillSpine(atkUnit, tgUnit, isPlayerSkill, targetType);
    }

    /// <summary>
    /// 투사체 스파인 오브젝트를 테이블로부터 획득하고 종류에 맞게
    /// 연산 코루틴 실행
    /// </summary>
    /// <param name="atkUnit"> 시전 객체 </param>
    /// <param name="tgUnit"> 적용 대상 객체 </param>
    /// <param name="isPlayerSkill">플레이어가 사용한 스킬인지 확인</param>
    /// <param name="targetType"> 적용 방식</param>
    /// <param name="isGroup"></param>
    /// <param name="many"></param>
    /// <param name="_isCritical"> 치명타 여부</param>
    internal void SetAttackSpine(BattleUnitData atkUnit, BattleUnitData tgUnit, bool isPlayerSkill, TargetType targetType, bool isGroup, int many, bool _isCritical = false)
    {
#if UNITY_EDITOR
        Debug.Log( "Attack" + "\n" + atkUnit.Data.name + " 대상 객체 " + tgUnit.Data.name + "타겟 분류 : " + targetType + " 그룹?" + isGroup);

#endif

        SkillSpineDataMap skillSpineDataMap = null;
        int skillID = atkUnit.GetSkillID(_isCritical);
        int checkSkillID = many != -1 ? skillID * 10 + many : skillID;
        if (skillTable.ContainsKey(checkSkillID))
        {
            skillSpineDataMap = skillTable[checkSkillID];
        }
        else
        {
            GameCore.Instance.ResourceMgr.GetInstanceObject(ABType.AB_Prefab, skillID, (_obj) =>
            {
                if (_obj != null)
                {
                    skillSpineDataMap = new SkillSpineDataMap(_obj.GetComponent<SkeletonAnimation>());
                    skillTable.Add(checkSkillID, skillSpineDataMap);
                    _obj.transform.parent = this.transform;
                }
            });
        }
        if (skillSpineDataMap != null)
        {
            if (atkUnit.Data.atkType == AttackType.SpineFixed)
                skillSpineDataMap.StartSkillSpine(atkUnit, tgUnit, isPlayerSkill, targetType);
            else if (atkUnit.Data.atkType == AttackType.SpineMove)
                skillSpineDataMap.StartMoveSpine(atkUnit, tgUnit, isPlayerSkill, targetType);
        }
    }
}
