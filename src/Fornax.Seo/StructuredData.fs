//
// Copyright (c) 2021 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at http://mozilla.org/MPL/2.0/.
//

namespace Fornax.Seo

/// <summary>
/// Internal representations of SEO metadata objects
/// </summary>
/// <exclude />
module StructuredData =
    open Html
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq
    open Newtonsoft.Json.Serialization
    open System
    open System.IO
    open System.Net.Http

    let jsonOptions =
        JsonSerializerSettings(
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = CamelCasePropertyNamesContractResolver()
        )

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

    module internal SchemaDotOrg =
        let private Spec = @"https://schema.org/version/latest/schemaorg-current-https.jsonld"

        type TypeConverter() =
            inherit JsonConverter()

            override __.CanConvert(objType: Type) = true

            override __.ReadJson(reader: JsonReader, objType: Type, value: obj, serializer: JsonSerializer) =
                let node = JToken.Load(reader)

                match node.Type with
                | JTokenType.String -> node.ToObject<string>()
                | JTokenType.Array ->
                    node.ToObject<string array>()
                    |> Array.tryPick (fun (v: string) -> if v.StartsWith("rdfs:") then Some v else None)
                    |> Option.defaultValue null
                    |> box
                | _ -> value

            override __.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
                serializer.Serialize(writer, value)

        type RdfsLabelConverter() =
            inherit TypeConverter()

            override __.ReadJson(reader: JsonReader, objType: Type, value: obj, serializer: JsonSerializer) =
                let node = JToken.Load(reader)

                match node.Type with
                | JTokenType.Object -> (node.ToObject<RdfsLabel>()).Value |> box
                | _ -> base.ReadJson(reader, objType, value, serializer)

        and [<Struct>] RdfsLabel =
            { [<JsonProperty("@language")>]
              mutable Language: string
              [<JsonProperty("@value")>]
              mutable Value: string }

        type Schema() =
            [<JsonProperty("@context")>]
            member val Context = { Rdf = ""; Rdfs = ""; Schema = ""; Xsd = "" } with get, set
            [<JsonProperty("@graph")>]
            member val Graph = [| { Id = ""; Type = ""; RdfsLabel = "" } |] with get, set

        and [<Struct>] Context =
            { [<JsonProperty("rdf")>]
              mutable Rdf: string
              [<JsonProperty("rdfs")>]
              mutable Rdfs: string
              [<JsonProperty("schema")>]
              mutable Schema: string
              [<JsonProperty("xsd")>]
              mutable Xsd: string }

        and [<Struct>] GraphNode =
            { [<JsonProperty("@id")>]
              mutable Id: string
              [<JsonProperty("@type")>]
              [<JsonConverter(typeof<TypeConverter>)>]
              mutable Type: string
              [<JsonProperty("rdfs:label")>]
              [<JsonConverter(typeof<RdfsLabelConverter>)>]
              mutable RdfsLabel: string }

        type SchemaProvider() =
            static let client = new HttpClient()
            let mutable schema = Schema()

            let initSchema =
                task {
                    try
                        let! res = client.GetAsync(Spec)

                        if res.IsSuccessStatusCode then
                            let! body = res.Content.ReadAsStreamAsync()
                            use stream = new StreamReader(body, System.Text.UTF8Encoding())
                            schema <- JsonConvert.DeserializeObject<Schema>(stream.ReadToEnd(), jsonOptions)
                        else
                            eprintfn $"Requesting {Spec} failed with message \"{res.ReasonPhrase}\""
                    with exc ->
                        let srcPath = $"{Path.Combine(__SOURCE_DIRECTORY__, __SOURCE_FILE__)},{__LINE__}"
                        eprintfn $"{exc.GetType().Name}: {exc.Message} ({srcPath})"
                }

            do initSchema.Wait()

            member val Context = schema.Context
            member val Graph = schema.Graph
            override __.Finalize() = client.Dispose()
