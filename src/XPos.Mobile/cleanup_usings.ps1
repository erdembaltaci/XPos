$files = Get-ChildItem -Path . -Filter *.razor -Recurse | Where-Object { $_.Name -ne "_Imports.razor" }
foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $newContent = $content | Where-Object { 
        $_ -notmatch "@using XPos.Shared.DTOs" -and 
        $_ -notmatch "@using XPos.Shared.Enums" -and
        $_ -notmatch "@using XPos.Mobile.Services"
    }
    Set-Content -Path $file.FullName -Value $newContent
}
