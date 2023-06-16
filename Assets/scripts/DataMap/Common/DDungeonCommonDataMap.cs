using System;
using System.Collections.Generic;

internal class DDungeonCommonDataMap : CSVParse
{
    internal int dailyDefTicket;        // 요일던전 티켓 기본 개수
    internal int dailyPurchaseCount;    // 요일던전 티켓 추가 구매 가능 횟수
    internal int dailyDefPurchaseCost;  // 요일던전 티켓 추가 구매 기본 비용(Purl)
    internal int dailyAddPurchaseCost;  // 요일던전 티켓 추가 구매 추가 비용(Purl)
    internal int dailyResetTicketTime;  // 요일던전 티켓 리셋 시간 (시 단위)
    internal int dailyEndTime;          // 요일던전 종료 시간 (시 단위)
    internal int dailyStartTime;        // 요일던전 시작 시간 (시 단위)
    internal int dailyLimitTime;        // 요일던전 시작 시간 (시 단위)

    internal override int SetData(string[] _csvData)
    {
        id = 1;
        int idx = 0;
        ToParse(_csvData[idx++], out dailyDefTicket);
        ToParse(_csvData[idx++], out dailyPurchaseCount);
        ToParse(_csvData[idx++], out dailyDefPurchaseCost);
        ToParse(_csvData[idx++], out dailyAddPurchaseCost);
        ToParse(_csvData[idx++], out dailyResetTicketTime);
        ToParse(_csvData[idx++], out dailyEndTime);
     //   ToParse(_csvData[idx++], out dailyStartTime);
     //   ToParse(_csvData[idx++], out dailyLimitTime);

        return 1;
    }
}