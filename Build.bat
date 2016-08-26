@echo off
rem *****Begin Comment*****
rem Builds CLP
rem *****End Comment*******
setlocal
cd /d "%~dp0"

rem MSBuild path
set msbuildexe="%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"

git describe --abbrev=0 --tags > latestTag.txt
for /f "delims=" %%i in ('git rev-list HEAD --count') do set commitCount=%%i
set /p latestTag=<latestTag.txt
del latestTag.txt
set releaseVersion=%latestTag%.%commitCount%
echo %releaseVersion%

rem Directory paths
set localDirectory=%~dp0
set outputDirectory=%localDirectory%output
set buildsDirectory=%outputDirectory%\builds
set anyCPUBuild=%buildsDirectory%\AnyCPU
set x64Build=%buildsDirectory%\x64
set x86Build=%buildsDirectory%\x86
set releasesDirectory=%outputDirectory%\releases
set anyCPURelease=%releasesDirectory%\AnyCPU
set x64Release=%releasesDirectory%\x64
set x86Release=%releasesDirectory%\x86

rem Clean paths
rmdir /q /s "%outputDirectory%" 1>nul 2>nul

rem Initialize paths
mkdir "%buildsDirectory%" 1>nul 2>nul
mkdir "%anyCPURelease%" 1>nul 2>nul
mkdir "%x64Release%" 1>nul 2>nul
mkdir "%x86Release%" 1>nul 2>nul

rem Set build platform: Release, Debug
rem "%~1" is the first arg
set buildPlatformConfiguration=Release
if "%~1" neq "" (set buildPlatformConfiguration=%~1)

echo Making Classroom Learning Partner: %buildPlatformConfiguration% Builds.
echo.

echo Building AnyCPU...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="Any CPU" /p:WarningLevel=0 /v:q /t:rebuild

echo Building x64...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="x64" /p:WarningLevel=0 /v:q /t:rebuild

echo Building x86...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="x86" /p:WarningLevel=0 /v:q /t:rebuild

if not "%ERRORLEVEL%"=="0" (echo ERROR: Build Failed. & goto Quit)
echo.

echo Deploying AnyCPU...
mkdir "%anyCPURelease%\lib" 1>nul 2>nul
xcopy /y "%anyCPUBuild%\"*.dll "%anyCPURelease%\lib" 1>nul 2>nul
xcopy /y "%anyCPUBuild%\Classroom Learning Partner.exe" "%anyCPURelease%" 1>nul 2>nul
xcopy /y "%anyCPUBuild%\Classroom Learning Partner.exe.config" "%anyCPURelease%" 1>nul 2>nul

echo Deploying x64...
mkdir "%x64Release%\lib" 1>nul 2>nul
xcopy /y "%x64Build%\"*.dll "%x64Release%\lib" 1>nul 2>nul
xcopy /y "%x64Build%\Classroom Learning Partner.exe" "%x64Release%" 1>nul 2>nul
xcopy /y "%x64Build%\Classroom Learning Partner.exe.config" "%x64Release%" 1>nul 2>nul

echo Deploying x86...
mkdir "%x86Release%\lib" 1>nul 2>nul
xcopy /y "%x86Build%\"*.dll "%x86Release%\lib" 1>nul 2>nul
xcopy /y "%x86Build%\Classroom Learning Partner.exe" "%x86Release%" 1>nul 2>nul
xcopy /y "%x86Build%\Classroom Learning Partner.exe.config" "%x86Release%" 1>nul 2>nul

echo.
echo All operations completed successfully.

:Quit
echo.
pause