using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoStateComponent : MonoBehaviour
{
    [SerializeField] UISprite spFillUpgrade;
    [SerializeField] UILabel lbValueUpgrade;
    [SerializeField] UISprite spFillRevol;
    [SerializeField] UILabel lbValueRevol;
    [SerializeField] UISprite spFillAwaken;
    [SerializeField] UILabel lbValueAwaken;

    [SerializeField] Transform[] ItemRoots;

    CardBase[] equipments = new CardBase[5];

    System.Action cbClickItemUp;

    internal void Init(HeroSData _data, System.Action _cbClickItemUp)
    {
        spFillUpgrade.fillAmount = Random.value;
        cbClickItemUp = _cbClickItemUp;

        var unit = GameCore.Instance.DataMgr.GetUnitData(_data.key);
        float evol = Mathf.Clamp(unit.evolLvl, 0, 5);
        float awaken = Mathf.Clamp(unit.evolLvl - 5, 0, 5);
        var maxEnchant = GameCore.Instance.DataMgr.GetMaxStrengthenLevel(unit.evolLvl);

        spFillUpgrade.fillAmount = 1f - (float)_data.enchant / maxEnchant;
        lbValueUpgrade.text = string.Format("{0} [c] {1}", _data.enchant, maxEnchant);

        spFillRevol.fillAmount = 1f - (evol / 5);
        lbValueRevol.text = string.Format("{0} [c] {1}", evol, 5);

        spFillAwaken.fillAmount = awaken / 5;
        lbValueAwaken.text = string.Format("{0} [c] {1}", awaken, 5);


        for(int i = 0; i < equipments.Length; ++i)
        {
            if (equipments[i] != null)
            {
                Destroy(equipments[i].gameObject);
                equipments[i] = null;
            }
        }

        // 장착 아이템
        for (int i = 0; i < _data.equipItems.Length; ++i)
        {
            if (_data.equipItems[i] <= 0)
                continue;
            var item = GameCore.Instance.PlayerDataMgr.GetItemSData(_data.equipItems[i]);
            var data = GameCore.Instance.DataMgr.GetItemData(item.key);
            int idx = data.subType - (ItemSubType.EquipItem+1);
            if( equipments[idx] != null )
                Debug.Log("이미 장창된 타입이 아이템입니다! " + idx);
            
            equipments[idx] = CardBase.CreateBigCard(item, ItemRoots[idx], 
                (uid) => cbClickItemUp(), 
                (uid)=> GameCore.Instance.ShowCardInfo(GameCore.Instance.PlayerDataMgr.GetItemSData(uid)));
        }
    }
}
