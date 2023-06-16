using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 에셋 번들의 데이터 타입
/// </summary>
internal enum ABType
{
	AB_None,
	AB_Prefab,
	AB_Atlas,
	AB_Texture,
	AB_Audio,
    AB_UnityAtlas,
    AB_BattleStory,
    AB_CSV,
    // Todo : 에셋번들 종류 추가(enum)

}


/// <summary>
/// 파일 로드 경로 데이터
/// </summary>
internal static class FilePath 
{
	// 반드시 ABType과 일치해야한다.
	internal static Dictionary<ABType, string> assetPath;
	internal static Dictionary<DataMapType, string> dataMapPath;


	/// <summary>
	/// 데이터 파일경로 초기화
	/// 반드시 GameCore 초기화시 데이터 파일을 로드하기 전에 호출되어야 한다.
	/// </summary>
	internal static void Init()
	{
		if(assetPath == null)
			InitAssetPath();

		if (dataMapPath == null)
			IniteDataMapPath();


	}


	/// <summary>
	/// Init AssetBundle Load Path
	/// </summary>
	private static void InitAssetPath()
	{
		assetPath = new Dictionary<ABType, string>();

#if UNITY_EDITOR
        assetPath.Add(ABType.AB_Prefab, Path.Combine(Application.streamingAssetsPath, "Windows"));
		assetPath.Add(ABType.AB_Atlas, Path.Combine(Application.streamingAssetsPath, "Windows"));
		assetPath.Add(ABType.AB_Texture, Path.Combine(Application.streamingAssetsPath, "Windows"));
		assetPath.Add(ABType.AB_Audio, Path.Combine(Application.streamingAssetsPath, "Windows"));
        assetPath.Add(ABType.AB_UnityAtlas, Path.Combine(Application.streamingAssetsPath, "Windows"));
        assetPath.Add(ABType.AB_BattleStory, Path.Combine(Application.streamingAssetsPath, "Windows"));
        assetPath.Add(ABType.AB_CSV, Path.Combine(Application.streamingAssetsPath, "Windows"));
#elif TEST_MODE
        assetPath.Add(ABType.AB_Prefab, Path.Combine(Application.streamingAssetsPath, "Bundle"));
		assetPath.Add(ABType.AB_Atlas, Path.Combine(Application.streamingAssetsPath, "Bundle"));
		assetPath.Add(ABType.AB_Texture, Path.Combine(Application.streamingAssetsPath, "Bundle"));
		assetPath.Add(ABType.AB_Audio, Path.Combine(Application.streamingAssetsPath, "Bundle"));
        assetPath.Add(ABType.AB_UnityAtlas, Path.Combine(Application.streamingAssetsPath, "Bundle"));
        assetPath.Add(ABType.AB_BattleStory, Path.Combine(Application.streamingAssetsPath, "Bundle"));
        assetPath.Add(ABType.AB_CSV, Path.Combine(Application.streamingAssetsPath, "Bundle"));
#elif UNITY_ANDROID || UNITY_IOS

		assetPath.Add(ABType.AB_Prefab, Path.Combine(Application.persistentDataPath, "Android"));
		assetPath.Add(ABType.AB_Atlas, Path.Combine(Application.persistentDataPath, "Android"));
		assetPath.Add(ABType.AB_Texture, Path.Combine(Application.persistentDataPath, "Android"));
		assetPath.Add(ABType.AB_Audio, Path.Combine(Application.persistentDataPath, "Android"));
        assetPath.Add(ABType.AB_UnityAtlas, Path.Combine(Application.persistentDataPath, "Android"));
        assetPath.Add(ABType.AB_BattleStory, Path.Combine(Application.persistentDataPath, "Android"));
        assetPath.Add(ABType.AB_CSV, Path.Combine(Application.persistentDataPath, "Android"));
#endif
        // Todo : 에셋번들 종류 추가(path)
    }


    /// <summary>
    /// Init DataMap Load Path
    /// </summary>
    private static void IniteDataMapPath()
	{
		dataMapPath = new Dictionary<DataMapType, string>();
		string path = Path.Combine("Table", "Character");
		dataMapPath.Add(DataMapType.Unit,						Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BCharacter.txt")));
		dataMapPath.Add(DataMapType.Chemistry,			Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BChemistry.txt")));
		dataMapPath.Add(DataMapType.Skill,					Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BSkill.txt")));
		dataMapPath.Add(DataMapType.TeamSkill,			Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "Bteamskill.txt")));       // 임의로 만듦
		dataMapPath.Add(DataMapType.Effect,					Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BEffect.txt")));

		path = Path.Combine("Table", "Common");
		dataMapPath.Add(DataMapType.BattleCommon,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BBattleCommon.txt")));
		dataMapPath.Add(DataMapType.CharacterCommon,    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BCharacterCommon.txt")));	
		dataMapPath.Add(DataMapType.InventoryCommon,    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BInventoryCommon.txt")));	
		dataMapPath.Add(DataMapType.MyRoomCommon,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BMyRoomCommon.txt")));	
		dataMapPath.Add(DataMapType.FarmingCommon,	    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BFarmingCommon.txt")));
		dataMapPath.Add(DataMapType.StoryCommon,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BStoryCommon.txt")));
		dataMapPath.Add(DataMapType.StaminaCommon,	    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BStaminaCommon.txt")));
        dataMapPath.Add(DataMapType.PvECommon,	        Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BPvECommon.txt")));
		dataMapPath.Add(DataMapType.PvPCommon,	        Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BPvPCommon.txt")));
        dataMapPath.Add(DataMapType.FriendCommon,	    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BFriendsCommon.txt")));
        dataMapPath.Add(DataMapType.DDungeonCommon,	    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BDDungeonCommon.txt")));
        dataMapPath.Add(DataMapType.Audio, Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BDDungeonCommon.txt")));

        path = Path.Combine("Table", "Graphic_Data");
		dataMapPath.Add(DataMapType.AssetBundle,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BABLoad.txt")));
		dataMapPath.Add(DataMapType.Sprite,				Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BSprite.txt")));

		path = Path.Combine("Table", "Item");
		dataMapPath.Add(DataMapType.Item,				Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BItem.txt")));
		dataMapPath.Add(DataMapType.ItemEffect,			Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BRuleItemEffect.txt")));
		dataMapPath.Add(DataMapType.SetFurniture,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BSetFurniture.txt")));
		dataMapPath.Add(DataMapType.SetFurnitureSet,    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BSetFurnitureSet.txt")));

		path = Path.Combine("Table", "MyRoom");
		dataMapPath.Add(DataMapType.MyRoom,				Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BMyRoom.txt")));					
		dataMapPath.Add(DataMapType.MyRoomEffect,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BRuleRoomEffect.txt")));	
		dataMapPath.Add(DataMapType.MyRoomRandomBox,    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BMyRoomRandomBox.txt")));	

		path = Path.Combine("Table", "Stage");
		dataMapPath.Add(DataMapType.Stage,				Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BStage.txt")));     

		path = Path.Combine("Table", "Farming");
		dataMapPath.Add(DataMapType.Farming,			Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BFarming.txt")));
		dataMapPath.Add(DataMapType.FarmingReward,	    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BFarmingReward.txt")));

		path = Path.Combine("Table", "Story");
		dataMapPath.Add(DataMapType.StoryChapter,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BChapter.txt")));
		dataMapPath.Add(DataMapType.Story,				Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BStory.txt")));
		dataMapPath.Add(DataMapType.StoryReward,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BStoryReward.txt")));

		path = Path.Combine("Table", "Gacha");
		dataMapPath.Add(DataMapType.Gacha,				Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BGacha.txt")));

		path = Path.Combine("Table", "Making");
		dataMapPath.Add(DataMapType.MakingSlotCost,	    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BMakingSlotCost.txt")));

		path = Path.Combine("Table", "PvP");
		dataMapPath.Add(DataMapType.PvPRateReward,      Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BRateReward.txt")));
		dataMapPath.Add(DataMapType.PvPAdvancement,     Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BAdvancement.txt")));

		path = Path.Combine("Table", "EXP");
		dataMapPath.Add(DataMapType.Exp,                Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BExp.txt")));

		path = Path.Combine("Table", "String");
		dataMapPath.Add(DataMapType.StringCharacter,    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BCharacterString.txt")));
		dataMapPath.Add(DataMapType.StringItem,			Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BItemString.txt")));
		dataMapPath.Add(DataMapType.StringFarming,	    Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BFarmingString.txt")));
		dataMapPath.Add(DataMapType.StringStory,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BStoryString.txt")));
		dataMapPath.Add(DataMapType.StringGacha,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BGachaString.txt")));
        dataMapPath.Add(DataMapType.StringProfile,		Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BChrProfileString.txt")));

        path = Path.Combine("Table", "Daily");
		dataMapPath.Add(DataMapType.DailyDungeon,       Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BDDungeon.txt")));
		dataMapPath.Add(DataMapType.DailyDungeonReward,	Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BDDReward.txt")));

        path = Path.Combine("Table", "Raid");
        dataMapPath.Add(DataMapType.Raid,               Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BRaid.txt")));
        dataMapPath.Add(DataMapType.RaidRankReward,     Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BRRReward.txt")));

        path = Path.Combine("Table", "Mission");
        dataMapPath.Add(DataMapType.Mission,            Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BMissionDefine.txt")));
        dataMapPath.Add(DataMapType.MissionDaily,       Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BDailyMission.txt")));
        dataMapPath.Add(DataMapType.MissionWeekly,      Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BWeeklyMission.txt")));
        dataMapPath.Add(DataMapType.MissionAchieve,     Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BAchieve.txt")));
        dataMapPath.Add(DataMapType.MissionQuest,       Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BQuest.txt")));
        dataMapPath.Add(DataMapType.MissionAccumReward, Path.Combine(Application.streamingAssetsPath, Path.Combine(path, "BAccumReward.txt")));

        // Todo : 데이터맵 종류 추가(path)
    }
}