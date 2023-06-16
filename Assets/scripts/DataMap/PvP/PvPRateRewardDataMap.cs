using System;
using System.Collections.Generic;
internal class PvPRateRewardDataMap : CSVParse
{
	internal string name;
	internal int gold;
	internal int perl;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out gold);
		ToParse(_csvData[idx++], out perl);

		return id;
	}
}
