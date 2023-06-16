using System;

internal class MissionDefineDataMap : CSVParse
{
    internal string text;
    internal string name;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out text);
        ToParse(_csvData[idx++], out name);
        return id;
    }


    public static string GetMissionDiscString(int _key, int _value1 = -1, int _value2 = -1)
    {
        var data = GameCore.Instance.DataMgr.GetMissionDefineData(_key);
        if (data == null)
            return string.Empty;

        switch (_key)
        {
            case CommonType.DEF_KEY_MISSION_DEFINE + 0: return string.Format(data.text, _value1, _value2);  // 스테이지 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 1: return string.Format(data.text, _value1, _value2);  // 클리어시 영웅들의 체력 {0}% 이상으로 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 2: return string.Format(data.text, _value1.ToString("N0"), _value2);  // 총 데미지 {0} 이상으로 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 3: return string.Format(data.text, _value1.ToString("N0"), _value2);  // 오버킬 총합 {0} 이상으로 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 4: return string.Format(data.text, _value1, _value2);  // 아무도 사망하지 않고 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 5: return string.Format(data.text, _value1, _value2);  // 팀스킬을 사용하지 않고 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 6: return string.Format(data.text, _value1.ToString("N0"), _value2);  // 전투력 총합 {0} 이하로 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 7: return string.Format(data.text, CardDataMap.GetStrRank(_value1), _value2);  // {0} 등급 이하의 캐릭터로만 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 8: return string.Format(data.text, _value2, CardDataMap.GetStrType(_value1));  // {1} 유형 캐릭터를 {0}명 포함하여 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 9: return string.Format(data.text, CardDataMap.GetStrType(_value1), _value2);  // {0} 유형 캐릭터를 포함하지 않고 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 10: return string.Format(data.text, _value1, _value2);  // {0}명 이하의 영웅만으로 클리어

            case CommonType.DEF_KEY_MISSION_DEFINE + 11: return string.Format(data.text, _value1.ToString("N0"), _value2); // 스토리모드 {0}회 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 12: return string.Format(data.text, _value1, StoryDataMap.GetStageName(_value2)); // {1} 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 13: return string.Format(data.text, _value1.ToString("N0"), _value2); // 친구 마이룸 {0}회 청소
            case CommonType.DEF_KEY_MISSION_DEFINE + 14: return string.Format(data.text, _value1.ToString("N0"), _value2); // 우정 보내기 {0}회
            case CommonType.DEF_KEY_MISSION_DEFINE + 15: return string.Format(data.text, _value1, _value2); // 추천친구 사용
            case CommonType.DEF_KEY_MISSION_DEFINE + 16: return string.Format(data.text, _value1.ToString("N0"), _value2); // 영웅 강화 {0}회 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 17: return string.Format(data.text, _value1, _value2.ToString("N0")); // ☆{0} 이상 영웅 {1}명 진화 성공
            case CommonType.DEF_KEY_MISSION_DEFINE + 18: return string.Format(data.text, _value1.ToString("N0"), _value2); // 강화 재료로 영웅 카드 {0}장 사용
            case CommonType.DEF_KEY_MISSION_DEFINE + 19: return string.Format(data.text, _value1, _value2.ToString("N0")); // ☆{1} 이상 영웅 {0}명 최대 강화 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 20: return string.Format(data.text, _value1.ToString("N0"), _value2); // 각성 {0}명 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 21: return string.Format(data.text, _value1.ToString("N0"), _value2); // 레이드 {0}회 도전
            case CommonType.DEF_KEY_MISSION_DEFINE + 22: return string.Format(data.text, _value1.ToString("N0"), _value2); // 레이드 {0}회 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 23: return string.Format(data.text, _value1.ToString("N0"), StoryDataMap.GetStrDiffcult(_value2)); // 레이드 {1} {0}회 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 24: return string.Format(data.text, _value1.ToString("N0"), _value2); // 요일던전 {0}회 도전
            case CommonType.DEF_KEY_MISSION_DEFINE + 25: return string.Format(data.text, _value1.ToString("N0"), _value2); // 요일던전 {0}회 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 26: return string.Format(data.text, _value1.ToString("N0"), StoryDataMap.GetStrDiffcult(_value2)); // 요일던전 {1} {0}회 클리어
            case CommonType.DEF_KEY_MISSION_DEFINE + 27: return string.Format(data.text, _value1.ToString("N0"), _value2); // PVP {0}회 도전
            case CommonType.DEF_KEY_MISSION_DEFINE + 28: return string.Format(data.text, _value1.ToString("N0"), _value2); // PVP {0}승 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 29: return string.Format(data.text, _value1, PvPAdvancementDataMap.GetStrRankByKey(_value2)); // PVP {1}등급 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 30: return string.Format(data.text, _value1.ToString("N0"), _value2); // 캐릭터 제조 {0}회 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 31: return string.Format(data.text, _value1.ToString("N0"), _value2); // 아이템 제조 {0}회 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 32: return string.Format(data.text, _value1.ToString("N0"), _value2); // 가구 제조 {0}회
            case CommonType.DEF_KEY_MISSION_DEFINE + 33: return string.Format(data.text, _value1.ToString("N0"), _value2); // 아이템 강화 {0}회
            case CommonType.DEF_KEY_MISSION_DEFINE + 34: return string.Format(data.text, _value1.ToString("N0"), _value2); // 무료 뽑기 {0}회 사용
            case CommonType.DEF_KEY_MISSION_DEFINE + 35: return string.Format(data.text, _value1.ToString("N0"), _value2); // 캐릭터 뽑기 {0}회
            case CommonType.DEF_KEY_MISSION_DEFINE + 36: return string.Format(data.text, _value1.ToString("N0"), _value2); // 가구 뽑기 {0}회 수행
            case CommonType.DEF_KEY_MISSION_DEFINE + 37: return string.Format(data.text, _value1.ToString("N0"), _value2); // 가구 보유랑 {0}개 이상 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 38: return string.Format(data.text, _value1.ToString("N0"), _value2); // 가구 종류 {0}개 이상 획득
            case CommonType.DEF_KEY_MISSION_DEFINE + 39: return string.Format(data.text, _value1.ToString("N0"), _value2); // 아이템 뽑기 {0}회 수행
            case CommonType.DEF_KEY_MISSION_DEFINE + 40: return string.Format(data.text, _value1.ToString("N0"), _value2); // 전투력 총합 {0} 이상 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 41: return string.Format(data.text, _value1.ToString("N0"), _value2); // 캐릭터 {0}명 수집
            case CommonType.DEF_KEY_MISSION_DEFINE + 42: return string.Format(data.text, _value1.ToString("N0"), _value2); // 누적 {0}일 출석
            case CommonType.DEF_KEY_MISSION_DEFINE + 43: return string.Format(data.text, _value1.ToString("N0"), _value2); // 골드 누적 사용 {0}이상
            case CommonType.DEF_KEY_MISSION_DEFINE + 44: return string.Format(data.text, _value1.ToString("N0"), _value2); // 골드 수집 {0} 이상
            case CommonType.DEF_KEY_MISSION_DEFINE + 45: return string.Format(data.text, _value1.ToString("N0"), _value2); // 자동파밍 {0}회 이상 달성
            case CommonType.DEF_KEY_MISSION_DEFINE + 46: return string.Format(data.text, _value1.ToString("N0"), _value2); // 스트라이커 사용 {0}회
            case CommonType.DEF_KEY_MISSION_DEFINE + 47: return string.Format(data.text, _value1.ToString("N0"), _value2); // 상점 이용 {0}회
            case CommonType.DEF_KEY_MISSION_DEFINE + 48: return string.Format(data.text, _value1, _value2); // 골드 상점 이용
            case CommonType.DEF_KEY_MISSION_DEFINE + 49: return string.Format(data.text, _value1, _value2); // 아이템 장착하기
            case CommonType.DEF_KEY_MISSION_DEFINE + 50: return string.Format(data.text, _value1, _value2); // 한 캐릭터의 모든 장비 착용
            case CommonType.DEF_KEY_MISSION_DEFINE + 51: return string.Format(data.text, _value1.ToString("N0"), _value2); // 한 캐릭터의 장비 전투력 합 {0} 이상 달성

            default: return "Invalid";
        }
    }

    public static void MoveRedirect(int _key, int _value1 = -1, int _value2 = -1)
    {
        var data = GameCore.Instance.DataMgr.GetMissionDefineData(_key);
        if (data == null)
            return;

        switch (_key)
        {
            case CommonType.DEF_KEY_MISSION_DEFINE + 11: GameCore.Instance.ChangeSubSystem(SubSysType.Story, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 12: GameCore.Instance.ChangeSubSystem(SubSysType.Story, new StoryPara(_value2, GameCore.Instance.PlayerDataMgr.GetStorySData(_value2) != null)); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 13: GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, new IDH.MyRoom.MyRoomSysParameter() { showFriendList = true }); return; // 마이룸의 친구 팝업
            case CommonType.DEF_KEY_MISSION_DEFINE + 14: GameCore.Instance.CommonSys.ShowLoadingPage(true, ()=> {
                                                             GameCore.Instance.CommonSys.ShowFriendWindow();
                                                             GameCore.Instance.DoWaitCall(2, ()=>GameCore.Instance.CommonSys.ShowLoadingPage(false));
                                                         }); return; // 메인화면 친구 팝업
            case CommonType.DEF_KEY_MISSION_DEFINE + 15: GameCore.Instance.ChangeSubSystem(SubSysType.Story, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 16: 
            case CommonType.DEF_KEY_MISSION_DEFINE + 17:
            case CommonType.DEF_KEY_MISSION_DEFINE + 18:
            case CommonType.DEF_KEY_MISSION_DEFINE + 19:
            case CommonType.DEF_KEY_MISSION_DEFINE + 20: GameCore.Instance.ChangeSubSystem(SubSysType.Inven, new StoryPara(0,false)); return; // 영웅 인벤
            case CommonType.DEF_KEY_MISSION_DEFINE + 21: 
            case CommonType.DEF_KEY_MISSION_DEFINE + 22:
            case CommonType.DEF_KEY_MISSION_DEFINE + 23: GameCore.Instance.ChangeSubSystem(SubSysType.RaidPrepare, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 24: 
            case CommonType.DEF_KEY_MISSION_DEFINE + 25:
            case CommonType.DEF_KEY_MISSION_DEFINE + 26: GameCore.Instance.ChangeSubSystem(SubSysType.DailyPrepare, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 27:
            case CommonType.DEF_KEY_MISSION_DEFINE + 28:
            case CommonType.DEF_KEY_MISSION_DEFINE + 29: GameCore.Instance.ChangeSubSystem(GameCore.Instance.PlayerDataMgr.PvPData.placement ? SubSysType.PvPReady : SubSysType.PvPGradeTest, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 30: GameCore.Instance.ChangeSubSystem(SubSysType.Make, new StoryPara(0, false)); return; // 캐릭터 제조 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 31: GameCore.Instance.ChangeSubSystem(SubSysType.Make, new StoryPara(1, false)); return; // 아이템 제조 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 32: GameCore.Instance.ChangeSubSystem(SubSysType.Make, new StoryPara(2, false)); return; // 가구 제조 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 33: GameCore.Instance.ChangeSubSystem(SubSysType.Inven, new StoryPara(1, false)); return; // 아이템
            case CommonType.DEF_KEY_MISSION_DEFINE + 34: 
            case CommonType.DEF_KEY_MISSION_DEFINE + 35: GameCore.Instance.ChangeSubSystem(SubSysType.Gacha, new StoryPara(0, false)); return; // 캐릭터 뽑기 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 36:
            case CommonType.DEF_KEY_MISSION_DEFINE + 37: 
            case CommonType.DEF_KEY_MISSION_DEFINE + 38: GameCore.Instance.ChangeSubSystem(SubSysType.Gacha, new StoryPara(2, false)); return; // 가구 뽑기 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 39: GameCore.Instance.ChangeSubSystem(SubSysType.Gacha, new StoryPara(1, false)); return; // 장비 뽑기 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 40: GameCore.Instance.ChangeSubSystem(SubSysType.EditTeam, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 41: GameCore.Instance.ChangeSubSystem(SubSysType.Gacha, new StoryPara(0, false)); return; // 캐릭터 뽑기 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 42: GameCore.Instance.ChangeSubSystem(SubSysType.Attendance, null); return; // 출석이벤트 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 43: // 골드 상점 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 44: GameCore.Instance.ChangeSubSystem(SubSysType.Story, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 45: GameCore.Instance.ChangeSubSystem(SubSysType.Farming, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 46: GameCore.Instance.ChangeSubSystem(SubSysType.Story, null); return;
            case CommonType.DEF_KEY_MISSION_DEFINE + 47: // 상점 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 48: // 골드 상점 화면
            case CommonType.DEF_KEY_MISSION_DEFINE + 49: 
            case CommonType.DEF_KEY_MISSION_DEFINE + 50: 
            case CommonType.DEF_KEY_MISSION_DEFINE + 51: GameCore.Instance.ChangeSubSystem(SubSysType.Inven, new StoryPara(0, false)); return; // 영웅 엔벤
            default:
                UnityEngine.Debug.Log("Invalid");
                return;
        }
    }
}
