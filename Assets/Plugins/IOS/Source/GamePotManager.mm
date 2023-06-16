#import "GamePotManager.h"
#import <GamePot/GamePot.h>
#import <GamePotChannel/GamePotChannel.h>
#import <StoreKit/StoreKit.h>
// #if __has_include(<GamePotAd/GamePotAd.h>)
// #import <GamePotAd/GamePotAd.h>
// #endif
// #if __has_include(<GamePotAdFacebook/GamePotAdFacebook.h>)
// #import <GamePotAdFacebook/GamePotAdFacebook.h>
// #endif
// #if __has_include(<GamePotAdAdjust/GamePotAdAdjust.h>)
// #import <GamePotAdAdjust/GamePotAdAdjust.h>
// #endif
#if __has_include(<Gamepot/GamePotSendLog.h>)
#import <Gamepot/GamePotSendLog.h>
#import <Gamepot/GamePotSendLogCharacter.h>
#endif

#if __has_include(<GamePotFacebook/GamePotFacebook.h>)
#import <GamePotFacebook/GamePotFacebook.h>
#endif
#if __has_include(<GamePotGameCenter/GamePotGameCenter.h>)
#import <GamePotGameCenter/GamePotGameCenter.h>
#endif
#if __has_include(<GamePotGoogleSignIn/GamePotGoogleSignIn.h>)
#import <GamePotGoogleSignIn/GamePotGoogleSignIn.h>
#endif
#if __has_include(<GamePotNaver/GamePotNaver.h>)
#import <GamePotNaver/GamePotNaver.h>
#endif
#if __has_include(<GamePotLogger/GamePotLogger.h>)
#import <GamePotLogger/GamePotLogger.h>
#endif
#if __has_include(<GamePotTwitter/GamePotTwitter.h>)
#import <GamePotTwitter/GamePotTwitter.h>
#endif
#if __has_include(<GamePotLine/GamePotLine.h>)
#import <GamePotLine/GamePotLine.h>
#endif
#if __has_include(<GamePotApple/GamePotApple.h>)
#import <GamePotApple/GamePotApple.h>
#endif
// #if __has_include(<GamePotNaverCafe/GamePotNaverCafe.h>)
// #import <GamePotNaverCafe/GamePotNaverCafe.h>
// #endif
UIViewController *UnityGetGLViewController();
const char* ListenerForUnity = "GamePotiOSManager";
@interface GamePotManager()  <GamePotPurchaseDelegate> {

}
@end

@implementation GamePotManager


@synthesize handler;

- (void)GamePotPurchaseSuccess:(GamePotPurchaseInfo *)_info
{
    // _info.Prdouctid와 adjustKey 비교 하여 adjustValue를 setAdjustKey에 추가
// #if __has_include(<GamePotAdAdjust/GamePotAdAdjust.h>)
    //    if(adjustBillingDic != nil && adjustBillingDic.count > 0)
    //    {
    //        for (NSString *key in adjustBillingDic) {
    //            if([[_info productId] isEqualToString:key])
    //                [_info setAdjustKey:[adjustBillingDic valueForKey:key]];
    //        }
    //    }
// #endif

// #if __has_include(<GamePotAd/GamePotAd.h>)
//        [[GamePotAd getInstance] tracking:BILLING obj:_info];
// #endif
    //    UnitySendMessage(ListenerForUnity, "onCharged", sendStr);

    UnitySendMessage(ListenerForUnity, "onPurchaseSuccess", [[_info toJsonString] UTF8String]);

}

- (void)GamePotPurchaseFail:(NSError *)_error
{
    // 결제 실패
    if(_error)
    {
        UnitySendMessage(ListenerForUnity, "onPurchaseFailure", [self getError:_error]);
    }
    else
    {
        UnitySendMessage(ListenerForUnity, "onPurchaseFailure", "");
    }
}

- (void)GamePotPurchaseCancel
{
    // 결제 중 유저에 의한 취소
    UnitySendMessage(ListenerForUnity, "onPurchaseCancel", "");
}

+ (GamePotManager *)sharedManager {

    static GamePotManager *sharedMyManager = nil;

    static dispatch_once_t onceToken;

    dispatch_once(&onceToken, ^{
        sharedMyManager = [[self alloc] init];
        [[GamePot getInstance] setPurchaseDelegate:sharedMyManager];
    });

    return sharedMyManager;
}

#pragma mark UnityBinding

- (void) pluginVersion:(NSString*)version
{
    NSLog(@"GamePot Plugin Version : %@", version);
}

//////////////////////
// Common API
/////////////////////

- (NSString*) getConfig:(NSString*) key
{
    NSLog(@"getConfig key = %@", key);
    @try
    {
         return [[GamePot getInstance] getConfig:key];
    }
    @catch(NSException *exception)
    {
        NSLog(@"GetConfig exception: %@", exception);
        return @"";
    }
}

- (NSString*) getConfigs
{
    @try
    {
        NSArray* array = [[GamePot getInstance] getConfigs];

        NSData *data = [NSJSONSerialization dataWithJSONObject:array
                                                       options:NSJSONWritingPrettyPrinted
                                                         error:nil];

        NSString *jsonStr = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
        return jsonStr;
    }
    @catch(NSException *exception)
    {
        NSLog(@"GetConfig exception: %@", exception);
        return @"";
    }

}

- (void) setLanguage:(int)gameLanguage
{
    NSLog(@"setLanguage gameLanguage = %@", gameLanguage);
}

//////////////////////
// Channel API
//////////////////////
- (int) sendLocalPush:(NSString*)strTitle setMessage:(NSString*)strMessage setDate:(NSString*)strDate
{
     return [[GamePot getInstance] sendLocalPush:strTitle setMessage:strMessage setDateString:strDate];
}
- (void) cancelLocalPush: (int) pushId
{
     [[GamePot getInstance] cancelLocalPush:pushId];
}
- (NSString *) getLinkedList
{
    return [[GamePotChannel getInstance] getLinkedListJsonString];
}

- (BOOL) isLinked:(NSString*)linkType
{
    GamePotChannelType type = NONE;

    if([linkType isEqualToString:@"GUEST"])
    {
        type = GUEST;
    }
    else if([linkType isEqualToString:@"GOOGLE"])
    {
        type = GOOGLE;
    }
    else if([linkType isEqualToString:@"FACEBOOK"])
    {
        type = FACEBOOK;
    }
    else if([linkType isEqualToString:@"GAMECENTER"])
    {
        type = GAMECENTER;
    }
    else if([linkType isEqualToString:@"NAVER"])
    {
        type = NAVER;
    }
    else if([linkType isEqualToString:@"TWITTER"])
    {
        type = TWITTER;
    }
    else if([linkType isEqualToString:@"LINE"])
    {
        type = LINE;
    }
    else if([linkType isEqualToString:@"APPLE"])
    {
        type = APPLE;
    }

    return [[GamePotChannel getInstance] isLinked:type];
}

- (NSString *) getPushStatus
{
    return [[GamePot getInstance] getPushStatusJsonString];
}

- (NSString *) getLastLoginType
{
    GamePotChannelType channelType = [[GamePotChannel getInstance] lastLoginType];
    NSString *strChannelType = @"NONE";
    switch (channelType) {
        case GOOGLE:
            strChannelType = @"GOOGLE";
            break;
        case FACEBOOK:
            strChannelType = @"FACEBOOK";
            break;
        case NAVER:
            strChannelType = @"NAVER";
            break;
        case GAMECENTER:
            strChannelType = @"GAMECENTER";
            break;
        case TWITTER:
            strChannelType = @"TWITTER";
            break;
        case LINE:
            strChannelType = @"LINE";
            break;
        case APPLE:
            strChannelType = @"APPLE";
            break;
        case GUEST:
            strChannelType = @"GUEST";
            break;
        default:
            break;
    }
    return strChannelType;
}
/**
@property(nonatomic) NSString* memberid;
@property(nonatomic) NSString* userid;
@property(nonatomic) NSString* name;
@property(nonatomic) NSString* profileUrl;
@property(nonatomic) NSString* token;
@property(nonatomic) NSString* email;
**/
- (const char*) getUser:(GamePotUserInfo*) userInfo
{
    NSString* jsonString = @"{}";

    if(userInfo != nil)
    {
        jsonString = [userInfo toJsonString];
    }

    return [jsonString cStringUsingEncoding:NSUTF8StringEncoding];
}
- (const char*) getError:(NSError*) error
{
     NSMutableDictionary* errorData = [[NSMutableDictionary alloc] init];
    [errorData setObject:[NSNumber numberWithInteger:[error code]] forKey:@"code"];
    [errorData setObject:[error localizedDescription] forKey:@"message"];

    NSError* jsonParsingError = nil;
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:errorData options:NSJSONWritingPrettyPrinted error:&jsonParsingError];
    if(jsonParsingError != nil)
    {
        NSLog(@"Login Error Result json parsing Error");
        return nil;
    }
    NSString* jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    return [jsonString cStringUsingEncoding:NSUTF8StringEncoding];
}
- (void) invokeGuestLogin
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"invokeGuestLogin");
        @try
        {
            [[GamePotChannel getInstance] Login:GUEST viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
                UnitySendMessage(ListenerForUnity, "onLoginSuccess", [self getUser:userInfo]);
            } cancel:^{
                UnitySendMessage(ListenerForUnity, "onLoginCancel",  "");
            } fail:^(NSError *error) {
                if(error)
                {
                    UnitySendMessage(ListenerForUnity, "onLoginFailure", [self getError:error]);
                    return;
                }
                UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
            } update:^(GamePotAppStatus *appStatus) {

                if([appStatus resultPayload] != nil)
                {
                    [appStatus setResultPayload:[(GamePotUserInfo*)[appStatus resultPayload] toJsonString]];
                }

                UnitySendMessage(ListenerForUnity, "onNeedUpdate", [[appStatus toJsonString] UTF8String]);
            } maintenance:^(GamePotAppStatus *appStatus) {
                UnitySendMessage(ListenerForUnity, "onMainternance", [[appStatus toJsonString] UTF8String]);
            }];
        }
        @catch(NSException *exception)
        {
            NSLog(@"createGuestLogin exception: %@", exception);
            UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
        }
    });
}

- (void) invokeGoogleLogin
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"invokeGoogleLogin");
        @try
        {
            [[GamePotChannel getInstance] Login:GOOGLE viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
                UnitySendMessage(ListenerForUnity, "onLoginSuccess", [self getUser:userInfo]);
            } cancel:^{
                UnitySendMessage(ListenerForUnity, "onLoginCancel",  "");
            } fail:^(NSError *error) {
                if(error)
                {
                    UnitySendMessage(ListenerForUnity, "onLoginFailure", [self getError:error]);
                    return;
                }
                UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
            } update:^(GamePotAppStatus *appStatus) {
                if([appStatus resultPayload] != nil)
                {
                    [appStatus setResultPayload:[(GamePotUserInfo*)[appStatus resultPayload] toJsonString]];
                }

                UnitySendMessage(ListenerForUnity, "onNeedUpdate", [[appStatus toJsonString] UTF8String]);
            } maintenance:^(GamePotAppStatus *appStatus) {
                UnitySendMessage(ListenerForUnity, "onMainternance", [[appStatus toJsonString] UTF8String]);
            }];
        }
        @catch(NSException *exception)
        {
            NSLog(@"createGoogleLogin exception: %@", exception);
            UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
        }
    });
}

- (void) invokeFBLogin
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"invokeFBLogin");
        @try
        {
            [[GamePotChannel getInstance] Login:FACEBOOK viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
                UnitySendMessage(ListenerForUnity, "onLoginSuccess", [self getUser:userInfo]);
            } cancel:^{
                UnitySendMessage(ListenerForUnity, "onLoginCancel",  "");
            } fail:^(NSError *error) {
                if(error)
                {
                    UnitySendMessage(ListenerForUnity, "onLoginFailure", [self getError:error]);
                    return;
                }
                UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
            } update:^(GamePotAppStatus *appStatus) {
                if([appStatus resultPayload] != nil)
                {
                    [appStatus setResultPayload:[(GamePotUserInfo*)[appStatus resultPayload] toJsonString]];
                }
                UnitySendMessage(ListenerForUnity, "onNeedUpdate", [[appStatus toJsonString] UTF8String]);
            } maintenance:^(GamePotAppStatus *appStatus) {
                UnitySendMessage(ListenerForUnity, "onMainternance", [[appStatus toJsonString] UTF8String]);
            }];
        }
        @catch(NSException *exception)
        {
            NSLog(@"createFBLogin exception: %@", exception);
            UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
        }
    });
}

- (void) invokeGameCenterLogin
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"invokeGameCenterLogin");
        @try
        {
            [[GamePotChannel getInstance] Login:GAMECENTER viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
                UnitySendMessage(ListenerForUnity, "onLoginSuccess", [self getUser:userInfo]);
            } cancel:^{
                UnitySendMessage(ListenerForUnity, "onLoginCancel",  "");
            } fail:^(NSError *error) {
                if(error)
                {
                    UnitySendMessage(ListenerForUnity, "onLoginFailure", [self getError:error]);
                    return;
                }
                UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
            } update:^(GamePotAppStatus *appStatus) {
                if([appStatus resultPayload] != nil)
                {
                    [appStatus setResultPayload:[(GamePotUserInfo*)[appStatus resultPayload] toJsonString]];
                }
                UnitySendMessage(ListenerForUnity, "onNeedUpdate", [[appStatus toJsonString] UTF8String]);
            } maintenance:^(GamePotAppStatus *appStatus) {
                UnitySendMessage(ListenerForUnity, "onMainternance", [[appStatus toJsonString] UTF8String]);
            }];
        }
        @catch(NSException *exception)
        {
            NSLog(@"createGameCenterLogin exception: %@", exception);
            UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
        }
    });
}

- (void) invokeNaverLogin
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"invokeNaverLogin");
        @try
        {
            [[GamePotChannel getInstance] Login:NAVER viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
                UnitySendMessage(ListenerForUnity, "onLoginSuccess", [self getUser:userInfo]);
            } cancel:^{
                UnitySendMessage(ListenerForUnity, "onLoginCancel",  "");
            } fail:^(NSError *error) {
                if(error)
                {
                    UnitySendMessage(ListenerForUnity, "onLoginFailure", [self getError:error]);
                    return;
                }
                UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
            } update:^(GamePotAppStatus *appStatus) {
                if([appStatus resultPayload] != nil)
                {
                    [appStatus setResultPayload:[(GamePotUserInfo*)[appStatus resultPayload] toJsonString]];
                }
                UnitySendMessage(ListenerForUnity, "onNeedUpdate", [[appStatus toJsonString] UTF8String]);
            } maintenance:^(GamePotAppStatus *appStatus) {
                UnitySendMessage(ListenerForUnity, "onMainternance", [[appStatus toJsonString] UTF8String]);
            }];
        }
        @catch(NSException *exception)
        {
            NSLog(@"createGameCenterLogin exception: %@", exception);
            UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
        }
    });
}

- (void) invokeTwitterLogin
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"invokeTwitterLogin");
        @try
        {
            [[GamePotChannel getInstance] Login:TWITTER viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
                UnitySendMessage(ListenerForUnity, "onLoginSuccess", [self getUser:userInfo]);
            } cancel:^{
                UnitySendMessage(ListenerForUnity, "onLoginCancel",  "");
            } fail:^(NSError *error) {
                if(error)
                {
                    UnitySendMessage(ListenerForUnity, "onLoginFailure", [self getError:error]);
                    return;
                }
                UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
            } update:^(GamePotAppStatus *appStatus) {
                if([appStatus resultPayload] != nil)
                {
                    [appStatus setResultPayload:[(GamePotUserInfo*)[appStatus resultPayload] toJsonString]];
                }
                UnitySendMessage(ListenerForUnity, "onNeedUpdate", [[appStatus toJsonString] UTF8String]);
            } maintenance:^(GamePotAppStatus *appStatus) {
                UnitySendMessage(ListenerForUnity, "onMainternance", [[appStatus toJsonString] UTF8String]);
            }];
        }
        @catch(NSException *exception)
        {
            NSLog(@"createGameCenterLogin exception: %@", exception);
            UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
        }
    });
}

- (void) invokeLineLogin
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"invokeLineLogin");
        @try
        {
            [[GamePotChannel getInstance] Login:LINE viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
                UnitySendMessage(ListenerForUnity, "onLoginSuccess", [self getUser:userInfo]);
            } cancel:^{
                UnitySendMessage(ListenerForUnity, "onLoginCancel",  "");
            } fail:^(NSError *error) {
                if(error)
                {
                    UnitySendMessage(ListenerForUnity, "onLoginFailure", [self getError:error]);
                    return;
                }
                UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
            } update:^(GamePotAppStatus *appStatus) {
                if([appStatus resultPayload] != nil)
                {
                    [appStatus setResultPayload:[(GamePotUserInfo*)[appStatus resultPayload] toJsonString]];
                }
                UnitySendMessage(ListenerForUnity, "onNeedUpdate", [[appStatus toJsonString] UTF8String]);
            } maintenance:^(GamePotAppStatus *appStatus) {
                UnitySendMessage(ListenerForUnity, "onMainternance", [[appStatus toJsonString] UTF8String]);
            }];
        }
        @catch(NSException *exception)
        {
            NSLog(@"createGameCenterLogin exception: %@", exception);
            UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
        }
    });
}

- (void) invokeAppleLogin
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSLog(@"invokeAppleLogin");
        @try
        {
            [[GamePotChannel getInstance] Login:APPLE viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
                UnitySendMessage(ListenerForUnity, "onLoginSuccess", [self getUser:userInfo]);
            } cancel:^{
                UnitySendMessage(ListenerForUnity, "onLoginCancel",  "");
            } fail:^(NSError *error) {
                if(error)
                {
                    UnitySendMessage(ListenerForUnity, "onLoginFailure", [self getError:error]);
                    return;
                }
                UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
            } update:^(GamePotAppStatus *appStatus) {
                if([appStatus resultPayload] != nil)
                {
                    [appStatus setResultPayload:[(GamePotUserInfo*)[appStatus resultPayload] toJsonString]];
                }
                UnitySendMessage(ListenerForUnity, "onNeedUpdate", [[appStatus toJsonString] UTF8String]);
            } maintenance:^(GamePotAppStatus *appStatus) {
                UnitySendMessage(ListenerForUnity, "onMainternance", [[appStatus toJsonString] UTF8String]);
            }];
        }
        @catch(NSException *exception)
        {
            NSLog(@"createGameCenterLogin exception: %@", exception);
            UnitySendMessage(ListenerForUnity, "onLoginFailure", "");
        }
    });
}

- (void) deleteMember
{
    NSLog(@"CommonDeleteMember");
    @try
    {
        [[GamePotChannel getInstance] DeleteMemberWithSuccess:^{
            //            const char *sendStr = [[NSString stringWithFormat:@"탈퇴를 성공했습니다."] UTF8String];
            UnitySendMessage(ListenerForUnity, "onDeleteMemberSuccess", "" );
        } fail:^(NSError *error) {
            NSDictionary *dic = [error userInfo];
            if(dic)
            {
                UnitySendMessage(ListenerForUnity, "onDeleteMemberFailure", [self getError:error]);
            }
            else
            {
                UnitySendMessage(ListenerForUnity, "onDeleteMemberFailure", "");
            }
        }];
    }
    @catch(NSException *exception)
    {
        NSLog(@"CommonDeleteMember exception: %@", exception);
        UnitySendMessage(ListenerForUnity, "onDeleteMemberFailure", "");
    }
}

//////////////////////
// Chat API
//////////////////////
- (void) coupon:(NSString*) couponNumber userData:(NSString*)userData {
    [[GamePot getInstance] coupon:couponNumber userData:userData handler:^(BOOL _success, NSError *_error) {
        if(_success)
        {
            UnitySendMessage(ListenerForUnity, "onCouponSuccess", "");
        }
        else
        {
            UnitySendMessage(ListenerForUnity, "onCouponFailure", [self getError:_error]);
        }
    }];
}

// - (void) createTutirialData:(NSString*) tutorialData
// {
// #if __has_include(<GamePotAd/GamePotAd.h>)
//     NSData* jsonData = [tutorialData dataUsingEncoding:NSUTF8StringEncoding];
//     NSMutableDictionary *dic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONWritingPrettyPrinted error:nil];
//     NSString * contentData = [dic valueForKey:@"contentData"];
//     NSString * contentId = [dic valueForKey:@"contentId"];
//     NSString * isSuccess = [dic valueForKey:@"isSuccess"];
//     NSString * adjustKey = [dic valueForKey:@"adjustKey"];

//     TrackerTutorial* tutorialEvent = [[TrackerTutorial alloc] init];
//     [tutorialEvent setContentData:contentData];
//     [tutorialEvent setContentId:contentId];
//     [tutorialEvent setSuccess:[isSuccess boolValue]];
//     [tutorialEvent setAdjustKey:adjustKey];

//     [[GamePotAd getInstance] tracking:TUTORIAL_COMPLETE obj:tutorialEvent];
// #endif
// }

// - (void) createLevelData:(NSString*) levelData
// {
// #if __has_include(<GamePotAd/GamePotAd.h>)
//     NSData* jsonData = [levelData dataUsingEncoding:NSUTF8StringEncoding];
//     NSMutableDictionary *dic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONWritingPrettyPrinted error:nil];
//     NSString * levelInfo = [dic valueForKey:@"level"];
//     NSString * adjustKey = [dic valueForKey:@"adjustKey"];

//     TrackerLevel* level = [[TrackerLevel alloc] init];

//     [level setLevel:levelInfo];
//     [level setAdjustKey:adjustKey];

//     [[GamePotAd getInstance] tracking:LEVEL obj:level];
// #endif
// }

// - (void) createEventData:(NSString*) eventData
// {
// #if __has_include(<GamePotAd/GamePotAd.h>)
//     NSData* jsonData = [eventData dataUsingEncoding:NSUTF8StringEncoding];
//     NSMutableDictionary *dic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONWritingPrettyPrinted error:nil];
//     NSString * eventInfo = [dic valueForKey:@"event"];
//     NSString * adjustKey = [dic valueForKey:@"adjustKey"];

//     TrackerEvent* event = [[TrackerEvent alloc] init];
//     [event setEvent:eventInfo];
//     [event setAdjustKey:adjustKey];

//     [[GamePotAd getInstance] tracking:EVENT obj:event];
// #endif
// }

//tracking 가공 함수 End

// - (void) tracking:(NSString*) adActions info:(NSString *)info {
//     if([adActions isEqualToString:@"TUTORIAL_COMPLETE"])
//         [self createTutirialData:info];
//     else if([adActions isEqualToString:@"LEVEL"])
//         [self createLevelData:info];
//     else if([adActions isEqualToString:@"EVENT"])
//         [self createEventData:info];
// }

// // NaverCafe
// - (void) naverCafeInit {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] setup];
//     #endif
// }

// - (void) naverCafeInitGlobal {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] setupGlobal];
//     #endif
// }

// - (void) naverCafeStartHome {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] start:UnityGetGLViewController() setMenuIndex:0];
//     #endif
// }

// - (void) naverCafeStartImageWrite:(NSString*) imageUri {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] startImageWrite:UnityGetGLViewController() setFilepath:imageUri];
//     #endif
// }

// - (void) naverCafeStartAllVideos {}
// - (void) naverCafeStartWrite {}
// - (void) naverCafeStartAllImages {}

// - (void) naverCafeStartVideoWrite:(NSString*)videoUri {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] startVideoWrite:UnityGetGLViewController() setFilepath:videoUri];
//     #endif
// }



// - (void) naverCafeSetWidgetEnable:(BOOL) enable {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] setWidgetEnable:enable];
//     #endif
// }

// - (void) naverCafeSetUseScreenshot:(BOOL) enable {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] setUseWidgetScreenShot:enable];
//     #endif
// }

// - (void) naverCafeSetUserId:(NSString*) userId {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] setUserId:userId];
//     #endif
// }

// - (void) naverCafeSetChannelCode:(NSString*) channelCode {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] setChannelCode:channelCode];
//     #endif
// }

// - (void) naverCafeSetUseVideoRecord:(BOOL) enable {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] setUseWidgetVideoRecord:enable];
//     #endif
// }

// - (void) naverCafeStartMenu:(int) menuId {
//     #if __has_include(<NNavarCafe/NNavarCafe.h>)
//     [[NNaverCafe getInstance] start:UnityGetGLViewController() setMenuIndex:menuId];
//     #endif
// }


- (void) logout
{
    [[GamePotChannel getInstance] LogoutWithSuccess:^{
        UnitySendMessage(ListenerForUnity, "onLogoutSuccess",  "");
    } fail:^(NSError *error) {
        UnitySendMessage(ListenerForUnity, "onLogoutFailure", [self getError:error]);
    }];
}

- (void) login:(NSString*) loginType {
    if([loginType isEqualToString:@"GUEST"]){
        [self invokeGuestLogin];
    }
    else if([loginType isEqualToString:@"GOOGLE"]){
        [self invokeGoogleLogin];
    }
    else if([loginType isEqualToString:@"FACEBOOK"]){
        [self invokeFBLogin];
    }
    else if([loginType isEqualToString:@"GAMECENTER"]){
        [self invokeGameCenterLogin];
    }
    else if([loginType isEqualToString:@"NAVER"]){
        [self invokeNaverLogin];
    }
    else if([loginType isEqualToString:@"TWITTER"]){
        [self invokeTwitterLogin];
    }
    else if([loginType isEqualToString:@"LINE"]){
        [self invokeLineLogin];
    }
    else if([loginType isEqualToString:@"APPLE"]){
        [self invokeAppleLogin];
    }
    else if([loginType isEqualToString:@"NONE"]){}
}

// - (void) addAd:(NSString*) adType
// {
// #if __has_include(<GamePotAd/GamePotAd.h>)
//     if([adType isEqualToString:@"FACEBOOK"]){
// #if __has_include(<GamePotAdFacebook/GamePotAdFacebook.h>)
//         [[GamePotAd getInstance] addAds:[[GamePotAdFacebook alloc] init]];
// #endif
//     }
//     else if([adType isEqualToString:@"ADJUST"]){
// #if __has_include(<GamePotAdAdjust/GamePotAdAdjust.h>)
//         [[GamePotAd getInstance] addAds:[[GamePotAdAdjust alloc] init]];
// #endif
//     }
// #endif
// }

- (void) enableGameCenter:(BOOL) enable;
{
    [[GamePotChannel getInstance] enableGameCenter:enable];
}

-(void) purchase:(NSString*)productId uniqueId:(NSString*)uniqueId
{
    [[GamePot getInstance] purchase:productId uniqueId:uniqueId];
}

- (void) showNaverCafe:(int)menuId setLandScape:(BOOL)landscape
{
#if __has_include(<GamePotNaverCafe/GamePotNaverCafe.h>)
    dispatch_async(dispatch_get_main_queue(), ^{
        [[GamePotNaverCafe getInstance] setOrientationIsLandscape:landscape];
        if(menuId > 0)
            [[GamePotNaverCafe getInstance] start:UnityGetGLViewController() setMenuIndex: menuId];
        else
            [[GamePotNaverCafe getInstance] start:UnityGetGLViewController()];
    });
#endif
}
- (void) deleteLinking:(NSString*)linkType
{
   GamePotChannelType channelType = NONE;
     if([linkType isEqualToString:@"GOOGLE"])
         channelType = GOOGLE;
     else if([linkType isEqualToString:@"FACEBOOK"])
        channelType = FACEBOOK;
     else if([linkType isEqualToString:@"GAMECENTER"])
        channelType = GAMECENTER;
     else if([linkType isEqualToString:@"NAVER"])
        channelType = NAVER;
     else if([linkType isEqualToString:@"FACEBOOK"])
        channelType = FACEBOOK;
     else if([linkType isEqualToString:@"TWITTER"])
        channelType = TWITTER;
     else if([linkType isEqualToString:@"LINE"])
        channelType = LINE;
    else if([linkType isEqualToString:@"APPLE"])
        channelType = APPLE;
     else
        return;

    [[GamePotChannel getInstance] DeleteLinking: channelType success:^ {
        UnitySendMessage(ListenerForUnity, "onDeleteLinkingSuccess", "");
    } fail:^(NSError *error) {
        if(error)
        {
            UnitySendMessage(ListenerForUnity, "onDeleteLinkingFailure", [self getError:error]);
            return;
        }

        UnitySendMessage(ListenerForUnity, "onCreateLinkingFailure", "");
    }];

}
- (void) createLinking:(NSString*)linkType
{
    GamePotChannelType channelType = NONE;
     if([linkType isEqualToString:@"GOOGLE"])
         channelType = GOOGLE;
     else if([linkType isEqualToString:@"FACEBOOK"])
        channelType = FACEBOOK;
     else if([linkType isEqualToString:@"GAMECENTER"])
        channelType = GAMECENTER;
     else if([linkType isEqualToString:@"NAVER"])
        channelType = NAVER;
     else if([linkType isEqualToString:@"FACEBOOK"])
        channelType = FACEBOOK;
     else if([linkType isEqualToString:@"TWITTER"])
        channelType = TWITTER;
     else if([linkType isEqualToString:@"LINE"])
        channelType = LINE;
     else if([linkType isEqualToString:@"APPLE"])
        channelType = APPLE;
     else
        return;

    dispatch_async(dispatch_get_main_queue(), ^{
        [[GamePotChannel getInstance] CreateLinking:channelType viewController:UnityGetGLViewController() success:^(GamePotUserInfo* userInfo) {
            UnitySendMessage(ListenerForUnity, "onCreateLinkingSuccess", [self getUser:userInfo]);
        } cancel:^{
            UnitySendMessage(ListenerForUnity, "onCreateLinkingCancel", "");
        } fail:^(NSError *error) {
            if(error)
            {
                UnitySendMessage(ListenerForUnity, "onCreateLinkingFailure", [self getError:error]);
                return;
            }

            UnitySendMessage(ListenerForUnity, "onCreateLinkingFailure", "");
        }];
    });
}

- (void) addChannel:(NSString*)channelType
{
    if([channelType isEqualToString:@"FACEBOOK"])
    {
#if __has_include(<GamePotFacebook/GamePotFacebook.h>)
        GamePotChannelInterface* facebook = [[GamePotFacebook alloc] init];
        [[GamePotChannel getInstance] addChannelWithType:FACEBOOK interface:facebook];
#endif
    }
    else if([channelType isEqualToString:@"GOOGLE"])
    {
#if __has_include(<GamePotGoogleSignIn/GamePotGoogleSignIn.h>)
        GamePotChannelInterface* google   = [[GamePotGoogleSignIn alloc] init];
        [[GamePotChannel getInstance] addChannelWithType:GOOGLE interface:google];
#endif
    }
    else if([channelType isEqualToString:@"GAMECENTER"])
    {
#if __has_include(<GamePotGameCenter/GamePotGameCenter.h>)
        GamePotChannelInterface* gamecenter = [[GamePotGameCenter alloc] init];
        [[GamePotChannel getInstance] addChannelWithType:GAMECENTER interface:gamecenter];
#endif
    }
    else if([channelType isEqualToString:@"NAVER"])
    {
#if __has_include(<GamePotNaver/GamePotNaver.h>)
        GamePotChannelInterface* naver    = [[GamePotNaver alloc] init];
        [[GamePotChannel getInstance] addChannelWithType:NAVER interface:naver];
#endif
    }
    else if([channelType isEqualToString:@"LINE"])
    {
#if __has_include(<GamePotLine/GamePotLine.h>)
        GamePotChannelInterface* line    = [[GamePotLine alloc] init];
        [[GamePotChannel getInstance] addChannelWithType:NAVER interface:line];
#endif
    }
    else if([channelType isEqualToString:@"TWITTER"])
    {
#if __has_include(<GamePotTwitter/GamePotTwitter.h>)
        GamePotChannelInterface* twitter    = [[GamePotTwitter alloc] init];
        [[GamePotChannel getInstance] addChannelWithType:NAVER interface:twitter];
#endif
    }
    else if([channelType isEqualToString:@"APPLE"])
    {
#if __has_include(<GamePotApple/GamePotApple.h>)
        GamePotChannelInterface* apple    = [[GamePotApple alloc] init];
        [[GamePotChannel getInstance] addChannelWithType:APPLE interface:apple];
#endif
    }
}

- (NSString*) getPurchaseItems
{
   return [[GamePot getInstance] getPurchaseItemsJsonString];
}

- (void) setAdjustData:(NSString*)adjustBillingData
{
    //    #if __has_include(<GamePotAdAdjust/GamePotAdAdjust.h>)
    //    NSData* jsonData = [adjustBillingData dataUsingEncoding:NSUTF8StringEncoding];
    //    adjustBillingDic = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONWritingPrettyPrinted error:nil];
    //    #endif
}

- (void) setPush:(BOOL) pushEnable
{
    [[GamePot getInstance] setPushEnable:pushEnable success:^{
        UnitySendMessage(ListenerForUnity, "onPushSuccess","");
    } fail:^(NSError *error) {
        UnitySendMessage(ListenerForUnity, "onPushFailure", [self getError:error]);
    }];
}

- (void) setPushNight:(BOOL) pushEnable
{
    [[GamePot getInstance] setNightPushEnable:pushEnable success:^{
        UnitySendMessage(ListenerForUnity, "onPushNightSuccess","");
    } fail:^(NSError *error) {
        UnitySendMessage(ListenerForUnity, "onPushNightFailure", [self getError:error]);
    }];
}

- (void) setPushAd:(BOOL) pushEnable
{
    [[GamePot getInstance] setAdPushEnable:pushEnable success:^{
        UnitySendMessage(ListenerForUnity, "onPushAdSuccess","");
    } fail:^(NSError *error) {
        UnitySendMessage(ListenerForUnity, "onPushAdFailure", [self getError:error]);
    }];
}

- (void) setPushStatus:(BOOL) pushEnable setNight:(BOOL) nightPushEnable setAd:(BOOL) adPushEnable
{
    [[GamePot getInstance] setPushStatus:pushEnable night:nightPushEnable ad:adPushEnable success:^{
        UnitySendMessage(ListenerForUnity, "onPushStatusSuccess","");
    } fail:^(NSError *error) {
        UnitySendMessage(ListenerForUnity, "onPushStatusFailure", [self getError:error]);
    }];

}

- (void) showNoticeWebView
{
    dispatch_async(dispatch_get_main_queue(), ^{
        [[GamePot getInstance] showNoticeWebView:UnityGetGLViewController()];
    });
}

- (void) showCSWebView
{
    dispatch_async(dispatch_get_main_queue(), ^{
        [[GamePot getInstance] showHelpWebView:UnityGetGLViewController()];
    });
}

- (void) showWebView:(NSString*)url
{
    dispatch_async(dispatch_get_main_queue(), ^{
        [[GamePot getInstance] showWebView:UnityGetGLViewController() setType:WEBVIEW_NORMALWITHBACK setURL:url];
    });
}

// - (void) setSandbox:(BOOL) enable
// {
// #if __has_include(<GamePotAd/GamePotAd.h>)
//    [[GamePotAd getInstance] setSandbox:enable];
// #endif
// }

- (void) showAppStatus:(NSString*)status
{
    if(status == nil || [status isEqualToString:@""])
    {
        NSLog(@"Status is empty");

        return;
    }

    NSError* error = nil;

    NSDictionary* dic = [NSJSONSerialization JSONObjectWithData:[status dataUsingEncoding:NSUTF8StringEncoding] options:NSJSONReadingMutableContainers error:&error];

    if(error)
    {
        NSLog(@"Update Parsing Error = %@",[error description]);

        return;
    }

    NSString* type = [dic valueForKey:@"type"];

    if(type == nil || [type isEqual:[NSNull null]])
    {
        return;
    }

    NSLog(@"type = %@", type);

    if([type isEqualToString:@"maintenance"])
    {
        NSLog(@"Start MAINTENANCE");
        id objMessage = [dic valueForKey:@"message"];

        if(objMessage != nil && [objMessage isEqual:[NSNull null]] == NO)
        {
            NSString* url = [dic valueForKey:@"url"];

            if(url == nil || [url isEqual:[NSNull null]])
            {
                url = @"";
            }

            GamePotAppStatus* appStatus = [[GamePotAppStatus alloc] init:[dic objectForKey:@"message"] setURL:url];

            [[GamePot getInstance] showAppStatusPopup:UnityGetGLViewController() setAppStatus:appStatus setCloseHandler:^{
                UnitySendMessage(ListenerForUnity, "onAppClose","");
            }];
        }

    }
    else if([type isEqualToString:@"needupdate"])
    {
        NSLog(@"Start NEEDUPDATE");
        id objMessage = [dic valueForKey:@"message"];
        id objCurrentAppVersion = [dic valueForKey:@"currentAppVersion"];
        id objUpdateAppVersion = [dic valueForKey:@"updateAppVersion"];
        id objCurrentAppVersionCode = [dic valueForKey:@"currentAppVersionCode"];
        id objUpdateAppVersionCode = [dic valueForKey:@"updateAppVersionCode"];
        id objIsForce = [dic valueForKey:@"isForce"];
        id objResultPayload = [dic valueForKey:@"resultPayload"];

        if(objMessage != nil && [objMessage isEqual:[NSNull null]] == NO &&
            objCurrentAppVersion != nil && [objCurrentAppVersion isEqual:[NSNull null]] == NO &&
            objUpdateAppVersion != nil && [objUpdateAppVersion isEqual:[NSNull null]] == NO &&
            objCurrentAppVersionCode != nil && [objCurrentAppVersionCode isEqual:[NSNull null]] == NO &&
            objUpdateAppVersionCode != nil && [objUpdateAppVersionCode isEqual:[NSNull null]] == NO &&
            objIsForce != nil && [objIsForce isEqual:[NSNull null]] == NO &&
            objResultPayload != nil && [objResultPayload isEqual:[NSNull null]] == NO)
        {
            GamePotAppStatus* appStatus = [[GamePotAppStatus alloc] init:[dic objectForKey:@"message"]
                                                    setCurrentAppVersion:[dic objectForKey:@"currentAppVersion"]
                                                     setUpdateAppVersion:[dic objectForKey:@"updateAppVersion"]
                                                setCurrentAppVersionCode:[[dic objectForKey:@"currentAppVersionCode"] intValue]
                                                 setUpdateAppVersionCode:[[dic objectForKey:@"updateAppVersionCode"] intValue]
                                                                setForce:[[dic objectForKey:@"isForce"] boolValue]];



            [appStatus setResultPayload:objResultPayload];

            [[GamePot getInstance] showAppStatusPopup:UnityGetGLViewController()
                                         setAppStatus:appStatus
                                      setCloseHandler:^{
                                          UnitySendMessage(ListenerForUnity, "onAppClose","");
                                      } setNextHandler:^(NSObject *resultPayload) {
                                          UnitySendMessage(ListenerForUnity, "onLoginSuccess", [(NSString*)resultPayload UTF8String]);
                                      }];
        }
    }
}

- (void) showAgreeDialog:(NSString*)info
{
    GamePotAgreeOption* option = nil;

    if([GamePotUtil isEmptyString:info])
    {
        option = [[GamePotAgreeOption alloc] init:BLUE];
    }
    else
    {
        NSError* error = nil;

        NSDictionary* dic = [NSJSONSerialization JSONObjectWithData:[info dataUsingEncoding:NSUTF8StringEncoding] options:NSJSONReadingMutableContainers error:&error];

        if(error)
        {
            NSLog(@"Update Parsing Error = %@",[error description]);
            return;
        }

        NSString* theme = [dic valueForKey:@"theme"];

        if([theme isEqualToString:@"bule"])
        {
            option = [[GamePotAgreeOption alloc] init:BLUE];
        }
        else
        {
            option = [[GamePotAgreeOption alloc] init:GREEN];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"contentBackGradient"]] == NO)
        {
            NSString* data = [dic valueForKey:@"contentBackGradient"];

            NSArray* arrData = [data componentsSeparatedByString:@","];

            if([arrData count] >= 2)
            {
                [option setContentBackGradient:@[[self convertNumber:arrData[0]], [self convertNumber:arrData[1]]]];
            }
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"contentCheckColor"]] == NO)
        {
            NSString* data = [dic valueForKey:@"contentCheckColor"];

            [option setContentCheckColor:[[self convertNumber:data] intValue]];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"contentIconColor"]] == NO)
        {
            NSString* data = [dic valueForKey:@"contentIconColor"];

            [option setContentIconColor:[[self convertNumber:data] intValue]];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"contentIconDrawable"]] == NO)
        {
            NSString* data = [dic valueForKey:@"contentIconDrawable"];

            UIImage* img = [UIImage imageNamed:[data stringByAppendingString:@".png"]];

            [option setContentIcon:img];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"contentShowColor"]] == NO)
        {
            NSString* data = [dic valueForKey:@"contentShowColor"];

            [option setContentShowColor:[[self convertNumber:data] intValue]];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"contentTitleColor"]] == NO)
        {
            NSString* data = [dic valueForKey:@"contentTitleColor"];

            [option setContentTitleColor:[[self convertNumber:data] intValue]];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"footerBackGradient"]] == NO)
        {
            NSString* data = [dic valueForKey:@"footerBackGradient"];

            NSArray* arrData = [data componentsSeparatedByString:@","];

            if([arrData count] >= 2)
            {
                [option setFooterBackGradient:@[[self convertNumber:arrData[0]], [self convertNumber:arrData[1]]]];
            }
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"footerButtonGradient"]] == NO)
        {
            NSString* data = [dic valueForKey:@"footerButtonGradient"];

            NSArray* arrData = [data componentsSeparatedByString:@","];

            if([arrData count] >= 2)
            {
                [option setFooterButtonGradient:@[[self convertNumber:arrData[0]], [self convertNumber:arrData[1]]]];
            }
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"footerButtonOutlineColor"]] == NO)
        {
            NSString* data = [dic valueForKey:@"footerButtonOutlineColor"];

            [option setFooterButtonOutlineColor:[[self convertNumber:data] intValue]];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"footerTitle"]] == NO)
        {
            NSString* data = [dic valueForKey:@"footerTitle"];

            [option setFooterTitle:data];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"footerTitleColor"]] == NO)
        {
            NSString* data = [dic valueForKey:@"footerTitleColor"];

            [option setFooterTitleColor:[[self convertNumber:data] intValue]];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"headerBackGradient"]] == NO)
        {
            NSString* data = [dic valueForKey:@"headerBackGradient"];

            NSArray* arrData = [data componentsSeparatedByString:@","];

            if([arrData count] >= 2)
            {
                [option setHeaderBackGradient:@[[self convertNumber:arrData[0]], [self convertNumber:arrData[1]]]];
            }
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"headerBottomColor"]] == NO)
        {
            NSString* data = [dic valueForKey:@"headerBottomColor"];

            [option setHeaderBottomColor:[[self convertNumber:data] intValue]];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"headerIconDrawable"]] == NO)
        {
            NSString* data = [dic valueForKey:@"headerIconDrawable"];

            UIImage* img = [UIImage imageNamed:[data stringByAppendingString:@".png"]];

            [option setHeaderIcon:img];
        }

        NSString* data = [dic valueForKey:@"headerTitle"];

        if(data != nil && [data isEqualToString:@"{none}"] == NO)
        {
            [option setHeaderTitle:data];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"headerTitleColor"]] == NO)
        {
            NSString* data = [dic valueForKey:@"headerTitleColor"];

            [option setHeaderTitleColor:[[self convertNumber:data] intValue]];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"showNightPush"]] == NO)
        {
            NSString* data = [dic valueForKey:@"showNightPush"];

            [option setShowNightPush:[data isEqualToString:@"true"] ? YES:NO];

        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"allMessage"]] == NO)
        {
            NSString* data = [dic valueForKey:@"allMessage"];

            [option setAllMessage:data];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"termMessage"]] == NO)
        {
            NSString* data = [dic valueForKey:@"termMessage"];

            [option setTermMessage:data];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"privacyMessage"]] == NO)
        {
            NSString* data = [dic valueForKey:@"privacyMessage"];

            [option setPrivacyMessage:data];
        }

        if([GamePotUtil isEmptyString:[dic valueForKey:@"nightPushMessage"]] == NO)
        {
            NSString* data = [dic valueForKey:@"nightPushMessage"];

            [option setNightPushMessage:data];
        }
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        [[GamePot getInstance] showAgreeView:UnityGetGLViewController() option:option handler:^(GamePotAgreeInfo *result) {

            if(result != nil)
            {
                UnitySendMessage(ListenerForUnity, "onAgreeDialogSuccess", [[result toJsonString] UTF8String]);
            }
            else
            {
                NSMutableDictionary* details = [[NSMutableDictionary alloc] init];
                [details setObject:@"Agree Error" forKey:NSLocalizedDescriptionKey];

                NSError* error = [[NSError alloc] initWithDomain:@"GamePot" code:-1000 userInfo:details];
                UnitySendMessage(ListenerForUnity, "onAgreeDialogFailure", [self getError:error] );
            }

        }];
    });
}

- (NSNumber*) convertNumber:(NSString*)_string
{
    unsigned color = 0;

    [[NSScanner scannerWithString:_string] scanHexInt:&color];

    NSNumber* resultColor = [[NSNumber alloc] initWithUnsignedInt:color];

    return resultColor;
}

- (void)showTerms
{
    [[GamePot getInstance] showTerms:UnityGetGLViewController()];
}

- (void)showPrivacy
{
    [[GamePot getInstance] showPrivacy:UnityGetGLViewController()];
}

- (void)sendLog:(NSString*)type errCode:(NSString*)errCode errMessage:(NSString*)errMessage
{
#if __has_include(<GamePotLogger/GamePotLogger.h>)
    if([type isEqualToString:@"d"])
    {
        [GamePotLogger debug:errCode withMessage:errMessage];
    }
    else if([type isEqualToString:@"i"])
    {
        [GamePotLogger info:errCode withMessage:errMessage];
    }
    else if([type isEqualToString:@"w"])
    {
        [GamePotLogger warn:errCode withMessage:errMessage];
    }
    else if([type isEqualToString:@"e"])
    {
        [GamePotLogger error:errCode withMessage:errMessage];
    }
#endif

}

- (void)showNotice
{
    dispatch_async(dispatch_get_main_queue(), ^{
        [[GamePot getInstance] showNotice:UnityGetGLViewController() setSchemeHandler:^(NSString *scheme) {
            UnitySendMessage(ListenerForUnity, "onReceiveScheme", [scheme UTF8String]);
        }];
    });
}

- (void)showNotice:(BOOL)showTodayButton
{
    dispatch_async(dispatch_get_main_queue(), ^{
    [[GamePot getInstance] showNotice:UnityGetGLViewController() setShowTodayButton:showTodayButton setSchemeHandler:^(NSString *scheme) {
         UnitySendMessage(ListenerForUnity, "onReceiveScheme", [scheme UTF8String]);
    } setExitHandler:nil];
        });
}

- (void)showFaq
{
    dispatch_async(dispatch_get_main_queue(), ^{
        [[GamePot getInstance] showFAQWebView:UnityGetGLViewController()];
    });

}

- (BOOL)characterInfo:(NSString *)info;
{
     if(info == nil || [info isEqualToString:@""])
    {
        NSLog(@"Info is empty");
        return NO;
    }

#if __has_include(<Gamepot/GamePotSendLog.h>)
    NSError* error = nil;
    NSDictionary* dic = [NSJSONSerialization JSONObjectWithData:[info dataUsingEncoding:NSUTF8StringEncoding] options:NSJSONReadingMutableContainers error:&error];

    if(error)
    {
        NSLog(@"Update Parsing Error = %@",[error description]);
        
        return NO;
    }

    GamePotSendLogCharacter* body = [[GamePotSendLogCharacter alloc] init];
    
    for(id key in dic)
    {
        [body put:[dic valueForKey:key] forKey:key];
        NSLog(@"value: %@, Key: %@", [dic valueForKey:key], key);
    }

    BOOL result = [GamePotSendLog characterInfo:body];
    
    return result;
#else
    return NO;
#endif
}

- (void)setLoggerUserid:(NSString *)userid
{
    #if __has_include(<GamePotLogger/GamePotLogger.h>)
    [GamePotLogger setUserId:userid];
    #endif
}
@end
