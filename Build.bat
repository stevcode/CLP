@echo off
rem Builds CLP
setlocal
cd /d "%~dp0"

rem MSBuild path
set msbuildexe="%programfiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

rem Directory paths
set localDirectory=%~dp0
set iconFile="%localDirectory%Classroom Learning Partner\Resources\Images\Icons\CLPPaperClipLogoCircled.ico"
set scriptsDirectory=%localDirectory%tools\Scripts
set outputDirectory=%localDirectory%output
set versionTrackingDirectory=%outputDirectory%\version-tracking
mkdir "%versionTrackingDirectory%" 1>nul 2>nul

echo Calculating Latest Version...
rem Set Version
set /a dailyBuildVersion=0
for /f %%i in ('cscript "%scriptsDirectory%\DateVersionGenerator.vbs" //Nologo') do set dateVersion=%%i

set assemblyVersion=%dateVersion%.%dailyBuildVersion%

rem Increment dailyBuildVersion
:DoWhile
if not exist "%versionTrackingDirectory%\%assemblyVersion%.version" (goto EndDoWhile)
set /a dailyBuildVersion=%dailyBuildVersion% + 1
set assemblyVersion=%dateVersion%.%dailyBuildVersion%
goto DoWhile
:EndDoWhile
echo Latest Version Calculated.
echo.

echo Updating Version Assembly Info...

type nul >"%versionTrackingDirectory%\%assemblyVersion%.version"

for /f "delims=" %%i in ('git rev-parse --short HEAD') do set shortHash=%%i
set hashVersion=%assemblyVersion%-r%shortHash%

rem Remove last line from VersionAssemblyInfo.cs
set versionAssemblyInfo=%localDirectory%VersionAssemblyInfo.cs
set tempVersionFile=%localDirectory%VersionAssemblyInfo.tmp

rem if exist "%tempVersionFile%" del /q "%tempVersionFile%"
echo // This file is overwritten during the build process. No changes you make will persist. > "%versionAssemblyInfo%"
echo using System.Reflection; >> "%versionAssemblyInfo%"
echo [assembly: AssemblyVersion("%assemblyVersion%")] >> "%versionAssemblyInfo%"
echo [assembly: AssemblyInformationalVersion("%hashVersion%")] >> "%versionAssemblyInfo%"

echo Version Assembly Info Updated.
echo.

rem More directory paths
set buildsDirectory=%outputDirectory%\1-builds
set TeacherBuildDirectory=%buildsDirectory%\Teacher
set StudentBuildDirectory=%buildsDirectory%\Student
set ProjectorBuildDirectory=%buildsDirectory%\Projector
set releasesDirectory=%outputDirectory%\2-releases
set TeacherReleaseDirectory=%releasesDirectory%\Teacher-%assemblyVersion%
set StudentReleaseDirectory=%releasesDirectory%\Student-%assemblyVersion%
set ProjectorReleaseDirectory=%releasesDirectory%\Projector-%assemblyVersion%

echo Cleaning Build and Release Directories...
rmdir /q /s "%buildsDirectory%" 1>nul 2>nul
rmdir /q /s "%releasesDirectory%" 1>nul 2>nul
echo Directories Cleaned.
echo.

echo Initializing Build and Release Directories...
mkdir "%buildsDirectory%" 1>nul 2>nul
mkdir "%TeacherBuildDirectory%" 1>nul 2>nul
mkdir "%StudentBuildDirectory%" 1>nul 2>nul
mkdir "%ProjectorBuildDirectory%" 1>nul 2>nul
mkdir "%releasesDirectory%" 1>nul 2>nul
mkdir "%TeacherReleaseDirectory%" 1>nul 2>nul
mkdir "%StudentReleaseDirectory%" 1>nul 2>nul
mkdir "%ProjectorReleaseDirectory%" 1>nul 2>nul
echo Directories Initialized.
echo.

echo Making Classroom Learning Partner: Release Builds. Version %hashVersion%.
echo.

echo Building Teacher Configuration...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=Teacher /p:Platform="Any CPU" /p:WarningLevel=0 /v:q /t:rebuild

if not "%ERRORLEVEL%"=="0" (echo ERROR: Teacher Build Failed. & goto Quit)
echo Teacher Configuration Built.
echo.

echo Building Student Configuration...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=Student /p:Platform="Any CPU" /p:WarningLevel=0 /v:q /t:rebuild

if not "%ERRORLEVEL%"=="0" (echo ERROR: Student Build Failed. & goto Quit)
echo Student Configuration Built.
echo.

echo Building Projector Configuration...
%msbuildexe% "Classroom Learning Partner.sln" /nologo /m /p:Configuration=Projector /p:Platform="Any CPU" /p:WarningLevel=0 /v:q /t:rebuild

if not "%ERRORLEVEL%"=="0" (echo ERROR: Projector Build Failed. & goto Quit)
echo Projector Configuration Built.
echo.

echo Releasing Teacher Build...
mkdir "%TeacherReleaseDirectory%\lib" 1>nul 2>nul
xcopy /y "%TeacherBuildDirectory%\"*.dll "%TeacherReleaseDirectory%\lib" 1>nul 2>nul
xcopy /y "%TeacherBuildDirectory%\Classroom Learning Partner.exe" "%TeacherReleaseDirectory%" 1>nul 2>nul
xcopy /y "%TeacherBuildDirectory%\Classroom Learning Partner.exe.config" "%TeacherReleaseDirectory%" 1>nul 2>nul
echo Teacher Build Released.
echo.

echo Releasing Student Build...
mkdir "%StudentReleaseDirectory%\lib" 1>nul 2>nul
xcopy /y "%StudentBuildDirectory%\"*.dll "%StudentReleaseDirectory%\lib" 1>nul 2>nul
xcopy /y "%StudentBuildDirectory%\Classroom Learning Partner.exe" "%StudentReleaseDirectory%" 1>nul 2>nul
xcopy /y "%StudentBuildDirectory%\Classroom Learning Partner.exe.config" "%StudentReleaseDirectory%" 1>nul 2>nul
echo Student Build Released.
echo.

echo Releasing Teacher Build...
mkdir "%ProjectorReleaseDirectory%\lib" 1>nul 2>nul
xcopy /y "%ProjectorBuildDirectory%\"*.dll "%ProjectorReleaseDirectory%\lib" 1>nul 2>nul
xcopy /y "%ProjectorBuildDirectory%\Classroom Learning Partner.exe" "%ProjectorReleaseDirectory%" 1>nul 2>nul
xcopy /y "%ProjectorBuildDirectory%\Classroom Learning Partner.exe.config" "%ProjectorReleaseDirectory%" 1>nul 2>nul
echo Teacher Build Released.
echo.

echo All operations completed successfully.

:Quit
echo.
pause