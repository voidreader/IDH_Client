using System;
using System.Collections.Generic;
using UnityEngine;

internal class BattleUnitPool : IComparer<BattleUnitData>
{
    private Dictionary<int, BattleUnitData> friendUnits;  // 친구 유닛들
    private Dictionary<int, BattleUnitData> units;  // 현 스테이지에 등장하는 모든 유닛들
	private List<int> InCombatUnits;                // 전투중인 유닛들
    private BattlePara battlePara;
    
    private void AddFriendUnit(int _playerTeemIdx)
    {
        List<PvPOppUnitSData> listFriend = battlePara.friendUnitList;
        int _friendSkill = battlePara.friendTeamSkill;
        if (_friendSkill != 0)
        {
            for (int i = 0; i < listFriend.Count; ++i)
            {
                if (listFriend[i].charID > 0)
                {
                    var sdata = new HeroSData(listFriend[i].charID);
                    //var sdata = new HeroSData(1000044);
                    var fieldId = GenerateFieldID(-1, listFriend[i].position);
                    var unit = new BattleUnitData(fieldId, sdata, new Stat(listFriend[i].charStatInfos), _friendSkill, true, -1);
                    unit.GetSkillID(true); // resource preloading
                    friendUnits.Add(fieldId, unit);
                }
            }
        }
        else
        {
            int nullPosition = 0;
            for (int i = 0; i < 6; ++i)
            {
                var unitData = GameCore.Instance.PlayerDataMgr.GetUnitSDataByTeam(_playerTeemIdx, i);
                if (unitData == null) { nullPosition = i; break; }
            }
            int friendMainCharacter = battlePara.friendIcon;
            var sdata = new HeroSData(friendMainCharacter);
            var fieldId = GenerateFieldID(-1, nullPosition);
            friendUnits.Add(fieldId, new BattleUnitData(fieldId, sdata, new Stat(listFriend[0].charStatInfos), _friendSkill, true, -1));
        }
            
    }
	internal void Init(BattlePara _battlePara, StageDataMap _data)
	{
        battlePara = _battlePara;
        int _playerTeemIdx = battlePara.playerTeam;

        InCombatUnits = new List<int>(12);
		units = new Dictionary<int, BattleUnitData>();
        friendUnits = new Dictionary<int, BattleUnitData>();

        AddPlayerUnits(_playerTeemIdx);

        // Add Frind Unit
        AddFriendUnit(_playerTeemIdx);

        // Add Enemy Unit
        for (int i = 0; i < _data.teamCnt; i++)
		{
			for (int j = 0; j < _data.monster_Ids.GetLength(1); ++j)
			{
				if (_data.monster_Ids[i, j] != -1)
				{
                    var sdata = new HeroSData(_data.monster_Ids[i, j]);
                    var fieldId = GenerateFieldID(i + 1, j);
                    var statInfos = GetStatInfos(sdata.key, true);
                    Stat stat = statInfos == null ? sdata.GetStat() : new Stat(statInfos.StatInfo);
                    var unit = new BattleUnitData(fieldId, sdata, stat, _playerTeemIdx, false, i + 1, (i==2 && j==4));
                    unit.SetStatInfosData(statInfos);
                    unit.GetSkillID(true); // resource preloading
                    //sdata.dormitory
                    //BattleEffect be = new BattleEffect()
                    //unit.AddEffect()
                    units.Add(fieldId, unit);
				}
			}
		}

    }

    internal void Init(BattlePara _battlePara, List<PvPOppUnitSData> pvpCharList)
    {
        battlePara = _battlePara;
        int _playerTeemIdx = battlePara.playerTeam;
        InCombatUnits = new List<int>(12);
        units = new Dictionary<int, BattleUnitData>();

        AddPlayerUnits(_playerTeemIdx);

        // Add opp Units
        for (int i = 0; i < pvpCharList.Count; ++i)
        {
            if (pvpCharList[i].charID > 0)
            {
                var sdata = new HeroSData(pvpCharList[i].charID); // 미구현(아무런 버프나 상태도 적용되지 않음)
                var fieldId = GenerateFieldID(1, pvpCharList[i].position);
                float[] pvpCharStatInfo = pvpCharList[i].charStatInfos;
                //pvpCharStatInfo[1] = 100000000000;
                Stat stat = pvpCharStatInfo == null ? sdata.GetStat() : new Stat(pvpCharStatInfo);
                units.Add(fieldId, new BattleUnitData(fieldId, sdata, stat, _playerTeemIdx, false, 1));
            }
        }
    }
    internal void Init(BattlePara _battlePara, HeroSData _raidBoss)
    {
        battlePara = _battlePara;
        int _playerTeemIdx = battlePara.playerTeam;
        InCombatUnits = new List<int>(12);
        units = new Dictionary<int, BattleUnitData>();

        AddPlayerUnits(_playerTeemIdx);

        // Add Raid Boss
        var fieldId = GenerateFieldID(1, 0);
        units.Add(fieldId, new BattleUnitData(fieldId, _raidBoss, _raidBoss.GetStat(), _playerTeemIdx, false, 1));
    }
    private StatInfos GetStatInfos(long _UID, bool isEnemy)
    {
        if (battlePara == null) return null;
        List<StatInfos> statInfoList = isEnemy ? battlePara.enemyStatInfosList : battlePara.unitStatInfosList;
        if (statInfoList == null) return null;

        for (int i = 0; i < statInfoList.Count; i++)
        {
            if ((int)_UID != statInfoList[i].CharUID)
                continue;
            return statInfoList[i];
        }
        return null;
    }
    public void AddPlayerUnits(int _teamIdx)
    {
        for (int i = 0; i < 6; ++i)
        {
            var unitData = GameCore.Instance.PlayerDataMgr.GetUnitSDataByTeam(_teamIdx, i);
            if (unitData != null)
            {
                var fieldId = GenerateFieldID(0, i);
                var statInfos = GetStatInfos(unitData.uid, false);
                //statInfos[1] = 100000000000;
                Stat stat = statInfos == null ? unitData.GetStat() : new Stat(statInfos.StatInfo);
                var unit = new BattleUnitData(fieldId, unitData, stat, _teamIdx, true, 0);
                unit.SetStatInfosData(statInfos);
                unit.GetSkillID(true); // resource preloading
                units.Add(fieldId, unit);
            }
        }
    }

    internal void Clear()
	{
		units.Clear();
		InCombatUnits.Clear();
	}
    internal BattleUnitData GetUnitByFieldID(int _fieldId)
    {
        if (_fieldId < 0)
        {
            if (friendUnits != null && friendUnits.ContainsKey(_fieldId))
                return friendUnits[_fieldId];
        }
        else
        {
            if (units.ContainsKey(_fieldId))
                return units[_fieldId];
        }
        return null;
    }
    internal BattleUnitData GetFriendUnitByFieldID(int _fieldId)
	{
		if(friendUnits != null && friendUnits.ContainsKey(_fieldId))
			return friendUnits[_fieldId];
		return null;
	}
    internal BattleUnitData GetFriendUnitByTeam(int _team, int _formation)
    {
        var fieldId = GenerateFieldID(_team, _formation);
        if (friendUnits.ContainsKey(fieldId))
            return friendUnits[fieldId];
        return null;
    }
	internal BattleUnitData GetUnitByTeam(int _team, int _formation)
	{
		var fieldId = GenerateFieldID(_team, _formation);
        if (_team < 0)
        {
            if (friendUnits.ContainsKey(fieldId))
                return friendUnits[fieldId];
        }
        else
        {
            if (units.ContainsKey(fieldId))
                return units[fieldId];
        }
		return null;
	}

    internal BattleUnitData GetUnitByFriendTeam(int _team, int _formation)
    {
        var fieldId = GenerateFieldID(_team, _formation);
        if (friendUnits.ContainsKey(fieldId))
            return friendUnits[fieldId];
        return null;
    }

    internal void UpdateUnitWorldPosInCombat()
	{
		for(int i = 0; i < InCombatUnits.Count; ++i)
			units[InCombatUnits[i]].UpdateWorldPos();
	}

	internal BattleUnitData GetUnitInCombatByTeam(int _team, int _formation)
	{
		var fieldId = GenerateFieldID(_team, _formation);
		if (units.ContainsKey(fieldId) && InCombatUnits.Contains(fieldId))
			return units[fieldId];
		return null;
	}

	/// <summary>
	/// 전투에 참여하는유닛을 추가한다.
	/// 콜백함수로 캐미 버프를 설정한다.
	/// </summary>
	/// <param name="_team">추가될 팀</param>
	/// <param name="_cbSetChemiBuffs">BattleSys.CBSetChemiBuff(int _fieldId) 호출</param>
	internal void AddCombatTeam(int _team, Action<int> _cbSetChemiBuffs)
	{
		for( int i = 0; i < 6; ++i)
		{
			var fieldId = GenerateFieldID(_team, i);
			if (units.ContainsKey(fieldId))
			{
				_cbSetChemiBuffs(fieldId);
				InCombatUnits.Add(fieldId);
			}
		}
	}

	internal void RemoveCombatUnit(int _fieldId)
	{
		InCombatUnits.Remove(_fieldId);
	}

	internal int GetCombatUnitCount()
	{
        if (InCombatUnits == null)
            return 0;

		return InCombatUnits.Count;
	}

	internal BattleUnitData GetCombatUnit(int _idx)
	{
    	if(InCombatUnits != null && 0 <= _idx && _idx < InCombatUnits.Count)
	    	return units[InCombatUnits[_idx]];
		return null;
	}

	internal void RemoveUnit(int _fieldId)
	{
		if (units.ContainsKey(_fieldId))
		{
			units[_fieldId].CleanUp();
			units.Remove(_fieldId);
		}
		RemoveCombatUnit(_fieldId);
	}

	internal static int GetTeamByFieldId(int _fieldId)
	{
		return _fieldId >> 6;
	}

	internal static int GetFormationByFieldId(int _fieldId)
	{
		return _fieldId & 0x3F;
	}

	internal static int GetLineByFieldId(int _fieldId)
	{
		return (_fieldId & 0x3F) % 3; // 0 ~ 2
	}

	internal static int GenerateFieldID(int _team, int _formation)
	{
		return (_team << 6) + _formation;
	}
    internal void SetGetBackAnimation(int _fieldId)
    {
        if (units.ContainsKey(_fieldId) == false)
        {
            if(friendUnits.ContainsKey(_fieldId) == false)
            return;
            var unitFriend = friendUnits[_fieldId].SpineCtrl;
            unitFriend.GetBackStraightAnimation();
            return;
        }
            
        var unit = units[_fieldId].SpineCtrl;
        unit.GetBackStraightAnimation();
        //unit.GetBackStraightAnimation();
    }

    // Todo 이것을 지우고 마지막 애니메이션 이벤트키으로 구별할것
    private bool CheckRemoveJumpInTeamSkill(BattleUnitData battleUnit)
    {
        int teamSkillNameNum = battleUnit.GetTeamSkillSkillNameNum();
        switch(teamSkillNameNum)
        {
            case 6:
            case 22:
            case 17:
                return true;

            case 14:
                return battleUnit.Data.charIdType == 1300015;

            case 13:
                return battleUnit.Data.charId == 1100010;

            case 23:
                return battleUnit.Data.charIdType == 1300051;

            default:
                return false;
        }
    }

    internal void SetStrikeExtraAnimation(int _fieldId, SpineAnimation _anim, string _animationName, bool _move)
    {
        if (!friendUnits.ContainsKey(_fieldId))
        {
            UnityEngine.Debug.LogError(_fieldId + "(fieldID) 유닛이 존재하지 않습니다.");
            return;
        }

        int _playerTeemIdx = battlePara.playerTeam;
        var unit = friendUnits[_fieldId].SpineCtrl;
        unit.SetAnimationTeam(_anim, _animationName, _move, CheckRemoveJumpInTeamSkill(friendUnits[_fieldId]));
    }
    internal void SetTeamExtraAnimation(int _fieldId, SpineAnimation _anim, string _animationName, bool _move)
    {
        if (!units.ContainsKey(_fieldId))
        {
            UnityEngine.Debug.LogError(_fieldId + "(fieldID) 유닛이 존재하지 않습니다.");
            return;
        }

        var unit = units[_fieldId].SpineCtrl;
        unit.SetAnimationTeam(_anim, _animationName, _move, CheckRemoveJumpInTeamSkill(units[_fieldId]));
    }
    internal BattleUnitData GetPartner(BattleUnitData unit)
    {
        int teamIdx = unit.TeamSkillIndex;
        var tsKey = (teamIdx > 4300000) ? teamIdx : GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(teamIdx);
        if (tsKey != 0)
        {
            var teamSkillData = GameCore.Instance.DataMgr.GetTeamSkillData(tsKey);
            for (int i = 0; i < teamSkillData.needChar.Length; i++)
            {
                if (teamSkillData.needChar[i] == unit.Data.charIdType) continue;
                int teamNumber = unit.TeamNumber ;// == -1 ? 0 : unit.TeamNumber;
                return GetUnitDataByCharIDType(teamNumber, teamSkillData.needChar[i]);
            }
        }
        return null;
    }
    private BattleUnitData GetUnitDataByCharIDType(int _team, int _charIDType)
    {
        for (int i = 0; i < 6; i++)
        {
            BattleUnitData unitData = GetUnitByTeam(_team, i);
            if (unitData == null) continue;
            if (unitData.Data.charIdType == _charIDType)
                return unitData;
        }
        return null;
    }
    internal void SetExtraAnimation(int _fieldId, SpineAnimation _anim, bool _move )
	{
        BattleUnitData unitData = null;
        if (units.ContainsKey(_fieldId)) unitData = units[_fieldId];
        else if (friendUnits.ContainsKey(_fieldId)) unitData = friendUnits[_fieldId];
        else
        {
            UnityEngine.Debug.LogError(_fieldId + "(fieldID) 유닛이 존재하지 않습니다.");
            return;
        }

		var unit = unitData.SpineCtrl;
		unit.SetAnimation(_anim, _move);
	}

	internal void SetNormalAnimation(int _fieldId, SpineAnimation _anim)
	{
		if (units == null || !units.ContainsKey(_fieldId))
			return;

        if (units[_fieldId].SpineCtrl != null)
    		units[_fieldId].SpineCtrl.SetAnimation(_anim);
	}

    internal bool CheckFriendRunning()
    {
        foreach(var unitData in friendUnits)
        {
            if(unitData.Value.SpineCtrl.isRunning == true)
                return true;
        }
        return false;

    }
    internal bool CheckRunning()
	{
        if (InCombatUnits == null)
            return true;

        for (int i = 0; i < InCombatUnits.Count; ++i)
            if (units[InCombatUnits[i]].SpineCtrl.isRunning)
                return true;
        return false;

	}


	internal bool CheckTeamDead(int _team)
	{
		_team = _team << 6;
		for (int i = 0; i < 6; i++)
		{
			if (units.ContainsKey(_team + i))
				if (false == units[_team + i].IsDead())
					return false;
		}

		return true;
	}

	/// <summary>
	/// 타겟 팀내에 어그로가 가장 높은 대상의 필드아이드를 반환한다.
	/// </summary>
	/// <param name="_teamIdx">공격할 팀 인덱스</param>
	/// <returns>공격할 대상의 필드아이디</returns>
	internal int GetAutoTargetIndex(int _team)
	{
        PriorityQueue<BattleUnitData> tgs = new PriorityQueue<BattleUnitData>(this);

        for (int i = 0; i < 3; i++)
		{
			var fieldId1 = GenerateFieldID(_team, i);
			var fieldId2 = GenerateFieldID(_team, i + 3);

			if (units.ContainsKey(fieldId1))
			{
				var unit = units[fieldId1];
				if (unit != null && !unit.IsDead())
                    tgs.Push(unit);
			}
			else if (units.ContainsKey(fieldId2))
			{
				var unit = units[fieldId2];
				if (unit != null && !unit.IsDead())
                    tgs.Push(unit);
            }
		}

        BattleCommonDataMap battleCommonData = GameCore.Instance.PlayerDataMgr.GetBattleCommonData();
        var rand = UnityEngine.Random.value * 100f;
        int priority = rand < battleCommonData.PriorityAggro_1 ? 1 :
                       rand < battleCommonData.PriorityAggro_1 + battleCommonData.PriorityAggro_2 ? 2 : 3;

        int tgFID = -1;
        for (int i = 0; i < priority && tgs.Count != 0; ++i)
            tgFID = tgs.Pop().FieldId;

        return tgFID;
	}

    public int Compare(BattleUnitData x, BattleUnitData y)
    {
        // 내림차순으로 정렬
        return y.GetStat(UnitStat.Aggro).CompareTo(x.GetStat(UnitStat.Aggro));
    }


    /// <summary>
    /// 공격 가능한 대상 필드 아이디 리스트 반환
    /// </summary>
    /// <param name="_team">대상 팀</param>
    /// <returns></returns>
    internal int[] AttackAbleList(int _team)
	{
		List<int> fids = new List<int>();
		for (int i = 0; i < 3; i++)
		{
			var fieldId1 = GenerateFieldID(_team, i);
			var fieldId2 = GenerateFieldID(_team, i+3);

			if (units.ContainsKey(fieldId1))
			{
				var unit = units[fieldId1];
				if (unit != null && !unit.IsDead())
				{
					fids.Add(fieldId1);
					continue;
				}
			}
			if(units.ContainsKey(fieldId2))
			{
				var unit = units[fieldId2];
				if (unit != null && !unit.IsDead())
					fids.Add(fieldId2);
			}
		}

		return fids.ToArray();
	}

	internal bool IsInCombat(int tgFieldId)
	{
		return InCombatUnits.Contains(tgFieldId);
	}

	internal void StopAllCoroutineInCombat()
	{
		for( int i = 0; i < InCombatUnits.Count; ++i)
			units[InCombatUnits[i]].SpineCtrl.StopAllCoroutines();
	}

    internal void StopAllAnimation()
    {
        for (int i = 0; i < InCombatUnits.Count; ++i)
            units[InCombatUnits[i]].SpineCtrl.SkelAnim.timeScale = 0f;
    }
}
