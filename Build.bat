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
set anyCPUBuildRelease=%buildsDirectory%\Release\AnyCPU
set x64BuildRelease=%buildsDirectory%\Release\x64
set x86BuildRelease=%buildsDirectory%\Release\x86
set anyCPUBuildStandalone=%buildsDirectory%\Standalone\AnyCPU
set x64BuildStandalone=%buildsDirectory%\Standalone\x64
set x86BuildStandalone=%buildsDirectory%\Standalone\x86
set releasesDirectory=%outputDirectory%\2-releases
set anyCPURelease=%releasesDirectory%\Release\AnyCPU
set x64Release=%releasesDirectory%\Release\x64
set x86Release=%releasesDirectory%\Release\x86
set anyCPUStandalone=%releasesDirectory%\Standalone\AnyCPU
set x64Standalone=%releasesDirectory%\Standalone\x64
set x86Standalone=%releasesDirectory%\Standalone\x86
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
mkdir "%anyCPUStandalone%" 1>nul 2>nul
mkdir "%packagesDirectory%" 1>nul 2>nul
mkdir "%installerDirectory%" 1>nul 2>nul
echo Directories initialized.
echo.

rem Set Version
set /a dailyBuildVersion=0
for /f %%i in ('cscript "%scriptsDirectory%\DateVersionGenerator.vbs" //Nologo') do set dateVersion=%%i
for /f "delims=" %%i in ('git rev-parse --short HEAD') do set shortHash=%%i
set assemblyVersion=%dateVersion%.%dailyBuildVersion%

rem Increment dailyBuildVersion
:DoWhile
if not exist "%installerDirectory%\CLP Setup - %assemblyVersion%.exe" (goto EndDoWhile)
set /a dailyBuildVersion=%dailyBuildVersion% + 1
set assemblyVersion=%dateVersion%.%dailyBuildVersion%
goto DoWhile
:EndDoWhile

set hashVersion=%dateVersion%.%dailyBuildVersion%-r%shortHash%

rem Remove last line from VersionAssemblyInfo.cs
set versionAssemblyInfo=%localDirectory%VersionAssemblyInfo.cs
set tempVersionFile=%localDirectory%VersionAssemblyInfo.tmp

rem if exist "%tempVersionFile%" del /q "%tempVersionFile%"
echo // This file is overwritten during the build process. No changes you make will persist. > "%versionAssemblyInfo%"
echo using System.Reflection; >> "%versionAssemblyInfo%"
echo [assembly: AssemblyVersion("%assemblyVersion%")] >> "%versionAssemblyInfo%"
echo [assembly: AssemblyInformationalVersion("%hashVersion%")] >> "%versionAssemblyInfo%"

echo Making Classroom Learning Partner: Release and Standalone Builds. Version %hashVersion%.
echo.

echo Building Release Configuration...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=Release /p:Platform="Any CPU" /p:WarningLevel=0 /v:q /t:rebuild

if not "%ERRORLEVEL%"=="0" (echo ERROR: Release Build Failed. & goto Quit)
echo Release Configuration built.
echo.

echo Building Standalone Configuration...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=Standalone /p:Platform="Any CPU" /p:WarningLevel=0 /v:q /t:rebuild

if not "%ERRORLEVEL%"=="0" (echo ERROR: Standalone Build Failed. & goto Quit)
echo Standalone Configuration built.
echo.

echo Releasing AnyCPU Release...
mkdir "%anyCPURelease%\lib" 1>nul 2>nul
xcopy /y "%anyCPUBuildRelease%\"*.dll "%anyCPURelease%\lib" 1>nul 2>nul
xcopy /y "%anyCPUBuildRelease%\Classroom Learning Partner.exe" "%anyCPURelease%" 1>nul 2>nul
xcopy /y "%anyCPUBuildRelease%\Classroom Learning Partner.exe.config" "%anyCPURelease%" 1>nul 2>nul
echo AnyCPU Release released.
echo.

echo Releasing AnyCPU Standalone...
mkdir "%anyCPUStandalone%\lib" 1>nul 2>nul
xcopy /y "%anyCPUBuildStandalone%\"*.dll "%anyCPUStandalone%\lib" 1>nul 2>nul
xcopy /y "%anyCPUBuildStandalone%\Classroom Learning Partner.exe" "%anyCPUStandalone%" 1>nul 2>nul
xcopy /y "%anyCPUBuildStandalone%\Classroom Learning Partner.exe.config" "%anyCPUStandalone%" 1>nul 2>nul
echo AnyCPU Standalone released.
echo.

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