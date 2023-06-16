//
//  GamePotFacebook.h
//  GamePotFacebook
//
//  Created by Danial on 2018. 4. 20..
//  Copyright © 2018년 All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <GamePotChannel/GamePotChannel.h>

#define GAMEPOTFACEBOOK_VERSION @"1.0.1"

//! Project version number for FacebookChannel.
FOUNDATION_EXPORT double GamePotFacebookVersionNumber;

//! Project version string for FacebookChannel.
FOUNDATION_EXPORT const unsigned char GamePotFacebookVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <FacebookChannel/PublicHeader.h>

@interface GamePotFacebook : GamePotChannelInterface

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
