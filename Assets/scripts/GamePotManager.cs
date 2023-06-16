using System;
using System.Collections;
using System.Collections.Generic;
using GamePotUnity;


public class GamePotManager : IGamePot
{
    public Action cbAcceptPolicy;
    public Action cblogined;
    public Action cblogouted;
    public Action cbDialog;

    public Action<bool,string> cbPurchase;

    public NCommon.LoginType LoginType { get; private set; }
    public NUserInfo UserInfo { get; private set; }
    public bool Logined { get { return UserInfo != null; } }


    public void Reset()
    {
        LoginType = NCommon.LoginType.NONE;
        UserInfo = null;
    }


    public void autoLogin()
    {
        if (Logined)
        {
            UnityEngine.Debug.LogWarning("Alreay Gamepot Logined! from autoLogin");
            if (cblogined != null)
                cblogined.Invoke();
            return;
        }

        NCommon.LoginType type = GamePot.getLastLoginType();
        if (type != NCommon.LoginType.NONE)
            GamePot.login(type);
        else
            UnityEngine.Debug.LogWarning("First Login");
    }

    public void Login(NCommon.LoginType type)
    {
        if (Logined)
        {
            UnityEngine.Debug.LogWarning("Alreay Gamepot Logined!  from Login");
            if (cblogined != null)
                cblogined.Invoke();
            return;
        }

        switch (type)
        {
            case NCommon.LoginType.GOOGLE:
            case NCommon.LoginType.FACEBOOK:
            case NCommon.LoginType.APPLE:
            case NCommon.LoginType.GUEST:
                GamePot.login(type);
                break;

            case NCommon.LoginType.NONE:
            case NCommon.LoginType.GOOGLEPLAY:
            case NCommon.LoginType.NAVER:
            case NCommon.LoginType.GAMECENTER:
            case NCommon.LoginType.LINE:
            //case NCommon.LoginType.GUEST:
            default:
                UnityEngine.Debug.LogWarning("Not supported Type. " + type);
                break;
        }
    }

    public void Logout()
    {
        GamePot.logout();
    }

    public void onAppClose()
    {
        // TODO: 강제 업데이트나 점검 기능을 case 2 방식으로 구현하는 경우
        // TODO: 앱을 강제 종료할 수 있기 때문에 이 곳에 앱을 종료할 수 있도록 구현하세요.
        // socket disconnect();

        GameCore.Instance.NetMgr.DoDisconnect();
        GameCore.Instance.QuitApplication();
    }
    public void onNeedUpdate(NAppStatus status){
        // TODO: 파라미터로 넘어온 status 정보를 토대로 팝업을 만들어 사용자에게 알려줘야 합니다.
        // TODO: 아래 두 가지 방식 중 한 가지를 선택하세요.
        // case 1: 인게임 팝업을 통해 개발사에서 직접 UI 구현
        // case 2: SDK의 팝업을 사용(이 경우에는 아래 코드를 호출해 주세요.)
        GamePot.showAppStatusPopup(status.ToJson());
    }
    public void onMainternance(NAppStatus status){
        // TODO: 파라미터로 넘어온 status 정보를 토대로 팝업을 만들어 사용자에게 알려줘야 합니다.
        // TODO: 아래 두 가지 방식 중 한 가지를 선택하세요.
        // case 1: 인게임 팝업을 통해 개발사에서 직접 UI 구현
        // case 2: SDK의 팝업을 사용(이 경우에는 아래 코드를 호출해 주세요.)
        GamePot.showAppStatusPopup(status.ToJson());
    }
    public void onLoginCancel(){
        // 사용자가 임의로 로그인을 취소한 경우
    }
    public void onLoginSuccess(NUserInfo userInfo){
        LoginType = GamePot.getLastLoginType();
        if (LoginType == NCommon.LoginType.GOOGLE ||
            LoginType == NCommon.LoginType.FACEBOOK ||
            LoginType == NCommon.LoginType.APPLE ||
            LoginType == NCommon.LoginType.GUEST)
        {
            UserInfo = userInfo;
        }
        // NUserInfo.memberid 서버로 보내는 파라미터 추가 후 Create 또는 Login으로 이동
        // 자동 로그인
        // NCommon.LoginType type = GamePot.getLastLoginType();
        // if(type != NCommon.LoginType.NONE) {
        // {
        // 마지막에 로그인했던 로그인 타입으로 로그인하는 방식입니다.
        //  GamePot.login(type);
        // }
        // else
        // {
        // 처음 게임을 실행했거나 로그아웃한 상태. 로그인을 할 수 있는 로그인 화면으로 이동해 주세요.
        // }
        GamePot.showAppStatusPopup("로그인 성공 " + userInfo.memberid);

        if (cblogined != null)
            cblogined.Invoke();

        CancelLocalPush();
    }

    public void onLoginFailure(NError error){
        // 로그인을 실패하는 경우
        // error.message를 팝업 등으로 유저에게 알려주세요.
        GamePot.showAppStatusPopup("로그인 실패");
        GameCore.Instance.ShowAlert("Fail. " + error.code + "\n" + error.message);
    }
    public void onDeleteMemberSuccess(){
        GameCore.Instance.NetMgr.Req_Account_Delete();
        UserInfo = null;
        GamePot.logout();
    }
    public void onDeleteMemberFailure(NError error){
        GameCore.Instance.ShowAlert(error.message);
    }
    public void onLogoutSuccess(){
        UnityEngine.PlayerPrefs.SetInt("AgreeDialog", 0);
        UserInfo = null;
        if (cblogouted != null)
            cblogouted.Invoke();
    }
    public void onLogoutFailure(NError error)
    {
        GameCore.Instance.ShowAlert("Error. " + error.code + "\n" + error.message);
    }



    public void onCouponSuccess()
    {
        // Wait for Server Receive
        // GameCore.Instance.NetMgr.IncReqCount("Coupon");

        GameCore.Instance.ShowNotice("쿠폰 보상", "보상이 지급되었습니다.", 0);
    }

    public void onCouponFailure(NError error)
    {
        GameCore.Instance.ShowNotice("실패", error.message, 0);
    }



    public void onPurchaseSuccess(NPurchaseInfo purchaseInfo){
        if (cbPurchase != null)
        {
            cbPurchase(true, purchaseInfo.productId);
            cbPurchase = null;
        }

    }
    public void onPurchaseFailure(NError error){
        if (cbPurchase != null)
        {
            cbPurchase(false, error.message);
            cbPurchase = null;
        }

    }
    public void onPurchaseCancel(){
        if (cbPurchase != null)
        {
            cbPurchase(false, null);
            cbPurchase = null;
        }
    }
    public void onCreateLinkingSuccess(NUserInfo userInfo){

    }
    public void onCreateLinkingFailure(NError error){

    }
    public void onCreateLinkingCancel(){

    }
    public void onDeleteLinkingSuccess(){

    }
    public void onDeleteLinkingFailure(NError error){

    }
    public void onPushSuccess(){

    }
    public void onPushFailure(NError error){

    }
    public void onPushNightSuccess(){

    }
    public void onPushNightFailure(NError error){

    }
    public void onPushAdSuccess(){

    }
    public void onPushAdFailure(NError error){

    }
    public void onPushStatusSuccess(){

    }
    public void onPushStatusFailure(NError error){

    }
    public void onAgreeDialogSuccess(NAgreeResultInfo info){
        // info.agree : 필수 약관을 모두 동의한 경우 true
        // info.agreeNight : 야간 광고성 수신 동의를 체크한 경우 true, 그렇지 않으면 false
        // agreeNight 값은 로그인 완료 후 setPushNightStatus api를 통해 전달하세요.
        GamePot.showAppStatusPopup(info.agree.ToString());
        if(info.agree)
        {
            GamePot.setPushNightStatus(GameCore.Instance.PlayerDataMgr.pushSetting.night /*info.agreeNight*/);
            if (cbAcceptPolicy != null)
                cbAcceptPolicy.Invoke();

            UnityEngine.PlayerPrefs.SetInt("AgreeDialog", 1);
            if (cbDialog != null)
                cbDialog.Invoke();
        }

    }
    public void onAgreeDialogFailure(NError error){

        GameCore.Instance.ShowNotice("실패", error.message, 0);
        // error.message를 팝업 등으로 유저에게 알려주세요.

    }
    public void onReceiveScheme(string scheme){

    }
    public void onLoadAchievementSuccess(List<NAchievementInfo> info){

    }
    public void onLoadAchievementFailure(NError error){

    }
    public void onLoadAchievementCancel(){

    }



    public static int GetLoginTypeNumber(NCommon.LoginType _type)
    {
        switch (_type)
        {
            case NCommon.LoginType.GOOGLE:
                return 1;
            case NCommon.LoginType.FACEBOOK:
                return 2;
            case NCommon.LoginType.APPLE:
                return 3;
            case NCommon.LoginType.GUEST:
                return 4;
            case NCommon.LoginType.NONE:
            case NCommon.LoginType.GOOGLEPLAY:
            case NCommon.LoginType.NAVER:
            case NCommon.LoginType.GAMECENTER:
            case NCommon.LoginType.TWITTER:
            case NCommon.LoginType.LINE:
            default:
                return 0;
        }
    }

    public void SetLocalPush()
    {
        // Todo Regist local Push
        if (UserInfo != null)
        {
            SetLocalPushOfVigor();
            SetLocalPushOfPvPTicket();
            SetLocalPushOfRaidTicket();
            SetLocalPushOfSelfMyRoomCleaning();
        }
    }

    void SetLocalPushOfVigor()
    {
        if (!GameCore.Instance.PlayerDataMgr.pushSetting.vigor)
            return;

        var consts = GameCore.Instance.DataMgr.GetStaminaConstData();
        var item = GameCore.Instance.PlayerDataMgr.GetItemByKey(CommonType.ITEMKEY_VIGOR);
        if (item == null)
            return;

        var cnt = GameCore.Instance.PlayerDataMgr.MaxVigor - item.count;
        if (cnt <= 0)
            return;

        var sec = consts.createVigorTime - (float)(GameCore.nowTime - item.createDate).TotalSeconds;
        var dateTime = GameCore.nowTime.AddSeconds(sec).
                                        AddSeconds(consts.createVigorTime * (cnt - 1));

        if (dateTime <= GameCore.nowTime)
            return;

        var data = GameCore.Instance.DataMgr.GetRandomPushString(0);
        int pushId = GamePot.sendLocalPush(dateTime, data.title, data.desc);
        UnityEngine.PlayerPrefs.SetInt("LPVigor", pushId);

        UnityEngine.Debug.LogFormat("[PUSH] Vigor : {0} / {1}. Create Date : {3}. Remain Date : {2}", item.count, GameCore.Instance.PlayerDataMgr.MaxVigor, dateTime, item.createDate);
    }

    void SetLocalPushOfPvPTicket()
    {
        if (!GameCore.Instance.PlayerDataMgr.pushSetting.pvp)
            return;

        var consts = GameCore.Instance.DataMgr.GetPvPConstData();
        var item = GameCore.Instance.PlayerDataMgr.GetItemByKey(CommonType.ITEMKEY_TICKET_PVP);
        if (item == null)
            return;

        var cnt = consts.ticketMaxGenCount - item.count;
        if (cnt <= 0)
            return;

        var sec = consts.ticketGenTime - (float)(GameCore.nowTime - item.createDate).TotalSeconds;
        var dateTime = GameCore.nowTime.AddSeconds(sec).
                                    AddSeconds(consts.ticketGenTime * (cnt - 1));

        if (dateTime <= GameCore.nowTime)
            return;

        var data = GameCore.Instance.DataMgr.GetRandomPushString(1);
        int pushId = GamePot.sendLocalPush(dateTime, data.title, data.desc);
        UnityEngine.PlayerPrefs.SetInt("LPPvPTicket", pushId);

        UnityEngine.Debug.LogFormat("[PUSH] PvP Ticket : {0} / {1}. Create Date : {3}. Remain Time : {2} sec.", item.count, consts.ticketMaxGenCount, sec, item.createDate);
    }

    void SetLocalPushOfRaidTicket()
    {
        if (!GameCore.Instance.PlayerDataMgr.pushSetting.raid)
            return;

        var consts = GameCore.Instance.DataMgr.GetPvEConstData();
        var now = GameCore.nowTime;

        if (consts.raiTicketResetTime <= now.Hour)
            return;

        now = now.AddHours(consts.raiTicketResetTime - now.TimeOfDay.TotalHours);
        //GameCore.Instance.DataMgr.GetRandomPushString()
        int pushId = GamePot.sendLocalPush(now, " 레이드 티켓 가득", "피의 전투를 해보아효~!");
        UnityEngine.PlayerPrefs.SetInt("LPRaidTicket", pushId);
    }


    void SetLocalPushOfSelfMyRoomCleaning()
    {
        if (!GameCore.Instance.PlayerDataMgr.pushSetting.clean ||
            GameCore.Instance.PlayerDataMgr.UserMyRoom == null)
            return;
        
        // 먼지들 중 최장 시간에 완료되는 먼지를 찾는다.
        var list = GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList;
        var maxTime = DateTime.MaxValue;

        foreach (var item in list)
        {
            foreach(var stain in item.StainDataList)
            {
                var gap = stain.CleanEndTime - GameCore.nowTime;
                if (0 < gap.Ticks && stain.CleanEndTime.Ticks > maxTime.Ticks)
                    maxTime = stain.CleanEndTime;
            }
        }

        if (maxTime == DateTime.MaxValue)
            return;

        if (maxTime <= GameCore.nowTime)
            return;

        var data = GameCore.Instance.DataMgr.GetRandomPushString(2);
        int pushId = GamePot.sendLocalPush(maxTime, data.title, data.desc);
        UnityEngine.PlayerPrefs.SetInt("LPMyRoom", pushId);

        UnityEngine.Debug.LogFormat("[PUSH] MyRoom Stain. End Date : {0}.", maxTime);
    }

    public void CancelLocalPush()
    {
        var vigor  = UnityEngine.PlayerPrefs.GetInt("LPVigor", 0);
        var pvp    = UnityEngine.PlayerPrefs.GetInt("LPPvPTicket", 0);
        var raid   = UnityEngine.PlayerPrefs.GetInt("LPRaidTicket", 0);
        var myroom = UnityEngine.PlayerPrefs.GetInt("LPMyRoom", 0);

        if (0 <= vigor) GamePot.cancelLocalPush(vigor.ToString());
        if (0 <= pvp)   GamePot.cancelLocalPush(pvp.ToString());
        if (0 <= raid)  GamePot.cancelLocalPush(raid.ToString());
        if (0 <= myroom) { GamePot.cancelLocalPush(myroom.ToString()); }
        if (0 <= vigor) GamePot.cancelLocalPush(vigor.ToString());

        UnityEngine.PlayerPrefs.SetInt("LPVigor", 0);
        UnityEngine.PlayerPrefs.SetInt("LPPvPTicket", 0);
        UnityEngine.PlayerPrefs.SetInt("LPRaidTicket", 0);
        UnityEngine.PlayerPrefs.SetInt("LPMyRoom", 0);
    }

    void SetNightPushSetting(bool _enable)
    {
        GamePot.setPushNightStatus(_enable);
    }
}
