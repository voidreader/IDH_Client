using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPackageRewardSData : JsonParse {

    private string strId;
    private string strValue;
    private string strCon;


    public int id;
    public int value;
    public int con;
    public int reward;

    internal override bool SetData(JSONObject json)
    {
        ToParse(json, "ID", out strId);
        ToParse(json, "VALUE", out strValue);
        ToParse(json, "CON", out strCon);
        ToParse(json, "REWARD", out reward);

        if (strId != null)
        {
            id = Convert.ToInt32(strId);
            value = Convert.ToInt32(strValue);
            con = Convert.ToInt32(strCon);
        }
        else
        {
            ToParse(json, "ID", out id);
            ToParse(json, "VALUE", out value);
            ToParse(json, "CON", out con);

            strId = Convert.ToString(id);
            strValue = Convert.ToString(value);
            strCon = Convert.ToString(con);
        }
        return true;
    }
}
