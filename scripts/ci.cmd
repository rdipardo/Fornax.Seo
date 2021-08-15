@echo off
dotnet test /v:m /p:nowarn=\"3218;3390\" test/Fornax.Seo.Tests/Fornax.Seo.Tests.fsproj
IF NOT %ERRORLEVEL% == 0 (GOTO END)
cd example\Fornax.Seo.Example
build %1
cd ..\..
:END
