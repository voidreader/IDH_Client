#import <Foundation/Foundation.h>

@interface GamePotPurchaseInfo : NSObject
@property (nonatomic) NSString* adjustKey; // adjustKey
@property (nonatomic) NSString* price; // 가격
@property (nonatomic) NSString* productId; // 결제 아이템 ID
@property (nonatomic) NSString* currency; // 가격 통화
@property (nonatomic) NSString* orderId; // IOS의 TransectionId
@property (nonatomic) NSString* productName; // 결제 아이템 이름
@property (nonatomic) NSString* uniqueId; // 개발사 uniqueId
@property (nonatomic) NSString* signature; // IOS 에서는 사용 되지 않음.
@property (nonatomic) NSString* originalData; // 영수증 Data

- (NSString*)toString;
- (NSDictionary*) toDictionary;
- (NSString* ) toJsonString;
@end
