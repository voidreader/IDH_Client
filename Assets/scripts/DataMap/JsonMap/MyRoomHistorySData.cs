using System;
using System.Collections.Generic;

public class MyRoomHistorySData : JsonParse
{
	public enum MRHType
	{
		PvP = 1,
		Clean = 2,
	}

	public int HISTORY_UID;
	public MRHType TYPE;  // 1 - PvP, 2 - 청소도움

	public string ATTACK_USER_NAME;
	public bool SUCCESS;
	public int REVENGE;

	public DateTime CREATE_TIME;

	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "HISTORY_UID", out HISTORY_UID);
		ToParse(_json, "TYPE", out TYPE);
		ToParse(_json, "ATTACK_USER_NAME", out ATTACK_USER_NAME);
		ToParse(_json, "SUCCESS", out SUCCESS);
		ToParse(_json, "REVENGE", out REVENGE);
		ToParse(_json, "CREATE_TIME", out CREATE_TIME);

		return true;
	}

}
