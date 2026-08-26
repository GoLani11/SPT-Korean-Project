@echo off
setlocal

set "PROJECT_ROOT=%~dp0"
set "TRANSLATION_ROOT=%PROJECT_ROOT%..\spt-korean-translate"
set "PYTHON_EXE=%TRANSLATION_ROOT%\.venv\Scripts\python.exe"
set "CLIENT_REFERENCE_ROOT=D:\SPT3.8.3"

if not exist "%CLIENT_REFERENCE_ROOT%" set "CLIENT_REFERENCE_ROOT=D:\SPT"

if not exist "%PYTHON_EXE%" (
    echo Python environment was not found:
    echo %PYTHON_EXE%
    echo Create the translation repository virtual environment first.
    pause
    exit /b 1
)

"%PYTHON_EXE%" "%PROJECT_ROOT%tools\package_release_versions.py" --translation-root "%TRANSLATION_ROOT%" --client-reference-spt-root "%CLIENT_REFERENCE_ROOT%" %*
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if not "%EXIT_CODE%"=="0" (
    echo Release package build failed.
    pause
    exit /b %EXIT_CODE%
)

echo The 7 release ZIP files are ready under:
echo %PROJECT_ROOT%artifacts\release
pause
