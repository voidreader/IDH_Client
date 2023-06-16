using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePotUnity;
using GamePotUnityAOS;

public class ShopPara : ParaBase
{
    public int openTab = 4; // 탭 넘버 ( 4 : 아이템탭, 0 ~ : 해당 타입 탭
    public int openPos;     // ~ 0 : 펄(기본)
                        // 1 : 골드
                        // 2 : 기타
}

internal class ShopSys : SubSysBase
{
    ShopUI ui;
    ShopPara shopPara;

    internal ShopSys() : base(SubSysType.Shop)
    {
        needInitDataTypes = new InitDataType[] {
        };
    }

    protected override void EnterSysInner(ParaBase _para)
    {
        Name = "상점";
        if (_para != null)
            shopPara = _para.GetPara<ShopPara>();
        else
            shopPara = null;

        //서버에다가 상점을 여는데 가장 기본적인 정보들을 요청한다.
        GameCore.Instance.NetMgr.Req_Shop_Inquiry();

        // Hierarchy에 등록되어있는 ShopUI를 찾아와 등록한뒤 초기화 과정을 실행.
        ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/PanelShopUI", GameCore.Instance.ui_root).GetComponent<ShopUI>();
        ui.Init();

        base.EnterSysInner(_para);

        GameCore.Instance.CommonSys.tbUi.CloseBottomBR();


        //NPurchaseItem[] items = GamePot.getPurchaseItems();
        //foreach (NPurchaseItem item in items)
        //{
        //    Debug.Log(item.productId);        // 상품ID
        //    Debug.Log(item.price);            // 가격
        //    Debug.Log(item.title);            // 제목
        //    Debug.Log(item.description);    // 설명
        //}

        //JSONObject json = new JSONObject("{\"ID\":2200001,\"TYPE\":3,\"MAIL_TYPE\":10,\"NAME\":\"aaa\", \"REWARD\":[{\"ID\":\"3000001\",\"CON\":\"-1\",\"VALUE\":\"100000\"},{\"ID\":\"3000002\",\"CON\":\"-1\",\"VALUE\":\"1000\"},{\"ID\":\"3000008\",\"CON\":\"-1\",\"VALUE\":\"10\"},{\"ID\":\"3000009\",\"CON\":\"-1\",\"VALUE\":\"10\"}],\"PERIOD\":-1,\"STRING_ID\":9600001,\"PR\":2,\"VALUE\":9900,\"SALE_TYPE\":2,\"CODE\":\"imageframe.idh.newpackage9900\"}");
        //cachedRewardJson = json.GetField("REWARD");
        //CBPurchase(true, "aa");

        //var goods = new JSONObject("{\"ID\":2200001,\"TYPE\":3,\"MAIL_TYPE\":10,\"NAME\":\"신규가입 - 브론즈\",\"REWARD\":[{\"ID\":3000001,\"CON\":-1,\"VALUE\":100000},{\"ID\":3000002,\"CON\":-1,\"VALUE\":1000},{\"ID\":3000008,\"CON\":-1,\"VALUE\":10},{\"ID\":3000009,\"CON\":-1,\"VALUE\":10}],\"PERIOD\":-1,\"STRING_ID\":9600001,\"PR\":2,\"VALUE\":9900,\"SALE_TYPE\":2,\"CODE\":\"imageframe.idh.newpackage9900\"}");
        //GameCore.Instance.ShowAgree("ss", "aasd", 0, () => {
        //     ui.UpdateToBuy(goods);
        // });
        
    }

    protected override void RegisterHandler()
    {
        handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
        handlerMap.Add(GameEventType.ANS_SHOP_BUY, ANS_SHOP_BUY);
        handlerMap.Add(GameEventType.ANS_SHOP_INQUIRY, ANS_SHOP_INQUIRY);
        handlerMap.Add(GameEventType.ANS_SHOP_INQUIRY_ITEM_SKIN, ANS_SHOP_INQUIRY_ITEM_SKIN);
        handlerMap.Add(GameEventType.ANS_SHOP_TAKE_ITEM, ANS_SHOP_TAKE_ITEM);
        handlerMap.Add(GameEventType.ANS_PURCHASE, ANS_PURCHASE);
        base.RegisterHandler();
    }

    protected override void ExitSysInner(ParaBase _para)
    {
        base.ExitSysInner(_para);
        GameCore.Instance.CommonSys.tbUi.UncloseBottomBR();
        if (ui != null)
        {
            GameObject.Destroy(ui.gameObject);
            ui = null;
        }
    }

    internal override void ClickBackButton()
    {
        base.ClickBackButton();
    }


    internal bool ANS_SHOP_INQUIRY(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;
        int code = -1;
        data.GetField(ref code, "result");

        switch(code)
        {
            case 0:
                var shopData = data.GetField("SHOP");
                Dictionary<int, List<ShopInquirySData>> shopInquirySDataDic = new Dictionary<int, List<ShopInquirySData>>();

                if(shopInquirySDataDic.Count != 0) shopInquirySDataDic.Clear();

                if (shopData.type == JSONObject.Type.ARRAY)
                {
                    for(int i = 0; i < shopData.Count; ++i)
                    {
                        ShopInquirySData shopinquirydata = new ShopInquirySData();
                        shopinquirydata.SetData(shopData[i]);
                        if (!shopInquirySDataDic.ContainsKey(shopinquirydata.type))
                            shopInquirySDataDic.Add(shopinquirydata.type, new List<ShopInquirySData>());

                        shopInquirySDataDic[shopinquirydata.type].Add(shopinquirydata);

                    }
                    //UI에 정보를 넘겨준다.
                    ui.SetInquirySData(shopInquirySDataDic, shopPara);

                    //자꾸 얘먼저 들어와서 itemUI가 null이 떠서 빡쳐서 정보 들어오면 보내라고 말함. 아이템과 스킨관련한 내용을 담아준다.
                    GameCore.Instance.NetMgr.Req_Shop_Inquiry_Item_Skin();
                }
                return true;
            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 요류", 0); break;
            default: break;
        }
        return false;
    }

    internal bool ANS_SHOP_INQUIRY_ITEM_SKIN(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;
        int code = -1;
        data.GetField(ref code, "result");
        if (code == 0)
        {
            var shopData = data.GetField("SHOP");
            List<ShopInquiryItemSkinSData> inquiryItemSkinSData = new List<ShopInquiryItemSkinSData>();

            if (inquiryItemSkinSData.Count != 0) inquiryItemSkinSData.Clear();
            if (shopData.type == JSONObject.Type.ARRAY)
            {
                for(int i = 0; i < shopData.Count; ++i)
                {
                    ShopInquiryItemSkinSData SData = new ShopInquiryItemSkinSData();
                    SData.SetData(shopData[i]);
                    inquiryItemSkinSData.Add(SData);

                    //2000007 : 아이템 관련 리스트. / 2000008 : 스킨 관련 리스트.
                    if (inquiryItemSkinSData[i].id == 2000007) ui.GetInquiryItemSData(inquiryItemSkinSData[i], shopPara);
                    else if (inquiryItemSkinSData[i].id == 2000008) ui.GetInquirySkinSData(inquiryItemSkinSData[i], shopPara);
                }
            }

            //OnClickItemShop();
            var tab = shopPara != null ? shopPara.openTab : 0;
            var pos = shopPara != null ? shopPara.openPos : 0;

            ui.OpenTab(tab, pos);

            return true;
        }
        else
        {
            GameCore.Instance.ShowNotice("실패", "쿼리 요류", 0);
            return false;
        }
    }


    JSONObject cachedRewardJson; // [{ID,VALUE}, ... ]
    internal bool ANS_SHOP_BUY(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;
        int code = -1;
        data.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var goods = data.GetField("GOODS");
                var pr = 0;
                goods.GetField(ref pr, "PR");
                if (pr == 0)
                {
                    GameCore.Instance.ShowNotice("실패", "남은 구매 기회가 없습니다.", 0);
                }
                else
                {
                    var saleType = 0;
                    goods.GetField(ref saleType, "SALE_TYPE");

                    if (saleType == 2) // 현금 결제
                    {
                        // Todo request Purchase
                        var productID = "";
                        goods.GetField(ref productID, "CODE");
                        RequestPurchase(productID);

                        cachedRewardJson = goods;
                    }
                    else // 아이템 재화 결제
                    {
                        UpdateShopData(goods);
                        GameCore.Instance.PlayerDataMgr.SetRewardItems(data.GetField("REWARD"));
                        GameCore.Instance.CommonSys.UpdateMoney();
                        //GameCore.Instance.ShowAlert("구매에 성공하였습니다. 구매하신 물품은 우편함으로 지급됩니다.");
                    }
                }

                return true;
            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 요류", 0); break;
            case 2: GameCore.Instance.ShowNotice("구매 불가", "보유 재화가 부족합니다.", 0); break;
            default: break;
        }
        return false;
    }

    internal bool ANS_SHOP_TAKE_ITEM(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;
        int code = -1;
        data.GetField(ref code, "result");

        switch(code)
        {
            case 0:
                int id = -1;
                int con = -1;
                var reward = data.GetField("REWARD");

                reward.GetField(ref id, "ID");
                reward.GetField(ref con, "CON");

                ui.GetTakeItemSData(id, con);
                GameCore.Instance.ShowAlert("수령에 성공하였습니다. 수령하신 물품은 우편함으로 지급됩니다.");
                return true;
            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 요류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "Request Parameter 누락", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "구매 이력 없음", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "레벨업 패키지 아님", 0); break;
            case 5: GameCore.Instance.ShowNotice("실패", "해당 조건 없음", 0); break;
            case 6: GameCore.Instance.ShowNotice("실패", "이미 보상 지급 됨", 0); break;
            case 7: GameCore.Instance.ShowNotice("실패", "조건 부적합", 0); break;
            case 8: GameCore.Instance.ShowNotice("실패", "보상 지급 완료.", 0); break;
            default: break;             
        }
        return false;
    }

    internal bool ANS_PURCHASE(ParaBase para)
    {
        var data = para.GetPara<PacketPara>().data.data;
        int code = -1;
        data.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var reward = data.GetField("REWARD");

                var list = new CardSData[reward.Count];
                for (int i = 0; i < reward.Count; ++i)
                {
                    int id = -1;
                    int cnt = -1;
                    reward[i].GetField(ref id, "REWARD_ITEM_ID");
                    reward[i].GetField(ref cnt, "VALUE");

                    if (CardDataMap.IsItemKey(id))  list[i] = new ItemSData(id, cnt);
                    else                            list[i] = new HeroSData(id);
                }

                GameCore.Instance.ShowReceiveItemPopup("구매 완료", "구매 완료되었습니다. 우편함을 확인해주세요.",list);

                GameCore.Instance.ShowAlert("수령에 성공하였습니다. 수령하신 물품은 우편함으로 지급됩니다.");
                return true;
            
            default: GameCore.Instance.ShowNotice("실패", "수령 실패 " + code, 0); break;
        }
        return true;
    }




    void RequestPurchase(string _productID)
    {
#if UNITY_EDITOR
        GameCore.Instance.ShowNotice("실패", "에디터에선 불가능합니다.", 0);
#else
        NPurchaseItem[] items = GamePot.getPurchaseItems();

        Debug.Log("Purchase Item Count : " + items.Length);
        //foreach (NPurchaseItem item in items)
        //    Debug.Log(item.productId + "  " + item.price + "  " + item.title + "  " +  item.description);

        foreach (NPurchaseItem item in items)
        {
            if (item.productId == _productID)
            {
                GamePot.purchase(_productID);
                GameCore.Instance.GamePotMgr.cbPurchase = CBPurchase;
                GameCore.Instance.NetMgr.IncReqCount("gamepot_purchase");
                return;
            }
        }

        GameCore.Instance.ShowNotice("구매 실패", "해당 상품을 찾지 못했습니다." + items.Length, 0);
#endif
    }

    void CBPurchase(bool _success, string _str)
    {
        GameCore.Instance.NetMgr.DecReqCount("gamepot_purchase");
        Debug.Log("Receive Purchase Return. " + _success + " " + _str);

        if (_success)
        {
            UpdateShopData(cachedRewardJson);
        }
        else
        {
            if (_str == null)
            {
                //canceled;
            }
            else
            {
                GameCore.Instance.ShowAlert("결제 실패 - " + _str);
            }
        }
    }

    void UpdateShopData(JSONObject _goods)
    { 
        int type = 0;
        _goods.GetField(ref type, "TYPE");

        switch(type)
        {
            case 1: // 월정액
            //    ReceiveBuyMonthPackage(_goods);
            //    GameCore.Instance.ShowNotice("구매", "구매완료되었습니다. 우편함을 확인해주세요.", 0);
            //    break;

            case 3: // 패키지
            case 4: // 아이템
            //    ReceiveBuyItem(_goods);
            //    break;

            case 2: // levelup
            
                GameCore.Instance.ShowNotice("구매", "구매완료되었습니다.", 0);
                break;
        }

        ui.UpdateToBuy(_goods);
    }

    void ReceiveBuyItem(JSONObject _goods)
    {
        bool first = false;
        int firstBonus = 0;
        _goods.GetField(ref first, "FIRST");

        if (first)
            _goods.GetField(ref firstBonus, "FIRST_BONUS");

        var rewardJson = _goods.GetField("REWARD");
        CardSData[] list = new CardSData[rewardJson.Count];
        for (int i = 0; i < list.Length; ++i)
        {
            int id = 0;
            int cnt = 0;
            int con = 0;
            rewardJson[i].GetField(ref id, "ID");
            rewardJson[i].GetField(ref cnt, "VALUE");
            rewardJson[i].GetField(ref con, "CON");

            if (0 < con) // 레벨업 패키지일 경우
                continue;

            if (CardDataMap.IsUnitKey(id)) list[i] = new HeroSData(id);
            else list[i] = new ItemSData(id, cnt + firstBonus);
        }

        GameCore.Instance.ShowReceiveItemPopup("구매", "구매 되었습니다. 우편함을 확인해주세요.", list);
    }

    void ReceiveBuyMonthPackage(JSONObject _goods)
    {
        var rewardJson = _goods.GetField("REWARD");
        
        int id = 0;
        int cnt = 0;
        int con = 0;
        rewardJson[0].GetField(ref id, "ID");
        rewardJson[0].GetField(ref cnt, "VALUE");
        rewardJson[0].GetField(ref con, "CON");

        CardSData[] list = new CardSData[1];
        if (CardDataMap.IsUnitKey(id)) list[0] = new HeroSData(id);
        else                           list[0] = new ItemSData(id, cnt);

        GameCore.Instance.ShowReceiveItemPopup("구매", "구매 되었습니다. 우편함을 확인해주세요.", list);
    }
}
