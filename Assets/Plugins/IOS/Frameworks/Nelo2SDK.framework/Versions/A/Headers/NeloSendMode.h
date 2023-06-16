//
//  NeloSendMode.h
//  Nelo2-iOS-SDK
//
//  Created by Chcano on 2015. 4. 3..
//  Copyright (c) 2015년 Global Platform Development Lab. , NHN Business Platform. All rights reserved.
//

#ifndef Nelo2_iOS_SDK_NeloSendMode_h
#define Nelo2_iOS_SDK_NeloSendMode_h

//typedef NS_ENUM(NSInteger, NeloSendMode){
//    ALL,
//    SESSION_BASED
//};

typedef enum
{
    ALL = 100,
    WIFI = 200,
    SESSION_BASED = 300
} NeloSendMode;

#endif
