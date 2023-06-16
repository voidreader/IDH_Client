using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class HeroUpSys : SubSysBase, ISequenceAction
{
    public HeroUpSys() : base(SubSysType.HeroUp)
    {
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
        };
    }

    HeroUpUI ui;
    HeroInfoPara para;

    int cachedHeroKey;
    int cachedHeroEnchant;

    /// <summary>
    /// 캐싱용 클래스
    /// </summary>
    public StrengthenCostDataMap CostDataMap { get; private set; }


    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);

        Name = "영웅 성장";

        ui = HeroUpUI.Create(GameCore.Instance.ui_root);
        para = _para.GetPara<HeroInfoPara>();
        var unit = GameCore.Instance.PlayerDataMgr.GetUnitSData(para.uid);

        CostDataMap = new StrengthenCostDataMap();

        cachedHeroKey = unit.key;
        cachedHeroEnchant = unit.enchant;

        ui.Init(unit);
        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, ui.GetTutorialTransformList);

    }

    protected override void ExitSysInner(ParaBase _para)
    {
        base.ExitSysInner(_para);

        GameObject.Destroy(ui.gameObject);
    }

    protected override void RegisterHandler()
    {
        handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
        handlerMap.Add(GameEventType.ANS_CHARACTER_STRENGTHEN_EXP, ANS_CHARACTER_STRENGTHEN_EXP);
        handlerMap.Add(GameEventType.ANS_CHARACTER_STRENGTHEN, ANS_CHARACTER_STRENGTHEN);
        handlerMap.Add(GameEventType.ANS_CHARACTER_EVOLUTION, ANS_CHARACTER_EVOLUTION);
        base.RegisterHandler();
    }

    internal override void ClickBackButton()
    {
        //base.ClickBackButton();
        GameCore.Instance.ChangeSubSystem(SubSysType.HeroInfo, para);
    }

    bool ANS_CHARACTER_STRENGTHEN_EXP(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Strengthen);

                GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                GameCore.Instance.PlayerDataMgr.SetCardSData(para.GetField("CHA_LIST"));
                var cards = ui.GetOnSlotCards();
                for (int i = 0; i < cards.Length; ++i)
                    GameCore.Instance.PlayerDataMgr.RemoveUnit(cards[i].SData.uid);

                ui.ClearSlot();
                ui.UpdateInventoryCardList();
                ui.UpdateCardListHeight();
                ui.UpdateUnitInfo(false, false);

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "전송 데이터 누락", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "존재하지 않는 UID", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "현재 최대 강화 수치", 0); break;
            case 5: GameCore.Instance.ShowNotice("실패", "재료 합산 경험치 부족", 0); break;
            case 6: GameCore.Instance.ShowNotice("실패", "재화 부족", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }


    bool ANS_CHARACTER_STRENGTHEN(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Strengthen);

                GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                GameCore.Instance.PlayerDataMgr.SetCardSData(para.GetField("CHA_LIST"));
                var cards = ui.GetOnSlotCards();
                for(int i = 0; i < cards.Length; ++i)
                    GameCore.Instance.PlayerDataMgr.RemoveUnit(cards[i].SData.uid);
                
                ui.ClearSlot();
                ui.UpdateInventoryCardList();
                ui.UpdateCardListHeight();
                ui.UpdateUnitInfo(true, false);

                GameCore.Instance.SetActiveBlockPanelInvisable(true);
                var nowUnit = GameCore.Instance.PlayerDataMgr.GetUnitSData(this.para.uid);
                var prevUnit = new HeroSData() { key = cachedHeroKey, enchant = cachedHeroEnchant };
                GameCore.Instance.DoWaitCall(1.1f, () =>
                {
                    GameCore.Instance.SetActiveBlockPanelInvisable(false);
                    var popup = HeroUpPopup.Create(GameCore.Instance.ui_root);
                    popup.Init(nowUnit, prevUnit);
                    GameCore.Instance.ShowObject("강화", null, popup.gameObject, 4, new MsgAlertBtnData[1] {
                        new MsgAlertBtnData("확인", new EventDelegate(() => {
                            popup.StopParticle();
                            GameCore.Instance.CloseMsgWindow();
                        }), true, null, SFX.Sfx_UI_Confirm)
                    });
                });

                cachedHeroKey = nowUnit.key;
                cachedHeroEnchant = nowUnit.enchant;

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "전송 데이터 누락", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "존재하지 않는 UID", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "현재 최대 강화 수치", 0); break;
            case 5: GameCore.Instance.ShowNotice("실패", "재료 합산 경험치 부족", 0); break;
            case 6: GameCore.Instance.ShowNotice("실패", "재화 부족", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_CHARACTER_EVOLUTION(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Evolution);

                GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                GameCore.Instance.PlayerDataMgr.SetCardSData(para.GetField("CHA_LIST"));
                var cards = ui.GetOnSlotCards();

                ui.ClearSlot();
                ui.UpdateInventoryCardList();
                ui.UpdateCardListHeight();
                ui.UpdateUnitInfo(false, true);

                var nowUnitData = GameCore.Instance.PlayerDataMgr.GetUnitData(this.para.uid);
                var nowUnit = GameCore.Instance.PlayerDataMgr.GetUnitSData(this.para.uid);
                var prevUnit = new HeroSData() { key = cachedHeroKey, enchant = cachedHeroEnchant };

                cachedHeroKey = nowUnit.key;
                cachedHeroEnchant = nowUnit.enchant;

                GameCore.Instance.SetActiveBlockPanelInvisable(true);
                GameCore.Instance.DoWaitCall(1.9f, () =>
                {
                    GameCore.Instance.SetActiveBlockPanelInvisable(false);
                    var popup = HeroUpPopup.Create(GameCore.Instance.ui_root);
                    popup.Init(nowUnit, prevUnit);
                    GameCore.Instance.ShowObject(nowUnitData.evolLvl > 5 ? "각성" : "진화", 
                        null, popup.gameObject, 4, new MsgAlertBtnData[1] {
                        new MsgAlertBtnData("확인", new EventDelegate(() => {
                            popup.StopParticle();
                            GameCore.Instance.CloseMsgWindow();
                        }), true, null, SFX.Sfx_UI_Confirm)
                    });
                });



                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "전송 데이터 누락", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "존재하지 않는 UID", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "최대 강화 캐릭터가 아님", 0); break;
            case 5: GameCore.Instance.ShowNotice("실패", "최대 진화단계", 0); break;
            case 6: GameCore.Instance.ShowNotice("실패", "재화 부족", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        switch (tutorialNum)
        {
            case 7:
                nActionList.Add(() => {
                    var tutoData = GameCore.Instance.PlayerDataMgr.TutorialData.Clone();
                    tutoData.main = 7;
                    GameCore.Instance.NetMgr.Req_Account_Change_Tutorial(tutoData);
                    GameCore.Instance.lobbyTutorial.TurnOnTutorial(() => { });
                });
                break;
            default:
                break;
        }
        return nActionList;
    }
}
