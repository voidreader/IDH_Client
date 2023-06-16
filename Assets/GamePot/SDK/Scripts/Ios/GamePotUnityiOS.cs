using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;
using AOT;
using GamePotUnity;
//using GamePotSendLogCharacter;

#if !UNITY_EDITOR && UNITY_IOS

namespace GamePotUnityiOS {

	public class GamePotUnityPluginiOS : MonoBehaviour {

		private static GamePotUnityPluginiOS mGamePotInstance	= null;
		static IGamePot mGamePotInterface;
        public class GamePotBinding
		{
			private static string Tag = "[GamePotUnityiOS]";

			public static GamePotUnityPluginiOS initPlugin()
			{
			 	if (mGamePotInstance == null) {
					mGamePotInstance = new GameObject("GamePotUnityPluginiOS").AddComponent<GamePotUnityPluginiOS>();
				}

				if (GameObject.Find ("GamePotiOSManager") != null)
				{
					Debug.Log ("updating existing prefab");
					GameObject.Find ("GamePotiOSManager").GetComponent<GamePotiOSManager> ();
				}
				else
				{
					Debug.Log ("creating GamePot iOS prefab");
					GameObject clone = (GameObject)Instantiate(Resources.Load("prefabs/GamePotiOSManager") as GameObject) ;
					clone.name = "GamePotiOSManager";
					clone.GetComponent<GamePotiOSManager> ();
				}
                // addChannel(NCommon.ChannelType.GOOGLE);
                // addChannel(NCommon.ChannelType.NAVER);
                // addChannel(NCommon.ChannelType.FACEBOOK);
                // addChannel(NCommon.ChannelType.GOOGLEPLAY);
                // addChannel(NCommon.ChannelType.GOOGLE);
                pluginVersion(GamePot.UnityPluginVersion);

				return mGamePotInstance;
			}
			public static void setListener(IGamePot inter){
				mGamePotInterface = inter;
				GamePotEventListener listener =
				GamePotiOSManager.instance.gameObject.GetComponent<GamePotEventListener> ();
				listener.setListener(inter);
			}

			public static void  pluginVersion(String version)
			{
				pluginVersionByUnity (version);
			}

			[DllImport("__Internal")]
			private static extern void pluginVersionByUnity (String version);

			//////////////////////
			// Common API
			//////////////////////

			//////////////////////
			// Chat API
			//////////////////////

			public static void  joinChannel(String prevChannel)
			{
				joinChannelByUnity(prevChannel);
			}

			[DllImport("__Internal")]
			private static extern void joinChannelByUnity (String prevChannel);

			public static void  leaveChannel(String prevChannel)
			{
				leaveChannelByUnity(prevChannel);
			}

			[DllImport("__Internal")]
			private static extern void leaveChannelByUnity (String prevChannel);

            public static void  sendMessage(String prevChannel, String message)
			{

				Debug.Log ("chatMessage - prevChannel :  " +prevChannel + " / message : " +message);
				sendMessageByUnity( prevChannel,  message);
			}

			[DllImport("__Internal")]
            private static extern void sendMessageByUnity (String prevChannel, String message);

            // public static void addAd(NCommon.AdType adType)
            // {
            //     addAdByUnity(adType.ToString());
            // }

            // [DllImport("__Internal")]
            // private static extern void addAdByUnity(string adType);

            // public static void tracking(NCommon.AdActions adActions, string adjustKey)
            // {
            //     trackingByUnity(adActions.ToString(), adjustKey);
            // }

            // public static void tracking(NCommon.AdActions adActions, TrackingInfo trackingInfo)
            // {
            //     trackingByUnity(adActions.ToString(), trackingInfo.trackingInfoConvertToJson());
            // }

            // [DllImport("__Internal")]
            // private static extern void trackingByUnity(string adActions, string adjustKey);



			public static string getLastLoginType()
            {
				return getLastLoginTypeByUnity();
            }

            [DllImport("__Internal")]
			private static extern string getLastLoginTypeByUnity();

            public static void login(NCommon.LoginType loginType)
            {
                loginByUnity(loginType.ToString());
            }

            [DllImport("__Internal")]
            private static extern void loginByUnity(string loginType);

            // public static void setSandbox(bool enable)
            // {
            //     setSandboxByUnity(enable);
            // }

            // [DllImport("__Internal")]
            // private static extern void setSandboxByUnity(bool enable);

            //public static void naverCafeInit()
            //{
            //    naverCafeInitByUnity();
            //}

            //[DllImport("__Internal")]
            //private static extern void naverCafeInitByUnity();

            // public static void naverCafeInitGlobal()
            // {
            //     naverCafeInitGlobalByUnity();
            // }

            // [DllImport("__Internal")]
            // private static extern void naverCafeInitGlobalByUnity();

			public static void deleteMember()
            {
                deleteMemberByUnity();
            }

            [DllImport("__Internal")]
            private static extern void deleteMemberByUnity();


			public static string getConfig(String key)
            {
                return getConfigByUnity(key);
            }

            [DllImport("__Internal")]
            private static extern string getConfigByUnity(String key);

            public static string getConfigs()
            {
                return getConfigsByUnity();
            }

            [DllImport("__Internal")]
            private static extern string getConfigsByUnity();

            public static void purchase(String productId, String uniqueId)
            {
                purchaseByUnity(productId, uniqueId);
            }

            [DllImport("__Internal")]
            private static extern void purchaseByUnity(String productId, String uniqueId);

            public static void coupon(String couponNumber, String userData)
            {
                couponByUnity(couponNumber, userData);
            }

            [DllImport("__Internal")]
            private static extern void couponByUnity(String couponNumber, String userData);

            public static void setLanguage(int gameLanguage)
            {
                setLanguageByUnity(gameLanguage);
            }

            [DllImport("__Internal")]
            private static extern void setLanguageByUnity(int gameLanguage);

            public static int sendLocalPush(string sdate, string title, string text)
            {
                return sendLocalPushByUnity(sdate, title, text);
            }

            [DllImport("__Internal")]
            private static extern int sendLocalPushByUnity(string sdate, string title, string text);


            public static void cancelLocalPush(int pushId)
            {
                 cancelLocalPushByUnity(pushId);
            }

            [DllImport("__Internal")]
            private static extern void cancelLocalPushByUnity(int pushId);


			public static string getLinkedList()
            {
                return getLinkedListByUnity();
            }

            [DllImport("__Internal")]
			private static extern string getLinkedListByUnity();

            public static bool isLinked(string linkType)
            {
                return isLinkedByUnity(linkType);
            }

            [DllImport("__Internal")]
			private static extern bool isLinkedByUnity(string linkType);

			//public static string getLanguage()
			//{
			//	return getLanguageByUnity();
			//}

			//[DllImport("__Internal")]
			//private static extern string getLanguageByUnity();

            // public static void showNaverCafe(int menuIndex, bool landscape)
            // {
            //     showNaverCafeByUnity(menuIndex, landscape);
            // }
            // [DllImport("__Internal")]
            // private static extern void showNaverCafeByUnity(int menuIndex, bool landscape);

            public static void logout()
			{
				logoutByUnity();
			}

			[DllImport("__Internal")]
			private static extern void logoutByUnity();

			public static void createLinking(NCommon.LinkingType linkingType)
            {
				createLinkingByUnity(linkingType.ToString());
            }


            [DllImport("__Internal")]
            private static extern void createLinkingByUnity(string linkType);


            public static void deleteLinking(NCommon.LinkingType linkingType)
			{
				deleteLinkingByUnity(linkingType.ToString());
			}

            [DllImport("__Internal")]
            private static extern void deleteLinkingByUnity(string linkType);

            public static void addChannel(NCommon.ChannelType channelType)
            {
                addChannelByUnity(channelType.ToString());
            }

            [DllImport("__Internal")]
            private static extern void addChannelByUnity(string linkType);

            public static void enableGameCenter(bool enable)
            {
                enableGameCenterByUnity(enable);
            }

            [DllImport("__Internal")]
            private static extern void enableGameCenterByUnity(bool enable);

            public static string getPurchaseItems()
            {
                return getPurchaseItemByUnity();
            }

            [DllImport("__Internal")]
            private static extern string getPurchaseItemByUnity();

            // public static void setAdjustData(AdjustInfo adjustDatas)
            // {
            //     setAdjustDataByUnity(adjustDatas.trackingInfoConvertToJson());
            // }

            // [DllImport("__Internal")]
            // private static extern void setAdjustDataByUnity(string adjustBillingData);


// TODO: push
			public static void setPush(bool pushEnable)
            {
				setPushByUnity(pushEnable);
            }

            [DllImport("__Internal")]
            private static extern void setPushByUnity(bool pushEnable);

            public static void setPushNight(bool pushEnable)
            {
				setPushNightByUnity(pushEnable);
            }

            [DllImport("__Internal")]
			private static extern void setPushNightByUnity(bool pushEnable);


            public static void setPushAd(bool pushEnable)
            {
				setPushAdByUnity(pushEnable);
            }

            [DllImport("__Internal")]
			private static extern void setPushAdByUnity(bool pushEnable);

            public static void setPushState(bool pushEnable, bool nightPushEnable, bool adPushEnable)
            {
				setPushStateByUnity(pushEnable, nightPushEnable, adPushEnable);
            }

            [DllImport("__Internal")]
			private static extern void setPushStateByUnity(bool pushEnable, bool nightPushEnable, bool adPushEnable);

            public static string getPushStatus()
            {
                return getPushStatusByUnity();
            }

            [DllImport("__Internal")]
            private static extern string getPushStatusByUnity();

            public static void showNoticeWebView()
            {
                showNoticeWebViewByUnity();
            }

            [DllImport("__Internal")]
            private static extern void showNoticeWebViewByUnity();

            public static void showCSWebView()
            {
                showCSWebViewByUnity();
            }

            [DllImport("__Internal")]
            private static extern void showCSWebViewByUnity();

            public static void showWebView(string url)
            {
                showWebViewByUnity(url);
            }

            [DllImport("__Internal")]
            private static extern void showWebViewByUnity(string url);

            public static void showAppStatusPopup(string status)
            {
                showAppStatusPopupByUnity(status);
            }

            [DllImport("__Internal")]
            private static extern void showAppStatusPopupByUnity(string status);

            public static void showAgreeDialog(string info)
            {
                showAgreeDialogByUnity(info);
            }

            [DllImport("__Internal")]
            private static extern void showAgreeDialogByUnity(string info);

            public static void showTerms()
            {
                showTermsByUnity();
            }

            [DllImport("__Internal")]
            private static extern void showTermsByUnity();

            public static void showPrivacy()
            {
                showPrivacyByUnity();
            }

            [DllImport("__Internal")]
            private static extern void showPrivacyByUnity();

            public static void sendLog(string type, string errCode, string errMessage)
            {
                sendLogByUnity(type, errCode, errMessage);
            }

            [DllImport("__Internal")]
            private static extern void sendLogByUnity(string type, string errCode, string errMessage);

             public static void setLoggerUserid(string userid)
            {
                setLoggerUseridByUnity(userid);
            }

            [DllImport("__Internal")]
            private static extern void setLoggerUseridByUnity(string userid);

            public static void showNotice()
            {
                showNoticeByUnity();
            }

            [DllImport("__Internal")]
            private static extern void showNoticeByUnity();


            public static void showNotice(bool showTodayButton)
            {
                showNoticeWithFlagByUnity(showTodayButton);
            }

            [DllImport("__Internal")]
            private static extern void showNoticeWithFlagByUnity(bool showTodayButton);


            public static void showFaq()
            {
                showFaqByUnity();
            }

            [DllImport("__Internal")]
            private static extern void showFaqByUnity();


            public static bool characterInfo(string info)
            {
                return characterInfoByUnity(info);
            }

            [DllImport("__Internal")]
            private static extern bool characterInfoByUnity(string info);


		}
	}

}

#endif