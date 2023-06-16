using System;
using System.Collections.Generic;

class ShopItemStringDataMap : CSVParse
{
    internal string str;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out str);

        return id;
    }
}
