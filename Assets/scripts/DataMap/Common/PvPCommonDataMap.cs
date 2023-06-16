using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PvPCommonDataMap : CSVParse
{
	internal int rechallengeCount;
	internal int unionCount;
	internal int researchInitVal;
	internal int researchIncVal;
	internal int serverResetInitVal;
    internal int serverResetGradeIncVal;
    internal int winPoint;
    internal int losePoint;
    /*
    더 있지만 클라는 여기까지
     7 Column
    */

    internal int ticketMaxGenCount; // 티켓 자동 생성 한계값
    internal int ticketDefCount;    // 티켓 기본값               Not Using
    internal int ticketGenTime;     // 티켓 자동 생성 소요 시간
    internal DayOfWeek[] resetWeek; // 결투장 리셋 요일들
    internal int pvpEndTime;        // 결투장 종료 시간 (시 단위)
    internal int pvpStartTime;      // 결투장 시작 시간 (시 단위)
    internal int rewardTakableTime; // 보상 수령 기한 (일 단위)
    internal int pvpLimitTime;      // 결투장 시작 시간 (시 단위)

    internal override int SetData(string[] _csvData)
	{
		id = 1;

		int idx = 0;
		ToParse(_csvData[idx++], out rechallengeCount);
		ToParse(_csvData[idx++], out unionCount);
		ToParse(_csvData[idx++], out researchInitVal);
		ToParse(_csvData[idx++], out researchIncVal);

		ToParse(_csvData[idx++], out serverResetInitVal);
		ToParse(_csvData[idx++], out serverResetGradeIncVal);
		ToParse(_csvData[idx++], out winPoint);
		ToParse(_csvData[idx++], out losePoint);

        idx += 7;

        ToParse(_csvData[idx++], out ticketMaxGenCount);
        ToParse(_csvData[idx++], out ticketDefCount);
        ToParse(_csvData[idx++], out ticketGenTime);

        string str;
        ToParse(_csvData[idx++], out str);
        var strs = str.Split('/');
        resetWeek = new DayOfWeek[strs.Length];
        for (int i = 0; i < strs.Length; ++i)
            resetWeek[i] = (DayOfWeek)Convert.ToInt32(strs[i]);

        ToParse(_csvData[idx++], out pvpEndTime);
        ToParse(_csvData[idx++], out pvpStartTime);
        ToParse(_csvData[idx++], out rewardTakableTime);
        ToParse(_csvData[idx++], out pvpLimitTime);

        return id;
	}
}
