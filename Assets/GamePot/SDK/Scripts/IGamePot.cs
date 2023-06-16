using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IGamePot
{

    void onAppClose();
    void onNeedUpdate(NAppStatus status);
    void onMainternance(NAppStatus status);
    void onLoginCancel();
    void onLoginSuccess(NUserInfo userInfo);
    void onLoginFailure(NError error);
    void onDeleteMemberSuccess();
    void onDeleteMemberFailure(NError error);
    void onLogoutSuccess();
    void onLogoutFailure(NError error);
    void onCouponSuccess();
    void onCouponFailure(NError error);
    void onPurchaseSuccess(NPurchaseInfo purchaseInfo);
    void onPurchaseFailure(NError error);
    void onPurchaseCancel();
    void onCreateLinkingSuccess(NUserInfo userInfo);
    void onCreateLinkingFailure(NError error);
    void onCreateLinkingCancel();
    void onDeleteLinkingSuccess();
    void onDeleteLinkingFailure(NError error);
    void onPushSuccess();
    void onPushFailure(NError error);
    void onPushNightSuccess();
    void onPushNightFailure(NError error);
    void onPushAdSuccess();
    void onPushAdFailure(NError error);
    void onPushStatusSuccess();
    void onPushStatusFailure(NError error);
    void onAgreeDialogSuccess(NAgreeResultInfo info);
    void onAgreeDialogFailure(NError error);
    void onReceiveScheme(string scheme);
    void onLoadAchievementSuccess(List<NAchievementInfo> info);
    void onLoadAchievementFailure(NError error);
    void onLoadAchievementCancel();
}
