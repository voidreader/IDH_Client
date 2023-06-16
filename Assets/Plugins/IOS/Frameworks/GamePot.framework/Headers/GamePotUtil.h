#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#define SYSTEM_VERSION_EQUAL_TO(v)                  ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] == NSOrderedSame)
#define SYSTEM_VERSION_GREATER_THAN(v)              ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] == NSOrderedDescending)
#define SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(v)  ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] != NSOrderedAscending)
#define SYSTEM_VERSION_LESS_THAN(v)                 ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] == NSOrderedAscending)
#define SYSTEM_VERSION_LESS_THAN_OR_EQUAL_TO(v)     ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] != NSOrderedDescending)

@interface GamePotUtil : NSObject
+ (BOOL)isEmptyString:(NSString* _Nullable)_value;
+ (NSError* _Nonnull)makeError:(NSInteger)_code message:(NSString* _Nonnull)_message;
+ (NSString* _Nonnull) getConvertedID:(NSString* _Nonnull)_rawId;
+ (BOOL)validateString:(NSString * _Nonnull)string withPattern:(NSString * _Nonnull)pattern;
+ (NSDictionary* _Nonnull)getPlist;
+ (UIImage* _Nonnull)GetFilenameFromBundle:(NSString* _Nonnull)filename withType:(NSString* _Nonnull)filetype;
+ (UIImage * _Nonnull)tintedImageFromImage:(UIImage * _Nonnull)image withColor:(UIColor * _Nonnull)color;
+ (NSString* _Nonnull) getDeviceLanguage;
+ (NSString *) getDeviceLanguageOrigin;
+ (NSString *) getDeviceCountryCodeOrigin;
+ (UIViewController *)topViewController;
+ (NSBundle* )getGamePotBundle;
+ (NSString*)getGamePotLocalizable:(NSString*)_key default:(NSString*)_default;
+ (NSString*) getMakeParameter:(NSString*)_projectid memberid:(NSString*)_memberId sdkversion:(NSString*)_sdkversion lang:(NSString*)_language;
+ (NSString *) md5:(NSString *) input;
+ (NSString *) randomStringWithLength:(int)_len;
@end
