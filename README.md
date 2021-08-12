# BanGround-Unity

## Gitflow
1. No direct push to `master` or `release` branch.
All work must be done on a separate branch and a pull request should be created to merge the changes to `master` branch.
A pull request from `master` to `release` should be created for each build.

*Exception*: Small fixes that do not cause behavioral change are allowed to be commited directly to `master`.

2. Commit messages, pull requests, and branch names should clearly describe the purpose.

3. Massive scene changes should be done on a separate branch.
An issue should be created to describe the purpose of the changes and to notify other developers that the scene is being modified to avoid merge conflicts.

4. All pull requests must be reviewed and approved by at least one other developer before merging.

## Note
- Run ```submodule.bat``` before run unity editor to resolve submodules.
- Fmod version: 2.01.111

- Bass library layout
    - Assets/Plugins/Bass
        - Android
            - Bass.Net.Android.dll
            - arm64-v8a / armebi-v7a / x86
                - libbass.so
                - libbass_fx.so
                - libbassenc.so
                - libbassenc_ogg.so
        - iOS
            - Bass.Net.iOS.dll
            - libbass.a
            - libbass_fx.a
            - libbassenc.a
            - libbassenc_ogg.a
        - macOS (If you need to use editor in macOS)
            - Bass.Net.OSX.dll
            - libbass.dylib
            - libbass_fx.dylib
            - libbassenc.dylib
            - libbassenc_ogg.dylib
        - Windows
            - Bass.Net.dll
            - x86 / x86_64
                - bass.dll
                - bass_fx.dll
                - bassenc.dll
                - bassenc_ogg.dll

    (All files can be downloaded from [un4seen.com](https://www.un4seen.com/))
