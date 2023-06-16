using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePotUnity;

internal class LoadingUI : MonoBehaviour
{
    enum UIType {
        cartoon,
        tip
    }
    [SerializeField] GameObject _loadingIcon;
    [SerializeField] GameObject _loginIcon;

    Action<NCommon.LoginType> cbStart;
    Action<NCommon.LoginType, string> cbCreateAccount;

    public NCommon.LoginType accountType { get; set; }

    CreateAccountScript createAccount;

    [Header("-- Download UI --")]
    [SerializeField] GameObject _downloadRoot;
    [SerializeField] UI2DSprite _downloadBG;
    [SerializeField] UISprite _downloadGauge;
    [SerializeField] UILabel _downloadLabel;

    Sprite[] tips;
    int tipIdx;
    Sprite[] cartoons;
    int cartoonIdx;
    UIType type;

    [Header("-- Platform Login Buttons --")]
    [SerializeField] GameObject ButtonFacebook;
    [SerializeField] GameObject ButtonGoogle;
    [SerializeField] GameObject ButtonGuest;
    [SerializeField] GameObject ButtonApple;



    internal void Init(Action<NCommon.LoginType> _cbStart, Action<NCommon.LoginType, string> _cbCreateAccount)
	{
		cbStart = _cbStart;
        cbCreateAccount = _cbCreateAccount;

        _loadingIcon.SetActive(true);
        _loginIcon.SetActive(false);
        #region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
        // [NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.

        //_loadingIcon = UnityCommonFunc.GetGameObjectByName(gameObject, "loading");
        //_loginIcon = UnityCommonFunc.GetGameObjectByName(gameObject, "login");

        //_downloadRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "downloadRoot");
        //_downloadBG = UnityCommonFunc.GetComponentByName<UI2DSprite>(gameObject, "spDownloadBG");
        //_downloadGauge = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "spDownloadGauge");
        //_downloadLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbDownload");
        #endregion

        UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbVersion").text = GameCore.Instance.GetVersion();

        //GameCore.Instance.PlayerDataMgr.SetDataUnit(null);
        //GameCore.Instance.PlayerDataMgr.AddUnit(new HeroSData(1000001) { uid = 1 });
        //GameCore.Instance.PlayerDataMgr.AddUnit(new HeroSData(1000011) { uid = 2 });
        //GameCore.Instance.PlayerDataMgr.AddUnit(new HeroSData(1000021) { uid = 3 });
        //GameCore.Instance.PlayerDataMgr.AddUnit(new HeroSData(1000031) { uid = 4 });

        type = UIType.cartoon;
    }

	internal void ShowCreateAccountUI()
	{
        GameCore.Instance.CloseMsgWindow();
        createAccount = CreateAccountScript.Create(GameCore.Instance.ui_root);
        createAccount.Init(accountType, cbCreateAccount);
        GameCore.Instance.ShowObject(CSTR.MSG_HEAD_CreateAccount, null, createAccount.gameObject, 0, new MsgAlertBtnData[] {
            new MsgAlertBtnData(CSTR.TEXT_Quit, new EventDelegate(()=> 
            {
                GameCore.Instance.CloseMsgWindow();
                createAccount = null;
            }), true, null, SFX.None),
            new MsgAlertBtnData(CSTR.TEXT_Accept, new EventDelegate(() => 
            {
                createAccount.OnSubmitNameInput();                
            }), true, null, SFX.None)
        });
	}

    internal void SetResponse(string _text)
    {
        if (createAccount != null)
            createAccount.SetResponse(_text);
    }

    public void OnClickLoadingView()
    {
        if (_downloadRoot.activeInHierarchy)
            return;

        if (_loadingIcon.activeInHierarchy == true)
        {
            if (((LoadingSys)GameCore.Instance.SubsysMgr.GetNowSubSys()).TryGamePotAutoLogin())
            {
                // skip login
            }
            else
            {
                _loadingIcon.SetActive(false);

                // 플랫폼 로그인 환경에 따라 다르게 처리 
#if UNITY_ANDROID
                ButtonFacebook.SetActive(true);
                ButtonGoogle.SetActive(true);
                ButtonGuest.SetActive(false);
                ButtonApple.SetActive(false);
#elif UNITY_IOS
                ButtonFacebook.SetActive(true);
                ButtonGoogle.SetActive(true);
                ButtonGuest.SetActive(true);
                ButtonApple.SetActive(true);
#endif

#if UNITY_EDITOR
                ButtonGuest.SetActive(true);
#endif

                _loginIcon.SetActive(true);
            }
        }
    }
	public void OnClickStartGuest()
	{
        accountType = NCommon.LoginType.GUEST;

        GameCore.Instance.ShowAgree(CSTR.MSG_HEAD_GuestLogin, CSTR.MSG_GuestLogin, 0, () =>
        {
            GameCore.Instance.CloseMsgWindow();
#if UNITY_EDITOR
            cbStart(accountType);
#else
            ((LoadingSys)GameCore.Instance.SubsysMgr.GetNowSubSys()).TryLogin(NCommon.LoginType.GUEST);
#endif
        });

    }

    public void OnClickStartGoogle()
    {
        accountType = NCommon.LoginType.GOOGLE;

        ((LoadingSys)GameCore.Instance.SubsysMgr.GetNowSubSys()).TryLogin(NCommon.LoginType.GOOGLE);
    }

    public void OnClickStartFaceBook()
    {
        accountType = NCommon.LoginType.FACEBOOK;
        // Something code
        ((LoadingSys)GameCore.Instance.SubsysMgr.GetNowSubSys()).TryLogin(NCommon.LoginType.FACEBOOK);
    }

    public void OnClickStartApple()
    {
        Debug.Log(">> OnClickStartApple <<");

        accountType = NCommon.LoginType.APPLE;
        // Something code
        ((LoadingSys)GameCore.Instance.SubsysMgr.GetNowSubSys()).TryLogin(NCommon.LoginType.APPLE);
    }

    internal void ShowDownloadPage()
    {
        _downloadRoot.SetActive(true);
        tips = new Sprite[2];
        tips[0] = GameCore.Instance.ResourceMgr.GetLocalObject<Sprite>(CSTR.RSC_LoadingTip1, false);
        tips[1] = GameCore.Instance.ResourceMgr.GetLocalObject<Sprite>(CSTR.RSC_LoadingTip2, false);
        tipIdx = 0;

        // NOTE : 기획문서상에서 존재하는 다운로드 카툰이미지의 개수.
        int count = 29;
        cartoons = new Sprite[count];
        for (int i = 0; i < count; ++i)
            cartoons[i] = GameCore.Instance.ResourceMgr.GetLocalObject<Sprite>(string.Format(CSTR.RSC_LoadingCartoon, i + 1), false);
        
        cartoonIdx = 0;
        _downloadBG.sprite2D = cartoons[cartoonIdx];
    }

    internal void HideDownloadPage()
    {
        _downloadRoot.SetActive(false);
    }

    internal void SetDownloadGauge(float _value, float _volume, float _total)
    {
        //Debug.Log(_value + " / " + _volume + " / " + _total);
        _downloadGauge.fillAmount = _value;
        _downloadLabel.text = string.Format(CSTR.DownloadProgress, _volume / 1000000, _total / 1000000);
    }

    public void OnClickNextCartoon()
    {
        if(type == UIType.cartoon){
            cartoonIdx = (cartoonIdx + 1) % cartoons.Length;
            _downloadBG.sprite2D = cartoons[cartoonIdx];
        } else {
            tipIdx = (tipIdx + 1) % tips.Length;
            _downloadBG.sprite2D = tips[tipIdx];
        }
    }

    public void OnClickPrevCartoon()
    {
        if(type == UIType.cartoon){
            cartoonIdx = (cartoons.Length + cartoonIdx - 1) % cartoons.Length;
            _downloadBG.sprite2D = cartoons[cartoonIdx];
        } else {
            tipIdx = (tips.Length + tipIdx - 1) % tips.Length;
            _downloadBG.sprite2D = tips[tipIdx];
        }
    }

    public void OnClickSkipCartoon()
    {
        type = UIType.tip;
        _downloadBG.sprite2D = tips[tipIdx];
        // UnityCommonFunc.GetGameObjectByName(gameObject, "btDownloadNext").SetActive(false);
        // UnityCommonFunc.GetGameObjectByName(gameObject, "btDownloadPrev").SetActive(false);
        UnityCommonFunc.GetGameObjectByName(gameObject, "SkipBtRoot").SetActive(false);
    }
}
