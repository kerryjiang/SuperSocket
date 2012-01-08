@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% ClientEngine\SuperSocket.ClientEngine.csproj /p:Configuration=Debug /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% ClientEngine\SuperSocket.ClientEngine.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% ClientEngine\SuperSocket.ClientEngine.Net35.csproj /p:Configuration=Debug /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% ClientEngine\SuperSocket.ClientEngine.Net35.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% ClientEngine\SuperSocket.ClientEngine.Mono.csproj /p:Configuration=Debug /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% ClientEngine\SuperSocket.ClientEngine.Mono.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% ClientEngine\SuperSocket.ClientEngine.Silverlight.csproj /p:Configuration=Debug /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% ClientEngine\SuperSocket.ClientEngine.Silverlight.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% ClientEngine\SuperSocket.ClientEngine.WindowsPhone.csproj /p:Configuration=Debug /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% ClientEngine\SuperSocket.ClientEngine.WindowsPhone.csproj /p:Configuration=Release /t:Clean;Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\supersocket.snk
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

pause