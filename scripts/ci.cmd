@echo off
dotnet test /v:m /p:nowarn=\"3218;3390\" test/Fornax.Seo.Tests/Fornax.Seo.Tests.fsproj
cd example\Fornax.Seo.Example
build %1
cd ..\..
