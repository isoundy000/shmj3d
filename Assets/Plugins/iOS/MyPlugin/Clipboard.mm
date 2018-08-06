
#import "Clipboard.h"

@implementation Clipboard

- (void)_copyTextToClipboard : (NSString *)text
{
    UIPasteboard *pb = [UIPasteboard generalPasteboard];
    
    pb.string = text;
}

- (NSString *)_getTextFromClipboard
{
    UIPasteboard *pb = [UIPasteboard generalPasteboard];
    
    return pb.string;
}

@end

extern "C" {
    static Clipboard *iosClipboard;
    
    void copyTextToClipboard(const char *text) {
        NSString *content = [NSString stringWithUTF8String: text];
        
        if (iosClipboard == NULL)
            iosClipboard = [[Clipboard alloc] init];
        
        [iosClipboard _copyTextToClipboard: content];
    }
    
    char *_MakeStringCopy(const char *str) {
        if (str == NULL) return NULL;
        
        char *ret = (char *)malloc(strlen(str) + 1);
        strcpy(ret, str);
        
        return ret;
    }
    
    const char *getTextFromClipboard() {
        if (iosClipboard == NULL)
            iosClipboard = [[Clipboard alloc] init];
        
        return _MakeStringCopy([[iosClipboard _getTextFromClipboard] UTF8String]);
    }
}
