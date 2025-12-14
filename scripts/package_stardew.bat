@echo off
setlocal ENABLEDELAYEDEXPANSION

REM Usage: package_stardew.bat <zip-password> [game-dir]
if "%~1"=="" (
  echo Usage: %~nx0 ^<zip-password^> [game-dir]
  exit /b 1
)

set "ZIP_PASSWORD=%~1"
set "GAME_DIR=%~2"
if "%GAME_DIR%"=="" set "GAME_DIR=Stardew Valley"
set "ZIP_NAME=Stardew Valley.zip"
set "OUTPUT_ZIP=%GAME_DIR%\%ZIP_NAME%"

REM Check for 7z in PATH
where 7z >nul 2>&1
if errorlevel 1 (
  echo ERROR: 7z is not available on PATH. Please install 7-Zip and ensure '7z.exe' is in PATH.
  exit /b 1
)

REM Ensure game directory exists
if not exist "%GAME_DIR%" (
  echo ERROR: Game directory '%GAME_DIR%' not found.
  exit /b 1
)

REM Remove previous zip and parts if present
if exist "%OUTPUT_ZIP%" del /f /q "%OUTPUT_ZIP%"
if exist "%OUTPUT_ZIP%.001" del /f /q "%OUTPUT_ZIP%.*"

REM Create password-protected ZIP and split into 10,240 KB parts (AES-256)
REM -tzip   => ZIP format
REM -p      => password
REM -mem=AES256 => strong encryption
REM -v10240k => split volume size 10,240 KB

pushd "%GAME_DIR%"
7z a -tzip "%ZIP_NAME%" "*" -p"%ZIP_PASSWORD%" -mem=AES256 -v10240k
set "ERR=%ERRORLEVEL%"
popd

if not "%ERR%"=="0" (
  echo ERROR: 7z packaging failed with exit code %ERR%.
  exit /b %ERR%
)

REM Display resulting parts
echo Packaging complete. Created parts:
dir /b "%OUTPUT_ZIP%.*" 2>nul

endlocal
exit /b 0
