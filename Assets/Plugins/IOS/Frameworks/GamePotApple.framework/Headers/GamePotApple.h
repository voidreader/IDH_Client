//
//  GamePotApple.h
//  GamePotApple
//
//  Created by Lee Chungwon on 2019/10/18.
//  Copyright © 2019 io.gamepot. All rights reserved.
//

#import <Foundation/Foundation.h>

#import <GamePotChannel/GamePotChannel.h>

#define GAMEPOTAPPLE_VERSION @"1.0.1"

//! Project version number for GamePotApple.
FOUNDATION_EXPORT double GamePotAppleVersionNumber;

//! Project version string for GamePotApple.
FOUNDATION_EXPORT const unsigned char GamePotAppleVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <GamePotApple/PublicHeader.h>
@interface GamePotApple : GamePotChannelInterface

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


