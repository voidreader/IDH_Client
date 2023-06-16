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

@interface GamePotWebView : UIView

typedef NS_ENUM(NSInteger, WEBVIEW_TYPE)
{
    WEBVIEW_INTRO = 1,
    WEBVIEW_NORMAL,
    WEBVIEW_NORMALWITHBACK
};

@property (nonatomic) WEBVIEW_TYPE type;
@property (nonatomic) NSString* projectId;
@property (nonatomic) NSString* memberId;
@property (nonatomic) NSString* language;
@property (nonatomic) ExitWebviewCompletionHandler handler;

- (void)show:(UIViewController*)_viewController setType:(WEBVIEW_TYPE)_type setURL:(NSString*)_url;

@end
