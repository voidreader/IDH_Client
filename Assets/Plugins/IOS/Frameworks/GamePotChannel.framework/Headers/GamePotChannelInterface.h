//
//  GamePotChannelInterface.h
//  ChannelBase
//
//  Created by Danial on 2018. 4. 20..
//  Copyright © 2018년 All rights reserved.
//
#ifndef GamePotChannelInterface_h
#define GamePotChannelInterface_h

#import <Foundation/Foundation.h>
#import <GamePot/GamePot.h>
#import <UIKit/UIKit.h>
#import "GamePotChannelHandler.h"

@interface GamePotChannelInterface:NSObject

- (void) doInit;

- (void) doLoginWithViewController:(UIViewController*)_viewController
                           success:(GamePotChannelManagerSuccess)_success
                            cancel:(GamePotChannelManagerCancel)_cancel
                              fail:(GamePotChannelManagerFail)_fail;

- (void) doLocalUser:(GamePotChannelManagerSuccess)_success fail:(GamePotChannelManagerFail)_fail;

- (BOOL) doValidLogin;

- (void) doLogout:(GamePotCommonSuccess)_success fail:(GamePotCommonFail)_fail;

- (void) doUnregister;

- (BOOL) doOpenURL;

- (void) doShowAchievement:(UIViewController*)_viewController;
- (void) doShowLeaderboard:(UIViewController*)_viewController;
- (void) doLoadAchievement:(GamePotChannelManagerHandler)_handler;
- (void) doAchievementSubmitPercent:(NSString *)_achievementId percent:(int)_percent handler:(GamePotChannelManagerHandler)_handler;
- (void) doLeaderboardSubmitScore:(NSString *)_leaderboardId score:(int)_score handler:(GamePotChannelManagerHandler)_handler;


- (void) application:(UIApplication*)_application didFinishLaunchingWithOptions:(NSDictionary*)_launchOptions;

- (BOOL) application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options;

@end
#endif
