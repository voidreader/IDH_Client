using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class RaidPreparePara : ParaBase
{
}

internal class RaidPrepareSys : SubSysBase
{
    public RaidPrepareSys() : base(SubSysType.RaidPrepare)
    {
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Team,
        };
    }

    RaidPrepareUI ui;
    int BOSS_NO;
    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);

        Name = "레이드";

        ui = RaidPrepareUI.Create(GameCore.Instance.ui_root);
        //ui.Init();

        if (_para != null)
        {
            var para = _para.GetPara<StoryPara>();
            ui.OnChangeTeam(para.selectedTeamIdx);
            ui.OnChangeDiffcult(para.storyKey);
        }

        GameCore.Instance.NetMgr.Req_Friend_Striker();
        GameCore.Instance.NetMgr.Req_Raid_Prepare();
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
        handlerMap.Add(GameEventType.ANS_FRIEND_STRIKER, ANS_FRIEND_STRIKER);
        handlerMap.Add(GameEventType.ANS_RAID_PREPARE, ANS_RAID_PREPARE);
        handlerMap.Add(GameEventType.ANS_RAID_START, ANS_RAID_START);

        handlerMap.Add(GameEventType.ANS_RAID_MYRANK, ANS_RAID_MYRANK);
        handlerMap.Add(GameEventType.ANS_RAID_RANK50, ANS_RAID_RANK50);
        handlerMap.Add(GameEventType.ANS_RAID_TEAM_INFO, ANS_RAID_TEAM_INFO);
        base.RegisterHandler();
    }


    void RaidSDataUpdate(int _bossNo)
    {
        // Clear? 
        var sdata = GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(0);
        if(sdata == null || GameCore.Instance.DataMgr.GetRaidData(sdata.key).group != _bossNo)
            GameCore.Instance.PlayerDataMgr.ClearRaidSData();

        // Generate
        var it = ((DataMapCtrl<RaidDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.Raid)).GetEnumerator();
        while(it.MoveNext())
            if (it.Current.Value.group == _bossNo && GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(it.Current.Value.difficult-1) == null)
                GameCore.Instance.PlayerDataMgr.AddRaidSData(new RaidSData() { key = it.Current.Value.id });
    }


    bool ANS_FRIEND_STRIKER(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var friendList = para.GetField("FRIEND_LIST");
                var recommandList = para.GetField("RECOMMAND_LIST");

                FriendSData[] friends = new FriendSData[friendList.Count];
                for (int i = 0; i < friends.Length; ++i)
                {
                    friends[i] = new FriendSData();
                    friends[i].SetData(friendList[i]);
                }

                FriendSData[] recommands = new FriendSData[recommandList.Count];
                for (int i = 0; i < recommands.Length; ++i)
                {
                    recommands[i] = new FriendSData();
                    recommands[i].SetData(recommandList[i]);
                }

                ui.SetFriend(friends, recommands);
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_RAID_PREPARE(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.PlayerDataMgr.SetRaidSData(para.GetField("RAID"));
                para.GetField(ref BOSS_NO, "BOSS_NO");

                RaidSDataUpdate(BOSS_NO);
                ui.Init();
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_RAID_START(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var sdata = GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(ui.SelectedDiff);
                GameCore.Instance.ChangeSubSystem(SubSysType.RaidBattle, new RaidBattlePara() { playerTeam = ui.SelectedTeamIdx, type = InGameType.Raid, stageId = sdata.key, damage = GameCore.Instance.PlayerDataMgr.GetRaidSData(sdata.key).damage });
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "티켓 부족", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "전투력 부족", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "존재하지 않는 레이드 ID", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_RAID_MYRANK(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var json = para.GetField("MY_RANK");

                var list = new List<RaidRankSData>();
                for (int i = 0; i < json.Count; ++i)
                {
                    var data = new RaidRankSData();
                    data.SetData(json[i]);
                    list.Add(data);
                }

                ui.SetRankItemData(list);

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_RAID_RANK50(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var json = para.GetField("TOP");

                var list = new List<RaidRankSData>();
                for (int i = 0; i < json.Count; ++i)
                {
                    var data = new RaidRankSData();
                    data.SetData(json[i]);
                    list.Add(data);
                }

                ui.SetRankItemData(list);
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_RAID_TEAM_INFO(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var list = para.GetField("RANK_DATA").GetField("CHA_LIST");
                List<PvPOppUnitSData> units = new List<PvPOppUnitSData>();
                if (list != null)
                {
                    for (int i = 0; i < list.Count; ++i)
                    {
                        var unit = new PvPOppUnitSData();
                        unit.SetData(list[i]);
                        units.Add(unit);
                    }
                }
                var teamInfo = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("UI/Components/TeamInfo", ui.transform);
                teamInfo.GetComponent<PvPTeamInfoComponent>().SetData(units, RaidRankListItemScript.lastActiveItem.data.USER_NAME);
                GameCore.Instance.ShowObject("", null, teamInfo, 4, new MsgAlertBtnData[] { new MsgAlertBtnData() { text = "확인", enable = true, ed = new EventDelegate(() => GameCore.Instance.CloseMsgWindow()), clickSound = SFX.Sfx_UI_Button } });
                RaidRankListItemScript.lastActiveItem = null;
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }
}
