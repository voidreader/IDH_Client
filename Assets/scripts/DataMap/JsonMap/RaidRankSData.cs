using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RaidRankSData : JsonParse
{
    public long USER_UID;
    public int RAID_ID;
    public int DAMAGE;
    public float TAKE_TIME;
    public string USER_NAME;
    public int USER_LEVEL;
    public int DELEGATE_ICON;
    public int RANK;
    public int POWER;

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "USER_UID", out USER_UID);
        ToParse(_json, "RAID_ID", out RAID_ID);
        ToParse(_json, "DAMAGE", out DAMAGE);
        ToParse(_json, "TAKE_TIME", out TAKE_TIME);
        ToParse(_json, "USER_NAME", out USER_NAME);
        ToParse(_json, "USER_LEVEL", out USER_LEVEL);
        ToParse(_json, "DELEGATE_ICON", out DELEGATE_ICON);
        ToParse(_json, "RANK", out RANK);
        ToParse(_json, "POWER", out POWER);

        return true;
    }
}
