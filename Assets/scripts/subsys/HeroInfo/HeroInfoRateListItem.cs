using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HeroInfoRateListItem : MonoBehaviour
{
    public static HeroInfoRateListItem Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("HeroInfo/RateListItem", _parent);
        return go.GetComponent<HeroInfoRateListItem>();
    }

    [SerializeField] UISprite spIcon;
    [SerializeField] UILabel lbName;
    [SerializeField] UILabel lbText;
    [SerializeField] UILabel lbRate;
    [SerializeField] UILabel lbDownCount;
    [SerializeField] UILabel lbUpCount;

    public HeroEvaluateSData data { get; private set; }
    Action<HeroInfoRateListItem> cbSpeach;
    Action<long> cbDown;
    Action<long> cbUp;

    public void Init(HeroEvaluateSData _data, Action<HeroInfoRateListItem> _cbSpeach, Action<long> _cbDown, Action<long> _cbUp)
    {
        data = _data;
        cbSpeach = _cbSpeach;
        cbDown = _cbDown;
        cbUp = _cbUp;
        
        GameCore.Instance.SetUISprite(spIcon, GameCore.Instance.DataMgr.GetUnitDataByCharID(_data.typicalKey).GetSmallProfileSpriteKey());
        lbName.text = _data.name;
        lbText.text = _data.comment;
        lbRate.text = _data.score.ToString("F1");
        lbDownCount.text = _data.downCount.ToString("N0");
        lbUpCount.text = _data.upCount.ToString("N0");
        lbDownCount.color = _data.imDown != 0 ? (Color)CommonType.COLOR_05 : Color.white;
        lbUpCount.color = _data.imUp != 0 ? (Color)CommonType.COLOR_05 : Color.white;
    }

    public void IncDown()
    {
        lbDownCount.text = (data.downCount+1).ToString("N0");
    }
    public void IncUp()
    {
        lbUpCount.text = (data.upCount + 1).ToString("N0");
    }

    public void OnClickDown()
    {
        if (data.imDown == 0)
        {
            data.imUp = 0;
            data.imDown =1;
            cbDown(data.UID);
        }
    }

    public void OnClickUp()
    {
        if (data.imUp == 0)
        {
            data.imUp = 1;
            data.imDown = 0;
            cbUp(data.UID);
        }
    }

    public void OnClickSpeachBlock()
    {
        cbSpeach(this);
    }
}
