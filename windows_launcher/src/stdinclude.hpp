#pragma once

#include <Windows.h>
#include <ShlObj.h>

#include <iostream>
#include <filesystem>

extern "C" __declspec(dllimport) int UnityMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nShowCmd);

const char* va(const char* fmt, ...);
bool create_debug_console();
bool grab_source_mutex();
void release_source_mutex();
void install_file_type();
void try_copy_kirapack();