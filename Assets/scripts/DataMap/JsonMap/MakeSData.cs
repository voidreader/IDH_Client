using System;
using System.Collections.Generic;

internal class MakeSData : JsonParse
{
	internal int key;
	internal int type; // 1 영웅, 2 장비, 3 가구
	internal int state;  // 상태. 0 구매 가능, 1 대여중, 2 구매됨
	internal DateTime period;	// 대여 종료 시간
	internal DateTime makeSTime;  // 제조 시작 시간
	internal DateTime makeETime; // 제조 종료 시간


	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "MAKING_ID", out key);
		ToParse(_json, "TYPE", out type);
		ToParse(_json, "STATUS", out state);
		ToParse(_json, "PERIOD", out period);
		ToParse(_json, "START_TIME", out makeSTime);
		ToParse(_json, "END_TIME", out makeETime);
		//ToParse(_json, "REWARD_TIME_ID", out );
		return true;
	}


	internal TimeSpan GetDelay()
	{
		return makeETime - makeSTime;
	}

	internal bool isActive()
	{
		return state == 0;
	}

	internal bool IsRental()
	{
		return state == 1 && period != default(DateTime);
	}

	internal bool IsMaking()
	{
		return makeETime != default(DateTime);
	}

	internal bool IsMakeDone()
	{
		return IsMaking() && makeETime < GameCore.nowTime;
	}

}
