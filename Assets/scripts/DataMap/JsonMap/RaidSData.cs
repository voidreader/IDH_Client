
public class RaidSData : JsonParse
{
    internal int key; // raid Key
    internal int damage;
    internal float take_Time;

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "RAID_ID", out key);
        ToParse(_json, "DAMAGE", out damage);
        ToParse(_json, "TAKE_TIME", out take_Time);

        return true;
    }
}
