//
//  GamePotWebView.h
//  GamePot
//
//  Created by Lee Chungwon on 29/09/2018.
//  Copyright © 2018 itsB. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "GamePotHandler.h"

@interface GamePotAgreeOption:NSObject

typedef NS_ENUM(NSInteger, THEME) {
    BLUE,
    GREEN,
};

@property(nonatomic) NSArray<NSNumber*>* headerBackGradient;
@property(nonatomic) int headerBottomColor;
@property(nonatomic) UIImage* headerIcon;
@property(nonatomic) NSString* headerTitle;
@property(nonatomic) int headerTitleColor;

@property(nonatomic) NSArray<NSNumber*>* contentBackGradient;
@property(nonatomic) UIImage* contentIcon;
@property(nonatomic) int contentIconColor;
@property(nonatomic) int contentCheckColor;
@property(nonatomic) int contentTitleColor;
@property(nonatomic) int contentShowColor;

@property(nonatomic) NSArray<NSNumber*>* footerBackGradient;
@property(nonatomic) NSArray<NSNumber*>* footerButtonGradient;
@property(nonatomic) int footerButtonOutlineColor;
@property(nonatomic) NSString* footerTitle;
@property(nonatomic) int footerTitleColor;

@property(nonatomic) BOOL showNightPush;

@property(nonatomic) NSString* allMessage;
@property(nonatomic) NSString* termMessage;
@property(nonatomic) NSString* privacyMessage;
@property(nonatomic) NSString* nightPushMessage;

- (instancetype) init;
- (instancetype) init:(THEME)_theme;
- (NSString*) toString;

@end



@interface GamePotAgreeView : UIView

@property (nonatomic) NSString* projectId;
@property (nonatomic) NSString* memberId;
@property (nonatomic) NSString* language;
@property (nonatomic) NSString* apiUrl;
@property (nonatomic) NSString* ncpId;

- (instancetype)init:(GamePotAgreeOption*)_option handler:(GamePotAgreeHandler)_handler;
- (void)show:(UIViewController*)_viewController;

@end

