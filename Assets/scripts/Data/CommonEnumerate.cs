using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EGPSEventType
{
	None = 0,
	Success,            // 권한 획득
	Faided,             // 권한 실패
	RequestPermission,  // 권한 필요
	Cancel,             // 권한 취소
	TimeOut,            // 권한 획득 시간 초과
	Unsupport,          // 지원하지 않음
	Disabled,           // GPS OFF
	Enabled,            // GPS ON
	StartLocation,      // 위치 전송 시작
	UpdateLocation,     // 위치 정보 갱신
	EndLocation         // 위치 전송 종료
}

public enum EBundleState
{
	NONE = 0,
	READY,
	DOWNLOAD,
	DOWNLOAD_COMPLETE,
	LOADING,
	LOADING_COMPLETE,
	DOWNLOAD_ERROR,
	LOADING_ERROR
}

public enum ECameraIndex
{
	UIPage = 0,
	Popup,
	Ingame,
	End
}

public enum ECameraType
{
	Follow = 0,
	Free,
	LookAt,
	Forward,
	FollowTarget,
	End
}

public enum ESceneType
{
	Intro = 0,
	Login,
	Opening,
    Lobby,
    MyRoom,
	End
}

public enum EUIType
{
	Page = 0,
	Popup,
	Components,
}

public enum EUIPage
{
	UIJoystickMain = 0,

	UIBlackMarket,
	UICharacter,
	UIEquipment,
	UIEquipSkill,
	UIExploration,
	UIInventory,
	UIItemMaking,
	UIItemUpgrade,
	UILBSMain,
	UIOpening,
	UIParty,
	UIPlaySummon,
	UIShop,
	UISkillUpgrade,

	UIBattle,
	UIGameMining,
	UIGameFelling,
	UIGameCollect,
	UIGameFishing,
	UIGameHunting,
	UIGameResult,

	// myroom
	UIMyRoomMain,
    UITownShop,
    UITownInfo,
    UITownShopInfo,
    UITownOpenArea,
    UITownNotice,

    // pvp
    UIPvPReady,
    UIPvPResult,
    UIPvPGradeTest,
    UIPvPMatch,


    End
}

public enum EUIPopup
{
	UIPopupCharStat = 0,
	UIPopupItemInfo,
	UIPopupItemDelete,
	UIPopupInvenItem,
	UIPopupItemUse,
	UIPopupExploreResult,
	UIPopupLevelUp,
	UIPopupPostBox,
	UIPopupCardSerial,
	UIPopupOption,
	UIPopupPurchase,
	UIPopupOKCancel,
	UIPopupToastMsg,

	UILoading,
	UIPopupConnect,
	End
}

public enum EUIComponents
{
	PageTitle = 0,
	CrewSlot,
	DragCrewSlot,

	ItemWeaponSlot,
	ItemCostumeSlot,
	ItemHelmetSlot,
	ItemAccessorySlot,
	ItemMaterialSlot,
	ItemExpenableSlot,
	ItemSkillSlot,
	ItemPieceSlot,
	TextSlot,

	MailSlot,
	MarketGoodsSlot,

	CrewStatSlot,
	CardSlot,

	ShopSlot,

	IconDot,

	BattlePartySlot,

	// HUD
	NameTag,
	HUDHP,
	HUDDamage,
	HUDEnemyDamage,
	HUDActPoint,

    // Town
    TownStatusIcon,
    TownBalloonText,
    TownCrewClickOK,

    // MyRoom
    StainComponent,

    End
}

public enum EResourceType
{
	None = 0,
	Body,
	Head,
	Helmet,
	Weapon,
	Object,
	Prop,
	Mobs,
	Map,
	Effect,
	Sound,
}

public enum EEntityType
{
	Player = 0,
	NonPC,
	Monster,
	Building,
	Object,
	END
}

public enum EEntityMotionType
{
	None = 0,
	Grabbed,
	OutTown,
	InTown,
	GoStart,
	Hurray,
	Find
}

public enum EFieldObjectType
{
	None = 0,
	WayPoint,
	EventPortal,

	Tree = 10,
	Mineral,
	Bush,
	FishingSpot,
	Animal,

	Monster = 20,
	BlackMarket = 30,
}

public enum EMailRewardType	{ None = 0, Goods, Item }

public enum EItemType
{
	None = 0,
	Material = 10,		// 재료
	Expendable = 20,	// 소모품
	Equipment = 30,		// 장비
	Skill = 40,			// 스킬 보석
	Recipe = 50,		// 레시피
	Town = 60,			// 상가
	TownItem = 70,		// 상가 상품
	CharPiece = 80,		// 캐릭터 조각
}

public enum EItemGrade
{
	None = 0,
	Common,
	UnCommon,
	Rare,
	Master
}

public enum EItemSort
{
	None = 0, Name = 1, Level, Grade, SubType,
}

public enum EExpendItemType
{
	None = 0,
	RecoveryShock = 10,
	RecoveryShock_All,
	Heal,
	Heal_All,
}

public enum EExpendActType
{
	All = 0,
	Battle,
	NonBattle,
	Field,
	Town,
}
public enum EExpendSubType
{
	SlotItem = 1,
	NonSlotItem,
}

public enum EEquipType
{
	None = 0,
	Weapon,
	Costume,
	Helmet,
	Accessory,
	Skill
}

public enum EEquipIndex
{
	Weapon = 0,
	Costume,
	Helmet,
	Accessory,
	Max
}

public enum EWeaponType
{
	None = 0,
	OneHand,
	DualHand,
	TwoHand,
	Bow,
	Gun,
	DualGun,
	Throw
}

public enum ERecipeType
{
	None = 0,
	Equip,
	Building,
	Research
}

public enum EBuildingType
{
	None = 0,
	Shop,
	Ground,
	Road,
	Environment
}

public enum EStatus
{
	None = 0,
	Offense,
	Defense,
	HP,
	CriDamage,
	CriRate,

	OffSpeed,
	MovSpeed,
	CoolTime,

	AddActPoint,
	Clean,
	Talk,
	Knowledge,
	Passion,
	Spirit,
	Talent,
	Eye,

	FieldSpeed,
	FieldObjCount,

	Fell_TimeUp,
	Mine_TimeUp,
	Collect_TimeUp,
	Fish_TimeUp,
	Hunt_TimeUp,

	Fell_Mastery,
	Mine_Mastery,
	Collect_Mastery,
	Fish_Mastery,
	Hunt_Mastery,

	Fell_Speed,
	Mine_Speed,
	Collect_Speed,
	Fish_Speed,
	Hunt_Speed,

	AdvTimeDown,
	AdvAmountUp,

	Town_Env,
	Town_Service,
	Town_Quality,
	Town_Popularity,
	Town_Price,
	Town_MakeTime,

	MAX
}

public enum EStatusCategory
{
	None = 0,
	Base,
	Job,
	Offense,
	Defense,
	Facility,
	Account
}

public enum EStatusUseType
{
	None = 0,
	Battle,
	Living,
	Facility,
	User
}

public enum EEnemyGrade { Normal = 1, Named, Boss, RaidBoss }

public enum EGoodsType { Coin = 0, Cocoa, ActPoint, Cash, Watch, End }

public enum EShopCategory { None = 0, Coin = 1, Cocoa, ActPoint, Package }

public enum ESoundID // Sound File Name
{
    // BGM
    None = 0,
    BGM_Opening,
    BGM_MyRoom,
    BGM_Battle,


    // SFX
    sd_ef_001,
    sd_ef_002,
    sd_ef_003,
    sd_ef_004,
    sd_ef_005,
    sd_ef_006,
    sd_ef_007,
    sd_ef_008,
    sd_ef_009,
    sd_ef_010,
    sd_ef_011,
    sd_ef_012,
    sd_ef_013,
    sd_ef_014,
    sd_ef_015,
    sd_ef_016,
    sd_ef_017,
    sd_ef_018,
    sd_ef_019,
    sd_ef_020,
    sd_ef_021,
    sd_ef_022,
    sd_ef_023,
    sd_ef_024,
    sd_ef_025,
    sd_ef_026,
    sd_ef_027,
    sd_ef_028,
    sd_ef_029,
    sd_ef_030,
    sd_ef_031,
    sd_ef_032,
    sd_ef_033,
    sd_ef_034,
    sd_ef_035,
    sd_ef_036,
    sd_ef_037,
    sd_ef_038,
    END
}

public struct stRank
{
    public int level;
    public string name;
    public int icon;
    public int rank;
    public int point;
    public int power;
}

public struct stPvPGrade
{
    public string name;
    public int icon;
}

public struct stPVPChar
{
    public int charUID;
    public int teamIdx;
    public int position;
    public int skill;
    public int userUID;
    public int charID;
    public int charLevel;
    public int dispatch;
    public int myRoomID;
    public int farmingID;
    public string createDate;
    
}

public struct stRoomBaseItem
{
    public int itemID;
    public int itemType;
    public int ingameTex;
}
