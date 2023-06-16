using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realtime.LITJson;

public class GamePotSendLogCharacter
{
    public static string NAME = "name";
    public static string PLAYER_ID = "player_id";
    public static string SERVER_ID = "server_id";
    public static string LEVEL = "level";
    public static string USERDATA = "userdata";

    public GamePotSendLogCharacter(){}
    
    public JsonData body = new JsonData();


    public void put(string key, string value)
    {
        body[key] = value;
    }

    public string toString(){return body.ToJson();}
    //public string toString() { return JsonConvert.SerializeObject(body); }

    public void test()
    {
        Debug.Log(body.ToJson());
    }

}