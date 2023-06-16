using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class EquipItemSlotCtrl : MonoBehaviour
{
    [SerializeField] EquipItemSlot[] slots;
    [SerializeField] GameObject[] effects;

    public int SelectedIdx { get; private set; }

    private void Awake()
    {
        SelectedIdx = -1;
    }

    public Transform GetSlotTransform(int pos)
    {
        return slots[pos].transform;
    }

    public void Init(HeroSData _hero, Action<int> _cbClickEmptySlot, Action<int> _cbClickSlotInCard, Action<long> _cbUpgrade, Action<long> _cbUnequip, Action<int> _cbChangeTab)
    {
        for(int i = 0; i < slots.Length; ++i)
        {
            var n = i;
            slots[i].Init(()=> _cbClickSlotInCard(n), ()=> _cbClickEmptySlot(n), _cbUpgrade, _cbUnequip, ()=> _cbChangeTab(n));
        }

        //for (int i = 0; i < _hero.equipItems.Count; ++i)
        //{
        //    var type = GameCore.Instance.PlayerDataMgr.GetItemData(_hero.equipItems[i]).subType - (ItemSubType.EquipItem+1);
        //    slots[type].SetCard(GameCore.Instance.PlayerDataMgr.GetItemSData(_hero.equipItems[i]));
        //}
    }

    public CardBase GetCardByIndex(int _idx)
    {
        return slots[_idx].card;
    }

    public void SetSelect(ItemSData _sdata)
    {
        var idx = EquipItemUI.GetEquipTypeIdx(_sdata);
        bool hi = slots[idx].card == null || slots[idx].card.SData.uid != _sdata.uid;
        bool btns = slots[idx].card != null && slots[idx].card.SData.uid == _sdata.uid;
        SetSelect(idx, hi, btns);
    }

    public void SetSelect(int _newIdx, bool _highlight, bool _showBtns)
    {
        SetSelectCheckInBound(SelectedIdx, false, false, false);
        SetSelectCheckInBound(_newIdx, true, _highlight, _showBtns);
        SelectedIdx = _newIdx;
    }

    public void ClearSelect()
    {
        SetSelectCheckInBound(SelectedIdx, false, false, false);
        SelectedIdx = -1;
    }


    void SetSelectCheckInBound(int _idx, bool _select, bool _highlight, bool _showBtns)
    {
        if (0 <=_idx && _idx < slots.Length)
            slots[_idx].SetSelect(_select, _highlight, _showBtns);
    }

    /// <summary>
    /// 슬롯의 카드를 바꾸고 바꾸기 전의 카드를 반환한다.
    /// 아이템의 타입에 따라 알아서 슬롯 위치를 설정한다.
    /// </summary>
    /// <param name="_card">바뀔 카드</param>
    /// <returns>바뀌기 전의 카드</returns>
    public CardSData SetItem(CardSData _card)
    {
        var item = GameCore.Instance.DataMgr.GetItemData(((ItemSData)_card).key);
        var idx = item.subType - (ItemSubType.EquipItem + 1);

        effects[idx].SetActive(false);
        effects[idx].SetActive(true);

        return slots[idx].SetCard(_card);
    }

    public CardSData UnposSlot(int _idx)
    {
        return slots[_idx].SetCard(null);
    }
}

