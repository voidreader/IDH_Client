using System;
using System.Collections.Generic;

public class PvPSData : JsonParse
{
	internal long userUID;					    // 유저 UID
	internal int grade;							// PvP 등급
	internal int group;							// PvP 등급안의 그룹임
	internal int power;							// 전투력
	internal int point;							// PvP 점수
	internal int win;							// 승리 수
	internal int defeat;						// 패배 수
	internal int consecutive;				    // 연승 수
	internal int enemyUID;					    // 마지막 매칭 유저 UID
	internal int rediscover;				    // 재탐색 횟수(자기꺼)
	internal bool placement;				    // 배치고사 여부
	internal int rechallengeCount;              // 재도전 횟수
	internal string userName;                   // 유저 이름
	internal int userLevel;                     // 유저 레벨
	internal int typicalKey;				    // 대표 캐릭터 키값

	internal override bool SetData(JSONObject _json)
	{
        if (_json == null)
            return false; // 아직 pvp 데이터가 생성되지 않음.

        ToParse(_json, "USER_UID", out userUID);
		ToParse(_json, "GRADE", out grade);
		ToParse(_json, "GROUP", out group);
		ToParse(_json, "POWER", out power);
		ToParse(_json, "POINT", out point);
		ToParse(_json, "WIN", out win);
		ToParse(_json, "DEFEAT", out defeat);
		ToParse(_json, "CONSECUTIVE", out consecutive);
		ToParse(_json, "ENEMY_UID", out enemyUID);
		ToParse(_json, "REDISCOVER", out rediscover);
		ToParse(_json, "PLACEMENT", out placement);
		ToParse(_json, "RECHALLENGE_COUNT", out rechallengeCount);
		ToParse(_json, "USER_NAME", out userName);
		ToParse(_json, "USER_LEVEL", out userLevel);
		ToParse(_json, "CHA_ID", out typicalKey);

		if (placement && rechallengeCount == 10)
			grade = 7000009;

		return true;
	}




}
