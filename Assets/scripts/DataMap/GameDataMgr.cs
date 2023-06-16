using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데이터맵 타입
/// </summary>
internal enum DataMapType
{
	AssetBundle,		// 에셋번들
	Stage,					// 스테이지
//	StageInfo,			// 스토리 화면에서 보여질 스에티이 정보들
//	Chapter,				// 스토리 챕터 정보
	Sprite,					// 스프라이트
	Unit,						// 캐릭터
	Chemistry,			// 캐미스트리
	Skill,					// 스킬
	TeamSkill,			// 팀스킬
	Effect,					// 버프효과

	Item,						// 아이템
	ItemEffect,			// 아이템 효과
	SetFurniture,		// 가구 세트
	SetFurnitureSet,// 가구 세트별 데이터

	Farming,				// 파밍
	FarmingReward,	// 파밍 보상
	Gacha,					// 뽑기
	MakingSlotCost,			// 제조 슬롯

	Story,					// 스토리
	StoryChapter,		// 스토리 챕터
	StoryReward,		// 스토리 보상

	MyRoom,					// 마이룸
	MyRoomEffect,		// 마이룸 효과
	MyRoomRandomBox,// 마이룸 랜덤 박스

	PvPRateReward,	// PvP 등급 보상
	PvPAdvancement,	// PvP 등급 조정
	Exp,						// 레벨업에 대한 데이터

	BattleCommon,		// 전투에 계산 공식에 사용되는 상수
	CharacterCommon,// 영웅 관련 상수
	InventoryCommon,// 인벤토리 관련 상수
	FarmingCommon,  // 파밍관련 상수
	StoryCommon,		// 스토리관련 상수
	MyRoomCommon,			// 마이룸 관련 상수
	StaminaCommon,	// 특수 재화(행동력, PVP)에대한 상수
    PvPCommon,      // PvP 관련 상수
    PvECommon,      // 레이드 관련 상수
    FriendCommon,   // 친구 관련 상수
    DDungeonCommon, // 요일던전 관련 상수
    ItemStrengthenCommon, // 아이템 강화 배율 상후
    MissionCommon,      // 미션 관련 상수

    StringCharacter,// 캐릭터 스트링
	StringItem,			// 아이템 스트링
	StringFarming,	// Farming 스트링
	StringStory,		// 스토리 스트링
	StringGacha,		// 뽑기 스트링
    StringMail,         // 메일용 문자열들
    StringChrChemy,     // 케미 스트링
    StringProfile,      // 영웅 프로필

    DailyDungeon,       // 요일던전 데이터
    DailyDungeonReward, // 요일던전 보상

    Raid,               // 레이드 스테이지 데이터
    RaidRankReward,     // 레이드 랭킹 보상

    Mission,            // 미션 데이터 정의
    MissionDaily,       // 일간 미션
    MissionWeekly,      // 주간 미션
    MissionAchieve,     // 업적
    MissionQuest,       // 퀘스트
    MissionAccumReward, // 미션 클리어 보상

    Strengthen,         // 캐릭터 강화 수치 테이블
    StrengthenCost,     // BSNeed로, 강화 비용 테이블
    ItemStrengthenCost, // 아이템 강화 비용 테이블
    Powercal,           //캐릭터 강화 수치

    BattleChapter,      //각 챕터 스토리 정보 테이블
    Tutorial,           //튜토리얼
    Audio,              //오디오 테이블


    ACheck,             // 출첵 항목
    ACheckReward,       // 출첵 항목의 일차별 아이템


    // Todo : 데이터맵 종류 추가 (enum)

    ShopPackageInfo,
    ShopItemString,
    ShopPackage,
    PushString,

    MainBanner,         // 로비화면 좌하단의 배너
}


/// <summary>
/// 데이터맵을 총괄적으로 관리한다.(사실 들고 있기만 한다.)
/// </summary>
internal class GameDataMgr
{
	static Dictionary<DataMapType,object> tables;

	static Dictionary<long, ChemistryDataMap> chemiFindMap;
	static Dictionary<int, List<TeamSkillDataMap>> teamSkillFindMap;
	static Dictionary<int, List<SetFurnitureSetDataMap>> setInteriorFindMap;
	static Dictionary<int, List<PvPAdvancementDataMap>> pvpAdvancementFindMap;

    static Dictionary<int, List<int>> achieveFindMap;  // 업적 찾기 테이블

    static Dictionary<int, List<int>> openStageMap;     // 열리는스테이지 찾는 테이블 

    /// <summary>
    /// 생성자
    /// 데이터맵 테이블을 생성한다.
    /// </summary>
    internal GameDataMgr()
	{
		if(tables==null)
		{
			tables = new Dictionary<DataMapType, object>();
            AddTable<ABLocaDataMap>(DataMapType.AssetBundle, "BABLoad.txt");
            AddTable<SpriteDataMap>(DataMapType.Sprite, "BSprite.txt");
            AddTable<StageDataMap>(DataMapType.Stage, "BStage.txt");
            AddTable<StoryChapterDataMap>(DataMapType.StoryChapter, "BChapter.txt");
            AddTable<StoryDataMap>(DataMapType.Story, "BStory.txt");
            AddTable<StoryRewardDataMap>(DataMapType.StoryReward, "BStoryReward.txt");

            AddTable<UnitDataMap>(DataMapType.Unit, "BCharacter.txt");
            AddTable<ChemistryDataMap>(DataMapType.Chemistry, "BChemistry.txt");
            AddTable<SkillDataMap>(DataMapType.Skill, "BSkill.txt");
            AddTable<TeamSkillDataMap>(DataMapType.TeamSkill, "Bteamskill.txt");
            AddTable<EffectDataMap>(DataMapType.Effect, "BEffect.txt");

            AddTable<ItemDataMap>(DataMapType.Item, "BItem.txt");
            AddTable<RuleItemEffectDataMap>(DataMapType.ItemEffect, "BRuleItemEffect.txt");
            AddTable<SetFurnitureDataMap>(DataMapType.SetFurniture, "BSetFurniture.txt");
            AddTable<SetFurnitureSetDataMap>(DataMapType.SetFurnitureSet, "BSetFurnitureSet.txt");

            AddTable<FarmingDataMap>(DataMapType.Farming, "BFarming.txt");
            AddTable<FarmingRewardDataMap>(DataMapType.FarmingReward, "BFarmingReward.txt");
            AddTable<GachaDataMap>(DataMapType.Gacha, "BGacha.txt");
            AddTable<MakingSlotCostDataMap>(DataMapType.MakingSlotCost, "BMakingSlotCost.txt");

            AddTable<MyRoomDataMap>(DataMapType.MyRoom, "BMyRoom.txt");
            //AddTable<MyRoomEffectDataMap>(DataMapType.MyRoomEffect, "BRuleRoomEffect.txt");
            //AddTable<MyRoomRandomBoxDataMap>(DataMapType.MyRoomRandomBox, "BMyRoomRandomBox.txt");

            AddTable<PvPRateRewardDataMap>(DataMapType.PvPRateReward, "BRateReward.txt");
            AddTable<PvPAdvancementDataMap>(DataMapType.PvPAdvancement, "BAdvancement.txt");

            AddTable<ExpDataMap>(DataMapType.Exp, "BExp.txt");

            AddTable<StringDataMap>(DataMapType.StringCharacter, "BCharacterString.txt");
            AddTable<StringDataMap>(DataMapType.StringItem, "BItemString.txt");
            AddTable<StringDataMap>(DataMapType.StringFarming, "BFarmingString.txt");
            AddTable<StringDataMap>(DataMapType.StringStory, "BStoryString.txt");
            AddTable<StringDataMap>(DataMapType.StringGacha, "BGachaString.txt");
            AddTable<StringDataMap>(DataMapType.StringMail, "BMailString.txt");
            AddTable<ChrChemyDataMap>(DataMapType.StringChrChemy, "BChrChemyString.txt");

            AddTable<ProfileDataMap>(DataMapType.StringProfile, "BChrProfileString.txt");
            
            AddTable<CharacterCommonDataMap>(DataMapType.CharacterCommon, "BCharacterCommon.txt");
            AddTable<InventoryCommonDataMap>(DataMapType.InventoryCommon, "BInventoryCommon.txt");
            AddTable<MyRoomCommonDataMap>(DataMapType.MyRoomCommon, "BMyRoomCommon.txt");
            AddTable<FarmingCommonDataMap>(DataMapType.FarmingCommon, "BFarmingCommon.txt");
            AddTable<StoryCommonDataMap>(DataMapType.StoryCommon, "BStoryCommon.txt");
            AddTable<StaminaCommonDataMap>(DataMapType.StaminaCommon, "BStaminaCommon.txt");
            AddTable<PvPCommonDataMap>(DataMapType.PvPCommon, "BPvPCommon.txt");
            AddTable<PvECommonDataMap>(DataMapType.PvECommon, "BPvECommon.txt");
            AddTable<FriendCommonDataMap>(DataMapType.FriendCommon, "BFriendsCommon.txt");
            AddTable<DDungeonCommonDataMap>(DataMapType.DDungeonCommon, "BDDungeonCommon.txt");
            AddTable<ItemStrengthenCommonDataMap>(DataMapType.ItemStrengthenCommon, "BItemStrenghten.txt");
            AddTable<MissionCommonDataMap>(DataMapType.MissionCommon, "BMissionCommon.txt"); 

            AddTable<BDDungeonDataMap>(DataMapType.DailyDungeon, "BDDungeon.txt");
            AddTable<BDDRewardDataMap>(DataMapType.DailyDungeonReward, "BDDReward.txt");

            AddTable<RaidDataMap>(DataMapType.Raid, "BRaid.txt");
            AddTable<RaidRankRewardDataMap>(DataMapType.RaidRankReward, "BRRReward.txt");

            AddTable<MissionDefineDataMap>(DataMapType.Mission, "BMissionDefine.txt");
            AddTable<MissionDataMap>(DataMapType.MissionDaily, "BDailyMission.txt");
            AddTable<MissionDataMap>(DataMapType.MissionWeekly, "BWeeklyMission.txt");
            AddTable<AchieveDataMap>(DataMapType.MissionAchieve, "BAchieve.txt");
            AddTable<MissionDataMap>(DataMapType.MissionQuest, "BQuest.txt");
            AddTable<MissionAccumRewardDataMap>(DataMapType.MissionAccumReward, "BAccumReward.txt");

            AddTable<StrengthenDataMap>(DataMapType.Strengthen, "BStrengthen.txt");
            AddTable<StrengthenCostDataMap>(DataMapType.StrengthenCost, "BSNeed.txt");
            AddTable<ItemStrengthenCostDataMap>(DataMapType.ItemStrengthenCost, "BISNeed.txt");
            AddTable<PowercalDataMap>(DataMapType.Powercal, "BPowercal.txt");


            AddTableList<BattleChapterDataMap>(DataMapType.BattleChapter, "BScript.txt");
            AddTableList<TutorialDataMap>(DataMapType.Tutorial, "BOBTTutoScript.txt");
            AddTable<SoundCommonDataMap>(DataMapType.Audio, "BSound.txt");

            AddTable<ACheckDataMap>(DataMapType.ACheck, "BACheck.txt");
            AddTable<ACheckRewardDataMap>(DataMapType.ACheckReward, "BACheckReward.txt");

            // Todo : 데이터맵 타입 추가시 (테이블 추가)

            AddTable<ShopPackageInfoDataMap>(DataMapType.ShopPackageInfo, "BShopPackageInfo.txt");
            AddTable<ShopItemStringDataMap>(DataMapType.ShopItemString, "BShopString.txt");
            AddTable<ShopPackageDataMap>(DataMapType.ShopPackage, "BShopPackage.txt");
            AddTable<PushStringDataMap>(DataMapType.PushString, "BPushStirng.txt");
            AddTable<MainBannerDataMap>(DataMapType.MainBanner, "BMainBanner.txt");


            Debug.Log("DataMgr Init Done");
        }

		//캐미스트리 전용 재배치된 테이블 생성(빠른 찾기를위해)
		if (chemiFindMap == null)
		{
			chemiFindMap = new Dictionary<long, ChemistryDataMap>();
			var enumerator = ((DataMapCtrl<ChemistryDataMap>)tables[DataMapType.Chemistry]).GetEnumerator();
			while(enumerator.MoveNext())
			{
				long key  = (long)enumerator.Current.Value.tgId;
						 key |= (long)enumerator.Current.Value.needId << 32;

				if(chemiFindMap.ContainsKey(key))
					Debug.LogError("대상이 중복되는 캐미가 존재합니다!" + key.ToString("x"));
				else
					chemiFindMap.Add(key, enumerator.Current.Value);
			}
		}

		// TeamSkill 전용 빠른 찾기를 위한테이블
		if(teamSkillFindMap == null)
		{
			teamSkillFindMap = new Dictionary<int, List<TeamSkillDataMap>>();
			var enumerator = ((DataMapCtrl<TeamSkillDataMap>)tables[DataMapType.TeamSkill]).GetEnumerator();
			while(enumerator.MoveNext())
			{
				var data = enumerator.Current.Value;
				for (int i = 0; i < data.needChar.Length; i++)
				{
					if (!teamSkillFindMap.ContainsKey(data.needChar[i]))
						teamSkillFindMap.Add(data.needChar[i], new List<TeamSkillDataMap>());
					teamSkillFindMap[data.needChar[i]].Add(data);
				} 
			}
		}

		// 세트 가구 전용 빠른 찾기를 위한 테이블
		if(setInteriorFindMap == null)
		{
			setInteriorFindMap = new Dictionary<int, List<SetFurnitureSetDataMap>>();
			var enumerator = ((DataMapCtrl<SetFurnitureSetDataMap>)tables[DataMapType.SetFurnitureSet]).GetEnumerator();
			while (enumerator.MoveNext())
			{
				var data = enumerator.Current.Value;
				if (!setInteriorFindMap.ContainsKey(data.groupID))
					setInteriorFindMap.Add(data.groupID, new List<SetFurnitureSetDataMap>());
				setInteriorFindMap[data.groupID].Add(data);
			}
		}

		// PvP 조정에 관련된 테이블 ( rateType으로 분류되고, rankLimit으로 정렬된다. )
		if(pvpAdvancementFindMap == null)
		{
			pvpAdvancementFindMap = new Dictionary<int, List<PvPAdvancementDataMap>>();
			var enumerator = ((DataMapCtrl<PvPAdvancementDataMap>)tables[DataMapType.PvPAdvancement]).GetEnumerator();
			while (enumerator.MoveNext())
			{
				var data = enumerator.Current.Value;
				if (!pvpAdvancementFindMap.ContainsKey(data.rateType))
					pvpAdvancementFindMap.Add(data.rateType, new List<PvPAdvancementDataMap>());

				var list = pvpAdvancementFindMap[data.rateType];
				list.Add(data);
				for(int i = list.Count-1; i > 0; --i)
				{
					if (list[i].rankLimit < list[i - 1].rankLimit)
					{
						var tmp = list[i];
						list[i] = list[i - 1];
						list[i - 1] = tmp;
					}
					else
						break;
				}
			}
		}

        // 업적 미션 찾기 테이블
        if(achieveFindMap == null)
        {
            achieveFindMap = new Dictionary<int, List<int>>();
            var enumerator = ((DataMapCtrl<AchieveDataMap>)tables[DataMapType.MissionAchieve]).GetEnumerator();

            while(enumerator.MoveNext())
            {
                var data = enumerator.Current.Value;
                if (!achieveFindMap.ContainsKey(data.defineKey))
                    achieveFindMap.Add(data.defineKey, new List<int>());
                achieveFindMap[data.defineKey].Add(data.id);
            }
        }

        if (openStageMap == null)
        {
            openStageMap = new Dictionary<int, List<int>>();
            var enumerator = ((DataMapCtrl<StoryDataMap>)tables[DataMapType.Story]).GetEnumerator();
            while (enumerator.MoveNext())
            {
                var data = enumerator.Current.Value;
                if (!openStageMap.ContainsKey(data.openCondition))
                    openStageMap.Add(data.openCondition, new List<int>());
                openStageMap[data.openCondition].Add(data.id);
            }
        }
	}

    void AddTable<T>(DataMapType _type, string _fileName) where T : CSVParse, new()
    {
        tables.Add(_type, new DataMapCtrl<T>(_type, _fileName));
    }
    void AddTableList<T>(DataMapType _type, string _fileName) where T : CSVParse, new()
    {
        tables.Add(_type, new DataMapListCtrl<T>(_type, _fileName));
    }

    internal object GetDataList(DataMapType _type)
	{
		return tables[_type];
	}

	#region 타입별 데이터반환 함수

	internal ABLocaDataMap GetAssetBundleData(int _id)
	{	return ((DataMapCtrl<ABLocaDataMap>)tables[DataMapType.AssetBundle]).GetData(_id);	}
	internal SpriteDataMap GetSpriteData(int _id)
	{ return ((DataMapCtrl<SpriteDataMap>)tables[DataMapType.Sprite]).GetData(_id); }
	internal StoryChapterDataMap GetStoryChapterData(int _id)
	{ return ((DataMapCtrl<StoryChapterDataMap>)tables[DataMapType.StoryChapter]).GetData(_id); }
	internal StageDataMap GetStageData(int _id)
	{ return ((DataMapCtrl<StageDataMap>)tables[DataMapType.Stage]).GetData(_id); }
	internal StoryDataMap GetStoryData(int _id)
	{ return ((DataMapCtrl<StoryDataMap>)tables[DataMapType.Story]).GetData(_id); }
	internal StoryRewardDataMap GetStoryRewardData(int _id)
	{ return ((DataMapCtrl<StoryRewardDataMap>)tables[DataMapType.StoryReward]).GetData(_id); }
	internal UnitDataMap GetUnitData(int _id)
	{ return ((DataMapCtrl<UnitDataMap>)tables[DataMapType.Unit]).GetData(_id); }
	internal ChemistryDataMap GetchemistryData(int _id)
	{ return ((DataMapCtrl<ChemistryDataMap>)tables[DataMapType.Chemistry]).GetData(_id); }
	internal SkillDataMap GetSkillData(int _id)
	{ return ((DataMapCtrl<SkillDataMap>)tables[DataMapType.Skill]).GetData(_id); }
	internal TeamSkillDataMap GetTeamSkillData(int _id)
	{ return ((DataMapCtrl<TeamSkillDataMap>)tables[DataMapType.TeamSkill]).GetData(_id); }
	internal EffectDataMap GetEffectData(int _id)
	{ return ((DataMapCtrl<EffectDataMap>)tables[DataMapType.Effect]).GetData(_id); }
	internal ItemDataMap GetItemData(int _id)
	{ return ((DataMapCtrl<ItemDataMap>)tables[DataMapType.Item]).GetData(_id); }
	internal RuleItemEffectDataMap GetItemEffectData(int _id)
	{ return ((DataMapCtrl<RuleItemEffectDataMap>)tables[DataMapType.ItemEffect]).GetData(_id); }
	internal SetFurnitureDataMap GetSetInteriorData(int _id)
	{ return ((DataMapCtrl<SetFurnitureDataMap>)tables[DataMapType.SetFurniture]).GetData(_id); }
	internal SetFurnitureSetDataMap GetSetInteriorSetData(int _id)
	{ return ((DataMapCtrl<SetFurnitureSetDataMap>)tables[DataMapType.SetFurnitureSet]).GetData(_id); }
	internal FarmingDataMap GetFarmingData(int _id)
	{ return ((DataMapCtrl<FarmingDataMap>)tables[DataMapType.Farming]).GetData(_id); }
	internal FarmingRewardDataMap GetFarmingRewardData(int _id)
	{ return ((DataMapCtrl<FarmingRewardDataMap>)tables[DataMapType.FarmingReward]).GetData(_id); }
	internal GachaDataMap GetGachaData(int _id)
	{ return ((DataMapCtrl<GachaDataMap>)tables[DataMapType.Gacha]).GetData(_id); }
	internal MakingSlotCostDataMap GetMakingSlotData(int _id)
	{ return ((DataMapCtrl<MakingSlotCostDataMap>)tables[DataMapType.MakingSlotCost]).GetData(_id); }
	internal MyRoomDataMap GetMyRoomData(int _id)
	{ return ((DataMapCtrl<MyRoomDataMap>)tables[DataMapType.MyRoom]).GetData(_id); }
	//internal MyRoomEffectDataMap GetMyRoomEffectData(int _id)
	//{ return ((DataMapCtrl<MyRoomEffectDataMap>)tables[DataMapType.MyRoomEffect]).GetData(_id); }
	internal MyRoomRandomBoxDataMap GetMyRoomRandomBoxData()
	{ return ((DataMapCtrl<MyRoomRandomBoxDataMap>)tables[DataMapType.MyRoomRandomBox]).GetData(1); }
	internal PvPRateRewardDataMap GetPvPRateRewardData(int _id)
	{ return ((DataMapCtrl<PvPRateRewardDataMap>)tables[DataMapType.PvPRateReward]).GetData(_id); }
	internal PvPAdvancementDataMap GetPvPAdvancementData(int _id)
	{ return ((DataMapCtrl<PvPAdvancementDataMap>)tables[DataMapType.PvPAdvancement]).GetData(_id); }
	internal ExpDataMap GetExpData(int _id)
	{ return ((DataMapCtrl<ExpDataMap>)tables[DataMapType.Exp]).GetData(_id); }
	internal CharacterCommonDataMap GetCharacterConstData()
	{ return ((DataMapCtrl<CharacterCommonDataMap>)tables[DataMapType.CharacterCommon]).GetData(1); }
	internal InventoryCommonDataMap GetInventoryConstData()
	{ return ((DataMapCtrl<InventoryCommonDataMap>)tables[DataMapType.InventoryCommon]).GetData(1); }
	internal FarmingCommonDataMap GetFarmingConstData()
	{ return ((DataMapCtrl<FarmingCommonDataMap>)tables[DataMapType.FarmingCommon]).GetData(1); }
	internal StoryCommonDataMap GetStoryConstData()
	{ return ((DataMapCtrl<StoryCommonDataMap>)tables[DataMapType.StoryCommon]).GetData(1); }
	internal MyRoomCommonDataMap GetMyRoomConstData()
	{ return ((DataMapCtrl<MyRoomCommonDataMap>)tables[DataMapType.MyRoomCommon]).GetData(1); }
	internal StaminaCommonDataMap GetStaminaConstData()
	{ return ((DataMapCtrl<StaminaCommonDataMap>)tables[DataMapType.StaminaCommon]).GetData(1); }
    internal PvPCommonDataMap GetPvPConstData()
    { return ((DataMapCtrl<PvPCommonDataMap>)tables[DataMapType.PvPCommon]).GetData(1); }
    internal PvECommonDataMap GetPvEConstData()
    { return ((DataMapCtrl<PvECommonDataMap>)tables[DataMapType.PvECommon]).GetData(1); }
    internal FriendCommonDataMap GetFriendConstData()
    { return ((DataMapCtrl<FriendCommonDataMap>)tables[DataMapType.FriendCommon]).GetData(1); }
    internal DDungeonCommonDataMap GetDailyDungeonConstData()
    { return ((DataMapCtrl<DDungeonCommonDataMap>)tables[DataMapType.DDungeonCommon]).GetData(1); }
    internal ItemStrengthenCommonDataMap GetItemStrengthenConstData()
    { return ((DataMapCtrl<ItemStrengthenCommonDataMap>)tables[DataMapType.ItemStrengthenCommon]).GetData(1); }
    internal MissionCommonDataMap GetMissionConstData()
    { return ((DataMapCtrl<MissionCommonDataMap>)tables[DataMapType.MissionCommon]).GetData(1); }

    internal string GetCharacterStringData(int _id)
	{ return ((DataMapCtrl<StringDataMap>)tables[DataMapType.StringCharacter]).GetData(_id).str; }
	internal string GetItemStringData(int _id)
	{ return ((DataMapCtrl<StringDataMap>)tables[DataMapType.StringItem]).GetData(_id).str; }
	internal string GetFarmingStringData(int _id)
	{ return ((DataMapCtrl<StringDataMap>)tables[DataMapType.StringFarming]).GetData(_id).str; }
	internal string GetStoryStringData(int _id)
	{ return ((DataMapCtrl<StringDataMap>)tables[DataMapType.StringStory]).GetData(_id).str; }
	internal string GetGachaStringData(int _id)
	{ return ((DataMapCtrl<StringDataMap>)tables[DataMapType.StringGacha]).GetData(_id).str; }
    internal string GetMailStringData(int _id)
    { return ((DataMapCtrl<StringDataMap>)tables[DataMapType.StringMail]).GetData(_id).str; }
    internal  ChrChemyDataMap GetChrChemyStringData(int _id)
    { return ((DataMapCtrl<ChrChemyDataMap>)tables[DataMapType.StringChrChemy]).GetData(_id); }

    internal ProfileDataMap GetProfileStringData(int _charKey)
    { return ((DataMapCtrl<ProfileDataMap>)tables[DataMapType.StringProfile]).GetData(_charKey); }


    internal BDDungeonDataMap GetDailyDungeonData(int _key)
    { return ((DataMapCtrl<BDDungeonDataMap>)tables[DataMapType.DailyDungeon]).GetData(_key); }
    internal BDDRewardDataMap GetDailyDungeonRewardData(int _key)
    { return ((DataMapCtrl<BDDRewardDataMap>)tables[DataMapType.DailyDungeonReward]).GetData(_key); }

    internal RaidDataMap GetRaidData(int _key)
    { return ((DataMapCtrl<RaidDataMap>)tables[DataMapType.Raid]).GetData(_key); }
    internal RaidRankRewardDataMap GetRaidRankRewardData(int _key)
    { return ((DataMapCtrl<RaidRankRewardDataMap>)tables[DataMapType.RaidRankReward]).GetData(_key); }


    internal MissionDefineDataMap GetMissionDefineData(int _key)
    { return ((DataMapCtrl<MissionDefineDataMap>)tables[DataMapType.Mission]).GetData(_key); }

    internal MissionDataMap GetMissionData(MissionType _type, int _key)
    {
        switch (_type)
        {
            case MissionType.Daily: return ((DataMapCtrl<MissionDataMap>)tables[DataMapType.MissionDaily]).GetData(_key);
            case MissionType.Weekly:return ((DataMapCtrl<MissionDataMap>)tables[DataMapType.MissionWeekly]).GetData(_key);
            case MissionType.Achieve:return ((DataMapCtrl<AchieveDataMap>)tables[DataMapType.MissionAchieve]).GetData(_key);
            case MissionType.Quest:return ((DataMapCtrl<MissionDataMap>)tables[DataMapType.MissionQuest]).GetData(_key);
        }
        return null;
    }

    internal MissionDataMap GetMissionDailyData(int _key)
    { return ((DataMapCtrl<MissionDataMap>)tables[DataMapType.MissionDaily]).GetData(_key); }

    internal MissionDataMap GetMissionWeeklyData(int _key)
    { return ((DataMapCtrl<MissionDataMap>)tables[DataMapType.MissionWeekly]).GetData(_key); }

    internal AchieveDataMap GetMissionAchieveData(int _key)
    { return ((DataMapCtrl<AchieveDataMap>)tables[DataMapType.MissionAchieve]).GetData(_key); }

    internal MissionDataMap GetMissionQuestData(int _key)
    { return ((DataMapCtrl<MissionDataMap>)tables[DataMapType.MissionQuest]).GetData(_key); }

    internal MissionAccumRewardDataMap GetMissionAccumRewardData(int _key)
    { return ((DataMapCtrl<MissionAccumRewardDataMap>)tables[DataMapType.MissionAccumReward]).GetData(_key); }

    internal StrengthenDataMap GetStrengthenData(int StrengthenType, int _charType)//int strengthenDataID)
    //{ return ((DataMapCtrl<StrengthenDataMap>)tables[DataMapType.Strengthen]).GetData(strengthenDataID); }
    { return ((DataMapCtrl<StrengthenDataMap>)tables[DataMapType.Strengthen]).GetData((StrengthenType << 8) + _charType); }

    internal StrengthenCostDataMap GetStrengthenCostData(int _key)
    { return ((DataMapCtrl<StrengthenCostDataMap>)tables[DataMapType.StrengthenCost]).GetData(_key); }

    internal StrengthenCostDataMap GetStrengthenCostData(int _evol, int _strengthen)
    {
        return GetStrengthenCostData((_evol << 8) + _strengthen);
    }
    internal PowercalDataMap GetPowercalData(int _key)
    {
        return ((DataMapCtrl<PowercalDataMap>)tables[DataMapType.Powercal]).GetData(_key);
    }

    internal ItemStrengthenCostDataMap GetItemStrengthenCostData(int _key)
    { return ((DataMapCtrl<ItemStrengthenCostDataMap>)tables[DataMapType.ItemStrengthenCost]).GetData(_key); }

    internal ItemStrengthenCostDataMap GetItemStrengthenCostData(int _rank, int _strengthen)
    {
        return GetItemStrengthenCostData((_rank << 8) + _strengthen);
    }

    internal bool IsHaveItemStrengthenCostData(int _rank, int _strengthen)
    {
        return ((DataMapCtrl<ItemStrengthenCostDataMap>)tables[DataMapType.ItemStrengthenCost]).IsHave((_rank << 8) + _strengthen);
    }

    internal BattleChapterDataMap GetBattleChatperData(int chapter)
    {
        return ((DataMapCtrl<BattleChapterDataMap>)tables[DataMapType.BattleChapter]).GetData(chapter);
    }

    internal SoundCommonDataMap GetSoundCommonData(int key)
    {
        return ((DataMapCtrl<SoundCommonDataMap>)tables[DataMapType.Audio]).GetData(key);
    }

    // Todo : 데이터맵 타입 추가시 (GetData함수 추가)

    internal ShopPackageInfoDataMap GetShopPackageInfoData(int key)
    {
        return ((DataMapCtrl<ShopPackageInfoDataMap>)tables[DataMapType.ShopPackageInfo]).GetData(key);
    }

    internal ShopItemStringDataMap GetShopItemStringData(int key)
    {
        return ((DataMapCtrl<ShopItemStringDataMap>)tables[DataMapType.ShopItemString]).GetData(key);
    }
    
    internal ShopPackageDataMap GetShopPackageData(int key)
    {
        return ((DataMapCtrl<ShopPackageDataMap>)tables[DataMapType.ShopPackage]).GetData(key);
    }

    internal ACheckDataMap GetACheckData(int key)
    {
        return ((DataMapCtrl<ACheckDataMap>)tables[DataMapType.ACheck]).GetData(key);
    }

    internal ACheckRewardDataMap GetACheckRewardData(int key)
    {
        return ((DataMapCtrl<ACheckRewardDataMap>)tables[DataMapType.ACheckReward]).GetData(key);
    }

    internal MainBannerDataMap GetMainBannerData(int key)
    {
        return ((DataMapCtrl<MainBannerDataMap>)tables[DataMapType.MainBanner]).GetData(key);
    }


    #endregion 타입별 데이터반환 함수

    /// <summary>
    /// 캐미데이터 파인드맵에서 데이터를 찾아 반환한다.
    /// </summary>
    /// <param name="_tgId">버프를 받는 유닛 아이디</param>
    /// <param name="_needId">버프를 받는데 필요한 유닛 아이디</param>
    /// <returns>일치하는 캐미데이터</returns>
    internal ChemistryDataMap GetchemistryDataInFindMap(int _tgId, int _needId)
	{
		long key  = (long)_tgId;
				 key |= (long)_needId << 32;

		if (chemiFindMap.ContainsKey(key))
			return chemiFindMap[key];
		return null;
	}

	/// <summary>
	/// 팀 스킬 데이터를 파인드맵에서 데이터를 찾아 반환한다.
	/// 데이터를 그대로 전달하므로, 절대 쓰기를 해서는 안된다.
	/// </summary>
	/// <param name="_charKey"></param>
	/// <returns>해당 유닛이 속한 팀스킬 데이터 리스트</returns>
	internal List<TeamSkillDataMap> GetTeamSkillDataInFindMap(int _charKey)
	{
		if(teamSkillFindMap.ContainsKey(_charKey))
			return teamSkillFindMap[_charKey];
		return null;
	}

	internal UnitDataMap GetUnitDataByCharID(int _charID)
	{
		var it = ((DataMapCtrl<UnitDataMap>)tables[DataMapType.Unit]).GetEnumerator();
		while(it.MoveNext())
		{
			var data = it.Current.Value;
			if (data.charIdType == _charID)
				return data;
		}

		return null;
	}

    internal StoryDataMap GetStoryDataByIndex(int _chapter, int _stage, int _diff)
    {
        var iter = ((DataMapCtrl<StoryDataMap>)tables[DataMapType.Story]).GetEnumerator();
        while (iter.MoveNext())
        {
            if (iter.Current.Value.chapter == _chapter &&
                iter.Current.Value.difficult == _diff &&
                iter.Current.Value.stage == _stage)
                return iter.Current.Value;
        }

        return null;

    }

    /// <summary>
    /// 세트 가구를 찾기위한 파인드 맵 관련
    /// </summary>
    /// <returns></returns>
    internal Dictionary<int, SetFurnitureDataMap>.Enumerator GetSetFunitureList()
	{
		return ((DataMapCtrl<SetFurnitureDataMap>)tables[DataMapType.SetFurniture]).GetEnumerator();
	}

	internal List<SetFurnitureSetDataMap> GetSetFurnitureSetList(int _groupID)
	{
		if(!setInteriorFindMap.ContainsKey(_groupID))
		{
			Debug.LogError("Invalid Data." + _groupID);
			return null;
		}
		return setInteriorFindMap[_groupID];
	}


	internal List<PvPAdvancementDataMap> GetPvPAdvancementFindMapData(int _grade)
	{
		if (!pvpAdvancementFindMap.ContainsKey(_grade))
			return null;

		return pvpAdvancementFindMap[_grade];
	}

    internal List<int> GetAchieveFindMapData(int _defineKey)
    {
        if(achieveFindMap.ContainsKey(_defineKey))
            return achieveFindMap[_defineKey];
        return null;
    }

    internal List<int> GetOpenStages(int _storyKey)
    {
        if (openStageMap.ContainsKey(_storyKey))
            return openStageMap[_storyKey];
        return null;
    }

    /// <summary>
    /// 모든 캐미데이터 검사를 위해 (특정캐릭터의 캐미데이터들 찾기)
    /// </summary>
    /// <returns></returns>
    internal Dictionary<int, ChemistryDataMap>.Enumerator GetChemistryEnumertor()
    {
        return ((DataMapCtrl<ChemistryDataMap>)tables[DataMapType.Chemistry]).GetEnumerator();
    }

    /// <summary>
    /// 모든 팀스킬데이터 검사를 위해 (특정캐릭터의 팀스킬 데이터들 찾기)
    /// </summary>
    /// <returns></returns>
    internal Dictionary<int, TeamSkillDataMap>.Enumerator GetTeamSkillEnumertor()
    {
        return ((DataMapCtrl<TeamSkillDataMap>)tables[DataMapType.TeamSkill]).GetEnumerator();
    }

    internal int GetMaxStrengthenLevel(int _evol)
    {
        var it = ((DataMapCtrl<StrengthenCostDataMap>)tables[DataMapType.StrengthenCost]).GetEnumerator();
        int maxStrengthLevel = 0;
        while(it.MoveNext())
        {
            if (it.Current.Value.evol != _evol)
                continue;

            maxStrengthLevel = Mathf.Max(maxStrengthLevel, it.Current.Value.strengthen);
        }

        return maxStrengthLevel;
    }

    internal int GetMaxItemStrengthenLevel(int _rank)
    {
        var it = ((DataMapCtrl<ItemStrengthenCostDataMap>)tables[DataMapType.ItemStrengthenCost]).GetEnumerator();
        int maxStrengthLevel = 0;
        while (it.MoveNext())
        {
            if (it.Current.Value.rank != _rank)
                continue;

            maxStrengthLevel = Mathf.Max(maxStrengthLevel, it.Current.Value.strengthen);
        }

        return maxStrengthLevel;
    }


    internal int GetACheckCount()
    {
        return ((DataMapCtrl<ACheckDataMap>)tables[DataMapType.ACheck]).GetCount();
    }

    internal Dictionary<int, ACheckDataMap>.Enumerator GetACheckEnumertor()
    {
        return ((DataMapCtrl<ACheckDataMap>)tables[DataMapType.ACheck]).GetEnumerator();
    }

    internal Dictionary<int, MainBannerDataMap>.Enumerator GetMainBannerEnumertor()
    {
        return ((DataMapCtrl<MainBannerDataMap>)tables[DataMapType.MainBanner]).GetEnumerator();
    }

    internal List<ACheckDataMap> GetACheckList()
    {
        return ((DataMapCtrl<ACheckDataMap>)tables[DataMapType.ACheck]).GetList();
    }

    internal int GetFirstACheckReward(int _aCheckKey)
    {
        var iter = ((DataMapCtrl<ACheckRewardDataMap>)tables[DataMapType.ACheckReward]).GetEnumerator();
        while (iter.MoveNext())
            if (iter.Current.Value.index == _aCheckKey)
                return iter.Current.Value.id;

        Debug.LogError("Not Found ACheck Reward Data. " + _aCheckKey);
        return -1;
    }

    internal int GetLastACheckReward(int _aCheckKey)
    {
        int id = -1;
        var iter = ((DataMapCtrl<ACheckRewardDataMap>)tables[DataMapType.ACheckReward]).GetEnumerator();
        while (iter.MoveNext())
            if (iter.Current.Value.index == _aCheckKey)
                id = iter.Current.Value.id;

        return id;
    }

    internal PushStringDataMap GetRandomPushString(int _type)
    {
        List<PushStringDataMap> list = new List<PushStringDataMap>();

        var iter = ((DataMapCtrl<PushStringDataMap>)tables[DataMapType.PushString]).GetEnumerator();
        while (iter.MoveNext())
            if (iter.Current.Value.type == _type)
                list.Add(iter.Current.Value);

        return list[UnityEngine.Random.Range(0, list.Count - 1)];
    }

    internal int GetMaxLevel()
    {
        return ((DataMapCtrl<ExpDataMap>)tables[DataMapType.Exp]).GetCount();
    }
}
