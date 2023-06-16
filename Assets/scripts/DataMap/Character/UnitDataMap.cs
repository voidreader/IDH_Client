using System;
using System.Collections.Generic;

internal enum UnitStat
{
	None,						// None
	Hp,             // 체력
	Attack,					// 공격력
	Armor,					// 방어력
	Vigor,					// 행동력
	Agility,				// 민첩성
	Concent,                // 집중력
    Recovery,       // 회복력
    Mental,         // 정신력 (소모량)
	Aggro,          // 어그로

	// Todo : 유닛 스탯 추가시 ( enum )

	Count,		// 스탯 개수 카운트용 반드시 마지막에 둘것
}


/// <summary>
/// 유닛 스탯 래퍼
/// </summary>
internal class Stat
{
	private float[] stats;
	private bool bLockWrite = false;

	internal Stat()
	{
		stats = new float[(int)UnitStat.Count];
	}

	internal Stat(float[] _stats, bool _lock = false)
	{
		bLockWrite = _lock;
		if( _stats.Length == (int)UnitStat.Count)
			stats = _stats;
	}

	internal Stat(Stat _src, bool _lock = false)
	{
		bLockWrite = _lock;
		stats = new float[(int)UnitStat.Count];
		for (int i = 0; i < stats.Length; i++)
			stats[i] = _src.GetStat((UnitStat)i);
	}

	internal float GetStat(UnitStat _stat)
	{
		return stats[(int)_stat];
	}

	internal void SetStat(UnitStat _stat, float _value)
	{
		if (!bLockWrite)
			stats[(int)_stat] = _value;
		else
			throw new Exception("해당 스탯은 락이 걸려있어 수정할 수 없습니다.");
	}

    internal Stat AddStat(Stat _stat)
    {
        for (int i = 0; i < stats.Length; i++)
            stats[i] += _stat.GetStat((UnitStat)i);

        return this;
    }

    internal Stat SubStat(Stat _stat)
    {
        for (int i = 0; i < stats.Length; i++)
            stats[i] -= _stat.GetStat((UnitStat)i);

        return this;
    }

    internal Stat MulStat(Stat _stat, float _scale = 0.01f)
    {
        for (int i = 0; i < stats.Length; i++)
            stats[i] *= _stat.GetStat((UnitStat)i) * _scale;

        return this;
    }

    internal float GetPower(int _type)
    {
        var powercal = GameCore.Instance.DataMgr.GetPowercalData(_type);

        var power = 0f;
        for (int i = 1; i < stats.Length; i++)
            power += stats[i] * powercal.PowercalStats[i-1];

        return power;// * 3f;
    }

    public static string GetStatName(UnitStat _stat)
    {
        switch(_stat)
        {
            case UnitStat.Hp:       return "체력";
            case UnitStat.Attack:   return "공격력";
            case UnitStat.Armor:    return "방어력";
            case UnitStat.Vigor:    return "행동력";
            case UnitStat.Agility:  return "민첩성";
            case UnitStat.Concent:  return "집중력";
            case UnitStat.Recovery: return "회복력";
            case UnitStat.Mental:   return "정신력";
            case UnitStat.Aggro:    return "어그로";
            default:                return "N/A";
        }
    }
}

enum AttackType
{
	None,
	Near = 1,	// 근접
    Gun = 2,	// 원거리 총
	Magic = 3,  // 원거리 마법
    Sword = 4,  // 근접 검
    SpineFixed = 5,
    SpineMove = 6,

    Count	// 카운트용
}

internal class UnitDataMap : CardDataMap
{
    internal int charId;                //동종
	internal int charIdType;			// 동종 구분.
	internal int evolLvl;               // 진화단계
	internal int discId;				// 캐릭터 설명 아이디
	internal int charType;				// 캐릭터 타입
	internal AttackType atkType;        // 공격 타입
	internal Stat stats;				// 스탯

	internal int skillId;				// 스킬 아이디
	internal int prefabId;				// 프리팹(리소스) 아이디

	internal int sellValue;				// 판매가격

	internal int belong;				// 소속	
    internal int exp;                   // 처치시 획득 경험치 (Not Use)

    internal int attackSpineID;         // 일반 공격 시 나오는 스파인
    internal int attackCriSpineID = -1; // 일반 공격(크리) 시 나오는 스파인
    //internal int[] weekType;            // 데미지 배율 적용 타입
    //internal float[] weekValue;         // 데미지 배율 (최종 데미지에 이 값을 곱함)

    //internal int gander;        // 1 남자, 2 여자

    internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out charId);
        ToParse(_csvData[idx++], out charIdType);
		 // 서버용 타입 데이터는 건너 뜀
		ToParse(_csvData[idx++], out rank);
		if (rank != 99) rank %= 10;
		ToParse(_csvData[idx++], out evolLvl);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out discId);
		ToParse(_csvData[idx++], out charType);
		ToParse(_csvData[idx++], out atkType);

		float[] st = new float[(int)UnitStat.Count];
		for (int i = 1; i < st.Length; i++)
			ToParse(_csvData[idx++], out st[i]);
		stats = new Stat(st, true);

		ToParse(_csvData[idx++], out skillId);
		ToParse(_csvData[idx++], out prefabId);

        idx += 6;

        ToParse(_csvData[idx++], out sellValue);

		ToParse(_csvData[idx++], out belong);
        ToParse(_csvData[idx++], out exp);
        ToParse(_csvData[idx++], out attackSpineID);

        if (0 < attackSpineID)
            attackCriSpineID = attackSpineID + 7000; // spineID + 29000;

        //weekType = new int[2];
        //weekValue = new float[2];
        //ToParse(_csvData[idx++], out weekType[0]);
        //ToParse(_csvData[idx++], out weekValue[0]);
        //ToParse(_csvData[idx++], out weekType[1]);
        //ToParse(_csvData[idx++], out weekValue[1]);

        imageID = id + 10000; //tmp;
		type = CardType.Character; // tmp
		return id;
	}

	internal int GetIllustSpeiteKey() { return prefabId + 1300000; } // charIdType
    internal int GetBigCardSpriteKey() { return prefabId + 1301000; } // charIdType + 1000
	internal int GetSmallCardSpriteKey() { return prefabId + 1302000; }
	internal int GetBattleCardSpriteKey() { return prefabId + 1303000; }
	internal int GetSmallProfileSpriteKey() { return prefabId + 1304000; }
	internal int GetBigProfileSpriteKey() { return prefabId + 1305000; }
	internal int GetSkillSpriteKey() { return prefabId + 1360000; }

	internal static int GetIllustSpeiteKey(int prefabId) { return prefabId + 1300000; }
	internal static int GetBigCardSpriteKey(int prefabId) { return prefabId + 1301000; }
	internal static int GetSmallCardSpriteKey(int prefabId) { return prefabId + 1302000; }
	internal static int GetBattleCardSpriteKey(int prefabId) { return prefabId + 1303000; }
	internal static int GetSmallProfileSpriteKey(int prefabId) { return prefabId + 1304000; }
	internal static int GetBigProfileSpriteKey(int prefabId) { return prefabId + 1305000; }
	internal static int GetSkillSpriteKey(int prefabId) { return prefabId + 1360000; }

	internal string GetBelongString()
	{
		switch(belong)
		{
			case 1: return "SPOON";
			case 2: return "KNIFE";
			case 3: return "정부기관";
			case 4: return "악마";
			case 5: return "가짜KNIFE";
			case 6: return "불로불사";
			case 7: return "혼혈";
			case 8: return "특기자";
			case 9: return "영물";

			default: return "ENEMY";
		}
	}
    internal void SetStats(ref float[] value, int _enchant = 0)
    {
        var strengthenData = GameCore.Instance.DataMgr.GetStrengthenData(1, charType);
        //int i = 0;
        value[1] += stats.GetStat(UnitStat.Hp) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Hp), _enchant);
        value[2] += stats.GetStat(UnitStat.Attack) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Attack), _enchant);
        value[3] += stats.GetStat(UnitStat.Armor) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Armor), _enchant);
        value[4] += stats.GetStat(UnitStat.Vigor) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Vigor), _enchant);
        value[5] += stats.GetStat(UnitStat.Agility) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Agility), _enchant);
        value[6] += stats.GetStat(UnitStat.Concent) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Concent), _enchant);
        value[7] += stats.GetStat(UnitStat.Recovery) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Recovery), _enchant);
        value[8] += stats.GetStat(UnitStat.Mental) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Mental), _enchant);
        value[9] += stats.GetStat(UnitStat.Aggro) * (float)Math.Pow(strengthenData.stats.GetStat(UnitStat.Aggro), _enchant);
    }

    internal void SetStatsPowercal(ref float[] value)
    {
        PowercalDataMap powercalDataMap = GameCore.Instance.DataMgr.GetPowercalData(charType);
        float[] multiplyValue = powercalDataMap.PowercalStats;
        //int i = 0;
        value[1] = value[1] * multiplyValue[0];
        value[2] = value[2] * multiplyValue[1];
        value[3] = value[3] * multiplyValue[2];
        value[4] = value[4] * multiplyValue[3];
        value[5] = value[5] * multiplyValue[4];
        value[6] = value[6] * multiplyValue[5];
        value[7] = value[7] * multiplyValue[6];
        value[8] = value[8] * multiplyValue[7];
        value[9] = value[9] * multiplyValue[8];
    }


    internal int GetDefPower(int _enchant = 0)
    {
        float result = 0;
        float[] statValue = new float[(int)UnitStat.Count];
        SetStats(ref statValue, _enchant);
        SetStatsPowercal(ref statValue);
        for (int i = 0; i < statValue.Length; i ++)
            result += statValue[i];

        return (int)result;
    }


    internal Stat GetStat(int _enchant = 0)
    {
        var strengthenData = GameCore.Instance.DataMgr.GetStrengthenData(1, charType);

        Stat stat = new Stat(stats);
        int _enchantCount = (_enchant <= 0) ? 0 : _enchant;
        for (UnitStat i = UnitStat.None + 1; i < UnitStat.Count; ++i)
            for (int n = 0; n < _enchantCount; n++)
            {
                stat.SetStat(i, stat.GetStat(i) * strengthenData.stats.GetStat(i));
            }
                

        return stat;
    }

    internal string GetCharTypeString()
    {
        return GetStrType(charType);
    }

    internal bool IsExpCard()
    {
        return atkType == AttackType.None;
    }
}
