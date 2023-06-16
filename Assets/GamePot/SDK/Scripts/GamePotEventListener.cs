using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realtime.LITJson;

public class GamePotEventListener : MonoBehaviour
{

    public IGamePot GamePotInterface;

    public void setListener(IGamePot v)
    {
        Debug.Log("GamePotEventListener::setListener()");
        GamePotInterface = v;
    }

    public void onAppClose()
    {
        Debug.Log("GamePotEventListener::onAppClose()");
        GamePotInterface.onAppClose();
    }

    public void onNeedUpdate(string result)
    {
        Debug.Log("GamePotEventListener::onNeedUpdate-" + result);
        NAppStatus status = JsonMapper.ToObject<NAppStatus>(result);
        GamePotInterface.onNeedUpdate(status);
    }

    public void onMainternance(string result)
    {
        Debug.Log("GamePotEventListener::onMainternance-" + result);
        NAppStatus status = JsonMapper.ToObject<NAppStatus>(result);
        GamePotInterface.onMainternance(status);
    }

    public void onLoginSuccess(string result)
    {
        Debug.Log("GamePotEventListener::onLoginSuccess-" + result);
        NUserInfo uesrInfo = JsonMapper.ToObject<NUserInfo>(result);
        GamePotInterface.onLoginSuccess(uesrInfo);
    }
    public void onLoginCancel()
    {
        Debug.Log("GamePotEventListener::onLoginCancel()");
        GamePotInterface.onLoginCancel();
    }

    public void onLoginFailure(string result)
    {
        Debug.Log("GamePotEventListener::onLoginFailure()-" + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onLoginFailure(error);
    }

    public void onDeleteMemberFailure(string result)
    {
        Debug.Log("GamePotEventListener::onDeleteMemberFailure() - " + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onDeleteMemberFailure(error);
    }

    public void onDeleteMemberSuccess()
    {
        Debug.Log("GamePotEventListener::onDeleteMemberSuccess()");
        GamePotInterface.onDeleteMemberSuccess();
    }

    public void onLogoutFailure(string result)
    {
        Debug.Log("GamePotEventListener::onLogOutFailure() - " + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onLogoutFailure(error);
    }

    public void onLogoutSuccess(string result)
    {
        Debug.Log("GamePotEventListener::onLogOutSuccess()");
        GamePotInterface.onLogoutSuccess();
    }

    // public void onShowGlinkResult(string result)
    // {
    //     Debug.Log("GamePotEventListener::onShowGlinkResult()");
    // 	//		GamePotInterface.onShowGlinkResult(result);
    // }

    public void onCouponSuccess(string result)
    {
        Debug.Log("GamePotEventListener::onCouponSuccess()");
        GamePotInterface.onCouponSuccess();
    }

    public void onCouponFailure(string result)
    {
        Debug.Log("GamePotEventListener::onCouponFailure() : " + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onCouponFailure(error);
    }

    public void onPurchaseSuccess(string result)
    {
        NPurchaseInfo info = JsonMapper.ToObject<NPurchaseInfo>(result);
        Debug.Log("GamePotEventListener::onPurchaseSuccess()");
        GamePotInterface.onPurchaseSuccess(info);
    }

    public void onPurchaseFailure(string result)
    {
        Debug.Log("GamePotEventListener::onPurchaseFailure() - " + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onPurchaseFailure(error);
    }

    public void onPurchaseCancel()
    {
        Debug.Log("GamePotEventListener::onPurchaseCancel()");
        GamePotInterface.onPurchaseCancel();
    }


    public void onCreateLinkingCancel(string result)
    {
        Debug.Log("GamePotEventListener::onCreateLinkCancel()" + result);
        GamePotInterface.onCreateLinkingCancel();
    }

    public void onCreateLinkingSuccess(string result)
    {
        Debug.Log("GamePotEventListener::onCreateLinkSuccess() - " + result);
        NUserInfo uesrInfo = JsonMapper.ToObject<NUserInfo>(result);
        GamePotInterface.onCreateLinkingSuccess(uesrInfo);
    }

    public void onCreateLinkingFailure(string result)
    {
        Debug.Log("GamePotEventListener::onCreateLinkFailure() - " + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onCreateLinkingFailure(error);
    }
    public void onDeleteLinkingSuccess()
    {
        Debug.Log("GamePotEventListener::onDeleteLinkSuccess()");
        GamePotInterface.onDeleteLinkingSuccess();
    }

    public void onDeleteLinkingFailure(string result)
    {
        Debug.Log("GamePotEventListener::onDeleteLinkFailure() - " + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onDeleteLinkingFailure(error);
    }

    public void onPushSuccess()
    {
        Debug.Log("GamePotEventListener::onPushSuccess()");
        GamePotInterface.onPushSuccess();
    }

    public void onPushFailure(string result)
    {
        Debug.Log("GamePotEventListener::onPushFailure()" + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onPushFailure(error);
    }

    public void onPushNightSuccess()
    {
        Debug.Log("GamePotEventListener::onPushNightSuccess()");
        GamePotInterface.onPushNightSuccess();
    }

    public void onPushNightFailure(string result)
    {
        Debug.Log("GamePotEventListener::onPushNightFailure()" + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onPushNightFailure(error);
    }

    public void onPushAdSuccess()
    {
        Debug.Log("GamePotEventListener::onPushAdSuccess()");
        GamePotInterface.onPushAdSuccess();
    }

    public void onPushAdFailure(string result)
    {
        Debug.Log("GamePotEventListener::onPushAdFailure()" + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onPushAdFailure(error);
    }

    public void onPushStatusSuccess()
    {
        Debug.Log("GamePotEventListener::onPushStatusSuccess()");
        GamePotInterface.onPushStatusSuccess();
    }

    public void onPushStatusFailure(string result)
    {
        Debug.Log("GamePotEventListener::onPushStatusFailure()" + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onPushStatusFailure(error);
    }

    public void onAgreeDialogSuccess(string result)
    {
        Debug.Log("GamePotEventListener::onAgreeDialogSuccess() - " + result);
        NAgreeResultInfo resultInfo = JsonMapper.ToObject<NAgreeResultInfo>(result);
        GamePotInterface.onAgreeDialogSuccess(resultInfo);
    }

    public void onAgreeDialogFailure(string result)
    {
        Debug.Log("GamePotEventListener::onAgreeDialogFailure()" + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onAgreeDialogFailure(error);
    }

    public void onReceiveScheme(string scheme)
    {
        Debug.Log("GamePotEventListener::onReceiveScheme()" + scheme);
        GamePotInterface.onReceiveScheme(scheme);
    }

    public void onLoadAchievementSuccess(string result)
    {
        Debug.Log("GamePotEventListener::onLoadAchievementSuccess()" + result);
        List<NAchievementInfo> resultInfo = JsonMapper.ToObject<List<NAchievementInfo>>(result);
        GamePotInterface.onLoadAchievementSuccess(resultInfo);
    }

    public void onLoadAchievementFailure(string result)
    {
        Debug.Log("GamePotEventListener::onLoadAchievementFailure()" + result);
        NError error = JsonMapper.ToObject<NError>(result);
        GamePotInterface.onLoadAchievementFailure(error);
    }

    public void onLoadAchievementCancel()
    {
        Debug.Log("GamePotEventListener::onLoadAchievementCancel()");
        GamePotInterface.onLoadAchievementCancel();
    }
}
