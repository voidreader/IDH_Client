using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region EventHandling Defines

/// <summary>
/// 이벤트 데이터의 타입을 구분하는 용도로 사용
/// </summary>
public enum GameEventType : byte
{
	None = 0,	
	MsgAlert,             // 알람창											// MsgAlertPara
	InfoAlert,						// 아이템등 상세정보 팝업			// InfoAlertPara
	AddToBottom,          // Bottom UI에 오브젝트 추가	// AddToBottomPara
	SetReturnCB,          // Return버튼에 콜백 할당			// SetReturnCBPara
	ChangeSys,            // SubSys 전환								// ChangeSysPara(inner Para - 해당 SubSysPara)
	//ChangeSysBack,			// SubSys전환 뒤로 가기


	REQ_ACCOUNT_RECONNECT, // 재연결 시도

	FromServerData,				// Naver Used									// PacketPara

	ANS_ACCOUNT_CREATE,					// 계정 생성
	ANS_ACCOUNT_LOGIN,					// 로그인
	ANS_ACCOUNT_LOGOUT,         // 로그아웃
    ANS_ACCOUNT_KICK,               
    ANS_ACCOUNT_RECONNECT,			// 재연결
	ANS_ACCOUNT_SETCOMDELE,         // 코멘트 및 대표 캐릭 변경
    ANS_ACCOUNT_SETMAINMYROOM,
    ANS_ACCOUNT_DELETE,             // 계정 탈퇴
    ANS_ACCOUNT_LOAD,                 // 서버에 유저데이터가 로드되었을때 오는 대답

    ANS_UPDATE_DEVICE,          // 로그인시 데이터

    ANS_CHARACTER_DELETE,       // 캐릭터 삭제
	ANS_CHARACTER_SALE,         // 캐릭터 판매

    ANS_ITEM_SALE,              // 아이템 판매
    ANS_ITEM_GEN_CHECK,         // 젠형 아이템 확인(타이머가 돌았을때 호출 - 아무떄나 해도 되므로 타이머가 노출되었을때 동작)
    ANS_ITEM_EQUIP,             // 장비 장착
    ANS_ITEM_EQUIP_CHANGE,      // 장비 변경
    ANS_ITEM_UNEQUIP,           // 장비 해제
    ANS_ITEM_STRENGTHEN_EXP,    // 장비 강화(재료넣기)
    ANS_ITEM_STRENGTHEN,        // 장비 강화

    ANS_UPDATE_TEAM,			// 캐릭터 업데이트(팀 데이터)
	ANS_ACCOUNT_CHARSLOT,       // 캐릭터 슬롯 확장
	ANS_ACCOUNT_EQUIPSLOT,			// 장비 아이템 슬롯 확장
	ANS_CURRENT_TIME,           // 현재 서버 시간
	ANS_LEVEL_UP,								// 레벨 업시 받는 데이터
    ANS_TUTORIAL,               // 튜토리얼 진행도


    ANS_FARMING_CREATE,         // 파밍 시작
	ANS_FARMING_GETLIST,        // 파밍 조회
	ANS_FARMING_DONE,			// 파밍 완료
	ANS_FARMING_CANCEL,         // 파밍 취소

	ANS_CHAT,					// 채팅 데이터
    ANS_CHAT_NOTIFICATION,      // 채팅 알림 데이터.
    ANS_CHAT_FILTER,            // 채팅 필터 리스트.

    ANS_GACHA,					// 뽑기
	ANS_MAKE_GETLIST,			// 제조 정보 가져오기
	ANS_MAKE_UNLOCK,			// 제조 슬롯 구매
	ANS_MAKE_START,				// 제조 시작
	ANS_MAKE_QUICK,				// 즉시 제조권 사용
    ANS_MAKE_TAKEALL,           // 제조 완료된 항목 모두 수령
    ANS_MAKE_DONE,				// 제조 완료

	ANS_STORY_GETLIST,			// 스토리 데이터 업데이트
	ANS_STORY_START,            // 스토리 전투 시작
	ANS_STORY_DONE,             // 스토리 전투 종료
	ANS_STORY_REWARD,           // 스토리 챕터 보상

    ANS_MYROOM_Buy,             // 마이룸 언락
    ANS_MYROOM_HISTORY,
	ANS_MYROON_REVENGE,
	ANS_MYROOM_BUILD,
    ANS_MYROOM_ReturnToInventoryObject,
    ANS_MYROOM_GetInfo,         // 마이룸 데이터 가져오기
    ANS_MYROOM_FriendList,
    ANS_MYROOM_FriendRoomDataList,


    ANS_CHARACTER_ALLOCATE,
    ANS_CHARACTER_UNALLOCATE,   // arrange
    ANS_CHARACTER_COMMENT,      // 평가하기 (신규)
    ANS_CHARACTER_COMMENT_EDIT, // 평가하기 (수정)
    ANS_CHARACTER_EVALUATE,     // 평가 (Up / Down) 
    ANS_CHARACTER_EVALUATE_MINE,// 평가 조회 (내것)
    ANS_CHARACTER_EVALUATE_LIST,// 평가 리스트를 가져온다.
    ANS_CHARACTER_POWER,        // 미션/업적용 최대 팀 전투력
    ANS_CHARACTER_STRENGTHEN_EXP,// 강화 재료
    ANS_CHARACTER_STRENGTHEN,   // 강화
    ANS_CHARACTER_EVOLUTION,    // 진화
    ANS_MYROOM_START_CLEAN,
	ANS_MYROOM_FINISH_REVENGE,
    ANS_MYROOM_END_CLEAN,
    ANS_MYROOM_END_CLEAN_ALL,
    ANS_MYROOM_GETBUFFLIST,     // 전투에서 사용할 마이룸 버프 리스트

    ANS_PVP_QUITGRADETEST,
    ANS_PVP_RANKLIST,
    ANS_PVP_CONFIRMLASTSEASON,
    ANS_PVP_TEAMINFO,
    ANS_PVP_GRADETEST,
    ANS_PVP_MATCHLIST,
    ANS_PVP_FINISHPLACEMENT,
    ANS_PVP_STARTBATTLE,
    ANS_PVP_FINISHBATTLE,


    ANS_FRIEND_REQUEST,         // 친구 요청    
    ANS_FRIEND_LIST,            // 친구 목록
    ANS_FRIEND_SEARCH,          // 유저 검색
    ANS_FRIEND_RECOMMENDLIST,   // 추천 친구 목록
    ANS_FRIEND_REQUESTEDLIST,   // 친구 요청 목록
    ANS_FRIEND_ACCEPTABLELIST,  // 친구 수락 목록
    ANS_FRIEND_TEAMINFO,        // 친구 팀 정보
    ANS_FRIEND_ACCEPT_OK,       // 친구 요청 수락
    ANS_FRIEND_ACCEPT_NO,       // 친구 요청 거절
    ANS_FRIEND_REMOVE,          // 친구 삭제
    ANS_FRIEND_SEND,            // 친구 우정 보내기
    ANS_FRIEND_RECEIVE,         // 친구 우정 받기
    ANS_FRIEND_STRIKER,         // 스트라이커 데이터

    ANS_DUNGEON_DAILY_START,    // 요일던전 전투 시작
    ANS_DUNGEON_DAILY_END,      // 요일던전 전투 종료
    ANS_DUNGEON_DAILY_BUY_TICKET,// 요일던전 티킷 구매

    ANS_RAID_PREPARE,           // 레이드 준비(메인화면)
    ANS_RAID_START,             // 레이드 시작
    ANS_RAID_END,               // 레이드 종료
    ANS_RAID_MYRANK,            // 레이드 나의 랭킹
    ANS_RAID_RANK50,            // 레이드 랭킹 탑 50위
    ANS_RAID_TEAM_INFO,         // 레이드 팀 보기

    ANS_MAIL_LIST,              // 우편 조회
    ANS_MAIL_GET,               // 우편 수령

    ANS_MISSION_LIST,           // 미션 데이터
    ANS_MISSION_LIST_CHECK,     //미션 데이터 체크.
    ANS_MISSION_REWARD,         // 보상수령
    ANS_MISSION_REWARD_TOP,    // 달성도 보상 수령

    ANS_NOTIFY_MYROOM_MISSION_COUNT,    //미션보상, 마이룸먼지청소보상 갯수.
    ANS_NOTIFY_FRIEND_MAIL_COUNT,       //친구우정, 메일 갯수.
    ANS_NOTIFY_FRIEND_COUNT,            //친구우정 갯수.
    ANS_NOTIFY_MAIL_COUNT,              //메일 보상 갯수.

    ANS_NOTIFY_INSPECTION,              // 점검 알람
    ANS_NOTIFY_HOTTIME,                 // 핫타임 알람
    ANS_NOTIFY_NOTICE,                  // 공지사항

    NOTIFY_FRIEND,                      // 요청에 의한 응답이 아닌 알람
    NOTIFY_MAIL,                        // 요청에 의한 응답이 아닌 알람

    ANS_SHOP_INQUIRY,                   //상점의 기본정보.
    ANS_SHOP_INQUIRY_ITEM_SKIN,         //상점의 아이템 스킨 정보.
    ANS_SHOP_BUY,                       //상점 구매시 호출.
    ANS_SHOP_TAKE_ITEM,                 //상점 레벨업 패키지 아이템 획득시 호출.

    ANS_PURCHASE,                       // 결제 리시브
    ANS_PURCHASE_COUPON,                // 쿠폰 리시브

    ANS_ATTENDANCE_RECEIVE,             // 출석체크 보상 수령
    ANS_ATTENDANCE_LOOKUP,              // 출석체크 조회

    ANS_PUSH_SAVE,                      // 푸시 설정 저장

    ANS_INITDATA,                       // 부분 유저데이터

    // Todo : 이벤트 타입이 추가(Enum)


}


public enum InitDataType
{
    Character,
    Farming,
    MyRoom,
    Item,
    Gacha,
    Team,
    Story,
    ChapterReward,
    PvP,

    Count // Never Use
}





/// <summary>
/// 이벤트 핸들링 가능한 클래스를 위한 인터페이스
/// </summary>
public interface IEventHandler
{
	/// <summary>
	/// 이벤트 처리
	/// </summary>
	/// <param name="_evt">처리할 이벤트 데이터</param>
	/// <returns>이벤트 처리를 하고 이상이 없다면 true</returns>
	bool HandleMessage(GameEvent _evt);
}


/// <summary>
/// 이벤트 핸드링에 사용될 데이터
/// TokenId - 고유ID값
/// EvtType - 이벤트 종류
/// Para - 이벤트 데이터
/// </summary>
public class GameEvent
{
	private static int m_currentTokenId = 0;
	private readonly int m_tokenId;

	public int TokenId { get { return m_tokenId; } }

	public GameEventType EvtType { get; set; }  // 이벤트 분류
	public ParaBase Para { get; set; }          // 이벤트 데이터(각 타입별로 형변환 하여 사용)

	public GameEvent(GameEventType _type, ParaBase _para)
	{
		EvtType = _type;
		Para = _para;
		m_tokenId = m_currentTokenId++;
	}

	public override string ToString()
	{
		return "[" + m_tokenId +"] " + EvtType.ToString();
	}
}

/// <summary>
/// 이벤트 데이터 Base
/// 모든 이벤트 base는 이 클래스를 상속받아야 한다.
/// </summary>
public class ParaBase
{

	/// <summary>
	/// 데이터를 형변환하여 반환한다.
	/// </summary>
	/// <typeparam name="T">변환될 데이터형. 반드시 ParaBase를 상속받은 클래스여야 한다.</typeparam>
	/// <returns></returns>
	public T GetPara<T>() where T : ParaBase
	{
		return this as T;
	}
}

#endregion EventHandling Defines