@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% SuperSocket.ClientEngine.2010.sln /p:Configuration=Debug /t:Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% SuperSocket.ClientEngine.2010.sln /p:Configuration=Release /t:Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

pause