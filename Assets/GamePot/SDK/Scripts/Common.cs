using UnityEngine;
using System.Collections;

public class NCommon
{

    // openDeep MenuID
    // public enum AdActions
    // {
    //     APPLICATION_START,
    //     RESUME,
    //     PAUSE,
    //     TUTORIAL_COMPLETE,
    //     LEVEL,
    //     EVENT,
    //     BILLING
    // }

    public enum LoginType
    {
        NONE,
        GOOGLE,
        GOOGLEPLAY,
        FACEBOOK,
        NAVER,
        GAMECENTER,
        TWITTER,
        LINE,
        APPLE,
        GUEST
    }

    public enum ChannelType
    {
        GOOGLEPLAY,
        GOOGLE,
        FACEBOOK,
        NAVER,
        GAMECENTER,
        TWITTER,
        LINE,
        APPLE,
    }

    // public enum AdType
    // {
    //     FACEBOOK,
    //     ADJUST,
    //     ADBRIX,
    // }

    public enum LinkingType
    {
        GOOGLEPLAY,
        GAMECENTER,
        GOOGLE,
        FACEBOOK,
        NAVER,
        TWITTER,
        LINE,
        APPLE,
    }

    public enum GameOrientation
    {
        portrait = 1,
        landscape = 2,
    }

    public enum GameLanguage
    {
        KOREAN = 0,
        ENGLISH = 1,
        CHINESE_CN = 2,
        CHINESE_TW = 3,
        GERMAN = 4,
        JAPANESE = 5
    }
}

