using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class StoryPara : ParaBase
{
    internal bool openPrepare;
	internal int storyKey;
    internal int selectedTeamIdx;
    internal int retry;

    public StoryPara(int _storyKey, bool _openPrepare, int _selectedTeamIdx = 0, int _retry = -1)
    {
        openPrepare = _openPrepare;
        storyKey = _storyKey;
        selectedTeamIdx = _selectedTeamIdx;
        retry = _retry;
    }
}
public struct MissionInfo
{
    internal int missionID;
    internal int missionValue1;
    internal int missionValue2;
    public MissionInfo(JSONObject jsonObject)
    {
        JsonParse.ToParse(jsonObject, "ID", out missionID);
        JsonParse.ToParse(jsonObject, "V1", out missionValue1);
        JsonParse.ToParse(jsonObject, "V2", out missionValue2);
    }
}
public class StatInfos
{
    int charUID;
    float[] statInfos;
    public float addedCritical;
    public float autoRecovery;
    public float reduceCool;

    internal int CharUID { get { return charUID; } }
    internal float[] StatInfo { get { return statInfos; } }

    public StatInfos(JSONObject jsonObject, bool isEnemy = false)
    {
        if (isEnemy == true)
            JsonParse.ToParse(jsonObject, "CHA_ID", out charUID);
        else
            JsonParse.ToParse(jsonObject, "CHA_UID", out charUID);
        JsonParse.ToParse(jsonObject, "STAT", out statInfos);

        JsonParse.ToParse(jsonObject, "CRITICAL", out addedCritical);
        JsonParse.ToParse(jsonObject, "AUTORECOVERY", out autoRecovery);
        JsonParse.ToParse(jsonObject, "REDUCECOOL", out reduceCool);
    }
    public float[] GetStatInfos(int _charUID)
    {
        if (charUID != _charUID)
            return null;
        return statInfos;
    }
}
internal class StorySys : SubSysBase, ISequenceAction
{
    static int lastStageKey = -1;

    StoryUI storyUI;
	PrepareUI prepareUI;

    int nowStageKey = -1;

    internal StorySys() : base(SubSysType.Story)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Story,
            InitDataType.Team,
            InitDataType.Character,
            InitDataType.ChapterReward,
            InitDataType.Item,
        };
    }

	protected override void EnterSysInner(ParaBase _para)
	{
        base.EnterSysInner(_para);
        Name = "스토리";

        // 10 클리어 후 다시 하기시 챕터 언락 에니메이션 스킵
        if (_para != null && _para.GetPara<StoryPara>().openPrepare && _para.GetPara<StoryPara>().retry == 0)
            GameCore.Instance.PlayerDataMgr.ResetUnlockStoryKey();

        storyUI = StoryUI.Create();
        storyUI.Init(OpenPrepare);

        prepareUI = PrepareUI.Create();
        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, storyUI.GetTutorialTransformList, prepareUI.GetTutorialTransformList);
        prepareUI.gameObject.SetActive(false);

        GameCore.Instance.CommonSys.UpdateMoney();
        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.StoryScene);
        if (_para != null && GameCore.Instance.PlayerDataMgr.CheckUnlockStory() == false)
        {
            var para = _para.GetPara<StoryPara>();
            if(para.retry != -1)
            {
                int storyKey = para.storyKey + para.retry;
                int chapterSaveInfo = GameCore.chapterSave;
                int chapter = chapterSaveInfo == -1 ?(storyKey - 7000000) / 10 + 1 : chapterSaveInfo;
                int difficult = (storyKey - 7000000) / 100000 + 1;
                storyUI.SetUI(chapter, difficult);
            }
            else
            {
                //GameCore에 저장된 Chapter정보가 없을경우(-1) 가장 최신의 챕터정보값을 가져온다.
                int chapterSaveInfo = GameCore.chapterSave;
                storyUI.SetChapter(chapterSaveInfo == -1 ? 
                                    GameCore.Instance.DataMgr.GetStoryData(para.storyKey).chapter : 
                                    chapterSaveInfo);
            }
            if (para.openPrepare)
                OpenPrepare(para.storyKey);
        }
        isOpen = false;
        GameCore.Instance.NetMgr.Req_Friend_Striker();
        //GameCore.Instance.NetMgr.Req_Notify_Friend_Mail_Count();
    }

	protected override void RegisterHandler()
	{
		handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
		handlerMap.Add(GameEventType.ANS_STORY_START, ANS_STORY_START);
		handlerMap.Add(GameEventType.ANS_STORY_REWARD, ANS_STORY_REWARD);
        handlerMap.Add(GameEventType.ANS_FRIEND_STRIKER, ANS_FRIEND_STRIKER);
        base.RegisterHandler();
	}

	protected override void ExitSysInner(ParaBase _para)
	{
		storyUI.Destroy();
		prepareUI.Destroy();
		base.ExitSysInner(_para);
	}

	internal override void ClickBackButton()
	{
        if (GameCore.Instance.CommonSys.tbUi.GetFriendType() == true) GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu()) GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        else if(prepareUI.gameObject.activeSelf)
        {
            storyUI.SetActive(true);// gameObject.SetActive(true);
            prepareUI.gameObject.SetActive(false);
			Name = "스토리";
            nowStageKey = -1;
        }
		else
        {
			if (!storyUI.GetNowGachaPlaying()) base.ClickBackButton();
		}
	}
    internal override void UpdateUI()
    {
        if(isOpen == false)
        {
            if (GameCore.Instance.IsDoneResourceLoad())
            {
                isOpen = true;
                GameCore.Instance.DoWaitCall(3, () => GameCore.Instance.CommonSys.ShowLoadingPage(false));
            }
        }
        storyUI.CheckLabelDiscPosition();
    }
    [SerializeField]bool isOpen;
    internal void OpenPrepare(int _stageInfoKey)
	{
        isOpen = false;

        lastStageKey = 
        nowStageKey = _stageInfoKey;

        prepareUI.Init(_stageInfoKey);
		var data = GameCore.Instance.DataMgr.GetStoryData(_stageInfoKey);
		Name = string.Format("{0}-{1}.{2}", data.chapter, data.stage, data.name);

		prepareUI.gameObject.SetActive(true);
		storyUI.SetActive(false);
	}


	private bool ANS_STORY_START(ParaBase _para)
	{
		prepareUI.SetActiveTouchGuard(false);

		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");
        
        switch (code)
		{
			case 0:
				

			case 1: //GameCore.Instance.ShowNotice("실패", "아이템 인벤토리가 꽉찼습니다. 공간을 확보해주세요.", 0); break;
			case 2: //GameCore.Instance.ShowNotice("실패", "영웅 인벤토리가 꽉찼습니다. 공간을 확보해주세요.", 0); break;
                List<PvPOppUnitSData> friendCharList = new List<PvPOppUnitSData>();
                var oppFriendJson = para.GetField("FRIEND");
                string UserName = string.Empty;
                int friendIcon = -1;
                int teamSkill = -1;
                if (oppFriendJson != null)
                {
                    for (int i = 0; i < oppFriendJson.GetField("LIST").Count; ++i)
                    {
                        PvPOppUnitSData data = new PvPOppUnitSData();
                        data.SetData(oppFriendJson.GetField("LIST")[i]);
                        friendCharList.Add(data);
                    }
                    JsonParse.ToParse(oppFriendJson, "UN", out UserName);
                    JsonParse.ToParse(oppFriendJson, "ICON", out friendIcon);
                    JsonParse.ToParse(oppFriendJson, "SKILL", out teamSkill);
                }

                if (friendCharList.Count == 0 && GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning && GameCore.Instance.lobbyTutorial.tutorialPos == 5)
                {
                    friendCharList.Clear();
                    friendCharList.Add(new PvPOppUnitSData() { position = 1, charID = 1000001, charStatInfos = new float[] { 0, 148, 368, 299, 134, 90, 269, 179, 403, 313 } });
                    friendCharList.Add(new PvPOppUnitSData() { position = 0, charID = 1000031, charStatInfos = new float[] { 0, 307, 279, 186, 279, 186, 186, 279, 186, 279 } });
                    friendCharList.Add(new PvPOppUnitSData() { position = 2, charID = 1000111, charStatInfos = new float[] { 0, 197, 269, 179, 269, 269, 358, 179, 179, 179 } });
                    UserName = "가짜친구";
                    friendIcon = 1000002;
                    teamSkill = 4300001;
                }

                List<StatInfos> statInfoList = new List<StatInfos>();
                var statInfos = para.GetField("STAT");
                for(int i = 0; i < statInfos.Count; i ++)
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
                var missionInfos = para.GetField("MISSION");
                List<MissionInfo> missionInfoList = new List<MissionInfo>();
                for(int i = 0; i < missionInfos.Count; i ++)
                {
                    MissionInfo nMissionInfo = new MissionInfo(missionInfos[i]);
                    missionInfoList.Add(nMissionInfo);
                }

                GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                //GameCore.Instance.ChangeSubSystem(SubSysType.Battle, BattlePara.CreateStoryPara(PrepareUI.selectedTeamIndex, prepareUI.storyKey));// { type = 0, playerTeam = PrepareUI.selectedTeamIndex, stageId = prepareUI.stageKey });
                GameCore.Instance.ChangeSubSystem(SubSysType.Battle, new BattlePara()
                {
                    playerTeam = PrepareUI.selectedTeamIndex,
                    stageId = prepareUI.storyKey,
                    friendName = UserName,
                    friendUnitList = friendCharList,
                    friendIcon = friendIcon,
                    friendTeamSkill = teamSkill,
                    unitStatInfosList = statInfoList,
                    enemyStatInfosList = enemyStatInfoList,
                    missionInfoList = missionInfoList
                });
                return true;
            case 3: GameCore.Instance.ShowNotice("실패", "열리지 않은 STORY ID", 0); break;
			case 4: GameCore.Instance.ShowNotice("실패", "존재하지 않는 팀", 0); break;
			case 5: GameCore.Instance.ShowNotice("실패", "전투력 부족", 0); break;
			case 6: GameCore.Instance.ShowAgree("전투 실패", "[c][8000FF]에너지[-][/c]가 부족합니다.\n 상점으로 이동하시겠습니까?", 0, () =>
            {
                GameCore.Instance.CloseMsgWindow();
                GameCore.Instance.ChangeSubSystem(SubSysType.Shop, null);
                //상점으로 이동하는 부분. 상점 미구현으로 인하여 추가코드 X
            }); break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
		}
		return false;
	}

	private bool ANS_STORY_REWARD(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				var chapterReward = para.GetField("UPDATE_CHAPTER_REWARD");
				GameCore.Instance.PlayerDataMgr.SetDataStoryChapter(chapterReward);
				//var reward = para.GetField("REWARD");
				//var cha_List = reward.GetField("CHA_LIST");
				//var item_List = reward.GetField("ITEM_LIST");
				//if (cha_List != null) GameCore.Instance.PlayerDataMgr.SetCardSData(cha_List);
				//if (item_List != null) GameCore.Instance.PlayerDataMgr.SetCardSData(item_List);

				//var printList = reward.GetField("REWARD");
				//List<int> rewardKeys = new List<int>();
				//List<int> rewardCounts = new List<int>();
				//for (int i = 0; i < printList.Count; ++i)
				//{
				//	int key = 0;
				//	int count = 0;
				//	printList[i].GetField(ref key, "REWARD_ITEM_ID");
				//	printList[i].GetField(ref count, "REWARD_ITEM_COUNT");

				//	rewardKeys.Add(key);
				//	rewardCounts.Add(count);
				//}
				//GameCore.Instance.ShowReceiveItem(rewardKeys.ToArray(), rewardCounts.ToArray());
				var list = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
				GameCore.Instance.ShowReceiveItem(list);
				storyUI.UpdateReward();
				return true;

			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			case 2: GameCore.Instance.ShowNotice("실패", "존재하지 않는 챕터", 0); break;
			case 3: GameCore.Instance.ShowNotice("실패", "이미 수령한 보상", 0); break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
		}

		return false;
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

                bool runningTuto = GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning;
                bool tutoStrike = runningTuto && GameCore.Instance.lobbyTutorial.tutorialPos == 5;

                if (tutoStrike)
                {
                    var dummy = new JSONObject();
                    dummy.AddField("USER_UID", -99);
                    dummy.AddField("USER_NAME", "가짜친구");
                    dummy.AddField("DELEGATE_ICON", 1000001);
                    dummy.AddField("USER_LEVEL", 1);
                    dummy.AddField("COMM", "");
                    dummy.AddField("LOGIN_DATE", "2019-01-11 14:33:02");
                    dummy.AddField("SKILL", 4300001);
                    dummy.AddField("DELEGATE_TEAM_POWER", 17602);
                    recommandList.Add(dummy);
                }

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

                if (runningTuto && GameCore.Instance.lobbyTutorial.tutorialPos < 5)
                    prepareUI.SetFriend(null, null);
                else
                    prepareUI.SetFriend(friends, recommands);
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        switch (tutorialNum)
        {
            case 1:
                nActionList.Add(() => { });
                break;
            case 4:
                nActionList.Add(() => { });
                break;
            case 5:
                nActionList.Add(() => { });
                break;
            default:
                break;
        }
        return nActionList;
    }
}

