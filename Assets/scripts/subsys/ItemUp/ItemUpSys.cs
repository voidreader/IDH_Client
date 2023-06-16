using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ItemUpSys : SubSysBase
{
    ItemUpUI ui;
    HeroInfoPara para;

    public ItemUpSys() : base(SubSysType.ItemUp)
    {
        needInitDataTypes = new InitDataType[] {
            InitDataType.Item,
        };
    }

    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);
        Name = "장비 성장";
        para = _para.GetPara<HeroInfoPara>();
        ui = ItemUpUI.Create(GameCore.Instance.ui_root);
        ui.Init(GameCore.Instance.PlayerDataMgr.GetItemSData(para.uid));
    }

    protected override void ExitSysInner(ParaBase _para)
    {
        base.ExitSysInner(_para);
        if (ui != null)
        {
            GameObject.Destroy(ui.gameObject);
            ui = null;
        }
    }

    protected override void RegisterHandler()
    {
        handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
        handlerMap.Add(GameEventType.ANS_ITEM_STRENGTHEN_EXP, ANS_ITEM_STRENGTHEN_EXP);
        handlerMap.Add(GameEventType.ANS_ITEM_STRENGTHEN, ANS_ITEM_STRENGTHEN);
        base.RegisterHandler();
    }

    internal override void ClickBackButton()
    {
        //base.ClickBackButton();
        if (para.returnSys == SubSysType.EquipItem)
        {
            var item = GameCore.Instance.PlayerDataMgr.GetItemSData(para.uid);
            GameCore.Instance.ChangeSubSystem(para.returnSys, new HeroInfoPara(item.equipHeroUID, SubSysType.None));
        }
        else
        {
            GameCore.Instance.ChangeSubSystem(para.returnSys, new StoryPara(1, false));
        }
    }

    bool ANS_ITEM_STRENGTHEN_EXP(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Strengthen);

                GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                GameCore.Instance.PlayerDataMgr.SetCardSData(para.GetField("ITEM_LIST"));
                var cards = ui.GetOnSlotCards();
                for (int i = 0; i < cards.Length; ++i)
                    GameCore.Instance.PlayerDataMgr.RemoveItem(cards[i].SData.uid, 1);

                ui.RemoveAllCardInSlot();
                ui.UpdateItemInfo(false);
                ui.UpdateCardListHeight();
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "전송 데이터 누락", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "존재하지 않는 아이템", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "최대 강화 단계", 0); break;
            case 5: GameCore.Instance.ShowNotice("실패", "경험치 부족", 0); break;
            case 6: GameCore.Instance.ShowNotice("실패", "재화 부족", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_ITEM_STRENGTHEN(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Strengthen);

                var oldItem = GameCore.Instance.PlayerDataMgr.GetItemSData(this.para.uid).Clone();
                var newItem = new ItemSData();
                var itemList = para.GetField("ITEM_LIST");
                newItem.SetData(itemList[0]);

                GameCore.Instance.SetActiveBlockPanelInvisable(true);

                GameCore.Instance.DoWaitCall(1.2f, () =>
                {
                    GameCore.Instance.SetActiveBlockPanelInvisable(false);
                    var popup = ItemUpPopup.Create(GameCore.Instance.ui_root);
                    popup.Init(newItem, oldItem);
                    GameCore.Instance.ShowObject("강화", null, popup.gameObject, 4, new MsgAlertBtnData[1] { new MsgAlertBtnData("확인", new EventDelegate(() =>
                        {
                            popup.StopParticle();
                            GameCore.Instance.CloseMsgWindow();
                        }), true, null, SFX.Sfx_UI_Confirm)
                    });
                });

                GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                GameCore.Instance.PlayerDataMgr.SetCardSData(para.GetField("ITEM_LIST"));
                var cards = ui.GetOnSlotCards();
                for (int i = 0; i < cards.Length; ++i)
                    GameCore.Instance.PlayerDataMgr.RemoveItem(cards[i].SData.uid, 1);

                ui.RemoveAllCardInSlot();
                ui.UpdateItemInfo(true);
                ui.UpdateCardListHeight();

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "전송 데이터 누락", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "존재하지 않는 아이템", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "최대 강화 단계", 0); break;
            case 5: GameCore.Instance.ShowNotice("실패", "경험치 부족", 0); break;
            case 6: GameCore.Instance.ShowNotice("실패", "재화 부족", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

}
