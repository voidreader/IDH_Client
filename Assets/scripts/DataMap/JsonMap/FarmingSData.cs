using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class FarmingSData : JsonParse
{
	internal int id;
	internal DateTime startTime;
	internal DateTime endTime;
	//internal int rewardID;

	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "FARMING_ID", out id);
		ToParse(_json, "START_DATE", out startTime);
		ToParse(_json, "END_DATE", out endTime);
		//ToParse(_json, "REWARD_ID", out rewardID);

		return true;
	}
}
