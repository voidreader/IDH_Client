using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class PushStringDataMap : CSVParse
{
    internal int type; // 0 : 행동력, 2: PvP, 3: 청소
    internal string title;
    internal string desc;
    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out title);

        idx += 0;

        ToParse(_csvData[idx++], out desc);
        return id;
    }
}
