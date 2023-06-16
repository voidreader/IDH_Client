
using System;

internal class MissionAccumRewardDataMap : CSVParse
{
    internal MissionType type;         // 사용 위치
    internal int level;         // 사용 순서
    internal int value;         // 깨는데 필요한 개수
    internal int rewardKey;     // 보상 아이템 키
    internal int rewardValue;   // 보상 아이템 개수


    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out level);
        ToParse(_csvData[idx++], out value);
        ToParse(_csvData[idx++], out rewardKey);
        ToParse(_csvData[idx++], out rewardValue);

        return id;
    }
}
