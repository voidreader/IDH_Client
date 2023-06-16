using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class ShopPackageDataMap : CSVParse
{
    internal int type; // 소속 탭 구분
    internal int order; // -1이면 서브탭 없고, 0부터 순차적으로 서브탭 순서
    internal int mailType;
    internal string name;
    internal int[] list; // langth = 4
    internal DateTime startDate;
    internal DateTime endDate;
    internal int limit; // 지급 기간
    internal int bannerID;



    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out order);
        ToParse(_csvData[idx++], out mailType);
        ToParse(_csvData[idx++], out name);

        list = new int[4];
        for(int i = 0; i < list.Length; ++i)
            ToParse(_csvData[idx++], out list[i]);

        string dateStr;
        ToParse(_csvData[idx++], out dateStr);
        DateTime.TryParse(dateStr, out startDate);
        ToParse(_csvData[idx++], out dateStr);
        DateTime.TryParse(dateStr, out endDate);

        ToParse(_csvData[idx++], out limit);
        ToParse(_csvData[idx++], out bannerID);

        return id;
    }
}