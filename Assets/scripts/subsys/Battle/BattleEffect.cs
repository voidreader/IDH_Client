using System;
using System.Collections.Generic;

enum BattleEffectType
{	
	None,					//												관리 테이블		지속턴	버프파츠
	NoramlAttack,	// 일반 데미지 공격				-							-				사용
	NormalChemi,	// 일반 캐미 버프					chemis				무한		사용
	SpecialChemi,	// 특수 캐피 버프					chemis				무한		일부사용
	RSA_Chemi,		// 확률 상이태상 버프			chemis				무한		사용
	Buff,					// 일반 버프							buffs					사용		사용
	StateBuff,		// 일반 상태이상 버프			states				사용		일부사용
}

class BattleEffect
{
	internal EffectDataMap effect;
	internal BattleEffectType type;

	internal int continuance;   //남은 턴수 

	internal bool Applied { get; set; }

	internal BattleEffect(EffectDataMap _effect)
	{
		effect = _effect;
		continuance = _effect.count;
		Applied = false;
		type = (_effect.type == EffectType.Buff) ? ((_effect.count == -1) ? BattleEffectType.NormalChemi : BattleEffectType.Buff) :
						(_effect.type < EffectType.SpecialBuff) ? BattleEffectType.NoramlAttack :
						(_effect.type < EffectType.RandStateAtk) ? BattleEffectType.SpecialChemi :
						(_effect.type < EffectType.StateBuff) ? BattleEffectType.RSA_Chemi :
						(_effect.type < EffectType.Count) ? BattleEffectType.StateBuff : BattleEffectType.None;
	}


	/// <summary>
	/// 상태이상공격 만들때 사용
	/// </summary>
	/// <param name="_type"></param>
	/// <param name="_value"></param>
	/// <param name="_count"></param>
	internal BattleEffect(EffectType _type, float _value, int _count)
	{
		effect = new EffectDataMap()
		{
			id = -10,
			type = _type,
			count = _count
		};
		effect.buffData.value1 = _value;

		continuance = effect.count;
		Applied = false;
		type = BattleEffectType.StateBuff;
	}


	/// <summary>
	/// 지속턴수를 하나 감소한다.
	/// </summary>
	/// <returns>0 이상이라면 true. 0이면 false</returns>
	internal bool ReduceContinuance()
	{
		return (continuance > 0 ? --continuance : ++continuance) != 0;
	}

	internal int GetContineuance()
	{
		return continuance;
	}

	internal void SetContinuance(int _num)
	{
		continuance = _num;
	}
}
