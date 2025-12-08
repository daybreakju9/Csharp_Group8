@echo off
setlocal enableextensions

REM Root directory of the repo (this script’s location, ends with backslash)
set "ROOT=%~dp0"
set "BACKEND=%ROOT%Backend"
set "DESKTOP=%ROOT%DesktopClient"
set "WAIT_URL=http://localhost:5000/"
set "MAX_WAIT=60"

echo Using root: %ROOT%

echo [1/4] Restoring backend...
pushd "%BACKEND%" || (echo Cannot cd to %BACKEND% & pause & exit /b 1)
dotnet restore
if errorlevel 1 (
    echo Backend restore failed. Exiting.
    popd
    pause
    exit /b 1
)
popd

echo [2/4] Starting backend (new window)...
start "Backend" cmd /k "pushd ""%BACKEND%"" && dotnet run"

echo [3/4] Waiting for backend to be reachable (%WAIT_URL%) ...
for /L %%i in (1,1,%MAX_WAIT%) do (
    powershell -Command "try { $r=Invoke-WebRequest -UseBasicParsing '%WAIT_URL%' -TimeoutSec 2; exit 0 } catch { exit 1 }"
    if %ERRORLEVEL%==0 (
        echo Backend is up.
        goto :restore_desktop
    )
    timeout /t 1 >nul
)
echo Backend not reachable after %MAX_WAIT% seconds. Will start desktop client anyway.

:restore_desktop
echo [4/5] Restoring desktop client...
pushd "%DESKTOP%" || (echo Cannot cd to %DESKTOP% & pause & exit /b 1)
dotnet restore
if errorlevel 1 (
    echo DesktopClient restore failed. Exiting.
    popd
    pause
    exit /b 1
)
popd

echo [5/5] Starting desktop client (new window)...
start "DesktopClient" cmd /k "pushd ""%DESKTOP%"" && dotnet run"

echo All processes launched. Close windows to stop them.
pause

endlocal

