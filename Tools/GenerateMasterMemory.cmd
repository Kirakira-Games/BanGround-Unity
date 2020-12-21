@echo off
pushd ..
MasterMemory.Generator\win-x64\MasterMemory.Generator.exe -i "./Assets/Codes/Storage/Database/Models" -o "./Assets/Codes/Storage/Database/Generated" -n "BanGround.Database.Generated" || goto fail1
mpc.exe -i ./Assets/Codes/Storage/Database/Models -o ./Assets/Codes/Storage/Database/Generated || goto fail2
popd
echo.
echo Done!
pause
goto :eof

:fail1
echo Failed to generate MasterMemory!
pause
goto :eof

:fail2
echo Failed to run mpc! Do you have mpc installed? If not, install it via dotnet:
echo.
echo   dotnet tool install --global MessagePack.Generator
echo.
pause
goto :eof