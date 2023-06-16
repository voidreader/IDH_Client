using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PvECommonDataMap : CSVParse
{
    internal int raidDefTicket;         // 레이드 티켓 기본 개수
    internal int raidEndTime;           // 레이드 종료 시간 (시 단위)
    internal int raidStartTime;         // 레이드 시작 시간 (시 단위)
    internal int raiTicketResetTime;    // 레이드 티켓 리셋 시간 (시 단위)

    // 이하 데이터 생략
    // 3 Column

    internal int rewardTakableTime;     // 보상 수령 기한 (일 단위)

    internal override int SetData(string[] _csvData)
    {
        id = 1;

        int idx = 0;
        ToParse(_csvData[idx++], out raidDefTicket);
        ToParse(_csvData[idx++], out raidEndTime);
        ToParse(_csvData[idx++], out raidStartTime);
        ToParse(_csvData[idx++], out raiTicketResetTime);

        idx += 3;

        ToParse(_csvData[idx++], out rewardTakableTime);

        return id;
    }
}
