using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class PushSettingSData : JsonParse
{
    public bool vigor;  // 행동력
    public bool raid;   // 레이드 티켓
    public bool pvp;    // pvp 티켓
    public bool evnt;    // 이벤트
    public bool clean;  // 마이룸 청소
    public bool night;  // 야간 푸시

    internal override bool SetData(JSONObject _json)
    {
        int tmp = 0;
        _json.GetField(ref tmp, "behavior"); vigor = tmp != 0;
        _json.GetField(ref tmp, "raid"); raid = tmp != 0;
        _json.GetField(ref tmp, "pvp"); pvp = tmp != 0;
        _json.GetField(ref tmp, "event"); evnt = tmp != 0;
        _json.GetField(ref tmp, "myroom"); clean = tmp != 0;
        _json.GetField(ref tmp, "night"); night = tmp != 0;

        return true;
    }


    public bool IsNotEquals(PushSettingSData _data)
    {
        return (vigor ^ _data.vigor) |
                (raid ^ _data.raid) |
                (pvp ^ _data.pvp) |
                (evnt ^ _data.evnt) |
                (clean ^ _data.clean) |
                (night ^ _data.night);
    }

    public PushSettingSData Clone()
    {
        var result = new PushSettingSData();

        result.vigor = vigor;
        result.raid = raid;
        result.pvp = pvp;
        result.evnt = evnt;
        result.clean = clean;
        result.night = night;

        return result;
    }
}
