@echo off
setlocal EnableExtensions
REM ============================================================================
REM  KHOI PHUC TruckingSystem tu 1 ban sao luu do db_backup.bat tao ra.
REM  Chay bang cmd QUYEN ADMIN.
REM  Cach dung:
REM    db_restore.bat                      -> liet ke cac ban sao luu
REM    db_restore.bat 20261115-2300        -> khoi phuc THU vao CSDL rieng (an toan)
REM    db_restore.bat 20261115-2300 THAT   -> khoi phuc DE LEN CSDL dang chay
REM ============================================================================

REM ---------------- CAU HINH: giong db_backup.bat ----------------
set DB_NAME=TruckingSystemDb
set PG_USER=postgres
set PG_HOST=localhost
set PG_PORT=5432
set BACKUP_DIR=D:\TruckingBackup
set PG_BIN=C:\Program Files\PostgreSQL\18\bin
set APP_DIR=C:\TruckingSystem
set SVC=TruckingSystemAPI
REM ----------------------------------------------------------------

set PGPASSFILE=%APP_DIR%\pgpass.conf

if "%~1"=="" (
  echo Cac ban sao luu hien co trong %BACKUP_DIR%:
  dir /b /ad "%BACKUP_DIR%\2*"
  echo.
  echo Chay lai:  db_restore.bat ^<ten ban sao luu^>         ^(khoi phuc thu, an toan^)
  echo       hoac db_restore.bat ^<ten ban sao luu^> THAT    ^(de len du lieu dang chay^)
  exit /b 0
)

set SRC=%BACKUP_DIR%\%~1
if not exist "%SRC%\%DB_NAME%.dump" (
  echo [LOI] Khong thay "%SRC%\%DB_NAME%.dump"
  exit /b 1
)

if /i not "%~2"=="THAT" goto TEST

REM ------------------------- KHOI PHUC THAT -------------------------
echo.
echo  CANH BAO: Du lieu HIEN TAI cua %DB_NAME% se bi THAY bang ban sao luu %~1.
echo  Moi thay doi sau thoi diem sao luu se MAT.
set /p OK=Go CO de tiep tuc: 
if /i not "%OK%"=="CO" ( echo Da huy. & exit /b 0 )

echo [1/4] Sao luu nhanh du lieu hien tai truoc khi de (phong khi can quay lai) ...
call "%~dp0db_backup.bat"

echo [2/4] Dung dich vu API ...
sc stop %SVC% >nul 2>&1
timeout /t 5 /nobreak >nul

echo [3/4] Khoi phuc co so du lieu ...
"%PG_BIN%\pg_restore.exe" -h %PG_HOST% -p %PG_PORT% -U %PG_USER% -w --clean --if-exists --no-owner -d %DB_NAME% "%SRC%\%DB_NAME%.dump"
if errorlevel 1 echo [CHU Y] pg_restore bao canh bao/loi - doc thong bao o tren.

echo [4/4] Chep lai anh/video/giay to con thieu va chay lai dich vu ...
robocopy "%BACKUP_DIR%\Storage" "%APP_DIR%\Storage" /E /R:2 /W:5 /NP /NFL /NDL >nul
sc start %SVC%
echo XONG. Mo ung dung kiem tra lai.
exit /b 0

REM ------------------------- KHOI PHUC THU -------------------------
:TEST
set TEST_DB=%DB_NAME%_restore_test
echo Khoi phuc THU vao CSDL rieng: %TEST_DB% (khong dung toi du lieu dang chay)
"%PG_BIN%\dropdb.exe" -h %PG_HOST% -p %PG_PORT% -U %PG_USER% -w --if-exists %TEST_DB%
"%PG_BIN%\createdb.exe" -h %PG_HOST% -p %PG_PORT% -U %PG_USER% -w %TEST_DB%
if errorlevel 1 ( echo [LOI] Khong tao duoc %TEST_DB% & exit /b 1 )
"%PG_BIN%\pg_restore.exe" -h %PG_HOST% -p %PG_PORT% -U %PG_USER% -w --no-owner -d %TEST_DB% "%SRC%\%DB_NAME%.dump"
if errorlevel 1 echo [CHU Y] pg_restore bao canh bao/loi - doc thong bao o tren.
echo.
echo Kiem tra so luong don va chuyen trong ban khoi phuc thu:
"%PG_BIN%\psql.exe" -h %PG_HOST% -p %PG_PORT% -U %PG_USER% -w -d %TEST_DB% -c "SELECT (SELECT count(*) FROM \"TransportOrders\") AS so_don, (SELECT count(*) FROM \"Trips\") AS so_chuyen;"
echo.
echo Xong khoi phuc thu. Neu so lieu khop voi ung dung la ban sao luu dung duoc.
echo Xoa CSDL thu khi khong can:  "%PG_BIN%\dropdb.exe" -U %PG_USER% %TEST_DB%
exit /b 0
