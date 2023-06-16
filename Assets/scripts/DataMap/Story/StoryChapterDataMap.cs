using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class StoryChapterDataMap : CSVParse
{
	internal int chapter;
	internal int difficult;
	internal string name;
	internal int discID;
	internal int stamina;
	internal int[] rewardID;
	internal int[] rewardCount;
	internal int imgID;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out chapter);
		ToParse(_csvData[idx++], out difficult);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out discID);
		ToParse(_csvData[idx++], out stamina);

		rewardID = new int[3];
		rewardCount = new int[3];
		for(int i = 0; i < rewardID.Length;++i)
		{
			ToParse(_csvData[idx++], out rewardID[i]);
			ToParse(_csvData[idx++], out rewardCount[i]);
		}

		ToParse(_csvData[idx++], out imgID);

		return id;
	}

	internal static int GenerateChapterKey(int _chap, int _diff)
	{
		var key = 7400000;
		key += (_chap - 1) * 3;
		key += _diff;
		return key;
	}
}
