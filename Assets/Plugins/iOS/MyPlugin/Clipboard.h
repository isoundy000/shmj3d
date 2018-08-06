
@ interface Clipboard : NSObject

extern "C" {
    void copyTextToClipboard(const char *text);
    
    const char *getTextFromClipboard();
}

@end
