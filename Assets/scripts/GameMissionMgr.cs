using System;
using System.Collections.Generic;

public enum MissionType
{
    Daily,
    Weekly,
    Achieve,
    Quest,

    Count // NEVER USE
}

public enum MissionState
{
    Takable,
    Running,
    Lock,
    Complete,
}

public class MissionBundle
{
    public MissionType type;
    public MissionTopSData topData;
    public List<MissionSData> datas;
}


internal class GameMissionMgr : IEventHandler
{
    protected Dictionary<GameEventType, Func<ParaBase, bool>> handlerMap;  // 핸들러 테이블<이벤트 서브 타입, 해당 서브 타입 이벤트 처리 루틴>

    /*MissionSData[] missionSDatas;*/
    Dictionary<int, MissionSData>[] missions; // 업적은 ID가 아닌 defineKey를 KEY로 사용한다.
    Dictionary<int, MissionTopSData>[] missionTops;
    List<MissionBundle>[] bundleData;   // missions, missionTops데이터를 번들로 갖는다.

    Dictionary<int, int> defineCountMap; // 현재 어떤 디파인미션이 몇개인지를 저장한다.

    public MissionUI missionUI;

    public bool ShowFlag;

    // 반드시 테이블 데이터가 초기화되어 있어야 한다.
    public GameMissionMgr()
    {
        defineCountMap = new Dictionary<int, int>();
        missions = new Dictionary<int, MissionSData>[4];
        missionTops = new Dictionary<int, MissionTopSData>[4];
        for (int i = 0; i < 4; ++i)
        {
            missions[i] = new Dictionary<int, MissionSData>();
            missionTops[i] = new Dictionary<int, MissionTopSData>();
        }

        //InitBundleData();


        handlerMap = new Dictionary<GameEventType, Func<ParaBase, bool>>();

        InitEventList();

        var it = handlerMap.GetEnumerator();
        while (it.MoveNext())
            GameCore.Instance.EventMgr.RegisterHandler(this, it.Current.Key);
    }


    public bool HandleMessage(GameEvent _evt)
    {
        if (handlerMap.ContainsKey(_evt.EvtType))
            return handlerMap[_evt.EvtType](_evt.Para);
        return false;
    }


    void InitEventList()
    {
        handlerMap.Add(GameEventType.ANS_MISSION_LIST, ANS_MISSION_LIST);
        handlerMap.Add(GameEventType.ANS_CHARACTER_POWER, ANS_CHARACTER_POWER);
        handlerMap.Add(GameEventType.ANS_MISSION_REWARD, ANS_MISSION_REWARD);
        handlerMap.Add(GameEventType.ANS_MISSION_REWARD_TOP, ANS_MISSION_REWARD_TOP);
    }


    public void InitBundleData()
    {
        bundleData = new List<MissionBundle>[4];
        for (int i = 0; i < 4; ++i)
            bundleData[i] = new List<MissionBundle>();

        // daily
        var dailyTopIter = ((DataMapCtrl<MissionAccumRewardDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MissionAccumReward)).GetEnumerator();
        var dailyIter = ((DataMapCtrl<MissionDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MissionDaily)).GetEnumerator();
        var daily = new MissionBundle();
        daily.type = MissionType.Daily;

        while (dailyTopIter.MoveNext())
        {
            if (dailyTopIter.Current.Value.type == MissionType.Daily)
            {
                daily.topData = new MissionTopSData();
                daily.topData.UID = dailyTopIter.Current.Value.id;
                break;
            }
        }

        daily.datas = new List<MissionSData>();
        while (dailyIter.MoveNext())
        {
            var data = new MissionSData();
            data.UID = dailyIter.Current.Value.id;
            daily.datas.Add(data);
        }
        bundleData[(int)MissionType.Daily].Add(daily);


        // weekly
        var weeklyTopIter = ((DataMapCtrl<MissionAccumRewardDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MissionAccumReward)).GetEnumerator();
        var weeklyIter = ((DataMapCtrl<MissionDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MissionWeekly)).GetEnumerator();
        var weekly = new MissionBundle();
        weekly.type = MissionType.Weekly;

        while (weeklyTopIter.MoveNext())
        {
            if (weeklyTopIter.Current.Value.type == MissionType.Weekly)
            {
                weekly.topData = new MissionTopSData();
                weekly.topData.UID = weeklyTopIter.Current.Value.id;
                break;
            }
        }

        weekly.datas = new List<MissionSData>();
        while (weeklyIter.MoveNext())
        {
            var data = new MissionSData();
            data.UID = weeklyIter.Current.Value.id;
            weekly.datas.Add(data);
        }
        bundleData[(int)MissionType.Weekly].Add(weekly);


        // achieve
        var achieveTopIter = ((DataMapCtrl<MissionAccumRewardDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MissionAccumReward)).GetEnumerator();
        var achieveIter = ((DataMapCtrl<AchieveDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MissionAchieve)).GetEnumerator();
        var achieve = new MissionBundle();
        achieve.type = MissionType.Achieve;

        while (achieveTopIter.MoveNext())
        {
            if (achieveTopIter.Current.Value.type == MissionType.Achieve)
            {
                achieve.topData = new MissionTopSData();
                achieve.topData.UID = achieveTopIter.Current.Value.id;
            }
        }

        achieve.datas = new List<MissionSData>();
        while (achieveIter.MoveNext())
        {
            if (achieveIter.Current.Value.level != 1)
                continue;

            var data = new MissionSData();
            data.UID = achieveIter.Current.Value.id;
            achieve.datas.Add(data);
        }
        bundleData[(int)MissionType.Achieve].Add(achieve);


        // Quest
        var questTopIter = ((DataMapCtrl<MissionAccumRewardDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MissionAccumReward)).GetEnumerator();
        var questIter = ((DataMapCtrl<MissionDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MissionQuest)).GetEnumerator();
        while (questTopIter.MoveNext())
        {
            if (questTopIter.Current.Value.type != MissionType.Quest)
                continue;

            var topID = questTopIter.Current.Value.id;
            var quest = new MissionBundle();
            quest.type = MissionType.Quest;

            // top datas
            quest.topData = new MissionTopSData();
            quest.topData.UID = topID;

            // datas
            quest.datas = new List<MissionSData>(10);
            for (int i = 0; i < 10; ++i)
            {
                questIter.MoveNext();
                var data = new MissionSData();
                data.UID = questIter.Current.Value.id;
                quest.datas.Add(data);
            }

            bundleData[(int)MissionType.Quest].Add(quest);
        }
    }

    internal void Mission_Start()
    {
        missionUI.Show(true);
    }
    bool ANS_MISSION_LIST(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                missions[(int)MissionType.Quest].Clear();
                defineCountMap.Clear();

                SetMissionData(MissionType.Daily, para.GetField("DAILY"));
                SetMissionData(MissionType.Weekly, para.GetField("WEEKLY"));
                SetMissionData(MissionType.Achieve, para.GetField("ACHIEVE"));
                SetMissionData(MissionType.Quest, para.GetField("QUEST"));

                GenerateMissionData();
                if (missionUI == null)
                    missionUI = MissionUI.Create(GameCore.Instance.ui_root);

                missionUI.Init();
                //GameCore.Instance.SetDynamicTutorial(GetDynamicReturnTutorialDataList);
                
                if (ShowFlag)
                {
                    ShowFlag = false;
                    missionUI.Show(true);
                }
                else
                    missionUI.Show(false);

                break;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); return false;
        }
        return true;
    }

    internal List<ReturnTutorialData> GetDynamicReturnTutorialDataList(int nTutorialPos)
    {
        List<ReturnTutorialData> returnTutorialList = new List<ReturnTutorialData>();
        switch (nTutorialPos)
        {
            case 13:
                break;
        }
        return returnTutorialList;
    }

    bool ANS_CHARACTER_POWER(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0: break;
            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); return false;
        }
        return true;
    }

    bool ANS_MISSION_REWARD(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var list = para.GetField("LIST");
                for (int i = 0; i < list.Count; ++i)
                {
                    var sdata = SetMissionData(list[i]);
                    if (sdata.type == MissionType.Quest)
                        SetQuestData(sdata);
                    missionUI.SetMissionData(sdata);
                }

                UpdateTopSData(para.GetField("TOP"));
                missionUI.TackNotification();
                var rewards = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                GameCore.Instance.ShowNotice("보상", "보상이 우편으로 발송되었습니다.", 0);
                    
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Item_Get);
                break;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "존재하지 않거나 수령 완료 상태", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); return false;
        }
        return true;
    }
    
    bool ANS_MISSION_REWARD_TOP(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                UpdateTopSData(para.GetField("TOP"));
                missionUI.TackNotification();
                var reward = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));

                GameCore.Instance.ShowNotice("보상", "보상이 우편으로 발송되었습니다.", 0);
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Item_Get);
                break;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "존재하지 않거나 수령 완료 상태", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); return false;
        }
        return true;
    }

    void UpdateTopSData(JSONObject _topJson)
    {
        bool first = false;
        for (int i = 0; i < _topJson.Count; ++i)
        {
            var TopSData = new MissionTopSData();
            TopSData.SetData(_topJson[i]);

            var accum = GameCore.Instance.DataMgr.GetMissionAccumRewardData(TopSData.UID);
            if (!missionTops[(int)accum.type].ContainsKey(TopSData.UID))
                missionTops[(int)accum.type].Add(TopSData.UID, TopSData);
            else
                missionTops[(int)accum.type][TopSData.UID].SetData(_topJson[i]);

            if (accum.type == MissionType.Quest)
            {
                MissionBundle tgBundle = bundleData[(int)accum.type][accum.level - 1];
                tgBundle.topData.SetData(_topJson[i]);

                if(missionUI.QuestIndex == accum.level - 1)
                {
                    missionUI.UpdateCount();
                    missionUI.SetReward(tgBundle.topData.UID, missionUI.TabIndex);
                }
            }
            else
            {
                if (TopSData.REWARD || !first)
                {
                    if (!TopSData.REWARD)
                        first = true;

                    MissionBundle tgBundle = bundleData[(int)accum.type][0];
                    tgBundle.topData.SetData(_topJson[i]);

                    missionUI.UpdateCount();
                    missionUI.SetReward(tgBundle.topData.UID, missionUI.TabIndex);
                }
            }
        }
        
    }

    public bool MissionUIActive()
    {
        if (missionUI != null)
            return missionUI.gameObject.activeSelf;
        else return false;
    }
    public void Close()
    {
        missionUI.Show(false);
        GameCore.Instance.NetMgr.Req_Notify_MyRoom_Count();
    }

    void SetMissionData(MissionType _type, JSONObject _json)
    {
        // Todo
        var top = _json.GetField("TOP");
        var list = _json.GetField("LIST");
        int idx = (int)_type;

        for (int i = 0; i < top.Count; ++i)
        {
            var data = new MissionTopSData();
            data.SetData(top[i]);

            int id = data.UID;

            if (missionTops[idx].ContainsKey(id))
                missionTops[idx][id] = data;
            else
                missionTops[idx].Add(id, data);
        }

        for (int i = 0; i < list.Count; ++i)
            SetMissionData(list[i]);
    }

    void SetQuestData(MissionSData _sdata)
    {
        var quest = bundleData[(int)MissionType.Quest];

        for(int i = 0; i < quest.Count;++i)
        {
            for (int j = 0; j < quest[i].datas.Count; ++j)
            {
                if (quest[i].datas[j].UID == _sdata.UID)
                {
                    quest[i].datas[j] = _sdata;
                    missionUI.SetMissionData(_sdata);
                    return;
                }
            }
        }
    }

    MissionSData SetMissionData(JSONObject _json)
    {
        var sdata = new MissionSData();
        sdata.SetData(_json);
        int idx = (int)sdata.type;
        int defineKey = GameCore.Instance.DataMgr.GetMissionData(sdata.type, sdata.UID).defineKey;
        int id = (sdata.type == MissionType.Achieve) ? defineKey :   // 업적일 때
                                                      sdata.UID;     // 업적 이외일 때

        // 퀘스트는 무조건 한개만 존재하기 때문에 이전 데이터 삭제
        if (sdata.type == MissionType.Quest)
            ClearMissionQuestData();

        if (missions[idx].ContainsKey(id))
            missions[idx][id].SetData(_json);
        else
        {
            missions[idx].Add(id, sdata);
            AddDefineCount(defineKey);
        }

        return sdata;
    }


    void ClearMissionQuestData()
    {
        var iter = missions[(int)MissionType.Quest].GetEnumerator();
        while (iter.MoveNext())
            SubDefineCount(GameCore.Instance.DataMgr.GetMissionQuestData(iter.Current.Key).defineKey);

        missions[(int)MissionType.Quest].Clear();
    }

    void AddDefineCount(int _defineKey)
    {
        if (defineCountMap.ContainsKey(_defineKey))
            defineCountMap[_defineKey]++;
        else
            defineCountMap.Add(_defineKey, 1);
    }

    void SubDefineCount(int _defineKey)
    {
        if (defineCountMap.ContainsKey(_defineKey))
            defineCountMap[_defineKey]--;
    }


    void GenerateMissionData()
    {
        bool bQuestComplete = false; 

        for(int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < bundleData[i].Count; ++j)
            {
                // Top Data
                var bundle = bundleData[i][j];
                if(i == (int)MissionType.Quest)
                {
                    if (missionTops[i].ContainsKey(bundle.topData.UID))
                        bundle.topData = missionTops[i][bundle.topData.UID];
                }
                else
                {
                    var iter = missionTops[i].GetEnumerator();
                    while (iter.MoveNext())
                    {
                        bundle.topData = iter.Current.Value;
                        if (bundle.topData.REWARD == false)
                            break;
                    }
                }

                // List Data
                if (i == (int)MissionType.Quest)
                {
                    for (int k = 0; k < bundle.datas.Count; ++k)
                    {
                        int id = bundle.datas[k].UID;

                        if (missions[i].ContainsKey(id))
                        {
                            bundle.datas[k] = missions[i][id];
                            bQuestComplete = true;
                            break;
                        }
                        else if(!bQuestComplete)
                        {
                            bundle.datas[k].state = MissionState.Complete;
                        }
                    }
                }
                else
                { 
                    for (int k = 0; k < bundle.datas.Count; ++k)
                    {
                        int id = bundle.datas[k].UID;        // 업적 이외일 때
                        if (i == (int)MissionType.Achieve)  // 업적일 때
                            id = GameCore.Instance.DataMgr.GetMissionAchieveData(id).defineKey;

                        if (missions[i].ContainsKey(id))
                            bundle.datas[k] = missions[i][id];
                    }
                }
            }
        }

        CheckMissionAll();
    }

    public MissionBundle GetBundle(MissionType _type, int _idx = 0)
    {
        int idx = (int)_type;
        if( (idx < 0 || bundleData.Length <= idx) ||
            (_idx < 0 || bundleData[idx].Count <= _idx) )
        {
            UnityEngine.Debug.LogError("Index out of bound");
            return null;
        }

        return bundleData[(int)_type][_idx];
    }

    public int GetNowQuestPage()
    {
        var quest = bundleData[(int)MissionType.Quest];
        for(int i = 0; i < quest.Count; ++i)
        {
            if (quest[i].datas[quest[i].datas.Count - 1].state == MissionState.Complete)
                continue;

            return i;
        }

        return 0;
    }

    public int GetQuestPageCount()
    {
        return bundleData[(int)MissionType.Quest].Count;
    }

    public int GetAchieveValue(int _defineKey)
    {
        var achieve = missions[(int)MissionType.Achieve];
        if (achieve.ContainsKey(_defineKey))
            return achieve[_defineKey].VALUE;

        return -1;
    }

    // 기존 값보다 더 높은 값일 때 해당 값으로 대체한다.
    public bool UpdateMissionValue(int _defineKey, int _value)
    {
        bool bChange = false;
        for (int i = 0; i < missions.Length; ++i)
        {
            var mission = missions[i];
            if (i == (int)MissionType.Achieve)
            {
                if (mission.ContainsKey(_defineKey) && 
                    mission[_defineKey].VALUE < _value)
                {
                    mission[_defineKey].VALUE = _value;
                    bChange = true;
                }
            }
            else
            {
                var iter = mission.GetEnumerator();
                while (iter.MoveNext())
                {
                    var data = GameCore.Instance.DataMgr.GetMissionData((MissionType)i, iter.Current.Value.UID);
                    if (data.defineKey == _defineKey && 
                        iter.Current.Value.VALUE < _value)
                    {
                        mission[iter.Current.Value.UID].VALUE = _value;
                        bChange = true;
                    }
                }
            }
        }

        return bChange;
    }

    // 현재 미션을 확인한다.
    public void CheckMissionAll()
    {
        var iter = defineCountMap.GetEnumerator();
        while(iter.MoveNext())
            CheckMission(iter.Current.Key);
    }

    public void CheckMission(int _defineKey)
    {
        if (!defineCountMap.ContainsKey(_defineKey))
            return;

        if (defineCountMap[_defineKey] <= 0)
            return;

        switch (_defineKey)
        {
            case CommonType.DEF_KEY_MISSION_DEFINE + 40: // 전투력 총합 
                var power = GetMaxTeamPower();
                if (UpdateMissionValue(CommonType.DEF_KEY_MISSION_DEFINE + 40, power))
                    GameCore.Instance.NetMgr.Req_Character_Power(power);
                break;

            default:
                break;
        }

    }

    int GetMaxTeamPower()
    {
        int maxPower = 0;
        for (int i = 0; i < 5; i++)
        {
            var power = GameCore.Instance.PlayerDataMgr.GetTeamPower(0);
            if (maxPower < power)
                maxPower = power;
        }

        return maxPower;
    }

    internal bool TakeNotificationCheck()
    {
        var pageCount = GameCore.Instance.MissionMgr.GetQuestPageCount();
        for (int p = 0; p < pageCount; p++)
        {
            var qBundle = GameCore.Instance.MissionMgr.GetBundle(MissionType.Quest, p);
            if (qBundle.topData.state == MissionState.Takable) { return true; }

            for (int i = 0; i < qBundle.datas.Count; ++i)
                if (qBundle.datas[i].state == MissionState.Takable) { return true; }
        }

        var bundle = GameCore.Instance.MissionMgr.GetBundle(MissionType.Daily);
        if (bundle.topData.state == MissionState.Takable) { return true; }

        for (int i = 0; i < bundle.datas.Count; ++i)
            if (bundle.datas[i].state == MissionState.Takable) { return true; }

        for (int b = 1; b < 3; ++b)
        {
            bundle = GameCore.Instance.MissionMgr.GetBundle((MissionType)b);
            if (bundle.topData.state == MissionState.Takable) { return true; }

            for (int i = 0; i < bundle.datas.Count; ++i)
                if (bundle.datas[i].state == MissionState.Takable) { return true; }
        }

        return false;
    }

}
