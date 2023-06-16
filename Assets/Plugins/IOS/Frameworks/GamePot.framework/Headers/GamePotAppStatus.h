//
//  GamePotAppStatus.h
//  GamePot
//
//  Created by Lee Chungwon on 2018. 9. 26..
//  Copyright © 2018년 itsB. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef NS_ENUM (NSInteger, GamePotStatus)
{
    MAINTENANCE = 0,
    NEEDUPDATE = 1
};

@interface GamePotAppStatus : NSObject

@property(nonatomic) GamePotStatus type;
@property(nonatomic) NSString* message;
@property(nonatomic) NSString* url;
@property(nonatomic) NSString* currentAppVersion;
@property(nonatomic) NSString* updateAppVersion;
@property(nonatomic) int currentAppVersionCode;
@property(nonatomic) int updateAppVersionCode;
@property(nonatomic) BOOL isForce;
@property(nonatomic) NSObject* resultPayload;

- (instancetype)init:(NSString*)_message setURL:(NSString*)_url;

- (instancetype)init:(NSString*)_message
setCurrentAppVersion:(NSString*)_currentAppVersion
 setUpdateAppVersion:(NSString*)_updateAppVersion
setCurrentAppVersionCode:(int)_currentAppVersionCode
setUpdateAppVersionCode:(int)_updateAppVersionCode
setForce:(BOOL)_force;

- (NSString*) toJsonString;

- (NSDictionary*) toDictonary;

- (NSString*) toString;

@end

