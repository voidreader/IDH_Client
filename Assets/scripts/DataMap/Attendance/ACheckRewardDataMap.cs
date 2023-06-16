using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class ACheckRewardDataMap : CSVParse
{
    internal int index;
    internal int day;
    //internal int ID;
    internal int reward;
    internal int rewardValue;
    internal string mailString;

    // 기타 쓰지 않는 데이터(추가 확률등) 로딩 안함

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out index);
        ToParse(_csvData[idx++], out day);
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out reward);
        ToParse(_csvData[idx++], out rewardValue);
        ToParse(_csvData[idx++], out mailString);
        return id;
    }
}
