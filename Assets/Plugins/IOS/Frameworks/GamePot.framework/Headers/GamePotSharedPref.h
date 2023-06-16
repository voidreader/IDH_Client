#import <Foundation/Foundation.h>

@interface GamePotSharedPref : NSObject

+ (GamePotSharedPref*) getInstance;

- (void)saveObject:(id)_value forKey:(NSString*)_key;

- (void)saveInteger:(NSInteger)_value forKey:(NSString*)_key;

- (void)saveBool:(BOOL)_value forKey:(NSString*)_key;

- (id)loadObject:(NSString*)_key;

- (NSInteger)loadInteger:(NSString*)_key;

- (BOOL)loadBool:(NSString*)_key;

- (void)remove:(NSString*)_key;

@end
