using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class FarmingCommonDataMap : CSVParse
{
	internal int MaxCount;

	internal override int SetData(string[] _csvData)
	{
		id = 1;

		int idx = 0;
		ToParse(_csvData[idx++], out MaxCount);

		return id;
	}
}
