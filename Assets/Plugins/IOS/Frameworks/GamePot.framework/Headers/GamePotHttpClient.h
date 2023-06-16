#import <Foundation/Foundation.h>
#import "GamePotHandler.h"

#define INTERNAL_TIMEOUT 10
#define RETRY_COUNT 3
#define RETRY_INTERVAL 1

//#define BASE_URL  @"https://execute-api.adzest.com"
//#define API_KEY @"xBSg9Qj5GQtyFJGpgK7HvxdleSwC284vkXw9XI50"

typedef void (^GamePotHttpClientHandler) (NSDictionary* _data, NSError* _error);

@interface GamePotHttpClient : NSObject
- (instancetype) initWithURL:(NSString*)_url withHeader:(NSDictionary*)_header sandbox:(BOOL)_sandbox;
- (void) POST:(NSString*)_appendURL andParams:(NSDictionary*)_params handler:(GamePotHttpClientHandler)_handler;
- (void) GET:(NSString*)_appendURL andParams:(NSDictionary*)_params handler:(GamePotHttpClientHandler)_handler;
- (void) DELETE:(NSString*)_appendURL andParams:(NSDictionary*)_params handler:(GamePotHttpClientHandler)_handler;
@end
