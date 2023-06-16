using UnityEngine;
using System.Collections;
using Realtime.LITJson;

public class NAppStatus
{
    public string type { get; set; }
    public string message { get; set; }
    public string url { get; set; }
    public string currentAppVersion { get; set; }
    public string updateAppVersion { get; set; }
    public int currentAppVersionCode { get; set; }
    public int updateAppVersionCode { get; set; }
    public bool isForce { get; set; }
    public string resultPayload { get; set; }

    public string ToJson()
    {
        JsonData data = new JsonData();

        data["type"] = type;
        data["message"] = message;

        if (type.Equals("maintenance"))
        {
            if (url != null && !url.Equals("null"))
            {
                data["url"] = url;
            }
        }
        else if (type.Equals("needupdate"))
        {
            if (currentAppVersion != null && !currentAppVersion.Equals("null"))
            {
                data["currentAppVersion"] = currentAppVersion;
            }
            if (updateAppVersion != null && !updateAppVersion.Equals("null"))
            {
                data["updateAppVersion"] = updateAppVersion;
            }

            data["currentAppVersionCode"] = currentAppVersionCode;
            data["updateAppVersionCode"] = updateAppVersionCode;
            data["isForce"] = isForce;

            NUserInfo userinfo = JsonMapper.ToObject<NUserInfo>(resultPayload);
            data["resultPayload"] = userinfo.ToJson();
        }

        Debug.Log("NAppStatus::ToJson() - " + data.ToJson());

        return data.ToJson();
    }
}