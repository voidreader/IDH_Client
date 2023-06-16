using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FriendSData : JsonParse
{
    public long USER_UID; // UID
    public string USER_NAME; 
    public int DELEGATE_ICON; // 대표아이콘 사진 id값
    public int USER_LEVEL;
    public string COMM; // 자기소개글
    public DateTime LOGIN_DATE; // 최근 로그인 날짜
    public DateTime REQUEST_DATE; // 친구요청한 로그인 날짜 (불확실함)
    public DateTime FRIENDSHIP_DATE; // 우정포인트 받은시간
    public int FRIENDSHIP_FLAG; //이 친구의 우정포인트 수신가능여부(?)(0 없음 1 있음)
    public int DELEGATE_TEAM_POWER; // 대표팀 전투력
    public int SKILL; // 대표 팀 스킬 

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "USER_UID", out USER_UID);
        ToParse(_json, "USER_NAME", out USER_NAME);
        ToParse(_json, "DELEGATE_ICON", out DELEGATE_ICON);
        ToParse(_json, "USER_LEVEL", out USER_LEVEL);
        ToParse(_json, "COMM", out COMM);
        ToParse(_json, "LOGIN_DATE", out LOGIN_DATE);
        ToParse(_json, "REQUEST_DATE", out REQUEST_DATE);
        ToParse(_json, "FRIENDSHIP_DATE", out FRIENDSHIP_DATE);
        ToParse(_json, "DELEGATE_TEAM_POWER", out DELEGATE_TEAM_POWER);
        ToParse(_json, "FRIENDSHIP_FLAG", out FRIENDSHIP_FLAG);

        ToParse(_json, "SKILL", out SKILL);

        return true;
    }
}
