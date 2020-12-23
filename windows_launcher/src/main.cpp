#include "stdinclude.hpp"

extern "C"
{
    __declspec(dllexport) DWORD NvOptimusEnablement = 0x00000001;
    __declspec(dllexport) int AmdPowerXpressRequestHighPerformance = 1;
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nShowCmd)
{
    create_debug_console();
    install_file_type();
    try_copy_kirapack();
    
    auto handle = FindWindow(L"UnityWndClass", L"BanGround");
    if (handle != nullptr)
    {
        SetForegroundWindow(handle);
        return 0;
    }

    return UnityMain(hInstance, hPrevInstance, lpCmdLine, nShowCmd);
}
