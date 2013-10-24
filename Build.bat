@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% SuperSocket.2010.sln /p:Configuration=Debug /t:Clean;Rebuild /p:OutputPath=..\bin\Net40\Debug
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% SuperSocket.2010.sln /p:Configuration=Release /t:Clean;Rebuild /p:OutputPath=..\bin\Net40\Release
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% SuperSocket.2010.NET35.sln /p:Configuration=Debug /t:Clean;Rebuild /p:OutputPath=..\bin\Net35\Debug
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% SuperSocket.2010.NET35.sln /p:Configuration=Release /t:Clean;Rebuild /p:OutputPath=..\bin\Net35\Release
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\v4.0.30319\SKUs\.NETFramework,Version=v4.5" 2>nul
if errorlevel 0 (
    %msbuild% SuperSocket.2012.sln /p:Configuration=Debug /t:Clean;Rebuild /p:OutputPath=..\bin\Net45\Debug
	FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

	%msbuild% SuperSocket.2012.sln /p:Configuration=Release /t:Clean;Rebuild /p:OutputPath=..\bin\Net45\Release
	FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"
)

pause