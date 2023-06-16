using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class FarmingRewardDataMap : CSVParse
{
	internal int[] coin;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		coin = new int[5];
		for (int i = 0; i < coin.Length; i++)
			ToParse(_csvData[idx++], out coin[i]);

		return id;
	}
}
