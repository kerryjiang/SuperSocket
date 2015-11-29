@echo off

set msbuild="%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe"

%msbuild% SuperSocket.msbuild /t:Release-SuperSocketProtoBase

pause