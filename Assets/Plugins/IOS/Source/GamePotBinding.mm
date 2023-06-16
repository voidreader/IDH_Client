#import "GamePotManager.h"

/*
 * iOS 플러그인
 */

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

extern "C"
{
     char* MakeStringCopy (const char* string)
    {
        if (string == NULL)
            return NULL;

        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        return res;
    }
    void pluginVersionByUnity (const char* version)
    {
        [[GamePotManager sharedManager] pluginVersion:GetStringParam(version)];
    }
    //////////////////////
	// Common API
	//////////////////////

	char* getConfigByUnity (const char* key)
    {
        NSString *result = [[GamePotManager sharedManager] getConfig:GetStringParam(key)];

        if (result == nil) {
            result = @"";
        }

        return MakeStringCopy([result UTF8String]);
    }

    char* getConfigsByUnity ()
    {
        NSString *result = [[GamePotManager sharedManager] getConfigs];

        if (result == nil) {
            result = @"";
        }

        return MakeStringCopy([result UTF8String]);
    }

	void setLanguageByUnity (int gameLanguage)
    {
        [[GamePotManager sharedManager] setLanguage:gameLanguage];
    }

    //////////////////////
    // Channel API
    //////////////////////

    char * getLastLoginTypeByUnity ()
    {
         NSString *result = [[GamePotManager sharedManager] getLastLoginType];
        if (result == nil) {
            result = @"";
        }
        return MakeStringCopy([result UTF8String]);
    }
    char * getLinkedListByUnity()
    {
        NSString *result = [[GamePotManager sharedManager] getLinkedList];
        if (result == nil) {
            result = @"";
        }
        return MakeStringCopy([result UTF8String]);
    }

    char * getPurchaseItemByUnity()
    {
        NSString *result = [[GamePotManager sharedManager] getPurchaseItems];
        if (result == nil) {
            result = @"";
        }
        return MakeStringCopy([result UTF8String]);
    }
	void deleteMemberByUnity ()
    {
        [[GamePotManager sharedManager] deleteMember];
    }

    //////////////////////
    // Chat API
    //////////////////////

	void joinChannelByUnity (const char* prevChannel)
    {
        [[GamePotManager sharedManager] joinChannel:GetStringParam(prevChannel)];
    }

	void leaveChannelByUnity (const char* prevChannel)
    {
        [[GamePotManager sharedManager] leaveChannel:GetStringParam(prevChannel)];
    }

	void sendMessageByUnity (const char* prevChannel, const char* message)
    {
        [[GamePotManager sharedManager] sendMessage:GetStringParam(prevChannel) sendmessage:GetStringParam(message)];
    }

    void couponByUnity(const char* couponNumber, const char* userData)
    {
        [[GamePotManager sharedManager] coupon:GetStringParam(couponNumber) userData:GetStringParam(userData)];
    }

    // void trackingByUnity(const char* adActions, const char* info)
    // {
    //     [[GamePotManager sharedManager] tracking:GetStringParam(adActions) info:GetStringParam(info)];
    // }


    void logoutByUnity()
    {
        [[GamePotManager sharedManager] logout];
    }

    // void naverCafeInitGlobalByUnity()
    // {
    //     // 함수 없음
    // }


    void loginByUnity(const char* loginType)
    {
        [[GamePotManager sharedManager]login:GetStringParam(loginType)];
    }

    int sendLocalPushByUnity(const char* strDate, const char* strTitle, const char* strMessage)
    {
       return [[GamePotManager sharedManager] sendLocalPush:GetStringParam(strTitle) setMessage:GetStringParam(strMessage) setDate:GetStringParam(strDate)];
    }

    void cancelLocalPushByUnity(int pushId)
    {
        [[GamePotManager sharedManager] cancelLocalPush:pushId];
    }

    // void showNaverCafeByUnity(int menuIndex, BOOL landscape)
    // {
    //     [[GamePotManager sharedManager] showNaverCafe: menuIndex setLandScape: landscape];
    // }
    // void addAdByUnity(const char* adType)
    // {
    //     [[GamePotManager sharedManager] addAd:GetStringParam(adType)];
    // }

    void enableGameCenterByUnity(bool enable)
    {
        [[GamePotManager sharedManager] enableGameCenter:enable];
    }


    void purchaseByUnity(const char* productId, const char* uniqueId)
    {
        [[GamePotManager sharedManager] purchase:GetStringParam(productId) uniqueId:GetStringParam(uniqueId)];
    }

    bool isLinkedByUnity(const char* linkType)
    {
        return [[GamePotManager sharedManager] isLinked:GetStringParam(linkType)];
    }

    void createLinkingByUnity(const char* linkType)
    {
        [[GamePotManager sharedManager] createLinking:GetStringParam(linkType)];
    }


    void deleteLinkingByUnity(const char* linkType)
    {
        [[GamePotManager sharedManager] deleteLinking:GetStringParam(linkType)];
    }

    void addChannelByUnity(const char* channelType)
    {
        [[GamePotManager sharedManager]addChannel:GetStringParam(channelType)];
    }

    // void setSandboxByUnity(BOOL enable)
    // {
    //     [[GamePotManager sharedManager] setSandbox:enable];
    // }
    // void setAdjustDataByUnity(const char* adjustBillingData)
    // {
    //     [[GamePotManager sharedManager] setAdjustData:GetStringParam(adjustBillingData)];
    // }

    void setPushByUnity(BOOL pushEnable)
    {
        [[GamePotManager sharedManager] setPush:pushEnable];
    }

    void setPushNightByUnity(BOOL pushEnable)
    {
        [[GamePotManager sharedManager] setPushNight:pushEnable];
    }

    void setPushAdByUnity(bool pushEnable)
    {
        [[GamePotManager sharedManager] setPushAd:pushEnable];
    }

    void setPushStateByUnity(bool pushEnable, bool nightPushEnable, bool adPushEnable)
    {
        [[GamePotManager sharedManager] setPushStatus:pushEnable setNight:nightPushEnable setAd:adPushEnable];
    }

    void showNoticeWebViewByUnity()
    {
        [[GamePotManager sharedManager] showNoticeWebView];
    }

    void showWebViewByUnity(const char* url)
    {
        [[GamePotManager sharedManager] showWebView:GetStringParam(url)];
    }
    void showCSWebViewByUnity()
    {
        [[GamePotManager sharedManager] showCSWebView];
    }

    char* getPushStatusByUnity()
    {
         NSString *result = [[GamePotManager sharedManager] getPushStatus];
        if (result == nil) {
            result = @"";
        }
        return MakeStringCopy([result UTF8String]);
    }

    void showAppStatusPopupByUnity(const char* status)
    {
        [[GamePotManager sharedManager] showAppStatus:GetStringParam(status)];
    }

    void showAgreeDialogByUnity(const char* info)
    {
        [[GamePotManager sharedManager] showAgreeDialog:GetStringParam(info)];
    }

    void showTermsByUnity()
    {
        [[GamePotManager sharedManager] showTerms];
    }
    void showPrivacyByUnity()
    {
        [[GamePotManager sharedManager] showPrivacy];
    }

    void sendLogByUnity(const char* type, const char* errCode, const char* errMessage)
    {
        [[GamePotManager sharedManager] sendLog:GetStringParam(type) errCode:GetStringParam(errCode) errMessage:GetStringParam(errMessage)];
    }

    void setLoggerUseridByUnity(const char* userid)
    {
        [[GamePotManager sharedManager] setLoggerUserid:GetStringParam(userid)];
    }

    void showNoticeByUnity()
    {
        [[GamePotManager sharedManager] showNotice];
    }

    void showNoticeWithFlagByUnity(BOOL showTodayButton)
    {
        [[GamePotManager sharedManager] showNotice:showTodayButton];
    }

    void showFaqByUnity()
    {
        [[GamePotManager sharedManager] showFaq];
    }

    bool characterInfoByUnity(const char* info)
    {
        return [[GamePotManager sharedManager] characterInfo:GetStringParam(info)];
    }

}
