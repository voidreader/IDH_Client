//
//  GamePotAgreeInfo.h
//  GamePot
//
//  Created by Lee Chungwon on 11/01/2019.
//  Copyright © 2019 itsB. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface GamePotAgreeInfo : NSObject
@property (nonatomic) BOOL agree;
@property (nonatomic) BOOL agreeNight;
- (NSDictionary*) toDictionary;
- (NSString* ) toJsonString;

@end
