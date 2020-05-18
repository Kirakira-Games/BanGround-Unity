//
//  UUIDTool.m
//  ZYUUID
//
//  Created by 竹雨 on 2018/4/13.
//  Copyright © 2018年 竹雨. All rights reserved.
//
 
#import "UUIDTool.h"
 
NSString * const kUUIDKey = @"com.myApp.uuid";
 
@implementation UUIDTool
 
// 普通的获取UUID的方法
+ (NSString *)getUUID {
    CFUUIDRef puuid = CFUUIDCreate( nil );
    CFStringRef uuidString = CFUUIDCreateString(nil, puuid);
    NSString *result = (NSString *)CFBridgingRelease(CFStringCreateCopy( NULL, uuidString));
    return result;
}
 
 
#pragma mark - 获取到UUID后存入系统中的keychain中
 
+ (NSString *)getUUIDInKeychain {
    // 1.直接从keychain中获取UUID
    NSString *getUDIDInKeychain = (NSString *)[UUIDTool load:kUUIDKey];
    NSLog(@"从keychain中获取UUID%@", getUDIDInKeychain);
    
    // 2.如果获取不到，需要生成UUID并存入系统中的keychain
    if (!getUDIDInKeychain || [getUDIDInKeychain isEqualToString:@""] || [getUDIDInKeychain isKindOfClass:[NSNull class]]) {
        // 2.1 生成UUID
        CFUUIDRef puuid = CFUUIDCreate(nil);
        CFStringRef uuidString = CFUUIDCreateString(nil, puuid);
        NSString *result = (NSString *)CFBridgingRelease(CFStringCreateCopy(NULL, uuidString));
        CFRelease(puuid);
        CFRelease(uuidString);
        NSLog(@"生成UUID：%@",result);
        // 2.2 将生成的UUID保存到keychain中
        [UUIDTool save:kUUIDKey data:result];
        // 2.3 从keychain中获取UUID
        getUDIDInKeychain = (NSString *)[UUIDTool load:kUUIDKey];
    }
    
    return getUDIDInKeychain;
}
 
 
#pragma mark - 删除存储在keychain中的UUID
 
+ (void)deleteKeyChain {
    [self delete:kUUIDKey];
}
 
 
#pragma mark - 私有方法
 
+ (NSMutableDictionary *)getKeyChainQuery:(NSString *)service {
    return [NSMutableDictionary dictionaryWithObjectsAndKeys:(id)kSecClassGenericPassword,(id)kSecClass,service,(id)kSecAttrService,service,(id)kSecAttrAccount,(id)kSecAttrAccessibleAfterFirstUnlock,(id)kSecAttrAccessible, nil];
}
 
// 从keychain中获取UUID
+ (id)load:(NSString *)service {
    id ret = nil;
    NSMutableDictionary *keychainQuery = [self getKeyChainQuery:service];
    [keychainQuery setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnData];
    [keychainQuery setObject:(id)kSecMatchLimitOne forKey:(id)kSecMatchLimit];
    CFDataRef keyData = NULL;
    if (SecItemCopyMatching((CFDictionaryRef)keychainQuery, (CFTypeRef *)&keyData) == noErr) {
        @try {
            ret = [NSKeyedUnarchiver unarchiveObjectWithData:(__bridge NSData *)keyData];
        }
        @catch (NSException *exception) {
            NSLog(@"Unarchive of %@ failed: %@", service, exception);
        }
        @finally {
            NSLog(@"finally");
        }
    }
    
    if (keyData) {
        CFRelease(keyData);
    }
    NSLog(@"ret = %@", ret);
    return ret;
}
 
+ (void)delete:(NSString *)service {
    NSMutableDictionary *keychainQuery = [self getKeyChainQuery:service];
    SecItemDelete((CFDictionaryRef)keychainQuery);
}
 
// 将生成的UUID保存到keychain中
+ (void)save:(NSString *)service data:(id)data {
    // Get search dictionary
    NSMutableDictionary *keychainQuery = [self getKeyChainQuery:service];
    // Delete old item before add new item
    SecItemDelete((CFDictionaryRef)keychainQuery);
    // Add new object to search dictionary(Attention:the data format)
    [keychainQuery setObject:[NSKeyedArchiver archivedDataWithRootObject:data] forKey:(id)kSecValueData];
    // Add item to keychain with the search dictionary
    SecItemAdd((CFDictionaryRef)keychainQuery, NULL);
}
 
 
@end
#if defined (__cplusplus)
extern "C" {
#endif
 
char* _getUUIDInKeychain()
{
    NSString *s = [UUIDTool getUUIDInKeychain];
    const char* output = [s UTF8String];
    char* result = (char*)malloc(strlen(output)+1);
    strcpy(result,output);
    return result;
    //UnitySendMessage("ScriptHolder","GetUUID",s.UTF8String);
}
 
# if defined (__cplusplus)
}
#endif