using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class BDDungeonDataMap : CSVParse
{
    internal int[] playableDays; // int[3]
    internal int difficult;
    internal int stageKey;
    internal string name;
    internal int powerRecommend;
    internal string goldMin;
    internal string goldMax;
    internal int clearExp;
    internal int rewardKey;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);

        playableDays = new int[3];
        ToParse(_csvData[idx++], out playableDays[0]);
        ToParse(_csvData[idx++], out playableDays[1]);
        ToParse(_csvData[idx++], out playableDays[2]);

        ToParse(_csvData[idx++], out difficult);
        ToParse(_csvData[idx++], out stageKey);
        ToParse(_csvData[idx++], out name);
        ToParse(_csvData[idx++], out powerRecommend);
        ToParse(_csvData[idx++], out goldMin);
        ToParse(_csvData[idx++], out goldMax);
        ToParse(_csvData[idx++], out clearExp);
        ToParse(_csvData[idx++], out rewardKey);

        return id;
    }
}