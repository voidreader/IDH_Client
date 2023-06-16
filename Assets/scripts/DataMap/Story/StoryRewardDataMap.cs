using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class StoryRewardDataMap : CSVParse
{
	internal int[] rewardID;
	internal int[] rewardValue;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		rewardID = new int[4];
		rewardValue = new int[4];

		for( int i = 0; i < 4; ++i)
		{
			ToParse(_csvData[idx++], out rewardID[i]);
			ToParse(_csvData[idx++], out rewardValue[i]);
			idx += 2; // 드랍확률 계산
		}

		return id;
	}
}
