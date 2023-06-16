using System;
using System.Collections.Generic;


enum EffectType
{
	None,

	HPAttack,     // 체력 공격
	MTAttack,     // 정신력 공격

	HPHeal,				// 체력 회복
	MTHeal,				// 정신력 회복

	Buff,					// 버프 & 디버프 ( value1가 해당 버프 아이디 )

	SpecialBuff = 10,          // Never Used // 특수 버프 구분용

	FollowAtk,        // 일반 공격시				// 따라서 공격한다.
	SetGuard,					// 타겟 결정시				// 타겟을 특정 캐릭으로 바꾸고, 해당 캐릭터를 이 캐릭터의 앞으로 이동시킨다.
	CounterAtk,				// 특정 아군 피격시 가격한 대상 공격
	AggroCtrl,        // 어그로 계산시			// 특정대상이 공격할때만 어그로 스탯을 조작
	ProportionDmg,    // 데미지 계산시			// 피격 유닛 체력 비례데미지

	RandStateAtk = 50,     // 일반공격시 확률 상태이상

	RSA_Paralyze,     // 마비
	RSA_Sleep,        // 수면
	RSA_Stun,         // 스턴
	RSA_HPPoison,     // 체력 독
	RSA_MTPoison,     // 멘탈 독

	StateBuff = 100,		// NeverUsed // 상태이상버프 구분용

	paralyze,     // 마비
	Sleep,        // 수면
	Stun,         // 스턴
	HPPoison,     // 체력 독
	MTPoison,     // 멘탈 독
    RandomStateBuff,    //랜덤으로 나오는 디버프
    Purification,       //정화

	Count,                  // Never Used // 카운트용

    spineAnimationEnemy = 95, // 스파인 애니메이션 중 적팀 전체에 애니메이션 적용
    spineAnimationTeam = 96, // 스파인 애니메이션 중 팀원 전체에 애니메이션 적용
    AirBorn = 97,            // 스파인 애니메이션 중 들어올리는 상태
    spineAnimation = 98,    // 스파인 애니메이션 단일 대상 스킬용
    spineAnimationWide = 99,// 스파인 애니메이션 단체 대상 스킬용
}
[System.Flags]
public enum TargetType
{
    None = 0x00,
    SingleEnemy = 0x01,    // 단일 적
    AllEnemy = 0x02,       // 적 전체
    Self = 0x04,           // 시전자 자신
    TeamAll = 0x08,        // 팀 전체
    All = 0x10,            // 전체
    TeamAllCenter = 0x20,
    AllEnemyEach = 0x40,    //적 전체(단일)

    Count
}

internal struct EffectPart
{
	internal int type;            // 버프타입 배율값 : 0, 절대값 : 1
	internal UnitStat tgStat;     // 버프가 영향을 주는 것
	internal float value1;        // 각 타입마다 임의로 사용되는 값
	internal float value2;        // 각 타입마다 임의로 사용되는 값
}

class EffectDataMap : CSVParse
{
	internal TargetType			targetType;		// 타겟 ( 0 : 단일 적, 1: 단일자신, 2: 적전체, 3:팀전체, 4 전체 )
	internal EffectType			type;			// 효과 타입
	internal EffectPart			buffData;		// 효과 데이터값  // 갑의 타입( 0 : 배율, 1: 절대값 )
	internal int				count;			// 지속시간 ( -1일때 무한 )

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
        int targetTypeInt = 0;
        ToParse(_csvData[idx++], out targetTypeInt);
        switch (targetTypeInt)
        {
            case 0: targetType = TargetType.SingleEnemy; break;
            case 1: targetType = TargetType.AllEnemy; break;
            case 2: targetType = TargetType.Self; break;
            case 3: targetType = TargetType.TeamAll; break;
            case 4: targetType = TargetType.All; break;
            case 5: targetType = TargetType.TeamAllCenter;break;
            case 6: targetType = TargetType.AllEnemyEach;break;
            default: targetType = TargetType.None; break;
        }
        //ToParse(_csvData[idx++], out targetType);
        ToParse(_csvData[idx++], out type);
		ToParse(_csvData[idx++], out buffData.type);
		ToParse(_csvData[idx++], out buffData.tgStat);
		ToParse(_csvData[idx++], out buffData.value1);
		ToParse(_csvData[idx++], out buffData.value2);
		ToParse(_csvData[idx++], out count);

		return id;
	}

}
