@echo off
REM ============================================================================
REM  Cai / CAP NHAT API thanh dich vu Windows "TruckingSystemAPI" (bat may la tu chay).
REM  Chuot phai file nay -> Run as administrator.
REM  Lan dau: tao C:\TruckingSystem\API + chung chi HTTPS + file cau hinh, roi dang ky dich vu.
REM  Cac lan sau (sua code xong): dung dich vu -> xuat ban ban moi -> chay lai. Cau hinh giu nguyen.
REM ============================================================================
chcp 65001 >nul
net session >nul 2>&1 || (echo Can chay bang quyen Administrator: chuot phai -^> Run as administrator. & pause & exit /b 1)
cd /d "%~dp0.."
set SVC=TruckingSystemAPI
set DEST=C:\TruckingSystem\API
set CERT=C:\TruckingSystem\localhost.pfx
set CERTPWD=TruckingCert@2026

echo [1/5] Dung dich vu (neu dang chay)...
sc query %SVC% >nul 2>&1 && (sc stop %SVC% >nul 2>&1 & timeout /t 5 /nobreak >nul)

echo [2/5] Xuat ban API vao %DEST% ...
if exist "%DEST%" rmdir /s /q "%DEST%"
dotnet publish src\TruckingSystem.API -c Release -r win-x64 --self-contained false -p:DebugType=none -o "%DEST%"
if errorlevel 1 goto :fail

echo [3/5] Chung chi HTTPS cho https://localhost:8080 ...
if not exist "%CERT%" (
  dotnet dev-certs https -ep "%CERT%" -p %CERTPWD%
  if errorlevel 1 goto :fail
) else echo    Da co %CERT% - giu nguyen.

echo [4/5] File cau hinh rieng cho dich vu (appsettings.Production.json)...
if not exist "C:\TruckingSystem\appsettings.Production.json" (
  > "C:\TruckingSystem\appsettings.Production.json" (
    echo {
    echo   "Kestrel": {
    echo     "Endpoints": {
    echo       "Https": {
    echo         "Url": "https://localhost:8080",
    echo         "Certificate": { "Path": "C:\\TruckingSystem\\localhost.pfx", "Password": "%CERTPWD%" }
    echo       }
    echo     }
    echo   }
    echo }
  )
  echo    Da tao C:\TruckingSystem\appsettings.Production.json - MO RA KIEM TRA truoc khi dung that.
) else echo    Da co - giu nguyen.
copy /y "C:\TruckingSystem\appsettings.Production.json" "%DEST%\appsettings.Production.json" >nul

echo [5/5] Dang ky + chay dich vu %SVC% ...
sc query %SVC% >nul 2>&1 || (
  sc create %SVC% binPath= "\"%DEST%\TruckingSystem.API.exe\"" start= delayed-auto DisplayName= "TruckingSystem API"
  sc description %SVC% "May chu API he thong TruckingSystem (Logistics-System)"
  sc failure %SVC% reset= 86400 actions= restart/10000/restart/10000/restart/60000
)
sc start %SVC%
timeout /t 8 /nobreak >nul
sc query %SVC% | find "RUNNING" >nul && (
  echo.
  echo XONG - dich vu dang chay. Thu mo https://localhost:8080 hoac dang nhap Desktop.
) || (
  echo.
  echo Dich vu CHUA chay duoc. Chay tay de xem loi:  "%DEST%\TruckingSystem.API.exe"
  echo hoac xem Event Viewer -^> Windows Logs -^> Application.
)
pause
exit /b 0

:fail
echo.
echo CO LOI - xem thong bao phia tren.
pause
exit /b 1
