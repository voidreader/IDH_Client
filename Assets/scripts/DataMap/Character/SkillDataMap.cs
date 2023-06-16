using System;
using System.Collections.Generic;

enum PositionType
{
    None = 0,
    NearJump = 1,
    Distance = 2,
    NearStraight = 3,
    WideArea = 4,
    EnemyFront = 5,
    EnemyCenter = 6,
    TeamFront = 7,
    TeamBack = 8
}
class SkillDataMap : CSVParse
{
	internal string name;		// 스킬 명
	internal PositionType type;	// 스킬 타입 ( 이것으로 행동 분기)
	internal string disc;       // 스킬 설명
	internal int[] effectCount; // 스킬 사용 횟수(다단 공격일때 사용. 다단이 아닐때 0 또는 -1)
	internal int[] effects;     // 스킬 효과 아이디들(최대 5개)
	internal float mtUsage;		// 멘탈 소모량
	internal float firstCool;	// 선쿨
	internal float baseCool;	// 후쿨 기본값
	internal float accCool;		// 쿨 증가값
	internal float maxCool;     // 쿨 최대값
    internal int endCount;      //스킬 횟수 카운트
    internal int skillID;       //스킬에 관련된 스파인 애니메이션 키값
    internal int soundPath;     //임시용 사운드 경로 저장
    internal int isSpineLoop;   //스파인 애니메이션 반복인지 아닌지
    internal float frameValue;  //직선 이동을 위한 이동 프레임 비율(0~1)
	//internal int icon;		// 아이콘 아이디


	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out name);
        int _type;
		ToParse(_csvData[idx++], out _type);
        type = (PositionType)_type;

        ToParse(_csvData[idx++], out disc);

		effectCount = new int[5];
		effects = new int[5];
		for (int i = 0; i < effects.Length; ++i)
		{
			ToParse(_csvData[idx++], out effectCount[i]);
			ToParse(_csvData[idx++], out effects[i]);
		}
        ToParse(_csvData[idx++], out firstCool);
		ToParse(_csvData[idx++], out baseCool);
		ToParse(_csvData[idx++], out accCool);
		ToParse(_csvData[idx++], out maxCool);

        ToParse(_csvData[idx++], out endCount);
        ToParse(_csvData[idx++], out skillID);
        ToParse(_csvData[idx++], out soundPath);
        ToParse(_csvData[idx++], out isSpineLoop);
        ToParse(_csvData[idx++], out frameValue);
        //ToParse(_csvData[idx++], out icon);

        return id;
	}
}
