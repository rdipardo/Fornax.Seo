//
// Copyright (c) 2021 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at http://mozilla.org/MPL/2.0/.
//

namespace Fornax.Seo

/// Internal representations of HTML elements generated from SEO metadata
/// <exclude />
module Tags =
    open Html
    open StructuredData
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq
    open System
    open System.Diagnostics

    let private Schemata = SchemaDotOrg.SchemaProvider()

    let private getSchemaOrgType name =
        Schemata.Graph
        |> Array.tryFind (fun s -> s.Id = $"schema:{name}")
        |> function
        | Some schema -> schema.RdfsLabel
        | None -> invalidArg name "Invalid Schema.org type! See https://schema.org/docs/full.html"

    let private getOpenGraphType (name: string) =
        [ "website"
          "article"
          "book"
          "profile"
          "music.song"
          "music.album"
          "music.playlist"
          "music.radio_station"
          "video.movie"
          "video.episode"
          "video.tv_show"
          "video.other" ]
        |> List.tryFind (fun typ -> name.ToLowerInvariant().Equals(typ))
        |> function
        | Some typ -> typ
        | None -> invalidArg name "Invalid OpenGraph type! See https://ogp.me/#types"

    let private tryGetResource (meta: Map<string, string>) (key: string) =
        meta
        |> Map.fold (fun m k v -> Map.add (k.ToLowerInvariant()) v m) Map.empty
        |> Map.tryFind (key.ToLowerInvariant())
        |> function
        | Some src -> if String.IsNullOrEmpty(src) then null else src
        | None -> null

    let private parseHost (url: string) =
        Uri.TryCreate(url, UriKind.Absolute)
        |> function
        | (true, u) when List.contains u.Scheme [ Uri.UriSchemeHttps; Uri.UriSchemeHttp ] ->
            UriBuilder(u, Port = -1).Uri.AbsoluteUri
        | _ ->
            let srcPath = $"{System.IO.Path.Combine(__SOURCE_DIRECTORY__, __SOURCE_FILE__)},{__LINE__}"
            eprintfn $"WARNING: Can't resolve domain from {url}' - use an absolute URL instead ({srcPath})"
            null

    let private parseUrl root url canonical =
        parseHost root
        |> function
        | null -> null
        | baseUrl ->
            let domain = UriBuilder(baseUrl, Fragment = "", Scheme = Uri.UriSchemeHttps)

            Uri.TryCreate(url, UriKind.RelativeOrAbsolute)
            |> function
            | (false, _) -> String.Empty
            | (true, uri) ->
                let resource = if uri.IsAbsoluteUri then uri.AbsolutePath else uri.OriginalString

                let path =
                    if canonical then
                        resource.Split([| '/'; '#'; '?'; '&' |], StringSplitOptions.RemoveEmptyEntries)
                        |> Array.takeWhile
                            (fun part ->
                                [ "index.htm"; "index.html"; "index.php" ]
                                |> (List.contains <| part.ToLowerInvariant())
                                |> not)
                        |> String.concat "/"
                    else
                        resource

                $"""{domain.Scheme}{Uri.SchemeDelimiter}{domain.Host}{("/" + path).Replace("//", "/")}"""

    let private toIsoDateString (date: DateTime option) =
        date
        |> function
        | Some d -> d.ToString("yyyy-MM-ddTHH:mm:ssK")
        | None -> null

    let private dateModified (page: ContentObject) =
        page.Published
        |> function
        | Some _ -> defaultArg page.Modified DateTime.Now |> Some
        | None -> None

    type JsonLinkData(page: ContentObject) =
        let author = page.Author
        let links = author.SocialMedia
        let headline = defaultArg page.Headline null
        let objectType = defaultArg page.ObjectType "WebSite"
        let contentType = defaultArg page.ContentType "Article"
        let url = parseUrl page.BaseUrl page.Url false
        let metaOpt = defaultArg page.Meta [ ("", "") ] |> Map.ofList

        [<JsonProperty("@context")>]
        member val Context: string = Schemata.Context.Schema

        [<JsonProperty("@type")>]
        member val Schema: string = getSchemaOrgType objectType

        member val MainEntityOfPage = { Schema = getSchemaOrgType contentType; Id = url }
        member val Url = url
        member val Name = page.Title
        member val Description = page.Description
        member val Headline = headline
        member val Author = { Schema = "Person"; Name = author.Name; Links = links }
        member val DatePublished = toIsoDateString page.Published
        member val DateModified = toIsoDateString (dateModified page)

        override this.ToString() =
            let jsonLD = JObject.Parse(JsonConvert.SerializeObject(this, Formatting.None, jsonOptions))
            let toCamelCase (str: string) = $"{Char.ToLowerInvariant(str.[0])}{str.Substring(1)}".Replace("_", "")

            metaOpt
            |> Map.iter
                (fun p _ ->
                    let value = tryGetResource metaOpt p

                    if not <| obj.ReferenceEquals(value, null) then
                        jsonLD.Add(JProperty(toCamelCase p, value))
                    else
                        ())

            JsonConvert.SerializeObject(jsonLD, Formatting.None, jsonOptions)

        member this.ToHtml() = script [ HtmlProperties.Type "application/ld+json" ] [ !!this.ToString() ]


    type OpenGraph(page: ContentObject) =
        let masthead =
            page.Title.Split([| '|'; '-' |], StringSplitOptions.RemoveEmptyEntries)
            |> Array.tryHead
            |> function
            | Some t -> t.Trim()
            | None -> page.Title

        let isArticle =
            [ "blog"; "news"; "page"; "post"; "about" ]
            |> List.tryFind (fun p -> page.Url.ToLowerInvariant().Contains(p))
            |> Option.isSome

        let url = parseUrl page.BaseUrl page.Url false
        let canonical = parseUrl page.BaseUrl page.Url true
        let lang = defaultArg page.Locale "en_US"
        let metaOpt = defaultArg page.Meta [ ("", "") ] |> Map.ofList
        let fornaxVersionInfo = FileVersionInfo.GetVersionInfo((typeof<HtmlElement>).Assembly.Location)

        member val Title = page.Title
        member val Type = (defaultArg page.OpenGraphType "article") |> getOpenGraphType
        member val Description = page.Description
        member val Site_name = defaultArg page.SiteName masthead
        member val Url = url
        member val Locale = lang.Replace('-', '_')
        member val Image = tryGetResource metaOpt "image"

        member this.ToHtml() =
            (this.GetType().GetProperties()
             |> Array.filter (fun prop -> not <| obj.ReferenceEquals(prop.GetValue(this, null), null))
             |> Array.map (fun prop -> "og:" + prop.Name.ToLowerInvariant(), prop.GetValue(this, null).ToString())
             |> (Map.ofArray
                 >> Map.fold (fun lst p c -> lst @ [ meta [ Property p; Content c ] ]) []))
            @ [ meta [ Name "generator"; Content $"fornax v{fornaxVersionInfo.FileVersion}" ]
                meta [ Name "description"; Content this.Description ]
                meta [ Name "author"; Content page.Author.Name ]
                meta [ Name "twitter:card"; Content "summary" ]
                meta [ Property "twitter:title"; Content this.Title ] ]
              @ (if isArticle then
                     let article =
                         { Published_time = toIsoDateString page.Published
                           Modified_time = toIsoDateString (dateModified page)
                           Author = page.Author.Name
                           Tags = page.Tags }

                     article.ToHtml()
                 else
                     [])
                @ (if not <| String.IsNullOrEmpty(canonical) then
                       [ link [ Rel "canonical"; Href canonical ] ]
                   else
                       [])
