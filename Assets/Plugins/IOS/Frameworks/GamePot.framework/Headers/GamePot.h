#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "GamePot.h"
#import "GamePotHttpClient.h"
#import "GamePotHandler.h"
#import "GamePotLog.h"
#import "GamePotGraphQLRequest.h"
#import "GamePotSharedPref.h"
#import "GamePotUtil.h"
#import "GamePotChat.h"
#import "SystemServices.h"
#import "GamePotError.h"
#import "GamePotPurchaseInfo.h"
#import "GamePotWebView.h"
#import "GamePotAgreeView.h"
#import "UIView+Toast.h"
//#import "GamePotLocaleManager.h"

/* ==========================================================================
 * 2.0.0
 - Release
 - 캐릭터 정보 전달 기능 추가 GamePotSendLogCharacter
 
 * 2.0.1
 - AppleID 기능 추가
 - UIWebView 관련 문제 수정

 * 2.0.2
 - 결제 스토어 추가 (MyCard, Galaxy Store)
 - SDK 언어 추가 [한국어, 영어, 이탈리아어, 태국어, 베트남어, 일본어, 중문(번체, 간체), 인도네시아어, 독일어, 스페인어, 프랑스어]
============================================================================= */
#define GAMEPOT_VERSION @"2.0.2"

//! Project version number for Common.
FOUNDATION_EXPORT double GamePotVersionNumber;

//! Project version string for Common.
FOUNDATION_EXPORT const unsigned char GamePotVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <Common/PublicHeader.h>

//#define SHARED_KEY_LINKING_ID @"GamePotLinkingId"
#define SHARED_KEY_GUEST_ID @"GamePotGuestId"
#define SHARED_KEY_GUEST_PASSWORD @"GamePotGuestPassword"
#define SHARED_KEY_LAST_LOGIN_TYPE @"GamePotLastLoginType"
#define SHARED_KEY_LICENSE_CACHE @"GamePotLicenseCache"
#define SHARED_KEY_SOCKET_CACHE @"GamePotSocketCache"
#define SHARED_KEY_CHECK_ONEDAY @"GamePotCheckOneDay"


@protocol GamePotPurchaseDelegate<NSObject>
@required
- (void) GamePotPurchaseSuccess:(GamePotPurchaseInfo*)_info;
- (void) GamePotPurchaseFail:(NSError*)_error;
- (void) GamePotPurchaseCancel;
@end

@interface GamePot : NSObject

@property (nonatomic) id purchaseDelegate;

+ (GamePot*) getInstance;

- (void) setup;

- (void) setupWithAppStatus:(GamePotAppStatusNeedUpdateHandler)_update
             setMaintenance:(GamePotAppStatusMaintenanceHandler)_maintenance;

- (NSArray*) getConfigs;
- (NSString*) getConfig:(NSString*)_key;

- (NSString*) getMemberId;
- (NSString*) getToken;

- (NSArray*) getPurchaseItems;

- (NSString*) getPurchaseItemsJsonString;

- (void) setLanguage:(NSString*)_language;

//! 결제
- (void) purchase:(NSString*)_productId;


//! unique id 포함한 결제
- (void) purchase:(NSString*)_productId uniqueId:(NSString*)_uniqueId;

- (NSArray*) getDetails;
- (NSString*) getLocalizePrice:(NSString*)_productId;
- (void) coupon:(NSString*)_couponNumber handler:(GamePotCouponHandler)_handler;
- (void) coupon:(NSString*)_couponNumber userData:(NSString*)_userData handler:(GamePotCouponHandler)_handler;

//! Push 상태 설정
- (void) setPushEnable:(BOOL)_pushEnable success:(GamePotCommonSuccess)_success fail:(GamePotCommonFail)_fail;

//! 야간 Push 상태 설정
- (void) setNightPushEnable:(BOOL)_pushEnable success:(GamePotCommonSuccess)_success fail:(GamePotCommonFail)_fail;

//! 광고 Push 상태 설정
- (void) setAdPushEnable:(BOOL)_pushEnable success:(GamePotCommonSuccess)_success fail:(GamePotCommonFail)_fail;

- (void) setPushStatus:(BOOL)_pushEnable
                 night:(BOOL)_nightPushEnable
                    ad:(BOOL)_adPushEnable
               success:(GamePotCommonSuccess)_success
                  fail:(GamePotCommonFail)_fail;

//! Push 상태
- (BOOL) getPushEnable;

//! 야간 Push 상태
- (BOOL) getNightPushEnable;

//! 광고 Push 상태
- (BOOL) getAdPushEnable;

//! 전체 Push 상태를 NSDictionary 타입으로 리턴
- (NSDictionary*) getPushStatus;

//! 전체 Push 상태를 JsonString 타입으로 리턴
- (NSString*) getPushStatusJsonString;

- (void)openAppStore;

- (void) checkAppStatus:(GamePotCommonSuccess)_success
         setFailHandler:(GamePotCommonFail)_fail
       setUpdateHandler:(GamePotAppStatusNeedUpdateHandler)_updateHandler
  setMaintenanceHandler:(GamePotAppStatusMaintenanceHandler)_maintenanceHandler;

- (void) showAppStatusPopup:(UIViewController*)_viewController
               setAppStatus:(GamePotAppStatus*)_appStatus
            setCloseHandler:(GamePotAppStatusCloseHandler)_closeHandler
             setNextHandler:(GamePotAppStatusNextHandler)_nextHandler;

- (void) showAppStatusPopup:(UIViewController*)_viewController
               setAppStatus:(GamePotAppStatus*)_appStatus
            setCloseHandler:(GamePotAppStatusCloseHandler)_closeHandler;

- (int) sendLocalPush:(NSString*)_title setMessage:(NSString*)_message setDateString:(NSString*)_date;

- (void) cancelLocalPush:(int)_id;

- (void)showNoticeWebView:(UIViewController*)_viewController;

- (void)showNotice:(UIViewController*)_viewController
  setSchemeHandler:(GamePotSchemeHandler)_schemeHandler;

- (void)showNotice:(UIViewController *)_viewController
  setSchemeHandler:(GamePotSchemeHandler)_schemeHandler
    setExitHandler:(ExitNoticeCompeltionHandler)_exitHandler;

- (void)showNotice:(UIViewController*)_viewController
setShowTodayButton:(BOOL)_showTodayButton
  setSchemeHandler:(GamePotSchemeHandler)_schemeHandler
    setExitHandler:(ExitNoticeCompeltionHandler)_exitHandler;


- (void)showHelpWebView:(UIViewController*)_viewController;
- (void)showWebView:(UIViewController*)_viewController setType:(WEBVIEW_TYPE)_type setURL:(NSString*)_url;
- (void)setWebViewExitHandler:(ExitWebviewCompletionHandler)_handler;
- (void)showFAQWebView:(UIViewController*)_viewController;

- (void)showTerms:(UIViewController*)_viewController;
- (void)showPrivacy:(UIViewController*)_viewController;

- (void)showAgreeView:(UIViewController*)_viewController option:(GamePotAgreeOption*)_option handler:(GamePotAgreeHandler)_handler;

//! 사용하지 말것
- (void) doDeleteMember:(GamePotInnerHandler)_handler;
- (void) doCreateLinking:(NSString*)_userName
                password:(NSString*)_password
                provider:(NSString*)_provider
                 handler:(GamePotInnerHandler)_handler;

- (void) doSignOut:(GamePotInnerHandler)_handler;
- (void) doLinkingByUser:(NSString*)_userName provider:(NSString*)_provider token:(NSString*)_token handler:(GamePotInnerHandler)_handler;
- (void) doDeleteLinking:(NSString*)_provider setHandler:(GamePotInnerHandler)_handler;
- (void) doGuest:(GamePotInnerHandler)_handler;
- (BOOL) doLinked:(NSString*)_providerType;
- (void) startOperation:(NSOperation*)_operation;

- (void) handleRemoteNotificationsWithDeviceToken:(NSData *)deviceToken;

- (BOOL) isReady;

- (NSString*) getChannelToken;
- (NSString*) getChannel;
- (NSString*) getChannelId;

- (void) setupWithSetProject:(NSString*)_projectId setLincens:(NSString*)_licensURL;
- (void) setBeta:(BOOL)_betaEnable;

@end
