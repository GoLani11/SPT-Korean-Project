@echo off
setlocal

set "PROJECT_ROOT=%~dp0"
set "TRANSLATION_ROOT=%PROJECT_ROOT%..\spt-korean-translate"
set "PYTHON_EXE=%TRANSLATION_ROOT%\.venv\Scripts\python.exe"
set "CLIENT_REFERENCE_ROOT=D:\SPT_3.8.3"

if not exist "%CLIENT_REFERENCE_ROOT%" set "CLIENT_REFERENCE_ROOT=D:\SPT"

if not exist "%PYTHON_EXE%" (
    echo Python environment was not found:
    echo %PYTHON_EXE%
    echo Create the translation repository virtual environment first.
    pause
    exit /b 1
)

"%PYTHON_EXE%" "%PROJECT_ROOT%tools\package_release.py" --translation-root "%TRANSLATION_ROOT%" --spt-383-root "%CLIENT_REFERENCE_ROOT%" %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if not "%EXIT_CODE%"=="0" (
    echo Release package build failed.
    pause
    exit /b %EXIT_CODE%
)

echo The unified release ZIP and upload documents are ready under:
echo %PROJECT_ROOT%artifacts\release-2.1.0
pause
