//
//  NLogger.h
//  NLogger
//
//  Created by Danial on 2018. 7. 26..
//  Copyright © 2018년 Danial. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>

#define GAMEPOTLOGGER_VERSION @"1.0.0"
//! Project version number for NLogger.
FOUNDATION_EXPORT double NLoggerVersionNumber;

//! Project version string for NLogger.
FOUNDATION_EXPORT const unsigned char NLoggerVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <NLogger/PublicHeader.h>

@interface GamePotLogger : NSObject

+ (void) init;

+ (void) setUserId:(NSString*)_userId;

+ (void) setLogType:(NSString*)_logType;

+ (void) setLogSource:(NSString*)_logSource;

+ (void) setCrashBlock:(void (^)(void))block;

+ (void) setBeta:(BOOL)_enableBeta;
/*
 NELO2_LOGLEVEL_DEBUG
 NELO2_LOGLEVEL_INFO
 NELO2_LOGLEVEL_WARN
 NELO2_LOGLEVEL_ERROR
 NELO2_LOGLEVEL_FATAL
*/
+ (void) setLogLevelFilter:(NSString*)logLevelFilter;

+ (void) setNeloSendMode:(NSString*)sendMode;

+ (void) enableClientSideSymbolication:(bool)enable;

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

@end
