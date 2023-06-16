using System;

internal class RaidRankRewardDataMap : CSVParse
{
    public int diffculty;
    public string text;
    //public int type;    // 리워드 랭킹 산정식에서 사용됨. => 클라이언트에선 사용 X
    public int item;
    public int itemCount;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out diffculty);
        ToParse(_csvData[idx++], out text);
        idx++; // ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out item);
        ToParse(_csvData[idx++], out itemCount);

        return id;
    }
}
