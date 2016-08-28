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
set iconFile="%localDirectory%Classroom Learning Partner\Resources\Images\Icons\CLPPaperClipLogoCircled.ico"
set nuGetDirectory=%localDirectory%tools\NuGet
set scriptsDirectory=%localDirectory%tools\Scripts
set outputDirectory=%localDirectory%output
set buildsDirectory=%outputDirectory%\1-builds
set anyCPUBuild=%buildsDirectory%\AnyCPU
set x64Build=%buildsDirectory%\x64
set x86Build=%buildsDirectory%\x86
set releasesDirectory=%outputDirectory%\2-releases
set anyCPURelease=%releasesDirectory%\AnyCPU
set x64Release=%releasesDirectory%\x64
set x86Release=%releasesDirectory%\x86
set packagesDirectory=%outputDirectory%\3-packages
set installerDirectory=%outputDirectory%\4-installer

echo Cleaning directories...
rmdir /q /s "%buildsDirectory%" 1>nul 2>nul
rmdir /q /s "%releasesDirectory%" 1>nul 2>nul
echo Directories cleaned.
echo.

echo Initializing directories...
mkdir "%buildsDirectory%" 1>nul 2>nul
mkdir "%anyCPURelease%" 1>nul 2>nul
mkdir "%x64Release%" 1>nul 2>nul
mkdir "%x86Release%" 1>nul 2>nul
mkdir "%packagesDirectory%" 1>nul 2>nul
mkdir "%installerDirectory%" 1>nul 2>nul
echo Directories initialized.
echo.

rem Set build platform: Release, Debug
rem "%~1" is the first arg
set buildPlatformConfiguration=Release
if "%~1" neq "" (set buildPlatformConfiguration=%~1)

rem Set Version
set dailyBuildVersion=0
for /f %%i in ('cscript "%scriptsDirectory%\DateVersionGenerator.vbs" //Nologo') do set dateVersion=%%i
for /f "delims=" %%i in ('git rev-parse --short HEAD') do set shortHash=%%i
set assemblyVersion=%dateVersion%.%dailyBuildVersion%
set hashVersion=%dateVersion%.%dailyBuildVersion%-r%shortHash%

rem Remove last line from VersionAssemblyInfo.cs
set versionAssemblyInfo=%localDirectory%VersionAssemblyInfo.cs
set tempVersionFile=%localDirectory%VersionAssemblyInfo.tmp

rem if exist "%tempVersionFile%" del /q "%tempVersionFile%"
echo // This file is overwritten during the build process. No changes you make will persist. > "%versionAssemblyInfo%"
echo using System.Reflection; >> "%versionAssemblyInfo%"
echo [assembly: AssemblyVersion("%assemblyVersion%")] >> "%versionAssemblyInfo%"
echo [assembly: AssemblyInformationalVersion("%hashVersion%")] >> "%versionAssemblyInfo%"

echo Making Classroom Learning Partner: %buildPlatformConfiguration% Builds. Version %hashVersion%.
echo.

echo Building AnyCPU Configuration...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="Any CPU" /p:WarningLevel=0 /v:q /t:rebuild

rem echo Building x64...
rem %msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="x64" /p:WarningLevel=0 /v:q /t:rebuild

rem echo Building x86...
rem %msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=%buildPlatformConfiguration% /p:Platform="x86" /p:WarningLevel=0 /v:q /t:rebuild

if not "%ERRORLEVEL%"=="0" (echo ERROR: Build Failed. & goto Quit)
echo AnyCPU Configuration built.
echo.

echo Releasing AnyCPU...
mkdir "%anyCPURelease%\lib" 1>nul 2>nul
xcopy /y "%anyCPUBuild%\"*.dll "%anyCPURelease%\lib" 1>nul 2>nul
xcopy /y "%anyCPUBuild%\Classroom Learning Partner.exe" "%anyCPURelease%" 1>nul 2>nul
xcopy /y "%anyCPUBuild%\Classroom Learning Partner.exe.config" "%anyCPURelease%" 1>nul 2>nul
echo AnyCPU released.
echo.

rem echo Deploying x64...
rem mkdir "%x64Release%\lib" 1>nul 2>nul
rem xcopy /y "%x64Build%\"*.dll "%x64Release%\lib" 1>nul 2>nul
rem xcopy /y "%x64Build%\Classroom Learning Partner.exe" "%x64Release%" 1>nul 2>nul
rem xcopy /y "%x64Build%\Classroom Learning Partner.exe.config" "%x64Release%" 1>nul 2>nul

rem echo Deploying x86...
rem mkdir "%x86Release%\lib" 1>nul 2>nul
rem xcopy /y "%x86Build%\"*.dll "%x86Release%\lib" 1>nul 2>nul
rem xcopy /y "%x86Build%\Classroom Learning Partner.exe" "%x86Release%" 1>nul 2>nul
rem xcopy /y "%x86Build%\Classroom Learning Partner.exe.config" "%x86Release%" 1>nul 2>nul

rem Create the NuGet package for Squirrel to use
echo Packaging CLP...
"%nuGetDirectory%\nuget" pack "%localDirectory%CLPDeployment.nuspec" -Version %assemblyVersion% -OutputDirectory "%packagesDirectory%"

if not "%ERRORLEVEL%"=="0" (echo ERROR: Creating the NuGet Package Failed. & goto Quit)
echo CLP packaged.
echo.

rem Attempt to build the installer using Squirrel
echo Creating CLP Installer...
set squirrelDirectory=%localDirectory%packages\squirrel.windows.1.4.4\tools
"%squirrelDirectory%\Squirrel.exe" --releasify "%packagesDirectory%"\ClassroomLearningPartner.%assemblyVersion%.nupkg --releaseDir "%installerDirectory%" --setupIcon "%~dp0CLPPaperClipLogoCircled.ico"

if not "%ERRORLEVEL%"=="0" (echo ERROR: Creating the installer failed. Check Squirrel version is still 1.4.4. & goto Quit)

move /y "%installerDirectory%\Setup.exe" "%installerDirectory%\CLP Setup - %assemblyVersion%.exe" >nul
move /y "%installerDirectory%\Setup.msi" "%installerDirectory%\CLP Setup - %assemblyVersion%.msi" >nul

echo CLP Installer created.
echo.

echo All operations completed successfully.

:Quit
echo.
pause