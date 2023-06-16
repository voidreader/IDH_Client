using System;
using System.Collections.Generic;
using UnityEngine;
internal class PvPRoomSData : JsonParse
{
	public int MYROOM_ID;
	public Vector2 ANCHOR;
	public int ANGLE;
	public Vector2[] POSITION;
	public int ITEM_ID;
	public int TYPE;

	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "MYROOM_ID", out MYROOM_ID);
		ToParse(_json, "ITEM_ID", out ITEM_ID);
		ToParse(_json, "TYPE", out TYPE);

		if (TYPE < 23)
		{
			ToParse(_json, "ANGLE", out ANGLE);
			ToParse(_json, "ANCHOR", out ANCHOR);
			ToParse(_json, "POSITION", out POSITION);
		}
		return true;
	}

}
