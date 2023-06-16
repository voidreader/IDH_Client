using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

internal enum ParticleType
{
    //Soft_NormalNear,
    //Soft_NormalFar,
    //Soft_SkillNear,
    //Soft_SkillFar,

    //Hard_NormalNear,
    //Hard_NormalFar,
    //Hard_SkillNear,
    //Hard_SkillFar,

    //Charge,

    //EXPLOSION_TEST,

    Death = 30001,
    Charge = 30002,
    MagicSkill = 30003,
    SwordSkill = 300017,
    NearSkill = 30004,
    GunSkill = 30005,
    Heal = 30006,
    MaxGuard = 30007,
    Buff = 30008,
    Debuff = 30009,
    DamageDown = 30010,
    Concentration = 30011,
    Purification = 30012,
    Sleep = 30013,
    Paralysis = 30014,
    Stun = 30015,
    Poison = 30016,
	Count			// Never Used
}

class BattleParticleMgr : MonoBehaviour
{
	public GameObject[] particles; // 파티클 종류 // ParticleType과 순서가 일치해야한다.
    public class EffectSpineDataMap
    {
        float durationTime;
        float presentTime;
        bool isWork;
        SkeletonAnimation skeletonAnimation;
        SkeletonData skelData;
        ParticleType effectType;
        int particlePos;    //0이면 바닥위치, 1이면 중앙위치


        public EffectSpineDataMap(SkeletonAnimation _skeletonAnimation, ParticleType _type)
        {
            skeletonAnimation = _skeletonAnimation;
            presentTime = 0f;
            skelData = skeletonAnimation.SkeletonDataAsset.GetSkeletonData(false);
            isWork = false;
            effectType = _type;
            particlePos = IsCenterPosition();
        }


        internal EffectSpineDataMap CheckEffectSpine(ParticleType _type)
        {
            if (isWork == true)
                return null;
            return effectType == _type ? this : null;
        }


        internal void StartEffectSpine(BattleUnitData tgUnit, bool isPlayerEffect, string animationName)
        {
            presentTime = 0f;
            isWork = true;
            //bool isBig = tgUnit.Transform.localScale.x > 1;
            skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = tgUnit.SpineCtrl.GetSortingOrder() + 1;// (isBig || particlePos >= 1 ? 1 : 0);
            skeletonAnimation.transform.rotation = Quaternion.Euler((isPlayerEffect) ? 31f : -31f, (isPlayerEffect) ? 0f : 180f, 0f);
            Transform parent = skeletonAnimation.transform.parent;
            skeletonAnimation.transform.parent = tgUnit.Transform;
            if (effectType == ParticleType.Death && tgUnit.Data.charIdType == 1300021)
            {
                if (isPlayerEffect) skeletonAnimation.transform.localPosition = new Vector3(tgUnit.Center.y, 0f, 0f);
                else                skeletonAnimation.transform.localPosition = new Vector3(-tgUnit.Center.y, 0f, 0f);
            }
            else
            {
                skeletonAnimation.transform.localPosition = (particlePos == 0 ? Vector3.zero : particlePos == 1 ? tgUnit.Center : Vector3.up * tgUnit.Center.y * 2);
            }
            //skeletonAnimation.transform.position += Vector3.back * 0.2f;
            skeletonAnimation.transform.parent = parent;

            skeletonAnimation.AnimationState.ClearTrack(0);
            var animation = skelData.FindAnimation(animationName);
            durationTime = animation.duration;
            skeletonAnimation.AnimationState.SetAnimation(0, animation, false);
            skeletonAnimation.gameObject.SetActive(true);
        }
        private int IsCenterPosition()
        {
            switch(effectType)
            {
                case ParticleType.Charge:
                case ParticleType.Death:
                case ParticleType.Heal:
                case ParticleType.Paralysis:
                case ParticleType.Poison:
                case ParticleType.Sleep:
                case ParticleType.Purification:
                    return 0;
                default:
                    return 1;

                case ParticleType.Stun:
                    return 2;
            }
        }
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
    public List<EffectSpineDataMap> effectSpineList = new List<EffectSpineDataMap>();
    //private ObjectPool<Transform>[] ptcPools;

    private void Update()
    {
        for(int i = 0; i < effectSpineList.Count; i++)
        {
            effectSpineList[i].CheckSpineDuration();
        }
    }

	public void ShowParticle(ParticleType _type, BattleUnitData _tgUnit,string _animationName)
	{
        for (int i = 0; i < effectSpineList.Count; i++)
        {
            if (effectSpineList[i].CheckEffectSpine(_type) != null)
            {
                effectSpineList[i].StartEffectSpine(_tgUnit, _tgUnit.IsPlayerTeam, _animationName);
                return;
            }
        }
        GameCore.Instance.ResourceMgr.GetInstanceObject(ABType.AB_Prefab, (int)_type, (_obj) =>
        {
            if (_obj == null)
            {
                Debug.LogError("Load Fail!!");
                return;
            }

            EffectSpineDataMap nEffectSpineDataMap = new EffectSpineDataMap(_obj.GetComponent<SkeletonAnimation>(), _type);
            effectSpineList.Add(nEffectSpineDataMap);
            _obj.transform.parent = this.transform;
            nEffectSpineDataMap.StartEffectSpine(_tgUnit, _tgUnit.IsPlayerTeam, _animationName);
        });
    }

	public void SetTimeScale(float _scale)
	{
		//for (int i = 0; i < ptcPools.Length; i++)
		//{
		//	var ptcs = particles[i].GetComponentsInChildren<ParticleSystem>();
		//	for (int j = 0; j < ptcs.Length; j++)
		//	{
		//		var main = ptcs[j].main;
		//		main.simulationSpeed = _scale;
		//	}
		//}
	}
}
