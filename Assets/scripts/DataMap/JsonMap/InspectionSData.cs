using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InspectionSData : JsonParse
{
    public string TITLE;    // 제목
    public string MSG;      // 내용
    public int DELEGATE_ICON; // 대표아이콘 사진 id값
    public DateTime START_DATE; // 점검 시작 시간
    public DateTime END_DATE; // 점검 종료 시간
    public int ACTIVE; // 활성화 여부 (0,1)

    internal override bool SetData(JSONObject _json)
    {
        ToParse(_json, "TITLE", out TITLE);
        ToParse(_json, "MSG", out MSG);
        ToParse(_json, "DELEGATE_ICON", out DELEGATE_ICON);
        ToParse(_json, "START_DATE", out START_DATE);
        ToParse(_json, "END_DATE", out END_DATE);
        ToParse(_json, "ACTIVE", out ACTIVE);

        return true;
    }
}
