using UnityEngine;
using System.Collections;
using Realtime.LITJson;

public class Response
{
    private string code;
    private string result;

    public string getCode()
    {
        return code;
    }

    public string getResult()
    {
        return result;
    }

    public void setCode(string code)
    {
        this.code = code;
    }

    public void setResult(string result)
    {
        this.result = result;
    }
}