using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoSkillComponenet : MonoBehaviour
{
    [SerializeField] Transform camiListRoot;
    [SerializeField] Transform teamListRoot;
    [SerializeField] UISprite spCami;
    [SerializeField] UISprite spTeam;

    List<HeroInfoCamiSkillListItem> camiItems = new List<HeroInfoCamiSkillListItem>();
    List<HeroInfoTeamSkillListItem> teamItems = new List<HeroInfoTeamSkillListItem>();

    internal void Init(HeroSData _data)
    {
        var data = GameCore.Instance.PlayerDataMgr.GetUnitData(_data.uid);

        { // 캐미
            var iter = GameCore.Instance.DataMgr.GetChemistryEnumertor();
            while (iter.MoveNext())
            {
                if (iter.Current.Value.tgId == data.charIdType)
                {
                    var item = HeroInfoCamiSkillListItem.Create(camiListRoot);
                    item.Init(iter.Current.Value);
                    camiItems.Add(item);
                }
            }
        }

        { // 팀스킬
            var iter = GameCore.Instance.DataMgr.GetTeamSkillEnumertor();
            while (iter.MoveNext())
            {
                var chars = iter.Current.Value.needChar;
                for(int i = 0; i < chars.Length; ++i)
                {
                    if (chars[i] == data.charIdType)
                    {
                        var item = HeroInfoTeamSkillListItem.Create(teamListRoot);
                        item.Init(iter.Current.Value);
                        teamItems.Add(item);
                        break;
                    }
                }
            }
        }
        OnClickChemistry();
    }


    public void OnClickChemistry()
    {
        spCami.spriteName = CommonType.BTN_5_ACTIVE;
        spTeam.spriteName = CommonType.BTN_5_NORMAL;
        camiListRoot.gameObject.SetActive(true);
        teamListRoot.gameObject.SetActive(false);
    }

    public void OnClickTeamSkill()
    {
        spCami.spriteName = CommonType.BTN_5_NORMAL;
        spTeam.spriteName = CommonType.BTN_5_ACTIVE;
        camiListRoot.gameObject.SetActive(false);
        teamListRoot.gameObject.SetActive(true);
    }
}
