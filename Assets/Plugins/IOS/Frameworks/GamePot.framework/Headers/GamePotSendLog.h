//
//  GamePotSendLog.h
//  GamePot
//
//  Created by Lee Chungwon on 2019/10/22.
//  Copyright © 2019 itsB. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "GamePotSendLogCharacter.h"

NS_ASSUME_NONNULL_BEGIN

@interface GamePotSendLog : NSObject

+ (void) initialize:(NSString*)_projectId setSandBox:(BOOL)_isSandBox;
+ (BOOL) characterInfo:(GamePotSendLogCharacter*)_info;
@end

NS_ASSUME_NONNULL_END
