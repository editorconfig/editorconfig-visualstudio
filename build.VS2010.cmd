@echo off

cd /d "%~p0"

set PATH=%PATH%;%ProgramFiles%\CMake 2.8\bin

cd Core
cmake . -G "Visual Studio 10" -DMSVC_MD=ON -DBUILD_DOCUMENTATION=OFF
IF ERRORLEVEL 1 EXIT /B 1
cd ..

set msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m

%msbuild% Core\editorconfig.sln %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

%msbuild% EditorConfig.VisualStudio.sln /p:PlatformToolset=v100 /p:Platform="Win32" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

echo.
pause
