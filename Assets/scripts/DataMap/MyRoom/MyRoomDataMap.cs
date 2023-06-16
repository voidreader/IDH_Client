using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class MyRoomDataMap : CSVParse
{
	internal string name;
	internal int defOpen; // 0 : 기본 열림. 1:기본 잠김
	internal int openType; // 열기위해 필요한 재화의 종류. 0 : None, 1 : 골드, 2: 캐시
	internal int openValue; // 열기위해 필요한 재화의 수.

	internal int[] satisfactionCost;
	internal MyRoomEffectType[] satisfactionEffectID;
	internal int[] satisfactionEffectValue; // 백분율

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out defOpen);
		ToParse(_csvData[idx++], out openType);
		ToParse(_csvData[idx++], out openValue);

		var count = 4;
		satisfactionCost = new int[count];
		satisfactionEffectID = new MyRoomEffectType[count];
		satisfactionEffectValue = new int[count];
		for (int i = 0; i < count; i++)
		{
			ToParse(_csvData[idx++], out satisfactionCost[i]);
			ToParse(_csvData[idx++], out satisfactionEffectID[i]);
			ToParse(_csvData[idx++], out satisfactionEffectValue[i]);
		}

		return id;
	}

    public static string GetStrMyRoomEffect(MyRoomEffectType _type, float _value)
    {
        switch (_type)
        {
            case MyRoomEffectType.MoreReward: return string.Format("스테이지 클리어보상 +{0:0.##}%", _value);
            case MyRoomEffectType.AutoHeal: return string.Format("자가 치유 +{0:0.##}%", _value);
            case MyRoomEffectType.MoreAttack: return string.Format("기본 공격력 증가 +{0:0.##}%", _value);
            case MyRoomEffectType.MoreCritical: return string.Format("크리티컬 확률 증가 +{0:0.##}%", _value);
            case MyRoomEffectType.MoreHP: return string.Format("기본 체력 증가 +{0:0.##}%", _value);
            case MyRoomEffectType.MoreConcent: return string.Format("집중력 수치 증가 +{0:0.##}%", _value);
            case MyRoomEffectType.MoreArmor: return string.Format("기본 방어력 증가 +{0:0.##}%", _value);
            case MyRoomEffectType.DecCoolTime: return string.Format("스킬 쿨타임 감소 -{0:0.##}%", _value);
            case MyRoomEffectType.Count:
            default: return "ERROR";
        }
    }
}
