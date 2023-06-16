using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class DailyDungeonSys : SubSysBase
{
    public DailyDungeonSys() : base(SubSysType.DailyPrepare)
    {
        needInitDataTypes = new InitDataType[] {
            InitDataType.Team,
        };
    }

    DailyDungeonUI ui;

    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);
        Name = "요일 던전";

        var selectedTemaIdx = 0;
        var openTabIdx = -1;
        var diff = -1;
        if (_para != null)
        {
            var para = _para.GetPara<StoryPara>();
            selectedTemaIdx = para.selectedTeamIdx;
            openTabIdx = para.storyKey & 0xFF;
            diff = para.storyKey >> 8;
        }

        ui = DailyDungeonUI.Create(GameCore.Instance.ui_root);
        ui.Init(selectedTemaIdx, openTabIdx, diff);


        GameCore.Instance.NetMgr.Req_Friend_Striker();
        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.DungeonScene);

        AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.Daily, 0);
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
        handlerMap.Add(GameEventType.ANS_DUNGEON_DAILY_START, ANS_DUNGEON_DAILY_START);
        //handlerMap.Add(GameEventType.ANS_DUNGEON_DAILY_END, ANS_DUNGEON_DAILY_END);
        handlerMap.Add(GameEventType.ANS_DUNGEON_DAILY_BUY_TICKET, ANS_DUNGEON_DAILY_BUY_TICKET);
        base.RegisterHandler();
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

    bool ANS_DUNGEON_DAILY_START(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                List<PvPOppUnitSData> friendCharList = new List<PvPOppUnitSData>();
                var oppFriendJson = para.GetField("FRIEND");
                if (oppFriendJson != null)
                {
                    for (int i = 0; i < oppFriendJson.GetField("LIST").Count; ++i)
                    {
                        PvPOppUnitSData data = new PvPOppUnitSData();
                        data.SetData(oppFriendJson.GetField("LIST")[i]);
                        friendCharList.Add(data);
                    }
                }
                int friendIcon;
                JsonParse.ToParse(oppFriendJson, "ICON", out friendIcon);
                int teamSkill;
                JsonParse.ToParse(oppFriendJson, "SKILL", out teamSkill);
                string UserName;
                JsonParse.ToParse(oppFriendJson, "UN", out UserName);

                List<StatInfos> statInfoList = new List<StatInfos>();
                var statInfos = para.GetField("STAT");
                for (int i = 0; i < statInfos.Count; i++)
                {
                    StatInfos statInfoStruct = new StatInfos(statInfos[i], false);
                    statInfoList.Add(statInfoStruct);
                }
                var enemyStatInfos = para.GetField("ENEMY");
                List<StatInfos> enemyStatInfoList = new List<StatInfos>();
                for (int i = 0; i < enemyStatInfos.Count; i++)
                {
                    StatInfos statInfoStruct = new StatInfos(enemyStatInfos[i], true);
                    enemyStatInfoList.Add(statInfoStruct);
                }

                GameCore.Instance.ChangeSubSystem(SubSysType.DailyBattle, new BattlePara() {
                    type = InGameType.Daily,
                    playerTeam = ui.GetSelectedTeamIndex(),
                    stageId = ui.GetSelecteDailyDungeonKey(),
                    friendName = UserName,
                    friendUnitList = friendCharList,
                    friendIcon = friendIcon,
                    friendTeamSkill = teamSkill,
                    unitStatInfosList = statInfoList,
                    enemyStatInfosList = enemyStatInfoList,
                });
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "티켓 부족", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "전투력 부족", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "존재하지 않는 던전", 0); break;
            case 5:
                break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_DUNGEON_DAILY_BUY_TICKET(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var list = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                ui.UpdateTiecket();
                --GameCore.Instance.PlayerDataMgr.LocalUserData.DailyDungeonRechargeAbleCount;
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "하루 구매 가능 횟수 초과", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;

            case 3:
                //GameCore.Instance.ShowNotice("실패", "재화 부족", 0);
                GameCore.Instance.ShowReduceResource(ResourceType.Cash);
                break;
            
        }

        return false;
    }

    internal void UpdateTicketCount()
    {
        ui.UpdateTiecket();
    }
}
