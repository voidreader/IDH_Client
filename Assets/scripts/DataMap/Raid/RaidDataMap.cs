using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class RaidDataMap : CSVParse
{
    internal int group;
    internal int difficult;
    internal int bossKey;
    internal string stageName;
    internal int powerRecommand;
    internal int[] accumDmgs = new int[4];
    internal int[] accumRwds = new int[4];
    internal int[] accumRwdCounts = new int[4];

    // internal int rewardKey; // Not Use


    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out group);
        ToParse(_csvData[idx++], out difficult);
        ToParse(_csvData[idx++], out bossKey);
        ToParse(_csvData[idx++], out stageName);
        ToParse(_csvData[idx++], out powerRecommand);

        for (int i = 0; i < accumDmgs.Length; ++i)
        {
            ToParse(_csvData[idx++], out accumDmgs[i]);
            ToParse(_csvData[idx++], out accumRwds[i]);
            ToParse(_csvData[idx++], out accumRwdCounts[i]);
        }

        return id;
    }
}