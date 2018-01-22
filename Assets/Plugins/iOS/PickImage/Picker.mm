//
//  Picker.m
//  Unity-iPhone
//
//  Created by 王雄 on 2018/1/18.
//



#import "Picker.h"
#import <MobileCoreServices/MobileCoreServices.h>

#define ORIGINAL_MAX_WIDTH 640.0f

#pragma mark Picker

@implementation Picker

+ (instancetype)sharedInstance {
    static Picker *instance;
    static dispatch_once_t token;
    dispatch_once(&token, ^{
        instance = [[Picker alloc] init];
    });
    return instance;
}

#pragma mark VPImageCropperDelegate
- (void)imageCropper:(VPImageCropperViewController *)cropperViewController didFinished:(UIImage *)editedImage {

    [cropperViewController dismissViewControllerAnimated:YES completion:^{
        NSLog(@"crop done");
        
        CGSize targetSize = CGSizeMake(256, 256);
        UIImage *target = [self imageByScalingAndCroppingForSourceImage:editedImage targetSize:targetSize];
        
        NSData *imagedata = UIImageJPEGRepresentation(target, 1.0);
        
        NSLog(@"path: %@", self.saveImagePath);
        NSString *file = [NSString stringWithFormat:@"%@icon.jpg", self.saveImagePath];
        
        NSLog(@"file: %@", file);
        [imagedata writeToFile:file atomically:YES];
        [self pickResult:0];
        [self dismissPicker];
    }];
}

- (void)imageCropperDidCancel:(VPImageCropperViewController *)cropperViewController {
    [cropperViewController dismissViewControllerAnimated:YES completion:^{
        [self pickResult:1];
        [self dismissPicker];
    }];
}

-(void)pickResult:(int)retcode {
    char tmp[255] = {0};
    sprintf(tmp, "%d", retcode);
    UnitySendMessage("AnysdkMgr", "onPickResp", tmp);
}

-(void)pickImage:(NSString *)path {
    NSLog(@"pick start");
    
    if (![UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypePhotoLibrary]) {
        NSLog(@"cant access photo library");
        [self pickResult:2];
        [self dismissPicker];
        return;
    }

    UIImagePickerController *picker = [[UIImagePickerController alloc] init];
    picker.sourceType = UIImagePickerControllerSourceTypePhotoLibrary;
    NSMutableArray *mediaTypes = [[NSMutableArray alloc] init];
    [mediaTypes addObject:(__bridge NSString *)kUTTypeImage];
    picker.mediaTypes = mediaTypes;
    picker.delegate = self;
    
    self.pickerController = picker;
    
    UIViewController *unityController = UnityGetGLViewController();
    [unityController presentViewController:picker animated:YES completion:^{
        self.saveImagePath = path;
    }];
}

#pragma mark - UIImagePickerControllerDelegate
-(void)imagePickerController:(UIImagePickerController*)picker didFinishPickingMediaWithInfo:(NSDictionary *)info {
    [picker dismissViewControllerAnimated:YES completion:^() {
        UIImage *portraitImg = [info objectForKey:@"UIImagePickerControllerOriginalImage"];
        portraitImg = [self imageByScalingToMaxSize:portraitImg];
        
        self.pickerController = nil;
        UIViewController *unityController = UnityGetGLViewController();
        
        VPImageCropperViewController *imgCropperVC = [[VPImageCropperViewController alloc] initWithImage:portraitImg cropFrame:CGRectMake(0, 100.0f, unityController.view.frame.size.width, unityController.view.frame.size.width) limitScaleRatio:3.0];
        imgCropperVC.delegate = self;
        
        [unityController presentViewController:imgCropperVC animated:YES completion:^{
            // TO DO
        }];
    }];
}

- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker {
    [self pickResult:1];
    [self dismissPicker];
}

#pragma mark image scale utility
- (UIImage *)imageByScalingToMaxSize:(UIImage *)sourceImage {
    if (sourceImage.size.width < ORIGINAL_MAX_WIDTH) return sourceImage;
    CGFloat btWidth = 0.0f;
    CGFloat btHeight = 0.0f;
    if (sourceImage.size.width > sourceImage.size.height) {
        btHeight = ORIGINAL_MAX_WIDTH;
        btWidth = sourceImage.size.width * (ORIGINAL_MAX_WIDTH / sourceImage.size.height);
    } else {
        btWidth = ORIGINAL_MAX_WIDTH;
        btHeight = sourceImage.size.height * (ORIGINAL_MAX_WIDTH / sourceImage.size.width);
    }
    CGSize targetSize = CGSizeMake(btWidth, btHeight);
    return [self imageByScalingAndCroppingForSourceImage:sourceImage targetSize:targetSize];
}

- (UIImage *)imageByScalingAndCroppingForSourceImage:(UIImage *)sourceImage targetSize:(CGSize)targetSize {
    UIImage *newImage = nil;
    CGSize imageSize = sourceImage.size;
    CGFloat width = imageSize.width;
    CGFloat height = imageSize.height;
    CGFloat targetWidth = targetSize.width;
    CGFloat targetHeight = targetSize.height;
    CGFloat scaleFactor = 0.0;
    CGFloat scaledWidth = targetWidth;
    CGFloat scaledHeight = targetHeight;
    CGPoint thumbnailPoint = CGPointMake(0.0,0.0);
    if (CGSizeEqualToSize(imageSize, targetSize) == NO)
    {
        CGFloat widthFactor = targetWidth / width;
        CGFloat heightFactor = targetHeight / height;
        
        if (widthFactor > heightFactor)
            scaleFactor = widthFactor; // scale to fit height
        else
            scaleFactor = heightFactor; // scale to fit width
        scaledWidth  = width * scaleFactor;
        scaledHeight = height * scaleFactor;
        
        // center the image
        if (widthFactor > heightFactor)
        {
            thumbnailPoint.y = (targetHeight - scaledHeight) * 0.5;
        }
        else
            if (widthFactor < heightFactor)
            {
                thumbnailPoint.x = (targetWidth - scaledWidth) * 0.5;
            }
    }
    UIGraphicsBeginImageContext(targetSize); // this will crop
    CGRect thumbnailRect = CGRectZero;
    thumbnailRect.origin = thumbnailPoint;
    thumbnailRect.size.width  = scaledWidth;
    thumbnailRect.size.height = scaledHeight;
    
    [sourceImage drawInRect:thumbnailRect];
    
    newImage = UIGraphicsGetImageFromCurrentImageContext();
    if(newImage == nil) NSLog(@"could not scale image");
    
    //pop the context to get back to the default
    UIGraphicsEndImageContext();
    return newImage;
}

- (void)dismissPicker
{
    self.saveImagePath = nil;
    
    if (self.pickerController != nil) {
        [self.pickerController dismissViewControllerAnimated:YES completion:^{
            self.pickerController = nil;
        }];
    }
}

@end

#pragma mark Unity Plugin

extern "C" {
    void pickImage(const char *path) {
        Picker *picker = [Picker sharedInstance];
        [picker pickImage:[NSString stringWithUTF8String:path]];
    }
}


