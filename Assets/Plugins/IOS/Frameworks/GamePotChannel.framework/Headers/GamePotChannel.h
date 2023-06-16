//
//  GamePotChannel.h
//  GamePotChannel
//
//  Created by Danial on 2018. 4. 20..
//  Copyright © 2018년 All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <GamePot/GamePot.h>
#import "GamePotChannelType.h"
#import "GamePotChannelInterface.h"
#import "GamePotChannelHandler.h"
#import "GamePotChannelError.h"
#import "GamePotUserInfo.h"


#define GAMEPOTCHANNEL_VERSION @"1.0.1"

//! Project version number for ChannelBase.
FOUNDATION_EXPORT double GamePotChannelVersionNumber;

//! Project version string for ChannelBase.
FOUNDATION_EXPORT const unsigned char GamePotChannelVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <ChannelBase/PublicHeader.h>

@interface GamePotChannel : NSObject

@property (nonatomic, assign, getter=enableGameCenter, setter=enableGameCenter:) BOOL enableGameCenter;

+ (GamePotChannel*) getInstance;

- (void) application:(UIApplication*)_application didFinishLaunchingWithOptions:(NSDictionary*)_launchOptions;

- (BOOL) application:(UIApplication *)_application openURL:(NSURL *)_url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)_options;

- (void) addChannelWithType:(GamePotChannelType)_type interface:(GamePotChannelInterface*)_interface;

- (void) Login:(GamePotChannelType)_type
viewController:(UIViewController*)_viewController
       success:(GamePotChannelManagerSuccess)_success
        cancel:(GamePotChannelManagerCancel)_cancel
          fail:(GamePotChannelManagerFail)_fail;

- (void) Login:(GamePotChannelType)_type
viewController:(UIViewController*)_viewController
       success:(GamePotChannelManagerSuccess)_success
        cancel:(GamePotChannelManagerCancel)_cancel
          fail:(GamePotChannelManagerFail)_fail
        update:(GamePotChannelAppStatus)_update
   maintenance:(GamePotChannelAppStatus)_maintenance;


- (void) LogoutWithSuccess:(GamePotCommonSuccess)_success fail:(GamePotCommonFail)_fail;

- (void) CreateLinking:(GamePotChannelType)_type
        viewController:(UIViewController*)_viewController
               success:(GamePotChannelManagerSuccess)_success
                cancel:(GamePotChannelManagerCancel)_cancel
                  fail:(GamePotChannelManagerFail)_fail;

- (void) DeleteLinking:(GamePotChannelType)_type success:(void(^)(void))_success fail:(void(^)(NSError* error))_fail;

- (void) DeleteMemberWithSuccess:(GamePotCommonSuccess)_success fail:(GamePotCommonFail)_fail;

- (GamePotChannelType) lastLoginType;

- (BOOL) isLinked:(GamePotChannelType)_type;

- (NSArray*)getLinkedList;
- (NSString*)getLinkedListJsonString;

- (void) showAchievement:(UIViewController*)_viewController;
- (void) showLeaderboard:(UIViewController*)_viewController;
- (void) loadAchievement:(GamePotChannelManagerHandler)_handler;
- (void) achievementSubmitPercent:(NSString *)_achievementId percent:(int)_percent handler:(GamePotChannelManagerHandler)_handler;
- (void) leaderboardSubmitScore:(NSString *)_leaderboardId score:(int)_score handler:(GamePotChannelManagerHandler)_handler;

@end

