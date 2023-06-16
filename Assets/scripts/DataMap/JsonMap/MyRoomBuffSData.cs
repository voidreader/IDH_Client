using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MyRoomBuffSData : JsonParse
{
    internal int SATIS; // 해당 만족도 수치 (무시)
    internal MyRoomEffectType EFFECT;
    internal float VALUE;
    //internal int rewardID;

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "SATIS", out SATIS);
        ToParse(_json, "EFFECT", out EFFECT);
        ToParse(_json, "VALUE", out VALUE);
        //ToParse(_json, "REWARD_ID", out rewardID);

        return true;
    }
}