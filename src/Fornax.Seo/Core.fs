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
        let tags = ResizeArray([ yield! openGraph.ToHtml(); jsonLd.ToHtml() ])

        let iconStyle = """
            .media-icon {
                font-size: 2.5rem;
                padding: 0.75rem;
            }
            .media-icon.x-twitter::after {
                content: '\1D54F';
                font-family: sans-serif;
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

        if not <| (List.isEmpty links && String.IsNullOrEmpty(email)) then
            tags.AddRange(
                [ link [ Rel "stylesheet"
                         Media "screen"
                         Href @"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.7.0/css/fontawesome.min.css" ]
                  link [ Rel "stylesheet"
                         Media "screen"
                         Href @"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.7.0/css/solid.min.css" ]
                  link [ Rel "stylesheet"
                         Media "screen"
                         Href @"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.7.0/css/brands.min.css" ]
                  style [] [ !!(minify iconStyle) ] ]
            )

        tags.ToArray() |> List.ofArray

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
            [ ("behance", ("Bēhance", "fa-behance-square"))
              ("bitbucket", ("Bitbucket", "fa-bitbucket"))
              ("bsky", ("Bluesky", "fa-bluesky"))
              ("deviantart", ("DeviantArt", "fa-deviantart"))
              ("discordapp", ("Discord", "fa-discord"))
              ("facebook", ("Facebook", "fa-facebook"))
              ("flickr", ("Flickr", "fa-flickr"))
              ("gitlab", ("GitLab", "fa-gitlab"))
              ("github", ("GitHub", "fa-github"))
              ("instagram", ("Instagram", "fa-instagram"))
              ("linkedin", ("LinkedIn", "fa-linkedin"))
              ("mastodon", ("Mastodon", "fa-mastodon"))
              ("meetup", ("Meetup", "fa-meetup"))
              ("pinterest", ("Pinterest", "fa-pinterest-square"))
              ("qq", ("Tencent QQ", "fa-qq"))
              ("quora", ("Quora", "fa-quora"))
              ("ravelry", ("Ravelry", "fa-ravelry"))
              ("reddit", ("Reddit", "fa-reddit"))
              ("scholar", ("Google Scholar", "fa-solid fa-graduation-cap"))
              ("slack", ("Slack", "fa-slack"))
              ("snapchat", ("Snapchat", "fa-snapchat-square"))
              ("soundcloud", ("SoundCloud", "fa-soundcloud"))
              ("sourceforge", ("SourceForge", "fa-solid fa-fire"))
              ("spotify", ("Spotify", "fa-spotify"))
              ("stackexchange", ("Stack Exchange", "fa-stack-exchange"))
              ("stackoverflow", ("Stack Overflow", "fa-stack-overflow"))
              ("t", ("Telegram", "fa-telegram"))
              ("telegram", ("Telegram", "fa-telegram"))
              ("tiktok", ("TikTok", "fa-tiktok"))
              ("tumblr", ("Tumblr", "fa-tumblr-square"))
              ("twitter", ("X", "fa-x-twitter"))
              ("viadeo", ("Viadeo", "fa-viadeo-square"))
              ("wechat", ("WeChat (微信)", "fa-weixin"))
              ("weixin", ("Weixin (微信)", "fa-weixin"))
              ("weibo", ("Weibo (新浪微博)", "fa-weibo"))
              ("whatsapp", ("WhatsApp", "fa-whatsapp"))
              ("x", ("X", "fa-x-twitter"))
              ("xing", ("Xing", "fa-xing-square"))
              ("youtube", ("YouTube", "fa-youtube")) ]
            |> Map.ofList

        let links =
            socialMedia
            |> List.map (
                (fun lnk ->
                    let defaultLinkIcon = "fa-solid fa-arrow-up-right-from-square"
                    let noMatch = (lnk, "", defaultLinkIcon)

                    (if Uri.IsWellFormedUriString(lnk, UriKind.RelativeOrAbsolute) then
                         let siteName =
                             lnk.Split([| '/'; '.' |], StringSplitOptions.RemoveEmptyEntries)
                             |> Array.tryFind (fun part -> Map.containsKey part mediaIcons)
                             |> function
                             | Some site -> site
                             | None -> lnk

                         let siteInfo = Map.tryFind siteName mediaIcons

                         Uri.TryCreate(lnk, UriKind.RelativeOrAbsolute)
                         |> function
                         | (true, uri) ->
                             let siteLink =
                                 if uri.IsAbsoluteUri then
                                     UriBuilder(
                                         uri.OriginalString,
                                         Scheme = Uri.UriSchemeHttps,
                                         Port = -1
                                     )
                                         .Uri
                                         .AbsoluteUri
                                 else
                                     $"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{uri.OriginalString}"
                                         .Replace("///", "//")

                             (siteLink,
                              siteInfo |> Option.defaultValue ("", "") |> fst,
                              siteInfo |> Option.defaultValue ("", defaultLinkIcon) |> snd)
                         | _ -> noMatch
                     else
                         noMatch))
                >> (fun siteInfo ->
                    let link, siteName, icon = siteInfo

                    let siteName =
                        match siteName with
                        | "" -> link
                        | _ -> siteName

                    let linkTitle =
                        (if String.IsNullOrEmpty(siteAuthor) then
                             String.Empty
                         else
                             $"Find {siteAuthor} on {siteName}")

                    let className =
                        List.tryFind
                            (fun c -> icon.Contains(c))
                            [ "fa-brands"; "fa-solid"; "fa-regular"; "fa-light"; "fa-thin" ]
                        |> function
                        | Some _ -> String.Empty
                        | None -> "fa-brands"

                    a [ Href link
                        HtmlProperties.Title linkTitle
                        Class "navicon"
                        HtmlProperties.Custom("aria-label", linkTitle) ] [
                        i [ Class(Regex.Replace($"media-icon {className} {icon}", @"\s{2,}", " ")) ] []
                    ])
            )

        if not <| String.IsNullOrEmpty(email) then
            let linkTitle = $"Contact {siteAuthor} at {email}"

            links
            @ [ a [ Href(Uri.UriSchemeMailto + ":" + email)
                    Class "navicon"
                    HtmlProperties.Custom("aria-label", linkTitle) ] [
                    i [ Class("media-icon fa-solid fa-envelope") ] []
                ] ]
        else
            links
