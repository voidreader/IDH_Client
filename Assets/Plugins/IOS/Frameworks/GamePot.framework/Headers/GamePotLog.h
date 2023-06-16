#ifndef GamePotLog_h
#define GamePotLog_h

#import <Foundation/Foundation.h>
#import "GamePotLogFomatter.h"

static DDLogLevel ddLogLevel = DDLogLevelVerbose;
@interface GamePotLog : NSObject
+ (void) setting;
+ (void) setSandbox:(BOOL)_sandbox;
@end

#endif

