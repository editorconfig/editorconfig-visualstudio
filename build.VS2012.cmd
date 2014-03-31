@echo off

cd /d "%~p0"

set PATH=%PATH%;%ProgramFiles%\CMake 2.8\bin

cd Core
cmake . -G "Visual Studio 11" -DMSVC_MD=ON -DBUILD_DOCUMENTATION=OFF
IF ERRORLEVEL 1 EXIT /B 1
cd ..

set msbuild="%programfiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
set msbuildparams=/p:Configuration=Release /t:Rebuild /nologo /v:m

%msbuild% Core\editorconfig.sln %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

%msbuild% EditorConfig.VisualStudio.sln /p:PlatformToolset=v110 /p:Platform="Win32" %msbuildparams%
IF ERRORLEVEL 1 EXIT /B 1

echo.
pause
