## 1.0.0
- Fix issues affecting binary compatibility with the current version of the fornax tool
- Dynamically set the "generator" `meta` property from the file version of the Fornax.Core assembly

## 0.1.1
- Fix faulty comparison that allowed empty strings to match any OpenGraph type
- Move the `StructuredData` module to a separate file and make some of its members value types
- Return interpolated strings wherever `sprintf` was used

## 0.1.0
- Bump FSharp.Data to 4.1.1
- Generate absolute URLs to all social media links

## 0.1.0-rc.1
Bump Newtonsoft.Json to 13.0.1

## 0.1.0-a60fd1f
Start bundling FSharp.Data.DesignTime.dll since consuming F# scripts will be broken without it.
See, for example, <https://stackoverflow.com/q/3102472>

## 0.1.0-d8291ee (DEPRECATED)
Initial preview release
