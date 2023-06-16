using UnityEngine;
using System.Collections;
using Realtime.LITJson;

public class NUserInfo
{
    public string memberid { get; set; }
    public string name { get; set; }
    public string profileUrl { get; set; }
    public string email { get; set; }
    public string token { get; set; }
    public string userid { get; set; }

    public string ToJson()
    {
        JsonData data = new JsonData();

        data["memberid"] = memberid;
        data["name"] = name;
        data["profileUrl"] = profileUrl;
        data["email"] = email;
        data["token"] = token;
        data["userid"] = userid;

        Debug.Log("NUserInfo::ToJson() - " + data.ToJson());

        return data.ToJson();
    }
}