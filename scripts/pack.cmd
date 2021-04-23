@echo off
SETLOCAL enableDelayedExpansion
SET BUILD_NUMBER=
SET NL=^


dotnet fsi /exec scripts/release-notes.fsx

DIR /B release\notes.txt >NUL
IF NOT %ERRORLEVEL% == 0 (GOTO END)

FOR /F "tokens=* USEBACKQ" %%F IN (`type release\notes.txt`) DO (
SET "PackageReleaseNotes=%%F"
)

git describe >NUL
IF NOT %ERRORLEVEL% == 0 (
  FOR /F "tokens=* USEBACKQ" %%F IN (`git rev-parse --short HEAD`) DO (
  SET "BUILD_NUMBER=--version-suffix %%F"
  )
)

dotnet tool restore --tool-manifest src/Fornax/.config/dotnet-tools.json
dotnet restore src/Fornax.Seo/Fornax.Seo.fsproj
dotnet build --no-restore src/Fornax.Seo/Fornax.Seo.fsproj -c Release -o release
dotnet pack src/Fornax.Seo/Fornax.Seo.fsproj -c Release -o release %BUILD_NUMBER%

:END
ENDLOCAL
