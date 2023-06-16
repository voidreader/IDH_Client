#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <GamePotChannel/GamePotChannel.h>

#define GAMEPOTGAMECENTER_VERSION @"1.0.1"

//! Project version number for ItsGameCenterChannel.
FOUNDATION_EXPORT double GamePotGameCenterVersionNumber;

//! Project version string for ItsGameCenterChannel.
FOUNDATION_EXPORT const unsigned char GamePotGameCenterVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <ItsGameCenterChannel/PublicHeader.h>

@interface GamePotGameCenter : GamePotChannelInterface

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

- (void)doShowLeaderboard:(UIViewController *)_viewController;
- (void)doShowAchievement:(UIViewController *)_viewController;
- (void)doLoadAchievement:(GamePotChannelManagerHandler)_handler;
- (void)doLeaderboardSubmitScore:(NSString *)_leaderboardId score:(int)_score handler:(GamePotChannelManagerHandler)_handler;
- (void)doAchievementSubmitPercent:(NSString *)_achievementId percent:(int)_percent handler:(GamePotChannelManagerHandler)_handler;
@end
