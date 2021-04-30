@echo off
SETLOCAL
SET "FSDOCS_CMD=build"
SET "SITE_ROOT=root /"
IF NOT "%CI%"=="" (
    SET "SITE_ROOT=root https://heredocs.io/Fornax.Seo/"
)
IF "%1"=="live" (
    SET "FSDOCS_CMD=watch"
    SET SITE_ROOT=
)
dotnet tool restore
dotnet build /v:m /p:nowarn="3218 3390" src/Fornax.Seo/Fornax.Seo.fsproj -c Release

copy README.md docs\index.md
dotnet fsdocs %FSDOCS_CMD% --projects %CD%/src/Fornax.Seo/Fornax.Seo.fsproj ^
    --eval --clean --strict ^
    --properties RepositoryBranch=main Configuration=Release ^
    --parameters %SITE_ROOT% ^
        fsdocs-logo-link https://www.nuget.org/packages/Fornax.Seo ^
        fsdocs-repository-link https://github.com/rdipardo/Fornax.Seo ^
        fsdocs-license-link https://github.com/rdipardo/Fornax.Seo/blob/main/LICENSE ^
        fsdocs-release-notes-link https://github.com/rdipardo/Fornax.Seo/blob/main/CHANGELOG.md

del docs\index.md
ENDLOCAL
