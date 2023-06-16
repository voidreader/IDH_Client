
#ifndef GamePotChannelType_h
#define GamePotChannelType_h
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
typedef NS_ENUM (NSInteger, GamePotChannelType)
{
    GUEST = 0,
    GOOGLE = 1,
    FACEBOOK = 2,
    GAMECENTER = 3,
    NAVER = 4,
    LINE = 5,
    TWITTER = 6,
    APPLE = 7,
    NONE = 999
};

//#define ChannelTypeToString(enum) [@[@"GUEST",@"GOOGLE",@"FACEBOOK",@"GAMECENTER",@"NAVER"] objectAtIndex:enum]

@interface GamePotChannelTypeUtil:NSObject
+ (NSString*) ChannelTypeToString:(GamePotChannelType)_type;
+ (GamePotChannelType) ChannelTypeEnumFromString:(NSString*)_type;
@end

#endif
