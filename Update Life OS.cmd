@echo off
cd /d "%~dp0"
call "Stop Life OS.cmd"
dotnet publish "LifeOS.MVC\LifeOS.MVC.csproj" -c Release -o DesktopApp --self-contained false
if errorlevel 1 (echo Update failed. & pause & exit /b 1)
echo Life OS updated. Double-click Start Life OS.cmd.
pause
