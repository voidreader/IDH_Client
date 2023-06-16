using Realtime.LITJson;
using System.Collections.Generic;

public class AdjustBillingData
{
    public string productId;
    public string adjustKey;

    public AdjustBillingData(string productId, string adjustKey) // inapp 결제 productId, Adjust key
    {
        this.productId = productId;
        this.adjustKey = adjustKey;
    }
}

public class AdjustInfo : TrackingInfo
{
    List<AdjustBillingData> adjustList;
    public AdjustInfo(List<AdjustBillingData> adjustList)
    {
        this.adjustList = adjustList;   
    }

    public string trackingInfoConvertToJson()
    {
        JsonData trackingInfo = new JsonData();

        for (int i = 0; i < adjustList.Count; i++)
        {
            AdjustBillingData data = adjustList[i];
            trackingInfo[data.productId] = data.adjustKey;
        }

        return trackingInfo.ToJson();
    }
}