//
// Copyright (c) 2021 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at http://mozilla.org/MPL/2.0/.
//

namespace Fornax.Seo

/// Extends the Fornax DSL with SEO meta tag generators
[<AutoOpen>]
module Core =
    open Html
    open Tags
    open System
    open System.Text.RegularExpressions

    /// <summary>
    ///   Generates structured data and OpenGraph tags
    /// </summary>
    /// <returns>
    ///   A list of &lt;<c>meta</c>&gt; tags with OpenGraph data and a &lt;<c>script</c>&gt; tag with JSON-LD
    ///   structured data
    /// </returns>
    /// <exception cref="System.ArgumentException">
    ///   Thrown if either <paramref name="page"/>'s <c>ObjectType</c> or <c>ContentType</c> is not recognized by the
    ///   <a href="https://schema.org/docs/full.html">Schema.org specification</a>,  or if <c>OpenGraphType</c> is not
    ///   a recognized <a href="https://ogp.me/#types">OpenGraph object type</a>
    /// </exception>
    /// <remarks>
    ///   If <paramref name="page"/>'s <c>Url</c> is relative or malformed, it will be rejected with a warning message.
    ///   See <a href="https://stackoverflow.com/a/64830732"> this post</a> for advice on avoiding relative URLs in
    ///   structured data
    /// </remarks>
    let seo (page: ContentObject) : HtmlElement list =
        let author = page.Author
        let email = author.Email.Trim()
        let links = author.SocialMedia
        let jsonLd = JsonLinkData(page)
        let openGraph = OpenGraph({ page with ContentType = Some jsonLd.MainEntityOfPage.Schema })

        let tags = [ yield! openGraph.ToHtml(); jsonLd.ToHtml() ]

        let iconStyle = """
            .media-icon {
                font-size: 2.5rem;
                padding: 0.75rem;
            }
            a.navicon, a.navicon:focus, a.navicon:active, a.navicon:hover, a.navicon:visited {
                text-decoration: none;
                outline: 0;
            }
            .is-dark .media-icon,
            .is-dark a.navicon,
            .is-dark a.navicon:focus,
            .is-dark a.navicon:active,
            .is-dark a.navicon:hover {
                color: #eee;
            }
            """

        let minify markup = Regex.Replace(markup, @"(?<=[{}:;,])\s+", "").Trim()

        if List.isEmpty links && String.IsNullOrEmpty(email) then
            tags
        else
            tags
            @ [ link [ Rel "stylesheet"
                       Media "screen"
                       Href @"https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" ]
                style [] [ !!(minify iconStyle) ] ]


    /// <summary>
    ///   Generates links to the social media profiles of the given <paramref name="author"/>
    /// </summary>
    /// <returns>
    ///   A list of social media links decorated with matching Font Awesome icons
    /// </returns>
    let socialMedia (author: ContentCreator) : HtmlElement list =
        let siteAuthor = author.Name
        let email = author.Email.Trim()
        let socialMedia = author.SocialMedia

        let mediaIcons =
            [ ("bitbucket", "fa-bitbucket")
              ("deviantart", "fa-deviantart")
              ("facebook", "fa-facebook-official")
              ("flickr", "fa-flickr")
              ("gitlab", "fa-gitlab")
              ("github", "fa-github")
              ("instagram", "fa-instagram")
              ("linkedin", "fa-linkedin-square")
              ("meetup", "fa-meetup")
              ("pinterest", "fa-pinterest-square ")
              ("qq", "fa-qq")
              ("quora", "fa-quora")
              ("ravelry", "fa-ravelry")
              ("reddit", "fa-reddit")
              ("slack", "fa-slack")
              ("snapchat", "fa-snapchat-square")
              ("soundcloud", "fa-soundcloud")
              ("spotify", "fa-spotify")
              ("stackexchange", "fa-stack-exchange")
              ("stackoverflow", "fa-stack-overflow")
              ("t", "fa-telegram")
              ("telegram", "fa-telegram")
              ("tumblr", "fa-tumblr-square")
              ("twitter", "fa-twitter")
              ("wechat", "fa-weixin")
              ("weixin", "fa-weixin")
              ("weibo", "fa-weibo")
              ("whatsapp ", "fa-whatsapp")
              ("youtube", "fa-youtube-play") ]
            |> Map.ofList

        let links =
            socialMedia
            |> List.map (
                (fun lnk ->
                    let noMatch = (lnk, "", "fa-external-link")

                    (if Uri.IsWellFormedUriString(lnk, UriKind.RelativeOrAbsolute) then
                         let link =
                             lnk.Split([| '/'; '.' |], StringSplitOptions.RemoveEmptyEntries)
                             |> Array.tryFind (fun part -> Map.containsKey part mediaIcons)
                             |> function
                             | Some site -> site
                             | None -> lnk

                         Uri.TryCreate(lnk, UriKind.RelativeOrAbsolute)
                         |> function
                         | (true, uri) ->
                             let siteLink =
                                 if uri.IsAbsoluteUri then
                                     UriBuilder(
                                         uri.OriginalString,
                                         Scheme = Uri.UriSchemeHttps,
                                         Port = -1
                                     ).Uri.AbsoluteUri
                                 else
                                     $"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{uri.OriginalString}"
                                         .Replace("///", "//")

                             (siteLink, link, Map.tryFind link mediaIcons |> Option.defaultValue "fa-external-link")
                         | _ -> noMatch
                     else
                         noMatch))
                >> (fun siteInfo ->
                    let link, site, icon = siteInfo

                    let siteName =
                        match site with
                        | "t" -> "telegram"
                        | "" -> link
                        | _ -> site

                    let linkTitle =
                        (if String.IsNullOrEmpty(siteAuthor) then
                             String.Empty
                         else
                             $"Find {siteAuthor} on {siteName}")

                    a [ Href link; HtmlProperties.Title linkTitle; Class "navicon" ] [
                        i [ Class(($"media-icon fa {icon}")); Custom("aria-hidden", "true") ] []
                    ])
            )

        if not <| String.IsNullOrEmpty(email) then
            links
            @ [ a [ Href(Uri.UriSchemeMailto + ":" + email); Class "navicon" ] [
                    i [ Class("media-icon fa fa-envelope"); Custom("aria-hidden", "true") ] []
                ] ]
        else
            links
