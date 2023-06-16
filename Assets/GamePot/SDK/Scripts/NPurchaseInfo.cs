using UnityEngine;
using System.Collections;
using Realtime.LITJson;

public class NPurchaseInfo
{
    public string price { get; set; }               // 가격
    public string adjustKey { get; set; }           // adjust Key
    public string productId { get; set; }           // 아이템 ID
    public string currency { get; set; }            // 통화
    public string orderId { get; set; }             // 스토어 order id
    public string productName { get; set; }         // 아이템 이름
    public string gamepotOrderId { get; set; }      // GAMEPOT에서 생성한 order id 
    public string uniqueId { get; set; }            // 개발사 unique ID
    public string signature { get; set; }           // 스토어 signature 
    public string originalJSONData { get; set; }    // 영수증 Data
}