echo off
setlocal
echo ************
echo cert setup starting
echo ************

cd Tool

call :setscriptvariables %1
IF NOT DEFINED SUPPORTED_MODE call :displayusage
IF DEFINED SUPPORTED_MODE call :cleancerts
IF DEFINED SETUP_SERVICE call :setupservice
GOTO end

:cleancerts
REM cleans up certs from previous runs.
echo ****************
echo Cleanup starting
echo ****************

echo -------------------------
echo del client certs
echo -------------------------
certmgr.exe -del -r CurrentUser -s TrustedPeople -c -n %SERVER_NAME%

echo -------------------------
echo del service certs
echo -------------------------
certmgr.exe -put -r LocalMachine -s My -c -n %SERVER_NAME% ..\GiantSocketServer.cer
IF %ERRORLEVEL% EQU 0 (
   DEL computer.cer
   echo ****************
   echo "You have a certificate with a Subject name matching your Machine name: %SERVER_NAME%"
   echo "If this certificate is from a cross machine run of WCF samples press any key to delete it."
   echo "Otherwise press Ctrl + C to abort this script."
   pause
   certmgr.exe -del -r LocalMachine -s My -c -n %SERVER_NAME%
)

:cleanupcompleted
echo *****************
echo Cleanup completed
echo *****************

GOTO :EOF

:setupservice

echo ************
echo Server cert setup starting
echo %SERVER_NAME%
echo ************
echo making server cert
echo ************

makecert.exe -sr LocalMachine -ss MY -a sha1 -n CN=%SERVER_NAME% -sky exchange -pe

IF DEFINED EXPORT_SERVICE (
    echo ************
    echo exporting service cert to GiantSocketServer.cer
    echo ************
    certmgr.exe -put -r LocalMachine -s My -c -n %SERVER_NAME% ..\GiantSocketServer.cer
) ELSE (
    echo ************
    echo copying server cert to client's CurrentUser store
    echo ************
    certmgr.exe -add -r LocalMachine -s My -c -n %SERVER_NAME% -r CurrentUser -s TrustedPeople
)
GOTO :EOF

:setscriptvariables
REM Parses the input to determine if we are setting this up for a single machine, client, or server
REM sets the appropriate name variables
SET SUPPORTED_MODE=1
SET SETUP_SERVICE=1
SET EXPORT_SERVICE=1
SET SERVER_NAME=%1
IF [%1]==[] SET SERVER_NAME=GiantSocketServer
ECHO %SERVER_NAME%

GOTO :EOF


:displayusage
ECHO Correct usage:
ECHO     Service Machine - MakeCert.bat GiantSocketServer
:end

pause
