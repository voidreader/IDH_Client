using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ResultBasePara : ParaBase
{
    internal InGameType type;       // 게임 타입 (Story, PvP, Daily, Raid)
    internal bool clear;			// 클리어 여부
    internal int playerTeamIdx;     // 사용한 팀 번호
    internal TimeSpan playTime;     // 플레이 시간
    internal List<int> isDead;         // 캐릭터가 죽었는지 확인

    internal static ResultStoryPara CreateStory(bool _clear, int _playerTeamIdx, TimeSpan _playTime, int _storyKey, bool[] _mission, int[] _DeadMonsters, int _ovkDmg)
    {
        return new ResultStoryPara()
        {
            type = InGameType.Story,
            clear = _clear,
            playerTeamIdx = _playerTeamIdx,
            playTime = _playTime,
            storyKey = _storyKey,
            mission = _mission,
            DeadMonsters = _DeadMonsters,
            ovkDmg = _ovkDmg
        };
    }

    internal static ResultDailyPara CreateDaily(bool _clear, int _playerTeamIdx, TimeSpan _playTime, int _DDungeonKey)
    {
        return new ResultDailyPara()
        {
            type = InGameType.Daily,
            clear = _clear,
            playerTeamIdx = _playerTeamIdx,
            playTime = _playTime,
            DDungeonKey = _DDungeonKey
        };
    }

    internal static ResultPvPPara CreatePvP(bool _win, int _playerTeamIdx, TimeSpan _playTime, ResultPvPUI.From _from, int _myCombat, int _oppCombet, int _historyUID)
    {
        return new ResultPvPPara()
        {
            type = InGameType.PvP,
            clear = _win,
            playerTeamIdx = _playerTeamIdx,
            playTime = _playTime,
            from = _from,
            myCombat = _myCombat,
            oppCombet = _oppCombet,
            historyUID = _historyUID
        };
    }

    internal static ResultRaidPara CreateRaid(bool _win, int _playerTeamIdx, TimeSpan _playTime, int _raidKey, int addedDmg)
    {
        return new ResultRaidPara()
        {
            type = InGameType.Raid,
            clear = _win,
            playerTeamIdx = _playerTeamIdx,
            playTime = _playTime,
            raidKey = _raidKey,
            addedDmg = addedDmg
        };
    }
}

internal class ResultStoryPara : ResultBasePara
{
    internal int storyKey;         // 스테이지 키
    internal bool[] mission;        // 클리어한 미션
    internal int[] DeadMonsters;    // 잡은 몬스터
    internal int ovkDmg;            // 오버킬 값
}

internal class ResultDailyPara : ResultBasePara
{
    internal int DDungeonKey;         // 요일던전 키
}
internal class ResultPvPPara : ResultBasePara
{
    internal ResultPvPUI.From from;
    internal int myCombat;      // 플레이어 전투력
    internal int oppCombet;     // 상대 전투력
    public long historyUID = -1; // Revenge일경우 UID. 아닐경우 -1
}
internal class ResultRaidPara : ResultBasePara
{
    internal int raidKey;
    internal int addedDmg;
}

internal class ResultSys : SubSysBase, ISequenceAction
{
    ResultUIBase ui;
    ResultBasePara resultPara;
    private bool clear;
    private int presentStoryKey;
    internal ResultSys() : base(SubSysType.Result)
    {
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Team,
        };

        preloadingBundleKeys = new int[] {
                CommonType.DEF_KEY_BGM_RESULT_W,
                CommonType.DEF_KEY_BGM_RESULT_L,
                CommonType.DEF_KEY_SFX_UI
        };
    }

    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);
        resultPara = _para.GetPara<ResultBasePara>();

        if (resultPara.clear)
            GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Mission_Success, false, false);
        //GameCore.Instance.SndMgr.PlayBGM(BGM.Result_Win, false);
        else
            GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Mission_Fail, false, false);
        //GameCore.Instance.SndMgr.PlayBGM(BGM.Result_Lose, false);

        if (ui == null)
            ui = ResultUIBase.Create(GameCore.Instance.Ui_root, resultPara.type);
        ui.Init(resultPara);

        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, ui.GetTutorialTransformList);
        GameCore.Instance.TurnOnTutorial(() => { }, 1, 4, 5);

        GameCore.Instance.SetActionAfterLoading(() => {
            if (ui != null)
                ui.GetComponent<Animator>().enabled = true;
        });

        switch (resultPara.type)
        {
            case InGameType.Story:
                {
                    clear = resultPara.clear;
                    var pr = resultPara.GetPara<ResultStoryPara>();
                    presentStoryKey = pr.storyKey;
                    GameCore.Instance.NetMgr.req_Story_Done(resultPara.clear, pr.storyKey, pr.mission, pr.ovkDmg, pr.playerTeamIdx+1);
                    var sdata = GameCore.Instance.PlayerDataMgr.GetStorySData(pr.storyKey);
                    for (int i = 0; i < pr.mission.Length; i++)
                        pr.mission[i] |= sdata.missionClear[i];
                }
                break;

            case InGameType.Daily:
                {
                    var pr = resultPara.GetPara<ResultDailyPara>();
                    GameCore.Instance.NetMgr.Req_Dungeon_Daily_End(pr.DDungeonKey, resultPara.clear);
                }
                break;

            case InGameType.PvP:
                {
                    var pr = resultPara.GetPara<ResultPvPPara>();
                    switch (pr.from)
                    {
                        case ResultPvPUI.From.PvP:
                            GameCore.Instance.NetMgr.Req_PvPFinishBattle(pr.myCombat, pr.oppCombet, pr.clear);
                            break;

                        case ResultPvPUI.From.Placement:
                            GameCore.Instance.NetMgr.Req_PvPFinishPlacement(pr.clear);
                            break;

                        case ResultPvPUI.From.Revenge:
                            GameCore.Instance.NetMgr.Req_MyRoomFinishRevenge(pr.clear, pr.historyUID);
                            break;
                    }
                }
                break;
            case InGameType.Raid:
                {
                    var pr = resultPara.GetPara<ResultRaidPara>();
                    GameCore.Instance.NetMgr.Req_Raid_End(pr.raidKey, pr.addedDmg, (float)pr.playTime.TotalSeconds, pr.playerTeamIdx);
                }
                break;

            default:
                break;
        }
    }

    protected override void RegisterHandler()
    {
        handlerMap = new Dictionary<GameEventType, Func<ParaBase, bool>>();
        handlerMap.Add(GameEventType.ANS_STORY_DONE, ANS_STORY_DONE);
        handlerMap.Add(GameEventType.ANS_DUNGEON_DAILY_END, ANS_DUNGEON_DAILY_END);

        handlerMap.Add(GameEventType.ANS_PVP_FINISHPLACEMENT, ANS_PVP_FINISHPLACEMENT);
        handlerMap.Add(GameEventType.ANS_PVP_FINISHBATTLE, ANS_PVP_FINISHBATTLE);
        handlerMap.Add(GameEventType.ANS_MYROOM_FINISH_REVENGE, ANS_MYROOM_FINISH_REVENGE);

        handlerMap.Add(GameEventType.ANS_RAID_END, ANS_RAID_END);

        base.RegisterHandler();
    }

    protected override void ExitSysInner(ParaBase _para)
    {
        base.ExitSysInner(_para);

        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.Lobby);

        GameObject.Destroy(ui.gameObject);
        ui = null;
    }

    private bool ANS_STORY_DONE(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var account = para.GetField("ACCOUNT");
                if (account != null) GameCore.Instance.PlayerDataMgr.SetDataAccount(account);

                var storyList = para.GetField("STORY_LIST");
                if (storyList != null)
                    GameCore.Instance.PlayerDataMgr.SetDataStory(storyList);
                
                ui.UpdateData();
                int lastStoryKey = GameCore.Instance.PlayerDataMgr.GetStorySDataDic;
                if (clear == false && presentStoryKey >= lastStoryKey)
                    ui.TurnOffNextButton();

                var rewardList = para.GetField("CHAPTER_REWARD_LIST");
                if (rewardList != null) GameCore.Instance.PlayerDataMgr.SetDataStoryChapter(rewardList);

                var list = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                ui.SetRewardItem(list);

                // Ovk reward
                GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("OVERKILL"));
                // 오버킬 리워드 프린트는 이미 출력되어 있으므로 하지 않는다.


                // 애드브릭스
                AdBrixStageClear(resultPara as ResultStoryPara);


                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "부접합한 data", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "스테이지 데이터 틀림", 0); break;
            case 4: GameCore.Instance.ShowNotice("실패", "활성화되지 않는 스테이지 데이터", 0); break;
            case 5: return true; // Stage Fail
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_DUNGEON_DAILY_END(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var account = para.GetField("ACCOUNT");
                if (account != null) GameCore.Instance.PlayerDataMgr.SetDataAccount(account);

                var list = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                ui.SetRewardItem(list);
                ui.UpdateData();

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    private bool ANS_PVP_FINISHPLACEMENT(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.PlayerDataMgr.PvPData.SetData(para.GetField("PVP")[0]);
                var grade = GameCore.Instance.PlayerDataMgr.PvPData.grade;
                ui.UpdateData();

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "요청 파라미터 누락", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    private bool ANS_PVP_FINISHBATTLE(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.PlayerDataMgr.PvPData.SetData(para.GetField("PVP")[0]);
                int addedScore = 0;
                para.GetField(ref addedScore, "SCORE");
                var rank = 0;
                para.GetField(ref rank, "GROUP_RANK");
                var rankGap = rank - GameCore.Instance.PlayerDataMgr.PvPGroupRank;
                GameCore.Instance.PlayerDataMgr.PvPGroupRank = rank;


                // Todo : Update Point Animation
                ui.UpdateData(addedScore, rankGap);

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "요청 파라미터 누락", 0); break;
            case 3:
                GameCore.Instance.ShowNotice("실패", "시즌 종료", 0);
                GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
                break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    private bool ANS_MYROOM_FINISH_REVENGE(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");
        switch (code)
        {
            case 0:
                ui.UpdateData();
                // Do Nothing
                return true;
            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }
        return false;
    }

    private bool ANS_RAID_END(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");
        switch (code)
        {
            case 0:
                var pr = resultPara.GetPara<ResultRaidPara>();
                var prevDmg = GameCore.Instance.PlayerDataMgr.GetRaidSData(pr.raidKey).damage;
                GameCore.Instance.PlayerDataMgr.SetRaidSData(para.GetField("RAID"));
                var addedDmg = GameCore.Instance.PlayerDataMgr.GetRaidSData(pr.raidKey).damage - prevDmg;

                var list = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));

                ui.SetRewardItem(list);
                ui.UpdateData(addedDmg);

                // Do Nothing
                return true;
            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }
        return false;
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        return nActionList;
    }



    /// <summary>
    /// 클리어시 애드브릭스에 보내는 스테이지 키값들
    /// </summary>
    static List<int> sendStageDatas = new List<int>{
        7000001,
        7000002,
        7000003,
        7000010,
        7000016
    };

    /// <summary>
    /// 클리어 정보를 애드브릭스로 보낸다.
    /// </summary>
    /// <param name="_para"></param>
    public void AdBrixStageClear(ResultStoryPara _para)
    {
        if (!_para.clear || !sendStageDatas.Contains(_para.storyKey))
            return;

        var data = GameCore.Instance.DataMgr.GetStoryData(_para.storyKey);
        #if UNITY_ANDROID
        // 스테이지 클리어
        // ADBrix제거
        //AdBrixRmAOS.AdBrixRm.gameStageCleared(string.Format("{0}-{1}", data.chapter, data.stage));
        #endif
    }
}
