@echo off
SETLOCAL
SET "FSDOCS_CMD=build"
SET "SITE_ROOT=root /"
IF NOT "%CI%"=="" (
    SET "SITE_ROOT=root https://heredocs.io/"
)
IF "%1"=="live" (
    SET "FSDOCS_CMD=watch"
    SET SITE_ROOT=
)
dotnet tool restore
dotnet build /v:m /p:nowarn=\"3218;3390\" src/Fornax.Seo/Fornax.Seo.fsproj -c Release

copy README.md docs\index.md
dotnet fsdocs %FSDOCS_CMD% --projects %CD%/src/Fornax.Seo/Fornax.Seo.fsproj ^
    --eval --clean --strict ^
    --properties RepositoryBranch=main Configuration=Release ^
    --parameters %SITE_ROOT% ^
        fsdocs-logo-link https://www.nuget.org/packages/Fornax.Seo ^
        fsdocs-repository-link https://github.com/rdipardo/Fornax.Seo ^
        fsdocs-license-link https://raw.githubusercontent.com/rdipardo/Fornax.Seo/main/LICENSE ^
        fsdocs-release-notes-link https://raw.githubusercontent.com/rdipardo/Fornax.Seo/main/CHANGELOG.md

del docs\index.md
ENDLOCAL
