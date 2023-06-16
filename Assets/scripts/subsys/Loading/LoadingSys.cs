using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using GamePotUnity;

internal class LoadingPara : ParaBase
{

}

internal class LoadingSys : SubSysBase
{
	LoadingUI ui;
	bool bAnserWait;
	bool AnserWait {
		get
		{
			return bAnserWait;
		}

		set
		{
			bAnserWait = value;
            GameCore.Instance.CommonSys.ShowLoadingIcon(bAnserWait);
		}
	}

    NCommon.LoginType loginType;


	internal LoadingSys() : base(SubSysType.Loading)
	{
        preloadingBundleKeys = null;
    }

	protected override void EnterSysInner(ParaBase _para)
	{
		base.EnterSysInner(_para);
        GameCore.Instance.NewPlayerData();

        AnserWait = false;

		if (ui == null)
		{
			ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_PanelLoadingUI, GameCore.Instance.ui_root).GetComponent<LoadingUI>();
			ui.Init(DoGameStart, DoAccountCreate);
		}

        RequestPermition();
        GameCore.Instance.SoundMgr.PlayMainBackgroundSound();

        GameCore.Instance.SetActiveBlockPanelInvisable(true);
        AssetLoader.Instance.StartDownloadAssetbundle(SetDownLoadComplete, ShowDownloadPage, SetDownloadGauge);
    }

	protected override void RegisterHandler()
	{
		if (handlerMap == null)
		{
			handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
			handlerMap.Add(GameEventType.ANS_ACCOUNT_CREATE, ANS_ACCOUNT_CREATE);
			handlerMap.Add(GameEventType.ANS_ACCOUNT_LOGIN, ANS_ACCOUNT_LOGIN);
            handlerMap.Add(GameEventType.ANS_ACCOUNT_KICK, ANS_ACCOUNT_KICK);
        }

		base.RegisterHandler();
	}

	protected override void ExitSysInner(ParaBase _para)
	{
		base.ExitSysInner(_para);

		if (ui != null )
		{
			GameObject.Destroy(ui.gameObject);
			ui = null;
		}
	}

	internal void DoGameStart(NCommon.LoginType _loginType)
	{
		if (bAnserWait)
			return;

        loginType = _loginType;

        //GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

#if UNITY_EDITOR
        if (GameCore.Instance.bOffline)
		{
			JSONObject data = new JSONObject();
			data.AddField("name", "kachuuu");
			data.AddField("level", 1);
			data.AddField("exp", 0);
			data.AddField("money", 10000);
			data.AddField("cash", 99999);
			data.AddField("character_cnt", 5);

			// 영웅
			JSONObject heros = new JSONObject();
			for(int i = 0; i < 5; ++i)
			{
				JSONObject hero = new JSONObject();
				hero.AddField("CHA_UID", (long)i);
				hero.AddField("CHA_ID", 1000001);
				hero.AddField("DISPATCH", 0);
				hero.AddField("TEAM", i+1);
				// FARMING_ID
				// CREATE_DATE
				heros.Add(hero);
			}
			data.AddField("character", heros);

			// delegate_Icon   ?

			// farming list

			// myroom

			// rule

			// ATYPE : 100

			Debug.Log(data);
			GameCore.Instance.PlayerDataMgr.SetData(data);
			GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, new LobbyPara() { });
			return;
		}
#endif

        bAnserWait = true;
        GameCore.Instance.NetMgr.Req_Account_Login(_loginType);
    }

    void DoAccountCreate(NCommon.LoginType _loginType, string _name)
    {
        if (bAnserWait)
            return;

        loginType = _loginType;

        //GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        bAnserWait = true;
        GameCore.Instance.NetMgr.Req_Account_Create(_name, _loginType);
    }
    public void ShowDownloadPage(){
        if (ui != null)
            ui.ShowDownloadPage();
    }
    public void SetDownloadGauge(float gauge, float volume, long total){
        if (ui != null)
            ui.SetDownloadGauge(gauge, volume, total);
    }
    public void SetDownLoadComplete(){
        if (ui != null)
            ui.HideDownloadPage();

        GameCore.Instance.SetActiveBlockPanelInvisable(false);

#if !UNITY_EDITOR 
        GameCore.Instance.GamePotMgr.cbAcceptPolicy = CBAcceptPolicy;
#endif
    }

    void CBAcceptPolicy()
    {
#if !UNITY_EDITOR
        GameCore.Instance.GamePotMgr.cbAcceptPolicy = null;
#endif
    }

    public bool TryGamePotAutoLogin()
    {
        if (GamePot.getLastLoginType() != NCommon.LoginType.NONE && GameCore.Instance.bLogined)
        {
#if !UNITY_EDITOR
            GameCore.Instance.GamePotMgr.cblogined = CBLogined;
            GameCore.Instance.GamePotMgr.autoLogin();
#endif
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TryLogin(NCommon.LoginType _type)
    {
#if !UNITY_EDITOR
        Debug.Log("Try Login " + _type);
        GameCore.Instance.GamePotMgr.cblogined = CBLogined;
        GameCore.Instance.GamePotMgr.Login(_type);
#else
        if (_type == NCommon.LoginType.NONE)
        {
            bAnserWait = true;
            GameCore.Instance.NetMgr.Req_Account_Login(_type);
        }
        else
        {
            Debug.LogError(CSTR.ERR_0001);
        }
#endif
    }

    void CBLogined()
    {
#if !UNITY_EDITOR
        GameCore.Instance.GamePotMgr.cblogined = null;
#endif
        Debug.Log(">> CBLogined <<");

        ui.accountType = loginType = GamePot.getLastLoginType();
        bAnserWait = true;
        GameCore.Instance.NetMgr.Req_Account_Login(loginType);
    }



    private bool CheckPermissions()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return true;
        }

        return AndroidPermissionsManager.IsPermissionGranted(CSTR.PERMMION_Storage);
    }

    bool quitFlag;
    public void RequestPermition()
    {
        if (!CheckPermissions())
        {
            AndroidPermissionsManager.RequestPermission(new[] { CSTR.PERMMION_Storage }, new AndroidPermissionCallback(
                grantedPermission => {
                    OnBrowseGalleryButtonPress();
                },
                deniedPermission => {
                    Debug.LogError(CSTR.ERR_0002);
                    quitFlag = true;
                },
                deniedPermissionAndDontAskAgain => {
                    Debug.LogError(CSTR.ERR_0003);
                    quitFlag = true;
                    // The permission was denied, and the user has selected "Don't ask again"
                    // Show in-game pop-up message stating that the user can change permissions in Android Application Settings
                    // if he changes his mind (also required by Google Featuring program)
                }
            ));
        }
    }

    public void OnBrowseGalleryButtonPress()
    {
        if (!CheckPermissions())
        {
            Debug.LogWarning(CSTR.WNG_0001);
            return;
        }
    }


    internal bool ANS_ACCOUNT_LOGIN(ParaBase _data)
	{
		bAnserWait = false;
		int result = -1;
		var data = _data.GetPara<PacketPara>().data.data;
		data.GetField(ref result, "result");
        string encryptData = "";
        data.GetField(ref encryptData, "data");
        encryptData = GameNetworkMgr.CustomDecrypt(encryptData);
        data = JSONObject.Create(encryptData);

		Debug.Log("Login Recive : " + result);

		switch (result)
		{
			case 0:
                GameCore.Instance.PlayerDataMgr.LoginType = loginType;
                GameCore.Instance.PlayerDataMgr.SetData(data);
                GameCore.Instance.InitDataMgr();

#if !UNITY_EDITOR
                if (PlayerPrefs.GetInt("AgreeDialog", 0) == 0)
                {
                    GameCore.Instance.GamePotMgr.cbDialog = CBStartLogin;
                    GamePot.showAgreeDialog();
                }
                else
#endif
                {
                    CBStartLogin();
                }

                GameCore.Instance.NetMgr.Req_Current_Time();

                return true;


			case 1: // 계정을 생성해야하는 경우
				ui.ShowCreateAccountUI();
				break;

			case 2: GameCore.Instance.ShowNotice(CSTR.MSG_HEAD_LoginFail, CSTR.MSG_BlockAccount, 0); break;
            case 3: GameCore.Instance.ShowNotice(CSTR.MSG_HEAD_LoginFail, CSTR.MSG_DeleteAccount, 0); break;
			default: GameCore.Instance.ShowNotice(CSTR.MSG_HEAD_LoginFail, string.Format(CSTR.MSG_WrongCode, result), 0); break;
		}

		return false;
	}

	internal bool ANS_ACCOUNT_CREATE(ParaBase _data)
	{
        
		bAnserWait = false;
		int result = -1;
		((PacketPara)_data).data.data.GetField(ref result, "result");
		switch (result)
		{
			case 0:
                ui.SetResponse(CSTR.MSG_CreateAccount_Able);
                GameCore.Instance.CloseMsgWindow();
                bAnserWait = true;
                GameCore.Instance.NetMgr.Req_Account_Login(loginType);

                // 애드브릭스 계정 생성
#if UNITY_ANDROID
                // ADBrix제거
                //AdBrixRmAOS.AdBrixRm.gameCharacterCreated();
#endif

                return true;

			case 1: ui.SetResponse(CSTR.MSG_CreateAccount_Already); break;
			case 2: ui.SetResponse(CSTR.MSG_CreateAccount_Violate); break;
			case 3: ui.SetResponse(CSTR.MSG_CreateAccount_Deleted); break;
			case 4: ui.SetResponse(CSTR.MSG_CreateAccount_SameUID); break;
			default: ui.SetResponse(string.Format(CSTR.MSG_WrongCode, result)); break;
		}

		return false;
	}

    bool ANS_ACCOUNT_KICK(ParaBase _data)
    {
        GameCore.Instance.ShowAlert(CSTR.MSG_KICK);
        GameCore.Instance.LogOut();
        return true;
    }


    internal override void ClickBackButton()
    {
        GameCore.Instance.ShowAgree(CSTR.MSG_HEAD_Quit, CSTR.MSG_AskQuit, 0, () =>
        {
            GameCore.Instance.QuitApplication();
        });
    }

    internal override void UpdateUI()
    {
        base.UpdateUI();

        if (quitFlag)
        {
            quitFlag = false;
            GameCore.Instance.QuitApplication();
        }
    }

    void CBStartLogin()
    {
#if !UNITY_EDITOR
        GameCore.Instance.GamePotMgr.cbDialog = null;
#endif
        GameCore.Instance.CommonSys.ShowLoadingPage(true, () => {
            GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, new LobbyPara() { });
        });
        GameCore.Instance.bLogined = true;

        GameCore.Instance.NetMgr.Req_Update_Device();
        GameCore.Instance.NetMgr.Req_Notify_Friend_Mail_Count();
        #if UNITY_ANDROID
        // 애드브릭스 로그인
        //ADBrix제거
        //AdBrixRmAOS.AdBrixRm.login(GameCore.Instance.PlayerDataMgr.Name);
        #endif
    }
}
