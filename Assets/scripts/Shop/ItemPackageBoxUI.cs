using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPackageBoxUI : MonoBehaviour
{
    List<ShopInquirySData> inquirySData = new List<ShopInquirySData>();

    internal void GetInquirySData(List<ShopInquirySData> data)
    {
        inquirySData = data;
    }


}
