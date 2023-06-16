using System;
using System.Collections.Generic;


internal class PvPOppUnitSData : JsonParse
{
	public int charUID;
	public int teamIdx;
	public int position;
	public int skill;
	public long userUID;
	public int charID;
    public int enchant;
    public float[] charStatInfos;
    public int charLevel;
	public int dispatch;
	public int myRoomID;
	public int farmingID;
	public string createDate;

	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "CHA_UID", out charUID);
		ToParse(_json, "TEAM", out teamIdx);
		ToParse(_json, "POSITION", out position);
		ToParse(_json, "SKILL", out skill);
		ToParse(_json, "USER_UID", out userUID);
		ToParse(_json, "CHA_ID", out charID);
        ToParse(_json, "ENCHANT", out enchant);
        ToParse(_json, "STAT", out charStatInfos);
		ToParse(_json, "CHA_LVL", out charLevel);

		ToParse(_json, "DISPATCH", out dispatch);
		ToParse(_json, "MYROOM_ID", out myRoomID);
		ToParse(_json, "FARMING_ID", out farmingID);
		ToParse(_json, "CREATE_DATE", out createDate);

		return true;
	}
}
