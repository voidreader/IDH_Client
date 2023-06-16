using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 배틀 유닛 데이터
/// </summary>
internal class BattleUnitData
{
    int fieldId;    // team*128 + formation;

    Transform FormationRoot; // 팀 전체가 속한 루트 트랜스폼
    Transform cachedTf;
    HeroSData sdata;
    UnitDataMap data;

    Stat defStat;           // data.Stats를 포인팅한다.
    Stat addedStat;     // 더해지는 스탯
    internal Stat AddedStat { get { return addedStat; } }
    Stat scaledStat;    // 곱한 뒤 더해지는 스탯
    internal Stat ScaledStat { get { return scaledStat; } }
    Stat nowStat;           // 최종적으로 사용되는 스탯

    bool isPlayerTeam;

    float nowHp;
    float nowMental;

    float skillCoolTime;
    float skillCoolAcc;

    bool cachedFlipX;

    Dictionary<int, List<BattleEffect>> chemis;
    Dictionary<int, BattleEffect> states;
    List<BattleEffect> buffs;

    BattleSkill skill;
    BattleSkill teamSkill;
    int skipAtkCount = 0;       // 스킬 사용으로 인한 공격 스킵 카운터
    int target = -1;                // 스킬 타겟을 임시로 담기 위한 변수
                                    //EquipItems items;
    bool airborne = true;                  //에어본 체크를 위한 변수
    int teamNumber;
    int teamSkillIdx;
    bool attackFirst = false;

    internal bool AttackFirst { set { attackFirst = value; } get { return attackFirst; } }

    SpineCharacterCtrl spineCtrl;

    UnitStatusUI statusUI; // 타겟을 따라다니기 위해..

    internal int FieldId { get { return fieldId; } }
    internal UnitDataMap Data { get { return data; } }
    internal float NowHp { get { return nowHp; } }
    internal float NowMental { set { nowMental = value; } get { return nowMental; } }

    internal bool IsPlayerTeam { get { return isPlayerTeam; } }

    internal SpineCharacterCtrl SpineCtrl {  get { return spineCtrl; } }
	internal Transform Transform { get { return cachedTf; } }
	//internal Transform ShadowTf { get; set; }
	internal Transform OnHeadTf { get; set; }
	internal Transform RootTf { get { return FormationRoot; } }
	internal Transform TeamRootTf { get { return FormationRoot.parent; } }
	internal UnitStatusUI StatusUI {  get { return statusUI; } set { statusUI = value; } }
	internal BattleSkill SkillData {  get { return skill; } }
    internal BattleSkill TeamSkillData { get { return teamSkill; } }
	internal Dictionary<int, List<BattleEffect>>.Enumerator ChemisEnumerator {  get { return chemis.GetEnumerator(); } }
	internal int ChemiBuffCount { get { return chemis.Count; } }
	internal Vector3 dstPos { get; set; }

	internal float Width { get { return spineCtrl.GetBoundWidth(); } }
	internal float Height { get { return spineCtrl.GetboundHeight(); } }
	internal Vector3 Center { get { return new Vector3(0, Height*0.45f, 0f); } }

    internal bool Airborne { get {
            airborne = (airborne) ? false : true;
            return airborne; } }

    internal bool TeamSkillable { get{ return   !states.ContainsKey((int)EffectType.Sleep) &&
                                                !states.ContainsKey((int)EffectType.Stun) &&
                                                !states.ContainsKey((int)EffectType.paralyze);}}
    internal bool Skillable { get { return	skillCoolAcc <= 0 &&
											!states.ContainsKey((int)EffectType.Sleep) &&
											!states.ContainsKey((int)EffectType.Stun) &&
											!states.ContainsKey((int)EffectType.paralyze); } }
	internal bool Attackable { get { return !states.ContainsKey((int)EffectType.Sleep) &&
											!states.ContainsKey((int)EffectType.Stun)&&
                                            !states.ContainsKey((int)EffectType.paralyze);}	}

    internal int TeamNumber {get { return teamNumber; } }
    internal int TeamSkillIndex { get { return teamSkillIdx; } }

    public DamagePower atk;
    public DamagePower grd;
    public float attackDamage;

    internal float mentalMultipleValue = 0f; //멘탈계산 할때 곱하는 수


    public float addedCritical { get; private set; }
    public float autoRecovery { get; private set; }
    public float reduceCool { get; private set; }



    int criSpineID = -1;

    public Dictionary<MyRoomEffectType, float> myroomEffect = new Dictionary<MyRoomEffectType, float>();


    public int GetSkillID(bool _critical)
    {
        if (!_critical || Data.attackCriSpineID <= 0)
            return Data.attackSpineID;
        else
        {
            if (criSpineID == -1)
            {
                criSpineID = Data.attackSpineID;
                GameCore.Instance.ResourceMgr.GetObject<UnityEngine.Object>(ABType.AB_Prefab, Data.attackCriSpineID, (_obj) =>
                  {
                      if (_obj != null)
                          criSpineID = Data.attackCriSpineID;
                  });
            }
            return criSpineID;
        }
    }

    public int GetTeamSkillSkillNameNum()
    {
        var tsKey = (teamSkillIdx > 4300000) ? teamSkillIdx : GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(teamSkillIdx);
        if (tsKey != 0)
        {
            var teamSkillData = GameCore.Instance.DataMgr.GetTeamSkillData(tsKey);
            return teamSkillData.skillNameNum;
        }
        return -1;
    }

    private bool bFirstCool = false;
	internal BattleUnitData(int _fieldId, HeroSData _sdata, Stat _stat, int teamIdx, bool _isPlayerTeam, int _teamNumber, bool _hpUp = false)
	{
		fieldId = _fieldId;
        sdata = _sdata;
        data = GameCore.Instance.DataMgr.GetUnitData(_sdata.key);
        //var strengthenData = GameCore.Instance.DataMgr.GetStrengthenData(data.charType);

        isPlayerTeam = _isPlayerTeam;
        teamNumber = _teamNumber;
        teamSkillIdx = teamIdx;

        defStat = _stat;
        //defStat = _sdata.GetStat();
        addedStat = new Stat();
		scaledStat = new Stat();
		nowStat = new Stat(defStat);

        // 임시. 적 보스 체력 뻥튀기용
        if (_hpUp)
        {
            var commonData = GameCore.Instance.PlayerDataMgr.GetBattleCommonData();
            nowStat.SetStat(UnitStat.Hp, nowStat.GetStat(UnitStat.Hp) * commonData.BossHPScale);
        }

        AddItemStat();
        AddUpgradeStat();
        CalcNowStat();
		nowHp = nowStat.GetStat(UnitStat.Hp);
        nowMental = 100f;
        //nowMental = nowStat.GetStat(UnitStat.Mental);

        skill = new BattleSkill(GameCore.Instance.DataMgr.GetSkillData(data.skillId));
		skillCoolTime = skill.Data.firstCool;
		skillCoolAcc = skillCoolTime;
		bFirstCool = true;
        airborne = true;
        var tsKey = (teamIdx > 4300000) ? teamIdx : GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(teamIdx);
        if(tsKey != 0)
        {
            var teamSkillData = GameCore.Instance.DataMgr.GetTeamSkillData(tsKey);
            for (int i = 0; i < teamSkillData.needChar.Length; i++)
            {
                if (teamSkillData.needChar[i] == data.charIdType)
                {
                    int teamSkillKey = teamSkillData.charSkill[i];
                    teamSkill = new BattleSkill(GameCore.Instance.DataMgr.GetSkillData(teamSkillKey));
                    //if (teamSkill != null) Debug.Log("팀 스킬 정보 : " + teamSkill.Data.name);
                    break;
                }
            }
        }

        // 마이룸 버프 데이터 생성
        if (_teamNumber == 0)
        {
            foreach(var mrSData in GameCore.Instance.PlayerDataMgr.myRoomBuffs)
            {
                switch (mrSData.EFFECT)
                {
                    case MyRoomEffectType.MoreReward:   /* Nothing */
                    case MyRoomEffectType.MoreCritical: break;
                    case MyRoomEffectType.MoreAttack:   nowStat.SetStat(UnitStat.Attack, nowStat.GetStat(UnitStat.Attack) * mrSData.VALUE); break;
                    case MyRoomEffectType.MoreHP:       nowStat.SetStat(UnitStat.Hp, nowStat.GetStat(UnitStat.Hp) * mrSData.VALUE); break;
                    case MyRoomEffectType.MoreConcent:  nowStat.SetStat(UnitStat.Concent, nowStat.GetStat(UnitStat.Concent) * mrSData.VALUE); break;
                    case MyRoomEffectType.MoreArmor:    nowStat.SetStat(UnitStat.Armor, nowStat.GetStat(UnitStat.Armor) * mrSData.VALUE); break;
                    case MyRoomEffectType.AutoHeal:
                    case MyRoomEffectType.DecCoolTime:  myroomEffect.Add(mrSData.EFFECT, mrSData.VALUE); break;
                }
            }
        }


        chemis = new Dictionary<int, List<BattleEffect>>();
		states = new Dictionary<int, BattleEffect>();
        buffs = new List<BattleEffect>();
	}

    public void SetStatInfosData(StatInfos _statInfo)
    {
        if (_statInfo == null)
            return;

        addedCritical = _statInfo.addedCritical;
        autoRecovery = _statInfo.autoRecovery;
        reduceCool = _statInfo.reduceCool;
    }

    void AddItemStat()
    {
        // 서버에서 계산된 데이터가 온다
        //addedStat.AddStat(sdata.GetItemStat(sdata.GetBaseStat()));
    }

    void AddUpgradeStat()
    {
        // 서버에서 계산된 데이터가 온다
        //addedStat.AddStat(sdata.GetBaseStat().SubStat(sdata.GetBaseStat(0)));
    }

    internal void SetWorldPosition(Vector3 _pos)
	{
		cachedTf.position = _pos;
        if (!statusUI.isBoss)
        //    statusUI.SetPosition(GameCore.Instance.WorldPosToUIPos(RootTf.position + new Vector3(0, -0.5f, 0)));
        //else
            statusUI.SetPosition(GameCore.Instance.WorldPosToUIPos(OnHeadTf.position));
	}

	internal void SetWorldPosition(Vector3 _pos, float _height)
	{
		var Y = new Vector3(0f, _height, 0f);
		//ShadowTf.localPosition = -Y;// + new Vector3(0f,1f,0f);
		cachedTf.position = _pos + Y;
        if (!statusUI.isBoss)
            //statusUI.SetPosition(GameCore.Instance.WorldPosToUIPos(RootTf.position + new Vector3(0,-0.5f,0)));
        //else
            statusUI.SetPosition(GameCore.Instance.WorldPosToUIPos(OnHeadTf.position));
	}

	internal Vector3 GetWorldPosition()
	{ return cachedTf.position; }

	internal Vector3 GetLocalPosition()
	{ return cachedTf.localPosition; }


	internal void UpdateWorldPos()
	{
        if (!statusUI.isBoss)
        //    statusUI.SetPosition(GameCore.Instance.WorldPosToUIPos(RootTf.position + new Vector3(0, -0.5f, 0)));
        //else
            statusUI.SetPosition( GameCore.Instance.WorldPosToUIPos(OnHeadTf.position) );
		dstPos = Transform.parent.position;
	}

	internal float GetStat(UnitStat _stat)
	{
		return nowStat.GetStat(_stat);
	} 

	internal void AddHp(float _value)
	{
		nowHp = Mathf.Clamp(nowHp + _value, 0, GetStat(UnitStat.Hp));
	}

    internal void AddMp(float _value)
    {
        nowMental = Mathf.Clamp(nowMental + _value, 0, 100);
        //nowMental = Mathf.Clamp(nowMental + _value, 0, GetStat(UnitStat.Mental));
        CalcNowStat();
        statusUI.SetMental(nowMental);
    }

	internal bool IsDead()
	{
		return nowHp <= 0;
	}


	// AddChemi
	/// <summary>
	/// 캐미버프를 추가한다.
	/// 만약 이미 존재하는 버프라면 false를 반환한다.
	/// 상태이상버프는 더욱 강한 것으로 갱신되고, 갱신되었다면 true를 반환
	/// </summary>
	/// <param name="_buff">추가될 버프</param>
	/// <returns>상태이상 : 갱신되면 true. 그외 : 없던 버프라면 true</returns>
	internal bool AddChemi(BattleEffect _buff, int _fieldId)
	{
		if (_buff.type == BattleEffectType.NormalChemi ||
				_buff.type == BattleEffectType.SpecialChemi ||
				_buff.type == BattleEffectType.RSA_Chemi)
		{
			if (!chemis.ContainsKey(_fieldId))
				chemis.Add(_fieldId, new List<BattleEffect>());
			chemis[_fieldId].Add(_buff);

			if (_buff.type != BattleEffectType.SpecialChemi &&  // 스페셜버프는 바로 적용하지 않기때문
					_buff.type != BattleEffectType.RSA_Chemi)				// 확률 상태이상은 적용해서 쓰는 것이 아니기 때문
				ApplyBuff(_buff);

			//Debug.Log("[" + FieldId + "] Add Cnemi " + _buff.effect.type);
			return true;
		}
		return false;
	}

	// RemoveChemi
	/// <summary>
	/// fieldId로 검색하여 해당 버프를 모두 삭제한다.
	/// 상태이상버프는 needFiledId를 필요로하지 않기때문에 이 함수로 삭제할 수 없다.
	/// </summary>
	/// <param name="_fieldId">삭제될 버프의 필요 필드 아이디</param>
	/// <param name="_ret">꺼진 버프의 아이콘아이디 배열을 반환하지 여부</param>
	/// <param name="_remove">버프가 완전히 삭제될지 여부</param>
	/// <returns>삭제된 버프의 아이콘 아이디(버프아이콘 삭제를 위해)</returns>
	internal void RemoveChemiByFieldId(int _fieldId, Action<int, BattleEffect> _cbRemoveIcon)
	{
		if( !chemis.ContainsKey(_fieldId))
			return;

		var list = chemis[_fieldId];
		for( int i = 0; i < list.Count; ++i)
		{
			if (DepplyBuff(list[i]))
			    _cbRemoveIcon(FieldId, list[i]);
		}
	}


	// ApplyChemi
	// DepplyCmemi
	internal bool SetApplySpecialChemi(EffectType _type, int _needFid, bool _apply)
	{
		if( chemis.ContainsKey(_needFid))
		{
			var list = chemis[_needFid];
			for (int i = 0; i < list.Count; ++i)
			{
				if (list[i].effect.type == _type)
				{
					if (list[i].Applied == _apply)
						return false;

					list[i].Applied = _apply;
					var buffpart = list[i].effect.buffData;

					switch (_type)
					{
						case EffectType.FollowAtk:

							if (_apply) scaledStat.SetStat(buffpart.tgStat, scaledStat.GetStat(buffpart.tgStat) + buffpart.value1);
							else		scaledStat.SetStat(buffpart.tgStat, scaledStat.GetStat(buffpart.tgStat) - buffpart.value1);
							CalcNowStat();
                            return true;

						case EffectType.AggroCtrl:
							if (_apply)	scaledStat.SetStat(UnitStat.Aggro, scaledStat.GetStat(UnitStat.Aggro) + buffpart.value1);
							else		scaledStat.SetStat(UnitStat.Aggro, scaledStat.GetStat(UnitStat.Aggro) - buffpart.value1);
							CalcNowStat();
                            return true;

						case EffectType.CounterAtk:
                            if (_apply)	scaledStat.SetStat(buffpart.tgStat, scaledStat.GetStat(buffpart.tgStat) + buffpart.value1);
							else		scaledStat.SetStat(buffpart.tgStat, scaledStat.GetStat(buffpart.tgStat) - buffpart.value1);
                            CalcNowStat();

                            //Debug.Log("[" + fieldId + "] " + buffpart.tgStat + " : " + nowStat.GetStat(buffpart.tgStat));

                            return true;

                        case EffectType.SetGuard:
                            if (_apply)	scaledStat.SetStat(buffpart.tgStat, scaledStat.GetStat(buffpart.tgStat) + buffpart.value1);
							else		scaledStat.SetStat(buffpart.tgStat, scaledStat.GetStat(buffpart.tgStat) - buffpart.value1);
                            CalcNowStat();
                            return true;
					}
					return true;
				}
			}
		}
		return false;
	}

	// AddEffect
	internal bool AddEffect(BattleEffect _effect)
	{
		switch(_effect.type)
		{
			case BattleEffectType.Buff:
                buffs.Add(_effect);
                if (_effect.effect.buffData.tgStat == UnitStat.Mental)
                {
                    mentalMultipleValue += _effect.effect.buffData.value1;
                }
                else
                {
                    ApplyBuff(_effect);
                }
                //Debug.Log("[" + FieldId + "] Add Buff " + _effect.effect.buffData.tgStat);
                return true;

			case BattleEffectType.StateBuff:
				var type = (int)_effect.effect.type;
				if(!states.ContainsKey(type))
				{
					states.Add(type, _effect);
					return true;
				}

				// 이미 존재할 경우
				if (_effect.effect.type == EffectType.HPPoison ||
						_effect.effect.type == EffectType.MTPoison )
				{
				
					var prevDmg = states[type].effect.buffData.value1;
					var nextDmg = _effect.effect.buffData.value1;
					if (prevDmg < nextDmg)
					{
						//effects.Remove((int)_effect.effect.type);
						states[type] = _effect;
						//Debug.Log("[" + FieldId + "] Add State " + _effect.effect.type);
						return true;
					}
					else if(prevDmg == nextDmg)
					{
						var prevCnt = states[type].GetContineuance();
						var nextCnt = _effect.GetContineuance();
						if( prevCnt < nextCnt )
						{
							states[type] = _effect;
							//Debug.Log("[" + FieldId + "] Add State " + _effect.effect.type);
							return true;
						}
					}
				}
				else
				{
					var prevCnt = states[type].GetContineuance();
					var nextCnt = _effect.GetContineuance();

					if (prevCnt < nextCnt)
					{
						states[type] = _effect;
						return true;
					}
				}
				return false;

			default:
				return false;
		}
	}

	// RemoveEffect
	// DistCounteffect
	internal void DisCountEffect(Action<int, BattleEffect> _cbRemoveIcon)
	{
        //버프 & 디버프
        for (int i = buffs.Count - 1; 0 <= i; --i)
        {
            if (!buffs[i].ReduceContinuance())
            {
                if (buffs[i].effect.buffData.tgStat == UnitStat.Mental)
                {
                    mentalMultipleValue -= buffs[i].effect.buffData.value1;
                }
                else
                {
                    if (DepplyBuff(buffs[i]))
                        _cbRemoveIcon(FieldId, buffs[i]);
                    buffs.RemoveAt(i);
                }
            }
        }


        //상태이상
        List<int> removeLists = new List<int>();
        var enumerator = states.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.Value.ReduceContinuance())
            {
                removeLists.Add(enumerator.Current.Key);
            }
            _cbRemoveIcon(FieldId, enumerator.Current.Value);
        }

        for (int i = 0; i < removeLists.Count; ++i)
            states.Remove(removeLists[i]);
    }
    List<BattleEffect> nDebuffList = new List<BattleEffect>();
    //정화 스킬
    internal void Purification(int count, Action<int, BattleEffect> _cbRemoveIcon)
    {
        int i = 0;
        while(i < count)
        {
            i++;
            int statesCount = states.Count;
            if(statesCount > 0)
            {
                List<int> effectTypeList = new List<int>();
                var enumerator = states.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    effectTypeList.Add((int)enumerator.Current.Value.effect.type);
                }
                if (effectTypeList.Count > 0)
                {
                    int randNum = UnityEngine.Random.Range(0, effectTypeList.Count);
                    RemoveStateBuff((EffectType)effectTypeList[randNum], _cbRemoveIcon);
                }
            }
            else
            {
                int buffsCount = buffs.Count;
                if (buffsCount <= 0)
                    continue;
                nDebuffList.Clear();
                for (int n = 0; n < buffsCount; n++)
                {
                    if (buffs[n].effect.buffData.value1 < 0) nDebuffList.Add(buffs[n]);
                }
                if (nDebuffList.Count <= 0) continue;
                int randNum = UnityEngine.Random.Range(0, nDebuffList.Count);
                //buffs[randNum].effect
                UnitStat removeStat = nDebuffList[randNum].effect.buffData.tgStat;
                for(int n = 0; n < nDebuffList.Count; n ++)
                {
                    if(nDebuffList[n].effect.buffData.tgStat == removeStat)
                    {
                        if (DepplyBuff(nDebuffList[randNum]))
                            _cbRemoveIcon(FieldId, nDebuffList[randNum]);
                        buffs.Remove(nDebuffList[randNum]);
                    }
                }
            }
        }
    }

	internal void RemoveStateBuff(EffectType _type, Action<int, BattleEffect> _cbRemoveIcon)
	{
		if( states.ContainsKey((int)_type))
		{
			states[(int)_type].SetContinuance(0);
			_cbRemoveIcon(fieldId, states[(int)_type]);
			states.Remove((int)_type);
		}
	}
    public float mentalPersentValue;
	/// <summary>
	/// 유닛에게 버프를 적용함
	/// </summary>
	/// <param name="_buff">적용할 버프</param>
	internal void ApplyBuff(BattleEffect _buff)
	{
	//	if (_buff.type == BattleEffectType.NormalChemi ||
	//			_buff.type == BattleEffectType.Buff)
		{
			// 적용된 버프는 재적용하지 않는다.
			if (_buff.Applied == true)
				return;

			_buff.Applied = true;
			//for (int j = 0; j < _buff.BuffCount; ++j)
			{
				var bp = _buff.effect.buffData;

                if (bp.type == 0) scaledStat.SetStat(bp.tgStat, scaledStat.GetStat(bp.tgStat) + bp.value1);
				else			  addedStat.SetStat(bp.tgStat, addedStat.GetStat(bp.tgStat) + bp.value1);
            }
			CalcNowStat();
		}
	}

	/// <summary>
	/// 유닛에세 적용된 버프를 해제함
	/// </summary>
	/// <param name="_buff">해제될 버프</param>
	internal bool DepplyBuff(BattleEffect _buff)
	{
		//if (_buff.Type < BattleEffect.StateBuff)
		{
			// 적용되지 않은 버프를 해제하지 않는다.
			if (_buff.Applied == false)
				return false;

			_buff.Applied = false;
			var bp = _buff.effect.buffData;
			if (bp.type == 0) scaledStat.SetStat(bp.tgStat, scaledStat.GetStat(bp.tgStat) - bp.value1);
			else			  addedStat.SetStat(bp.tgStat, addedStat.GetStat(bp.tgStat) - bp.value1);
		}
		CalcNowStat();
        return true;
	}


	internal BattleEffect GetStateEffect(EffectType _type)
	{
		if (states.ContainsKey((int)_type))
			return states[(int)_type];
		return null;
	}

	internal BattleEffect GetSpecialChemiEffect(EffectType _type, int _fid)
	{
		if (chemis.ContainsKey(_fid))
		{
			var list = chemis[_fid];
			for (int i = 0; i < list.Count; ++i)
				if (list[i].type == BattleEffectType.SpecialChemi &&
						list[i].effect.type == _type)
					return list[i];
		}
		return null;
	}

	internal void SubSkillCoolAcc(float _time )
	{
		skillCoolAcc -= _time;
	}

    internal void SetCoolTimeAcc(float value)
    {
        skillCoolAcc = value;
    }

	internal float GetCoolTimeAcc()
	{
		return skillCoolAcc;
	}

	internal float GetCoolTime()
	{
		return skillCoolTime;
	}

    public bool HaveStateBuff(EffectType _type)
    {
        if (states.ContainsKey((int)_type))
        {
            return states[(int)_type].continuance != 0;
        }
        else
            return false;
    }

    /// <summary>
    /// 스킬 쿨타임을 리셋한다.
    /// 증가값에 따라 쿨다운 시간이 증가한다.
    /// 보통 사용 직후 호출 된다.
    /// </summary>
    internal void ResetSkillCool(float mentalValue)
	{
		// 아직 쿨 타임이 남았다면 동작하지 않는다.
		if (0 < skillCoolAcc)
			return;

		if(bFirstCool)
		{
			skillCoolTime = skill.Data.baseCool;
			bFirstCool = false;
		}
		else
		{
            float maxCool = skill.Data.maxCool;
            float coolTime = skill.Data.baseCool + mentalValue; 
            skillCoolTime = Mathf.Min(coolTime, maxCool);
        }

        if (reduceCool != 0f) skillCoolAcc = skillCoolTime * (2f - reduceCool);  //후딜레이 및 아이템 효과에의한 후딜 경감
        else                  skillCoolAcc = skillCoolTime;
    }

	// EquipItem

	// UnequipItem

	internal void CalcNowStat()
	{
		float hp_percent = nowHp / nowStat.GetStat(UnitStat.Hp);
		float mental_percent = nowMental / nowStat.GetStat(UnitStat.Mental);

		for (int i = 0; i < (int)UnitStat.Count; ++i)
		{
			var stat = defStat.GetStat((UnitStat)i) * (1+(scaledStat.GetStat((UnitStat)i)* 0.01f)) + addedStat.GetStat((UnitStat)i);
            nowStat.SetStat((UnitStat)i, stat);
        }
		nowHp = nowStat.GetStat(UnitStat.Hp) * hp_percent;
		nowMental = nowStat.GetStat(UnitStat.Mental) * mental_percent;
	}

	internal void CleanUp()
	{
		chemis.Clear();
		buffs.Clear();
		states.Clear();
		GameObject.Destroy(cachedTf.gameObject);
	}

	internal void Instantiate(bool _flipX, UnitStatusUI _statusUI, Transform _parent, Func<int, int, Spine.TrackEntry, BattleEvent, float> _cbEvent)
	{
		FormationRoot = _parent;
		statusUI = _statusUI;
		cachedFlipX = _flipX;



		GameCore.Instance.ResourceMgr.GetInstanceObject(ABType.AB_Prefab, data.prefabId, (_obj) => {

            if (_obj == null)
            {
                Debug.LogError("Load Fail!!");
                return;
            }

            cachedTf = _obj.transform;
			cachedTf.gameObject.layer = LayerMask.NameToLayer("World");
			cachedTf.parent = FormationRoot;

            UnityCommonFunc.ResetTransform(cachedTf);

			// 스파인 캐릭터
			cachedTf.gameObject.AddComponent<Billboard>().Awake();

            var skillData = GameCore.Instance.DataMgr.GetSkillData(data.skillId);
            

            spineCtrl = cachedTf.gameObject.AddComponent<SpineCharacterCtrl>();
			spineCtrl.Init(cachedFlipX, fieldId, _cbEvent);
            spineCtrl.skillStraight = (skillData.type == PositionType.NearStraight ||
                                       skillData.type >= PositionType.EnemyFront);
            spineCtrl.GetboundHeight();
            //Debug.Log("이게 fieldID : " + fieldId + "  이게 라인 : " + BattleUnitPool.GetLineByFieldId(fieldId));
            int line = BattleUnitPool.GetLineByFieldId(fieldId);
            spineCtrl.SetSortingOrder((line + 1) * 2);

            // 그림자
            //ShadowTf = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/shadow", cachedTf).transform;
            //ShadowTf.localPosition = new Vector3(0f, 1f, 0f);

            // 온헤드
            OnHeadTf = new GameObject("onHead").transform;
			OnHeadTf.parent = Transform;
            //OnHeadTf.localPosition = new Vector3(0f, Height, 0f);
            OnHeadTf.localPosition = new Vector3(0f, 2.3f, 0f);
            OnHeadTf.localRotation = Quaternion.identity;

			//Debug.Log("["+fieldId+"]"+OnHeadTf.position + "->" + GameCore.Instance.WorldPosToUIPos(OnHeadTf.position) + OnHeadTf.eulerAngles);

			UpdateWorldPos();

			//콜리더
			var col = RootTf.gameObject.AddComponent<BoxCollider>();
			//col.center = new Vector3(0, spineCtrl.GetboundHeight()/2);
			//col.size = new Vector3(spineCtrl.GetBoundWidth(), spineCtrl.GetboundHeight(), 1f);
            col.center = new Vector3(0, 2.3f * 0.5f);
            col.size = new Vector3(1.4f, 2.3f);
            
            if (fieldId == 196)
            {
                cachedTf.transform.localScale = new Vector2(1.4f, 1.4f);
            }
            
        });
	}

	internal void ToggleTurn()
	{
		spineCtrl.ToggleFlipX();
	}

	internal void SetTurn(bool _back)
	{
		spineCtrl.FlipX((cachedFlipX) ? !_back : _back);
		//Debug.Log("[+" + fieldId + "] Flip : " +( _back ^ cachedFlipX));
	}

	internal bool IsLookLeft()
	{
		return spineCtrl.GetFlipX();
	}

	internal bool IsAtattackSkip()
	{
		if (skipAtkCount != 0)
		{
			--skipAtkCount;
			return true;
		}
		else
			return false;
	}

	internal void SetTarget(int _tgFid)
	{
		target = _tgFid;
	}

	internal int GetTarget()
	{
		return target;
	}

	internal void AddSkipAttackCount()
	{
		++skipAtkCount;
	}

    public void SetAttackDamage(DamagePower _atk, DamagePower _grd, float _damage)
    {
        attackDamage = _damage;
        atk = _atk;
        grd = _grd;
    }
}
