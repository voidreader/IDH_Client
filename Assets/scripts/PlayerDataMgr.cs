using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using IDH.MyRoom;

/// <summary>
/// 재화 및 재료 아이템의 테이블 키값과 반드시 일치해야한다.
/// </summary>
enum ResourceType
{
	Gold = 3000001,
	Cash = 3000002,
	Coin1 = 3000003,
	Coin2 = 3000004,
	Coin3 = 3000005,
	Coin4 = 3000006,
	Coin5 = 3000007,
	Ticket1 = 3000008,	// 빠른 영웅 제조권
	Ticket2 = 3000009,	// 빠른 장비 제조권
	Friendship = 3000010,
	PVPTicket = 3000011,
	Vigor = 3000012,
	Mailage = 3000013,
	Honor = 3000014,
    Ticket_daily = 3000015,
    Ticket_Raid = 3000016,
}
public class Tutorial : JsonParse
{
    internal int main;      //Must Tutorial의 진행위치값 (0~8)
                            //나머지 변수들은 0이면 아직 진행하지 않은것, 1은 진행한것
    internal int dungeon;   //9
    internal int raid;      //10
    internal int pvp;
    internal int myRoom;
    internal int manufact;
    internal int farming;
    internal int mission;
    internal int mail;

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "main", out main);

#if UNITY_EDITOR
        // 고정 튜토리얼 테스트용
        if (GameCore.Instance.fixedTutorialStartIndex > 0)
            main = GameCore.Instance.fixedTutorialStartIndex - 1;
#endif
        // NOTE : 메인 튜토리얼을 걷어내는 용도
        // 2020-03-02 이현철 [ 튜토리얼 삭제작업 ]
        main = 99;


        ToParse(_json, "dungeon", out dungeon);
        ToParse(_json, "raid", out raid);
        ToParse(_json, "pvp", out pvp);
        ToParse(_json, "myroom", out myRoom);
        ToParse(_json, "manufact", out manufact);
        ToParse(_json, "farming", out farming);
        ToParse(_json, "mission", out mission);
        ToParse(_json, "mail", out mail);
        return true;
    }


    public Tutorial Clone()
    {
        var clone = new Tutorial();
        clone.main = main;
        clone.dungeon = dungeon;
        clone.raid = raid;
        clone.pvp = pvp;
        clone.myRoom = myRoom;
        clone.manufact = manufact;
        clone.farming = farming;
        clone.mission = mission;
        clone.mail = mail;

        return clone;
    }
}
/// <summary>
/// 플레이어 데이터(팀구성, 소유 아이템, 플레이어 레벨 등) 관리
/// </summary>
internal class PlayerDataMgr : JsonParse, IEventHandler
{

    public LocalUserData LocalUserData { get; private set; }

    public NCommon.LoginType LoginType { get; set; }

    #region UserData_Content

    public TeamSData UserTeam { get; private set; }                                              // 팀정보

    public Dictionary<long, HeroSData> HeroSdataDic = new Dictionary<long, HeroSData>();        // 소지중인 모든 유닛
    public Dictionary<long, ItemSData> ItemSdataDic = new Dictionary<long, ItemSData>();      // 소지중인 모든 아이템(장비, 가구, 재료, 재화)

    private Dictionary<int, StorySData> storySDataDic = new Dictionary<int, StorySData>();     // 유저 진행 스토리 데이터.  // storySData[tableKey] = SData
    private int storyPrevSData = 0;
    private Dictionary<int, StoryChapterSData[]> storyRewardSDataDic = new Dictionary<int, StoryChapterSData[]>(); // 유저 진행 스토리 보상데이터 // storyRewardSData[chapter*100 + difficulty] = SData

    private Dictionary<long, FriendSData> friendSDataDic = new Dictionary<long, FriendSData>();// 유저 친구 데이터
    private Dictionary<int, DateTime> freeGachaCooltimeDic;                                  // 뽑기 쿨타임

    private Dictionary<int, MakeSData> makeSDataDic;         // 제조 데이터
    private List<FarmingSData> farmingSDataDic = new List<FarmingSData>();                     // 파밍 데이터
    private PvPSData pvpData = new PvPSData();
    private static List<HotTimeSData> hotTimeSDatas = new List<HotTimeSData>();       // 핫타임 데이터


    private BattleCommonDataMap battleCommonDatamap = new BattleCommonDataMap();
    private Tutorial tutorialDataMap = new Tutorial();

    public List<long> newHeroCardUidList = new List<long>();
    public List<long> newItemCardUidList = new List<long>();

    public PushSettingSData pushSetting;

    public Dictionary<int, AttendanceSData> attendanceDic = null; // 초기화 되기 전에는 null값을 갖는다.
    #endregion

    public bool InitedAttendanceData { get { return attendanceDic != null; } }
    public bool InitedMakeData { get { return makeSDataDic != null; } }

    bool[] mInitData = new bool[(int)InitDataType.Count];
    public bool LoadedDataToServer = false;//{ get; private set; } // 서버에 데이터가 로드되었다면 true.

    #region SystemData

    // 친구관련 리스트인듯?
    Action<FriendSData[]> cbIsLoaded;
    // Local Space
    private List<CardSData> expRewardItems = new List<CardSData>();                     // 스토리모드 진행중 누적된 보상데이터 리스트
    private List<RaidSData> raidSData = new List<RaidSData>();                          // 레이드모드 진행했던 기록 캐시 ( 0 - normal ,1 - hard, 2 - very hard )
    // pvp 재탐색 
    private int nResearchCount;


    #endregion

    public int GetStorySDataDic { get { return storySDataDic.Last().Key; } }
    //public int GetPrevStorySData { get { return storyPrevSData; } }
    public int unlockKey { get; private set; } // 언락 이펙트를 위한 스토리키 캐싱


    public List<MyRoomBuffSData> myRoomBuffs = new List<MyRoomBuffSData>(); // 마이룸 버프 데이터. 필요할때마다 호출하여 캐싱한다.



    public PlayerDataMgr()
    {
		GameCore.Instance.EventMgr.RegisterHandler(this, 
            GameEventType.ANS_LEVEL_UP, 
            GameEventType.ANS_UPDATE_TEAM,
            GameEventType.ANS_TUTORIAL);
	}

    #region UserData관련 GetProperty

    public string Name { get { return LocalUserData.Name; } }
    public int Level { get { return LocalUserData.Level; } }
    public int Exp { get { return LocalUserData.NowExp; } }
    public int MaxExp { get { return LocalUserData.NowMaxExp; } }
    public int HeroSlotLimitCount { get { return LocalUserData.HeroSlotLimitCount; } }
    public int EquipItemSlotLimitCount { get { return LocalUserData.EquipItemSlotLimitCount; } }
    public long MainCharacterUID { get { return LocalUserData.MainCharacterUID; } }
    public string Comment { get { return LocalUserData.Comment; } }
    public int MaxVigor { get { return LocalUserData.NowMaxVigor; } }
    public MyRoomInfo UserMyRoom { get; private set; }


    public bool IsInitInven_Item { get { return ItemSdataDic != null; } }
    public bool IsInitInven_Hero { get { return HeroSdataDic != null; } }

    #endregion

    #region UserInventoryData관련 GetProperty

    public int HeroCount { get { return HeroSdataDic.Count; } }

    #endregion

    //추가 변경 요함 아래 4줄
    public int PvPRank { get; set; }
    public int PvPGroupRank { get; set; }
    public int ResearchCount { get { return nResearchCount; } set { nResearchCount = value; } }
    public PvPSData PvPData { get { return pvpData; } set { pvpData = value; } }
    public BattleCommonDataMap BattleCommonData { get { return battleCommonDatamap; } }
    public Tutorial TutorialData { get { return tutorialDataMap; } }

    internal FriendSData[] GetFriendList() { return friendSDataDic.Values.ToArray(); }
    internal RaidSData GetRaidSData(int _key) { return raidSData.Find(data => data.key == _key); }
    internal long GetTeamIds(int _teamIdx, int _idx) { return UserTeam.uids[_teamIdx, _idx]; }
    internal int GetTeamSkillKey(int _teamIdx) { return UserTeam.skills[_teamIdx]; }
    internal long[] GetUnitIds() { return HeroSdataDic.Keys.ToArray(); }
    internal int GetFarmingCount() { return farmingSDataDic.Count; }
    internal bool IsArrange(long _uid) { return UserTeam.IsHaveUnit(_uid); }
    internal List<CardSData> GetExpRewards() { return expRewardItems; ; }
    internal void ClearExpReward() { expRewardItems.Clear(); }
    internal void ClearRaidSData() { raidSData.Clear(); }
    internal void SetMainCharacterUID(long uid) { LocalUserData.MainCharacterUID = uid; PvPData.typicalKey = GetUnitData(uid).id; }
    internal void SetComment(string str) { LocalUserData.Comment = str; }
    internal List<HeroSData> GetUserHeroSDataList() { return HeroSdataDic.Values.ToList(); }

    internal BattleCommonDataMap GetBattleCommonData()
    {
        return battleCommonDatamap;
    }

    internal FriendSData GetFriendOrNull(long userID)
    {
        FriendSData data = null;
        friendSDataDic.TryGetValue(userID, out data);
        return data;
    }

    internal HeroSData GetUserMainUnitSData()
    {
        if (HeroSdataDic.ContainsKey(MainCharacterUID) == false) return null;
        return HeroSdataDic[MainCharacterUID];
    }

    internal void SetDataAccount(JSONObject _json)
    {
        _json.GetField(ref LocalUserData.Level, "USER_LEVEL");
        _json.GetField(ref LocalUserData.TotalExp, "USER_EXP");
    }


    public void SetHotTimeSData(JSONObject _HOTTIME)
    {
        if (_HOTTIME == null)
            return;

        hotTimeSDatas.Clear();
        for (int i = 0; i < _HOTTIME.Count; ++i)
        {
            var sdata = new HotTimeSData();
            sdata.SetData(_HOTTIME[i]);

            hotTimeSDatas.Add(sdata);
        }
    }


    public void UpdateHotTimeData()
    {
        for(int i = hotTimeSDatas.Count-1; 0 <= i; --i)
            if (hotTimeSDatas[i].end <= GameCore.nowTime)
                hotTimeSDatas.RemoveAt(i);
    }

    public int GetHotTimeCount()
    {
        return hotTimeSDatas.Count;
    }

    public List<HotTimeSData> GetHotTimeDatas()
    {
        return hotTimeSDatas;
    }


    internal int GetTeamPower(int _idx)
    {
        int power = 0;
        for (int i = 0; i < 6; ++i)
        {
            var unit = GetUnitSDataByTeam(_idx, i);
            if (unit != null) power += unit.GetPower();
        }

        return power;
    }

    private void SetDataTeam(JSONObject _json)
    {
        if (_json == null) return;
        UserTeam = new TeamSData();
        UserTeam.SetData(_json);
    }

    public void ResetPlayerData()
    {
        Debug.LogError("ResetPlayer Data");
        LocalUserData = null;

        UserTeam = null;

        HeroSdataDic = null;
        ItemSdataDic = null;

        storySDataDic = null;

        storyPrevSData = 0;
        storyRewardSDataDic = null;

        friendSDataDic = null;
        freeGachaCooltimeDic = null;

        makeSDataDic = null;
        farmingSDataDic = null;
        pvpData = null;

        battleCommonDatamap = null;
        tutorialDataMap = null;

        newHeroCardUidList = null;
        newItemCardUidList = null;

        pushSetting = null;

        attendanceDic = null;
        mInitData = new bool[(int)InitDataType.Count];
        LoadedDataToServer = false;
    }

    public bool IsInited(InitDataType _type)
    {
        return mInitData[(int)_type];
    }

    public void InitDataOf(InitDataType _type, JSONObject _json)
    {
        if (mInitData[(int)_type])
            return; // 이미 초기화 됨

        mInitData[(int)_type] = true;

        switch (_type)
        {
            case InitDataType.Character:     SetDataUnit(_json); break;
            case InitDataType.Farming:       SetFarmingData(_json); break;
            case InitDataType.MyRoom:        SetMyRoomData(_json); break;
            case InitDataType.Item:          SetDataItem(_json); break;
            case InitDataType.Gacha:         SetDataGacha(_json); break;
            case InitDataType.Team:          SetDataTeam(_json); break;
            case InitDataType.Story:         SetDataStory(_json); break;
            case InitDataType.ChapterReward: SetDataStoryChapter(_json); break;
            case InitDataType.PvP:           pvpData.SetData(_json[0]); break;
        }
    }

    public void SetMyRoomBuffData(JSONObject _json)
    {
        myRoomBuffs.Clear();
        if (_json == null) return;
        if (_json.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _json.Count; ++i)
                SetMyRoomBuffDataInternal(_json[i]);
        }
        else
        {
            SetMyRoomBuffDataInternal(_json);
        }
    }


    void SetMyRoomBuffDataInternal(JSONObject _json)
    {
        var data = new MyRoomBuffSData();
        data.SetData(_json);
        myRoomBuffs.Add(data);
    }


    /// <summary>
	/// 최초 Lobby 진입시(Loiding 종료시) 호출되는 모든 플레이어 데이터 세팅
	/// </summary>
	/// <param name="jsonData"></param>
	/// <returns></returns>
	internal override bool SetData(JSONObject jsonData)
    {
        //if (Name != null) return false;
        if (jsonData == null)
        {
            Debug.LogError("Server Data Invalid");
            return false;
        }


//#if !UNITY_EDITOR
        string dkey = "";
        string div = "";
        jsonData.GetField(ref dkey, "dkey");
        jsonData.GetField(ref div, "div");
        GameCore.Instance.ResourceMgr.SetDecryptKey(dkey, div);

//#endif
//        GameCore.Instance.InitDataMgr();


        LocalUserData.CalculateNowExp nowExpDelegate = (int userLevel, int userTotalExp) =>
        {
            return userTotalExp - GameCore.Instance.DataMgr.GetExpData(userLevel).accExp;
        };

        LocalUserData.CalculateTableValueByLevel nowMaxExpDelegate = (int userLevel) =>
        {
            if (GameCore.Instance.DataMgr.GetMaxLevel() <= userLevel)
                return 0;

            var nowLevelData = GameCore.Instance.DataMgr.GetExpData(userLevel);
            var nextLevelData = GameCore.Instance.DataMgr.GetExpData(userLevel + 1);
            return nextLevelData.accExp - nowLevelData.accExp;
        };

        LocalUserData.CalculateTableValueByLevel nowMaxVigorDelegate = (int userLevel) =>
        {
            var stamina = GameCore.Instance.DataMgr.GetStaminaConstData();
            return stamina.defMaxVigor + stamina.addMaxVigor * (userLevel - 1);
        };


        LocalUserData = new LocalUserData(nowExpDelegate, nowMaxExpDelegate, nowMaxVigorDelegate);

        jsonData.GetField(ref LocalUserData.Name, "name");
        jsonData.GetField(ref LocalUserData.Level, "level");
        jsonData.GetField(ref LocalUserData.TotalExp, "exp");
        jsonData.GetField(ref LocalUserData.MainCharacterUID, "delegate_icon");
        jsonData.GetField(ref LocalUserData.Comment, "comment");
        jsonData.GetField(ref LocalUserData.HeroSlotLimitCount, "character_slot");
        jsonData.GetField(ref LocalUserData.EquipItemSlotLimitCount, "inven_slot");
        jsonData.GetField(ref LocalUserData.RatingOfPvP, "placement");
        jsonData.GetField(ref LocalUserData.RemainingDefeatCountOfPvP, "rechallenge_count");
        jsonData.GetField(ref LocalUserData.DeleteFriendRemainingCount, "del_frined_limit");
        jsonData.GetField(ref LocalUserData.DailyDungeonRechargeAbleCount, "add_dungeon_limit");


        LocalUserData.Comment = JsonTextParse.FromJsonText(LocalUserData.Comment);

        // 영웅 데이터 파싱
        //SetDataUnit(jsonData.GetField("character"));
        // 팀 데이터 파싱
        //SetDataTeam(jsonData.GetField("team"));
        // item 데이터 파싱
        //SetDataItem(jsonData.GetField("items"));
        // 카챠 데이터 파싱
        //SetDataGacha(jsonData.GetField("gacha"));
        // 스토리 데이터 파싱
        //storySDataDic.Clear();
        //SetDataStory(jsonData.GetField("story"));
        //SetPrevDataStory(); //가장 최근까지 깬 스테이지 정보값 출력
        // 스토리 보상 데이터 파싱
        //storyRewardSDataDic.Clear();
        //SetDataStoryChapter(jsonData.GetField("chapter_reward"));
        // 파밍 데이터 파싱
        //SetFarmingData(jsonData.GetField("FARMING"));
        // 제조 데이터 파싱
        //SetMakeData(jsonData.GetField("MAKING"));

        // myroom
        //SetMyRoomData(jsonData.GetField("myroom")); // 필요할 때 호출하도록 수정

        // 친구 데이터
        //friendSDataDic.Clear();
        //SetFriendSData(jsonData.GetField("friend"));

        // PVP 데이터 파싱
        //if (jsonData.GetField("pvp")[0] != null) pvpData.SetData(jsonData.GetField("pvp")[0]);

        //battlecommonData 파싱
        battleCommonDatamap.SetData(jsonData.GetField("battle_common")[0]);

        //Tutorial의 진행정도를 위한 파싱
        tutorialDataMap.SetData(jsonData.GetField("tutorial"));

        // 서버와의 시간차 저장
        string str = "";
        jsonData.GetField(ref str, "current_time");
        DateTime time = DateTime.ParseExact(str, "yyyy-MM-dd HH:mm:ss", null);
        GameCore.timeGap = time - DateTime.Now;
        GameCore.prevTime = time;

        //GameCore.Instance.ResourceMgr.SetDecryptKey("21FBE3FADEEE96A5CE4E366F6AE05DB7", "9E3BCEF4E0BC8F94");

        pushSetting = new PushSettingSData();
        pushSetting.SetData(jsonData.GetField("push"));

        return true;
    }

    internal HeroSData GetUnitSDataByTeam(int _team, int _idx)
    {
        if (UserTeam.uids[_team, _idx] <= 0) return null;
        return HeroSdataDic[UserTeam.uids[_team, _idx]];
    }

    internal UnitDataMap GetUnitDataByTeam(int _team, int _idx)
    {
        if (UserTeam.uids[_team, _idx] <= 0) return null;
        return GameCore.Instance.DataMgr.GetUnitData(HeroSdataDic[UserTeam.uids[_team, _idx]].key);
    }

    internal HeroSData GetUnitSData(long _id)
    {
        if (HeroSdataDic.ContainsKey(_id)) return HeroSdataDic[_id];
        return null;
    }

    internal UnitDataMap GetUnitData(long _id)
    {
        if (HeroSdataDic.ContainsKey(_id))
            return GameCore.Instance.DataMgr.GetUnitData(HeroSdataDic[_id].key);
        return null;
    }

    internal HeroSData[] GetUnitsByMyRoomIndex(int _num)
    {
        var list = from unit in HeroSdataDic
                   where unit.Value.dormitory == _num
                   select unit.Value;

        return list.ToArray();
    }

    /// <summary>
    /// 모든 유닛 데이터 설정
    /// </summary>
    /// <param name="_json"></param>
    /// <returns></returns>
    public bool SetDataUnit(JSONObject _json)
    {
        if (_json == null) return false;

        HeroSdataDic = new Dictionary<long, HeroSData>();

        for (int i = 0; i < _json.Count; ++i)
        {
            var data = _json[i];
            var unitSData = new HeroSData();
            unitSData.SetData(data);
            HeroSdataDic.Add(unitSData.uid, unitSData);
        }

        return true;
    }

    private void SetUnitSDataInternal(JSONObject _json)
    {
        if (_json == null) return;
        HeroSData data = new HeroSData();
        data.SetData(_json);

        if (HeroSdataDic.ContainsKey(data.uid)) HeroSdataDic[data.uid].SetData(_json);
        else
        {
            HeroSdataDic.Add(data.uid, data);
            newHeroCardUidList.Add(data.uid);
        }
    }


    public List<HeroSData> GetUnitsByFarmingKey(int _farmingKey)
    {
        List<HeroSData> list = new List<HeroSData>();
        foreach (var data in HeroSdataDic)
            if (data.Value.farming_Id == _farmingKey)
                list.Add(data.Value);

        return list;
    }

    /// <summary>
    /// Json데이터로 유닛 데이터 갱신 
    /// </summary>
    /// <param name="_json"></param>
    internal void SetUnitSData(JSONObject _json)
    {
        if (_json.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _json.Count; ++i)
                SetUnitSDataInternal(_json[i]);
        }
        else
        {
            SetUnitSDataInternal(_json);
        }
    }

    /// <summary>
    /// 재료 및 재화 아이템의 개수 반환
    /// </summary>
    /// <param name="_type"></param>
    /// <returns></returns>
    internal int GetReousrceCount(ResourceType _type)
    {
        var data = GetItemByKey((int)_type);
        if (data != null) return data.count;
        else return 0;
    }

    internal int GetEquipItemCount()
    {
        var list = from equipItem in ItemSdataDic
                   where equipItem.Value.type == CardType.Equipment
                   select equipItem;

        return list.Count();
    }

    internal ItemSData GetItemByKey(int _key)
    {
        foreach (var item in ItemSdataDic)
        {
            if (item.Value.key == _key) return item.Value;
        }

        return null;
    }

    internal ItemSData GetItemSData(long _id)
    {
        if (ItemSdataDic.ContainsKey(_id)) return ItemSdataDic[_id];
        return null;
    }

    internal ItemDataMap GetItemData(long _id)
    {
        if (ItemSdataDic.ContainsKey(_id)) return GameCore.Instance.DataMgr.GetItemData(ItemSdataDic[_id].key);
        return null;
    }

    internal long[] GetItemIds(CardType _type)
    {
        var list = from item in ItemSdataDic
                   where item.Value.type == _type
                   select item.Key;

        return list.ToArray();
    }

    private CardSData GetFakeRewardItem(JSONObject _json)
    {
        int type = -1;
        int key = 0;
        //_json.GetField(ref key, "REWARD_ITEM_ID");
        //if (CardDataMap.IsUnitKey(key))
        //    type = 99;

        _json.GetField(ref type, "ITEM_TYPE");

        switch (type)
        {
            case 99:  // Character
                HeroSData hero = new HeroSData();
                hero.uid = -1;// long.MaxValue;
                hero.SetRewardData(_json);
                return hero;

            case 0:   // equip Item
            case 5:   // interior Item
            default:

                ItemSData item = new ItemSData();
                item.uid = -1;// long.MaxValue;
                item.SetRewardData(_json);

                if (item.count < 0) return null;
                else return item;
        }
    }

    /// <summary>
	/// 모든 아이템 데이터 설정
	/// </summary>
	/// <param name="_json"></param>
	/// <returns></returns>
	private bool SetDataItem(JSONObject _json)
    {
        if (_json == null) return false;
        ItemSdataDic = new Dictionary<long, ItemSData>();

        for (int i = 0; i < _json.Count; ++i)
        {
            // 생성
            var json = _json[i];
            var sdata = (ItemSData)CardSData.CreateCardSData(json);
            if (sdata == null && GameCore.Instance.DataMgr.GetItemData(sdata.key) == null) continue;
            sdata.SetData(json);

            // 저장
            ItemSdataDic.Add(sdata.uid, sdata);

            // 장착정보 갱신
            if (sdata.type == CardType.Equipment)
            {
                var equipunitUID = sdata.equipHeroUID;
                if (0 <= equipunitUID && HeroSdataDic.ContainsKey(equipunitUID))
                {
                    var idx = GetItemData(sdata.uid).subType - (ItemSubType.EquipItem + 1);
                    HeroSdataDic[equipunitUID].equipItems[idx] = sdata.uid;
                }
                else
                    sdata.equipHeroUID = 0;
            }
        }

        return true;
    }

    /// <summary>
    /// Json데이터로 카드 데이터 갱신 (카드 데이터는 카드가 될 수 있는 데이터들. (unit, item)
    /// </summary>
    /// <param name="_json"></param>
    internal void SetCardSData(JSONObject _json)
    {
        if (_json == null) return;

        if (_json.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _json.Count; ++i)
                SetCardSDataInternal(_json[i]);
        }
        else
        {
            SetCardSDataInternal(_json);
        }

    }

    private void SetCardSDataInternal(JSONObject _json)
    {
        int value = -1;
        _json.GetField(ref value, "ITEM_UID");
        if (value == -1) SetUnitSDataInternal(_json);
        else SetItemSDataInternal(_json);
    }

    internal MakeSData GetMakeData(int _key)
    {
        if (makeSDataDic != null && makeSDataDic.ContainsKey(_key)) return makeSDataDic[_key];
        return null;
    }

    internal FarmingSData GetFarmingData(int _idx)
    {
        if (_idx < 0 || farmingSDataDic.Count <= _idx)
        {
            Debug.LogError("invalid Data");
            return null;
        }
        return farmingSDataDic[_idx];
    }

    internal FarmingSData GetFarmingDataByKey(int _key)
    {
        FarmingSData target = farmingSDataDic.Find(data => data.id == _key);
        return target;
    }

    internal StorySData GetStorySData(int _key)
    {
        if (storySDataDic.ContainsKey(_key)) return storySDataDic[_key];
        return null;
    }

    internal StoryChapterSData GetStorychapterSData(int _chapter, int _difficult, int _level)
    {
        var key = StoryChapterDataMap.GenerateChapterKey(_chapter, _difficult);
        if (storyRewardSDataDic.ContainsKey(key)) return storyRewardSDataDic[key][_level - 1];
        return null;
    }

    internal DateTime GetFreeGachaCool(int _type)
    {
        if (freeGachaCooltimeDic.ContainsKey(_type)) return freeGachaCooltimeDic[_type];
        return DateTime.MinValue;
    }

    private void SetItemSDataInternal(JSONObject _json)
    {
        if (_json == null) return;

        ItemSData data = new ItemSData();
        data.SetData(_json);

        if (ItemSdataDic.ContainsKey(data.uid)) ItemSdataDic[data.uid].SetData(_json);
        else
        {
            ItemSdataDic.Add(data.uid, data);
            if (data.type == CardType.Equipment || data.type == CardType.Interior)
                newItemCardUidList.Add(data.uid);
        }
    }

    /// <summary>
    /// 뽑기 데이터 설정
    /// </summary>
    /// <param name="_json"></param>
    public void SetDataGacha(JSONObject _json, bool _setNowTime = false)
    {
        if (_json == null) return;
        if (freeGachaCooltimeDic == null)
            freeGachaCooltimeDic = new Dictionary<int, DateTime>();

        if (_json.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _json.Count; ++i)
                SetDataGachaInternal(_json[i], _setNowTime);
        }
        else
            SetDataGachaInternal(_json, _setNowTime);
    }

    void SetDataGachaInternal(JSONObject _json, bool _setNowTime = false)
    {
        string str = null;
        int key = -1;
        DateTime time = DateTime.MinValue;

        _json.GetField(ref key, "GACHA_ID");
        _json.GetField(ref str, "PICK_TIME");

        if (str != null)        DateTime.TryParse(str, out time);
        else if (_setNowTime)   time = GameCore.nowTime;

        if (!freeGachaCooltimeDic.ContainsKey(key))
            freeGachaCooltimeDic.Add(key, time);
        else
            freeGachaCooltimeDic[key] = time;
    }

    internal void UpdateTeamData(JSONObject _json)
    {
        if (UserTeam == null)
            UserTeam = new TeamSData();
        UserTeam.SetData(_json);

#if UNITY_EDITOR
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < UserTeam.uids.GetLength(0); ++i)
        {
            sb.Append("[" + i + "]");
            for (int j = 0; j < UserTeam.uids.GetLength(1); ++j)
            {
                sb.Append(UserTeam.uids[i, j]);
                sb.Append(",");
            }
            sb.Append('\n');
        }
        Debug.Log(sb.ToString());
#endif
    }

    internal void SetEquip(long _unitUID, long _itemUID)
    {
        var unit = GetUnitSData(_unitUID);
        var item = GetItemSData(_itemUID);
        var idx = GetItemData(_itemUID).subType - (ItemSubType.EquipItem + 1);

        // 장착중이던 장비를 해제 한다.
        if (unit.equipItems[idx] > 0)
        {
            var oldItem = GetItemSData(unit.equipItems[idx]);
            oldItem.equipHeroUID = -1;
        }

        // 아이템을 장착중이던 유닛이 있다면 해제 시킨다.
        if (item.equipHeroUID > 0)
        {
            var otherUnit = GetUnitSData(item.equipHeroUID);
            otherUnit.equipItems[idx] = -1;
        }

        // 장착
        item.equipHeroUID = _unitUID;
        unit.equipItems[idx] = _itemUID;
    }

    internal void SetUnequip(long _unitUID, int _itemTypeIdx)
    {
        var unit = GetUnitSData(_unitUID);
         
        if (unit.equipItems[_itemTypeIdx] <= 0) return;

        var oldItem = GetItemSData(unit.equipItems[_itemTypeIdx]);
        oldItem.equipHeroUID = -1;
        unit.equipItems[_itemTypeIdx] = -1;
    }

    internal void SetFreeGachaCool(int _type, DateTime _time)
    {
        if (freeGachaCooltimeDic.ContainsKey(_type)) freeGachaCooltimeDic[_type] = _time;
        else freeGachaCooltimeDic.Add(_type, _time);
    }

    internal void SetMakeData(JSONObject _data)
    {
        if (makeSDataDic == null)
            makeSDataDic = new Dictionary<int, MakeSData>();

        if (_data == null) return;

        if (_data.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _data.Count; ++i)
                SetMakeSDataInternal(_data[i]);
        }
        else
        {
            SetMakeSDataInternal(_data);
        }
    }

    private void SetMakeSDataInternal(JSONObject _data)
    {
        MakeSData data = new MakeSData();
        data.SetData(_data);
        if (!makeSDataDic.ContainsKey(data.key)) makeSDataDic.Add(data.key, new MakeSData());
        makeSDataDic[data.key].SetData(_data);
    }

    internal void SetFarmingData(JSONObject _json)
    {
        if (_json == null || _json.type != JSONObject.Type.ARRAY) return;
        farmingSDataDic.Clear();

        for (int i = 0; i < _json.Count; ++i)
        {
            var data = new FarmingSData();
            data.SetData(_json[i]);
            farmingSDataDic.Add(data);
        }
    }

    internal void SetDataStory(JSONObject _json)
    {
        if (_json == null) return;
        if (_json.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _json.Count; ++i)
                SetStoryInternal(_json[i]);
        }
        else
        {
            SetStoryInternal(_json);
        }
    }


    private void SetStoryInternal(JSONObject _json)
    {
        StorySData data = new StorySData();
        data.SetData(_json);

        if (storySDataDic == null)
            storySDataDic = new Dictionary<int, StorySData>();

        if (storySDataDic.ContainsKey(data.key))
        {
            if (storySDataDic[data.key].clear == false && data.clear == true)
                SetUnLockStory(data.key);

            storySDataDic[data.key] = data;
        }
        else
        {
            storySDataDic.Add(data.key, data);
        }
    }

    
    /// <summary>
    /// 언락된 스테이지 중 언락 이펙트가 출력될 스테이지를 캐싱한다.
    /// </summary>
    /// <param name="_firstClearedStoryKey"></param>
    void SetUnLockStory(int _firstClearedStoryKey)
    {
        var keys = GameCore.Instance.DataMgr.GetOpenStages(_firstClearedStoryKey);
        if (keys == null)
            return;

        var clearData = GameCore.Instance.DataMgr.GetStoryData(_firstClearedStoryKey);
        foreach (int key in keys)
        {
            var data = GameCore.Instance.DataMgr.GetStoryData(key);
            if (data.chapter == clearData.chapter &&
                data.difficult == clearData.difficult)
                continue;

            if (clearData.stage == 10 && clearData.difficult == 1) // 난이도 보통
                PlayerPrefs.SetInt("FarmingChapterLock" + clearData.chapter, 0);

            if (unlockKey <= 0 || key < unlockKey)
                unlockKey = key;
        }
    }

    public bool CheckUnlockStory()
    {
        return 0 < unlockKey;
    }

    public StoryDataMap GetUnlockStoryData()
    {
        if (CheckUnlockStory() == true)
            return GameCore.Instance.DataMgr.GetStoryData(unlockKey);
        else
            return null;
    }

    internal void ResetUnlockStoryKey()
    {
        unlockKey = 0;
    }

    internal void SetDataStoryChapter(JSONObject _json)
    {
        if (_json == null) return;

        if (_json.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _json.Count; ++i)
                SetStoryChapterInternal(_json[i]);
        }
        else
        {
            SetStoryChapterInternal(_json);
        }
    }

    private void SetStoryChapterInternal(JSONObject _json)
    {
        StoryChapterSData data = new StoryChapterSData();
        data.SetData(_json);
        if (!storyRewardSDataDic.ContainsKey(data.key)) storyRewardSDataDic.Add(data.key, new StoryChapterSData[3]);
        storyRewardSDataDic[data.key][data.level - 1] = data;
    }

    internal void SetFriendSData(JSONObject _data)
    {
        if (_data == null) return;

        friendSDataDic.Clear();
        if (_data.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _data.Count; ++i)
                SetFriendSDataInternal(_data[i]);
        }
        else
        {
            SetFriendSDataInternal(_data);
        }
    }

    private void SetFriendSDataInternal(JSONObject _json)
    {
        FriendSData data = new FriendSData();
        data.SetData(_json);
        if (!friendSDataDic.ContainsKey(data.USER_UID)) friendSDataDic.Add(data.USER_UID, data);
        else friendSDataDic[data.USER_UID] = data;
    }

    public bool SetMyRoomData(JSONObject jsonData)
    {
        Debug.Log(jsonData.ToString());

        UserMyRoom = new MyRoomInfo();
        UserMyRoom.MyRoomDataList = new List<MyRoomData>();

        for (int roomIndex = 0; roomIndex < jsonData.Count; ++roomIndex)
        {
            MyRoomData roomData = new MyRoomData();
           
            jsonData[roomIndex].GetField(ref roomData.ID, "MYROOM_ID");
            jsonData[roomIndex].GetField(ref roomData.Delegate, "DELEGATE");

            JSONObject jsonItemList = jsonData[roomIndex].GetField("ITEM_LIST");
            if (jsonItemList == null)
            {
                Debug.Log("MyRoomSys ::: jsonItemList ITEM_LIST is null");
                return false;
            }

            #region MyRoomObjectData Set

            roomData.PlacedObjectList = new List<MyRoomObjectData>();

            for (int i = 0; i < jsonItemList.Count; ++i)
            {
                MyRoomObjectData placedItemData = new MyRoomObjectData();

                jsonItemList[i].GetField(ref placedItemData.ItemId, "ITEM_ID");
                jsonItemList[i].GetField(ref placedItemData.ItemUniqueid, "ITEM_UID");
                jsonItemList[i].GetField(ref placedItemData.UsedRoomId, "MYROOM_ID");
                jsonItemList[i].GetField(ref placedItemData.MyRoomUniqueid, "MYROOM_ITEM_UID");

                JSONObject positionDataList = jsonItemList[i].GetField("POSITION");

                if (positionDataList.IsArray == false) Debug.Log(string.Format("MyRoomSys ::: JsonData ITEM_LIST_[{0}] POSITION data is null", i));

                for (int k = 0; k < positionDataList.Count; ++k)
                {
                    Vector2 temp = Vector2.zero;
                    positionDataList[k].GetField(ref temp.x, "x"); //x
                    positionDataList[k].GetField(ref temp.y, "y"); //y
                    placedItemData.vectorList.Add(temp);
                }

                while (placedItemData.vectorList.Count < 2)
                {
                    placedItemData.vectorList.Add(Vector2.one);
                }

                placedItemData.SeverData = GetItemSData((long)placedItemData.ItemUniqueid);
                placedItemData.LocalData = GetItemData((long)placedItemData.ItemUniqueid);
                if (placedItemData.LocalData != null)
                {
                    roomData.PlacedObjectList.Add(placedItemData);
                    roomData.satisfactionValue += placedItemData.LocalData.optionValue[0];
                }
                else
                {
                    Debug.LogError("삭제된 가구가 있습니다.");
                }
            }


            #endregion

            #region MyRoomHeroData Set

            roomData.PlacedHeroList = new List<MyRoomHeroData>();

            var serchHeroList = from hero in HeroSdataDic
                                where hero.Value.dormitory == roomData.ID
                                select hero.Value;

            List<HeroSData> placedHeroList = serchHeroList.ToList();


            for (int i = 0; i < placedHeroList.Count; ++i)
            {
                MyRoomHeroData heroData = new MyRoomHeroData();

                heroData.ServerData = placedHeroList[i];
                heroData.LocalData = GameCore.Instance.DataMgr.GetUnitData(placedHeroList[i].key);
                roomData.PlacedHeroList.Add(heroData);
            }

            #endregion

            #region StainData Set

            roomData.StainDataList = new List<MyRoomStainData>();
            JSONObject jsonStainLIst = jsonData[roomIndex].GetField("STAIN_LIST");

            for (int i = 0; i < jsonStainLIst.Count; ++i)
            {
                JSONObject jsonStainData = jsonStainLIst[i];

                MyRoomStainData roomStain = new MyRoomStainData();
                //HELP_USER_NAME
                jsonStainLIst[i].GetField(ref roomStain.PlacedRoomId, "MYROOM_ID");
                jsonStainLIst[i].GetField(ref roomStain.UniqueId, "STAIN_UID");
                jsonStainLIst[i].GetField(ref roomStain.HelpUserId, "HELP_USER_UID");
                jsonStainLIst[i].GetField(ref roomStain.HelpUserName, "HELP_USER_NAME");
                jsonStainLIst[i].GetField(ref roomStain.RewardItemId, "REWARD_ITEM_ID");
                jsonStainLIst[i].GetField(ref roomStain.RewardItemCount, "REWARD_ITEM_COUNT");
                JsonParse.ToParse(jsonStainLIst[i], "START_TIME", out roomStain.CleanStartTime);
                JsonParse.ToParse(jsonStainLIst[i], "END_TIME", out roomStain.CleanEndTime);

                roomData.StainDataList.Add(roomStain);
            }

            #endregion


            UserMyRoom.MyRoomDataList.Add(roomData);
        }

        //UserMyRoom.TestPrintLog();

        return true;
    }

    internal List<long> GetNewHeroCardUidList()
    {
        if (newHeroCardUidList != null) return newHeroCardUidList;
        else return null;
    }

    internal List<long> GetNewItemCardUidList()
    {
        if (newItemCardUidList != null) return newItemCardUidList;
        else return null;
    }

    // 리스브 받는 처리후 출력될 아이템들을 가짜 CardSData로 만들어 반환한다.
    internal CardSData[] SetRewardItems(JSONObject _json)
    {
        //순서가 이상하길래 처리 수정함 20190630

        if (_json == null) return null;

        var cha_List = _json.GetField("CHA_LIST");
        if (cha_List != null)
        {
            GameCore.Instance.PlayerDataMgr.SetCardSData(cha_List);
        }
        var item_List = _json.GetField("ITEM_LIST");
        if (item_List != null)
        {
            GameCore.Instance.PlayerDataMgr.SetCardSData(item_List);
        }
        GameCore.Instance.CommonSys.UpdateMoney();

        var printList = _json.GetField("REWARD");
        if (printList == null) return null;

        List<CardSData> list = new List<CardSData>();
        for (int i = 0; i < printList.Count; ++i)
        {
            if (printList[i] == null)
            {
                Debug.LogError("Fake Reward Data is Null!! " + i);
                continue;
            }

            var data = GetFakeRewardItem(printList[i]);
            if (data != null)
                list.Add(data);
        }

        return list.ToArray();
    }

    internal RaidSData GetRaidSDataByDifficult(int _diff)
    {
        //잘 모르겠음

        for (int i = 0; i < raidSData.Count; ++i)
        {
            var data = GameCore.Instance.DataMgr.GetRaidData(raidSData[i].key);
            if (data.difficult - 1 == _diff) return raidSData[i];
        }

        return null;
    }

    internal void SetRaidSData(JSONObject _json)
    {
        if (_json.type == JSONObject.Type.ARRAY)
        {
            for (int i = 0; i < _json.Count; ++i)
                SetRaidSDataInternal(_json[i]);
        }
        else
            SetRaidSDataInternal(_json);
    }

    private void SetRaidSDataInternal(JSONObject _json)
    {
        RaidSData data = new RaidSData();
        data.SetData(_json);
        AddRaidSData(data);
    }

    public bool HandleMessage(GameEvent _evt)
	{
        switch(_evt.EvtType)
        {
            case GameEventType.ANS_LEVEL_UP: return ANS_LEVEL_UP(_evt.Para.GetPara<PacketPara>().data.data);
            case GameEventType.ANS_UPDATE_TEAM: return ANS_UPATE_TEAM(_evt.Para.GetPara<PacketPara>().data.data);
            case GameEventType.ANS_TUTORIAL: return true;
            default: return false;  
        }
	}

	internal void RemoveUnit(long _id)
	{
		if (!HeroSdataDic.ContainsKey(_id)) return;

        HeroSdataDic.Remove(_id);
		UserTeam.RemoveByUnit(_id);
        RemoveEquipByUnit(_id);
    }

    private void RemoveEquipByUnit(long _unitUid)
    {
        foreach(var item in ItemSdataDic)
        {
            if(item.Value.equipHeroUID == _unitUid) item.Value.equipHeroUID = -1;
        }
    }

	internal bool HasUnitSDataByCharID(int _key)
	{
        foreach (var unit in HeroSdataDic)
        {
            var data = GameCore.Instance.DataMgr.GetUnitData(unit.Value.key);
            if (data.charIdType == _key) return true;
        }

		return false;
	}

	internal bool AddUnit(HeroSData _sdata)
	{
		if (HeroSdataDic.ContainsKey(_sdata.uid))
		{
			Debug.LogError("Exist same UID Unit : " + _sdata.uid);
			return false;
		}

		HeroSdataDic.Add(_sdata.uid, _sdata);
		return true;
	}
	
	internal int RemoveItem(long _id, int _count)
	{
		if (!ItemSdataDic.ContainsKey(_id)) return -1;

        var sdata = ItemSdataDic[_id];
		sdata.count -= _count;

		if (sdata.count <= 0)
		{
			ItemSdataDic.Remove(_id);
			return 0;
		}
		return sdata.count;
	}

	internal void RemoveFarmingData(int _key)
	{
        FarmingSData target = farmingSDataDic.Find(data => data.id == _key);
        if (target != null) farmingSDataDic.Remove(target);
    }

	internal void AddFarmingData(JSONObject _json)
	{
		if (_json == null) return;
        var data = new FarmingSData();
		data.SetData(_json);
		farmingSDataDic.Add(data);
	}

    internal void AddFriend(FriendSData _data)
    { 
        if (!friendSDataDic.ContainsKey(_data.USER_UID)) friendSDataDic.Add(_data.USER_UID, _data);
        else Debug.LogError("이미 존재하는 친구 UID 입니다." + _data.USER_UID);
    }

    internal void RemoveFriend(FriendSData _data)
    {
        if (friendSDataDic.ContainsKey(_data.USER_UID)) friendSDataDic.Remove(_data.USER_UID);
        else Debug.LogError("존재하지 않는 친구 UID 입니다." + _data.USER_UID);
    }

	internal void AddExpReward(CardSData _sdata)
	{
        if (_sdata.type == CardType.Character || _sdata.type == CardType.Equipment)
        {
            expRewardItems.Add(_sdata);
            return;
        }

        CardSData target = expRewardItems.Find(data => data.key == _sdata.key);
        if (target != null) (target as ItemSData).count += (_sdata as ItemSData).count;
    }

    internal void AddRaidSData(RaidSData _data)
    {
        RaidSData target = raidSData.Find(data => data.key == _data.key);
        if (target != null) target = _data;
        else raidSData.Add(_data);
    }


    public void ResetDailyDungeonTicket()
    {
        var constData = GameCore.Instance.DataMgr.GetDailyDungeonConstData();
        LocalUserData.DailyDungeonRechargeAbleCount = constData.dailyPurchaseCount;

        var item = GameCore.Instance.PlayerDataMgr.GetItemByKey((int)ResourceType.Ticket_daily);
        if (item != null)
            item.count = constData.dailyDefTicket;

        if (GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.DailyPrepare)
        {
            var sys = GameCore.Instance.SubsysMgr.GetNowSubSys() as DailyDungeonSys;
            sys.UpdateTicketCount();
        }
    }


    internal bool IsInited_Attendance()
    {
        return attendanceDic != null;
    }

    internal int GetAttendanceLastTakedValue(int _aCheckKey)
    {
        if (attendanceDic == null ||
            !attendanceDic.ContainsKey(_aCheckKey))
            return 0;

        return attendanceDic[_aCheckKey].AI;
    }

    internal DateTime GetAttendanceLastTakedDate(int _aCheckKey)
    {
        if (attendanceDic == null ||
            !attendanceDic.ContainsKey(_aCheckKey))
            return DateTime.MinValue;

        return attendanceDic[_aCheckKey].RD;
    }

    internal void SetAttendanceData(JSONObject _json) // (_json = data.GetField("ATTENDANCE")) == List
    {
        if (attendanceDic == null)
            attendanceDic = new Dictionary<int, AttendanceSData>();

        if (_json.type == JSONObject.Type.ARRAY)
            for(int i = 0; i < _json.Count; ++i)
                SetAttendanceData_Internal(_json[i]);
        else
            SetAttendanceData_Internal(_json);
    }

    void SetAttendanceData_Internal(JSONObject _json)
    {
        int at = 0;
        _json.GetField(ref at, "AT");

        if (!attendanceDic.ContainsKey(at))
            attendanceDic.Add(at, new AttendanceSData());
        attendanceDic[at].SetData(_json);
    }


    internal void SetMainTutorial()
    {
        tutorialDataMap.main += 1;
        GameCore.Instance.NetMgr.Req_Account_Change_Tutorial(tutorialDataMap);
    }

    internal void SetSubTutorial(AutonomyTutoType tutorialType, int _idx)
    {
        switch(tutorialType)
        {
            case AutonomyTutoType.Daily:    tutorialDataMap.dungeon |= 1 << _idx; break;
            case AutonomyTutoType.Raid:     tutorialDataMap.raid    |= 1 << _idx; break;
            case AutonomyTutoType.PvP:      tutorialDataMap.pvp     |= 1 << _idx; break;
            case AutonomyTutoType.MyRoom:   tutorialDataMap.myRoom  |= 1 << _idx; break;
            case AutonomyTutoType.Manufact: tutorialDataMap.manufact|= 1 << _idx; break;
            case AutonomyTutoType.Farming:  tutorialDataMap.farming |= 1 << _idx; break;
            case AutonomyTutoType.Mission:  tutorialDataMap.mission |= 1 << _idx; break;
            case AutonomyTutoType.Mail:     tutorialDataMap.mail    |= 1 << _idx; break;
            default:Debug.LogError("Error_WrongType SubTutorial");break;
        }
        GameCore.Instance.NetMgr.Req_Account_Change_Tutorial(tutorialDataMap);
    }





    #region "Method For Message Recived From Server "

    private bool ANS_LEVEL_UP(JSONObject _data)
    {
        var reward = _data.GetField("REWARD");
        var list = SetRewardItems(reward);

        for (int i = 0; i < list.Length; i++) AddExpReward(list[i]);

        int level = 0;
        _data.GetField("ACCOUNT").GetField(ref level, "USER_LEVEL");

#if UNITY_ANDROID
        // 레벨업
        // ADBrix제거
        //AdBrixRmAOS.AdBrixRm.gameLevelAchieved(level);
#endif

        return true;
    }

    private bool ANS_UPATE_TEAM(JSONObject _data)
    {
        int code = -1;
        _data.GetField(ref code, "result");

        if (code == 0)
        {
            var team = _data.GetField("TEAM");
            UpdateTeamData(team);
            GameCore.Instance.ShowAlert("저장되었습니다.");
            GameCore.Instance.MissionMgr.CheckMission(CommonType.DEF_KEY_MISSION_DEFINE + 40);
            return true;
        }
        else
        {
            GameCore.Instance.ShowAlert("팀 정보 저장 실패");
        }

        return false;
    }

#endregion
}

