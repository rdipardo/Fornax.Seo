@echo off
dotnet tool restore --tool-manifest src/Fornax/.config/dotnet-tools.json
dotnet test /v:m test/Fornax.Seo.Tests/Fornax.Seo.Tests.fsproj
cd example\Fornax.Seo.Example
build %1
cd ..\..
