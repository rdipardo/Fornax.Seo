#r "../_lib/Fornax.Core.dll"
#r "../_lib/Fornax.Seo.dll"
#if !FORNAX
#load "../loaders/postloader.fsx"
#load "../loaders/pageloader.fsx"
#load "../loaders/globalloader.fsx"
#endif

open Html
open Fornax.Seo
open System

let private injectWebsocketCode (webpage: string) =
    let websocketScript = script [ Src "/js/websocket.js"; Defer true ] []
    let head = "<head>"
    let index = webpage.IndexOf head
    let tag = HtmlElement.ToString websocketScript
    webpage.Insert((index + head.Length + 1), sprintf "\t%s\n" tag)

let layout (ctx: SiteContents) (active: string) (content: HtmlElement seq) =
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo>()
    let siteAuthor = ctx.TryGetValue<ContentCreator>() |> Option.defaultValue ContentCreator.Default
    let links = socialMedia siteAuthor
    let pageTitle = siteInfo |> Option.map (fun si -> si.title) |> Option.defaultValue ""
    let tagline = siteInfo |> Option.map (fun si -> si.description) |> Option.defaultValue ""

    let siteRoot =
        siteInfo
        |> Option.map (fun si -> si.baseUrl)
        |> Option.defaultValue ContentObject.Default.BaseUrl

    let subtitle =
        if String.IsNullOrEmpty(active) then
            if String.IsNullOrEmpty(tagline) then "" else $" | {tagline}"
        else
            $" | {active}"

    let pages = ctx.TryGetValues<Pageloader.Page>() |> Option.defaultValue Seq.empty

    let menuEntries =
        pages
        |> Seq.map
            (fun p ->
                let cls = if p.title = active then "navbar-item is-active" else "navbar-item"
                a [ Class cls; Href p.link ] [ !!p.title ])
        |> Seq.toList

    let seoData = ctx.TryGetValues<ContentObject>() |> Option.defaultValue Seq.empty

    let pageMeta =
        seoData
        |> Seq.tryFind (fun p -> p.Title.Contains(active))
        |> function
        | Some info -> info
        | _ ->
            { ContentObject.Default with
                  Title = pageTitle
                  Description = tagline
                  BaseUrl = siteRoot
                  SiteName = Some pageTitle
                  Headline = Some tagline
                  Author = siteAuthor }

    let footer =
        seq {
            footer [ Class "is-dark" ] [
                div [ Class "columns" ] [
                    div [ Class "stacked" ] [
                        p [] [
                            !!($"&copy;&nbsp;%d{DateTime.Now.Year}&nbsp;")
                            !!siteAuthor.Name
                        ]
                    ]
                    (if not <| List.isEmpty links then
                         div [ Class "stacked" ] [ p [ Class "contact" ] links ]
                     else
                         div [] [])
                    div [ Class "stacked" ] [
                        p [] [
                            !! "Site generated with "
                            a [ Href "https://ionide.io/Tools/fornax.html" ] [ !! "Fornax" ]
                        ]
                    ]
                ]
            ]
        }

    let scripts = seq { script [ Src "/js/bulma.js"; Defer true ] [] }

    html [ Lang "en" ] [
        head [] [
            meta [ CharSet "utf-8" ]
            meta [ Name "viewport"; Content "width=device-width, initial-scale=1" ]
            title [] [ !!($"{pageTitle}{subtitle}") ]
            link [ Rel "icon"
                   HtmlProperties.Type "image/png"
                   Sizes "32x32"
                   Href "/images/favicon.png" ]
            link [ Rel "stylesheet"
                   Href "https://fonts.googleapis.com/css?family=Open+Sans" ]
            link [ Rel "stylesheet"
                   Href "https://unpkg.com/bulma@0.8.0/css/bulma.min.css" ]
            link [ Rel "stylesheet"
                   HtmlProperties.Type "text/css"
                   Href "/style/style.css" ]
            yield! seo pageMeta
        ]
        body [] [
            nav [ Class "navbar is-dark" ] [
                div [ Class "container" ] [
                    div [ Class "navbar-brand" ] [
                        a [ Class "navbar-item"; Href "/" ] [
                            img [ Src "/images/bulma.png"; Alt "Logo" ]
                        ]
                        span [ Class "navbar-burger burger"; Custom("data-target", "navbarMenu") ] [
                            span [] []
                            span [] []
                            span [] []
                        ]
                    ]
                    div [ Id "navbarMenu"; Class "navbar-menu" ] menuEntries
                ]
            ]
            yield! content
            yield! footer
            yield! scripts
        ]
    ]

let render (ctx: SiteContents) cnt =
    let disableLiveRefresh =
        ctx.TryGetValue<Postloader.PostConfig>()
        |> Option.map (fun n -> n.disableLiveRefresh)
        |> Option.defaultValue false

    cnt
    |> HtmlElement.ToString
    |> sprintf "<!DOCTYPE html>\n%s"
    |> fun n -> if disableLiveRefresh then n else injectWebsocketCode n

let published (post: Postloader.Post) =
    post.published
    |> Option.defaultValue System.DateTime.Now
    |> fun n -> n.ToString("yyyy-MM-dd")

let postLayout (useSummary: bool) (post: Postloader.Post) =
    div [ Class "card article" ] [
        div [ Class "card-content" ] [
            div [ Class "media-content has-text-centered" ] [
                p [ Class "title article-title" ] [ a [ Href post.link ] [ !!post.title ] ]
                p [ Class "subtitle is-6 article-subtitle" ] [
                    a [ Href "#" ] [ !!(defaultArg post.author "") ]
                    !!(sprintf "on %s" (published post))
                ]
            ]
            div [ Class "content article-body" ] [
                !!(if useSummary then post.summary else post.content)
            ]
        ]
    ]
