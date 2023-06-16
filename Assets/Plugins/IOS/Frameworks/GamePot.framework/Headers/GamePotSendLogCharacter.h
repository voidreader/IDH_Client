//
//  GamePotSendLogCharacter.h
//  GamePot
//
//  Created by Lee Chungwon on 2019/10/23.
//  Copyright © 2019 itsB. All rights reserved.
//

#import <Foundation/Foundation.h>
//#import "PURLog.h"

NS_ASSUME_NONNULL_BEGIN

#define GAMEPOT_PROJECT_ID @"project_id"
#define GAMEPOT_USER_ID @"user_id"

#define GAMEPOT_NAME @"name" // required
#define GAMEPOT_PLAYER_ID @"player_id" // optional
#define GAMEPOT_SERVER_ID @"server_id" // optional
#define GAMEPOT_LEVEL @"level" // optional
#define GAMEPOT_USERDATA @"userdata" // optional

@interface GamePotSendLogCharacter:NSObject
- (void) put:(NSString*)_value forKey:(NSString*)_key;
- (NSDictionary*) toDictionary;
- (NSString* ) toJsonString;

@end

NS_ASSUME_NONNULL_END
