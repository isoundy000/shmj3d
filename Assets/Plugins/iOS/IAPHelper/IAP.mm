
#import "IAP.h"
#import "IAPShare.h"

@implementation IAP

+ (IAP *)sharedInstance {
    static IAP *_sharedInstance = nil;
    static dispatch_once_t _onceToken;
    dispatch_once(&_onceToken, ^{
        _sharedInstance = [[IAP alloc] init];
        
    });
    return _sharedInstance;
}

-(void) _initIAP:(NSString *)products receipts:(NSString *)receipts {
    NSLog(@"initIAP");
    
    NSLog(@"receipt path: %@", receipts);
    
    if(![IAPShare sharedHelper].iap) {
        NSArray *array = [products componentsSeparatedByString:@","];
        NSSet* dataSet = [[NSSet alloc] initWithArray:array];
        
        NSLog(@"set: %@", dataSet);
        
        [IAPShare sharedHelper].iap = [[IAPHelper alloc] initWithProductIdentifiers:dataSet];
        [IAPShare sharedHelper].iap.receiptsPath = [NSString stringWithString:receipts];
    }
}

-(void) onRespFail:(int)code {
    char tmp[256] = { 0 };
    sprintf(tmp, "%d", code);
    UnitySendMessage("StoreMgr", "onBuyIAPFail", tmp);
}

-(void) onResp:(NSString*)file {
    char tmp[256] = { 0 };
    sprintf(tmp, "%s.plist", [file UTF8String]);
    UnitySendMessage("StoreMgr", "onBuyIAPResp", tmp);
}

-(void) _buyIAP:(NSString*)type
{
    NSLog(@"buyProduct %s", [type UTF8String]);
    
    IAPHelper *iap = [IAPShare sharedHelper].iap;
    
    NSLog(@"receiptPath %s", [iap.receiptsPath UTF8String]);
    
    iap.production = YES;
    
    [iap requestProductsWithCompletion:^(SKProductsRequest* request,SKProductsResponse* response)
     {
         if([[response products] count] > 0 ) {
             SKProduct *p = nil;
             
             for (SKProduct *prd in iap.products) {
                 if ([prd.productIdentifier isEqualToString:type]) {
                     p = prd;
                     break;
                 }
             }
             
             if (!p) {
                 NSLog(@"product %s not found", [type UTF8String]);
                 [self onRespFail:5];
                 
                 return;
             }
             
             [iap buyProduct:p onCompletion:^(SKPaymentTransaction* trans) {
                 if (trans.error)
                 {
                     NSLog(@"Fail %@",[trans.error localizedDescription]);
                     [self onRespFail:2];
                 } else if(trans.transactionState == SKPaymentTransactionStatePurchased) {
                     NSData *data = [NSData dataWithContentsOfURL:[[NSBundle mainBundle] appStoreReceiptURL]];
                     
                     {
                         NSString *file = [iap saveReceipt:data];
                         [self onResp:file];
                         return;
                     }
                     
                     [iap checkReceipt:data AndSharedSecret:nil  onCompletion:^(NSString *response, NSError *error) {
                         
                         NSDictionary* rec = [IAPShare toJSON:response];
                         
                         if([rec[@"status"] integerValue]==0)
                         {
                             
                             [iap provideContentWithTransaction:trans];
                             NSLog(@"SUCCESS %@",response);
                             NSLog(@"Pruchases %@", iap.purchasedProducts);

                             [self onRespFail:1];
                         }
                         else if ([rec[@"status"] integerValue]==21007) {
                             iap.production = NO;
                             [iap checkReceipt:data AndSharedSecret:nil  onCompletion:^(NSString *response2, NSError *error2) {
                                 
                                 NSDictionary* rec2 = [IAPShare toJSON:response2];
                                 if ([rec2[@"status"] integerValue]==0)
                                 {
                                     [iap provideContentWithTransaction:trans];
                                     NSLog(@"SUCCESS %@",response2);
                                     NSLog(@"Pruchases %@", iap.purchasedProducts);
                                     
                                     [self onRespFail:1];
                                     
                                 } else {
                                     NSLog(@"Fail");
                                     [self onRespFail:3];
                                 }
                             }];
                         }
                         else {
                             NSLog(@"Fail");
                             [self onRespFail:4];
                         }
                     }];
                 }
                 else if(trans.transactionState == SKPaymentTransactionStateFailed) {
                     NSLog(@"Fail");
                     [self onRespFail:5];
                 }
             }];//end of buy product
         }
     }];
}

@end

extern "C" {
    void initIAP(const char *products, const char *receipts) {
        IAP *obj = [IAP sharedInstance];
        [obj _initIAP:[NSString stringWithUTF8String:products] receipts:[NSString stringWithUTF8String:receipts]];
    }
    
    void buyIAP(const char *type) {
        IAP *obj = [IAP sharedInstance];
        [obj _buyIAP:[NSString stringWithUTF8String:type]];
    }
}
