
#ifndef GamePotChannelHandler_h
#define GamePotChannelHandler_h
#import <Foundation/Foundation.h>
#import "GamePotUserInfo.h"

typedef void(^GamePotChannelManagerSuccess) (GamePotUserInfo* userInfo);
typedef void(^GamePotChannelManagerCancel) (void);
typedef void(^GamePotChannelManagerFail) (NSError* error);
typedef void(^GamePotChannelAppStatus) (GamePotAppStatus* appStatus);

typedef void(^GamePotChannelManagerHandler) (id data, NSError* error);

#endif
