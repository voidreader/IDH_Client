using System;
using System.Collections.Generic;


class StrengthenCostDataMap : CSVParse
{                               // 영웅 강화시    // 아이템 강화시
    internal int evol;          // 진화단계       // 아이템 등급
    internal int strengthen;    // 강화단계       // 강화 단계
    internal int costExp;       // 필요 exp       // 필요 exp
    internal int costGold;      // 필요 Gold      // 필요 Gold
    internal int costCoin;      // 필요 Coin      // 필요 Coin

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out evol);
        ToParse(_csvData[idx++], out strengthen);
        id = (evol << 8) + strengthen;

        ToParse(_csvData[idx++], out costExp);
        ToParse(_csvData[idx++], out costGold);
        ToParse(_csvData[idx++], out costCoin);

        return id;
    }
}
