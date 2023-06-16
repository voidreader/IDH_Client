//
//  Nelo2SDK for iOS
//
//  Created by NHN Business Platform.
//  Copyright (c) 2012 NHN Business Platform. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "NeloLogProtocol.h"
#import "NeloSendMode.h"


@class NeloLogInstance;
@class NeloBlockQueue;
@class PLCrashReporter;
@class NeloReachability;

//fir test
@class AppleReachability;
//@class AFNetworkReachabilityManager;

#ifndef NELO2_NELO_h
#define NELO2_NELO_h

// default instance name
#define NELO2_DEFAULT_INSTANCE_NAME @"NELO2_DEFAULT_INSTANCE"
// sendmode
#define NELO2_SENDMODE_ALL @"ALL"
#define NELO2_SENDMODE_WIFI @"WIFI"
#define NELO2_SENDMODE_SESSION @"SESSION"
// loglevel
#define NELO2_LOGLEVEL_DEBUG @"DEBUG"
#define NELO2_LOGLEVEL_INFO @"INFO"
#define NELO2_LOGLEVEL_WARN @"WARN"
#define NELO2_LOGLEVEL_ERROR @"ERROR"
#define NELO2_LOGLEVEL_FATAL @"FATAL"




#endif

@interface NeloLog : NSObject
+ (bool) init:(NSString *)server onPort:(int)port ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion;
+ (bool) init:(NSString *)server onPort:(int)port ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion forUserId:(NSString*)userId;
+ (bool) init:(NSString *)server onPort:(int)port byProtocol:(NeloLogProtocol)protocol ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion forUserId:(NSString*)userId;

+ (bool) initAndStartSession:(NSString *)server onPort:(int)port ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion;
+ (bool) initAndStartSession:(NSString *)server onPort:(int)port ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion forUserId:(NSString*)userId;
+ (bool) initAndStartSession:(NSString *)server onPort:(int)port byProtocol:(NeloLogProtocol)protocol ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion forUserId:(NSString*)userId;

+ (void) debug:(NSString*)errorCode withMessage:(NSString*)message;
+ (void) error:(NSString*)errorCode withMessage:(NSString*)message;
+ (void) fatal:(NSString*)errorCode withMessage:(NSString*)message;
+ (void) info:(NSString*)errorCode withMessage:(NSString*)message;
+ (void) warn:(NSString*)errorCode withMessage:(NSString*)message;

+ (void) debug:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) error:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) fatal:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) info:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) warn:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;


+ (void) debug:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) info:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) warn:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) error:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) fatal:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;


+ (void) sendSessionLog;

+ (void) removeAllCustomFields;
+ (void) removeCustomFieldForKey:(NSString*)key;
+ (void) setCustomField:(NSString*)value forKey:(NSString*)key;

+ (void) setUserId:(NSString*)userId;
+ (void) setLogType:(NSString*)logType;
+ (void) setLogSource:(NSString*) logSource;
+ (void) setCrashBlock:(void (^)(void))block;
+ (void) setLogLevelFilter:(NSString*)logLevelFilter;
+ (void) setNeloSendMode:(NSString*)sendMode;

+ (void) enableClientSideSymbolication:(bool)enable;
+ (void) setSendSessionLog:(bool)enable;
+ (bool) getSendSessionLog;
+ (void) setSendSessionLog:(NSString *)instanceName withEnable:(bool)enable;
+ (bool) getSendSessionLog:(NSString *)instanceName;


// add send retry option
+(void) setSendRetry:(bool)enable;
+(bool) getSendRerty;
+(void) setSendRetry:(NSString *)instanceName withEnable:(bool)enable;
+(bool) getSendRerty:(NSString *)instanceName;

+ (NSString*) getNeloInstallID;
+ (NSString*) getNeloInstallID:(NSString *) instanceName;



// Dynamic instance

+ (bool) initWithInstance:(NSString *) instanceName ofServer:(NSString *)server onPort:(int)port ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion;
+ (bool) initWithInstance:(NSString *) instanceName ofServer:(NSString *)server onPort:(int)port ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion forUserId:(NSString*)userId;
+ (bool) initWithInstance:(NSString *) instanceName ofServer:(NSString *)server onPort:(int)port byProtocol:(NeloLogProtocol)protocol ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion forUserId:(NSString*)userId;

+ (bool) initWithInstanceAndSession:(NSString *) instanceName ofServer:(NSString *)server onPort:(int)port ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion;
+ (bool) initWithInstanceAndSession:(NSString *) instanceName ofServer:(NSString *)server onPort:(int)port ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion forUserId:(NSString*)userId;
+ (bool) initWithInstanceAndSession:(NSString *) instanceName ofServer:(NSString *)server onPort:(int)port byProtocol:(NeloLogProtocol)protocol ofProjectName:(NSString*)appName withProjectVersion:(NSString*)appVersion forUserId:(NSString*)userId;

+ (void) debugWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message;
+ (void) infoWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message;
+ (void) warnWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message;
+ (void) errorWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message;
+ (void) fatalWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message;


+ (void) debugWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) infoWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) warnWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) errorWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) fatalWithInstance:(NSString *) instanceName withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;

+ (void) debugWithInstance:(NSString *) instanceName withException:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) infoWithInstance:(NSString *) instanceName withException:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) warnWithInstance:(NSString *) instanceName withException:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) errorWithInstance:(NSString *) instanceName withException:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;
+ (void) fatalWithInstance:(NSString *) instanceName withException:(NSException*)exception withErrorCode:(NSString*)errorCode withMessage:(NSString*)message atLocation:(NSString*)location;


+ (void) removeAllCustomFieldsWithInstance:(NSString *)instanceName;
+ (void) removeCustomFieldForKeyWithInstance:(NSString *)instanceName withCumtomfieldKey:(NSString*)key;
+ (void) setCustomFieldWithInstance:(NSString *)instanceName withValue:(NSString*)value forKey:(NSString*)key;

+ (void) setUserIdWithInstance:(NSString *)instanceName withUserId:(NSString*)userId;
+ (void) setLogTypeWithInstance:(NSString *)instanceName withLogType:(NSString*)logType;
+ (void) setLogSourceWithInstance:(NSString *)instanceName withLogSource:(NSString*) logSource;
+ (void) setLogLevelFilterWithInstance:(NSString *)instanceName withLogLevelFilter:(NSString*)logLevelFilter;
+ (void) setNeloSendModeWithInstance:(NSString *)instanceName withSendMode:(NSString*)sendMode;



+ (bool) isReachable;
+ (NSArray *) getReservedFields;
+ (NSDictionary *) getNeloSendMode;
+ (NSDictionary *) getNeloLogLevel;
+ (void) setCrashInstanceName:(NSString *)crashInstanceName;

+ (NeloBlockQueue *) getBlockQueue;
+ (NeloBlockQueue *) getSessionQueue;
+ (NeloBlockQueue *) getWifiQueue;
+ (NeloBlockQueue* ) getSendFailedQueue;

//+ (NeloReachability *) getReachability;
+ (AppleReachability *) getReachability;
@end
