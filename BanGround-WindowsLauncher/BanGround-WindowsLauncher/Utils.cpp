#include "stdinclude.hpp"

#define VA_BUFFER_COUNT		4
#define VA_BUFFER_SIZE		32768

static char g_vaBuffer[VA_BUFFER_COUNT][VA_BUFFER_SIZE];
static int g_vaNextBufferIndex = 0;

const char* va(const char* fmt, ...)
{
	va_list ap;
	char* dest = &g_vaBuffer[g_vaNextBufferIndex][0];
	g_vaNextBufferIndex = (g_vaNextBufferIndex + 1) % VA_BUFFER_COUNT;
	va_start(ap, fmt);
	int res = _vsnprintf(dest, VA_BUFFER_SIZE, fmt, ap);
	dest[VA_BUFFER_SIZE - 1] = '\0';
	va_end(ap);

	if (res < 0 || res >= VA_BUFFER_SIZE)
	{
		//return "";
	}

	return dest;
}

bool CreateDebugConsole()
{
	BOOL result = AllocConsole();

	if (!result)
		return false;

	freopen("CONIN$", "r", stdin);
	freopen("CONOUT$", "w", stdout);
	freopen("CONOUT$", "w", stderr);

	SetConsoleTitleW(L"BanGround Dev Console");
	SetConsoleCP(CP_UTF8);
	SetConsoleOutputCP(CP_UTF8);

	std::cout << "KiraLoader Attached!\n";

	return true;
}

HANDLE g_hMutex = NULL;
bool GrabSourceMutex()
{
	// don't allow more than one instance to run
	g_hMutex = ::CreateMutex(NULL, FALSE, TEXT("banground_singleton_mutex"));

	unsigned int waitResult = ::WaitForSingleObject(g_hMutex, 0);

	// Here, we have the mutex
	if (waitResult == WAIT_OBJECT_0 || waitResult == WAIT_ABANDONED)
		return true;

	// couldn't get the mutex, we must be running another instance
	::CloseHandle(g_hMutex);

	return false;
}

void ReleaseSourceMutex()
{
	if (g_hMutex)
	{
		::ReleaseMutex(g_hMutex);
		::CloseHandle(g_hMutex);
		g_hMutex = NULL;
	}
}

void InstallFileType()
{
	char ownPth[MAX_PATH] = { 0 };
	char workdir[MAX_PATH] = { 0 };

	DWORD dwsize = MAX_PATH;
	HMODULE hModule = GetModuleHandle(nullptr);

	if (hModule != nullptr)
	{
		if (GetModuleFileNameA(hModule, ownPth, MAX_PATH) == ERROR)
		{
			return;
		}

		if (GetModuleFileNameA(hModule, workdir, MAX_PATH) == ERROR)
		{
			return;
		}
		else
		{
			char* endPtr = strstr(workdir, "BanGround-Unity.exe");
			if (endPtr != nullptr)
			{
				*endPtr = 0;
			}
			else
			{
				return;
			}
		}
	}
	else
	{
		return;
	}

	SetCurrentDirectoryA(workdir);

	HKEY hKey = nullptr;
	std::string data;

	LONG openRes = RegOpenKeyExA(HKEY_CURRENT_USER, "SOFTWARE\\Classes\\.kirapack\\shell\\import\\command", 0, KEY_ALL_ACCESS, &hKey);
	if (openRes == ERROR_SUCCESS)
	{
		char regred[MAX_PATH] = { 0 };

		// Check if the game has been moved.
		openRes = RegQueryValueExA(hKey, nullptr, nullptr, nullptr, reinterpret_cast<BYTE*>(regred), &dwsize);
		if (openRes == ERROR_SUCCESS)
		{
			char* endPtr = strstr(regred, "\" \"%1\"");
			if (endPtr != nullptr)
			{
				*endPtr = 0;
			}
			else
			{
				return;
			}

			RegCloseKey(hKey);
			if (strcmp(regred + 1, ownPth))
			{
				RegDeleteKeyA(HKEY_CURRENT_USER, "SOFTWARE\\Classes\\.kirapack");
			}
			else
			{
				return;
			}
		}
		else
		{
			RegDeleteKeyA(HKEY_CURRENT_USER, "SOFTWARE\\Classes\\.kirapack");
		}
	}
	else
	{
		RegDeleteKeyA(HKEY_CURRENT_USER, "SOFTWARE\\Classes\\.kirapack");
	}

	openRes = RegOpenKeyExA(HKEY_CURRENT_USER, "SOFTWARE\\Classes", 0, KEY_ALL_ACCESS, &hKey);

	if (openRes != ERROR_SUCCESS)
	{
		return;
	}

	openRes = RegCreateKeyExA(hKey, ".kirapack", 0, nullptr, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nullptr, &hKey, nullptr);

	if (openRes != ERROR_SUCCESS)
	{
		return;
	}

	openRes = RegSetValueExA(hKey, nullptr, 0, REG_SZ, reinterpret_cast <const BYTE*>("BanGround Kira Pack"), 20);

	if (openRes != ERROR_SUCCESS)
	{
		RegCloseKey(hKey);
		return;
	}

	openRes = RegCreateKeyExA(hKey, "DefaultIcon", 0, nullptr, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nullptr, &hKey, nullptr);

	if (openRes != ERROR_SUCCESS)
	{
		return;
	}

	data = va("%s,1", ownPth);
	openRes = RegSetValueExA(hKey, nullptr, 0, REG_SZ, reinterpret_cast<const BYTE*>(data.data()), data.size() + 1);
	RegCloseKey(hKey);

	if (openRes != ERROR_SUCCESS)
	{
		RegCloseKey(hKey);
		return;
	}

	HKEY kirapackRoot;
	openRes = RegOpenKeyExA(HKEY_CURRENT_USER, "SOFTWARE\\Classes\\.kirapack", 0, KEY_ALL_ACCESS, &kirapackRoot);

	if (openRes != ERROR_SUCCESS)
	{
		return;
	}

	openRes = RegCreateKeyExA(kirapackRoot, "shell", 0, nullptr, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nullptr, &hKey, nullptr);

	if (openRes != ERROR_SUCCESS)
	{
		return;
	}

	openRes = RegSetValueExA(hKey, nullptr, 0, REG_SZ, reinterpret_cast<const BYTE*>("import"), 7);
	RegCloseKey(hKey);

	openRes = RegCreateKeyExA(kirapackRoot, "shell\\import", 0, nullptr, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nullptr, &hKey, nullptr);

	if (openRes != ERROR_SUCCESS)
	{
		return;
	}

	openRes = RegSetValueExA(hKey, nullptr, 0, REG_SZ, reinterpret_cast<const BYTE*>("Import to BanGround"), 20);
	openRes = RegSetValueExA(hKey, "Icon", 0, REG_SZ, reinterpret_cast<const BYTE*>(ownPth), strlen(ownPth) + 1);
	RegCloseKey(hKey);

	if (openRes != ERROR_SUCCESS)
	{
		return;
	}

	openRes = RegCreateKeyExA(kirapackRoot, "shell\\import\\command", 0, nullptr, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nullptr, &hKey, nullptr);

	data = va("\"%s\" \"%s\"", ownPth, "%1");
	openRes = RegSetValueExA(hKey, nullptr, 0, REG_SZ, reinterpret_cast<const BYTE*>(data.data()), data.size() + 1);
	RegCloseKey(hKey);

	if (openRes != ERROR_SUCCESS)
	{
		return;
	}

	return;
}

void TryCopyKirapack()
{
	wchar_t buffer[MAX_PATH];
	SHGetSpecialFolderPath(NULL, buffer, CSIDL_LOCAL_APPDATA, false);

	std::wstring inbox(buffer);
	inbox += L"Low\\Kirakira Games\\BanGround\\InBox\\";

	if (GetFileAttributes(inbox.c_str()) == -1)
		CreateDirectory(inbox.c_str(), NULL);

	int argc = 0;
	wchar_t** argv = CommandLineToArgvW(GetCommandLineW(), &argc);

	int count = 0;

	for (int i = 1; i < argc; i++)
	{
		std::filesystem::path file(argv[i]);
		if (file.extension() != ".kirapack")
			continue;

		count++;
		std::filesystem::path newFile(inbox);
		newFile += file.filename();

		CopyFile(file.wstring().c_str(), newFile.wstring().c_str(), false);
	}

	if (count > 0)
		std::cout << "Copied " << count << " kirapacks." << std::endl;
}
