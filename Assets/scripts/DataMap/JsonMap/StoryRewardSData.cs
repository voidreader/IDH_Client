using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class StoryChapterSData : JsonParse
{
	internal int key;
	internal int level;  // 보상 수령 정도
	internal bool reward;

	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "CHAPTER_ID", out key);
		ToParse(_json, "LEVEL", out level);
		ToParse(_json, "REWARD", out reward);
		return true;
	}
}
