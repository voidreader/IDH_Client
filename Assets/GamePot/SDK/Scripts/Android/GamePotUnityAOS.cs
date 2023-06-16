using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using GamePotUnity;

namespace GamePotUnityAOS
{

    public class GamePotUnityPluginAOS : MonoBehaviour
    {

        private static GamePotUnityPluginAOS mGamePotInstance = null;
        private static AndroidJavaClass mGamePotClass = null;

        //Native Android Class Name
        public static string mGamePotClassName = "io.gamepot.unity.plugin.GamePotUnityPlugin";
        static IGamePot mGamePotInterface;

        public class GamePotBinding
        {
            public static void initPlugin()
            {
#if !UNITY_EDITOR && UNITY_ANDROID
				if(mGamePotInstance == null)
				{
                    Debug.Log ("GamePotUnityPluginAOS GameObject Created!!!");
                    mGamePotInstance = new GameObject("GamePotUnityPluginAOS").AddComponent<GamePotUnityPluginAOS>();
				}

                mGamePotClass = new AndroidJavaClass(mGamePotClassName + "$SDK");
                if(GamePotUnityPluginAOS.mGamePotClass != null)
				{
                    Debug.Log ("GamePotUnityPluginAOS Success!!!");
                    //mGamePotClass.CallStatic ("pluginVersion",GamePot.UnityPluginVersion);
				}
				else
				{
					Debug.LogError ("GamePotUnityPluginAOS  FAIL!!!");
				}

				if (GameObject.Find ("GamePotAndroidManager") != null)
				{
					Debug.Log ("updating existing prefab");
					GameObject.Find ("GamePotAndroidManager").GetComponent<GamePotAndroidManager> ();
				}
				else
				{
                    Debug.Log ("creating GamePot Android prefab");
					GameObject clone = (GameObject)Instantiate(Resources.Load("prefabs/GamePotAndroidManager") as GameObject) ;
					clone.name = "GamePotAndroidManager";
					clone.GetComponent<GamePotAndroidManager> ();
				}
#endif
            }

            public static GamePotUnityPluginAOS getInstance()
            {
                if (mGamePotInstance == null)
                {
                    mGamePotInstance = new GameObject("GamePotUnityPluginAOS").AddComponent<GamePotUnityPluginAOS>();
                }
                return mGamePotInstance;
            }

            public static void setListener(IGamePot inter)
            {
                Debug.Log("GamePotUnityPluginAOS::setListener");
                mGamePotInterface = inter;
                GamePotEventListener listener = GamePotAndroidManager.instance.gameObject.GetComponent<GamePotEventListener>();
                listener.setListener(inter);
            }

            //////////////////////
            // Common API
            //////////////////////
            public static void setLanguage(int gameLanguage)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("setLanguage", gameLanguage);
            }

            //////////////////////
            // Channel API
            //////////////////////
            public static int sendLocalPush(DateTime sendDate, String title, String message)
            {

                int result = -1;
                if (mGamePotClass != null)
                    result = mGamePotClass.CallStatic<int>("sendLocalPush", sendDate.ToString("yyyy-MM-dd HH:mm:ss"), title, message);
                return result;
            }
            public static void cancelLocalPush(Int32 pushId)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic<bool>("cancelLocalPush", pushId);
            }
            public static string getLanguage()
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<string>("getLanguage");
                }
                return "";
            }
            public static string getLastLoginType()
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<string>("getLastLoginType");
                }
                return "";
            }
            public static void login(NCommon.LoginType loginType)
            {
                Debug.Log(">> GamePotUnityAOS login called #1 <<");

                if (mGamePotClass != null)
                {
                    Debug.Log(">> GamePotUnityAOS login called #2 <<");
                    mGamePotClass.CallStatic("login", loginType.ToString());

                }
            }

            // public static void addAd(NCommon.AdType adType)
            // {
            //     if (mGamePotClass != null)
            //         mGamePotClass.CallStatic("addAd", adType.ToString());
            // }
            // public static void tracking(NCommon.AdActions adActions, string adjustKey)
            // {
            //     if (mGamePotClass != null)
            //         mGamePotClass.CallStatic("tracking", adActions.ToString(), adjustKey);
            // }

            // public static void tracking(NCommon.AdActions adActions, TrackingInfo trackingInfo)
            // {
            //     if (mGamePotClass != null)
            //         mGamePotClass.CallStatic("tracking", adActions.ToString(), trackingInfo.trackingInfoConvertToJson());
            // }

            // public static void setSandbox(bool enable)
            // {
            //     if (mGamePotClass != null)
            //         mGamePotClass.CallStatic("setSandbox", enable);
            // }

            public static void deleteMember()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("deleteMember");
            }

            public static void logout()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("logout");
            }

            public static string getConfig(string key)
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<string>("getConfig", key);
                }
                return "";
            }

            public static string getConfigs()
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<string>("getConfigs");
                }
                return "";
            }

            public static void coupon(string couponNumber, string userData)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("coupon", couponNumber, userData);
            }

            public static void purchase(string productId, String uniqueId)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("purchase", productId, uniqueId);
            }

            public static string getPurchaseItems()
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<string>("getPurchaseItems");
                }
                return "";
            }
            public static bool isLinked(string linkType)
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<bool>("isLinked", linkType);
                }
                return false;
            }
            public static string getLinkedList()
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<string>("getLinkedList");
                }
                return "";
            }
            public static void createLinking(NCommon.LinkingType linkType)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("createLinking", linkType.ToString());
            }
            public static void deleteLinking(NCommon.LinkingType linkType)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("deleteLinking", linkType.ToString());
            }

            public static void addChannel(NCommon.ChannelType channelType)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("addChannel", channelType.ToString());
            }

            public static void setPush(bool pushEnable)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("setPush", pushEnable);
            }

            public static void setPushNight(bool pushEnable)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("setPushNightStatus", pushEnable);
            }

            public static void setPushAd(bool pushEnable)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("setPushAdStatus", pushEnable);
            }

            public static void setPushState(bool pushEnable, bool nightPushEnable, bool adPushEnable)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("setPushStatus", pushEnable, nightPushEnable, adPushEnable);
            }

            public static string getPushStatus()
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<string>("getPushStatus");
                }
                return "";
            }

            // public static void showNaverCafe(int menuIndex, bool landscape)
            // {
            //     if (mGamePotClass != null)
            //         mGamePotClass.CallStatic("naverCafeStartHome");
            // }

            public static void showNoticeWebView()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showNoticeWebView");
            }

            public static void showWebView(string url)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showWebView", url);
            }

            public static void showCSWebView()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showCSWebView");
            }

            public static void showAppStatusPopup(string status)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showAppStatusPopup", status);
            }

            public static void showAgreeDialog(string info)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showAgreeDialog", info);
            }

            public static void showTerms()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showTerms");
            }

            public static void showPrivacy()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showPrivacy");
            }

            public static void showNotice()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showNotice");
            }

            public static void showNotice(bool showTodayButton)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showNotice", showTodayButton);
            }

            public static void showFaq()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showFaq");
            }


            public static void setLoggerUserid(string userid)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("setLoggerUserid", userid);
            }

            public static void sendLog(string type, string errCode, string errMessage)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("sendLog", type, errCode, errMessage);
            }

            public static void showAchievement()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showAchievement");
            }

            public static void showLeaderboard()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("showLeaderboard");
            }

            public static void unlockAchievement(string achievementId)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("unlockAchievement", achievementId);
            }

            public static void incrementAchievement(string achievementId, string count)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("incrementAchievement", achievementId, count);
            }

            public static void submitScoreLeaderboard(string leaderBoardId, string leaderBoardScore)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("submitScoreLeaderboard", leaderBoardId, leaderBoardScore);
            }

            public static void loadAchievement()
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("loadAchievement");
            }

            public static void purchaseThirdPayments(string productId, string uniqueId)
            {
                if (mGamePotClass != null)
                    mGamePotClass.CallStatic("purchaseThirdPayments", productId, uniqueId);
            }

            public static string getPurchaseThirdPaymentsItems()
            {
                if (mGamePotClass != null)
                {
                    return mGamePotClass.CallStatic<string>("getPurchaseThirdPaymentsItems");
                }
                return "";
            }

            public static bool characterInfo(string data)
            {
                if (mGamePotClass != null){
                    return mGamePotClass.CallStatic<bool>("characterInfo", data);
                }
                return false;
            }

            private static AndroidJavaObject javaArrayFromCS(string[] values)
            {
                AndroidJavaClass arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
                AndroidJavaObject arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("java.lang.String"), values.Length);
                for (int i = 0; i < values.Length; ++i)
                {
                    arrayClass.CallStatic("set", arrayObject, i, new AndroidJavaObject("java.lang.String", values[i]));
                }

                return arrayObject;
            }

            

        }

    }

}