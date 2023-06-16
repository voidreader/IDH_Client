using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopQuestionBoxComponent : MonoBehaviour
{
    public static ShopQuestionBoxComponent Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/ShopQuestionBox", _parent);
        var result = go.GetComponent<ShopQuestionBoxComponent>();
        return result;
    }

    ShopInquiryItemSkinRewardSData rewardSData;
    ShopPackageSData packageRewardSData;


    [SerializeField]    GameObject cardRoot;
    [SerializeField]    UILabel name;
    [SerializeField]    UILabel count;
    [SerializeField]    UILabel highlight;
    [SerializeField]    UILabel text;

    internal void GetRewardSData(List<ShopInquiryItemSkinRewardSData> data, int index)
    {
        rewardSData = data[index];
    }

    internal void GetPackageRewardSData(List<ShopPackageSData> data, int index)
    {
        packageRewardSData = data[index];
    }

    internal void PackageInit()
    {

        //name.text = GameCore.Instance.DataMgr.GetShopPackageInfoData(packageRewardSData.id).name.Replace('\n', ' ');
        name.text = packageRewardSData.name.Replace('\n', ' ');
        if (packageRewardSData.pr > -1)
            count.text = string.Format("구매 횟수 : {0:N0}회", packageRewardSData.pr);
        if(packageRewardSData.stringIndex > 1  && GameCore.Instance.DataMgr.GetShopItemStringData(packageRewardSData.stringIndex) != null)
            highlight.text = GameCore.Instance.DataMgr.GetShopItemStringData(packageRewardSData.stringIndex).str;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < packageRewardSData.rewardSData.Count; ++i)
        {
            if (i != 0)
                sb.Append('\n');

            // get form card type
            if (CardDataMap.IsItemKey(packageRewardSData.rewardSData[i].id))
                sb.Append(GameCore.Instance.DataMgr.GetItemData(packageRewardSData.rewardSData[i].id).name);
            else
                sb.Append(GameCore.Instance.DataMgr.GetUnitData(packageRewardSData.rewardSData[i].id).name);

            sb.Append(" x");
            sb.Append((packageRewardSData.rewardSData[i].value * (packageRewardSData.type == 1 ? 4 : 1)).ToString("N0"));// 월정액은 4번 반복이므로 x4를 한다.
        }

        text.text = sb.ToString();

        var card = CardBase.CreateBigCardByKey((int)ResourceType.Gold, cardRoot.transform, null, null) as ItemCardBase;
        card.SetPressCallback(null);
        card.GetCountLabel().text = "x 1";
        if (0 < packageRewardSData.texture)
            GameCore.Instance.SetUISprite(card.GetSprite(), packageRewardSData.texture);
    }

    internal void Init()
    {
        //name.text = GameCore.Instance.DataMgr.GetShopPackageInfoData(rewardSData.rewardID).name.Replace('\n', ' ');
        name.text = GameCore.Instance.DataMgr.GetItemData(rewardSData.rewardSData[0].id).name.Replace('\n', ' ');
        count.text = "";
        if(rewardSData.stringIndex > 1 && GameCore.Instance.DataMgr.GetShopItemStringData(packageRewardSData.stringIndex) != null)
            highlight.text = GameCore.Instance.DataMgr.GetShopItemStringData(rewardSData.stringIndex).str;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < rewardSData.rewardSData.Count; ++i)
        {
            if (i != 0)
                sb.Append('\n');
            sb.Append(GameCore.Instance.DataMgr.GetItemData(rewardSData.rewardSData[i].id).name);
            sb.Append(" x");
            sb.Append((rewardSData.rewardSData[i].value * (packageRewardSData.type == 1 ? 4 : 1)).ToString("N0")); // 월정액은 4번 반복이므로 x4를 한다.
        }

        text.text = sb.ToString();

        var card = CardBase.CreateBigCardByKey((int)ResourceType.Gold, cardRoot.transform, null, null) as ItemCardBase;
        card.SetPressCallback(null);
        card.GetCountLabel().text = "x 1";
        if (packageRewardSData.texture <= 0)
            GameCore.Instance.SetUISprite(card.GetSprite(), packageRewardSData.texture);
    }
}
