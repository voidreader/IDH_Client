using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class StoryCommonDataMap : CSVParse
{
	internal int[] rewardCond;
	internal int maxChapter;

	internal override int SetData(string[] _csvData)
	{
		id = 1;

		int idx = 0;
		rewardCond = new int[3];
		for (int i = 0; i < 3; ++i)
			ToParse(_csvData[idx++], out rewardCond[i]);

		ToParse(_csvData[idx++], out maxChapter);
		return id;
	}
}
