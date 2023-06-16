using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


internal class MainBannerDataMap : CSVParse
{                               //  0             1
    internal int type;          // OpenURL      OpenShop
    internal int atlasKey;
    internal string spriteName;
    internal string value1;     //  URL         ShopTabIndex
    internal int value2;        //  None        ShopSubTabIndex

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out atlasKey);
        ToParse(_csvData[idx++], out spriteName);
        ToParse(_csvData[idx++], out value1);
        ToParse(_csvData[idx++], out value2);

        return id;
    }
}
