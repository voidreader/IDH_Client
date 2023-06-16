using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRoomBuyScript : MonoBehaviour
{
    [SerializeField] UILabel lbGuide;
    [SerializeField] UILabel lbGuideRoomBuff;
    [SerializeField] UILabel lbBuff;
    [SerializeField] UISprite spCostIcon;
    [SerializeField] UILabel lbCost;

    public static int needCostType = 0;

    public static MyRoomBuyScript Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("MyRoom/BuyMyRoomRoot", _parent);
        return go.GetComponent<MyRoomBuyScript>();
    }

    internal void Init(MyRoomDataMap _data)
    {
        lbGuideRoomBuff.text = string.Format("숙소 {0} 만족도 버프 내용", _data.id);

        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < 4; ++i)
        {
            if (sb.Length != 0)
                sb.Append("\n");
            sb.Append(i + 1);
            sb.Append(".           ");
            sb.Append(MyRoomDataMap.GetStrMyRoomEffect(_data.satisfactionEffectID[i], _data.satisfactionEffectValue[i]));
        }
        lbBuff.text = sb.ToString();

        needCostType = _data.openType;

        if (_data.openType == 1) spCostIcon.spriteName = "ICON_MONEY_03";
        else                     spCostIcon.spriteName = "ICON_MONEY_02";

        lbCost.text = _data.openValue.ToString("N0");
    }
}
