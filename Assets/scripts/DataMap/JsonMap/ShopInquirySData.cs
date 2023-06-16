using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInquirySData : JsonParse
{
    public int Id;
    public int type;
    public int priority;
    public string packageName;
    public DateTime startEventTime;
    public DateTime endEventTime;

    public List<ShopPackageSData> PackageListSData = new List<ShopPackageSData>();

    internal override bool SetData(JSONObject json)
    {
        ToParse(json, "ID", out Id);
        ToParse(json, "TYPE", out type);
        ToParse(json, "PRIORITY", out priority);
        ToParse(json, "NAME", out packageName);
        ToParse(json, "START_DATE", out startEventTime);
        ToParse(json, "END_DATE", out endEventTime);

        JSONObject jsonObj = new JSONObject();

        jsonObj = json.GetField("LIST");
        for(int i = 0; i < jsonObj.Count; ++i)
        {
            ShopPackageSData data = new ShopPackageSData();
            data.SetData(jsonObj[i]);
            PackageListSData.Add(data);
        }

        return true;
    }

    internal TimeSpan GetDelay()
    {
        return endEventTime - startEventTime;
    }
}
