using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoProfileComponent : MonoBehaviour
{
    [SerializeField] UILabel lbBlong;
    [SerializeField] UILabel lbClass;
    [SerializeField] UILabel lbCharacter;
    [SerializeField] UILabel lbName;
    [SerializeField] UILabel lbAbility;
    [SerializeField] UILabel lbFeature;

    [SerializeField] UILabel lbDesc;

    internal void Init(HeroSData _data)
    {
        var data = GameCore.Instance.DataMgr.GetUnitData(_data.key);
        var profile = GameCore.Instance.DataMgr.GetProfileStringData(data.charIdType);
        lbBlong.text = data.GetBelongString();
        lbClass.text = profile.@class;
        lbCharacter.text = profile.character;
        lbName.text = profile.name;
        lbAbility.text = profile.ability;
        lbFeature.text = profile.feature;

        lbDesc.text = profile.desc;
    }
}
