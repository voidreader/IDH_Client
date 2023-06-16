using System;
using System.Collections.Generic;


class StrengthenDataMap : CSVParse
{
    internal Stat stats; // 곱해질 스탯

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;

        int strengthenType; // 1 : 강화, 2: 진화 , 3:각성. (클라는 1(강화)만 사용한다.)
        int charType;
        ToParse(_csvData[idx++], out strengthenType);
        ToParse(_csvData[idx++], out charType);

        id = (strengthenType << 8) + charType;

        idx++;

        float[] st = new float[(int)UnitStat.Count];
        for (int i = 1; i < st.Length; i++)
            ToParse(_csvData[idx++], out st[i]);
        stats = new Stat(st, true);


        return id;
    }
}


