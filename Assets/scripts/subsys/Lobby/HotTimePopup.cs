using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class HotTimeSData : JsonParse
{
    public string desc;
    public int value;
    public int type;
    public DateTime start;
    public DateTime end;

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "DESC", out desc);
        ToParse(_json, "VALUE", out value);
        ToParse(_json, "TYPE", out type);
        ToParse(_json, "START_TIME", out start);
        ToParse(_json, "END_TIME", out end);
        return true;
    }
}


public class HotTimePopup : MonoBehaviour
{
    [SerializeField] UILabel lbDate;
    [SerializeField] UILabel lbEffect;


    public static HotTimePopup Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_HotTimePopup, _parent);
        var result = go.GetComponent<HotTimePopup>();
        return result;
    }

    public void Init(List<HotTimeSData> _sdatas)
    {
        if (_sdatas == null || _sdatas.Count == 0)
            return;

        lbDate.text = string.Format(CSTR.HotTimeDate, 
            _sdatas[0].start.ToString(CSTR.TimeDateYMD), _sdatas[0].start.ToString(CSTR.TimeDateHm), 
            _sdatas[0].end.ToString(CSTR.TimeDateYMD), _sdatas[0].end.ToString(CSTR.TimeDateHm));


        StringBuilder sb = new StringBuilder();

        foreach (var data in _sdatas)
        {
            if (sb.Length != 0) sb.Append("\n");
            sb.Append(string.Format(CSTR.HotTimeDesc, data.desc, data.value));
        }

        lbEffect.text = sb.ToString();
    }
}
