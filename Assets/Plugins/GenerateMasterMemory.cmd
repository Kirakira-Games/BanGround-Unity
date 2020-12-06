@echo off
pushd ..\..
MasterMemory.Generator\win-x64\MasterMemory.Generator.exe -i "./Assets/Codes/Storage/Database/Models" -o "./Assets/Codes/Storage/Database/Generated" -n "BanGround.Database.Models"
mpc.exe -i ./Assets/Codes/Storage/Database/Models -o ./Assets/Codes/Storage/Database/Generated
popd
pause