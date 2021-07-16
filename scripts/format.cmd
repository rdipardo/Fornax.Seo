@echo off
FOR /F "tokens=* USEBACKQ" %%F IN (`git ls-files src/Fornax.Seo/*.fs`) DO (
  dotnet fantomas --check "%%F"
  IF NOT %ERRORLEVEL% == 0 (
      dotnet fantomas "%%F" --out "%%F"
  )
)

FOR /F "tokens=* USEBACKQ" %%F IN (`git ls-files test/*.fs`) DO (
  dotnet fantomas --check "%%F"
  IF NOT %ERRORLEVEL% == 0 (
      dotnet fantomas "%%F" --out "%%F"
  )
)
