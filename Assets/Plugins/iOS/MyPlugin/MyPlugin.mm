
#import "MyPlugin.h"
#import "Reachability.h"
#import <CoreTelephony/CTTelephonyNetworkInfo.h>

@implementation MyPlugin

+ (MyPlugin *)sharedInstance {
    static MyPlugin *_sharedInstance = nil;
    static dispatch_once_t _onceToken;
    dispatch_once(&_onceToken, ^{
        _sharedInstance = [[MyPlugin alloc] init];
        
    });
    return _sharedInstance;
}

-(int)_getBatteryInfo {
    UIDevice *device = [UIDevice currentDevice];
    
    device.batteryMonitoringEnabled = YES;
    
    CGFloat level = device.batteryLevel;
    
    UIDeviceBatteryState state = device.batteryState;
    
    int add = 0;
    
    if (UIDeviceBatteryStateUnplugged == state)
        add = 1000;
    else if (UIDeviceBatteryStateCharging == state)
        add = 2000;
    else if (UIDeviceBatteryStateFull == state)
        add = 3000;
    
    int power = floor(level * 100);
    
    return power + add;
}

-(int)_getNetworkInfo {
    Reachability *reach = [Reachability reachabilityForInternetConnection];
    int iType = 0;
    NSString *netconnType = @"";
    switch ([reach currentReachabilityStatus]) {
        case NotReachable:// 没有网络
            netconnType = @"no network";
            iType = 0;
            break;
        case ReachableViaWiFi:// Wifi
            netconnType = @"Wifi";
            iType = 1;
            break;
        case ReachableViaWWAN:// 手机自带网络
            // 获取手机网络类型
            CTTelephonyNetworkInfo *info = [[CTTelephonyNetworkInfo alloc] init];
            
            NSString *currentStatus = info.currentRadioAccessTechnology;
            
            if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyGPRS"]) {
                
                netconnType = @"GPRS";
                iType = 2;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyEdge"]) {
                
                netconnType = @"EDGE";
                iType = 2;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyWCDMA"]){
                
                netconnType = @"3G";
                iType = 3;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyHSDPA"]){
                
                netconnType = @"3.5G";
                iType = 3;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyHSUPA"]){
                
                netconnType = @"3.5G";
                iType = 3;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyCDMA1x"]){
                
                netconnType = @"2G";
                iType = 2;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyCDMAEVDORev0"]){
                
                netconnType = @"3G";
                iType = 3;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyCDMAEVDORevA"]){
                
                netconnType = @"3G";
                iType = 3;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyCDMAEVDORevB"]){
                
                netconnType = @"3G";
                iType = 3;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyeHRPD"]){
                
                netconnType = @"HRPD";
                iType = 3;
            }else if ([currentStatus isEqualToString:@"CTRadioAccessTechnologyLTE"]){
                
                netconnType = @"4G";
                iType = 4;
            }
            break;
    }
    
    int strength = 0;
    
    if (iType == 1) {
        UIApplication *app = [UIApplication sharedApplication];
        NSArray *subviews = [[[app valueForKey:@"statusBar"] valueForKey:@"foregroundView"] subviews];
        NSString *dataNetworkItemView = nil;
        
        for (id subview in subviews) {
            if([subview isKindOfClass:[NSClassFromString(@"UIStatusBarDataNetworkItemView") class]]) {
                dataNetworkItemView = subview;
                break;
            }
        }
        
        strength = [[dataNetworkItemView valueForKey:@"_wifiStrengthBars"] intValue];
    }
    
    return strength + iType * 1000;
}

@end

extern "C" {
    int getNetworkInfo() {
        MyPlugin *obj = [MyPlugin sharedInstance];
        return [obj _getNetworkInfo];
    }
    
    int getBatteryInfo() {
        MyPlugin *obj = [MyPlugin sharedInstance];
        return [obj _getBatteryInfo];
    }
}




