@echo off
SETLOCAL enableDelayedExpansion
SET BUILD_NUMBER=
SET NL=^


dotnet fsi /exec scripts/release-notes.fsx

IF NOT EXIST ".\release\notes.txt" (GOTO END)

FOR /F "tokens=* USEBACKQ" %%F IN (`type release\notes.txt`) DO (
SET "PackageReleaseNotes=%%F"
)

git describe --tags 2>NUL:
IF NOT %ERRORLEVEL% == 0 (
  FOR /F "tokens=* USEBACKQ" %%F IN (`git rev-parse --short HEAD`) DO (
  SET "BUILD_NUMBER=--version-suffix %%F"
  )
)

dotnet restore src/Fornax.Seo/Fornax.Seo.fsproj
dotnet pack /v:m /p:nowarn=\"3218;3384;3390\" src/Fornax.Seo/Fornax.Seo.fsproj -c Release -o release %BUILD_NUMBER%

:END
ENDLOCAL
