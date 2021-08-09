Push-Location

Set-Location -Path "..\Assets\Codes"

$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False

Get-ChildItem -Recurse | % {
    if($_.Extension -eq ".cs") {
        $content = [System.IO.File]::ReadAllText($_.FullName);
        $content = $content.Replace("`r`n","`n");
        [System.IO.File]::WriteAllText($_.FullName, $content, $Utf8NoBomEncoding);
    }
}

Pop-Location
