# BanGround-Unity

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/305eb5f7ded4481ba6c6320c3e5cddf4)](https://app.codacy.com/gh/Kirakira-Games/BanGround-Unity?utm_source=github.com&utm_medium=referral&utm_content=Kirakira-Games/BanGround-Unity&utm_campaign=Badge_Grade_Settings)

## Looking for maintainers

If you would like to offer help, feel free to contact us by opening an issue in this repository.

What you will do (You can choose any subset of work that you are comfortable with):
-   Learn and build amazing features in Unity3D with C#
-   Automate developing, testing, building, and releasing process
-   Refactor existing C# / typescript code and resolve technical debt
-   Optimize Unity scenes, prefabs, and code to accelerate rendering
-   Maintain backend server
-   Learn and gain experiences on backend development with typescript
-   Learn and build awesome user interface with vue.js
-   Design and write documentation on existing / pending features
-   Draw cute anime waifu

What we expect you to have:
-   Passion
-   Time

What we prefer you to have (any one of the following is sufficient):
-   For coding, basic experience in any object oriented programming language
-   For testing, basic experience in unit testing, mocking (preferably in C# or with Unity 3D)
-   For automation and infrastructure, basic experience in shell scripts (Windows / Linux / Mac OS) and SSH
-   For project management, basic experience in system design and software documentation
-   For art, provide your portfolio or sample work

## Gitflow
1.  No direct push to `master` or `release` branch. All work must be done on a separate branch and a pull request should be created to merge the changes to `master` branch. A pull request from `master` to `release` should be created for each build.

    *Exception*: Small fixes that do not cause behavioral change are allowed to be commited directly to `master`.

2.  Commit messages, pull requests, and branch names should clearly describe the purpose.

3.  Massive scene changes should be done on a separate branch.
An issue should be created to describe the purpose of the changes and to notify other developers that the scene is being modified to avoid merge conflicts.

4.  All pull requests must be reviewed and approved by at least one other developer before merging.

## Note
-   Run ```submodule.bat``` before run unity editor to resolve submodules.

-   Fmod version: 2.01.111

-   Bass library layout
    -   Assets/Plugins/Hidden/Bass
        -   Android
            -   Bass.Net.Android.dll
            -   arm64-v8a / armebi-v7a / x86
                -   libbass.so
                -   libbass_fx.so
                -   libbassenc.so
                -   libbassenc_ogg.so
        -   iOS
            -   Bass.Net.iOS.dll
            -   libbass.a
            -   libbass_fx.a
            -   libbassenc.a
            -   libbassenc_ogg.a
        -   macOS (If you need to use editor in macOS)
            -   Bass.Net.OSX.dll
            -   libbass.dylib
            -   libbass_fx.dylib
            -   libbassenc.dylib
            -   libbassenc_ogg.dylib
        -   Windows
            -   Bass.Net.dll
            -   x86 / x86_64
                -   bass.dll
                -   bass_fx.dll
                -   bassenc.dll
                -   bassenc_ogg.dll

    (All files can be downloaded from [un4seen.com](https://www.un4seen.com/))
