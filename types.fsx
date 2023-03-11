//
// Copyright (c) 2023 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.
//

let private toParamString (args: 'T seq when 'T :> obj) = Seq.map (fun a -> $"{a}") args |> String.concat " "

/// <summary>
/// A generic command line argument
/// <summary>
type CommandArg(param: string, value: string) =
    member val Param = param
    member val Value = value

    override __.ToString() = $"--{__.Param} {__.Value}"

type CommandArgList(args: CommandArg seq) =
    member val Args = args

    override __.ToString() = toParamString args

/// <summary>
/// A generic command line property
/// <summary>
type CommandProperty(prop: string, value: string) =
    member val Prop = prop
    member val Value = value

    override __.ToString() = $"{__.Prop}={__.Value}"

type CommandPropertyList(props: CommandProperty seq) =
    member val Props = props

    override __.ToString() = toParamString props

/// <summary>
/// A command line parameter for the fsdocs tool
/// <summary>
type FsdocsParameter(param: string, value: string, ?prefix: bool) =
    inherit CommandArg(param, value)

    let prefixString = if (defaultArg prefix true) then "fsdocs-" else ""

    override __.ToString() = $"{prefixString}{__.Param} {__.Value}"

type FsdocsParameterList(opts: FsdocsParameter seq) =
    member val opts = opts

    override __.ToString() = toParamString opts
