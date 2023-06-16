using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Realtime.LITJson;
using GamePotUnityAOS;
using System;


#if !UNITY_EDITOR && UNITY_IOS
using GamePotUnityiOS;
#endif

namespace GamePotUnity
{
    public class GamePot
    {
        public static string UnityPluginVersion = "2.0.2";

        public static NCommon.LoginType getLastLoginType()
        {
            string result = "";
#if !UNITY_EDITOR && UNITY_IOS
			result = GamePotUnityPluginiOS.GamePotBinding.getLastLoginType();
#elif !UNITY_EDITOR && UNITY_ANDROID
			result = GamePotUnityPluginAOS.GamePotBinding.getLastLoginType();
#elif UNITY_EDITOR
#endif
            switch (result)
            {
                case "GOOGLE":
                    return NCommon.LoginType.GOOGLE;
                case "GOOGLEPLAY":
                    return NCommon.LoginType.GOOGLEPLAY;
                case "FACEBOOK":
                    return NCommon.LoginType.FACEBOOK;
                case "NAVER":
                    return NCommon.LoginType.NAVER;
                case "GAMECENTER":
                    return NCommon.LoginType.GAMECENTER;
                case "TWITTER":
                    return NCommon.LoginType.TWITTER;
                case "LINE":
                    return NCommon.LoginType.LINE;
                case "APPLE":
                    return NCommon.LoginType.APPLE;
                case "GUEST":
                    return NCommon.LoginType.GUEST;
            }

            return NCommon.LoginType.NONE;
        }

        public static string getMemberId()
        {

            if (GamePotSettings.MemberInfo == null)
                return "";

            return GamePotSettings.MemberInfo.memberid;
        }

        public static string getMemberName()
        {
            if (GamePotSettings.MemberInfo == null)
                return "";

            return GamePotSettings.MemberInfo.name;
        }

        public static string getMemberEmail()
        {
            if (GamePotSettings.MemberInfo == null)
                return "";

            return GamePotSettings.MemberInfo.email;
        }

        public static string getMemberProfileUrl()
        {
            if (GamePotSettings.MemberInfo == null)
                return "";

            return GamePotSettings.MemberInfo.profileUrl;
        }
        public static string getMemberSocialId()
        {
            if (GamePotSettings.MemberInfo == null)
                return "";

            return GamePotSettings.MemberInfo.userid;
        }
        public static string getLanguage()
        {
            string result = "";
#if !UNITY_EDITOR && UNITY_IOS
                return "ko-KR";
    			//result = GamePotUnityPluginiOS.GamePotBinding.getLanguage();
#elif !UNITY_EDITOR && UNITY_ANDROID
    			result = GamePotUnityPluginAOS.GamePotBinding.getLanguage();
#elif UNITY_EDITOR
#endif
            return result;
        }

        public static List<NLinkingInfo> getLinkedList()
        {
            string result = "";
#if !UNITY_EDITOR && UNITY_IOS
                        result = GamePotUnityPluginiOS.GamePotBinding.getLinkedList();
#elif !UNITY_EDITOR && UNITY_ANDROID
                        result = GamePotUnityPluginAOS.GamePotBinding.getLinkedList();
#elif UNITY_EDITOR
#endif

            JsonData data = JsonMapper.ToObject(result);
            List<NLinkingInfo> itemData = new List<NLinkingInfo>();
            foreach (JsonData item in data)
            {
                Debug.Log("GamePot::getLinkedList-" + item["provider"]);

                NLinkingInfo info = new NLinkingInfo();
                if (item["provider"].ToString() == "google")
                    info.provider = NCommon.LinkingType.GOOGLE;
                else if (item["provider"].ToString() == "facebook")
                    info.provider = NCommon.LinkingType.FACEBOOK;
                else if (item["provider"].ToString() == "naver")
                    info.provider = NCommon.LinkingType.NAVER;
                else if (item["provider"].ToString() == "googleplay")
                    info.provider = NCommon.LinkingType.GOOGLEPLAY;
                else if (item["provider"].ToString() == "gamecenter")
                    info.provider = NCommon.LinkingType.GAMECENTER;
                else if (item["provider"].ToString() == "line")
                    info.provider = NCommon.LinkingType.LINE;
                else if (item["provider"].ToString() == "twitter")
                    info.provider = NCommon.LinkingType.TWITTER;
                else if (item["provider"].ToString() == "apple")
                    info.provider = NCommon.LinkingType.APPLE;
                itemData.Add(info);
            }
            return itemData;
        }
        public static void initPlugin()
        {
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.initPlugin();
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.initPlugin();
#elif UNITY_EDITOR
#endif
        }

        public static void setListener(IGamePot inter)
        {
#if !UNITY_EDITOR && UNITY_IOS
			GamePotEventListener listener = GamePotiOSManager.instance.gameObject.GetComponent<GamePotEventListener> ();
            listener.setListener(inter);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotEventListener listener = GamePotAndroidManager.instance.gameObject.GetComponent<GamePotEventListener> ();
            listener.setListener(inter);
#elif UNITY_EDITOR
#endif
        }

        //
        //        //업적, 리더보드 사용 시 추가
        //        public static void enableGameCenter(bool enable)
        //        {
        //            #if !UNITY_EDITOR && UNITY_IOS
        //			GamePotUnityPluginiOS.GamePotBinding.enableGameCenter(enable);
        //            #elif UNITY_EDITOR
        //            #endif
        //        }
        //
        //         public static void addAd(NCommon.AdType adType)
        //         {
        // #if !UNITY_EDITOR && UNITY_IOS
        // 			GamePotUnityPluginiOS.GamePotBinding.addAd(adType);
        // #elif !UNITY_EDITOR && UNITY_ANDROID
        // 			GamePotUnityPluginAOS.GamePotBinding.addAd(adType);
        // #elif UNITY_EDITOR
        // #endif
        //         }


        //         public static void tracking(NCommon.AdActions adActions, string adjustKey)
        //         {
        // #if !UNITY_EDITOR && UNITY_IOS
        // 			GamePotUnityPluginiOS.GamePotBinding.tracking(adActions, adjustKey);
        // #elif !UNITY_EDITOR && UNITY_ANDROID
        // 			GamePotUnityPluginAOS.GamePotBinding.tracking(adActions, adjustKey);
        // #elif UNITY_EDITOR
        // #endif
        //         }
        //         public static void showNaverCafe(int menuIndex, bool landscape)
        //         {
        // #if !UNITY_EDITOR && UNITY_IOS
        // 			GamePotUnityPluginiOS.GamePotBinding.showNaverCafe(menuIndex, landscape);
        // #elif !UNITY_EDITOR && UNITY_ANDROID
        // 			GamePotUnityPluginAOS.GamePotBinding.showNaverCafe(menuIndex, landscape);
        // #elif UNITY_EDITOR
        // #endif
        //         }
        //         public static void tracking(NCommon.AdActions adActions, TrackingInfo trackingInfo)
        //         {
        // #if !UNITY_EDITOR && UNITY_IOS
        // 			GamePotUnityPluginiOS.GamePotBinding.tracking(adActions, trackingInfo);
        // #elif !UNITY_EDITOR && UNITY_ANDROID
        // 			GamePotUnityPluginAOS.GamePotBinding.tracking(adActions, trackingInfo);
        // #elif UNITY_EDITOR
        // #endif
        //         }

        //Login

        //send Local Push
        public static int sendLocalPush(DateTime sendDate, string title, string message)
        {
#if !UNITY_EDITOR && UNITY_IOS
                        return GamePotUnityPluginiOS.GamePotBinding.sendLocalPush(sendDate.ToString("yyyy-MM-dd HH:mm:ss"), title, message);
#elif !UNITY_EDITOR && UNITY_ANDROID
            			return GamePotUnityPluginAOS.GamePotBinding.sendLocalPush(sendDate, title, message);
#elif UNITY_EDITOR
#endif
            return -1;
        }
        //send Local Push

        public static bool isLinked(string linkType)
        {
#if !UNITY_EDITOR && UNITY_IOS
                return GamePotUnityPluginiOS.GamePotBinding.isLinked(linkType);
#elif !UNITY_EDITOR && UNITY_ANDROID
                return GamePotUnityPluginAOS.GamePotBinding.isLinked(linkType);
#elif UNITY_EDITOR
#endif
            return false;
        }
        public static void cancelLocalPush(string pushId)
        {
#if !UNITY_EDITOR && UNITY_IOS
                 GamePotUnityPluginiOS.GamePotBinding.cancelLocalPush(Int32.Parse(pushId));
#elif !UNITY_EDITOR && UNITY_ANDROID
            	 GamePotUnityPluginAOS.GamePotBinding.cancelLocalPush(Int32.Parse(pushId));
#elif UNITY_EDITOR
#endif
            return;
        }
        public static void login(NCommon.LoginType loginType)
        {
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.login(loginType);
#elif !UNITY_EDITOR && UNITY_ANDROID
            GamePotUnityPluginAOS.GamePotBinding.login(loginType);
#elif UNITY_EDITOR
#endif
        }

        //         public static void setSandbox(bool enable)
        //         {
        // #if !UNITY_EDITOR && UNITY_IOS
        // 			GamePotUnityPluginiOS.GamePotBinding.setSandbox(enable);
        // #elif !UNITY_EDITOR && UNITY_ANDROID
        // 			GamePotUnityPluginAOS.GamePotBinding.setSandbox(enable);
        // #elif UNITY_EDITOR
        // #endif
        //         }

        public static void deleteMember()
        {
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.deleteMember();
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.deleteMember();
#elif UNITY_EDITOR
#endif
        }

        public static void logout()
        {
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.logout();
#elif !UNITY_EDITOR && UNITY_ANDROID
            GamePotUnityPluginAOS.GamePotBinding.logout();
#elif UNITY_EDITOR
#endif
        }

        public static string getConfig(string key)
        {
            string result = "";
#if !UNITY_EDITOR && UNITY_IOS
			result = GamePotUnityPluginiOS.GamePotBinding.getConfig(key);
#elif !UNITY_EDITOR && UNITY_ANDROID
			result = GamePotUnityPluginAOS.GamePotBinding.getConfig(key);
#elif UNITY_EDITOR
#endif
            return result;
        }

        public static string getConfigs()
        {
            string result = "";
#if !UNITY_EDITOR && UNITY_IOS
			result = GamePotUnityPluginiOS.GamePotBinding.getConfigs();
#elif !UNITY_EDITOR && UNITY_ANDROID
			result = GamePotUnityPluginAOS.GamePotBinding.getConfigs();
#elif UNITY_EDITOR
#endif
            return result;
        }

        public static void coupon(string couponNumber)
        {
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.coupon(couponNumber, "");
#elif !UNITY_EDITOR && UNITY_ANDROID
            GamePotUnityPluginAOS.GamePotBinding.coupon(couponNumber, "");
#elif UNITY_EDITOR
#endif
        }

        public static void coupon(string couponNumber, string userData)
        {
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.coupon(couponNumber, userData);
#elif !UNITY_EDITOR && UNITY_ANDROID
            GamePotUnityPluginAOS.GamePotBinding.coupon(couponNumber, userData);
#elif UNITY_EDITOR
#endif
        }


        public static void setLanguage(NCommon.GameLanguage gameLanguage)
        {
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.setLanguage((int)gameLanguage);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.setLanguage((int)gameLanguage);
#elif UNITY_EDITOR
#endif
        }

        public static void purchase(string productId)
        {
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.purchase(productId, "");
#elif !UNITY_EDITOR && UNITY_ANDROID
            GamePotUnityPluginAOS.GamePotBinding.purchase(productId, "");
#elif UNITY_EDITOR
#endif
        }

        public static void purchase(string productId, string uniqueId)
        {
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.purchase(productId, uniqueId);
#elif !UNITY_EDITOR && UNITY_ANDROID
            GamePotUnityPluginAOS.GamePotBinding.purchase(productId, uniqueId);
#elif UNITY_EDITOR
#endif
        }

        public static NPurchaseItem[] getPurchaseItems()
        {
            String result = "";
#if !UNITY_EDITOR && UNITY_IOS
			result = GamePotUnityPluginiOS.GamePotBinding.getPurchaseItems();
#elif !UNITY_EDITOR && UNITY_ANDROID
			result = GamePotUnityPluginAOS.GamePotBinding.getPurchaseItems();
#elif UNITY_EDITOR
#endif

            NPurchaseItem[] itemData = JsonMapper.ToObject<NPurchaseItem[]>(result);
            return itemData;
        }

        public static void createLinking(NCommon.LinkingType linkType)
        {
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.createLinking(linkType);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.createLinking(linkType);
#elif UNITY_EDITOR
#endif
        }
        public static void deleteLinking(NCommon.LinkingType linkType)
        {
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.deleteLinking(linkType);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.deleteLinking(linkType);
#elif UNITY_EDITOR
#endif
        }

        public static void setPushStatus(bool pushEnable)
        {
            Debug.Log("[GPUnity][Call] setPushStatus : " + pushEnable);

#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.setPush(pushEnable);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.setPush(pushEnable);
#elif UNITY_EDITOR
#endif
        }

        public static void setPushNightStatus(bool nightPushEnable)
        {
            Debug.Log("[GPUnity][Call] setPushNightStatus : " + nightPushEnable);
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.setPushNight(nightPushEnable);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.setPushNight(nightPushEnable);
#elif UNITY_EDITOR
#endif
        }

        public static void setPushADStatus(bool adPushEnable)
        {
            Debug.Log("[GPUnity][Call] setPushADStatus : " + adPushEnable);
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.setPushAd(adPushEnable);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.setPushAd(adPushEnable);
#elif UNITY_EDITOR
#endif
        }

        public static void setPushStatus(bool pushEnable, bool nightPushEnable, bool adPushEnable)
        {
            Debug.Log("[GPUnity][Call] setPush : " + pushEnable + " NightPush : " + nightPushEnable + " adPush : " + adPushEnable);
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.setPushState(pushEnable, nightPushEnable, adPushEnable);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.setPushState(pushEnable, nightPushEnable, adPushEnable);
#elif UNITY_EDITOR
#endif
        }

        public static NPushInfo getPushStatus()
        {
            Debug.Log("[GPUnity][Call] getPushStatus");

            String result = "";
#if !UNITY_EDITOR && UNITY_IOS
			result = GamePotUnityPluginiOS.GamePotBinding.getPushStatus();
#elif !UNITY_EDITOR && UNITY_ANDROID
			result = GamePotUnityPluginAOS.GamePotBinding.getPushStatus();
#elif UNITY_EDITOR
#endif
            Debug.Log("[GPUnity][Call] getPushStatus result : " + result);
            NPushInfo pushInfo = JsonMapper.ToObject<NPushInfo>(result);
            return pushInfo;
        }

        public static void showNoticeWebView()
        {
            Debug.Log("[GPUnity][Call] showNoticeWebView");
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showNoticeWebView();
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showNoticeWebView();
#elif UNITY_EDITOR
#endif
        }

        public static void showWebView(string url)
        {
            Debug.Log("[GPUnity][Call] showWebView url : " + url);
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showWebView(url);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showWebView(url);
#elif UNITY_EDITOR
#endif
        }

        public static void showCSWebView()
        {
            Debug.Log("[GPUnity][Call] showCSWebView");
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showCSWebView();
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showCSWebView();
#elif UNITY_EDITOR
#endif
        }

        public static void showAppStatusPopup(string status)
        {
            Debug.Log("[GPUnity][Call] showAppStatusPopup - " + status);
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showAppStatusPopup(status);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showAppStatusPopup(status);
#elif UNITY_EDITOR
#endif
        }

        public static void showAgreeDialog()
        {
            showAgreeDialog(null);
        }

        public static void showAgreeDialog(NAgreeInfo info)
        {
            Debug.Log("[GPUnity][Call] showAgreeDialog");
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showAgreeDialog(info != null ? info.ToJson().ToString() : null);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showAgreeDialog(info != null ? info.ToJson().ToString() : null);
#elif UNITY_EDITOR
#endif
        }

        public static void showTerms()
        {
            Debug.Log("[GPUnity][Call] showTerms");
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showTerms();
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showTerms();
#elif UNITY_EDITOR
#endif
        }

        public static void showPrivacy()
        {
            Debug.Log("[GPUnity][Call] showPrivacy");
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showPrivacy();
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showPrivacy();
#elif UNITY_EDITOR
#endif
        }

        public static void showNotice()
        {
            Debug.Log("[GPUnity][Call] showNotice");
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showNotice();
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showNotice();
#elif UNITY_EDITOR
#endif
        }

        public static void showNotice(bool showTodayButton)
        {
            Debug.Log("[GPUnity][Call] showNotice");
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showNotice(showTodayButton);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showNotice(showTodayButton);
#elif UNITY_EDITOR
#endif
        }

        public static void showFaq()
        {
            Debug.Log("[GPUnity][Call] showFaq");
#if !UNITY_EDITOR && UNITY_IOS
			GamePotUnityPluginiOS.GamePotBinding.showFaq();
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showFaq();
#elif UNITY_EDITOR
#endif
        }



        public static void d(string errCode, string errMessage)
        {
            Debug.Log("[GPUnity][Call] d");
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.sendLog("d",errCode, errMessage);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.sendLog("d", errCode, errMessage);
#elif UNITY_EDITOR
#endif
        }
        public static void i(string errCode, string errMessage)
        {
            Debug.Log("[GPUnity][Call] i");
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.sendLog("i",errCode, errMessage);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.sendLog("i", errCode, errMessage);
#elif UNITY_EDITOR
#endif
        }
        public static void w(string errCode, string errMessage)
        {
            Debug.Log("[GPUnity][Call] w");
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.sendLog("w",errCode, errMessage);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.sendLog("w", errCode, errMessage);
#elif UNITY_EDITOR
#endif
        }
        public static void e(string errCode, string errMessage)
        {
            Debug.Log("[GPUnity][Call] e");
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.sendLog("e",errCode, errMessage);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.sendLog("e", errCode, errMessage);
#elif UNITY_EDITOR
#endif
        }

        public static void setLoggerUserid(string userid)
        {
            Debug.Log("[GPUnity][Call] setLoggerUserid");
#if !UNITY_EDITOR && UNITY_IOS
            GamePotUnityPluginiOS.GamePotBinding.setLoggerUserid(userid);
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.setLoggerUserid(userid);
#elif UNITY_EDITOR
#endif
        }

        public static void showAchievement()
        {
            Debug.Log("[GPUnity][Call] showAchievement");
#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showAchievement();
#elif UNITY_EDITOR
#endif
        }

        public static void showLeaderboard()
        {
            Debug.Log("[GPUnity][Call] showLeaderboard");
#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.showLeaderboard();
#elif UNITY_EDITOR
#endif
        }

        public static void unlockAchievement(string achievementId)
        {
            Debug.Log("[GPUnity][Call] unlockAchievement - " + achievementId);
#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.unlockAchievement(achievementId);
#elif UNITY_EDITOR
#endif
        }

        public static void incrementAchievement(string achievementId, string count)
        {
            Debug.Log("[GPUnity][Call] incrementAchievement - " + achievementId + ", " + count);
#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.incrementAchievement(achievementId, count);
#elif UNITY_EDITOR
#endif
        }

        public static void submitScoreLeaderboard(string leaderBoardId, string leaderBoardScore)
        {
            Debug.Log("[GPUnity][Call] submitScoreLeaderboard - " + leaderBoardId + ", " + leaderBoardScore);
#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.submitScoreLeaderboard(leaderBoardId, leaderBoardScore);
#elif UNITY_EDITOR
#endif
        }

        public static void loadAchievement()
        {
            Debug.Log("[GPUnity][Call] loadAchievement");
#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.loadAchievement();
#elif UNITY_EDITOR
#endif
        }

        public static void purchaseThirdPayments(string productId)
        {
#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.purchaseThirdPayments(productId, "");
#elif UNITY_EDITOR
#endif
        }

        public static void purchaseThirdPayments(string productId, string uniqueId)
        {
            Debug.Log("[GPUnity][Call] purchaseThirdPayments - " + productId + ", " + uniqueId);
#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			GamePotUnityPluginAOS.GamePotBinding.purchaseThirdPayments(productId, uniqueId);
#elif UNITY_EDITOR
#endif
        }

        public static NPurchaseItem[] getPurchaseThirdPaymentsItems()
        {
            Debug.Log("[GPUnity][Call] getPurchaseThirdPaymentsItems");
            String result = "";

#if !UNITY_EDITOR && UNITY_IOS
#elif !UNITY_EDITOR && UNITY_ANDROID
			result = GamePotUnityPluginAOS.GamePotBinding.getPurchaseThirdPaymentsItems();
#elif UNITY_EDITOR
#endif

            NPurchaseItem[] itemData = JsonMapper.ToObject<NPurchaseItem[]>(result);
            return itemData;
        }

        public static Boolean characterInfo(GamePotSendLogCharacter info)
        {
            Debug.Log("[GPUnity][Call] characterInfo");
            Boolean result = false;

            if (info == null)
            {
                Debug.Log("GamePotSendLogCharacter is null");
                return false;
            }

#if !UNITY_EDITOR && UNITY_IOS
            result = GamePotUnityPluginiOS.GamePotBinding.characterInfo(info.toString());
#elif !UNITY_EDITOR && UNITY_ANDROID
			result = GamePotUnityPluginAOS.GamePotBinding.characterInfo(info.toString());
#elif UNITY_EDITOR
#endif
            return result;
        }
    }
}
