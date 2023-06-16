using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HeroInfoCamiSkillListItem : MonoBehaviour
{
    public static HeroInfoCamiSkillListItem Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("HeroInfo/CamiSkillListItem", _parent);
        return go.GetComponent<HeroInfoCamiSkillListItem>();
    }

    [SerializeField] Transform cardRoot;
    [SerializeField] GameObject haveIcon;

    [SerializeField] UILabel lbName;
    [SerializeField] UIGrid grid;
    [SerializeField] UILabel lbOptionBuff;
    [SerializeField] UILabel lbOptionDebuff;

    CardBase card;

    internal void Init(ChemistryDataMap _data)
    {
        if (card != null)
        {
            Destroy(card.gameObject);
            card = null;
        }

        var unit = GameCore.Instance.DataMgr.GetUnitDataByCharID(_data.needId);
        card = CardBase.CreateSmallCardByKey(unit.id, cardRoot, null, (id) => GameCore.Instance.ShowCardInfoNotHave((int)id));
        bool have = GameCore.Instance.PlayerDataMgr.HasUnitSDataByCharID(_data.needId);
        haveIcon.SetActive(!have);

        int dataNameID = 0;
        if(Int32.TryParse(_data.nameID, out dataNameID)) {
            ChrChemyDataMap dd = GameCore.Instance.DataMgr.GetChrChemyStringData(dataNameID);
            lbName.text = dd != null ? dd.name : "Not found string";
            if(dd.stat != "-1") lbOptionBuff.text = dd.stat;
            else lbOptionBuff.text = "";
            lbOptionBuff.transform.parent.gameObject.SetActive(true);
            //lbOptionBuff.transform.parent.gameObject.SetActive("-1" != dd.stat);


            if(dd.ef != "-1") lbOptionDebuff.text = dd.ef;
            else lbOptionDebuff.text = "";
            lbOptionDebuff.transform.parent.gameObject.SetActive(true);
            //lbOptionDebuff.transform.parent.gameObject.SetActive("-1" != dd.ef);
        } 

        grid.enabled = true;
    }

}
