using System;

internal class MissionDataMap : CSVParse
{
    internal enum Type
    {
        Story, 
        Mission,
        Achieve,
        Count, // Never Use
    }

    internal int defineKey;
    internal Type type;

    internal int value1;
    internal int value2;

    internal int rewardKey;
    internal int rewardValue;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out defineKey);
        ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out value1);
        ToParse(_csvData[idx++], out value2);
        ToParse(_csvData[idx++], out rewardKey);
        ToParse(_csvData[idx++], out rewardValue);

        return id;
    }
}


internal class AchieveDataMap : MissionDataMap
{
    internal int level;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out defineKey);
        ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out level);
        ToParse(_csvData[idx++], out value1);
        ToParse(_csvData[idx++], out value2);
        ToParse(_csvData[idx++], out rewardKey);
        ToParse(_csvData[idx++], out rewardValue);

        return id;
    }
}