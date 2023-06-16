using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoTeamSkillListItem : MonoBehaviour
{
    internal static HeroInfoTeamSkillListItem Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("HeroInfo/TeamSkillListItem", _parent);
        return go.GetComponent<HeroInfoTeamSkillListItem>();
    }

    [SerializeField] Transform[] cardRoots;
    [SerializeField] GameObject[] haveIcons;

    [SerializeField] UISprite spSkillIcon;
    [SerializeField] UILabel lbSkillName;

    List<CardBase> cards = new List<CardBase>();

    internal void Init(TeamSkillDataMap _data)
    {
        for(int i = 0; i < cards.Count;++i)
            Destroy(cards[i].gameObject);
        cards.Clear();

        int idx = 0;
        for (; idx < _data.needChar.Length; ++idx)
        {
            if (_data.needChar[idx] <= 0)
                break;

            var unit = GameCore.Instance.DataMgr.GetUnitDataByCharID(_data.needChar[idx]);
            var card = CardBase.CreateSmallCardByKey(unit.id, cardRoots[idx], null, (id) => GameCore.Instance.ShowCardInfoNotHave((int)id));
            cards.Add(card);
            bool have = GameCore.Instance.PlayerDataMgr.HasUnitSDataByCharID(_data.needChar[idx]);
            haveIcons[idx].SetActive(!have);
        }

        for(; idx < 5; ++idx)
            haveIcons[idx].SetActive(false);

        GameCore.Instance.SetUISprite(spSkillIcon, _data.imageID);

        int key = _data.id;
        spSkillIcon.GetComponent<ButtonRapper>().SetPressCallback(() => 
        {
            Debug.Log("Called Press");
            GameCore.Instance.ShowCardInfoNotHave(key);
        });
        spSkillIcon.GetComponent<ButtonRapper>().SetStopPressCallback(() =>
        {
            Debug.Log("Called Unpress");
            GameCore.Instance.CloseAlert();
        });

        lbSkillName.text = _data.name;
    }
}
