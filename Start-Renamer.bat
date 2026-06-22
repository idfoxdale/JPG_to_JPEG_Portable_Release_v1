@echo off
set "APP=%~dp0JPG_to_JPEG_Portable.exe"
set "PUBLISHED_APP=%~dp0bin\Release\net8.0-windows\win-x64\publish\JPG_to_JPEG_Portable.exe"

if exist "%APP%" (
    start "" "%APP%"
) else if exist "%PUBLISHED_APP%" (
    start "" "%PUBLISHED_APP%"
) else (
    echo JPG_to_JPEG_Portable.exe was not found.
    echo Build it with:
    echo dotnet publish -c Release
    pause
)
