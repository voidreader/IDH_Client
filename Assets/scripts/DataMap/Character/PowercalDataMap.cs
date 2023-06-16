using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PowercalDataMap : CSVParse
{
    internal float[] PowercalStats;

        
    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        PowercalStats = new float[9];
        for(int i = 0; i < PowercalStats.Length; i ++)
        {
            ToParse(_csvData[idx++], out PowercalStats[i]);
        }

        return id;
    }
}
