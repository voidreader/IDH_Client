using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class FriendCommonDataMap : CSVParse
{
    public int defFriendCount;          // 기본 친구 수
    public int AddFriendCountForLevel;  // 레벨당 오르는 친구 수
    public int maxFriendCount;          // 최대 친구 수
    public int maxDeleteCount;          // 하루 최대 친구 삭제 가능 수
    public int requestWaitDay;          // 친구 요청 대기 일 수
    public int recommendCount;          // 추천 친구 표시 개수
    public int recommendLevelValueMin;  // 추천친구 레벨 제한 상수 // 클라이언트 사용 X
    public int recommendLevelValueMax;  // 추천친구 레벨 제한 상수 // 클라이언트 사용 X
    public int printPrepareCount;       // 전투 준비시 표시될 친구 수
    public int printPrepareRcmdCount;   // 전투 준비시 표시될 추천 친구 수
    public int strikerCoolTime;         // 스트라이커 재사용 대기 시간 // 클라이언트 사용 X
    public int friendshipCooltime;      // 우정 보내기 쿨 타임


    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out defFriendCount);
        ToParse(_csvData[idx++], out AddFriendCountForLevel);
        ToParse(_csvData[idx++], out maxFriendCount);
        ToParse(_csvData[idx++], out maxDeleteCount);
        ToParse(_csvData[idx++], out requestWaitDay);
        ToParse(_csvData[idx++], out recommendCount);
        ToParse(_csvData[idx++], out recommendLevelValueMin);
        ToParse(_csvData[idx++], out recommendLevelValueMax);
        ToParse(_csvData[idx++], out printPrepareCount);
        ToParse(_csvData[idx++], out printPrepareRcmdCount);
        ToParse(_csvData[idx++], out strikerCoolTime);
        ToParse(_csvData[idx++], out friendshipCooltime);

        return 1;
    }

    public int CalcMaxFrinedCount(int _level)
    {
        return Math.Min(defFriendCount + (_level / AddFriendCountForLevel), maxFriendCount);
    }
}
