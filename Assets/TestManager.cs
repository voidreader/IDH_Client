using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GamePotUnity;

public class TestManager : MonoBehaviour {
    public Text TestText;
    public void InitGamePot(){
        GamePot.initPlugin();
        GamePot.setListener(new GamePotManager());
        TestText.text = "GamePot.initPlugin();GamePot.setListener(new GamePotManager());";
    }
    public void SetGamePotListener(){
        GamePot.setListener(new GamePotManager());
        TestText.text = "GamePot.setListener(new GamePotManager());";
    }
    
    public void LoginGoogle(){
        GamePot.login(NCommon.LoginType.GOOGLE);
        TestText.text = "GamePot.login(NCommon.LoginType.GOOGLE);";
    }
    public void LoginFacebook(){
        GamePot.login(NCommon.LoginType.FACEBOOK);
        TestText.text = "GamePot.login(NCommon.LoginType.FACEBOOK);";
    }
    public void LoginTwitter(){
        GamePot.login(NCommon.LoginType.TWITTER);
        TestText.text = "GamePot.login(NCommon.LoginType.TWITTER);";
    }
    public void ShowNotice(){
        GamePot.showNotice();
        TestText.text = "GamePot.setListener(new GamePotManager());";
    }
    public void ShowCSView(){
        GamePot.showCSWebView();
        TestText.text = "GamePot.showCSWebView();";
    }
    public void SetPushStatusTrue(){
        GamePot.setPushStatus(true);
        TestText.text = "GamePot.setPushStatus(true);";
    }
    public void SetPushStatusFalse(){
        GamePot.setPushStatus(false);
        TestText.text = "GamePot.setPushStatus(false);";
    }
    public void GetPushStatus(){
        NPushInfo pushInfo = GamePot.getPushStatus();
        TestText.text = "pushinfo.enable : " + pushInfo.enable + " pushinfo.night : " + pushInfo.night;
    }
	// Use this for initialization
}
