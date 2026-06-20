@echo off
setlocal

where pwsh >nul 2>nul
if errorlevel 1 (
    echo PowerShell 7 ^(pwsh^) was not found.
    echo Install PowerShell 7, then run this file again.
    pause
    exit /b 1
)

pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\package-release-variants.ps1" %*
set EXIT_CODE=%ERRORLEVEL%

echo.
if not "%EXIT_CODE%"=="0" (
    echo Release package build failed.
    pause
    exit /b %EXIT_CODE%
)

echo Release packages are ready under:
echo %~dp0artifacts\release
pause
