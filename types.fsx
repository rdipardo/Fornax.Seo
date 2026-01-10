//
// Copyright (c) 2023,2026 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.
//

/// <summary>
/// A generic command line argument
/// </summary>
type CommandArg(param: string, value: string) =
    member val Param = param
    member val Value = value

    override __.ToString() = $"--{__.Param} {__.Value}"

type CommandArgList<'T when 'T :> CommandArg>(args: 'T seq) =
    member val Args = args

    override __.ToString() = Seq.map string __.Args |> String.concat " "

/// <summary>
/// A generic command line property
/// </summary>
type CommandProperty(prop: string, value: string, ?prefix: string) =
    member val Prop = prop
    member val Value = value

    override __.ToString() = $"""{defaultArg prefix ""}{__.Prop}={__.Value}"""

type CommandPropertyList<'T when 'T :> CommandProperty>(props: 'T seq, ?delim: string) =
    member val Props = props

    override __.ToString() = Seq.map string __.Props |> String.concat (defaultArg delim " ")

/// <summary>
/// A command line parameter for the fsdocs tool
/// </summary>
type FsdocsParameter(param: string, value: string, ?prefix: bool) =
    inherit CommandArg(param, value)

    let prefixString = if (defaultArg prefix true) then "fsdocs-" else ""

    override __.ToString() = $"{prefixString}{__.Param} {__.Value}"

/// <summary>
/// A command line property for AltCover
/// </summary>
type AltCoverProperty(prop: string, value: string) =
    inherit CommandProperty(prop, value, prefix = "AltCover")
