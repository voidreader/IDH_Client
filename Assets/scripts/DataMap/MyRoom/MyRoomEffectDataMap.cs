using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MyRoomEffectType
{
    None,               //                      클라처리
	MoreReward,			// 스테이지 보상 증가
	AutoHeal,			// 매턴 자동 치유          0
	MoreAttack,			// 공격력 증가
	MoreCritical,		// 크리티컬 확률 증가
	MoreHP,				// 최대체력 증가
	MoreConcent,			// 어그로 증가
	MoreArmor,			// 방어력 증가
	DecCoolTime,		// 후딜 감소               0
	Count, // Never Use
}

internal class MyRoomEffectDataMap : CSVParse
{
	internal string name;
	internal MyRoomEffectType effectType;		
	internal int applyType;  // 0 : 절대값. 1 : 배율값(백분율)
	internal float value;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out effectType);
		ToParse(_csvData[idx++], out applyType);
		ToParse(_csvData[idx++], out value);

		return id;
	}
}
