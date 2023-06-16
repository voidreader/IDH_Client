//
//  GamePotTwitter.h
//  GamePotTwitter
//
//  Created by Lee Chungwon on 08/05/2019.
//  Copyright © 2019 io.gamepot. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <GamePotChannel/GamePotChannel.h>

#define GAMEPOTTWITTER_VERSION @"1.0.0"

//! Project version number for GamePotTwitter.
FOUNDATION_EXPORT double GamePotTwitterVersionNumber;

//! Project version string for GamePotTwitter.
FOUNDATION_EXPORT const unsigned char GamePotTwitterVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <GamePotTwitter/PublicHeader.h>

@interface GamePotTwitter : GamePotChannelInterface

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

