using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class TeamSData : JsonParse
{
	internal long[,] uids;
	internal int[] skills;

	internal TeamSData()
	{
		uids = new long[5, 6];
		skills = new int[5];
		for (int i = 0; i < uids.GetLength(0); i++)
		{
			skills[i] = 0;
			for (int j = 0; j < uids.GetLength(1); j++)
				uids[i, j] = -1L;
		}
	}
	internal override bool SetData(JSONObject _json)
	{
        // Clear
        for (int i = 0; i < uids.GetLength(0); ++i)
        {
            for (int j = 0; j < uids.GetLength(1); ++j)
                uids[i, j] = -1;
            skills[i] = 0;
        } 

		// Set
		for (int i = 0; i < _json.Count; ++i)
		{
			int team, pos, skill;
			long uid;
			ToParse(_json[i], "TEAM", out team);
			ToParse(_json[i], "POSITION", out pos);
			ToParse(_json[i], "CHA_UID", out uid);
			ToParse(_json[i], "SKILL", out skill);
			uids[team-1, pos] = uid;
			skills[team - 1] = skill;
		}

		return true;
	}

	internal void UpdateData(int _team, JSONObject _json)
	{
		for (int i = 0; i < 6; ++i)
			uids[_team-1, i] = -1L;

		for (int i = 0; i < _json.Count; ++i)
		{
			int pos;
			long uid;
			ToParse(_json[i], "POSITION", out pos);
			ToParse(_json[i], "CHA_UID", out uid);

			uids[_team-1, pos] = uid;
		}
	}

	internal void RemoveByUnit(long _uid)
	{
		for (int i = 0; i < uids.GetLength(0); i++)
			for (int j = 0; j < uids.GetLength(1); j++)
				if (uids[i, j] == _uid)
				{
					uids[i, j] = -1;
					return;
				}
	}

	internal bool IsHaveUnit(long _uid)
	{
		for (int i = 0; i < uids.GetLength(0); i++)
			for (int j = 0; j < uids.GetLength(1); j++)
				if (uids[i, j] == _uid)
				{
					return true;
				}
		return false;
	}
}
