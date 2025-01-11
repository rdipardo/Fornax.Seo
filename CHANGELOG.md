## 1.4.0
- Update static assets to Font Awesome 6
- Provide new profile icons for Bluesky, Mastodon, TikTok, Discord, etc.

## 1.3.1
- Update icon for X/Twitter profiles

## 1.3.0
- Upgrade runtime target to .NET 8
- Drop FSharp.Data.DesignTime from distribution
- Include Source Link metadata and embedded symbols in release builds

## 1.2.0
- Drop the Fornax.Core.dll assembly from distribution
- Make the Fornax.Core NuGet package a runtime dependency
- Bump Newtonsoft.Json to 13.0.3
- Use FAKE to orchestrate development workflows
- Generate `aria-label` attributes for all social media links

## 1.1.1
- Bump FSharp.Data to 4.2.10
- Bump Newtonsoft.Json to 13.0.2
- Serialize JSON-LD `@context` property as a simple string
- Use a shorter timestamp format in JSON-LD and OpenGraph fields
- Make social media link titles more readable

## 1.1.0
- Upgrade runtime target to .NET 6 (current LTS)
- Bump Fornax.Core assembly version to 0.14.3
- Integrate the newly added `HtmlElement.Custom` case
- Provide icons for more social media profiles

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
