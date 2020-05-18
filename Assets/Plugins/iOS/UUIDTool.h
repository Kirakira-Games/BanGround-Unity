#import <Foundation/Foundation.h>
 
@interface UUIDTool : NSObject
 
/**
 * 普通的获取UUID的方法
 */
+ (NSString *)getUUID;
 
/**
 * 获取到UUID后存入系统中的keychain中，保证以后每次可以得到相同的唯一标志
 * 不用添加plist文件，当程序删除后重装，仍可以得到相同的唯一标示
 * 但是当系统升级或者刷机后，系统中的钥匙串会被清空，再次获取的UUID会与之前的不同
 * @return keychain中存储的UUID
 */
+ (NSString *)getUUIDInKeychain;
 
/**
 * 删除存储在keychain中的UUID
 * 如果删除后，重新获取用户的UUID会与之前的UUID不同
 */
+ (void)deleteKeyChain;
 
 
@end