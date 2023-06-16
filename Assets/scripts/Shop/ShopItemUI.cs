using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour {

    ShopInquiryItemSkinSData itemSData;

    ShopQuestionBoxComponent questionBox;

    [SerializeField]    UIButton[] button;
    [SerializeField]    GameObject itemBoxList;

    List<GameObject> itemBox = new List<GameObject>();

    public int nowItemType = -1;

    internal void GetItemSData(ShopInquiryItemSkinSData sData, ShopPara _shopPara)
    {
        itemSData = sData;
        SetButton(itemSData);

        //var pos = _shopPara != null ? _shopPara.openPos : 0;
        OnClickButton(0);
        SelectButton(0);

        //button[0].gameObject.SetActive(false);
        // button[2].gameObject.SetActive(false);
    }


    private void SetDataList(List<ShopInquiryItemSkinRewardSData> sDataList, List<ShopInquiryItemSkinRewardSData> setDataList, int type)
    {
        //type : 0 = 유로재화, 1 = 게임재화, 2 = 아이템, 3 = 인테리어, 4 = 기타등등 

        if (setDataList.Count > 0) setDataList.Clear();

        var sorting = from sort in sDataList
                      where sort.sort == type &&
                      sort.type != 2 && sort.type != 3
                      orderby (sort.rewardID / 1000000)
                      select sort;

        foreach(ShopInquiryItemSkinRewardSData sData in sorting)
        {
            setDataList.Add(sData);
        }
    }

    private void SetButton(ShopInquiryItemSkinSData data)
    {
        for(int i = 0; i < button.Length; ++i)
        {
            int index = i;
            SetButton(button[i], ()=> { OnClickButton(index); SelectButton(index); }, null);
        }
    }

    private void SetButton(UIButton button, EventDelegate.Callback callBack, string label)
    {
        button.onClick.Add(new EventDelegate(()=> { callBack(); GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button); }));

        if (label == null) return;
        UnityCommonFunc.GetComponentByName<UILabel>(button.gameObject, "Label").text = label;
    }

    public void SelectButton(int index)
    {
        for (int i = 0; i < button.Length; ++i)
        {
            button[i].GetComponent<UISprite>().spriteName = "BTN_05_01_01";
        }
        button[index].GetComponent<UISprite>().spriteName = "BTN_05_01_02";
    }

    public void UpdateUI()
    {
        OnClickButton(nowItemType, true);
    }

    public void OnClickButton(int index, bool _force = false)
    {
        List<ShopInquiryItemSkinRewardSData> itemList = new List<ShopInquiryItemSkinRewardSData>();
        SetDataList(itemSData.rewardSData, itemList, index);

        if (!_force && nowItemType == index) return;
        nowItemType = index;

        if (itemBox.Count > 0)
        {
            for(int i = 0; i < itemBox.Count; ++i)
            {
                //이전단계에서 존재했던 아이템박스를 지우고
                Destroy(itemBox[i]);
                itemBox[i].transform.parent = GameCore.Instance.Ui_root;
            }
        }
        //리스트를 비운다.
        itemBox.Clear();

        //새로 담기 시작.
        for(int i = 0; i < itemList.Count; ++i)
        {
            itemBox.Add(GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/ItemBox", itemBoxList.transform));

            SetItemBox(itemBox, itemList, i);
        }

        //다시 생성하고 변경하는 작업이 끝났으니 재정렬을 시작하라.
        itemBoxList.GetComponent<UIGrid>().Reposition();
    }

    private void SetItemBox(List<GameObject> itemBox, List<ShopInquiryItemSkinRewardSData> itemList, int index)
    {
        if ((itemList[index].rewardID / 1000000) == 3)
        {
            itemBox[index].transform.GetChild(0).GetComponent<UILabel>().text = itemList[index].FIRST ? //itemList[index].count + " " + GameCore.Instance.DataMgr.GetItemData(itemList[index].rewardID).name;
                itemList[index].DESC : itemList[index].NAME;
        }
        else
        {
            itemBox[index].transform.GetChild(0).GetComponent<UILabel>().text = GameCore.Instance.DataMgr.GetShopPackageInfoData(itemList[index].rewardID).name;
            itemBox[index].transform.GetChild(3).gameObject.SetActive(true);
        }

        itemBox[index].transform.GetChild(4).gameObject.SetActive(itemList[index].FIRST);
        itemBox[index].transform.GetChild(1).GetChild(1).GetComponent<UILabel>().text = string.Format("{0:N0}", itemList[index].price);

        if (itemList[index].saleType == 0) itemBox[index].transform.GetChild(1).GetChild(0).GetComponent<UISprite>().spriteName = "ICON_MONEY_03";
        else if (itemList[index].saleType == 1) itemBox[index].transform.GetChild(1).GetChild(0).GetComponent<UISprite>().spriteName = "ICON_MONEY_02";
        else if (itemList[index].saleType == 2) itemBox[index].transform.GetChild(1).GetChild(0).GetComponent<UISprite>().spriteName = "W_75";

        if (CardDataMap.IsItemKey(itemList[index].rewardID))  //임시로 테이블과 이미지 정보가 없어서 없는 정보를 제외하고 출력하기.
        {
            var card = CardBase.CreateBigCardByKey(itemList[index].rewardID, itemBox[index].transform.GetChild(2).transform) as ItemCardBase;
            var cnt = itemList[index].count + (itemList[index].FIRST ? itemList[index].FIRST_BONUS : 0);
            if ( cnt <= 1)  card.GetCountLabel().text = "x 1";
            else            card.SetCount(cnt);

            if (0 < itemList[index].texture)
                GameCore.Instance.SetUISprite((card as ItemCardBase).GetSprite(), itemList[index].texture);

            card.SetPressCallback(()=> {
                GameCore.Instance.ShowCardInfoNotHave(itemList[index].rewardID, itemList[index].texture);
            });
        }
        //itemButton클릭시 활성화 될 함수. 생성.
        SetButton(itemBox[index].transform.GetChild(1).GetComponent<UIButton>(), () => { OnClickItemBoxButton(itemList, index); }, null);

        if (itemBox[index].transform.GetChild(3).gameObject.activeSelf)
        {
            SetButton(itemBox[index].transform.GetChild(3).GetComponent<UIButton>(), () => { OnClickQuestionButton(itemList, index); }, null);
        }
    }

    private void OnClickItemBoxButton(List<ShopInquiryItemSkinRewardSData> itemList, int index)
    {
        GameCore.Instance.ShowAgree("구매 팝업", "다음 캐쉬를 사용하여 선택하신 상품을\n 구매하시겠습니까?", string.Format("{0:N0}", itemList[index].price), MoneyType(itemList[index].saleType), 0, () =>
        {
            GameCore.Instance.NetMgr.Req_Shop_Buy(itemList[index].id);
            GameCore.Instance.CloseMsgWindow();
        });
    }

    private void OnClickQuestionButton(List<ShopInquiryItemSkinRewardSData> itemList, int index)
    {
        questionBox = ShopQuestionBoxComponent.Create(gameObject.transform);
        questionBox.GetRewardSData(itemList, index);
        questionBox.Init();

        if (itemList[index].pr != 0)
        {
            GameCore.Instance.ShowObject("구매 팝업", null, questionBox.gameObject, 0,
            new MsgAlertBtnData[] {
                new MsgAlertBtnData("닫기", new EventDelegate(GameCore.Instance.CloseMsgWindow)),
                new MsgAlertBtnData(string.Format("₩ {0:N0}", itemList[index].price),new EventDelegate(()=> { GameCore.Instance.CloseMsgWindow(); OnClickItemBoxButton(itemList, index);}))
            });
        }
        else
        {
            GameCore.Instance.ShowObject("구매 팝업", null, questionBox.gameObject, 0,
            new MsgAlertBtnData[] {
                new MsgAlertBtnData("닫기", new EventDelegate(GameCore.Instance.CloseMsgWindow)),
            });
        }
    }

    private MoneyType MoneyType(int index)
    {
        if (index == 0) return global::MoneyType.Pearl;
        else if (index == 1) return global::MoneyType.Gold;
        else return global::MoneyType.Cash;
    }

    public List<ShopInquiryItemSkinRewardSData> GetShopPackages(int _id)
    {
        List<ShopInquiryItemSkinRewardSData> list = new List<ShopInquiryItemSkinRewardSData>();
        foreach (var sdata in itemSData.rewardSData)
            if (sdata.id == _id)
                list.Add(sdata);

        return list;
    }
}
