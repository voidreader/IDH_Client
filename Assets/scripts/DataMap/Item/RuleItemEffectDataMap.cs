using System;
using System.Collections.Generic;


/// <summary>
/// 아이템 옵션 리스트
/// </summary>
enum ItemEffectType
{
	None,
	MoreReward,
	AutoHeal,
	MoreAttack, // 기본 공격력 증가
	MoreCritical,
	MoreHP,
	MoreAggro,
	MoreArmor,
	DecCoolTime,	// 쿨감

	HP,
	Attack,
	Armor,          // 방어력
	Vigor,          // 행동력
	Agility,        // 민첩성
	Concent,        // 집중력
	Recovery,       // 회복력
	Mental,         // 정신력 (소모량)
	Aggro,          // 어그로

	Count						// Never Use
}

class RuleItemEffectDataMap : CSVParse
{
	internal int group;
	internal string name;
	internal string disc;
	internal float ratePick;
	internal ItemEffectType effectType;
	internal int type;            // 버프타입 배율값 : 0, 절대값 : 1

	// Value 관련은 클라에선 사용치 않음

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;

		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out group);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out disc);
		ToParse(_csvData[idx++], out ratePick);
		ToParse(_csvData[idx++], out effectType);
		ToParse(_csvData[idx++], out type);


		return id;
	}
}
