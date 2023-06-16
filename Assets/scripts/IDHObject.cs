using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UserData
{
    public long UserID = -1;                                // 유저ID (-1 = LocalUser)
    public string Name;                                     // 유저이름
    public string Comment;                                  // 자기소개글
    public long MainCharacterUID = 26;                      // 대표 캐릭터
    public int Level;                                       // 레벨
}

public class LocalUserData : UserData
{
    public delegate int CalculateNowExp(int level, int totalExp);
    public delegate int CalculateTableValueByLevel(int level);

    private CalculateNowExp CalculateNowExpMethod;
    private CalculateTableValueByLevel CalculateNowMaxExpMethod;
    private CalculateTableValueByLevel CalculateNowVigorMethod;

    public int TotalExp;                                        //누적 경험치(exp)
    public int MaxExp;                                          // 해당레벨 맥스 경험치
    public int MaxVigor;                                        // 행동력(수정 할수도 있음)(아이템에 현재 행동력 수치 들어가있음)
    public int HeroSlotLimitCount;                              // 캐릭터 소지한도
    public int EquipItemSlotLimitCount;                              // 장비 아이템 소지 한도

    public int DeleteFriendRemainingCount;                      // 남은 친구 삭제 가능 횟수
    public int DailyDungeonRechargeAbleCount;                   // 남은 요일던전 티켓 충전 가능 횟수
    public int RatingOfPvP;                                     // pvp 배치고사 등급 혹은 위치? 관련
    public int RemainingDefeatCountOfPvP;                       // pvp 배치고사 남은 패배 횟수

    public int NowExp { get { return CalculateNowExpMethod.Invoke(Level, TotalExp); } }
    public int NowMaxExp { get { return CalculateNowMaxExpMethod.Invoke(Level); } }
    public int NowMaxVigor { get { return CalculateNowVigorMethod.Invoke(Level); } }

    public LocalUserData(CalculateNowExp nowExpDelegate,
        CalculateTableValueByLevel nowMaxExpDelegate, CalculateTableValueByLevel nowVigorDelegate)
    {
        CalculateNowExpMethod = nowExpDelegate;
        CalculateNowMaxExpMethod = nowMaxExpDelegate;
        CalculateNowVigorMethod = nowVigorDelegate;
    }
}

/// <summary>
/// 0701 미사용 클래스
/// </summary>
public class FriendUserData : UserData
{
    public DateTime LOGIN_DATE;
    public DateTime REQUEST_DATE;
    public DateTime FRIENDSHIP_DATE;
    public int DELEGATE_TEAM_POWER;
    public int FRIENDSHIP_FLAG;

    public int SKILL;
}

/// <summary>
/// 0701 미사용 클래스
/// </summary>
public class PvPUserData : UserData
{
    public int Rating;                            // PvP 등급
    public int GroupInRating;                     // PvP 등급안의 그룹임
    public int BattlePower;                       // 전투력
    public int Score;                             // PvP 점수
    public int WinCount;                          // 승리 수
    public int DefeatCount;                       // 패배 수
    public int WinningStreakCount;                // 연승 수
    public int LastBattleUserUID;                 // 마지막 매칭 유저 UID
    public int ReserachingCount;                  // 재탐색 횟수(자기꺼)
    public bool PlacementMatchGraduationStatus;   // 배치고사 졸업여부 (true = 완료)
    public int RechallengeCount;                  // 재도전 횟수
}

/// <summary>
/// 0701 미사용 클래스
/// </summary>
public class UserInventoryData
{
    public Dictionary<long, HeroSData> HeroList = new Dictionary<long, HeroSData>();
    public Dictionary<long, ItemSData> ItemList = new Dictionary<long, ItemSData>();
}