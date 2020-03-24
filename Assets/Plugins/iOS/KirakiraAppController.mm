#import "UnityAppController.h"

@interface KirakiraAppController : UnityAppController
@end

IMPL_APP_CONTROLLER_SUBCLASS (KirakiraAppController)
 
@implementation KirakiraAppController

// UIApplicationOpenURLOptionsKey was added only in ios10 sdk, while we still support ios9 sdk
- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options
{

    if (url)
    {
        BOOL needChange = [url startAccessingSecurityScopedResource];
        NSString *fileNameStr = [url lastPathComponent];
        NSString *Doc = [[NSHomeDirectory() stringByAppendingPathComponent:@"Documents"] stringByAppendingPathComponent:fileNameStr];
        NSString *path = [url path];
        NSData *data = [[NSFileManager defaultManager] contentsAtPath:path];
        [data writeToFile:Doc atomically:YES];
        if (needChange)
        {
            [url stopAccessingSecurityScopedResource];
        }
    }


    return [super application:app openURL:url options:options];
}
@end
