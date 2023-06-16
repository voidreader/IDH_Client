using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidRewardGradeComponent : MonoBehaviour
{
    [SerializeField] RaidRewardGradeScript[] items;
    [SerializeField] UILabel lbTotalDamage;

    int damage = 0;

    internal void Init(int _key)
    {
        Debug.Log("Key : "+ _key);
        var data = GameCore.Instance.DataMgr.GetRaidData(_key);
        damage = GameCore.Instance.PlayerDataMgr.GetRaidSData(_key).damage;

        int itemKey;
        int itemCount;
        int value = 0;


        for (int i = 0; i < items.Length-1; ++i)
        {
            itemKey = data.accumRwds[i];
            itemCount = data.accumRwdCounts[i];

            items[i].SetData(itemKey, itemCount, value, data.accumDmgs[i]);

            value = data.accumDmgs[i];
        }

        itemKey = data.accumRwds[items.Length-1];
        itemCount = data.accumRwdCounts[items.Length-1];
        items[items.Length-1].SetData(itemKey, itemCount, value, data.accumDmgs[items.Length-1], "CLEAR");

        SetDamage(damage);
    }

    public void SetDamage(int _dmg)
    {
        damage = _dmg;
        lbTotalDamage.text = damage.ToString("N0");
        for (int i = 0; i < items.Length ; ++i)
            items[i].SetDmg(damage);
    }

    public void AddDamage(int _addDmg)
    {
        SetDamage(damage + _addDmg);
    }
}
