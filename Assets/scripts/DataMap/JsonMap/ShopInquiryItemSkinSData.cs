using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInquiryItemSkinSData : JsonParse
{
    public int id;
    public int type;
    public string itemName;
    public List<ShopInquiryItemSkinRewardSData> rewardSData = new List<ShopInquiryItemSkinRewardSData>();

    internal override bool SetData(JSONObject json)
    {
        ToParse(json, "ID", out id);
        ToParse(json, "TYPE", out type);
        ToParse(json, "NAME", out itemName);

        JSONObject jsonObj = new JSONObject();

        jsonObj = json.GetField("LIST");
        for (int i = 0; i < jsonObj.Count; ++i)
        {
            ShopInquiryItemSkinRewardSData data = new ShopInquiryItemSkinRewardSData();
            data.SetData(jsonObj[i]);
            rewardSData.Add(data);
        }

        return true;
    }

}
