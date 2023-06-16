#import <Foundation/Foundation.h>

@interface GamePotChatMessage : NSObject
@property(nonatomic, strong) NSString* message;
@property(nonatomic, strong) NSString* channel;
- (instancetype) initWithMessage:(NSString*)_message channel:(NSString*)_channel;
@end

typedef void (^GamePotChatHandler) (GamePotChatMessage* message);

@interface GamePotChat : NSObject

@property(nonatomic, strong) NSString* projectId;
@property(nonatomic, assign) BOOL isSandbox;
@property(nonatomic, assign) BOOL isBeta;

+ (GamePotChat*) getInstance;
- (void)setup:(NSString*)_url;
- (void) send:(GamePotChatMessage*)_message;
- (BOOL) join:(NSString*)_channel;
- (BOOL) leave:(NSString*)_channel;
- (void) setHandler:(GamePotChatHandler)_handler;
- (void) start;
- (void) stop;
@end


