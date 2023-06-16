using System;

public class MissionSData : JsonParse
{
    internal int UID;        // 해당 미션 아이디
    internal int VALUE;     // 현재 값
    internal int TARGET;    // 목표값
    internal int REWARD;   // 보상 수령 여부
    internal MissionState state = MissionState.Lock;
    internal MissionType type;
    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "UID", out UID);
        ToParse(_json, "V", out VALUE);
        ToParse(_json, "S", out TARGET);
        ToParse(_json, "R", out REWARD);

        var target = 0;
        if (CommonType.DEF_KEY_MISSION_QUEST <= UID)
        {
            target = GameCore.Instance.DataMgr.GetMissionQuestData(UID).value1;
            type = MissionType.Quest;
        }
        else if (CommonType.DEF_KEY_MISSION_ACHIEVE <= UID)
        {
            target = GameCore.Instance.DataMgr.GetMissionAchieveData(UID).value1;
            type = MissionType.Achieve;
        }
        else if (CommonType.DEF_KEY_MISSION_WEEKLY <= UID)
        {
            target = GameCore.Instance.DataMgr.GetMissionWeeklyData(UID).value1;
            type = MissionType.Weekly;
        }
        else if (CommonType.DEF_KEY_MISSION_DAILY <= UID)
        {
            target = GameCore.Instance.DataMgr.GetMissionDailyData(UID).value1;
            type = MissionType.Daily;
        }

        state = (target > VALUE) ?  MissionState.Running :
                (REWARD != 0) ?     MissionState.Complete :
                                    MissionState.Takable;

        return true;
    }

}
