//
//  GamePotNaver.h
//  GamePotNaver
//
//  Created by Danial on 2018. 7. 26..
//  Copyright © 2018년 Danial. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <GamePotChannel/GamePotChannel.h>

#define GAMEPOTNAVER_VERSION @"1.0.1"

//! Project version number for NNaverChannel.
FOUNDATION_EXPORT double GamePotNaverVersionNumber;

//! Project version string for NNaverChannel.
FOUNDATION_EXPORT const unsigned char GamePotNaverVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <NNaverChannel/PublicHeader.h>

@interface GamePotNaver : GamePotChannelInterface

- (void)doInit;

- (BOOL)doValidLogin;

- (void)doLoginWithViewController:(UIViewController*)_viewController
                          success:(GamePotChannelManagerSuccess)_success
                           cancel:(GamePotChannelManagerCancel)_cancel
                             fail:(GamePotChannelManagerFail)_fail;

- (void)doLocalUser:(GamePotChannelManagerSuccess)_success fail:(GamePotChannelManagerFail)_fail;

- (void)doLogout:(GamePotCommonSuccess)_success fail:(GamePotCommonFail)_fail;

- (void)application:(UIApplication*)_application didFinishLaunchingWithOptions:(NSDictionary*)_launchOptions;

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options;

@end

