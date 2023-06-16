using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BattleBuffCtrl
{
    BattleUnitPool unitPool;
    BattleUIBase ui;
    BattleAttackCtrl atkCtrl;
    BattleSysBase sys;

    Dictionary<EffectType, List<long>> activeBuffUnits = new Dictionary<EffectType, List<long>>(); // 일반 버프 이외에 활성화된 버프가 있는 유닛 정보( selfFid << 32 | needFid )


    public void Init(BattleUnitPool _pool, BattleUIBase _ui, BattleAttackCtrl _atkCtrl)
    {
        unitPool = _pool;
        ui = _ui;
        atkCtrl = _atkCtrl;

        sys = GameCore.Instance.SubsysMgr.GetNowSubSys() as BattleSysBase;
    }

    public void Release()
    {
        unitPool = null;
        ui = null;
        atkCtrl = null;
    }

    /// <summary>
    /// InCombat에 존재하는 유닛들에게 캐미스트리 버프를 찾아 부여한다.
    /// </summary>
    /// <param name="_fieldId">InCombat에 추가되는 유닛의 필드 아이디</param>
    internal void CBSetChemiBuff(int _fieldId)
    {
        var newUnit = unitPool.GetUnitByFieldID(_fieldId);

        var cnt = unitPool.GetCombatUnitCount();
        for (int i = 0; i < cnt; ++i)
        {
            var unit = unitPool.GetCombatUnit(i);
            var bTeam = BattleUnitPool.GetTeamByFieldId(newUnit.FieldId) == BattleUnitPool.GetTeamByFieldId(unit.FieldId);

            // 새로운 유닛이 버프 받기
            var chemiData = GameCore.Instance.DataMgr.GetchemistryDataInFindMap(newUnit.Data.charIdType, unit.Data.charIdType);
            if (chemiData != null)
            {
                //AddBuffs(newUnit.FieldId, unit.FieldId, chemiData.anywayBuffIds);
                if (bTeam)  AddChamiBuffs(newUnit.FieldId, unit.FieldId, chemiData.teamBuffIds);
                else        AddChamiBuffs(newUnit.FieldId, unit.FieldId, chemiData.oppositeBuffIds);
            }

            // 기존 유닛이 버프 받기
            chemiData = GameCore.Instance.DataMgr.GetchemistryDataInFindMap(unit.Data.charIdType, newUnit.Data.charIdType);
            if (chemiData != null)
            {
                //AddBuffs(unit.FieldId, newUnit.FieldId, chemiData.anywayBuffIds);
                if (bTeam)  AddChamiBuffs(unit.FieldId, newUnit.FieldId, chemiData.teamBuffIds);
                else        AddChamiBuffs(unit.FieldId, newUnit.FieldId, chemiData.oppositeBuffIds);
            }
        }
    }


    /// <summary>
    /// 현재 전투중인 모든 유닛의 특정 타입의 버프를 모두 제거한다.
    /// </summary>
    /// <param name="_type">영향을 주던 유닛의 필드 아이디</param>
    internal void RemoveChemiBuffs(int _fieldId)
    {
        var cnt = unitPool.GetCombatUnitCount();
        for (int i = 0; i < cnt; ++i)
        {
            var unit = unitPool.GetCombatUnit(i);
            unit.RemoveChemiByFieldId(_fieldId, CBRemoveBuffIcon);
        }
    }

    private void AddChamiBuffs(int _fieldId, int _needFId, int[] buffIds)
    {
        if (buffIds == null)
            return;
        Array.Sort(buffIds);
        for (int i = 0; i < buffIds.Length; i++)
        {
            if (buffIds[i] == -1)
                continue;

            var effect = GameCore.Instance.DataMgr.GetEffectData(buffIds[i]);
            BattleEffect be = new BattleEffect(effect);
            if (be.type == BattleEffectType.Buff)
                be.type = BattleEffectType.NormalChemi;
            be.continuance = -1;
            AddBuff(_fieldId, _needFId, be);
        }
    }

    /// <summary>
    /// 유닛에게 버프를 적용한다.
    /// </summary>
    /// <param name="_fieldId">버프를 받을 유닛의 필드 아이디</param>
    /// <param name="_needFid">캐미를 받기위해 필요한 유닛의 필드 아이디</param>
    /// <param name="_bb">적용될 버프</param>
    internal void AddBuff(int _fieldId, int _needFid, BattleEffect _bb)
    {
        var statusUI = ui.GetUnitStatusUI(_fieldId);
        var cardUI = ui.GetUnitCardUI(_fieldId);
        var unit = unitPool.GetUnitByFieldID(_fieldId);
        if (unit == null) unit = unitPool.GetFriendUnitByFieldID(_fieldId);

        switch (_bb.type)
        {
            case BattleEffectType.Buff:
            case BattleEffectType.StateBuff:
                unit.AddEffect(_bb);
                break;

            case BattleEffectType.NormalChemi:
            case BattleEffectType.SpecialChemi:
            case BattleEffectType.RSA_Chemi:
                unit.AddChemi(_bb, _needFid);
                break;

            default: return;
        }

        // 버프 아이콘 추가 콜백
        AddBuffIcon(_fieldId, _bb);

        // set UI
        if (cardUI != null)
        {
            cardUI.ResetHp(unit.NowHp, unit.GetStat(UnitStat.Hp));
        }

        if (statusUI != null)
        {
            statusUI.ResetHp(unit.NowHp, unit.GetStat(UnitStat.Hp));
            statusUI.ResetMantal(unit.NowMental, 100);
            //statusUI.ResetMantal(unit.NowMental, unit.GetStat(UnitStat.Mental));
        }
    }

    internal void AddBuffIcon(int _tgFid, BattleEffect _effect)
    {
        switch (_effect.type)
        {
            case BattleEffectType.Buff:
            case BattleEffectType.NormalChemi:
                var statType = _effect.effect.buffData.tgStat;
                int value1 = (int)Math.Ceiling(_effect.effect.buffData.value1 / 5); // 버프아이콘 값 표시 설정

                if (ui.GetUnitStatusUI(_tgFid) != null)
                    ui.GetUnitStatusUI(_tgFid).AddStatBuff(statType, value1);
                return;

            case BattleEffectType.StateBuff:
                var stateType = _effect.effect.type;
                int value2 = (int)_effect.continuance;

                if (ui.GetUnitStatusUI(_tgFid) != null)
                    ui.GetUnitStatusUI(_tgFid).SetStateBuff(stateType, value2);

                var cardUI = ui.GetUnitCardUI(_tgFid);
                if (cardUI != null && _effect.type == BattleEffectType.StateBuff)
                    cardUI.UpdateState();
                return;

            default:
                return;
        }
    }

    /// <summary>
    /// 대상 유닛의 버프 중 특정 유닛의 영향으로 적용된 버프를 모두 삭제한다.
    /// </summary>
    /// <param name="_tgFieldId">버프가 삭제될 대상 유닛의 필드 아이디</param>
    /// <param name="_needFiledId">버프를 적용하도록 영향을 준 유닛의 필드 아이디</param>
    internal void RemoveBuff(int _tgFieldId, int _needFiledId)
    {
        var unit = unitPool.GetUnitByFieldID(_tgFieldId);

        // Remove Buff
        unit.RemoveChemiByFieldId(_needFiledId, CBRemoveBuffIcon);

        // Set StatusUI
        var statusUI = ui.GetUnitStatusUI(_tgFieldId);
        if (statusUI != null)
        {
            //remove Buff Icon
            //for (int i = 0; i < removedBuffIconIds.Length; ++i)
            //	if (removedBuffIconIds[i] != -1)
            //		statusUI.RemoveBuff(removedBuffIconIds[i]);

            statusUI.ResetHp(unit.NowHp, unit.GetStat(UnitStat.Hp));
            statusUI.ResetMantal(unit.NowMental, 100);
            //statusUI.ResetMantal(unit.NowMental, unit.GetStat(UnitStat.Mental));
        }

        // set CardUI
        var cardUI = ui.GetUnitCardUI(_tgFieldId);
        if (cardUI != null)
            cardUI.ResetHp(unit.NowHp, unit.GetStat(UnitStat.Hp));
    }

    internal void CBRemoveBuffIcon(int _tgFid, BattleEffect _effect)
    {
        switch (_effect.type)
        {
            case BattleEffectType.Buff:
            case BattleEffectType.NormalChemi:
                var statType = _effect.effect.buffData.tgStat;
                int value1 = (int)_effect.effect.buffData.value1 / 5;

                ui.GetUnitStatusUI(_tgFid).AddStatBuff(statType, -value1);
                break;

            case BattleEffectType.StateBuff:
                var stateType = _effect.effect.type;
                int value2 = (int)_effect.continuance;

                ui.GetUnitStatusUI(_tgFid).SetStateBuff(stateType, value2);
                break;

            default:
                break;
        }

        // set CardUI
        var unit = unitPool.GetUnitByFieldID(_tgFid);
        var cardUI = ui.GetUnitCardUI(_tgFid);
        if (cardUI != null)
        {
            cardUI.ResetHp(unit.NowHp, unit.GetStat(UnitStat.Hp));
            if (_effect.type == BattleEffectType.StateBuff)
                cardUI.UpdateState();
        }
    }

    /// <summary>
    /// 스페셜 버프 활성화 리스트에 유닛 추가
    /// </summary>
    /// <param name="_type">버프 타입</param>
    /// <param name="_fieldId">유닛의 필드 아이디</param>
    private void AddActiveBuffUnit(EffectType _type, int _fieldId, int needFid)
    {
        long id = ((long)_fieldId << 32) + (long)needFid;
        if (!activeBuffUnits.ContainsKey(_type))
            activeBuffUnits.Add(_type, new List<long>());

        if (activeBuffUnits[_type].Contains(id))
            return;

        activeBuffUnits[_type].Add(id);
    }

    internal int GetActiveBuffCount()
    {
        return activeBuffUnits.Count;
    }

    /// <summary>
    /// 특수버프 활성화
    /// </summary>
    /// <param name="_type">활성화할 버프 타입</param>
    /// <param name="_needFieldId">화성화할 버프의 필요 유닛의 필드 아이디</param>
    /// <param name="_fid">타입에 따라 사용되는 변수. FollowAttack : 공격 당하는 유닛, SetGuard : 공격하는 유닛</param> 
    /// <param name="_skill">타입에 따라 사용되는 변수. SetGuard : 공격할 유형</param> 
    /// <returns>특수 루틴이 실행될경우 true</returns>
    internal bool ApplySpecialBuff(EffectType _type, int _needFieldId, int _fid, bool _skill = false)
    {
        if (_type <= EffectType.SpecialBuff || EffectType.StateBuff <= _type)
        {
            //Debug.LogError("특수 버프 타입이 아닙니다." + _type);
            return false;
        }

        if (_type == EffectType.RandStateAtk)
            return ApplyRSAChemi(_needFieldId, _fid);

        var cnt = unitPool.GetCombatUnitCount();
        int followAtkCount = 0;
        for (int i = 0; i < cnt; ++i)
        {
            var unit = unitPool.GetCombatUnit(i);
            if (unit != null)
            {
                if (unit.SetApplySpecialChemi(_type, _needFieldId, true))
                {
                    

                    switch (_type)
                    {
                        case EffectType.FollowAtk:
                            if (unit.Attackable)
                            {
                                unit.SpineCtrl.StartCoroutine(atkCtrl.CoDoFollowAttack(unit, _fid, (++followAtkCount * 0.4f) / GameCore.timeScale));
                                AddActiveBuffUnit(_type, unit.FieldId, _needFieldId); // 동작중인 스페셜버프 리스트에 등록
                            }
                            else
                            {
                                // Can't Attack
                            }
                            break;

                        case EffectType.AggroCtrl:
                            AddActiveBuffUnit(_type, unit.FieldId, _needFieldId); // 동작중인 스페셜버프 리스트에 등록
                            // do nothing
                            //return false;
                            break;

                        case EffectType.SetGuard:
                            unit.SpineCtrl.StartCoroutine(sys.CoSpecialBuff( ()=> CoDoGuard(_needFieldId, unit.FieldId, _fid, _skill) ));
                            AddActiveBuffUnit(_type, unit.FieldId, _needFieldId); // 동작중인 스페셜버프 리스트에 등록
                            return true;

                        case EffectType.CounterAtk:
                            unit.SpineCtrl.Settarget(_fid);
                            AddActiveBuffUnit(_type, unit.FieldId, _needFieldId); // 동작중인 스페셜버프 리스트에 등록0
                            break;//return false;

                        // Todo : 특수 버프 추가시 (적용설정 분기)
                    }
                }
            }

        }
        return false;
    }


    /// <summary>
    /// 확률 상태이상 공격 적용 함수
    /// </summary>
    /// <param name="_atkFid"></param>
    /// <param name="_tgFid"></param>
    /// <returns></returns>
    internal bool ApplyRSAChemi(int _atkFid, int _tgFid)
    {
        var atkUnit = unitPool.GetUnitByFieldID(_atkFid);
        var tgUnit = unitPool.GetUnitByFieldID(_tgFid);

        var emtr = atkUnit.ChemisEnumerator;
        while (emtr.MoveNext())
        {
            var list = emtr.Current.Value;
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].type == BattleEffectType.RSA_Chemi)
                {
                    if (list[i].effect.buffData.value1 > UnityEngine.Random.value * 100)
                    {
                        //apply
                        var type = list[i].effect.type + 50;
                        var effect = new BattleEffect(type, list[i].effect.buffData.value2, list[i].effect.count);
                        tgUnit.AddEffect(effect);

                        AddBuffIcon(_tgFid, effect);
                    }
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 활성화된 특수 버프 끄기
    /// </summary>
    /// <param name="_type">비활성화 할 버프 타입</param>
    /// <returns>특수버프 발동시 true</returns>
    internal bool DepplySpecialBuff(EffectType _type)
    {
        if (!activeBuffUnits.ContainsKey(_type))
            return false;

        var list = activeBuffUnits[_type];
        while (0 < list.Count)
        {
            int fid = (int)(list[0] >> 32);
            int need = (int)(list[0] & 0xFFFFFFFF);

            var unit = unitPool.GetUnitByFieldID(fid);
            if (unit == null || unit.IsDead()) // 이미 죽어서 제외된 유닛이라면
            {
                list.RemoveAt(0);
                continue;
            }
            
            switch (_type)
            {
                case EffectType.CounterAtk:
                    if (atkCtrl.DoAttack(unit, unit.SpineCtrl.TargetFid))
                    {
                        ui.ShowEffectAlert(unit.GetWorldPosition(), EffectType.CounterAtk, 0);
                        return true;
                    }
                    else
                    {
                        unit.SetApplySpecialChemi(_type, need, false);
                        list.RemoveAt(0);
                        break;
                    }

                default:
                    unit.SetApplySpecialChemi(_type, need, false);
                    list.RemoveAt(0);
                    break;
            }
        }
        return false;
    }

    /// <summary>
    /// 특정 유닛의 특정 버프만 끈다.
    /// </summary>
    /// <param name="_type">off 할 버프 타입</param>
    /// <param name="_fieldId">버프가 꺼질 유닛의 필드 아이디</param>
    internal bool DepplySpecialBuff(EffectType _type, int _fieldId)
    {
        if (!activeBuffUnits.ContainsKey(_type))
            return false;

        var list = activeBuffUnits[_type];
        for (int i = 0; i < list.Count; ++i)
        {
            int fid = (int)(list[i] >> 32);
            int need = (int)(list[i] & 0xFFFFFFFF);
            if (fid == _fieldId)
            {
                list.RemoveAt(i);
                var unit = unitPool.GetUnitByFieldID(fid);
                unit.SetApplySpecialChemi(_type, need, false);

                //switch (_type)
                //{
                //    case EffectType.CounterAtk:
                //        atkCtrl.DoAttack(unit, unit.SpineCtrl.TargetFid);
                //        return true;
                //}
                //break;
            }
        }
        return false;
    }

    internal IEnumerator CoDoGuard(int _unit, int _guarder, int _atker, bool _skill)
    {
        var unit = unitPool.GetUnitByFieldID(_unit);
        var grder = unitPool.GetUnitByFieldID(_guarder);
        var atker = unitPool.GetUnitByFieldID(_atker);

        ui.ShowEffectAlert(grder.GetWorldPosition(), EffectType.SetGuard, 0);

        //	Guard move
        grder.SpineCtrl.Settarget(_unit);
        //grder.SpineCtrl.SetFrontSortingOrder();
        //atker.SpineCtrl.SetFrontSortingOrder();
        unitPool.SetExtraAnimation(_guarder, SpineAnimation.Move, true);
        //var duration = grder.SpineCtrl.SkelAnim.AnimationState.Tracks.Items[0].Animation.duration;
        //MoveToFace(_guarder, _unit, _guarder < _unit);

        while (unitPool.CheckRunning())
            yield return null;
        //yield return new WaitForSeconds(MoveDelay / 2 / GameCore.timeScale);

        // Atker attk
        atker.SpineCtrl.Settarget(_guarder);
        if (_skill)
            unitPool.SetExtraAnimation(_atker, SpineAnimation.Skill, atkCtrl.CheckMoveType(atker.SkillData));
        else
            unitPool.SetExtraAnimation(_atker, SpineAnimation.Attack, atker.Data.atkType == AttackType.Near || atker.Data.atkType == AttackType.Sword);

        while (unitPool.CheckRunning() && !grder.IsDead())
            yield return null;

        // guard move
        if (!grder.IsDead())
        {
            unitPool.SetExtraAnimation(_guarder, SpineAnimation.Move, false);

            //while (unitPool.CheckRunning())
            //	yield return null;
        }

        //atker.SpineCtrl.SetDefSortingOrder();
        DepplySpecialBuff(EffectType.SetGuard);
    }
}
