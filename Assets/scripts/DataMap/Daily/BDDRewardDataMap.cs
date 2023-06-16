using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class BDDRewardDataMap : CSVParse
{
    internal int itemKey;
    internal int itemCount;

    // 기타 쓰지 않는 데이터(추가 확률등) 로딩 안함

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out itemKey);
        ToParse(_csvData[idx++], out itemCount);
        return id;
    }
}
