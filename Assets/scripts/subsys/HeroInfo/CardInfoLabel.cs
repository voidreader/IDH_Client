using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class CardInfoLabel : MonoBehaviour
{
    [SerializeField] UISprite spRank;
    [SerializeField] UISprite spType;
    [SerializeField] UILabel lbType;
    [SerializeField] UILabel lbName;
    [SerializeField] UISprite[] spStars;
    [SerializeField] GameObject goStarsRoot;
    [SerializeField] UILabel lbKind;

    public void Init(CardSData _sdata)
    {
        if (_sdata.type == CardType.Character)
        {
            var unit = GameCore.Instance.PlayerDataMgr.GetUnitData(_sdata.uid);
            spRank.spriteName = string.Format("ICON_LV_{0:00}", (unit.rank + 1));
            spType.spriteName = string.Format("ICON_TYPE_01_{0:00}", (unit.charType));
            lbType.text = unit.GetCharTypeString();
            lbName.text = string.Format("{0}(+{1})", unit.name, _sdata.enchant);

            goStarsRoot.SetActive(true);
            lbKind.gameObject.SetActive(false);
            SetStar(unit.evolLvl);
        }
        else if( _sdata.type == CardType.Equipment)
        {
            var item = GameCore.Instance.PlayerDataMgr.GetItemData(_sdata.uid);
            spRank.spriteName = string.Format("ICON_LV_{0:00}", (item.rank + 1));
            spType.spriteName = string.Format("ICON_TYPE_01_{0:00}", (item.equipLimit));
            lbType.text = item.GetCharTypeString();
            lbName.text = string.Format("{0}(+{1})", item.name, _sdata.enchant);

            goStarsRoot.SetActive(false);
            lbKind.gameObject.SetActive(true);
            lbKind.text = string.Format("장비종류: " + item.GetSubTypeString());
        }
    }

    void SetStar(int _cnt)
    {
        var on = string.Format("ICON_STAR_{0:00}", _cnt <= 5 ? 1 : 2);
        var off = string.Format("ICON_STAR_{0:00}", _cnt <= 5 ? 0 : 1);

        _cnt = ((_cnt - 1) % 5) + 1;

        for (int i = 0; i < 5; ++i)
        {
            if (i < _cnt) spStars[i].spriteName = on;
            else spStars[i].spriteName = off;
        }
    }
}
