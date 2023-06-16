using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class HeroEvaluateSData : JsonParse
{
    public long UID; // 평가 작성 유저의 UID
    public int typicalKey;
    public string name;
    public string comment;
    public float score;
    public int downCount;
    public int upCount;
    public int imDown; // 플레이어가 이 평가를 다운을 했다면.
    public int imUp;// 플레이어가 이 평가를 업을 했다면.
    public DateTime createTime;

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "USER_UID", out UID);
        ToParse(_json, "USER_NAME", out name);
        ToParse(_json, "CHA_INDEX", out typicalKey);
        ToParse(_json, "COMMENT", out comment);
        ToParse(_json, "SCORE", out score);
        ToParse(_json, "DOWN", out downCount);
        ToParse(_json, "UP", out upCount);
        ToParse(_json, "UPDATE_TIME", out createTime);
        ToParse(_json, "IMUP", out imUp);
        ToParse(_json, "IMDOWN", out imDown);

        comment = GameCore.Instance.ChatMgr.Filtering(comment);

        return true;
    }
}