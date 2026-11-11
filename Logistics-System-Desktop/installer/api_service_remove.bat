@echo off
REM Go dich vu TruckingSystemAPI (chuot phai -> Run as administrator). Khong xoa C:\TruckingSystem.
net session >nul 2>&1 || (echo Can chay bang quyen Administrator. & pause & exit /b 1)
sc stop TruckingSystemAPI >nul 2>&1
timeout /t 5 /nobreak >nul
sc delete TruckingSystemAPI
pause
