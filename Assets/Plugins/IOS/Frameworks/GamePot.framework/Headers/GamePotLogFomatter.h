#import <Foundation/Foundation.h>
#import "CocoaLumberjack.h"

@interface GamePotLogFomatter : NSObject <DDLogFormatter>{
    int loggerCount;
    NSDateFormatter *threadUnsafeDateFormatter;
}
@end
