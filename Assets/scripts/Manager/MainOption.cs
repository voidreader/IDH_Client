using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using GamePotUnity;

public class MainOption : MonoBehaviour, IEventHandler
{
    public PushSettingSData pushSetting = null;

    public GameObject SoundOption;

    public UISprite spTabOption;
    public UISprite spTabInfo;
    public GameObject goOption;
    public GameObject goInfo;

    public UIInput ipCoupon;
    public UISprite spToggleVigor;
    public UISprite spToggleRaid;
    public UISprite spTogglePvP;
    public UISprite spToggleEvent;
    public UISprite spToggleClean;
    public UISprite spToggleNight;

    public UILabel lbVersion;

    public UISprite spToggleSkipGacha;
    public UISprite spToggleSkipStory;

    bool skipGachaAnim;
    bool skipStory;

    public void Init()
    {
        GameCore.Instance.SoundMgr.SetVolumeSlider(ref SoundOption);

        pushSetting = GameCore.Instance.PlayerDataMgr.pushSetting.Clone();

        spToggleVigor.spriteName = pushSetting.vigor ?  "BTN_ON" : "BTN_OFF";
        //spToggleRaid.spriteName = pushSetting.raid ?    "BTN_ON" : "BTN_OFF";
        spTogglePvP.spriteName = pushSetting.pvp ?      "BTN_ON" : "BTN_OFF";
        spToggleEvent.spriteName = pushSetting.evnt ?   "BTN_ON" : "BTN_OFF";
        spToggleClean.spriteName = pushSetting.clean ?  "BTN_ON" : "BTN_OFF";
        spToggleNight.spriteName = pushSetting.night?   "BTN_ON" : "BTN_OFF";

        lbVersion.text = string.Format("Ver.{0}", Application.version);

        skipGachaAnim = PlayerPrefs.GetInt(CommonType.PFS_SKIP_GACHAANIM, 1) != 0;
        skipStory = PlayerPrefs.GetInt(CommonType.PFS_SKIP_STORY, 1) != 0;

        spToggleSkipGacha.spriteName = skipGachaAnim ?  "BTN_ON" : "BTN_OFF";
        spToggleSkipStory.spriteName = skipStory ?      "BTN_ON" : "BTN_OFF";
    }


    public void OnEnable()
    {
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_PURCHASE_COUPON);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_ACCOUNT_DELETE);
    }


    public void OnDisable()
    {
        GameCore.Instance.EventMgr.UnregisterHandler(this);
    }

    public bool HandleMessage(GameEvent _evt)
    {
        var json = ((PacketPara)_evt.Para).data.data;
        int result = -1;
        json.GetField(ref result, "result");

        switch (_evt.EvtType)
        {
            case GameEventType.ANS_PURCHASE_COUPON: return ANS_PURCHASE_COUPON(json, result);
            case GameEventType.ANS_ACCOUNT_DELETE: return ANS_ACCOUNT_DELETE(json, result);
        }

        return true;
    }


    bool ANS_PURCHASE_COUPON(JSONObject _json, int result)
    {
        switch (result)
        {
            case 0:
                var jsonList = _json.GetField("REWARD");
                ItemSData[] items = new ItemSData[jsonList.Count];
                for (int i = 0; i < jsonList.Count; ++i)
                {
                    var key = 0;
                    var cnt = 0;
                    jsonList[i].GetField(ref key, "REWARD_ITEM_ID");
                    jsonList[i].GetField(ref cnt, "REWARD_ITEM_COUNT");
                    items[i] = new ItemSData();
                }
                GameCore.Instance.ShowReceiveItemPopup("획득", items);
                break;

            default:
                GameCore.Instance.ShowNotice("실패", "쿠폰 아이템 받기 실패. " + result, 0);
                break;
        }

        return true;
    }

    bool ANS_ACCOUNT_DELETE(JSONObject _json, int result)
    {
        switch (result)
        {
            case 0:
                GameCore.Instance.NetMgr.DoReconnect();
                GameCore.Instance.bLogined = false;
                GameCore.Instance.CloseMsgWindow();
                GameCore.Instance.ChangeSubSystem(SubSysType.Loading, null);
                break;
            default:
                GameCore.Instance.ShowNotice("실패", "계정 탈퇴 실패. " + result, 0);
                break;
        }

        return true;
    }

    public void OnToggleVigor()
    {
        pushSetting.vigor = !pushSetting.vigor;
        spToggleVigor.spriteName = pushSetting.vigor ? "BTN_ON" : "BTN_OFF";
    }

    public void OnToggleRaid()
    {
        pushSetting.raid = !pushSetting.raid;
        spToggleRaid.spriteName = pushSetting.raid? "BTN_ON" : "BTN_OFF";
    }

    public void OnTogglePvP()
    {
        pushSetting.pvp = !pushSetting.pvp;
        spTogglePvP.spriteName = pushSetting.pvp ? "BTN_ON" : "BTN_OFF";
    }

    public void OnToggleEvent()
    {
        pushSetting.evnt = !pushSetting.evnt;
        spToggleEvent.spriteName = pushSetting.evnt ? "BTN_ON" : "BTN_OFF";
    }

    public void OnToggleClean()
    {
        pushSetting.clean = !pushSetting.clean;
        spToggleClean.spriteName = pushSetting.clean ? "BTN_ON" : "BTN_OFF";
    }

    public void OnToggleNight()
    {
        pushSetting.night = !pushSetting.night;
        spToggleNight.spriteName = pushSetting.night ? "BTN_ON" : "BTN_OFF";
    }


    public void OnToggleSkipGachaAnimation()
    {
        skipGachaAnim = !skipGachaAnim;
        spToggleSkipGacha.spriteName = skipGachaAnim ? "BTN_ON" : "BTN_OFF";
    }

    public void OnToggleSkipStory()
    {
        skipStory = !skipStory;
        spToggleSkipStory.spriteName = skipStory ? "BTN_ON" : "BTN_OFF";
    }


    public void OnClickLogout()
    {
        GameCore.Instance.ShowAgree("로그아웃", "정말로 [c][7E00FF]로그아웃[-][/c] 하시겠습니까?", 0, () =>
        {
            GameCore.Instance.CloseMsgWindow();
            GameCore.Instance.CloseMsgWindow();
#if !UNITY_EDITOR
            GameCore.Instance.GamePotMgr.cblogouted = CBLogOut;
            GameCore.Instance.GamePotMgr.Logout();
#else
            CBLogOut();
#endif
        });
    }


    void CBLogOut()
    {
#if !UNITY_EDITOR
        GameCore.Instance.GamePotMgr.cblogouted = null;
#endif
        GameCore.Instance.LogOut();
    }


    public void OnClickTabOption()
    {
        goOption.SetActive(true);
        goInfo.SetActive(false);

        spTabOption.color = new Color32(0x01, 0x9E, 0x59, 0xFF);
        spTabInfo.color = new Color32(0x93, 0x93, 0x93, 0xFF);
    }

    public void OnClickTabInfo()
    {
        goInfo.SetActive(true);
        goOption.SetActive(false);

        spTabInfo.color = new Color32(0x01, 0x9E, 0x59, 0xFF);
        spTabOption.color = new Color32(0x93, 0x93, 0x93, 0xFF);
    }

    public void OnClickTermsOfUse()
    {
#if !UNITY_EDITOR
        GamePot.showTerms();
#else
        GameCore.Instance.ShowAlert("에디터에선 불가능합니다.");
#endif
    }

    public void OnClickPrivacyPolicy()
    {
#if !UNITY_EDITOR
        GamePot.showPrivacy();
#else
        GameCore.Instance.ShowAlert("에디터에선 불가능합니다.");
#endif
    }

    public void OnClickOperatingPolicy()
    {
        Application.OpenURL(CSTR.URL_OperationPolicy);
    }

    public void OnClickWithdrawal()
    {
        GameCore.Instance.ShowAgree("계정 탈퇴", "정말로 계정 탈퇴를 하시겠습니까?\n탈퇴된 계정은 복구할 수 없습니다.", 0, () =>
        {
            GameCore.Instance.CloseMsgWindow();

            if (GameCore.Instance.PlayerDataMgr.LoginType == NCommon.LoginType.NONE ||
                GameCore.Instance.PlayerDataMgr.LoginType == NCommon.LoginType.GUEST)
            {
                GameCore.Instance.NetMgr.Req_Account_Delete();
            }
            else
            {
#if !UNITY_EDITOR
                GamePot.deleteMember();
#else
                GameCore.Instance.ShowAlert("에디터에선 불가능합니다.");
#endif
            }
        });
    }

    public void OnClickCafe()
    {
        Application.OpenURL(CSTR.URL_Cafe);
    }

    public void OnClickNotice()
    {
#if !UNITY_EDITOR
        GamePot.showNotice(false);
#else
        GameCore.Instance.ShowAlert("에디터에선 불가능합니다.");
#endif
    }

    public void OnClickEnquiry()
    {
#if !UNITY_EDITOR
        GamePot.showCSWebView();
#else
        GameCore.Instance.ShowAlert("에디터에선 불가능합니다.");
#endif
    }

    public void OnClickCredit()
    {
        var credit = CreditWindow.Create();
        GameCore.Instance.ShowObject(null, null, credit.gameObject, 4, new MsgAlertBtnData[0]);
    }


    public void OnSubmitCoupon()
    {

#if !UNITY_EDITOR
        GamePot.coupon(ipCoupon.value);
#else
        GameCore.Instance.ShowAlert("에디터에선 불가능합니다.");
#endif
    }

    internal void Save()
    {
        GameCore.Instance.SoundMgr.SaveVolume();

#if !UNITY_EDITOR
        if (GameCore.Instance.PlayerDataMgr.pushSetting.night != pushSetting.night)
            GamePot.setPushNightStatus(pushSetting.night);
        if (GameCore.Instance.PlayerDataMgr.pushSetting.evnt != pushSetting.evnt)
            GamePot.setPushStatus(pushSetting.evnt);
#endif
        GameCore.Instance.PlayerDataMgr.pushSetting = pushSetting;

        PlayerPrefs.SetInt(CommonType.PFS_SKIP_GACHAANIM, skipGachaAnim ? 1 : 0);
        PlayerPrefs.SetInt(CommonType.PFS_SKIP_STORY, skipStory ? 1 : 0);
    }

    public bool IsChangePushSetting()
    {
        return pushSetting.IsNotEquals(GameCore.Instance.PlayerDataMgr.pushSetting);
    }
}
