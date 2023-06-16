using System;
using System.Collections.Generic;

class ShopPackageInfoDataMap : CSVParse
{
    internal string name;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out name);

        return id;
    }
}
