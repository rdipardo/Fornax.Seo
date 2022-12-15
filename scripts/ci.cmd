@echo off
dotnet test /v:m /p:nowarn=\"3218;3384;3390\" test/Fornax.Seo.Tests/Fornax.Seo.Tests.fsproj
IF NOT %ERRORLEVEL% == 0 (GOTO END)
pushd example\Fornax.Seo.Example
call build %1
popd
:END
