using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


internal class NoticeSData : JsonParse
{
    public int TYPE;             // 타입 (1~3). 현재 클라에선 사용하지 않음
    public string CONTENT;       // 공지 내용
    public DateTime START_TIME;  // 공지 시작 시간
    public DateTime END_TIME;    // 공지 종료 시간
    public int INTERVAL;        // 공지 출력 간격(초),  if 0 -> 1회 출력(이미 출력되었다면 ACTIVE를 false로 바꿈)
    public bool ACTIVE;         // 활성화 여부

    public DateTime NextActiveTime;

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "TYPE", out TYPE);
        ToParse(_json, "CONTENT", out CONTENT);
        ToParse(_json, "START_TIME", out START_TIME);
        ToParse(_json, "END_TIME", out END_TIME);
        ToParse(_json, "INTERVAL", out INTERVAL);
        ToParse(_json, "ACTIVE", out ACTIVE);

        UpdateNextActiveTime();

        return true;
    }

    public void UpdateNextActiveTime()
    {
        NextActiveTime = START_TIME;
        var gap = (int)(GameCore.nowTime - START_TIME).TotalSeconds;

        if (INTERVAL == 0)
        {
            if (gap > 0)
                ACTIVE = false;

            return;
        }
        if (gap < 0)
            return;

        gap += INTERVAL - (gap % INTERVAL);
        NextActiveTime = START_TIME.AddSeconds(gap);

        if (END_TIME < NextActiveTime)
            ACTIVE = false;
    }
}