//
//  Picker.h
//  Unity-iPhone
//
//  Created by 王雄 on 2018/1/18.
//

#pragma once

#import <UIKit/UIKit.h>
#import "VPImageCropperViewController.h"

@interface Picker : NSObject<UIImagePickerControllerDelegate, UINavigationControllerDelegate, VPImageCropperDelegate>

// UnityGLViewController keeps this instance.
@property(nonatomic) UIImagePickerController* pickerController;
@property(nonatomic) NSString *saveImagePath;

+ (instancetype)sharedInstance;

- (void)pickImage:(NSString *)path;

@end
