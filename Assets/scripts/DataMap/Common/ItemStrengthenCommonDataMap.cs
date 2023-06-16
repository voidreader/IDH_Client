using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


internal class ItemStrengthenCommonDataMap : CSVParse
{
    internal float prefixScale; // 접두사 옵션 강화 배율
    internal float optionScale; // 일반 옵션 강화 배율

    internal override int SetData(string[] _csvData)
    {
        id = 1;

        int idx = 0;
        ToParse(_csvData[idx++], out prefixScale);
        ToParse(_csvData[idx++], out optionScale);

        return id;
    }
}
