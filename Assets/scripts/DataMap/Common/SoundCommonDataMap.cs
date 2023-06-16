using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class SoundCommonDataMap : CSVParse
{
    internal int assetBundleID;
    internal string fileName;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out assetBundleID);
        ToParse(_csvData[idx++], out fileName);
        return id;
    }
}
