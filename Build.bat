@echo off
rem *****Begin Comment*****
rem Builds CLP
rem *****End Comment*******
setlocal
cd /d "%~dp0"

rem MSBuild path
set msbuildexe="%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"

rem Directory paths
set localDirectory=%~dp0
set outputDirectory=%localDirectory%output
set buildsDirectory=%outputDirectory%\builds
set releasesDirectory=%outputDirectory%\releases
set x86Release=%releasesDirectory%\x86
set x64Release=%releasesDirectory%\x64
set anyCPURelease=%releasesDirectory%\AnyCPU

rem Clean paths
rmdir /q /s "%outputDirectory%" 1>nul 2>nul

rem Initialize paths
mkdir "%buildsDirectory%" 1>nul 2>nul
mkdir "%x86Release%" 1>nul 2>nul
mkdir "%x64Release%" 1>nul 2>nul
mkdir "%anyCPURelease%" 1>nul 2>nul

rem Set build platform: Release, Debug
rem "%~1" is the first arg
set buildPlatformConfiguration=Release
if "%~1" neq "" (set buildPlatformConfiguration=%~1)

echo Making Classroom Learning Partner: %buildPlatformConfiguration% Builds.
echo.

echo Building x86...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="x86" /p:WarningLevel=0 /v:q /t:rebuild

echo Building x64...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="x64" /p:WarningLevel=0 /v:q /t:rebuild

echo Building AnyCPU...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="Any CPU" /p:WarningLevel=0 /v:q /t:rebuild

if not "%ERRORLEVEL%"=="0" (echo ERROR: Build Failed. & goto Quit)
echo.

rem echo "Copy x64"
rem xcopy /y %outputDirectory%\x64\Release\*.pdb %x64Release% 1>nul 2>nul
rem xcopy /y %outputDirectory%\x64\Release\*.dll %x64Release% 1>nul 2>nul
rem xcopy /y %outputDirectory%\x64\Release\SoundSwitch.exe %x64Release% 1>nul 2>nul
rem xcopy /y %outputDirectory%\x64\Release\SoundSwitch.exe.config %x64Release% 1>nul 2>nul


rem echo "Copy x86"
rem xcopy /y %outputDirectory%\x86\Release\*.pdb %x86Release% 1>nul 2>nul
rem xcopy /y %outputDirectory%\x86\Release\*.dll %x86Release% 1>nul 2>nul
rem xcopy /y %outputDirectory%\x86\Release\SoundSwitch.* %x86Release% 1>nul 2>nul
rem xcopy /y %outputDirectory%\x86\Release\SoundSwitch.exe.config %x86Release% 1>nul 2>nul

echo.
echo All operations completed successfully.

:Quit
echo.
pause