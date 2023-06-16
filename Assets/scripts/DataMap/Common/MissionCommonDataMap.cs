using System;
using System.Collections.Generic;

internal class MissionCommonDataMap : CSVParse
{
    internal int rewardReceiveLimit;        // 미션 보상 수령 기한
    internal int rewardReceiveLimit_Tuto;   // 튜토리얼 완료 보상 수령 기한
    internal int resetMissionTime;          // 미션 리셋 시간 (일일/주간)
    internal int ResetMissionWeek;          // 주간미션 리셋 요일(월)

    internal override int SetData(string[] _csvData)
    {
        id = 1;
        int idx = 0;
        ToParse(_csvData[idx++], out rewardReceiveLimit);
        ToParse(_csvData[idx++], out rewardReceiveLimit_Tuto);
        ToParse(_csvData[idx++], out resetMissionTime);
        ToParse(_csvData[idx++], out ResetMissionWeek);

        return 1;
    }
}