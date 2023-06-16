using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class EquipItemDesc : MonoBehaviour
{
    [SerializeField] GameObject goSelectGuide;
    [SerializeField] UILabel lbInfo;
    [SerializeField] UILabel[] lbFixedOptionName;
    [SerializeField] UILabel[] lbFixedOptionValue;

    ItemSData sdata;

    public void Clear()
    {
        goSelectGuide.SetActive(true);
        lbInfo.text = string.Empty;
        for (int i = 0; i < lbFixedOptionName.Length; ++i)
        {
            lbFixedOptionName[i].text = string.Empty;
            lbFixedOptionValue[i].text = string.Empty;
        }
    }

    public void SetData(ItemSData _sdata)
    {
        sdata = _sdata;
        if(_sdata == null)
        {
            Clear();
            return;
        }

        goSelectGuide.SetActive(false);

        var data = GameCore.Instance.DataMgr.GetItemData(_sdata.key);
        lbInfo.text = string.Format("{0}등급 {1} 장비 / {2}", CardDataMap.GetStrRank(data.rank), 
                                                               CardDataMap.GetStrType(data.equipLimit), 
                                                               ItemSData.GetPrefixOptionString(_sdata.prefixIdx, _sdata.prefixValue));

        if (_sdata.optionIdx != null)
        {
            int idx = 0;
            for (int i = 0; i < _sdata.optionIdx.Length; ++i)
            {
                if (_sdata.optionIdx[i] <= 0)
                    continue;    

                var effectData = GameCore.Instance.DataMgr.GetItemEffectData(_sdata.optionIdx[i]);
                lbFixedOptionName[idx].text = ItemSData.GetItemEffectString(effectData.effectType);
                lbFixedOptionValue[idx].text = string.Format("{0:N0}{1}", _sdata.optionValue[i], effectData.type == 0 ? "" : "%");
                idx++;
            }

            for (int i = 0; i < lbFixedOptionName.Length; ++i)
            {
                lbFixedOptionName[i].gameObject.SetActive(i < idx);
                lbFixedOptionValue[i].gameObject.SetActive(i < idx);
            }

            lbFixedOptionName[0].transform.parent.GetComponent<UIGrid>().enabled = true;
        }
    }
         
}
