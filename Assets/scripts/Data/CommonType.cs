using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonType
{
    public static readonly Color32 COLOR_01 = new Color32(0, 240, 255, 255);
    public static readonly Color32 COLOR_02 = new Color32(246,0,255, 255);
    public static readonly Color32 COLOR_03 = new Color32(255,234,0, 255);
    public static readonly Color32 COLOR_04 = new Color32(255,0,0, 255);
    public static readonly Color32 COLOR_05 = new Color32(36,255,0, 255);
    public static readonly Color32 COLOR_06 = new Color32(137,137,137, 255);
    public static readonly Color32 COLOR_07 = new Color32(0,118,192, 255);
    public static readonly Color32 COLOR_08 = new Color32(126,0,255, 255);
    public static readonly Color32 COLOR_09 = new Color32(255,192,0, 255);
    public static readonly Color32 COLOR_10 = new Color32(255,126,0, 255);
    public static readonly Color32 COLOR_11 = new Color32(1, 158, 89, 255);
    public static readonly Color32 COLOR_12 = new Color32(176,176,176, 255);
    public static readonly Color32 COLOR_13 = new Color32(47, 50, 53, 255);
    public static readonly Color32 COLOR_14 = new Color32(0,15,31, 255);


    public const string     BTN_1_NORMAL        = "BTN_01_01_01";
    public const string     BTN_1_ACTIVE        = "BTN_01_01_02";
    public const string     BTN_2_NORMAL        = "BTN_02_01_01";
    public const string     BTN_2_ACTIVE        = "BTN_02_01_02";
    public const string     BTN_3_NORMAL        = "BTN_03_01_01";
    public const string     BTN_3_ACTIVE        = "BTN_03_01_02";
    public const string     BTN_4_NORMAL        = "BTN_04_01_01";
    public const string     BTN_4_ACTIVE        = "BTN_04_01_02";
    public const string     BTN_5_NORMAL        = "BTN_05_01_01";
    public const string     BTN_5_ACTIVE        = "BTN_05_01_02";


    public const string		ANDROID_GAME_ID		= "1782536";
	public const string		IOS_GAME_ID			= "1782537";

	public const string		REWARDED_VIDEO		= "rewardedVideo";
	public const string		SKIPED_VIDEO		= "video";

	public const string		GOOGLE_STORE_URL	= "";
	public const string		APP_STORE_URL		= "";
	public const string		CAFE_URL			= "";
	public const string		PRIVACY_TERMS_URL	= "";
	public const string		SERVICE_TERMS_URL	= "";

	public static int		FIELD_RANGE_DIST	= 2500;
	public static int		FIELD_RANGE_VIEW	= 300;

	// Tag
	public static string	MONSTER_TAG_NAME	= "Monster";
	public static string	PLAYER_TAG_NAME		= "Player";
	public static string	FIELD_OBJ_TAG_NAME	= "FieldObject";
	public static string	OBJECT_TAG_NAME		= "MapObject";

	// Layer
	public static string	LAYER_PLAYER		= "Player";
	public static string	LAYER_ENEMY			= "Enemy";
	public static string	LAYER_UIMODEL		= "UIModel";
	public static string	LAYER_PLAYER_WEAPON	= "Weapon";
	public static string	LAYER_ENEMY_WEAPON	= "EnemyWeapon";

	// Dummy Point
	public static string	POINT_HEAD			= "Dum_Head";
	public static string	POINT_HAND_R		= "Dum_Hand_R";
	public static string	POINT_HAND_L		= "Dum_Hand_L";
	public static string	POINT_ARROW_SHOT	= "Dum_Arrow";
	public static string	POINT_FILED_OBJ_L	= "dum_L";
	public static string	POINT_FILED_OBJ_R	= "dum_R";
	public static string	POINT_ROOT			= "Dum_Root";

	// Animator
	public static string	ANIMATOR_ROOT		= "Animator/";
	public static string	ANIMATOR_BASIC		= "Animator/Basic";
	public static string	ANIMATOR_WORK		= "Animator/Work";
	public static string	ANIMATOR_ONE_HAND	= "Animator/OneHand";
	public static string	ANIMATOR_TWO_HAND	= "Animator/TwoHand";
	public static string	ANIMATOR_DUAL_HAND	= "Animator/DualHand";
	public static string	ANIMATOR_BOW		= "Animator/Bow";
	public static string	ANIMATOR_GUN		= "Animator/Gun";
	public static string	ANIMATOR_DUAL_GUN	= "Animator/DualGun";
	public static string	ANIMATOR_THROW		= "Animator/Throw";

	// Time
	public static float		AI_UPDATE_TIME		= 0.2f;
	public static float		CONNECT_WAIT_TIME	= 0.2f;

	public static string	PATH_UI_ROOT		= "UI/";
	//public static string	PATH_UI_PAGE		= "UI/Page/";
	public static string	PATH_UI_POPUP		= "UI/Popup/";
	public static string	UI_ROOT_NAME		= "UIRoot";
	public static string	UI_ROOT_HUD			= "Hud";
	public static string	UI_ROOT_PAGE		= "Page";
	public static string	UI_ROOT_POPUP		= "Popup";
	public static string	UI_ROOT_SYSTEM		= "System";

	public const string		SETTING_FILE_NAME	= "settingInfo.dat";

	public static string	ASSET_PATH_BODY		= "Assets/AssetBundleResources/Body/";
	public static string	ASSET_PATH_WEAPON	= "Assets/AssetBundleResources/Weapon/";
	public static string	ASSET_PATH_HEAD		= "Assets/AssetBundleResources/Head/";
	public static string	ASSET_PATH_HELMET	= "Assets/AssetBundleResources/Helmet/";
	public static string	ASSET_PATH_TABLE	= "Assets/AssetBundleResources/TextAssets/";
	public static string	ASSET_PATH_TEXTURE	= "Assets/AssetBundleResources/Textures/";
	public static string	ASSET_PATH_UI		= "Assets/AssetBundleResources/UI/";
	public static string	ASSET_PATH_EFFECT	= "Assets/AssetBundleResources/Effects/";
	public static string	ASSET_PATH_PROP		= "Assets/AssetBundleResources/Props/";
	public static string	ASSET_PATH_OBJECT	= "Assets/AssetBundleResources/bg/";
	public static string	ASSET_PATH_SOUND	= "Assets/AssetBundleResources/Sound/";

	public static string	ASSET_BUNDLE_BODY	= "body.unity3d";
	public static string	ASSET_BUNDLE_WEAPON	= "weapon.unity3d";
	public static string	ASSET_BUNDLE_HEAD	= "head.unity3d";
	public static string	ASSET_BUNDLE_HELMET = "helmet.unity3d";
	public static string	ASSET_BUNDLE_SOUND	= "sound.unity3d";
	public static string	ASSET_BUNDLE_UI		= "ui.unity3d";
	public static string	ASSET_BUNDLE_TEX	= "textures.unity3d";
	public static string	ASSET_BUNDLE_EFFECT = "effects.unity3d";
	public static string	ASSET_BUNDLE_TABLE	= "textassets.unity3d";
	public static string	ASSET_BUNDLE_OBJECT = "objects.unity3d";
	public static string	ASSET_BUNDLE_PROP	= "props.unity3d";

	public static int		MAX_BUILDING_LEVEL	= 5;
	public static int		MAX_PARTY_SLOT		= 4;
	public static int		MAX_EQUIP_LEVEL		= 20;
	public static int		MAX_CREW_LEVEL		= 30;
	public static int		MAX_ACCOUNT_LEVEL	= 99;

	public static int		UI_DEPTH_WEAPON		= 300;
	public static int		UI_DEPTH_COSTUME	= 310;
	public static int		UI_DEPTH_HELMET		= 320;
	public static int		UI_DEPTH_ACCESSORY	= 330;
	public static int		UI_DEPTH_MATERIAL	= 340;
	public static int		UI_DEPTH_EXPENDABLE	= 350;
	public static int		UI_DEPTH_SKILL		= 360;
	public static int		UI_DEPTH_CHARACTER	= 400;

	public static int		BASIC_SKILL_INDEX	= 58;

    public static int       TEAM_PVP_IDX = 4;

	public static int SP_TEAMSKILL_EMPTY = 4370001;


    public const int        DEFTABLEKEY_RAIDRANKREWARD = 4800000;

    public const int       UNIT_DEF_KEY = 1000000;
    public const int       ITEM_DEF_KEY = 3000000;

    public const int       ITEMKEY_GOLD = 3000001;
    public const int       ITEMKEY_PURL = 3000002;
    public const int       ITEMKEY_COIN_GUARD = 3000003;
    public const int       ITEMKEY_COIN_NEAR = 3000004;
    public const int       ITEMKEY_COIN_MAGIC = 3000005;
    public const int       ITEMKEY_COIN_SNIP = 3000006;
    public const int       ITEMKEY_COIN_SUPPOT = 3000007;
    public const int       ITEMKEY_TICKET_HERO = 3000008;
    public const int       ITEMKEY_TICKET_PART = 3000009;
    public const int       ITEMKEY_FRIENDSHIP = 3000010;
    public const int       ITEMKEY_TICKET_PVP = 3000011;
    public const int       ITEMKEY_VIGOR = 3000012;
    public const int       ITEMKEY_MILEAGE = 3000013;
    public const int       ITEMKEY_HONER = 3000014;
    public const int       ITEMKEY_TICKET_DAILY = 3000015;
    public const int       ITEMKEY_TICKET_RAID = 3000016;

    public const int       DEF_KEY_DAILY_DUNGEON    = 4000001;

    public const int       DEF_KEY_PVP_RANK         = 7000001;

    public const int       DEF_KEY_MISSION_DEFINE   = 8000001;
    public const int       DEF_KEY_MISSION_DAILY    = 8100001;
    public const int       DEF_KEY_MISSION_WEEKLY   = 8200001;
    public const int       DEF_KEY_MISSION_ACHIEVE  = 8300001;
    public const int       DEF_KEY_MISSION_QUEST    = 8400001;


    // 에셋 번들 키
    public const int       DEF_KEY_SPINE_EFF        = 30001;

    public const int       DEF_KEY_BATTLE_BG        = 99001;

    public const int       DEF_KEY_SFX_UI           = 6000000;
    public const int       DEF_KEY_SFX_BATTLE       = 6000001;
    public const int       DEF_KEY_SFX_SKILL_VOICE_P= 6000002;
    public const int       DEF_KEY_SFX_SKILL_VOICE_T= 6000003;
    public const int       DEF_KEY_SFX_SKILL_P      = 6000004;
    public const int       DEF_KEY_SFX_SKILL_T      = 6000005;
    

    public const int       DEF_KEY_BGM_MAIN         = 6000500;
    public const int       DEF_KEY_BGM_STORY        = 6000501;
    public const int       DEF_KEY_BGM_INGAME       = 6000502;
    public const int       DEF_KEY_BGM_RESULT_W     = 6000503;
    public const int       DEF_KEY_BGM_RESULT_L     = 6000504;
    public const int       DEF_KEY_BGM_EDITTEAM     = 6000505;
    public const int       DEF_KEY_BGM_PVP          = 6000506;
    public const int       DEF_KEY_BGM_MYROOM       = 6000507;
    public const int       DEF_KEY_BGM_CREDIT       = 6000508;



    // PlayerPrefs 키 스트링들
    public const string     PFS_SKIP_GACHAANIM      = "SkipGacahAnim";
    public const string     PFS_SKIP_STORY          = "SkipStory";
}



public static class CSTR
{
    public const string RSC_PanelLoadingUI = "Loading/PanelLoadingUI";
    public const string RSC_PanelLobbyUI = "lobby/PanelLoobyUI";

    public const string RSC_CreateAccount = "Loading/CreateAccount";
    public const string RSC_AdventureUI = "Lobby/AdventureUI";
    public const string RSC_Banner = "Lobby/banner";
    public const string RSC_SpeechBubble = "Lobby/speechBubble";
    public const string RSC_ChatItem_Mine = "Lobby/ChatItemRoot_mine";
    public const string RSC_ChatItem_Other = "Lobby/ChatItemRoot_other";
    public const string RSC_ChatItem_Notification = "Lobby/ChatItemRoot_Notification";
    public const string RSC_FriendListItem = "Lobby/FriendListItem";
    public const string RSC_TeamInfo = "UI/Components/TeamInfo";
    public const string RSC_HotTimePopup = "lobby/HotTimePopup";
    public const string RSC_FarmingContent = "Farming/content";


    public const string RSC_LoadingTip1 = "Loading/TIP_001";
    public const string RSC_LoadingTip2 = "Loading/TIP_002";
    public const string RSC_LoadingCartoon = "Loading/DOWN_{0:d3}";


    public const string PERMMION_Storage = "android.permission.READ_EXTERNAL_STORAGE";



    public const string ERR_0001 = "Can't Login in Editor";
    public const string ERR_0002 = "Denied Permission. Quit Application";
    public const string ERR_0003 = "Always Denied Permission. Quit Application";

    public const string WNG_0001 = "Missing permission to browse device gallery, please grant the permission first";

    public const string TEXT_Unembodiment = "미구현";
    public const string TEXT_Quit = "나가기";
    public const string TEXT_Accept = "확인";
    public const string TEXT_ReqFriend = "친구 신청";
    public const string TEXT_ReqFriend_Refuse = "친구 신청 거절";
    public const string TEXT_RcmFriend = "친구 추천";
    public const string TEXT_ActFriend = "친구 수락";
    public const string TEXT_DelFriend = "친구 삭제";
    public const string TEXT_SnedFriend = "친구 우정 보내기";
    public const string TEXT_RecvFriend = "친구 우정 받기";
    public const string TEXT_Req_Done = "신청 완료";
    public const string TEXT_Send = "보내기";
    public const string TEXT_Delete = "삭제";
    public const string TEXT_Req_Fail = "요청 실패";
    public const string TEXT_TeamInfo = "팀 정보";
    public const string TEXT_LeveUpReward = "레벨업 보상";
    public const string TEXT_MakeUpdate = "제조 정보 업데이트";
    public const string TEXT_FarmingUpdate = "파밍 정보 업데이트";
    public const string TEXT_AttendaceUpdate = "출첵체크 정보 업데이트";

    public const string MSG_HEAD_LoginFail = "Login Failed";
    public const string MSG_HEAD_Quit = "종료";
    public const string MSG_HEAD_CreateAccount = "영웅 생성";
    public const string MSG_HEAD_GuestLogin = "게스트 로그인";
    public const string MSG_HEAD_RemoveFriend = "친구 삭제";
    public const string MSG_HEAD_FindFriend = "친구 찾기";


    public const string MSG_BlockAccount = "Block Account";
    public const string MSG_DeleteAccount = "Delete Account";

    public const string MSG_WrongCode = "알 수 없는 에러 {0}";

    public const string MSG_CreateAccount_Able = "사용 가능한 이름입니다.";
    public const string MSG_CreateAccount_Already = "이미 사용중인 이름입니다.";
    public const string MSG_CreateAccount_Violate = "10자 이상이거나 특수문자 또는 공백이 포함되었습니다.";
    public const string MSG_CreateAccount_Deleted = "삭제 예정 계정의 이름입니다.";
    public const string MSG_CreateAccount_SameUID = "UDID 중복입니다.";
    public const string MSG_KICK = "강제 로그아웃되었습니다.";
    public const string MSG_AskQuit = "정말로 종료하시겠습니까?";
    public const string MSG_GuestLogin = "게스트로 로그인시 게임 내용이 저장되지 않으며,\n로그아웃이나 게임을 삭제 할 구영 정보가 지워질 수 있습니다.\n게스트로 플레이하시겠습니까?";
    public const string MSG_AskDeleteFriend = "정말 친구를 삭제하시겠습니까?\n[c][F600FF](하루 최대 {0}명 삭제 가능)[-][/c]";
    public const string MSG_PleaseWait = "잠시후 다시 시도해주세요.";
    public const string MSG_CantSendFriendship = "우정을 보낼수 있는 친구가 없습니다.";
    public const string MSG_CantReceiveFriendship = "우정을 받을 수 있는 친구가 없습니다.";
    public const string MSG_QuiryErr = "쿼리 오류";
    public const string MSG_EmptyRewFriend = "친구 신청 내역이 없습니다.";
    public const string MSG_EmptySeurchFriend = "조건에 맞는 상대가 없습니다. 아이디를 확인해 주세요.";
    public const string MSG_AlreadyReq = "이미 요청함";
    public const string MSG_AlreadyFriend = "이미 친구";
    public const string MSG_MaxumFriendCount = "최대 친구 초과";
    public const string MSG_MaxumFriendCount_oppo = "상대방의 최대 친구 초과";
    public const string MSG_MaxumDelFriendCount = "하루 친구 삭제 횟수 초과";
    public const string MSG_Change = "변경되었습니다.";
    public const string MSG_Change_Fail = "변경 실패";
    public const string MSG_EmptySlot = "슬롯 정보 없음";


    public const string GUIDE_InputHere = "여기에 입력하세요";


    public const string DownloadProgress = "필요한 파일을 다운로드 중입니다. [c][FFFFFF]{0:0.#}Mb / {1:0.#}Mb[-][/c]";
    public const string DailyChallengeCount = "오늘의 도전 가능 횟수:{0}";
    public const string ChatUniqueNotification = "[F600FF]{0}[-][000000]등급[-] [F600FF]{1}[-][000000]를 획득했습니다.[-]";
    public const string Level = "LV.{0}";

    public const string LastConnetion_Moment = "방금 전";
    public const string LastConnetion_Hour = "{0}시간 전";
    public const string LastConnetion_Day = "{0}일 전";
    public const string Remain_Day = "{0}일 남음";

    public const string Time_HHmmss = "{0:00}:{1:00}:{2:00}";

    public const string FriendCount = "친구 ({0}/{1})";
    public const string SendFriendship = "모두 보내기로 우정 {0}개를 보냈습니다.\n우정 {0}개를 획득하였습니다.";
    public const string ReceiveFriendship = "모두 받기로 우정 {0}개를 획득하였습니다.";

    public const string HotTimeDate = "시작 시간 : {0} [c][8917FD]{1}[-][/c]\n종료 시간 : {2} [c][8917FD]{3}[-][/c]";
    public const string TimeDateYMD = "yyyy년 MM월 dd일";
    public const string TimeDateHm = "HH시 mm분";
    public const string HotTimeDesc = "{0} [FFFF00]{1}%[-]";
    public const string playerInfo = "[24FF00]{0:N0}[-] / {1:N0}\n[24FF00]{2:N0}";

    // 카페 주소
    public const string URL_Cafe = @"https://cafe.naver.com/idhgame";

    // 이용 동의 약관
    public const string OperationPolicy = @"http://ifgame.iwinv.net/notice/2";

    // 개인정보 취급 방침 (모바일 운영정책)
    public const string URL_OperationPolicy = @"http://www.imageframe.kr/ginfo/gi04.html"; //@"http://ifgame.iwinv.net/notice/3";

    // 청약 철회 방침
    public const string URL_SubscriptionWithdrawal = @"http://ifgame.iwinv.net/notice/4";

    // 뽑기홥률 안내
    public const string URL_GachaRate = @"http://ifgame.iwinv.net/notice/5";

    // 제조 확률
    public static string URL_MakeRate = @"http://www.imageframe.kr/gameboard/?mod=document&pageid=1&uid=5";
}