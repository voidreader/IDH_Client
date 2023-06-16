using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class StorySData : JsonParse
{
	internal int key; // StoryDataMap ID
    internal int count; //StoryData cnt
    internal bool[] missionClear;
	internal bool clear;	// 한번이라도 깼다면 true

	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "STORY_ID", out key);

		if (missionClear == null) missionClear = new bool[3];
		ToParse(_json, "MISSION1", out missionClear[0]);
		ToParse(_json, "MISSION2", out missionClear[1]);
		ToParse(_json, "MISSION3", out missionClear[2]);
        ToParse(_json, "CNT", out count);
        ToParse(_json, "CLEAR", out clear);

		return true;
	}

	internal int GetMissionClearFlag()
	{
		int result = 0;
		if (missionClear[0]) result |= 1 << 0;
		if (missionClear[1]) result |= 1 << 1;
		if (missionClear[2]) result |= 1 << 2;
		return result;
	}

	internal int GetMossionClearCount()
	{
		int result = 0;
		for(int i = 0; i < missionClear.Length; ++i)
			if (missionClear[i])
				++result;

		return result;
	}
}
