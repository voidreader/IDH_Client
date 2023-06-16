using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BattleAttackCtrl : IComparer<BattleUnitData>
{
    BattleUnitPool unitPool;
    BattleUIBase ui;
    BattleBuffCtrl buffCtrl;
    BattleCommonDataMap consts;
    BattleFieldBase battleField;
    BattleSysBase sys;


    public PriorityQueue<BattleUnitData> attackQueue;  // 공격 순서 큐
    public Queue<int> SkillQueue = new Queue<int>();   // 스킬 리스트 SkillQueue = Target Field ID;
    public List<int> autoSkillQueue = new List<int>();// 자동 스킬 리스트


    public bool QueueingStrikeSkill;                   // 스트라이크 스킬 버튼을 눌러 동작확정이 되었다면 true. 동작 직후 false

    public bool QueueingTeamSkill;                  // 팀스킬 버튼을 눌러 동작확정이 되었다면 true. 동작 직후 false
    public float teamSkillDmgScale = 1f;            // 팀스킬시 추가 데미지 배율

    public float teamSkillAcc = 0f;                // 팀스킬 누적 게이지
    public float StrikeSkillAcc = 0f;              // 스트라이크 스킬 누적 게이지

    public int battleCount = 0;                     // 전투 진행도.(상대팀의 팀 인덱스값)

    public bool canUseSkillEnmey;                   // 적이 스킬을 사용하는가?

    private int skillTarget = -1;               // 현재 타겟을 랜덤으로 결정되기 때문에, 팀스킬시 최초 한번만 구하고 임시로 저장하기위한 변수

    private List<BattleUnitData> teamSkillUnitList;
    private List<BattleUnitData> teamBehindUnitList;
    private List<int> activeUnitId;
    private int teamNumber = 0;
    private int friendTeamSkill;
    private bool checkOnceForTutorial = false;
    private bool checkOnceForTutorialSkill = false;

    List<int> applyUnits = new List<int>();
    EffectDataMap[] effects = new EffectDataMap[5];


    public bool bTutoFirstSkillUsed = false; // 최초 튜토리얼시 스킬 사용 튜토리얼구분을 위한 임시변수


    public void Init(BattleUnitPool _unitPool, BattleUIBase _ui, BattleBuffCtrl _buffCtrl, BattleFieldBase _field, BattlePara _para)
    {
        int _teamNumber = _para.playerTeam;
        unitPool = _unitPool;
        ui = _ui;
        buffCtrl = _buffCtrl;
        battleField = _field;

        attackQueue = new PriorityQueue<BattleUnitData>(this);
        consts = GameCore.Instance.PlayerDataMgr.GetBattleCommonData();
        sys = GameCore.Instance.SubsysMgr.GetNowSubSys() as BattleSysBase;
        canUseSkillEnmey = sys.canUseSkillEnmey;

        teamSkillUnitList = new List<BattleUnitData>();
        teamBehindUnitList = new List<BattleUnitData>();
        activeUnitId = new List<int>();

        teamNumber = _teamNumber;

        friendTeamSkill = _para.friendTeamSkill;

        QueueingStrikeSkill = false;
        QueueingTeamSkill = false;
    }

    public void Release()
    {
        unitPool = null;
        ui = null;
        buffCtrl = null;
        battleField = null;
    }

    public bool bPauseTuto = false;

    /// <summary>
    /// 전투 처리
    /// </summary>
    public void UpdateBattle()
    {
        skillTarget = -1;
        if (bPauseTuto)
            return;
        
        //if(ui.isTeamSkillAble == true && checkOnceForTutorial == false && (GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.tutorialPos == 4))
        //{
        //    bPauseTuto = true;
        //    GameCore.Instance.TurnOnTutorial(() =>
        //    {
        //        checkOnceForTutorial = true;
        //        GameCore.Instance.StopTime();
        //        BattleSys battleSys = GameCore.Instance.SubsysMgr.GetNowSubSys() as BattleSys;
        //        //battleSys.State = BattleSysState.Pause;
        //    }, 4);

        //    return;
        //}

        //if(ui.isStrikeSkillAble == true && checkOnceForTutorial == false && (GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.tutorialPos == 5))
        //{
        //    bPauseTuto = true;
        //    GameCore.Instance.TurnOnTutorial(() =>
        //    {
        //        checkOnceForTutorial = true;
        //        GameCore.Instance.StopTime();
        //        BattleSys battleSys = GameCore.Instance.SubsysMgr.GetNowSubSys() as BattleSys;
        //        //battleSys.State = BattleSysState.Pause;
        //    }, 5);
        //    return;
        //}


        //협공 end
        buffCtrl.DepplySpecialBuff(EffectType.FollowAtk);

        //반격
        if (buffCtrl.DepplySpecialBuff(EffectType.CounterAtk))
            return;

        // 팀 스킬
        if (QueueingTeamSkill)
        {
            QueueingTeamSkill = false;
            var tsKey = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(teamNumber);
            if (tsKey == 0)
            {
                Debug.Log("Cancel teamSkill by tsKey.");
                return;
            }
            if (ui.IsTeamDeath == true)
            {
                Debug.Log("Cancel teamSkill by Death Unit.");
                return;
            }

            //ui.ShowTeamSkillActiveCover(false);
            teamSkillDmgScale = (teamSkillAcc == 2f) ? consts.TeamDmgIncConst_2 : consts.TeamDmgIncConst_1;
            teamSkillAcc = teamSkillAcc % 1f;
            ui.SetTeamSkillGauge(teamSkillAcc);

            /*if(*/DoTeamSkill(teamNumber, tsKey, battleCount, true);
            Debug.Log("Do TeamSkill");
            return;
        }
        // 스트라이크 스킬
        if (QueueingStrikeSkill)
        {
            QueueingStrikeSkill = false;
            // 미구현
            DoStrikeSkill(friendTeamSkill);
            return;
        }


        // solo Skill 수동 스킬 조작시
        if (SkillQueue.Count != 0)
        {
            var unit = unitPool.GetUnitByFieldID(SkillQueue.Dequeue());
            if (DoSkill(unit))
            {
                // DisCount State buff
                unit.DisCountEffect(buffCtrl.CBRemoveBuffIcon);
                return;
            }
        }

        // solo skill 자동 스킬
        QueuingAutoSkill(GameCore.atuoPlay);
        while (autoSkillQueue.Count != 0)
        {
            var unit = unitPool.GetUnitByFieldID(autoSkillQueue[0]);
            autoSkillQueue.RemoveAt(0);
            
            if (unit != null && unit.Skillable)
            {
                DoSkill(unit);
                unit.DisCountEffect(buffCtrl.CBRemoveBuffIcon);
                return;
            }
        }        //if (GameCore.atuoPlay)
        //{
        //    // 스킬 쿨 전수 조사
        //    // 그중 첫번째 하나를 동작한다.

        //    //var unit = GetSkillAbleUnitInCombat();
        //    //if(unit != null)
        //    //    EnqSkill(unit.FieldId, -1);
        //    var unit = GetSkillAbleUnitInCombat();
        //    if (unit != null && unit.SkillData != null)
        //    {
        //        DoSkill(unit);

        //        // DisCount State buff
        //        unit.DisCountEffect(buffCtrl.CBRemoveBuffIcon);
        //        return;
        //    }
        //}


        BattleUnitData atkUnit;
        do
        {
            // 공격 큐가 비었다면 공격 큐 세팅
            if (attackQueue.Count == 0)
            {
                var cnt = unitPool.GetCombatUnitCount();
                for (int i = 0; i < cnt; ++i)
                    attackQueue.Push(unitPool.GetCombatUnit(i));
            }

            // Dequeue
            atkUnit = attackQueue.Pop();

        } while (atkUnit.IsAtattackSkip());


        //독뎀 적용점
        var poisonbuff = atkUnit.GetStateEffect(EffectType.HPPoison);
        if (poisonbuff != null)
            if (sys.DoDamage(null, atkUnit, atkUnit.GetStat(UnitStat.Hp) * poisonbuff.effect.buffData.value1 * 0.01f, DamageType.HPPoison))
                return;
        poisonbuff = atkUnit.GetStateEffect(EffectType.MTPoison);
        if (poisonbuff != null)
            if (sys.DoDamage(null, atkUnit, atkUnit.GetStat(UnitStat.Mental) * poisonbuff.effect.buffData.value1 * 0.01f, DamageType.MTPoison))
                return;

        // unit solo attack
        //일반 공격
        if (DoAttack(atkUnit, -1))
        {
        }

        // DisCount State buff
        atkUnit.DisCountEffect(buffCtrl.CBRemoveBuffIcon);
    }


    public void QueuingAutoSkill(bool _auto)
    {
        int cnt = unitPool.GetCombatUnitCount();
        for (int i = 0; i < cnt; ++i)
        {
            var unit = unitPool.GetCombatUnit(i);
            int fieldID = unit.FieldId; 
            bool mine = BattleUnitPool.GetTeamByFieldId(fieldID) == 0;

            if (!unit.Skillable) // 스킬을 쓸 수 없는 상태라면
                continue;
            else if (mine && !_auto)             // 내유닛 : 자동이 아닐때
                continue;
            else if (!mine && !canUseSkillEnmey) // 적유닛 : 적은 스킬을 쓸 수 없을때
                continue;

            if (SkillQueue.Contains(fieldID) || // 이미 스킬 큐에 등록 되어 있다면 패스
                autoSkillQueue.Contains(fieldID))
                continue;

            autoSkillQueue.Add(fieldID);
        }
    }


    public void SetNextRound()
    {
        //공격 순번 리셋
        attackQueue.Clear();
        //플레이어 팀 MP 충전
        for(int i = 0; i < 6; i++)
        {
            BattleUnitData unitData = unitPool.GetUnitInCombatByTeam(teamNumber, i);
            if (unitData == null)
                continue;
            unitData.AddMp(GameCore.Instance.PlayerDataMgr.GetBattleCommonData().MentalHeal);
        }
        
    }
    private BattleUnitData GetSkillAbleUnitInCombat()
    {
        int cnt = unitPool.GetCombatUnitCount();
        for (int i = 0; i < cnt; ++i)
        {
            var unit = unitPool.GetCombatUnit(i);

            if (!canUseSkillEnmey)
                if (BattleUnitPool.GetTeamByFieldId(unit.FieldId) != 0)
                    continue;
            if (unit.TeamSkillable == false)
                continue;
            if (unit.GetCoolTimeAcc() <= 0)
            {
                return unit;
            }
        }
        return null;
    }

    internal void EnqSkill(int _fieldId, int _tgFid)
    {
        //SkillQueue.Add(_fieldId, _tgFid);
        SkillQueue.Enqueue(_fieldId);
        autoSkillQueue.Remove(_fieldId);

        unitPool.GetUnitByFieldID(_fieldId).SetTarget(_tgFid);

        var cardUI = ui.GetUnitCardUI(_fieldId);
        if (cardUI != null)
            cardUI.SetFill();
    }


    /// <summary>
    /// 일반 공격을 시작한다.
    /// 타겟 결정 및 애니메이션 시작
    /// </summary>
    /// <param name="_unit">공격하려는 유닛</param>
    /// <returns>공격 여부</returns>
    public bool DoAttack(BattleUnitData _unit, int _fid)
    {
        // 공격을 막는 조건들
        if (_unit == null || !_unit.Attackable)
            return false;

        var opponentTeamIdx = BattleUnitPool.GetTeamByFieldId(_unit.FieldId) != 0 ? 0 : battleCount;
        
        if (_fid == -1)
        {
            buffCtrl.ApplySpecialBuff(EffectType.AggroCtrl, _unit.FieldId, -1);
            _fid = unitPool.GetAutoTargetIndex(opponentTeamIdx);
            buffCtrl.DepplySpecialBuff(EffectType.AggroCtrl);
        }

        var tgUnit = unitPool.GetUnitByFieldID(_fid);
        if (tgUnit == null)
        {
            Debug.LogError("tgUnit is Null!!");
            return false;
        }

        DamagePower atk, grd;
        float attackDamage = sys.CalcDamage(_unit, tgUnit,out atk,out grd);
        _unit.SetAttackDamage(atk, grd, attackDamage);

        
        _unit.SpineCtrl.Settarget(_fid);

        if (sys.coSpecialBuff != 0 || !buffCtrl.ApplySpecialBuff(EffectType.SetGuard, _fid, _unit.FieldId, false))
            unitPool.SetExtraAnimation(_unit.FieldId, (atk == DamagePower.Critical) ? SpineAnimation.ChargeAttack : SpineAnimation.Attack, 
                                                        _unit.Data.atkType == AttackType.Near || _unit.Data.atkType == AttackType.Sword);
        //if (atk == DamagePower.Critical)
            //battleField.ShowParticle(ParticleType.Charge, _unit);
        // 협공
        buffCtrl.ApplySpecialBuff(EffectType.FollowAtk, _unit.FieldId, _fid);
        //최초 공격 리셋
        _unit.AttackFirst = true;

        // 마이룸버프에 따라 회복
        HealFromMyRoomBuffAndItem(_unit);

        return true;
    }


    private void SetTargetColor(bool isActive, bool isTeam, params BattleUnitData[] _unit)
    {
        int countCombatUnits = unitPool.GetCombatUnitCount();
        for (int i = 0; i < countCombatUnits; i++)
        {
            unitPool.GetCombatUnit(i).SpineCtrl.SetSpineCharacterColor(isActive);
        }
        BattleUnitData unitData = _unit[0];
        TargetType effectType = TargetType.None;
        for (int n = 0; n < _unit.Length; n++)
        {
            if (isTeam) _unit[n].TeamSkillData.HitCountReset();
            else        _unit[n].SkillData.HitCountReset();
            var cnt = (isTeam) ? _unit[n].TeamSkillData.GetNowEffects(ref effects, false) : _unit[n].SkillData.GetNowEffects(ref effects, false);
            if (cnt == 999) continue;
            for (int i = 0; i < cnt; i++)
            {
                if ((effectType & effects[i].targetType) == 0)
                    effectType |= effects[i].targetType;
                //if ((int)effects[i].targetType > (int)effectType)
                //{
                //    unitData = _unit[n];
                //    effectType = effects[i].targetType;
                //}
            }
        }
        //첫번째 타겟 세팅후, 스킬에 맞게 타겟이 되는 대상들을 applyUnits에 저장
        applyUnits.Clear();
        //int target = unitData.GetTarget();
        if (skillTarget <= 0 || unitPool.IsInCombat(skillTarget) == false)
        {
            var opponentTeamIdx = BattleUnitPool.GetTeamByFieldId(unitData.FieldId) > 0 ? 0 : battleCount;
            skillTarget = unitPool.GetAutoTargetIndex(opponentTeamIdx);
        }
        //Debug.LogError("Set SkillTarget : " + skillTarget + "from SetTargetColor()");


        //팀스킬 시전쪽 칼라(흰색) 및 sortingOrder값 수정
        for (int i = 0; i < _unit.Length; i++)
        {
            _unit[i].SpineCtrl.SetSpineCharacterColor(false);
            //var tgUnit = unitPool.GetUnitByFieldID(target);
            //int sortingOrder = isActive ? tgUnit.SpineCtrl.GetSortingOrder() + 1 : (BattleUnitPool.GetLineByFieldId(_unit[i].FieldId) + 1) * 2;
            //_unit[i].SpineCtrl.SetSortingOrder(sortingOrder);
        }
        //SetUnitsSortingOrder(target, isActive, _unit);
        //공격 받는 대상 쪽 칼라 흰색으로 수정
        sys.GetTargetForColor(effectType, unitData.FieldId, skillTarget, ref applyUnits);
        for (int i = 0; i < applyUnits.Count; i++)
        {
            var tg = sys.GetUnitData(applyUnits[i]);
            tg.SpineCtrl.SetSpineCharacterColor(false);
        }

    }
    public void SetUnitsSortingOrder(int target, bool isActive, params BattleUnitData[] _unit)
    {
        for(int i= 0; i < _unit.Length; i++)
        {
            //_unit[i].SpineCtrl.SetSpineCharacterColor(false);
            var tgUnit = unitPool.GetUnitByFieldID(target);
            int sortingOrder = isActive ? tgUnit.SpineCtrl.GetSortingOrder() + 1 : (BattleUnitPool.GetLineByFieldId(_unit[i].FieldId) + 1) * 2;
            _unit[i].SpineCtrl.SetSortingOrder(sortingOrder);
        }
    }

    private float GetMentalCalculate(BattleUnitData _unit)
    {
        float nowMental = (float)Math.Round(_unit.GetStat(UnitStat.Mental) * 0.01f, 0);
        float mentalDecrease = GameCore.Instance.PlayerDataMgr.GetBattleCommonData().MetalDecrease;
        float resultMentalValue = mentalDecrease - nowMental;
        resultMentalValue = resultMentalValue + (resultMentalValue * _unit.mentalMultipleValue * 0.01f * -1f);
        return resultMentalValue;
    }
    private void SetVigorValue(BattleUnitData _unit)
    {
        BattleCommonDataMap battleCommonData = GameCore.Instance.PlayerDataMgr.GetBattleCommonData();
        float decreaseValue = Mathf.Min(    battleCommonData.VigorLimitDecreaseConst, 
                                            battleCommonData.VigorDefaultDecreaseConst - Mathf.Round(_unit.GetStat(UnitStat.Vigor) * 0.01f) - Mathf.Round(_unit.GetStat(UnitStat.Agility) * 0.01f));
        _unit.AddedStat.SetStat(UnitStat.Vigor, Mathf.Round(_unit.GetStat(UnitStat.Vigor) * decreaseValue * 0.01f * -1));
        _unit.CalcNowStat();
    }
    public bool DoSkill(BattleUnitData _unit)
    {
        if (_unit == null || _unit.IsDead() || !_unit.Skillable)
            return false;

        //GameCore.Instance.SaveTime();
        //ui.SetSpeedButton(false);

        //skillTarget = _unit.GetTarget();
        _unit.SetTarget(-1);
        
        _unit.SkillData.HitCountReset();
        var skill = _unit.SkillData;

        //Debug.Log("[" + _unit.FieldId + "]" + skill.Data.disc + "Skill : " + skill.Data.endCount);
        SetVigorValue(_unit);
        //_unit.AddedStat.SetStat(UnitStat.Vigor, _unit.AddedStat.GetStat(UnitStat.Vigor) - 200f);
        float mentalValue = GetMentalCalculate(_unit);
        _unit.AddMp(-mentalValue);
        BattleCommonDataMap battleCommonData = GameCore.Instance.PlayerDataMgr.GetBattleCommonData();
        
        var calcCollTime = (10f - (int)(_unit.NowMental / 10f)) * battleCommonData.CoolTimeIncrease;
        calcCollTime -= Mathf.Max(0f, (float)Mathf.Round(_unit.GetStat(UnitStat.Recovery) * 0.01f));

        // 마이룸 버프에 따라 후딜 감소
        if (_unit.myroomEffect.ContainsKey(MyRoomEffectType.DecCoolTime))
            calcCollTime *= 1f - _unit.myroomEffect[MyRoomEffectType.DecCoolTime];

        _unit.ResetSkillCool(Math.Min(calcCollTime, battleCommonData.MaxCoolTime));

        //if (skillTarget == -1 && unitPool.IsInCombat(skillTarget) == false)
        //{
        //    Debug.LogError("NEW");
        //    var opponentTeamIdx = BattleUnitPool.GetTeamByFieldId(_unit.FieldId) > 0 ? 0 : battleCount;
        //    buffCtrl.ApplySpecialBuff(EffectType.AggroCtrl, _unit.FieldId, -1);
        //    skillTarget = unitPool.GetAutoTargetIndex(opponentTeamIdx);

        //    buffCtrl.DepplySpecialBuff(EffectType.AggroCtrl);
        //}
        //Debug.LogError("Set SkillTarget : " + skillTarget + "from DoSkill()");

        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Skill_Start, false);
        if (GameCore.Instance.TimeSave < 2)
            GameCore.Instance.SoundMgr.SetCharacterSkillSound(_unit, true, false);
        if (BattleUnitPool.GetTeamByFieldId(_unit.FieldId) <= 0)
            ui.ShowSkillVisualEffect(_unit, false);
        else
            ui.ShowSkillVisualEffect(_unit, true);

        battleField.ShowBlander(true);

        SetTargetColor(true, false, _unit);

        _unit.SpineCtrl.Settarget(skillTarget);

        //if (target != -1)
        //    unitPool.GetUnitByFieldID(target).SpineCtrl.SetFrontSortingOrder();
        //_unit.SpineCtrl.SetAttackerFrontSortingOrder();

        battleField.StartCoroutine(sys.CoRunningWait(() =>
        {
            // SetTarget 특수버프
            if (!buffCtrl.ApplySpecialBuff(EffectType.SetGuard, skillTarget, _unit.FieldId, true))
                unitPool.SetExtraAnimation(_unit.FieldId, SpineAnimation.Skill, CheckMoveType(skill));
            //if (skill.Data.type != PositionType.Distance)
                //battleField.ShowParticle(ParticleType.Charge, _unit);
        }));
        battleField.StartCoroutine(sys.CoRunningWait(() =>
        {
            //GameCore.Instance.RevertTime();
            //ui.SetSpeedButton(true);
            SetTargetColor(false, false, _unit);
            if (bTutoFirstSkillUsed == false)
            {
                bTutoFirstSkillUsed = true;
                GameCore.Instance.TurnOnTutorial(() =>
                {
                    GameCore.Instance.StopTime();
                    //sys.State = BattleSysState.Pause;
                }, 1);
            }
        }));
        _unit.AddSkipAttackCount();

        // 마이룸 버프에따라 회복
        HealFromMyRoomBuffAndItem(_unit);

        return true;
    }
    internal bool DoStrikeSkill(int _tsKey)
    {
        if (ui.IsStrikeDeath == true)
            return false;
        var tsKey = _tsKey;
        //GameCore.Instance.SaveTime();
        //ui.SetSpeedButton(false);

        teamSkillUnitList.Clear();
        teamBehindUnitList.Clear();
        activeUnitId.Clear();
        string animationName;

        //내 캐릭터들은 전부 안보이기 위한 리스트로 추가
        for (int i = 0; i < 6; ++i)
        {
            var unit = unitPool.GetUnitInCombatByTeam(0, i);
            if (unit == null) continue;
            teamBehindUnitList.Add(unit);
        }

        if (tsKey == 0)
        {
            BattleUnitData unit = null;
            for(int i = 0; i < 6; i++)
            {
                BattleUnitData unitData = unitPool.GetFriendUnitByTeam(-1, i);
                if (unitData == null)
                    continue;
                unit = unitData;
                break;
            }
            if (unit == null)
                return false;
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_StrikeSkill_Start, false);
            if (GameCore.Instance.TimeSave < 2)
                GameCore.Instance.SoundMgr.SetCharacterSkillSound(unit, true, false);
            //ui.SetSpeedButton(false);

            //skillTarget = unit.GetTarget();
            unit.SetTarget(-1);

            unit.SkillData.HitCountReset();
            var skill = unit.SkillData;

            //if (skillTarget == -1 && unitPool.IsInCombat(skillTarget) == false)
            //{
            //    Debug.LogError("NEW");
            //    var opponentTeamIdx = BattleUnitPool.GetTeamByFieldId(unit.FieldId) > 0 ? 0 : battleCount;
            //    buffCtrl.ApplySpecialBuff(EffectType.AggroCtrl, unit.FieldId, -1);
            //    skillTarget = unitPool.GetAutoTargetIndex(opponentTeamIdx);

            //    buffCtrl.DepplySpecialBuff(EffectType.AggroCtrl);
            //}
            //Debug.LogError("Set SkillTarget : " + skillTarget + "from DoStrikeSkill()");
            SetTargetColor(true, false, unit);
            ui.TurnOnFriendSkillUI();
            battleField.ShowBlander(true);
            unit.SpineCtrl.Settarget(skillTarget);

            //if (target != -1)
            //    unitPool.GetUnitByFieldID(target).SpineCtrl.SetFrontSortingOrder();
            //unit.SpineCtrl.SetAttackerFrontSortingOrder();

            battleField.StartCoroutine(sys.WaitForUIAnimation(() =>
            {
                for (int i = 0; i < teamBehindUnitList.Count; i++)
                {
                    unitPool.GetUnitByFieldID(teamBehindUnitList[i].FieldId).SpineCtrl.isRunning = true;
                }
                SpineCharacterCtrl characterCtrl = unit.SpineCtrl;
                sys.ShowTeamUnit(unit, true, () =>
                {
                    characterCtrl.isRunning = false;
                    // SetTarget 특수버프
                    if (!buffCtrl.ApplySpecialBuff(EffectType.SetGuard, skillTarget, unit.FieldId, true))
                        unitPool.SetExtraAnimation(unit.FieldId, SpineAnimation.Skill, CheckMoveType(skill));
                    //if (skill.Data.type != PositionType.Distance)
                        //battleField.ShowParticle(ParticleType.Charge, unit);
                });
                unit.SpineCtrl.isRunning = true;

            }));
            battleField.StartCoroutine(sys.CoFriendRunningWait(() =>
            {
                SpineCharacterCtrl characterCtrl = unit.SpineCtrl;
                sys.ShowTeamUnit(unit, false, () => {
                    SetTargetColor(false, false, unit);
                    characterCtrl.isRunning = false;
                    //GameCore.Instance.RevertTime();
                    //ui.SetSpeedButton(true);
                    for (int i = 0; i < teamBehindUnitList.Count; i++)
                    {
                        unitPool.GetUnitByFieldID(teamBehindUnitList[i].FieldId).SpineCtrl.isRunning = false;
                    }
                    GameCore.Instance.TurnOnTutorial(() =>
                    {
                        bPauseTuto = true;
                        GameCore.Instance.StopTime();
                        //sys.State = BattleSysState.Pause;
                    }, 4, 5);
                });
                unit.SpineCtrl.isRunning = true;                
                ui.SetStrikeSkillGauge(StrikeSkillAcc);
                ui.ShowStrikeSkillActiveCover(false);
            }));

        }
        else if(tsKey != 0)
        {
            var teamSkillData = GameCore.Instance.DataMgr.GetTeamSkillData(tsKey);
            animationName = string.Format("Union_skill_{0:d3}", teamSkillData.skillNameNum);

            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_StrikeSkill_Start, false);
            //친구 캐릭터들을 skill사용 리스트에 추가
            for (int i = 0; i < 6; i++)
            {
                BattleUnitData unitData = unitPool.GetFriendUnitByTeam(-1, i);
                if (unitData == null)
                    continue;
                //친구 캐릭터중 teamskill정보가 있는 캐릭터만 추가
                if (unitData.TeamSkillData != null)
                {
                    if (GameCore.Instance.TimeSave < 2)
                        GameCore.Instance.SoundMgr.SetCharacterSkillSound(unitData, true, true);
                    unitData.Transform.localPosition = new Vector3(-7f, 6f, 0f);
                    teamSkillUnitList.Add(unitData);
                    activeUnitId.Add(unitData.Data.charIdType);
                }
            }
            SetTargetColor(true, true, teamSkillUnitList.ToArray());
            //팀스킬 제목 보이는 이펙트 함수
            ui.TurnOnFriendSkillUI();
            battleField.ShowBlander(true);
            //제목 보이는 이펙트 이후 코루틴
            battleField.StartCoroutine(sys.WaitForUIAnimation(() =>
            {
                BeforeTeamSkill(true);
                //친구 캐릭터 이동이 끝난 이후 스킬 액션을 실행시키기 위한 코루틴
                battleField.StartCoroutine(sys.CoFriendSkill(() =>
                {
                    //팀스킬 사용
                    TeamSkill(animationName, true);
                    //팀스킬의 시간을 측정하여 해당 시간 이후 빠지는 액션을 실행시키기 위한 코루틴
                    AfterTeamSkill(true, animationName);
                }));
                //sys.GetTarget(teamSkillUnitList[0].TeamSkillData.Data.effects);

            }));
        }

        StrikeSkillAcc = 0f;
        return true;
    }
    private void TeamSkill(string animationName, bool isStrike)
    {
        //var opponentTeamIdx = BattleUnitPool.GetTeamByFieldId(teamSkillUnitList[0].FieldId) != ((isStrike) ? -1 : 0) ? 0 : battleCount;
        //teamSkillTarget = unitPool.GetAutoTargetIndex(opponentTeamIdx);
        for (int i = 0; i < teamSkillUnitList.Count; i++)
        {
            var unit = teamSkillUnitList[i];
            //int target = unit.GetTarget();
            //unit.SetTarget(-1);
            //unit.SkillData.HitCountReset();

            //if (tgFid == -1)// && target == -1 && unitPool.IsInCombat(target) == false)
            //{
            //    var opponentTeamIdx = BattleUnitPool.GetTeamByFieldId(unit.FieldId) != ((isStrike) ? -1 : 0) ? 0 : battleCount;
            //    buffCtrl.ApplySpecialBuff(EffectType.AggroCtrl, unit.FieldId, ((isStrike) ? -1 : 0));
            //    //target = unitPool.GetAutoTargetIndex(opponentTeamIdx);
            //    tgFid = unitPool.GetAutoTargetIndex(opponentTeamIdx);

            //    buffCtrl.DepplySpecialBuff(EffectType.AggroCtrl);
            //}
            unit.SpineCtrl.Settarget(skillTarget);
            //if (target != -1)
            //    unitPool.GetUnitByFieldID(target).SpineCtrl.SetFrontSortingOrder();
            //unit.SpineCtrl.SetAttackerFrontSortingOrder();

            BattleSkill teamSkill = unit.TeamSkillData;
            teamSkill.HitCountReset();
            if (isStrike == true)
                unitPool.SetStrikeExtraAnimation(unit.FieldId, SpineAnimation.TeamSkill, animationName, CheckMoveType(teamSkill));
            else
                unitPool.SetTeamExtraAnimation(unit.FieldId, SpineAnimation.TeamSkill, animationName, CheckMoveType(teamSkill));
        }
    }
    internal bool CheckMoveType(BattleSkill teamSkill)
    {
        switch(teamSkill.Data.type)
        {
            case PositionType.NearJump:
            case PositionType.NearStraight:
            case PositionType.EnemyFront:
            case PositionType.EnemyCenter:
            case PositionType.TeamFront:
            case PositionType.TeamBack:
                return true;
        
            default: return false;
        }
    }
    private void BeforeTeamSkill(bool isStrike)
    {
        if (isStrike == true)
            //친구 팀스킬 멤버들을 화면으로 이동
            for (int i = 0; i < teamSkillUnitList.Count; i++)
            {
                //isRunning이 true일 경우 캐릭터들이 다음 행동을 하지 않으므로 이동이 끝난 후 행동하게 하기위한 조치
                SpineCharacterCtrl characterCtrl = teamSkillUnitList[i].SpineCtrl;
                sys.ShowTeamUnit(teamSkillUnitList[i], true, () => { characterCtrl.isRunning = false; });
                teamSkillUnitList[i].SpineCtrl.isRunning = true;
            }
        //내 캐릭터들을 화면 밖으로 이동
        for (int i = 0; i < teamBehindUnitList.Count; i++)
        {
            sys.ShowTeamUnit(teamBehindUnitList[i], false, () => { });
            unitPool.GetUnitByFieldID(teamBehindUnitList[i].FieldId).SpineCtrl.isRunning = true;
        }
    }
    private void AfterTeamSkill(bool isStrike, string animationName)
    {
        float durationTime = 0;
        for(int i = 0; i < teamSkillUnitList.Count; i++)
        {
            float nowDuration = teamSkillUnitList[i].SpineCtrl.skelAnim.SkeletonDataAsset.GetSkeletonData(false).FindAnimation(animationName).duration;
            if (durationTime < nowDuration) durationTime = nowDuration;
        }
        battleField.StartCoroutine(GameCore.WaitForTime(durationTime, () =>
        {
            SetTargetColor(false, true, teamSkillUnitList.ToArray());
            if (isStrike)
            {
                for (int i = 0; i < teamSkillUnitList.Count; i++)
                {
                    SpineCharacterCtrl characterCtrl = teamSkillUnitList[i].SpineCtrl;
                    sys.ShowTeamUnit(teamSkillUnitList[i], false, () => { characterCtrl.isRunning = false; });
                    teamSkillUnitList[i].SpineCtrl.isRunning = true;
                }
                StrikeSkillAcc = 0f;
                ui.SetStrikeSkillGauge(StrikeSkillAcc);
                ui.ShowStrikeSkillActiveCover(false);
                ui.isStrikeFull = false;
            }
            else
            {
                    sys.OffTeamSkillCover();
            }
            for (int i = 0; i < teamBehindUnitList.Count; i++)
            {
                SpineCharacterCtrl characterCtrl = teamBehindUnitList[i].SpineCtrl;
                sys.ShowTeamUnit(teamBehindUnitList[i], true, () => { characterCtrl.isRunning = false; });
            }
            //GameCore.Instance.RevertTime();
            if (GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning)
            {
                if ((isStrike && GameCore.Instance.lobbyTutorial.tutorialPos == 5) ||
                    (!isStrike && GameCore.Instance.lobbyTutorial.tutorialPos == 4))
                {
                    if (checkOnceForTutorialSkill == false)
                    {
                        checkOnceForTutorialSkill = true;
                        GameCore.Instance.TurnOnTutorial(() =>
                        {
                            bPauseTuto = true;
                            GameCore.Instance.StopTime();
                        //sys.State = BattleSysState.Pause;
                    }, !isStrike ? 4 : 5);//, 5);
                    }
                }
            }
            //ui.SetSpeedButton(true);
        }));

        skillTarget = -1;
    }
    internal bool DoTeamSkill(int _teamNum, int _tsKey, int _tgTeamNum, bool isPlayerTeam)
    {
        var tsKey = _tsKey;
        if (tsKey == 0)
            return false;
        //GameCore.Instance.SaveTime();
        //ui.SetSpeedButton(false);

        teamSkillUnitList.Clear();
        teamBehindUnitList.Clear();
        activeUnitId.Clear();

        var teamSkillData = GameCore.Instance.DataMgr.GetTeamSkillData(tsKey);
        string animationName = string.Format("Union_skill_{0:d3}", teamSkillData.skillNameNum);

        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_TeamSkill_Start, false);
        for (int i = 0; i < 6; ++i)
        {
            var unit = unitPool.GetUnitInCombatByTeam((isPlayerTeam)?0 : _teamNum, i);
            if (unit == null) continue;

            BattleSkill teamSkill = unit.TeamSkillData;
            if (teamSkill != null)
            {
                if (unit.IsDead() == true)
                {
                    teamSkillUnitList.Clear();
                    return false;
                }

                teamSkill.HitCountReset();
                //! 상태이상 무시 // if (!unit.TeamSkillable) return false;
                if (GameCore.Instance.TimeSave < 2)
                    GameCore.Instance.SoundMgr.SetCharacterSkillSound(unit, true, true);
                teamSkillUnitList.Add(unit);
                activeUnitId.Add(unit.Data.id);
            }
            else
            {
                teamBehindUnitList.Add(unit);
            }
        }
        SetTargetColor(true, true, teamSkillUnitList.ToArray());
        ui.ShowTeamSkillVisualEffect(teamSkillData.name, activeUnitId.ToArray());
        battleField.ShowBlander(true);
        battleField.StartCoroutine(sys.WaitForUIAnimation(() =>
        {
            BeforeTeamSkill(false);
            TeamSkill(animationName, false);
            AfterTeamSkill(false, animationName);
            ui.SetTeamSkillGaugeBool(teamSkillAcc);
        }));
        return true;
    }


    void HealFromMyRoomBuffAndItem(BattleUnitData _unit)
    {
        float heal = 0;

        // 마이룸 버프 힐
        if (_unit.myroomEffect.ContainsKey(MyRoomEffectType.AutoHeal))
            heal += _unit.GetStat(UnitStat.Hp) * (1f - _unit.myroomEffect[MyRoomEffectType.AutoHeal]);

        // 아이템 치유 효과
        heal += _unit.GetStat(UnitStat.Hp) * _unit.autoRecovery * 0.01f;


        if (heal != 0)
            sys.DoDamage(null, _unit, heal, DamageType.HPHeal);
    }


    /// <summary>
    /// 따라 공격이 동작하는 루틴. 일정 시간 지연 후 공격을 한다.
    /// 대상이 사망할 경우 사망애니메이션 중에 공격하는데,
    /// 따로 처리하지 않고 지연 시간 후 무조건 공격한다.
    /// 만약 사망애니메이션이 끝나 캐릭터가 삭제되면 공격을 할 수 없다.
    /// </summary>
    /// <param name="_unit"></param>
    /// <param name="_fid"></param>
    /// <param name="_time"></param>
    /// <returns></returns>
    public IEnumerator CoDoFollowAttack(BattleUnitData _unit, int _fid, float _time)
    {
        yield return new WaitForSeconds(_time);
        ui.ShowEffectAlert(_unit.GetWorldPosition(), EffectType.FollowAtk, 0);
        DoAttack(_unit, _fid);
        //DepplySpecialBuff(EffectType.FollowAtk, _unit.FieldId);
    }

    /// <summary>
    /// 공격 순서 정렬
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int Compare(BattleUnitData _x, BattleUnitData _y)
    {
        return _x.GetStat(UnitStat.Vigor) < _y.GetStat(UnitStat.Vigor) ? 1 : -1;
    }
}
