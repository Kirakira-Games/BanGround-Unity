// UIApplicationOpenURLOptionsKey was added only in ios10 sdk, while we still support ios9 sdk
- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options
{
    id sourceApplication = options[UIApplicationOpenURLOptionsSourceApplicationKey], annotation = options[UIApplicationOpenURLOptionsAnnotationKey];

    NSMutableDictionary<NSString*, id>* notifData = [NSMutableDictionary dictionaryWithCapacity: 3];
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
        notifData[@"url"] = url;
        UnitySetAbsoluteURL(url.absoluteString.UTF8String);
    }
    if (sourceApplication) notifData[@"sourceApplication"] = sourceApplication;
    if (annotation) notifData[@"annotation"] = annotation;

    AppController_SendNotificationWithArg(kUnityOnOpenURL, notifData);
    return YES;
}