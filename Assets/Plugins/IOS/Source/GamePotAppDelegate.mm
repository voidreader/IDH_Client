//
//  GamePotAppDelegate.mm
//  GamePotSample
//
//  Created by Danial on 2018. 4. 20..
//  Copyright © 2018년 itsB. All rights reserved.
//

#import "GamePotAppDelegate.h"
#import <GamePot/GamePot.h>
#import <GamePotChannel/GamePotChannel.h>
#import <UserNotifications/UserNotifications.h>

#if __has_include(<AdBrixRm/AdBrixRM-Swift.h>)
#include <AdBrixRm/AdBrixRM-Swift.h>
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

#if __has_include(<GamePotTwitter/GamePotTwitter.h>)
#import <GamePotTwitter/GamePotTwitter.h>
#endif

#if __has_include(<GamePotLine/GamePotLine.h>)
#import <GamePotLine/GamePotLine.h>
#endif

#if __has_include(<GamePotApple/GamePotApple.h>)
#import <GamePotApple/GamePotApple.h>
#endif


#if __has_include(<GamePotAd/GamePotAd.h>)
#import <GamePotAd/GamePotAd.h>
#endif
#if __has_include(<GamePotAdFacebook/GamePotAdFacebook.h>)
#import <GamePotAdFacebook/GamePotAdFacebook.h>
#endif
#if __has_include(<GamePotAdAdjust/GamePotAdAdjust.h>)
#import <GamePotAdAdjust/GamePotAdAdjust.h>
#endif
#if __has_include(<GamePotNaverCafe/GamePotNaverCafe.h>)
#import <GamePotNaverCafe/GamePotNaverCafe.h>
#endif
#if __has_include(<GamePotAdAdbrix/GamePotAdAdbrix.h>)
#import <GamePotAdAdbrix/GamePotAdAdbrix.h>
#endif

#if __has_include(<GamePotLogger/GamePotLogger.h>)
#import <GamePotLogger/GamePotLogger.h>
#endif

@interface GamePotAppDelegate ()<UNUserNotificationCenterDelegate>

@end

@implementation GamePotAppDelegate

#define SYSTEM_VERSION_GRATERTHAN_OR_EQUALTO(v)  ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] != NSOrderedAscending)
- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    // Override point for customization after application launch.
    [super application:application didFinishLaunchingWithOptions:launchOptions];

    // GamePot 초기화
    [[GamePot getInstance] setup];

    // AD 모듈 설정

#if __has_include(<GamePotAd/GamePotAd.h>)
#if __has_include(<GamePotAdFacebook/GamePotAdFacebook.h>)
    GamePotAdInterface* adFacebook = [[GamePotAdFacebook alloc] init];
    [[GamePotAd getInstance] addAds:adFacebook];
#endif
#if __has_include(<GamePotAdAdjust/GamePotAdAdjust.h>)
    GamePotAdInterface* adAdjust = [[GamePotAdAdjust alloc] init];
    [[GamePotAd getInstance] addAds:adAdjust];
#endif
#if __has_include(<GamePotAdAdbrix/GamePotAdAdbrix.h>)
    GamePotAdInterface* adAdbrix = [[GamePotAdAdbrix alloc] init];
    [[GamePotAd getInstance] addAds:adAdbrix];
#endif
#endif



    // Channel 모듈 설정
#if __has_include(<GamePotFacebook/GamePotFacebook.h>)
    GamePotChannelInterface* facebook = [[GamePotFacebook alloc] init];
    [[GamePotChannel getInstance] addChannelWithType:FACEBOOK interface:facebook];
#endif
#if __has_include(<GamePotGoogleSignIn/GamePotGoogleSignIn.h>)
    GamePotChannelInterface* google   = [[GamePotGoogleSignIn alloc] init];
    [[GamePotChannel getInstance] addChannelWithType:GOOGLE interface:google];
#endif
#if __has_include(<GamePotGameCenter/GamePotGameCenter.h>)
    GamePotChannelInterface* gamecenter = [[GamePotGameCenter alloc] init];
    [[GamePotChannel getInstance] addChannelWithType:GAMECENTER interface:gamecenter];
#endif
#if __has_include(<GamePotNaver/GamePotNaver.h>)
    GamePotChannelInterface* naver    = [[GamePotNaver alloc] init];
    [[GamePotChannel getInstance] addChannelWithType:NAVER interface:naver];
#endif

#if __has_include(<GamePotTwitter/GamePotTwitter.h>)
    GamePotChannelInterface* twitter    = [[GamePotTwitter alloc] init];
    [[GamePotChannel getInstance] addChannelWithType:TWITTER interface:twitter];
#endif

#if __has_include(<GamePotLine/GamePotLine.h>)
    GamePotChannelInterface* line    = [[GamePotLine alloc] init];
    [[GamePotChannel getInstance] addChannelWithType:LINE interface:line];
#endif

#if __has_include(<GamePotApple/GamePotApple.h>)
    GamePotChannelInterface* apple    = [[GamePotApple alloc] init];
    [[GamePotChannel getInstance] addChannelWithType:APPLE interface:apple];
#endif

    // GamePot에 상태값 전달
    [[GamePotChannel getInstance] application:application didFinishLaunchingWithOptions:launchOptions];

    // 네이버 카페 초기화
#if __has_include(<GamePotNaverCafe/GamePotNaverCafe.h>)
    [[GamePotNaverCafe getInstance] setup];
#endif

#if __has_include(<GamePotLogger/GamePotLogger.h>)
    [GamePotLogger init];
#endif
    // Push Permission
    if(SYSTEM_VERSION_GRATERTHAN_OR_EQUALTO(@"10.0"))
    {
        UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
        center.delegate = self;
        [center requestAuthorizationWithOptions:(UNAuthorizationOptionSound | UNAuthorizationOptionAlert | UNAuthorizationOptionBadge) completionHandler:^(BOOL granted, NSError * _Nullable error){
            if(!error){
                dispatch_async(dispatch_get_main_queue(), ^{
                    [[UIApplication sharedApplication] registerForRemoteNotifications];
                });
            }
        }];
    }
    else
    {
        // Code for old versions
        UIUserNotificationType allNotificationTypes = (UIUserNotificationTypeSound | UIUserNotificationTypeAlert | UIUserNotificationTypeBadge);
        UIUserNotificationSettings *settings = [UIUserNotificationSettings settingsForTypes:allNotificationTypes categories:nil];
        [application registerUserNotificationSettings:settings];
        [application registerForRemoteNotifications];
    }
    return YES;
}

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options
{
#if __has_include(<AdBrixRm/AdBrixRM-Swift.h>)
    AdBrixRM *adBrix = [AdBrixRM sharedInstance];
    [adBrix deepLinkOpenWithUrl:url];
#endif

#if __has_include(<GamePotNaverCafe/GamePotNaverCafe.h>)

    BOOL GamePotChannelManagerResult = [[GamePotChannel getInstance] application:app openURL:url options:options];
    BOOL GamePotNaverCafeResult = [[GamePotNaverCafe getInstance] application:app openURL:url options:options];

    #if __has_include(<GamePotAd/GamePotAd.h>)
        [[GamePotAd getInstance] application:app openURL:url options:options];
    #endif

    return GamePotChannelManagerResult || GamePotNaverCafeResult;

#else

    BOOL GamePotChannelManagerResult = [[GamePotChannel getInstance] application:app openURL:url options:options];

    #if __has_include(<GamePotAd/GamePotAd.h>)
        [[GamePotAd getInstance] application:app openURL:url options:options];
    #endif

    return GamePotChannelManagerResult;

#endif

}

- (void)applicationWillResignActive:(UIApplication *)application {
    // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
    // Use this method to pause ongoing tasks, disable timers, and invalidate graphics rendering callbacks. Games should use this method to pause the game.
    [super applicationWillResignActive:application];
}

- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken
{
    [[GamePot getInstance] handleRemoteNotificationsWithDeviceToken:deviceToken];
}

- (void)applicationDidEnterBackground:(UIApplication *)application {
    // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later.
    // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
    [[GamePotChat getInstance] stop];
#if __has_include(<GamePotAd/GamePotAd.h>)
    [[GamePotAd getInstance] tracking:PAUSE];
#endif
    [super applicationDidEnterBackground:application];
}


- (void)applicationWillEnterForeground:(UIApplication *)application {
    // Called as part of the transition from the background to the active state; here you can undo many of the changes made on entering the background.

    [[GamePotChat getInstance] start];
#if __has_include(<GamePotAd/GamePotAd.h>)
    [[GamePotAd getInstance] tracking:RESUME];
#endif
    //[super applicationWillEnterForeground:application];
}


- (void)applicationDidBecomeActive:(UIApplication *)application {
    // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
    [super applicationDidBecomeActive:application];
}


- (void)applicationWillTerminate:(UIApplication *)application {
    // Called when the application is about to terminate. Save data if appropriate. See also applicationDidEnterBackground:.
    //[super applicationWillTerminate:application];
}

#pragma mark - UNUserNotificationCenterDelegate
- (void)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions))completionHandler
{
    NSLog(@"User Info : %@",notification.request.content.userInfo);
    //    completionHandler(UNAuthorizationOptionSound | UNAuthorizationOptionAlert | UNAuthorizationOptionBadge);
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void (^)(void))completionHandler
{
    NSLog(@"User Info : %@",response.notification.request.content.userInfo);
    completionHandler();
}

#pragma GamePotDelegate
@end

IMPL_APP_CONTROLLER_SUBCLASS(GamePotAppDelegate);
