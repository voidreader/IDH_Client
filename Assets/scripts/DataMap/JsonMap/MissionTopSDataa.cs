using System;

public class MissionTopSData : JsonParse
{
    internal int UID;        // 해당 미션 아이디
    internal int VALUE;     // 현재 값
    internal bool REWARD;   // 보상 수령 여부
    internal MissionState state = MissionState.Lock;
    //internal MissionType type;
    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "UID", out UID);
        ToParse(_json, "V", out VALUE);
        ToParse(_json, "R", out REWARD);

        //if (CommonType.DEF_KEY_MISSION_QUEST <= UID)        type = MissionType.Quest;
        //else if (CommonType.DEF_KEY_MISSION_ACHIEVE <= UID) type = MissionType.Achieve;
        //else if (CommonType.DEF_KEY_MISSION_WEEKLY <= UID)  type = MissionType.Weekly;
        //else if (CommonType.DEF_KEY_MISSION_DAILY <= UID)   type = MissionType.Daily;

        if(REWARD)
            state = MissionState.Complete;
        else if (10 <= VALUE)
            state = MissionState.Takable;
        else
            state = MissionState.Running;

        return true;
    }
}


