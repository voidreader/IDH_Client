using System;
using System.Collections.Generic;

internal class PvPGroupRankSData : JsonParse
{
	public long USER_UID;
	public string USER_NAME;
	public int USER_LEVEL;
	public int DELEGATE_ICON;
	public int POWER;

	public int RANK;
	public int GROUP;
	public int GRADE;
	public int BFGRADE;
	public int POINT;

	public int WIN;
	public int DEFEAT;
	public int CONSECUTIVE;

	public bool PLACEMENT;
	public int RECHALLENGE_COUNT;

	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "USER_UID", out USER_UID);
		ToParse(_json, "USER_NAME", out USER_NAME);
		ToParse(_json, "USER_LEVEL", out USER_LEVEL);
		ToParse(_json, "CHA_ID", out DELEGATE_ICON);
		ToParse(_json, "POWER", out POWER);
		ToParse(_json, "RANK", out RANK);
		ToParse(_json, "GROUP", out GROUP);
		ToParse(_json, "GRADE", out GRADE);
		ToParse(_json, "BFGRADE", out BFGRADE);
		ToParse(_json, "POINT", out POINT);
		ToParse(_json, "WIN", out WIN);
		ToParse(_json, "DEFEAT", out DEFEAT);
		ToParse(_json, "CONSECUTIVE", out CONSECUTIVE);

		ToParse(_json, "PLACEMENT", out PLACEMENT);
		ToParse(_json, "RECHALLENGE_COUNT", out RECHALLENGE_COUNT);

		if (PLACEMENT && RECHALLENGE_COUNT == 10)
			GRADE = 7000009;

		return true;
	}
}
