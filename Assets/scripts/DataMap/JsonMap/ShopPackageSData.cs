using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPackageSData : JsonParse {

    public string strId;
    public string strPr;
    public string strPrice;
    public string strperiod;

    // DB를 기준으로 각 역할

    public int id;
    public int type;            // 1: 월정액, 2: 레벨업, 3: 패키지, 4: 아이템
    public int pr;              // 남은 구매 회수
    public int price;           // 가격
    public int period;          
    public int stringIndex;     
    public int texture;         // 텍스쳐 ID
    public string name;         // 이름
    public List<ShopPackageRewardSData> rewardSData = new List<ShopPackageRewardSData>();

    internal override bool SetData(JSONObject json)
    {
        ToParse(json, "ID", out id);
        ToParse(json, "TYPE", out type);
        ToParse(json, "PR", out pr);
        ToParse(json, "VALUE", out price);
        ToParse(json, "NAME", out name);
        ToParse(json, "PERIOD", out period);
        ToParse(json, "STRING_ID", out stringIndex);
        ToParse(json, "TEXTURE", out texture);

        strPr = Convert.ToString(pr);
        strId = Convert.ToString(id);
        strPrice = Convert.ToString(price);
        strperiod = Convert.ToString(period);

        JSONObject jsonObj;

        jsonObj = json.GetField("REWARD");
        rewardSData.Clear();
        for (int i = 0; i < jsonObj.Count; i++)
        {
            ShopPackageRewardSData rewardData = new ShopPackageRewardSData();
            rewardData.SetData(jsonObj[i]);
            rewardSData.Add(rewardData);
        }

        name = name.Replace("\\\"", "\"");
        name = name.Replace("\\n", "\n");

        return true;
    }
}
