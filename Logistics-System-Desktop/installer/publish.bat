@echo off
REM ==== Đóng gói TruckingSystem Desktop: chạy file này trong Logistics-System-Desktop\installer ====
chcp 65001 >nul
cd /d "%~dp0.."

echo [1/2] Xuat ban Desktop (tu chay, khong can cai .NET)...
if exist publish\Desktop rmdir /s /q publish\Desktop
dotnet publish src\TruckingSystem.Desktop -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none ^
  -o publish\Desktop
if errorlevel 1 goto :fail

echo [2/2] Tao bo cai bang Inno Setup...
set ISCC="%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not exist %ISCC% set ISCC="%ProgramFiles%\Inno Setup 6\ISCC.exe"
if not exist %ISCC% (
  echo Chua cai Inno Setup 6 - tai o https://jrsoftware.org/isdl.php roi chay lai.
  goto :fail
)
%ISCC% installer\TruckingSystem.iss
if errorlevel 1 goto :fail

echo.
echo XONG! Bo cai nam o: publish\Installer
explorer publish\Installer
goto :eof

:fail
echo.
echo CO LOI - xem thong bao phia tren.
pause
