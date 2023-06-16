using System;
using System.Collections.Generic;
class FarmingDataMap : CSVParse
{
	internal int chapter;
	internal int stage;
	internal string name;
	internal TimeSpan time;        // 시간값(초)
	internal int powerCondition; 
	internal int conditionType;   
	internal int conditionValue; 
	internal int rewardID; 
	internal int imgID;			
	internal int condStringID;

	// rewards
	//internal int[] ids;
	//internal int[] count;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out chapter);
		ToParse(_csvData[idx++], out stage);
		ToParse(_csvData[idx++], out name);

		int sec = 0;
		ToParse(_csvData[idx++], out sec);
		time = new TimeSpan(0,0, sec);

		ToParse(_csvData[idx++], out powerCondition);
		ToParse(_csvData[idx++], out conditionType);
		ToParse(_csvData[idx++], out conditionValue);
		ToParse(_csvData[idx++], out rewardID);
		ToParse(_csvData[idx++], out imgID);
		ToParse(_csvData[idx++], out condStringID);

		//ids = new int[5];
		//count = new int[5];
		//for(int i = 0; i < 5; ++i)
		//{
		//	ToParse(_csvData[idx++], out ids[i]);
		//	ToParse(_csvData[idx++], out count[i]); 
		//}
		return id;
	}
}
