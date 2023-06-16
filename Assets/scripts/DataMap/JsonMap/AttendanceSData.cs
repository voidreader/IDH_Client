using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class AttendanceSData : JsonParse
{
    public int AT; // attendance Type(출첵 종류)
    public int AI; // attendance ID (마지막으로받은 출첵)

    public DateTime RD; // reward Date (마지막으로 출첵한 시간)

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "AT", out AT);
        ToParse(_json, "AI", out AI);

        string rdString = null;
        _json.GetField(ref rdString, "RD");
        if (rdString == null)
            RD = GameCore.nowTime.Date;
        else
            RD = DateTime.Parse(rdString);
        return true;
    }
}
