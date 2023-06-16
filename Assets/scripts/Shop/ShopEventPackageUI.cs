using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopEventPackageUI : MonoBehaviour
{
    Dictionary<int, List<ShopInquirySData>> shopInquirySData;
    ShopQuestionBoxComponent questionBox;
    List<GameObject> itemPackageBoxList = new List<GameObject>();

    [SerializeField] UISprite backIllustration;
    private GameObject levelUpPackageButton;
    private GameObject itemBoxList;

    private int nowIndex = -1;
    private int nowType = -1;
    private int everyDay = -1;

    internal void FirstSetting()
    {
        levelUpPackageButton = UnityCommonFunc.GetGameObjectByName(gameObject, "LevelUpPackageButton");
        itemBoxList = UnityCommonFunc.GetGameObjectByName(gameObject, "ItemBoxList");
        levelUpPackageButton.SetActive(false);
    }

    private void PackageBoxAllClier()
    {
        for (int i = 0; i < itemPackageBoxList.Count; ++i)
        {
            Destroy(itemPackageBoxList[i]);
            itemPackageBoxList[i].transform.parent = GameCore.Instance.Ui_root;
        }
        itemPackageBoxList.Clear();
    }

    internal void ChangeEventPackage(int index, int type, bool _force = false)
    {
        if (!_force && type == nowType && index == nowIndex) return;
        nowType = type;
        nowIndex = index;

        PackageBoxAllClier();

        //레벨업 패키지 관련 처리 중 일부내용 삭제 메모장 첨부.

        //index =  0 : HOT&NEW , 1 : 일일 혜택 상품 , 2 : 레벨업 패키지 , 3 : 패키지 상품.
        if (type != 2) SetItemBoxList_TypePackage(index, type);
        else if(type == 2) SetItemBox_TypeLevelUp(index, type);
    }

    internal void SetInquiryList(Dictionary<int, List<ShopInquirySData>> shopInquirySDataDic)
    {
        shopInquirySData = shopInquirySDataDic;
    }

    public List<ShopPackageSData> GetShopPackages(int _id)
    {
        List<ShopPackageSData> list = new List<ShopPackageSData>();
        foreach (var sdata in shopInquirySData)
        {
            for(int i = 0; i < sdata.Value.Count; ++i)
                foreach (var data in sdata.Value[i].PackageListSData)
                    if (data.id == _id)
                        list.Add(data);
        }

        return list;
    }

    /// <summary>
    /// TypePackage와 다른 구조인 레벨업 패키지를 위한 출력 및 셋팅 함수
    /// </summary>
    /// <param name="index"> </param>
    /// <param name="type"> 레벨업 패키지 값 : 2 ( 2020-02-17 현재까지 예외 사항 없음 : 이현철 )</param>
    internal void SetItemBox_TypeLevelUp(int index, int type)
    {
        List<ShopPackageSData> data;

        // 2 : 레벨업 패키지의 Type 값
        data = shopInquirySData[2][0].PackageListSData;

        if (data[index].pr >= 1)
        {
            // 기존 버튼을 초기화한 후 새로운 이벤트와 Label데이터를 셋팅
            levelUpPackageButton.GetComponent<UIButton>().onClick.Clear();
            levelUpPackageButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { OnClickItemPackageBox(data, index); }));
            levelUpPackageButton.gameObject.SetActive(true);
            levelUpPackageButton.transform.GetChild(1).GetComponent<UILabel>().text = string.Format("₩ {0:N0}", data[index].price);
        }
        else levelUpPackageButton.gameObject.SetActive(false);

        for (int i = 0; i < data[index].rewardSData.Count; ++i)
        {
            itemPackageBoxList.Add(GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/ItemPackageBox", itemBoxList.transform));

            int temp = i;
            itemPackageBoxList[i].transform.GetChild(1).GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { OnClickTakeItem(data[index], temp); }));

            //아이템 이름에 맞게 택스트 비/활성화.
            itemPackageBoxList[i].transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            itemPackageBoxList[i].transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
            itemPackageBoxList[i].transform.GetChild(1).GetChild(2).gameObject.SetActive(true);

            //아이템 박스 이름.
            itemPackageBoxList[i].transform.GetChild(0).GetComponent<UILabel>().text = "LV." + data[index].rewardSData[i].con + " 달성 시";

            //패키지 상품인지 아닌지 판단해서 퀘스쳔마크 띄움.
            if ((data[index].rewardSData[i].id / 1000000) == 2) itemPackageBoxList[i].transform.GetChild(3).gameObject.SetActive(true);

            //해당 레벨을 체크해서 획득 버튼의 클릭가능 유무를 체크.
            if (data[index].rewardSData[i].con > GameCore.Instance.PlayerDataMgr.Level)
            {
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<UISprite>().color = new Color32(0xFF, 0xFF, 0xFF, 0x3C);
            }
            if (data[index].rewardSData[i].reward >= 1)
            {
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<UISprite>().spriteName = "BOX_02_01";
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<UISprite>().color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
                itemPackageBoxList[i].transform.GetChild(1).GetChild(2).GetComponent<UILabel>().text = "수령 완료";
            }

            //구매 여부를 판단해서 버튼 오브젝트의 비/활성화 체크
            if (levelUpPackageButton.activeSelf) itemPackageBoxList[i].transform.GetChild(1).gameObject.SetActive(false);
            else itemPackageBoxList[i].transform.GetChild(1).gameObject.SetActive(true);

            //패키지 이미지 테이블 정보가 없어서 나중에 예외처리 제거하면 잘 돌아갈 것.
            var key = data[index].rewardSData[i].id;
            if ((key / 1000000) == 3)
            {
                var card = CardBase.CreateBigCardByKey(data[index].rewardSData[i].id, itemPackageBoxList[i].transform.GetChild(2).transform) as ItemCardBase;
                int cnt = data[index].rewardSData[i].value;
                if ( cnt <= 1)   card.GetCountLabel().text = "x 1";
                else             card.SetCount(cnt);
            }
            else
            {
                ItemCardBase card = CardBase.CreateBigCardByKey(3000001, itemPackageBoxList[i].transform.GetChild(2).transform) as ItemCardBase;
                int cnt = data[index].rewardSData[i].value;
                if( cnt <= 1)   card.GetCountLabel().text = "x 1";
                else            card.SetCount(cnt);

                if (CardDataMap.IsUnitKey(key) && GameCore.Instance.DataMgr.GetUnitData(key).IsExpCard())
                {
                    var expCardData = GameCore.Instance.DataMgr.GetUnitData(key);
                    GameCore.Instance.SetUISprite(card.GetSprite(), 1153020 - expCardData.rank);

                }
                else if (0 < data[i].texture)
                {
                    GameCore.Instance.SetUISprite(card.GetSprite(), data[index].texture);
                }
                else
                {
                    card.GetSprite().spriteName = "";
                    Debug.LogError("Can't find Texture. " + data[index].texture);
                }

                card.SetPressCallback(() =>
                {
                    GameCore.Instance.ShowCardInfoNotHave(key, data[index].texture);
                });
            }

        }
        //다시 생성하고 변경하는 작업이 끝났으니 재정렬을 시작하라.
        itemBoxList.GetComponent<UIGrid>().Reposition();

        var banner = GameCore.Instance.DataMgr.GetShopPackageData(shopInquirySData[2][0].Id).bannerID;
        GameCore.Instance.SetUISprite(backIllustration, banner);
    }

    internal void GetTakeItem(int id, int contidion)
    {
        CheckTakeItem(id, contidion);
    }

    /// <summary>
    /// 레벨업 패키지를 제외한 다른 상품을 출력 및 셋팅하는 함수.
    /// </summary>
    /// <param name="index"> 외부에서 들어오는 인덱스 정보. </param>
    /// <param name="type"> 0 : HOT&NEW , 1 : 일일 혜택 상품 , 2 : 레벨업 패키지 , 3 : 패키지 상품. , 4: 아이템(펄) </param>
    private void SetItemBoxList_TypePackage(int index, int type)
    {
        // LevelUpPackage에서 넘어올 경우를 대비하여 비활성화 문을 추가.
        levelUpPackageButton.gameObject.SetActive(false);

        // TODO : ANS_SHOP_INQUITYRY에 대한 자료 확인.
        // ANS_SHOP_INQUIRY를 통하여 초기화된 shopInquirySData에 type index에 맞게 데이터를 긁어와 셋팅
        // 서버로 부터 JSONData를 받아와 초기화하는 방식이기떄문에 테이블 확인하여 참고할 것.
        List<ShopPackageSData> data;
        data = shopInquirySData[type][index].PackageListSData;


        for (int i = 0; i < data.Count; ++i)
        {
            int number = i;
            itemPackageBoxList.Add(GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/ItemPackageBox", itemBoxList.transform));

            // 금액이 출력되는 부분에 구매 관련된 이벤트를 추가
            // 물음표 박스에 OnClickQuestionButton 이벤트를 추가.
            itemPackageBoxList[i].transform.GetChild(1).GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { OnClickItemPackageBox(data, number); }));
            itemPackageBoxList[i].transform.GetChild(3).GetComponent<UIButton>().onClick.Add(new EventDelegate(() => { OnClickQuestionButton(data, number); }));

            // id값이 2,000,000 ~ 2,999,999 사이의 값일 경우에만 QuestionButton이 떠오르도록 설정
            if ((data[i].id / 1000000) == 2) itemPackageBoxList[i].transform.GetChild(3).gameObject.SetActive(true);
        }
        for (int i = 0; i < itemPackageBoxList.Count; ++i)
        {
            int temp = i;
            //패키지 박스의 버튼 상태를 구분한다.
            SetPackageBoxButton(index, temp, type);

            // 패키지 이미지 테이블 정보가 없어서 나중에 예외처리 제거하면 잘 돌아갈 것.
            if (CardDataMap.IsItemKey(data[i].id))
            {
                // 카드 정보를 자식오브젝트에서 획득하여 카드를 생성한 뒤, 라벨 데이터 변경.
                var card = CardBase.CreateBigCardByKey(data[i].id, itemPackageBoxList[i].transform.GetChild(2).transform) as ItemCardBase;
                int cnt = data[index].rewardSData.Count;
                if ( cnt <= 1)   card.GetCountLabel().text = "x 1";
                else             card.SetCount(cnt);
            }
            else
            {
                // 맵 데이터에 Key값이 없을 경우에 새로운 카드를 제작하여 생성한뒤, 라벨 데이터를 변경한다.
                // 단, 이 경우에는 text에 x 1값으로 들어가며 기존 data가 가지고있는 texture를 사용한다.
                ItemCardBase card = CardBase.CreateBigCardByKey((int)ResourceType.Gold, itemPackageBoxList[i].transform.GetChild(2).transform) as ItemCardBase;
                card.GetCountLabel().text = "x 1";
                if (0 < data[i].texture)
                {
                    GameCore.Instance.SetUISprite(card.GetSprite(), data[i].texture);
                }
                else
                {
                    card.GetSprite().spriteName = "";
                    Debug.LogError("Can't find Texture. " + data[i].texture);
                }

                card.SetPressCallback(() =>
                {
                    GameCore.Instance.ShowCardInfoByStore(data[temp]);
                });
            }
        }
        // 생성 및 변경작업 종료후 재 정렬
        // 정렬은 중앙 하단을 기준으로 횡으로 퍼지는 형식이다.
        itemBoxList.GetComponent<UIGrid>().Reposition();

        // 데이터 메니저를 통하여 패널정보를 가져와 배너의 이미지를 변경.
        var pdata = GameCore.Instance.DataMgr.GetShopPackageData(shopInquirySData[type][index].Id);
        if (pdata != null)
            GameCore.Instance.SetUISprite(backIllustration, pdata.bannerID);
    }

    /// <summary>
    /// 패키지의 구매와 관련된 버튼의 각종 정보를 변경하는 함수
    /// </summary>
    /// <param name="inquiryIndex"> </param>
    /// <param name="rewardIndex"> </param>
    /// <param name="type"> 상점 종류에 따른 타입값. </param>
    private void SetPackageBoxButton(int inquiryIndex, int rewardIndex, int type)
    {
        List<ShopPackageSData> data;
        data = shopInquirySData[type][inquiryIndex].PackageListSData;

        // 아이템 이름 변경
        itemPackageBoxList[rewardIndex].transform.GetChild(0).GetComponent<UILabel>().text = data[rewardIndex].name;

        // 아이템 타입에 따른 이름 설정 변경
        switch (shopInquirySData[type][inquiryIndex].type)
        {
            // 1번 case의 경우 월정액등의 제품처럼 일수로 나눠야하는 경우 사용된다.
            case 1:
                everyDay = data[rewardIndex].period;
                itemPackageBoxList[rewardIndex].transform.GetChild(1).GetChild(0).GetComponent<UILabel>().text = data[rewardIndex].strperiod + "일";
                if (data[rewardIndex].pr > 0)
                {
                    itemPackageBoxList[rewardIndex].transform.GetChild(1).GetChild(1).GetComponent<UILabel>().text = "₩ " + string.Format("{0:N0}", data[rewardIndex].price);
                }
                else
                {
                    itemPackageBoxList[rewardIndex].transform.GetChild(1).GetChild(0).GetComponent<UILabel>().text = data[rewardIndex].strperiod + "일";
                    NotInStockItem_EveryDay(rewardIndex);
                }
                break;
            // 그외에 회수로 표기해야하는 경우나, 가격으로만 표기되어야하는 경우에 사용되는것.
            default:
                if (data[rewardIndex].pr > 0)
                {
                    itemPackageBoxList[rewardIndex].transform.GetChild(1).GetChild(0).GetComponent<UILabel>().text = string.Format("{0} 회", data[rewardIndex].pr);
                    itemPackageBoxList[rewardIndex].transform.GetChild(1).GetChild(1).GetComponent<UILabel>().text = "₩ " + string.Format("{0:N0}", data[rewardIndex].price);
                }
                else if (data[rewardIndex].pr == 0)
                {
                    NotInStockItem(rewardIndex);
                }
                else
                {
                    itemPackageBoxList[rewardIndex].transform.GetChild(1).GetChild(0).GetComponent<UILabel>().text = string.Empty;
                    itemPackageBoxList[rewardIndex].transform.GetChild(1).GetChild(1).GetComponent<UILabel>().text = "₩ " + string.Format("{0:N0}", data[rewardIndex].price);
                }
                break;
        }
    }
    
    private void NotInStockItem(int index)
    {
        itemPackageBoxList[index].transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
        itemPackageBoxList[index].transform.GetChild(1).GetComponent<UISprite>().spriteName = "BOX_02_01";
        itemPackageBoxList[index].transform.GetChild(1).GetComponent<UISprite>().color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        itemPackageBoxList[index].transform.GetChild(1).GetChild(0).GetComponent<UILabel>().text = "";
        itemPackageBoxList[index].transform.GetChild(1).GetChild(1).GetComponent<UILabel>().text = "소진됨";
        itemPackageBoxList[index].transform.GetChild(1).GetChild(1).GetComponent<UILabel>().alignment = NGUIText.Alignment.Center;
    }

    private void NotInStockItem_EveryDay(int index)
    {
        itemPackageBoxList[index].transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
        itemPackageBoxList[index].transform.GetChild(1).GetComponent<UISprite>().spriteName = "BOX_02_01";
        itemPackageBoxList[index].transform.GetChild(1).GetComponent<UISprite>().color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        itemPackageBoxList[index].transform.GetChild(1).GetChild(1).GetComponent<UILabel>().text = "사용중";
    }

    private void NotInStockLevelUpPackage(List<ShopPackageSData> data, int index)
    {
        for (int i = 0; i < data[index].rewardSData.Count; ++i)
        {
            if (data[index].rewardSData[i].con > GameCore.Instance.PlayerDataMgr.Level)
            {
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<UISprite>().color = new Color32(0xFF, 0xFF, 0xFF, 0x3C);
            }
            itemPackageBoxList[i].transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    private void SetButton(UIButton button, EventDelegate.Callback callBack, string label)
    {
        button.onClick.Add(new EventDelegate(callBack));
    }

    /// <summary>
    /// 팝업창을 띄워 확인처리 되었을때 결재를 진행하는 함수
    /// </summary>
    internal void OnClickItemPackageBox(List<ShopPackageSData> data, int index)
    {
        GameCore.Instance.ShowAgree("구매 팝업", "다음 캐쉬를 사용하여 선택하신 상품을\n 구매하시겠습니까?", string.Format("{0:N0}", data[index].price), MoneyType(2), 0, ()=> 
        {
            GameCore.Instance.CloseMsgWindow();
            GameCore.Instance.NetMgr.Req_Shop_Buy(data[index].id);

            //GameCore.Instance.NetMgr.Req_Shop_Inquiry();

            //if (nowType == 2) NotInStockLevelUpPackage(data, index);
            //else if (nowType == 0)
            //{
            //    NotInStockItem(index);
            //}
            //else if (nowType == 1)
            //{
            //    NotInStockItem_EveryDay(index);
            //    itemPackageBoxList[index].transform.GetChild(1).GetChild(0).GetComponent<UILabel>().text = string.Format("{0:N0}", everyDay - 1) + "일";
            //}
            //levelUpPackageButton.gameObject.SetActive(false);
            
        });
    }

    /// <summary>
    /// 지정된 아이템의 세부 정보를 새로운 창으로 띄우는 함수 ( 데이터는 서버로부터 받아온 테이블 기반으로 참조함 )
    /// </summary>
    /// <param name="itemList"> 서버로부터 받아온 테이블 데이터 </param>
    /// <param name="index"> </param>
    private void OnClickQuestionButton(List<ShopPackageSData> itemList, int index)
    {
        questionBox = ShopQuestionBoxComponent.Create(gameObject.transform);
        questionBox.GetPackageRewardSData(itemList, index);
        questionBox.PackageInit();

        if (itemList[index].pr != 0)
        {
            GameCore.Instance.ShowObject("구매 팝업", null, questionBox.gameObject, 0,
                new MsgAlertBtnData[] {
                   new MsgAlertBtnData("닫기", new EventDelegate(GameCore.Instance.CloseMsgWindow)),
                    new MsgAlertBtnData(string.Format("₩ {0:N0}", itemList[index].price), new EventDelegate(()=> { GameCore.Instance.CloseMsgWindow(); OnClickItemPackageBox(itemList, index); }))
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

    private void OnClickTakeItem(ShopPackageSData itemList, int index)
    {
        GameCore.Instance.NetMgr.Req_Shop_Take_Item(itemList.id, itemList.rewardSData[index].con);
    }

    private void CheckTakeItem(int id, int con)
    {
        var datas = shopInquirySData[2][0].PackageListSData;
        ShopPackageSData data = null;

        for(int i = 0; i < datas.Count; ++i)
        {
            if(id == datas[i].id)
            {
                data = datas[i];
                break;
            }
        }

        for(int i = 0; i < data.rewardSData.Count; ++i)
        {
            if(con == data.rewardSData[i].con)
            {
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<UISprite>().spriteName = "BOX_02_01";
                itemPackageBoxList[i].transform.GetChild(1).GetComponent<UISprite>().color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
                itemPackageBoxList[i].transform.GetChild(1).GetChild(2).GetComponent<UILabel>().text = "수령 완료";
            }
        }

        data.rewardSData[0].value = 1;
    }

    private MoneyType MoneyType(int index)
    {
        if (index == 0)      return global::MoneyType.Pearl;
        else if (index == 1) return global::MoneyType.Gold;
        else                 return global::MoneyType.Cash;
    }

}

