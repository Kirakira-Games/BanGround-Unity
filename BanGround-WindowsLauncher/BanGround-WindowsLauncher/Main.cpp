#include "stdinclude.hpp"

extern "C"
{
    __declspec(dllexport) DWORD NvOptimusEnablement = 0x00000001;
    __declspec(dllexport) int AmdPowerXpressRequestHighPerformance = 1;
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nShowCmd)
{
    CreateDebugConsole();
    InstallFileType();
    TryCopyKirapack();
    
    auto handle = FindWindow(L"UnityWndClass", L"BanGround");
    if (handle != nullptr)
    {
        SetForegroundWindow(handle);
        return 0;
    }

    return UnityMain(hInstance, hPrevInstance, lpCmdLine, nShowCmd);
}
