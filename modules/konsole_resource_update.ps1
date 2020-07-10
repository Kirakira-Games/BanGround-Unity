$rootDir = (Get-Location).ToString();
$rootDir += "\konsole-front\dist\spa";

$resourceList = @{};

$konsolePath = (Get-Location).ToString() + "\..\Assets\Resources\WebConsole";
$codePath = (Get-Location).ToString() + "\..\Assets\Codes\WebConsole.g.cs";

if(Test-Path -Path $konsolePath) {
    Remove-Item $konsolePath -Recurse -Force;
    mkdir $konsolePath | Out-Null;
}

$generatedCode = @"
// Auto generated by konsole_resource_update.ps1
using System.Collections.Generic;

class WebConsoleResource
{
`tpublic static Dictionary<string, string> list;

`tstatic WebConsoleResource()
`t{
`t`tlist = new Dictionary<string, string>
`t`t{`n
"@;

Get-ChildItem -Path konsole-front\dist\spa -Recurse | ForEach-Object {
    if(-not $_.Mode.Contains("d")) {
        $serverPath = $_.FullName.Replace($rootDir, "").Replace('\', '/');
        $resourcePath = "WebConsole" + $serverPath;
        $localPath = $konsolePath + $serverPath.Replace('/','\') + ".bytes";

        mkdir -Force (Split-Path $localPath) | Out-Null;
        Copy-Item -Path $_.FullName -Destination $localPath;

        $generatedCode += "`t`t`t{ `"" + $serverPath + "`", `"" + $resourcePath + "`" },`n";
    }
};

$generatedCode +=  @"
`t`t};
`t}

`tpublic static IEnumerator<KeyValuePair<string, string>> GetEnumerator()
`t{
`t`tforeach(var kvp in list)
`t`t{
`t`t`tyield return kvp;
`t`t}
`t}
}
"@ ;

if(Test-Path -Path $codePath) {
    Remove-Item -Path $codePath;
}

Set-Content -Path $codePath -Value $generatedCode -Force;