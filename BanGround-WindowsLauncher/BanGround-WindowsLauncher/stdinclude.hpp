#pragma once

#include <Windows.h>
#include <ShlObj.h>

#include <iostream>
#include <filesystem>

extern "C" __declspec(dllimport) int UnityMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nShowCmd);

const char* va(const char* fmt, ...);
bool CreateDebugConsole();
bool GrabSourceMutex();
void ReleaseSourceMutex();
void InstallFileType();
void TryCopyKirapack();