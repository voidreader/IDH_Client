using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInquiryItemSkinRewardSData : JsonParse
{

    public int id;
    public int type;
    public int sort;
    public int rewardID;
    public int count;
    public int saleType;
    public int price;
    public int stringIndex;
    public int texture;
    public int pr;

    public string NAME;
    public string DESC;     // 첫구매시 표기 되는 이름
    public int FIRST_BONUS; // 첫구매시 추가되는 아이템 수량
    public bool FIRST;      // 첫구매인지 여부(true 첫구매, false 첫구매 아님)

    public List<ShopPackageRewardSData> rewardSData = new List<ShopPackageRewardSData>();
    public DateTime startEventTime;
    public DateTime endEventTime;

    internal override bool SetData(JSONObject json)
    {
        ToParse(json, "ID", out id);
        ToParse(json, "TYPE", out type);
        ToParse(json, "SORT", out sort);
        ToParse(json, "REWARD_ID", out rewardID);
        ToParse(json, "CNT", out count);
        ToParse(json, "SALE_TYPE", out saleType);
        ToParse(json, "PRICE", out price);
        ToParse(json, "START_DATE", out startEventTime);
        ToParse(json, "END_DATE", out endEventTime);
        ToParse(json, "STRING_ID", out stringIndex);
        ToParse(json, "TEXTURE", out texture);
        ToParse(json, "PR", out pr);

        ToParse(json, "NAME", out NAME);
        ToParse(json, "DESC", out DESC);
        ToParse(json, "FIRST_BONUS", out FIRST_BONUS);
        json.GetField(ref FIRST, "FIRST");

        JSONObject jsonObj;

        jsonObj = json.GetField("REWARD");
        for (int i = 0; i < jsonObj.Count; i++)
        {
            ShopPackageRewardSData rewardData = new ShopPackageRewardSData();
            rewardData.SetData(jsonObj[i]);
            rewardSData.Add(rewardData);
        }

        // DESC 특수 문자 제거
        NAME = NAME.Replace("\\\"", "\"");
        NAME = NAME.Replace("\\n", "\n");

        // DESC 특수 문자 제거
        DESC = DESC.Replace("\\\"", "\"");
        DESC = DESC.Replace("\\n", "\n");

        return true;
    }

}
