using UnityEngine;
using System.Collections;
using Realtime.LITJson;

public class TutorialInfo : TrackingInfo
{
    public TutorialInfo(string contentData, string contentId, bool isSuccess) // 필수 데이터
	{
        this.contentData = contentData;
        this.contentId = contentId;
        this.isSuccess = isSuccess;
	}

    private string contentData = "";  							// 튜토리얼 정보
    private string contentId = ""; 								// content ID
    private bool isSuccess = false; 						    // 성공 여부
    private string adjustKey = "";                              // adjust Key

    public void setAdjustKey(string adjustKey)
	{
        this.adjustKey = adjustKey;
	}

    public string trackingInfoConvertToJson()
	{
		JsonData trackingInfo = new JsonData ();

        trackingInfo ["contentData"] 		= contentData;
        trackingInfo ["contentId"] 		    = contentId;
        trackingInfo ["isSuccess"] 		    = isSuccess.ToString();
        trackingInfo ["adjustKey"]          = adjustKey;

        return trackingInfo.ToJson();
	}
}