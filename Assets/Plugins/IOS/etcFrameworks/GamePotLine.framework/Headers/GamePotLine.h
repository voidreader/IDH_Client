//
//  GamePotLine.h
//  GamePotLine
//
//  Created by Lee Chungwon on 07/05/2019.
//  Copyright © 2019 io.gamepot. All rights reserved.
//
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <GamePotChannel/GamePotChannel.h>

#define GAMEPOTLINE_VERSION @"1.0.0"
//! Project version number for GamePotLine.
FOUNDATION_EXPORT double GamePotLineVersionNumber;

//! Project version string for GamePotLine.
FOUNDATION_EXPORT const unsigned char GamePotLineVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <GamePotLine/PublicHeader.h>


@interface GamePotLine : GamePotChannelInterface
    
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
