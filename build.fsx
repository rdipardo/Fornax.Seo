//
// Copyright (c) 2023 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.
//

#r "paket: groupref fake //"

#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#endif

open System.IO
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.Tools
open Fake.Core.TargetOperators
open Fake.IO.Globbing.Operators

#load "types.fsx"

type private CmdArg = Types.CommandArg
type private CmdProp = Types.CommandProperty
type private FsdocsParam = Types.FsdocsParameter
type private ArgList = Types.CommandArgList
type private PropList = Types.CommandPropertyList
type private FsdocsParamList = Types.FsdocsParameterList

/// <summary>
/// Retrieves the path to the XML descriptor and root directory of each managed project
/// </summary>
[<Struct>]
type private Project =
    static member TryGetFile(key) =
        [ ("Main", "src/Fornax.Seo/Fornax.Seo.fsproj")
          ("Test", "test/Fornax.Seo.Tests/Fornax.Seo.Tests.fsproj")
          ("Demo", "example/Fornax.Seo.Example/Fornax.Seo.Example.fsproj") ]
        |> Map.ofList
        |> Map.tryFind key

    static member File(key) =
        let file = Project.TryGetFile(key)
        defaultArg file ""

    static member Dir(key) =
        let file = Project.TryGetFile(key)
        defaultArg file __SOURCE_DIRECTORY__ |> Path.GetDirectoryName

let private msbuildParams =
    { MSBuild.CliArguments.Create() with
          // https://github.com/fsprojects/FAKE/issues/2722
          DisableInternalBinLog = true
          Verbosity = Some Minimal }

let private buildParams (opts: DotNet.BuildOptions) =
    { opts with MSBuildParams = msbuildParams; Configuration = DotNet.Release }

let private demoRunParams _ = { DotNet.Options.Create() with WorkingDirectory = Project.Dir("Demo") }

let private buildOrWatch (args: TargetParameter) =
    args.Context.Arguments
    |> List.tryFindBack (fun a -> "live".Equals(a))
    |> function
    | Some _ -> "watch"
    | None -> "build"

Target.initEnvironment ()

// --------------------------------------------------------------------------------------
// Build targets

Target.create "All" ignore

// --------------------------------------------------------------------------------------
Target.create "Build" (fun _ -> Project.File("Main") |> DotNet.build buildParams)

// --------------------------------------------------------------------------------------
Target.create
    "Test"
    (fun _ ->
        let result = Project.File("Test") |> DotNet.exec id "test"
        if not result.OK then failwith $"""{String.concat " " result.Messages}""")

// --------------------------------------------------------------------------------------
Target.create
    "Demo"
    (fun args ->
        let demoBuildParams (opts: DotNet.BuildOptions) =
            { buildParams (opts) with
                  OutputPath = Some(Path.Combine(Project.Dir("Demo"), "_lib")) }

        let fornaxCmd = buildOrWatch args

        Project.File("Demo") |> DotNet.build demoBuildParams

        DotNet.exec demoRunParams "fornax" fornaxCmd |> ignore)

// --------------------------------------------------------------------------------------
Target.create
    "Pack"
    (fun _ ->
        try
            let script = Path.Combine(__SOURCE_DIRECTORY__, "scripts", "release-notes.fsx")
            let releaseNotes = Path.Combine(__SOURCE_DIRECTORY__, "release", "notes.txt")
            let result = DotNet.exec id "fsi" $"/nologo /exec {script}"

            if not result.OK then
                failwith $"""{String.concat " " result.Messages}"""
            else
                let notes = File.ReadAllText(releaseNotes)

                Environment.setEnvironVar "PackageReleaseNotes" notes

            let sha = Git.Information.getCurrentHash ()
            let revName = Git.Information.showName __SOURCE_DIRECTORY__ sha

            let buildNumber =
                if revName.Contains("tags/") |> not then
                    revName.Split() |> Array.tryHead
                else
                    None

            let packParams (opts: DotNet.PackOptions) =
                { opts with
                      MSBuildParams = msbuildParams
                      Configuration = DotNet.Release
                      OutputPath = Some(Path.Combine(__SOURCE_DIRECTORY__, "release"))
                      VersionSuffix = buildNumber }

            Project.File("Main") |> DotNet.pack packParams
        with e -> failwith $"{e.Message}")

// --------------------------------------------------------------------------------------
Target.create
    "Docs"
    (fun args ->
        let runArg = buildOrWatch args
        let homepage = Path.Combine("docs", "index.md")
        let siteRoot = if Environment.hasEnvironVar ("CI") then "https://heredocs.io/" else "/"
        let gitBranch = Git.Information.getBranchName __SOURCE_DIRECTORY__
        let props = [ CmdProp("RepositoryBranch", gitBranch); CmdProp("Configuration", "Release") ]

        let fsdocParams =
            [ FsdocsParam("root", siteRoot, false)
              FsdocsParam("repository-link", "https://github.com/rdipardo/Fornax.Seo")
              FsdocsParam("logo-link", "https://www.nuget.org/packages/Fornax.Seo")
              FsdocsParam("license-link", "https://raw.githubusercontent.com/rdipardo/Fornax.Seo/main/LICENSE")
              FsdocsParam(
                  "release-notes-link",
                  "https://raw.githubusercontent.com/rdipardo/Fornax.Seo/main/CHANGELOG.md"
              ) ]

        let cmdArgs =
            [ CmdArg("projects", Path.GetFullPath(Project.File("Main")))
              CmdArg("properties", $"{PropList(props)}")
              CmdArg("parameters", $"{FsdocsParamList(fsdocParams)}")
              CmdArg("eval", "")
              CmdArg("clean", "")
              CmdArg("strict", "") ]

        try
            Shell.cleanDir "output"
            Shell.copyFile homepage "README.MD"

            let result = DotNet.exec id "fsdocs" $"{runArg} {ArgList(cmdArgs)}"

            if not result.OK then failwith $"""{String.concat " " result.Messages}"""
        finally
            Shell.rm homepage)

// --------------------------------------------------------------------------------------
Target.create
    "Format"
    (fun _ ->
        seq {
            yield! !! "**/*.fsx" -- ".fake/" -- "src/**" -- "example/**"
            yield! !! "src/Fornax.Seo/*.fs"
            yield! !! "test/Fornax.Seo.Tests/*.fs"
        }
        |> Seq.iter
            (fun src ->
                let result = DotNet.exec id "fantomas" $"--check {src}"
                if not result.OK then DotNet.exec id "fantomas" $"{src} --out {src}" |> ignore))

// --------------------------------------------------------------------------------------
Target.create
    "Clean"
    (fun _ ->
        seq {
            yield! !! "**/bin"
            yield! !! "**/obj"
            yield! !! "**/_lib"
        }
        |> Shell.cleanDirs)

// --------------------------------------------------------------------------------------
"Test"
=?> ("Docs", Environment.hasEnvironVar ("CI"))
=?> ("Clean", Environment.hasEnvironVar ("CI"))
==> "Pack"

"Test" =?> ("Demo", Environment.hasEnvironVar ("CI")) ==> "All"
"Build" ==> "Docs"

Target.runOrDefaultWithArguments "All"
