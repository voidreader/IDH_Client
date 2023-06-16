using System;
using System.Collections.Generic;


class ItemStrengthenCostDataMap : CSVParse
{                                   // 아이템 강화시
    internal int rank;                 // 아이템 등급
    internal int strengthen;          // 강화 단계
    internal int costExp;             // 필요 exp
    internal int costGold;           // 필요 Gold

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out rank);
        ToParse(_csvData[idx++], out strengthen);
        id = (rank << 8) + strengthen;

        ToParse(_csvData[idx++], out costExp);
        ToParse(_csvData[idx++], out costGold);

        return id;
    }
}
