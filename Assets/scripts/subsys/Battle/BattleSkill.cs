using System;
using System.Collections.Generic;

class BattleSkill
{
	SkillDataMap data;

	internal SkillDataMap Data { get { return data; } }
	internal bool bSetTarget { get; set; } // 타겟 지정형인지 아닌지 여부
    internal int maxSkillCount { get; private set; }
    internal int maxEndCount { get; private set; }
    internal int hitCount { get; private set; }
    internal int endCount { get; private set; }

    internal bool finishNow {  get { return maxEndCount == endCount; } }

    internal BattleSkill(SkillDataMap _data)
	{
		data = _data;
		hitCount = 0;
        endCount = 0;
        bSetTarget = false;
		for (int i = 0; i < data.effects.Length; i++)
		{
            maxSkillCount = Math.Max(maxSkillCount, data.effectCount[i]);

			if (data.effects[i] == -1)
				continue;

			var effect = GameCore.Instance.DataMgr.GetEffectData(data.effects[i]);
			bSetTarget |= effect.targetType == 0;
		}
        ++maxSkillCount;
        //maxEndCount = _data.endCount - 1;
        maxEndCount = _data.endCount;
    }
	

	internal void HitCountReset()
	{
        endCount = 0;
        hitCount = 0;
	}

	internal void IncHitCount()
	{
        ++endCount;
		++hitCount;
        if (hitCount >= maxSkillCount)
            hitCount = 0;
	}

	internal int GetHitCount()
	{
		return hitCount;
	}

	/// <summary>
	/// 현재 발동할 수 있는 이펙트 반환.
	/// 후 HitCount 증가
	/// </summary>
	/// <param name="_num"></param>
	/// <param name="_effects">최대 길이는 테이블 크기와 동일한 5개이다.</param>
	/// <returns>반환 effect 개수</returns>
	internal int GetNowEffects( ref EffectDataMap[] _effects , bool isCount)
	{
        if(endCount >= maxEndCount && maxEndCount != -1)
        {
            endCount++;
            return 999;
        }
		int accCnt = 0;
		int idx = 0;
		for (int i = 0; i < data.effects.Length; i++)
		{
			if (data.effectCount[i] == -1)
				break;

            if (idx != 0 && accCnt < data.effectCount[i])
                break;

			accCnt = data.effectCount[i];
            if (accCnt >= hitCount)
            {
                var effect = GameCore.Instance.DataMgr.GetEffectData(data.effects[i]);
                if (Data.isSpineLoop == 0 && CheckFirstRoop(effect) == false) { }
                else _effects[idx++] = effect;
            }
		}

		for(int i = idx; i < 5; ++i)
			_effects[i] = null;
		
		if(isCount == true) IncHitCount();
		return idx;
	}
    private bool CheckFirstRoop(EffectDataMap effect)
    {
        switch(effect.type)
        {
            case EffectType.spineAnimation:
            case EffectType.spineAnimationEnemy:
            case EffectType.spineAnimationTeam:
            case EffectType.spineAnimationWide:
                return (endCount <= hitCount);
            default:break;
        }
        return true;
    }

	internal int PeekNowEffects(ref EffectDataMap[] _effects)
	{
		int accCnt = 0;
		int idx = 0;
		for (int i = 0; i < data.effects.Length; i++)
		{
			if (data.effectCount[i] == -1)
				break;

			accCnt += data.effectCount[i];
			if (accCnt < hitCount)
				continue;

			var effect = GameCore.Instance.DataMgr.GetEffectData(data.effects[i]);
			_effects[idx++] = effect;

			if (accCnt > hitCount)
				break;
		}

		for (int i = idx; i < 5; ++i)
			_effects[i] = null;

		return idx;
	}
}
