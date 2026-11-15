@echo off
setlocal EnableExtensions
REM ============================================================================
REM  SAO LUU TruckingSystem: co so du lieu + anh/video/giay to + file cau hinh
REM  Chay tay (double-click / cmd quyen Admin) hoac de Task Scheduler chay moi dem.
REM ============================================================================

REM ---------------- CAU HINH: chi sua cac dong nay ----------------
set DB_NAME=TruckingSystemDb
set PG_USER=postgres
set PG_HOST=localhost
set PG_PORT=5432
set BACKUP_DIR=D:\TruckingBackup
set KEEP=14
set PG_BIN=C:\Program Files\PostgreSQL\18\bin
set APP_DIR=C:\TruckingSystem
REM Mat khau PostgreSQL KHONG ghi o day: dat trong file %APP_DIR%\pgpass.conf
REM (1 dong:  localhost:5432:*:postgres:MAT_KHAU )
REM ----------------------------------------------------------------

set PGPASSFILE=%APP_DIR%\pgpass.conf
if not exist "%PG_BIN%\pg_dump.exe" (
  echo [LOI] Khong thay pg_dump.exe trong "%PG_BIN%". Sua dong PG_BIN cho dung phien ban PostgreSQL.
  exit /b 1
)
if not exist "%PGPASSFILE%" (
  echo [LOI] Chua co file mat khau %PGPASSFILE%. Xem huong dan HUONG_DAN_SAO_LUU.txt.
  exit /b 1
)

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmm"') do set TS=%%i
set DEST=%BACKUP_DIR%\%TS%
set LOG=%BACKUP_DIR%\backup.log
if not exist "%DEST%" mkdir "%DEST%"

echo ================================================== >> "%LOG%"
echo [%TS%] Bat dau sao luu >> "%LOG%"
echo [1/4] Sao luu co so du lieu %DB_NAME% ...
"%PG_BIN%\pg_dump.exe" -h %PG_HOST% -p %PG_PORT% -U %PG_USER% -w -F c -f "%DEST%\%DB_NAME%.dump" %DB_NAME% 2>> "%LOG%"
if errorlevel 1 (
  echo [LOI] pg_dump that bai - xem %LOG%
  echo [%TS%] LOI: pg_dump that bai >> "%LOG%"
  exit /b 1
)

echo [2/4] Sao luu anh, video, giay to (chi chep file moi) ...
REM Thu muc Storage dung CHUNG cho moi lan sao luu: chi chep them file moi, khong xoa,
REM nen khong ton dung luong nhu chep lai toan bo moi ngay.
robocopy "%APP_DIR%\Storage" "%BACKUP_DIR%\Storage" /E /R:2 /W:5 /NP /NFL /NDL >> "%LOG%"
if errorlevel 8 (
  echo [LOI] Chep thu muc Storage that bai - xem %LOG%
  echo [%TS%] LOI: robocopy Storage >> "%LOG%"
  exit /b 1
)

echo [3/4] Sao luu file cau hinh ...
if exist "%APP_DIR%\appsettings.Production.json" copy /y "%APP_DIR%\appsettings.Production.json" "%DEST%\" >nul
if exist "%APP_DIR%\localhost.pfx" copy /y "%APP_DIR%\localhost.pfx" "%DEST%\" >nul

echo [4/4] Chi giu %KEEP% ban sao luu co so du lieu gan nhat ...
powershell -NoProfile -Command "Get-ChildItem -LiteralPath '%BACKUP_DIR%' -Directory | Where-Object { $_.Name -match '^\d{8}-\d{4}$' } | Sort-Object Name -Descending | Select-Object -Skip %KEEP% | Remove-Item -Recurse -Force"

echo [%TS%] Xong: %DEST% >> "%LOG%"
echo.
echo XONG. Ban sao luu: %DEST%
echo Anh/video/giay to: %BACKUP_DIR%\Storage
exit /b 0
