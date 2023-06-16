#import <Foundation/Foundation.h>
#import <GamePotChannel/GamePotChannel.h>
#import <UIKit/UIKit.h>

#define GAMEPOTGOOGLE_VERSION @"1.0.2"

//! Project version number for GoogleSignInChannel.
FOUNDATION_EXPORT double GamePotGoogleSignInVersionNumber;

//! Project version string for GoogleSignInChannel.
FOUNDATION_EXPORT const unsigned char GamePotGoogleSignInVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <GoogleSignInChannel/PublicHeader.h>

@interface GamePotGoogleSignIn : GamePotChannelInterface

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
