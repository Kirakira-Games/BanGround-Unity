Push-Location

Set-Location -Path "..\Assets\Codes"

Get-ChildItem -Recurse | % {
    if($_.Extension -eq ".cs") {
        $content = Get-Content -Path $_.FullName
        $content -replace "`r`n","`n"
        Set-Content -Path $_.FullName -Value $content
    }
}

Pop-Location
