using System;
using System.Collections.Generic;

internal class BattleCommonDataMap : JsonParse
{
	internal float DmgAtkCrt_C;	// 데미지 계산시 캐릭터 공격력 보정값
	internal float DmgCctCrt_C;  // 데미지 계산시 캐릭터 집중력 보정값
	internal float DmgLvlCrt_C;  // 데미지 계산시 캐릭터 레벨 보정값

	internal float DmgAtkCrt_I;  // 데미지 계산시 아이템 공격력 보정값
	internal float DmgCctCrt_I;  // 데미지 계산시 아이템 집중력 보정값
	internal float DmgLvlCrt_I;  // 데미지 계산시 아이템 레벨 보정값

	internal float DmgCrt_C;     // 데미지 계산시 캐릭터 최종 보정값
	internal float DmgCrt_I;     // 데미지 계산시 아이템 최종 보정값

	internal float DmgCriCctcrt;	// 크리확률 계산시 집중력 보정값
	internal float DmgCriAglcrt;  // 크리확률 계산시 민첩성 보정값
	internal float DmgCriConstMin; // 크리확률 계산시 최소값
	internal float DmgCriConstMax; // 크리확률 계산시 최대값

	internal float DmgCriCriScale;  // 크리티컬 공격시 적용 배율
	internal float DmgCriNorScale;  // 기본 공격시 적용 배율
	internal float DmgCriFilScale;  // 실수 공격시 적용 배율

	internal float GrdAmrCrt_C;  // 방어 계산시 캐릭터 방어력 보정값
	internal float GrdAglCrt_C;  // 방어 계산시 캐릭터 민첩성 보정값
	internal float GrdLvlCrt_C;  // 방어 계산시 캐릭터 레벨 보정값

	internal float GrdAtkCrt_I;  // 방어 계산시 아이템 공격력 보정값
	internal float GrdAglCrt_I;  // 방어 계산시 아이템 민첩성 보정값
	internal float GrdLvlCrt_I;  // 방어 계산시 아이템 레벨 보정값

	internal float GrdCrt_C;     // 방어 계산시 캐릭터 최종 보정값
	internal float GrdCrt_I;     // 방어 계산시 아이템 최종 보정값

	internal float GrdCriCctcrt; // 방어 크리확률 계산시 집중력 보정값
	internal float GrdCriAglcrt;  // 방어 크리확률 계산시 민첩성 보정값
	internal float GrdCriConstMin; // 방어 크리확률 계산시 최소값
	internal float GrdCriConstMax; // 방어 크리확률 계산시 최대값

	internal float GrdCriCriScale;  //  크리티컬 방어시 적용 배율
	internal float GrdCriNorScale;  // 기본 방어시 적용 배율
	internal float GrdCriFilScale;  // 실수 방어시 적용 배율

	internal float TeamDmgGaugeConst;	// 팀스킬 공격시 게이지 상수
	internal float TeamGrdGaugeConst;  // 팀스킬 피격시 게이지 상수

	internal float TeamDmgChemiConst;  // 팀스킬 공격시 케미 게이지 상수
	internal float TeamGrdChemiConst;  // 팀스킬 피격시 케미 게이지 상수

	internal float TeamDmgIncConst_1;	// 팀스킬 게이지 1칸일때 증가 상수
	internal float TeamDmgIncConst_2;  // 팀스킬 게이지 2칸일때 증가 상수

	internal float StlikeDmgConst;			// 스트라이크 공격시 게이지 상수
	internal float StlikeGrdConst;			// 스트라이크 피격시 게이지 상수

    internal float MetalDecrease;       // 스킬 사용시 기본 멘탈 수치 하락량
    internal float MaxCoolTime;         // 스킬 후쿨 최대량
    internal float CoolTimeIncrease;    // 후속 쿨타임 증가량
    internal float MentalHeal;          // 라운드 종료 후 각 캐릭터 멘탈 수치 회복량

    internal float VigorForVigorConst;          // 행동력 계산용 행동력 보정수치
    internal float AgilityForVigorConst;        // 행동력 계산용 민첩성 보정수치
    internal float VigorDefaultDecreaseConst;   // 행동력 기본 감소량
    internal float VigorLimitDecreaseConst;     // 행동력 계산시 스탯 보정치 최대량

    internal float PriorityAggro_1;     // 어그로 수치 1위 공격받을 확률
    internal float PriorityAggro_2;     // 어그로 수치 2위 공격받을 확률
    internal float PriorityAggro_3;     // 어그로 수치 3위 공격받을 확률

    internal float BossHPScale;     // 임시용 보스 체력 증가량

    internal override bool SetData(JSONObject _json)
	{
        //int idx = 0;
        ToParse(_json, "DmgAtkCrt_C", out DmgAtkCrt_C);
        ToParse(_json, "DmgCctCrt_C", out DmgCctCrt_C);
        ToParse(_json, "DmgLvlCrt_C", out DmgLvlCrt_C);
        ToParse(_json, "DmgAtkCrt_I", out DmgAtkCrt_I);
        ToParse(_json, "DmgCctCrt_I", out DmgCctCrt_I);
        ToParse(_json, "DmgLvlCrt_I", out DmgLvlCrt_I);
        ToParse(_json, "DmgCrt_C", out DmgCrt_C);
        ToParse(_json, "DmgCrt_I", out DmgCrt_I);
        ToParse(_json, "DmgCriCctcrt", out DmgCriCctcrt);
        ToParse(_json, "DmgCriAglcrt", out DmgCriAglcrt);
        ToParse(_json, "DmgCriConstMin", out DmgCriConstMin);
        ToParse(_json, "DmgCriConstMax", out DmgCriConstMax);
        ToParse(_json, "DmgCriCriScale", out DmgCriCriScale);
        ToParse(_json, "DmgCriNorScale", out DmgCriNorScale);
        ToParse(_json, "DmgCriFilScale", out DmgCriFilScale);
        ToParse(_json, "GrdAmrCrt_C", out GrdAmrCrt_C);
        ToParse(_json, "GrdAglCrt_C", out GrdAglCrt_C);
        ToParse(_json, "GrdLvlCrt_C", out GrdLvlCrt_C);
        ToParse(_json, "GrdAtkCrt_I", out GrdAtkCrt_I);
        ToParse(_json, "GrdAglCrt_I", out GrdAglCrt_I);
        ToParse(_json, "GrdLvlCrt_I", out GrdLvlCrt_I);
        ToParse(_json, "GrdCrt_C", out GrdCrt_C);
        ToParse(_json, "GrdCrt_I", out GrdCrt_I);
        ToParse(_json, "GrdCriCctcrt", out GrdCriCctcrt);
        ToParse(_json, "GrdCriAglcrt", out GrdCriAglcrt);
        ToParse(_json, "GrdCriConstMin", out GrdCriConstMin);
        ToParse(_json, "GrdCriConstMax", out GrdCriConstMax);
        ToParse(_json, "GrdCriCriScale", out GrdCriCriScale);
        ToParse(_json, "GrdCriNorScale", out GrdCriNorScale);
        ToParse(_json, "GrdCriFilScale", out GrdCriFilScale);
        ToParse(_json, "TeamDmgGaugeConst", out TeamDmgGaugeConst);
        ToParse(_json, "TeamGrdGaugeConst", out TeamGrdGaugeConst);
        ToParse(_json, "TeamDmgChemiConst", out TeamDmgChemiConst);
        ToParse(_json, "TeamGrdChemiConst", out TeamGrdChemiConst);
        ToParse(_json, "TeamDmgIncConst_1", out TeamDmgIncConst_1);
        ToParse(_json, "TeamDmgIncConst_2", out TeamDmgIncConst_2);
        ToParse(_json, "StlikeDmgConst", out StlikeDmgConst);
        ToParse(_json, "StlikeGrdConst", out StlikeGrdConst);
        ToParse(_json, "MetalDecrease", out MetalDecrease);
        ToParse(_json, "MaxCoolTime", out MaxCoolTime);
        ToParse(_json, "CoolTimeIncrease", out CoolTimeIncrease);
        ToParse(_json, "MentalHeal", out MentalHeal);
        ToParse(_json, "VigorForVigorConst", out VigorForVigorConst);
        ToParse(_json, "AgilityForVigorConst", out AgilityForVigorConst);
        ToParse(_json, "VigorDefaultDecreaseConst", out VigorDefaultDecreaseConst);
        ToParse(_json, "VigorLimitDecreaseConst", out VigorLimitDecreaseConst);
        ToParse(_json, "PriorityAggro_1", out PriorityAggro_1);
        ToParse(_json, "PriorityAggro_2", out PriorityAggro_2);
        ToParse(_json, "PriorityAggro_3", out PriorityAggro_3);

        ToParse(_json, "BossHPScale", out BossHPScale);

        return true;
	}
}
