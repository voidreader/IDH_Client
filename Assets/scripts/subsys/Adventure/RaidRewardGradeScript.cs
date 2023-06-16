using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidRewardGradeScript : MonoBehaviour
{
    [SerializeField] UILabel lbText;
    [SerializeField] UISlider slider;
    [SerializeField] GameObject obtain;

    CardBase card;
    int minDmg;
    int maxDmg;

    public void SetData(int _itemKey, int _count, int _minDmg, int _maxDmg, string _text = null)
    {
        if( card != null )
        {
            Destroy(card.gameObject);
            card = null;
        }

        minDmg = _minDmg;
        maxDmg = _maxDmg;

        card = CardBase.CreateSmallCardByKey(_itemKey, transform, null, (key)=>GameCore.Instance.ShowCardInfoNotHave((int)key));
        card.SetCount(_count);
        if (_text != null)
            lbText.text = _text;
        else
            lbText.text = maxDmg.ToString("N0");
    }

    public void SetDmg(int _dmg)
    {
        SetValue((float)(_dmg - minDmg) / (maxDmg - minDmg));
    }

    void SetValue(float _value)
    {
        _value = Mathf.Clamp01(_value);
        slider.value = _value;
        obtain.SetActive(1 <= _value);
        card.SetEnable(1 > _value);
    }
}
