using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
internal class EquipItemSlot : MonoBehaviour
{
    [SerializeField] GameObject goSelect; 
    [SerializeField] UITweener twHighlight;
    [SerializeField] UITweener[] tws;

    public CardBase card { get; private set; }
    public bool BSelect { get; private set; }

    Action cbClickCard;
    Action cbClickSlot;
    Action<long> cbUpgrade;
    Action<long> cbUnequip;
    Action cbChangeTab;
    public SaveAction saveAction = new SaveAction();

    public void Init(Action _cbClickCard, Action _cbClickSlot, Action<long> _cbUpgrade, Action<long> _cbUnequip, Action _cbChangeTab)
    {
        cbClickCard = _cbClickCard;
        cbClickSlot = _cbClickSlot;
        cbUpgrade = _cbUpgrade;
        cbUnequip = _cbUnequip;
        cbChangeTab = _cbChangeTab;
    }

    public CardSData SetCard(CardSData _card)
    {
        var result = SetEmpty();
        if(_card != null)
            card = CardBase.CreateSmallCard(_card, transform, CBClickCard, null);

        return result;
    }

    public CardSData SetEmpty()
    {
        CardSData result = null;
        if (card != null)
        {
            Destroy(card.gameObject);
            result = card.SData;
            card = null;

        }

        return result;
    }

    public void ToggleSelect()
    {
        CBClickCard(0);
        //SetSelect(!goSelect.activeSelf);
    }

    public void SetSelect(bool _select, bool _highlight, bool _showBtns)
    {
        goSelect.SetActive(_select);

        if (twHighlight.enabled != _highlight)
        {
            twHighlight.ResetToBeginning();
            twHighlight.enabled = _highlight;
        }

        for (int i = 0; i < tws.Length; ++i)
            tws[i].Play(_select && _showBtns);
    }

    public void CBClickEmptySlot()
    {
        if(twHighlight.enabled)
            if (cbClickSlot != null)
                cbClickSlot();

        if (cbChangeTab != null)
            cbChangeTab();
        saveAction.GetOnClickAction();
    }

    public void CBClickCard(long _uid)
    {
        if (cbChangeTab != null)
            cbChangeTab();

        if (cbClickCard != null)
            cbClickCard();
    }

    public void OnClickUpgrade()
    {
        if (cbUpgrade != null && card != null)
            cbUpgrade(card.ID);
    }

    public void OnClickUnpos()
    {
        if (cbUnequip != null && card != null)
            cbUnequip(card.ID);
    }
}
