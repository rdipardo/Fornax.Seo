//
// Copyright (c) 2021 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at http://mozilla.org/MPL/2.0/.
//

namespace Fornax.Seo

/// Internal representations of SEO metadata objects
/// <exclude />
module StructuredData =
    open Html
    open Newtonsoft.Json

    [<Struct>]
    type SchemaDotOrgContext =
        val Rdf: string
        val Rdfs: string
        val Schema: string
        val Xsd: string
        new(cxt: SchemaProvider.Context) = { Rdf = cxt.Rdf; Rdfs = cxt.Rdfs; Schema = cxt.Schema; Xsd = cxt.Xsd }

    [<Struct>]
    type MainEntity =
        { [<JsonProperty("@type")>]
          Schema: string
          [<JsonProperty("@id")>]
          Id: string }

    [<Struct>]
    type Author =
        { [<JsonProperty("@type")>]
          Schema: string
          Name: string
          [<JsonProperty("sameAs")>]
          Links: string list }

    type OpenGraphArticle =
        { Published_time: string
          Modified_time: string
          Author: string
          Tags: string list option }

        member this.ToHtml() =
            let tags =
                defaultArg this.Tags []
                |> List.map (fun t -> meta [ Property "article:tag"; Content t ])

            this.GetType().GetProperties()
            |> Array.filter
                (fun prop ->
                    not <| obj.ReferenceEquals(prop.GetValue(this, null), null)
                    && not <| obj.ReferenceEquals(prop.GetValue(this, null), None)
                    && prop.Name <> "Tags")
            |> Array.map (fun prop -> "article:" + prop.Name.ToLowerInvariant(), prop.GetValue(this, null).ToString())
            |> (Map.ofArray
                >> Map.fold (fun lst p c -> lst @ [ meta [ Property p; Content c ] ]) tags)
