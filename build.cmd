@echo off
dotnet tool restore
dotnet paket restore
dotnet fsi --langversion:8.0 --shadowcopyreferences+ -d:FAKE build.fsx %*
