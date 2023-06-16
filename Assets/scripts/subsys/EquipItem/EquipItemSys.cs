using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

internal class EquipItemSys : SubSysBase, ISequenceAction
{
    EquipItemUI ui;
    static HeroInfoPara para;

    int cachedPower;

    public EquipItemSys() : base(SubSysType.EquipItem)
    {
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Item,
        };
    }

    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);

        if (_para.GetPara<HeroInfoPara>().returnSys != SubSysType.None)
            para = _para.GetPara<HeroInfoPara>();

        Name = "장비 장착";

        ui = EquipItemUI.Create(GameCore.Instance.ui_root);
        ui.Init(GameCore.Instance.PlayerDataMgr.GetUnitSData(para.uid));
        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, ui.GetTutorialTransformList);
    }

    protected override void ExitSysInner(ParaBase _para)
    {
        base.ExitSysInner(_para);
        GameObject.Destroy(ui.gameObject);
        ui = null;
    }

    protected override void RegisterHandler()
    {
        handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
        handlerMap.Add(GameEventType.ANS_ITEM_EQUIP, ANS_ITEM_EQUIP);
        handlerMap.Add(GameEventType.ANS_ITEM_EQUIP_CHANGE, ANS_ITEM_EQUIP_CHANGE);
        handlerMap.Add(GameEventType.ANS_ITEM_UNEQUIP, ANS_ITEM_UNEQUIP);
        base.RegisterHandler();
    }

    internal override void ClickBackButton()
    {
        if (GameCore.Instance.CommonSys.tbUi.GetFriendType() == true) GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu()) GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        GameCore.Instance.ChangeSubSystem(SubSysType.HeroInfo, para);
    }


    bool ANS_ITEM_EQUIP(ParaBase _para)
    {
        var json = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        json.GetField(ref code, "result");
        ui.bRunningAutoEquip = false;

        switch (code)
        {
            case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_EquipItem);
                var list = json.GetField("ITEM_LIST");
                if (list.type == JSONObject.Type.ARRAY)
                {
                    for (int i = 0; i < list.Count; ++i)
                    {
                        long item_uid = -1;
                        long cha_uid = -1;
                        list[i].GetField(ref item_uid, "ITEM_UID");
                        list[i].GetField(ref cha_uid, "CHA_UID");
                        if (0 < cha_uid)
                        {
                            GameCore.Instance.PlayerDataMgr.SetEquip(EquipItemSys.para.uid, item_uid);
                            ui.SetEquipItem(item_uid, false);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Invalid Data!");
                }

                GameCore.Instance.PlayerDataMgr.SetCardSData(list);
                var nowPower = GameCore.Instance.PlayerDataMgr.GetUnitSData(para.uid).GetPower();
                ui.UpdatePower(nowPower - ui.cachedPower, false);
                ui.ShowPowerTextAnimation();
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "데이터 누락", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "존재하지 않는 캐릭터", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "존재하지 않는 아이템", 0); break;
            case 5: GameCore.Instance.ShowNotice("실패", "해당 아이템 장착 중", 0); break;
            case 6: GameCore.Instance.ShowNotice("실패", "현재 해당 위치 장비 착용 중", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_ITEM_EQUIP_CHANGE(ParaBase _para)
    {
        var json = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        json.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var prevPower = GameCore.Instance.PlayerDataMgr.GetUnitSData(para.uid).GetPower();

                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_EquipItem);
                var list = json.GetField("ITEM_LIST");
                if (list.type == JSONObject.Type.ARRAY)
                {
                    for (int i = 0; i < list.Count; ++i)
                    {
                        long item_uid = -1;
                        long cha_uid = -1;
                        list[i].GetField(ref item_uid, "ITEM_UID");
                        list[i].GetField(ref cha_uid, "CHA_UID");
                        if (0 < cha_uid)
                        {
                            GameCore.Instance.PlayerDataMgr.SetEquip(EquipItemSys.para.uid, item_uid);
                            ui.SetEquipItem(item_uid);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Invalid Data!");
                }

                GameCore.Instance.PlayerDataMgr.SetCardSData(list);
                var nowPower = GameCore.Instance.PlayerDataMgr.GetUnitSData(para.uid).GetPower();
                ui.UpdatePower(nowPower - prevPower, false);
                ui.ShowPowerTextAnimation();
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "데이터 누락", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "현재 해당 위치 장비 착용 중", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_ITEM_UNEQUIP(ParaBase _para)
    {
        var json = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        json.GetField(ref code, "result");
        ui.bRunningAutoEquip = false;

        switch (code)
        {
            case 0:
                var prevPower = GameCore.Instance.PlayerDataMgr.GetUnitSData(para.uid).GetPower();

                var list = json.GetField("ITEM_LIST");
                if (list.type == JSONObject.Type.ARRAY)
                {
                    for (int i = 0; i < list.Count; ++i)
                    {
                        long item_uid = -1;
                        list[i].GetField(ref item_uid, "ITEM_UID");
                        var slotIdx = EquipItemUI.GetEquipTypeIdx(GameCore.Instance.PlayerDataMgr.GetItemSData(item_uid));
                        GameCore.Instance.PlayerDataMgr.SetUnequip(EquipItemSys.para.uid, slotIdx);
                        ui.SetUnequip(slotIdx);
                    }
                }
                else
                {
                    Debug.LogError("Invalid Data!");
                }

                GameCore.Instance.PlayerDataMgr.SetCardSData(list);
                var nowPower = GameCore.Instance.PlayerDataMgr.GetUnitSData(para.uid).GetPower();

                if (ui.bAutoEquip)
                {
                    // 자동 교체
                    ui.SetAutoEquip();
                }
                else if (0 < ui.equipItemUID)
                {
                    // 교체
                    
                    //ui.UpdatePower(nowPower - prevPower, false);
                    //ui.ShowPowerTextAnimation();

                    ui.TryEquipItem(ui.equipItemUID);
                    ui.cachedPower = prevPower;
                }
                else
                {
                    // 빼기
                    ui.UpdatePower(nowPower - prevPower, false);
                    ui.ShowPowerTextAnimation();
                }


                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "데이터 누락", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }
    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        nActionList.Add(() => {
            var tutoData = GameCore.Instance.PlayerDataMgr.TutorialData.Clone();
            tutoData.main = 6;
            GameCore.Instance.NetMgr.Req_Account_Change_Tutorial(tutoData);
            GameCore.Instance.lobbyTutorial.TurnOnTutorial(()=> { });
        });

        return nActionList;
    }
}
