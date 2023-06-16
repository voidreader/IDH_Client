using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

enum ACheckType
{
    Month,  // 28일 짜리
    Week,   // 7일 짜리

    Count,  // Never Use
}

internal class ACheckDataMap : CSVParse
{
    internal ACheckType type;      // 0 : 28일짜리, 1 : 7일짜리
    internal int order;            // 출력 순서를 위한 지정
    internal string name;
    internal int tabTextureID;
    internal int textureID;

    internal DateTime startDate; // 가용 범위 startDate <= x < endDate
    internal DateTime endDate;

    // 기타 쓰지 않는 데이터(추가 확률등) 로딩 안함

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out order);
        ToParse(_csvData[idx++], out name);
        ToParse(_csvData[idx++], out tabTextureID);
        ToParse(_csvData[idx++], out textureID);

        ToParse(_csvData[idx++], out startDate);
        ToParse(_csvData[idx++], out endDate);
        return id;
    }
}
