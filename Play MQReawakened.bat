@echo off
:: Move to the folder where the .bat is located
cd /d "%~dp0"

:: Launch conhost, tell it to stay in the current directory, and run the script
start conhost.exe powershell.exe -ExecutionPolicy Bypass -NoExit -File "launcher-entrypoint.ps1"
exit