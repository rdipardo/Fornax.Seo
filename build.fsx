//
// Copyright (c) 2023,2024,2026 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.
//

#load ".paket/load/netstandard2.0/fake/Fake.Core.Target.fsx"
#load ".paket/load/netstandard2.0/fake/Fake.DotNet.Cli.fsx"
#load ".paket/load/netstandard2.0/fake/Fake.Tools.Git.fsx"

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
type private AltCoverProp = Types.AltCoverProperty
type private ArgList = Types.CommandArgList<CmdArg>
type private PropList = Types.CommandPropertyList<CmdProp>
type private FsdocsParamList = Types.CommandArgList<FsdocsParam>
type private AltCoverPropList = Types.CommandPropertyList<AltCoverProp>

// https://github.com/fsprojects/FAKE/issues/2719#issuecomment-1563725381
System.Environment.GetCommandLineArgs()
|> (Array.skipWhile (fun (x: string) -> x.Equals(__SOURCE_FILE__) |> not)
    >> Array.tail
    >> Array.toList)
|> Context.FakeExecutionContext.Create false __SOURCE_FILE__
|> Context.RuntimeContext.Fake
|> Context.setExecutionContext

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

let CI_BUILD = Environment.hasEnvironVar ("CI")

// --------------------------------------------------------------------------------------
// Build targets

Target.create "All" ignore

// --------------------------------------------------------------------------------------
Target.create "Build" (fun _ -> Project.File("Main") |> DotNet.build buildParams)

// --------------------------------------------------------------------------------------
Target.create
    "Test"
    (fun _ ->
        let cmdArgs =
            if CI_BUILD |> not then
                id
            else
                (fun (options: DotNet.Options) ->
                    let props =
                        [ AltCoverProp("FailFast", "true")
                          AltCoverProp("LocalSource", "true")
                          AltCoverProp("ShowGenerated", "false")
                          AltCoverProp("SourceLink", "true")
                          AltCoverProp("Trivia", "false")
                          AltCoverProp("VisibleBranches", "true")
                          AltCoverProp("ReportFormat", "OpenCover")
                          AltCoverProp("AssemblyExcludeFilter", $"""{"(Test)s?$"}""")
                          AltCoverProp("TypeFilter", $"""{"^.*(StartupCode\$||Pipe\s#).*$"}""")
                          AltCoverProp("MethodFilter", $"""{"^.*(op_||Invoke||MoveNext).*$"}""")
                          AltCoverProp("Report", Path.Combine(__SOURCE_DIRECTORY__, "coverage.xml")) ]

                    { options with
                          CustomParams =
                              [ string <| CmdProp("/p:AltCover", "true")
                                string <| AltCoverPropList(props, ";") ]
                              |> (String.concat ";" >> Some) })

        let result = Project.File("Test") |> DotNet.exec cmdArgs "test"
        if not result.OK then failwith $"""{String.concat " " result.Messages}""")

// --------------------------------------------------------------------------------------
Target.create
    "Demo"
    (fun args ->
        let fornaxCmd = buildOrWatch args

        let (|Ok|Error|) (proc: ProcessResult) = if proc.OK then Ok else Error(proc.Messages)

        match (DotNet.exec demoRunParams "paket" "update") with
        | Ok ->
            match (DotNet.exec demoRunParams "paket" "restore") with
            | Ok -> DotNet.exec demoRunParams "fornax" fornaxCmd |> ignore
            | Error errs -> failwith $"""{String.concat " " errs}"""
        | Error errs -> failwith $"""{String.concat " " errs}""")

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

            let msbuildParams =
                // make packages more Source-Link-friendly
                if CI_BUILD then
                    { msbuildParams with
                          Properties =
                              msbuildParams.Properties
                              @ [ ("ContinuousIntegrationBuild", "true"); ("EmbedAllSources", "true") ] }
                else
                    msbuildParams

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
        let siteRoot = if CI_BUILD then "https://rdipardo.github.io/Fornax.Seo/" else "/"
        let props = [ CmdProp("RepositoryBranch", "main"); CmdProp("Configuration", "Release") ]

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
              CmdArg("output", Path.Combine(__SOURCE_DIRECTORY__, "site"))
              CmdArg("properties", $"{PropList(props)}")
              CmdArg("parameters", $"{FsdocsParamList(fsdocParams)}")
              CmdArg("eval", "")
              CmdArg("clean", "")
              CmdArg("strict", "") ]

        try
            Shell.cleanDir "site"
            Shell.copyFile homepage "README.md"

            let result = DotNet.exec id "fsdocs" $"{runArg} {ArgList(cmdArgs)}"

            if not result.OK then failwith $"""{String.concat " " result.Messages}"""
        finally
            Shell.rm homepage)

// --------------------------------------------------------------------------------------
Target.create
    "Format"
    (fun _ ->
        seq {
            yield! !! "**/*.fsx" -- ".fake/" -- ".paket/**" -- "example/**"
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
"Test" =?> ("Clean", CI_BUILD) ==> "Pack"

"Test" =?> ("Demo", CI_BUILD) ==> "All"

"Pack" ==> "Demo"

"Build" ==> "Docs"

Target.getArguments ()
|> (Option.defaultValue Array.empty >> Array.tryHead >> Option.defaultValue "All")
|> Target.runOrDefaultWithArguments
