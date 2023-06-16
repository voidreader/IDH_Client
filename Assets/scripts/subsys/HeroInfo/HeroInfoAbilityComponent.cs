using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoAbilityComponent : MonoBehaviour
{
    [SerializeField] GameObject[] specializations;
    [SerializeField] UILabel[] heroValues;
    [SerializeField] UILabel[] itemValues;
    [SerializeField] UILabel lbSkill;
    [SerializeField] UILabel lbSkillDescription;
    internal void Init(HeroSData _sdata)
    {
        // 강화에 대한게 암것도 없음
        // 특화도.

        var data = GameCore.Instance.DataMgr.GetUnitData(_sdata.key);
        //var strengthenData = GameCore.Instance.DataMgr.GetStrengthenData(data.charType);
        var baseStat = _sdata.GetBaseStat();
        var itemStat = _sdata.GetItemStat(baseStat);
        for (int i = 1; i < (int)UnitStat.Count - 1; ++i) // 어그로 제외
        { 
            heroValues[i-1].text = string.Format("{0:N0}", baseStat.GetStat((UnitStat)i));
            itemValues[i-1].text = string.Format("+{0:N0}", itemStat.GetStat((UnitStat)i));
        }

        // 개인 스킬
        var dataMgr = GameCore.Instance.DataMgr;
        lbSkill.text = dataMgr.GetSkillData(dataMgr.GetUnitData(_sdata.key).skillId).name;
        lbSkillDescription.text = dataMgr.GetSkillData(dataMgr.GetUnitData(_sdata.key).skillId).disc;
    }
}
