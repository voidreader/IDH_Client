#ifndef GamePotHandler_h
#define GamePotHandler_h

#import "GamePotAppStatus.h"
#import "GamePotAgreeInfo.h"


typedef void (^GamePotCommonSuccess) (void);
typedef void (^GamePotCommonFail) (NSError* error);

typedef void (^GamePotPurchaseSuccess) (void);
typedef void (^GamePotPurchaseCancel) (void);
typedef void (^GamePotPurchaseFail) (NSError* _error);
//typedef void (^NChargeHandler) (BOOL _success, NSError* _error);

typedef void (^GamePotCouponHandler) (BOOL _success, NSError* _error);
typedef void (^GamePotPushHandler) (BOOL _success, NSError* _error);

typedef void (^GamePotInnerHandler) (BOOL _success, id _data, NSError* _error);

typedef void (^GamePotAppStatusNeedUpdateHandler)(GamePotAppStatus* status);
typedef void (^GamePotAppStatusMaintenanceHandler)(GamePotAppStatus* status);
typedef void (^GamePotAppStatusCloseHandler)(void);
typedef void (^GamePotAppStatusNextHandler)(NSObject* resultPayload);

typedef void (^ExitWebviewCompletionHandler)(void);
typedef void (^ExitNoticeCompeltionHandler)(void);

typedef void (^GamePotAgreeHandler) (GamePotAgreeInfo* result);
typedef void (^GamePotSchemeHandler) (NSString* scheme);

#endif
