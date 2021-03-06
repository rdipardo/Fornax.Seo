@echo off
SETLOCAL
SET "FORNAX_CMD=build"
IF "%1"=="live" ( SET "FORNAX_CMD=watch" )
dotnet build /v:m /p:nowarn=\"3218;3390\" -o _lib -c Release
dotnet build /v:m ../../src/Fornax/src/Fornax/Fornax.fsproj -o ../bin -c Release
dotnet ../bin/Fornax.dll %FORNAX_CMD%
ENDLOCAL
