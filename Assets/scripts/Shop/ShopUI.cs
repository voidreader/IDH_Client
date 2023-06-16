using System;
using System.Text;
using System.Timers;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopUI : MonoBehaviour {

    ShopEventPackageUI eventPackageUI;
    ShopItemUI itemUI;
    ShopAttentionComponent attentionPopUp;

    [SerializeField]    UIButton attentionButton;

    private UIButton[] mainButton = new UIButton[5];                                    //Left Button Hot&New, 패키지, 일일혜택, 레벨업, 아이템, 스킨   
    private UISprite[] mainButtonSprite = new UISprite[5];                              //메인버튼 이미지
    private GameObject[] subButtonList = new GameObject[3];                             //Hot&New, 패키지 하위버튼.

    private Dictionary<int, List<ShopInquirySData>> inquiryList = new Dictionary<int, List<ShopInquirySData>>();//상점 진입 시 기본적으로 서버에서 쥐여 주는 정보.
    private ShopInquiryItemSkinSData inquiryItemList = new ShopInquiryItemSkinSData();  //상점 진입 시 기본적으로 서버에서 쥐여 주는 아이템 정보.
    private ShopInquiryItemSkinSData inquirySkinList = new ShopInquiryItemSkinSData();  //상점 진입 시 기본적으로 서버에서 쥐여 주는 스킨 정보.

    List<GameObject> subButton_HOT = new List<GameObject>();
    List<GameObject> subButton_Package = new List<GameObject>();
    List<GameObject> subButton_LevelUp = new List<GameObject>();

    int nowTabIdx;                                                                      // 각종 OnClick에서 사용되고 UpdateToBuy에서 구매를 위하여 사용됨.
    int nowSubTabIdx;

    internal void Init()
    {
        // 각 mainButton의 인덱스 요소값.
        // 0 : Hot&New
        // 1 : 패키지
        // 2 : 일일 혜택
        // 3 : 레벨업 패키지
        // 4 : 아이템 ( 펄 ) 
        for(int i = 0; i < 5; ++i)
        {
            mainButton[i] = UnityCommonFunc.GetComponentByName<UIButton>(gameObject.transform.GetChild(1).gameObject, "MainButton" + (i + 1));
            mainButtonSprite[i] = UnityCommonFunc.GetComponentByName<UISprite>(gameObject.transform.GetChild(1).gameObject, "MainButton" + (i + 1));
        }
        for(int i = 0; i < subButtonList.Length; ++i)
        {
            subButtonList[i] = UnityCommonFunc.GetGameObjectByName(gameObject.transform.GetChild(1).gameObject, "SubPanel_" + (i + 1));
        }

        SetButton(mainButton[0], () => OnClickHotAndNew(), "HOT & NEW");
        SetButton(mainButton[1], () => OnClickPackage(), "패키지 상품");
        SetButton(mainButton[2], () => OnClickOneDayPackage(), "일일 혜택 상품");
        SetButton(mainButton[3], () => OnClickLavelUPPackage(), "레벨업 패키지");
        SetButton(mainButton[4], () => OnClickItemShop(), "아이템 상점");
        //SetButton(mainButton[5], OnClickSkinShop, "스킨 상점");

        SetButton(attentionButton, OnClickAttentionButton, null);
    }

    List<int> inquiryListNumber_HOT = new List<int>();
    List<int> inquiryListNumber_Package = new List<int>();
    List<int> inquiryListNumber_LevelUp = new List<int>();

    //하위버튼 생성.
    private void CreateSubButton()
    {
        List<string> hotName = new List<string>();
        List<string> packageName = new List<string>();
        List<string> levelUpName = new List<string>();

        foreach (var data in inquiryList)
        {
            if (data.Value[0].type == 0)
            {
                for (int j = 0; j < data.Value.Count; ++j)
                {
                    subButton_HOT.Add(GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/button", subButtonList[0].transform));
                    hotName.Add(data.Value[j].packageName);
                    inquiryListNumber_HOT.Add(j);
                }
            }
            else if(data.Value[0].type == 3)
            {
                for (int j = 0; j < data.Value.Count; ++j)
                {
                    subButton_Package.Add(GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/button", subButtonList[1].transform));
                    packageName.Add(data.Value[j].packageName);
                    inquiryListNumber_Package.Add(j);
                }
            }
            else if(data.Value[0].type == 2)
            {
                for (int j = 0; j < data.Value[0].PackageListSData.Count; ++j)
                {
                    subButton_LevelUp.Add(GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/button", subButtonList[2].transform));
                    levelUpName.Add(data.Value[0].PackageListSData[j].name);
                    inquiryListNumber_LevelUp.Add(j);
                }
            }
        }

        for (int i = 0; i < subButton_HOT.Count; i++)
        {
            int testNum = i;
            SetButton(subButton_HOT[i].GetComponent<UIButton>(), ()=> { OnClickHotAndNewSub(testNum); SetEventPrepab(inquiryListNumber_HOT[testNum], 0); }, hotName[i]);

        }
        for(int i = 0; i < subButton_Package.Count;++i)
        {
            int testNum = i;
            SetButton(subButton_Package[i].GetComponent<UIButton>(), ()=> { OnClickPackageSub(testNum); SetEventPrepab(inquiryListNumber_Package[testNum], 3); }, packageName[i]);
        }
        for(int i = 0; i  < subButton_LevelUp.Count; ++i)
        {
            int testNum = i;
            SetButton(subButton_LevelUp[i].GetComponent<UIButton>(), () => { OnClickLevelUpSub(testNum); SetEventPrepab(inquiryListNumber_LevelUp[testNum], 2); }, levelUpName[i]);
        }
    }

    //패키지 / 아이템 UI생성 및 초기화.
    private void CreatePackageUI()
    {
        if (eventPackageUI == null)
        {
            eventPackageUI = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/ShopEventPackageUI", gameObject.transform).GetComponent<ShopEventPackageUI>();
            eventPackageUI.FirstSetting();
        }
        if (itemUI == null)
        {
            itemUI = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/ItemShopUI", gameObject.transform).GetComponent<ShopItemUI>();
            itemUI.gameObject.SetActive(false);
        }
    }

    //스킨관련 정보가 전무하므로 일단 이것도 스탑시켜놓고 나중에 다시 작업하도록 한다.
    private void CreateSkinUI()
    {

    }

    private void SetButton(UIButton button, EventDelegate.Callback callBack, string label)
    {
        button.onClick.Add(new EventDelegate(callBack));

        if (label == null) return;
        UnityCommonFunc.GetComponentByName<UILabel>(button.gameObject, "Label").text = label;
    }

    /// <summary>
    /// 각 버튼들의 기본 정보를 초기화하고 자식으로 들어가있는 서브버튼들을 활성화하는 함수
    /// </summary>
    /// <param name="index"> button들의 구분 번호 </param>
    private void SelectMainButton(int index)
    {
        for (int i = 0; i < mainButtonSprite.Length; ++i)
        {
            //각 버튼들의 이미지, 크기, 라벨위치 초기화.
            mainButtonSprite[i].spriteName = "SBTN_02";
            mainButton[i].gameObject.transform.localScale = new Vector3(1.0f, 1.0f);
            mainButton[i].transform.GetChild(1).gameObject.SetActive(false);
            mainButton[i].gameObject.GetComponentInChildren<UILabel>().pivot = UIWidget.Pivot.Left;
        }
        //셀렉한 버튼의 이미지, 크기, 라벨위치 변경.
        mainButtonSprite[index].spriteName = "SBTN_01";
        mainButton[index].gameObject.transform.localScale = new Vector3(1.1f, 1.1f);
        mainButton[index].transform.GetChild(1).gameObject.SetActive(true);
        mainButton[index].gameObject.GetComponentInChildren<UILabel>().pivot = UIWidget.Pivot.Center;

        //이벤트 패키지 서브 버튼 초기화
        for (int i = 0; i < subButtonList.Length; ++i)
        {
            if (subButtonList[i].activeSelf) subButtonList[i].SetActive(false);
        }
        //메인버튼에 따른 서브버튼 활성화.
        if (index == 0) subButtonList[0].SetActive(true);
        if (index == 1) subButtonList[1].SetActive(true);
        if (index == 3) subButtonList[2].SetActive(true);
    }

    private void SelectSubButtonColor(GameObject button, bool isCheck = false)
    {
        if (isCheck) button.GetComponent<UISprite>().color = new Color32(0xF6, 0x00, 0xFF, 0xFF);
        else button.GetComponent<UISprite>().color = new Color32(0x72, 0x00, 0xFF, 0xFF);
    }

    private void OnClickAttentionButton()
    {
        Application.OpenURL(CSTR.URL_SubscriptionWithdrawal);
        //string text = "";
        //text = "주문 취소 및 반품\n 일반적으로 소비자는 자신이 체결한 전자상거래 계약에 대해 그 계약의 내용을 불문하고 그 청약철회 및 계약해제의 기간(통상 7일) 내에는 청약철회 등을 자유롭게 할 수 있습니다(「전자상거래 등에서의 소비자보호에 관한 법률」 제17조제1항).\n※ 소비자에게 불리한 규정(주문 취소나 반품 금지 등)이 포함된 구매계약은 효력이 없습니다(「전자상거래 등에서의 소비자보호에 관한 법률」 제35조).\n 하지만, 다음 어느 하나에 해당하는 경우에는 인터넷쇼핑몰 사업자의 의사에 반(反)해서 주문 취소 및 반품을 할 수 없습니다(「전자상거래 등에서의 소비자보호에 관한 법률」 제17조제2항 본문 및 「전자상거래 등에서의 소비자보호에 관한 법률 시행령」 제21조).\n1.소비자의 잘못으로 물건이 멸실(물건의 기능을 할 수 없을 정도로 전부 파괴된 상태)되거나 훼손된 경우(다만, 내용물을 확인하기 위해 포장을 훼손한 경우에는 취소나 반품이 가능)\n2.소비자가 사용해서 물건의 가치가 뚜렷하게 떨어진 경우\n3.시간이 지나 다시 판매하기 곤란할 정도로 물건의 가치가 뚜렷하게 떨어진 경우\n4.복제가 가능한 물건의 포장을 훼손한 경우\n5.용역 또는 「문화산업진흥 기본법」 제2조제5호의 디지털콘텐츠의 제공이 개시된 경우.다만, 가분적 용역 또는 가분적 디지털콘텐츠로 구성된 계약의 경우에는 제공이 개시되지 않은 부분은 제외\n6.소비자의 주문에 따라 개별적으로 생산되는 상품 또는 이와 유사한 상품 등의 청약철회 및 계약해제를 인정하는 경우 인터넷쇼핑몰 사업자에게 회복할 수 없는 중대한 피해가 예상되는 경우로서 사전에 주문 취소 및 반품이 되지 않는다는 사실을 별도로 알리고 소비자의 서면(전자문서 포함)에 의한 동의를 받은 경우\n인터넷쇼핑몰 사업자는 위 2.부터 5.까지의 사유에 해당하여 청약철회 등이 불가능한 상품에 대해 그 사실을 상품의 포장이나 그 밖에 소비자가 쉽게 알 수 있는 곳에 명확하게 적거나 시험 사용 상품을 제공하는 등의 방법으로 청약철회 등의 권리 행사가 방해받지 않도록 조치해야 합니다(「전자상거래 등에서의 소비자보호에 관한 법률」 제17조제6항 본문).만약 사업자가 이와 같은 조치를 안했다면, 소비자는 청약철회 등의 제한사유에도 불구하고 청약철회 등을 할 수 있습니다(「전자상거래 등에서의 소비자보호에 관한 법률」 제17조제2항 단서).\n 다만, 위의 5.중 디지털콘텐츠에 대하여 소비자가 청약철회 등을 할 수 없는 경우에는 청약철회 등이 불가능하다는 사실의 표시와 함께 다음의 어느 하나의 방법에 따라 시험 사용 상품을 제공하는 등의 방법으로 청약철회 등의 권리 행사가 방해받지 않도록 해야 합니다(「전자상거래 등에서의 소비자보호에 관한 법률」 제17조제6항 단서 및 「전자상거래 등에서의 소비자보호에 관한 법률 시행령」 제21조의2).\n√ 일부 이용의 허용: 디지털콘텐츠의 일부를 미리보기, 미리듣기 등으로 제공\n√ 한시적 이용의 허용: 일정 사용기간을 설정하여 디지털콘텐츠 제공\n√ 체험용 디지털콘텐츠 제공: 일부 제한된 기능만을 사용할 수 있는 디지털콘텐츠 제공\n√ 위의 방법으로 시험 사용 상품 등을 제공하기 곤란한 경우: 디지털콘텐츠에 관한 정보 제공";
        //attentionPopUp = ShopAttentionComponent.Create(gameObject.transform);
        //attentionPopUp.Init(text);

        //GameCore.Instance.ShowObject("청약 철회", null, attentionPopUp.gameObject, 1,
        //    new MsgAlertBtnData[] { new MsgAlertBtnData() {text = "확인", ed = new EventDelegate(GameCore.Instance.CloseMsgWindow) }});
    }

    #region 메인버튼 클릭시 사용되는 함수들

    // NOTE : 등록되어있으나 현재 사용되지 않는 함수.
    private void OnClickHotAndNew(bool _force = false)
    {
        nowTabIdx = 0;
        SelectMainButton(0);
        SetEventPrepab(inquiryListNumber_HOT[0], 0, _force);
        OnClickHotAndNewSub(0);

        EventPackageUIOpen();
    }

    private void OnClickPackage(bool _force = false)
    {
        nowTabIdx = 1;
        SelectMainButton(1);
        SetEventPrepab(inquiryListNumber_Package[0], 3, _force);
        OnClickPackageSub(0);

        EventPackageUIOpen();
    }

    private void OnClickOneDayPackage(bool _force = false)
    {
        nowTabIdx = 2;
        SelectMainButton(2);
        SetEventPrepab(0, 1, _force);

        EventPackageUIOpen();
    }

    private void OnClickLavelUPPackage(bool _force = false)
    {
        nowTabIdx = 3;
        SelectMainButton(3);
        SetEventPrepab(inquiryListNumber_LevelUp[0], 2, _force);
        OnClickLevelUpSub(0);

        EventPackageUIOpen();
    }

    /// <summary>
    /// 다른 OnClick관련 Shop함수들과 다르게 배치 구조가 다르기때문에 개별적으로 구현되어있음.
    /// </summary>
    private void OnClickItemShop()
    {
        nowTabIdx = 4;
        SelectMainButton(4);

        ItemUIOpen();
    }

    #endregion

    ////스킨관련 정보가 전무하므로 일단 이것도 스탑시켜놓고 나중에 다시 작업하도록 한다.
    //private void OnClickSkinShop()
    //{
    //    nowTabIdx = 5;
    //    SelectMainButton(5);
    //}

    // HOT&NEW의 서브버튼 클릭 시 실행되는 함수.
    private void OnClickHotAndNewSub(int index)
    {
        for(int i = 0; i < subButton_HOT.Count; ++i)
        {
            SelectSubButtonColor(subButton_HOT[i]);
        }
        SelectSubButtonColor(subButton_HOT[index], true);
    } 

    // 패키지 서브버튼 클릭 시 실행되는 함수.
    private void OnClickPackageSub(int index)
    {
        for(int i = 0; i < subButton_Package.Count; ++i)
        {
            SelectSubButtonColor(subButton_Package[i]);
        }
        SelectSubButtonColor(subButton_Package[index], true);
    }

    // 레벨업 패키지 서브버튼 클릭시 실행되는 함수.
    private void OnClickLevelUpSub(int index)
    {
        for(int i = 0; i < subButton_LevelUp.Count; ++i)
        {
            SelectSubButtonColor(subButton_LevelUp[i]);
        }
        SelectSubButtonColor(subButton_LevelUp[index], true);
    }

    private void SetEventPrepab(int index, int type, bool _force = false)
    {
        nowSubTabIdx = index;
        eventPackageUI.ChangeEventPackage(index, type, _force);
    }

    // 서버에서 주는 기본 정보를 받아오는 함수.
    internal void SetInquirySData(Dictionary<int, List<ShopInquirySData>> shopInquirySDataDic, ShopPara _shopPara)
    {
        inquiryList = shopInquirySDataDic;

        CreatePackageUI();

        eventPackageUI.SetInquiryList(shopInquirySDataDic);

        if (subButton_HOT.Count != 0 && subButton_Package.Count != 0 && subButton_LevelUp.Count != 0) return;

        CreateSubButton();

        mainButton[0].gameObject.SetActive(subButton_HOT.Count != 0);
        mainButton[1].gameObject.SetActive(subButton_Package.Count != 0);
        mainButton[3].gameObject.SetActive(subButton_LevelUp.Count != 0);

        //for(int i = 0; i < 6; ++i)
        //{
        //    if (mainButton[i] != mainButton[4]) mainButton[i].gameObject.SetActive(false);
        //}
    }

    internal void GetInquiryItemSData(ShopInquiryItemSkinSData sdata, ShopPara _shopPara)
    {
        itemUI.GetItemSData(sdata, _shopPara);
    }

    internal void GetInquirySkinSData(ShopInquiryItemSkinSData sdata, ShopPara _para)
    {
        if (_para == null)
            return;

        switch (_para.openTab)
        {
            case 0: itemUI.UpdateUI(); break;
            case 1: OnClickPackage(true); break;
            case 2: OnClickOneDayPackage(true); break;
            default:
                break;
        }
    }

    internal void GetTakeItemSData(int id, int condition)
    {
        eventPackageUI.GetTakeItem(id, condition);
    }

    internal void EventPackageUIOpen()
    {
        eventPackageUI.gameObject.SetActive(true);
        if (itemUI != null) itemUI.gameObject.SetActive(false);
    }

    internal void ItemUIOpen()
    {
        itemUI.gameObject.SetActive(true);
        eventPackageUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        gameObject.transform.GetChild(1).GetComponent<VerticalLayoutGroup>().enabled = false;
        gameObject.transform.GetChild(1).GetComponent<VerticalLayoutGroup>().enabled = true;
    }

    internal void UpdateToBuy(JSONObject _goods)
    {
        var id = 0;
        _goods.GetField(ref id, "ID");
        if (eventPackageUI.gameObject.activeSelf)
        {
            var list = eventPackageUI.GetShopPackages(id);
            foreach (var data in list)
            {
                data.SetData(_goods);
                if (0 < data.pr)
                    data.pr -= 1;
                if (0 < data.period)
                    data.period -= 1;
            }
        }
        else
        {
            // itemUI Nothing do
            var list = itemUI.GetShopPackages(id);
            foreach (var data in list)
            {
                data.SetData(_goods);
                data.FIRST = false;
            }
        }

        if (nowTabIdx == 4)
            OpenTab(nowTabIdx, itemUI.nowItemType);
        else
            OpenTab(nowTabIdx, nowSubTabIdx);
    }

    public void OpenTab(int _tab, int pos)
    {
        // 해당 탭이 없을 때 인덱스 강제 변경
        if (_tab == 0 && inquiryListNumber_HOT.Count == 0)     { _tab = 1; pos = 0; }
        if (_tab == 1 && inquiryListNumber_Package.Count == 0) { _tab = 2; pos = 0; }
        if (_tab == 3 && inquiryListNumber_LevelUp.Count == 0) { _tab = 2; pos = 0; }

        SelectMainButton(_tab);

        if (_tab == 4)  ItemUIOpen();
        else            EventPackageUIOpen();

        switch (_tab)
        {
            case 0: SetEventPrepab(inquiryListNumber_HOT[pos], 0, true);     OnClickHotAndNewSub(pos);  break;// OnClickHotAndNew(true); break;
            case 1: SetEventPrepab(inquiryListNumber_Package[pos], 3, true); OnClickPackageSub(pos);    break; //OnClickPackage(true); break;
            case 2: SetEventPrepab(0, 1, true);                                                         break; // OnClickOneDayPackage(true); break; 
            case 3: SetEventPrepab(inquiryListNumber_LevelUp[pos], 2, true); OnClickLevelUpSub(pos);    break; // OnClickLavelUPPackage();
            case 4: /*itemUI.UpdateUI();*/ itemUI.OnClickButton(pos); itemUI.SelectButton(pos);         break;
        }

        nowTabIdx = _tab;
        if (nowTabIdx == 4) itemUI.nowItemType = pos;
        else                nowSubTabIdx = pos;
    }
}
