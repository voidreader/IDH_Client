using System;
using System.Collections.Generic;
class PvPAdvancementDataMap : CSVParse
{
	public int rateType;
	public int rankLimit;
	public int rateAdvance;

	internal override int SetData(string[] _csvData)
	{

		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out rateType);
		ToParse(_csvData[idx++], out rankLimit);
		ToParse(_csvData[idx++], out rateAdvance);

		return id;
	}

    public static string GetStrRank(int _rank)
    {
        switch(_rank)
        {
            case 0: return "브론즈 2";
            case 1: return "브론즈 1";
            case 2: return "실버 2";
            case 3: return "실버 1";
            case 4: return "골드 2";
            case 5: return "골드 1";
            case 6: return "플래티넘";
            case 7: return "마스터";
            case 8: return "챔피언";
            default: return "invalid" + _rank;
        }
    }

    public static string GetStrRankByKey(int _key)
    {
        return GetStrRank(_key - CommonType.DEF_KEY_PVP_RANK);
    }
}
