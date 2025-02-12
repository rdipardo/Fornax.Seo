namespace Fornax.Seo.Tests

module StringAssert =
    let Contains (toFind: string, toSearch: string) = NUnit.Framework.Assert.That(toSearch.Contains(toFind))

module UnitTests =
    open NUnit.Framework
    open Html
    open HtmlAgilityPack
    open Fornax.Seo
    open Fornax.Seo.Tags
    open System.Diagnostics

    type internal TestKind =
        | Seo
        | Meta

    [<TestFixture>]
    type UnitTest() =
        let twitterLink = "https://twitter.com/someBody/"
        let xLink = "https://x.com/someBody/"
        let unmapped = "dev.to/webmaster"

        let links =
            [ "acmeinc.enterprise.slack.com"
              "https://story.snapchat.com/u/webmaster"
              "https://open.spotify.com/user/er811nzvdw2cy2qgkrlei9sqe/playlist/2lzTTRqhYS6AkHPIvdX9u3?si=KcZxfwiIR7OBPCzj20utaQ"
              "ell.stackexchange.com/users/00001/webmaster"
              "https://t.me/s/webmaster"
              "https://www.linkedin.com/in/webmaster"
              unmapped
              twitterLink
              xLink
              "www.discordapp.com/users/some1"
              "https://bsky.app/profile/no1"
              "https://mastodon.social/@nob0dy"
              "tiktok.com/@0x7fffffff"
              "patreon.com/404"
              "xing.com/someBodyElse"
              "https://www.behance.net/some1else"
              "https://scholar.google.com/citations?user=0X_qweryt24YUp"
              "sourceforge.net/u/some1/profile/" ]

        let pageAuthor = { Name = "Webmaster"; Email = "some1@example.com"; SocialMedia = links }

        let pageInfo =
            { Title = "A New Blog Posting"
              BaseUrl = "https://1fabblog"
              Url = "/news/new-post/2021/05/03/index.php#main_content"
              Description = "The latest posting to my regular blog"
              SiteName = Some "My Blog"
              Headline = Some "My Informative, Amusing Blog"
              ObjectType = Some "Blog"
              ContentType = Some "BlogPosting"
              OpenGraphType = Some "video.episode"
              Locale = Some "en-ca"
              Author = pageAuthor
              Published = Some System.DateTime.Now
              Modified = None
              Tags = Some [ "advice"; "careers"; "blogging" ]
              Meta = Some [ ("Image", "/avatar.jpg"); ("Publisher", pageAuthor.Name) ] }

        let pageMeta, authorMeta = (HtmlDocument(), HtmlDocument())

        do
            pageMeta.LoadHtml(head [] [ yield! seo pageInfo ] |> HtmlElement.ToString)
            authorMeta.LoadHtml(div [] [ yield! socialMedia pageAuthor ] |> HtmlElement.ToString)

        member private __.TryFindTagName (content: HtmlElement list) (tagName: string) =
            let doc = HtmlDocument()
            div [] content |> HtmlElement.ToString |> doc.LoadHtml
            doc.DocumentNode.SelectSingleNode($"//{tagName}") |> Option.ofObj

        member private __.TryFindTag (kind: TestKind) (xpath: string) =
            let doc = [ (Seo, pageMeta); (Meta, authorMeta) ] |> (Map.ofList >> Map.find kind)
            doc.DocumentNode.SelectSingleNode(xpath) |> Option.ofObj

        member private __.XPathFor(siteName, className, ?variant: string) =
            let linkTitle = $"Find {pageAuthor.Name} on {siteName}"
            $"""//a [@title="{linkTitle}" and @class="navicon" and @aria-label="{linkTitle}"]"""
            + $"""/i [@class="media-icon fa-{(defaultArg variant "brands")} fa-{className}"]"""

        member private x.RunTest
            (
                toTest: TestKind,
                toFind: string,
                ?expected: string,
                ?strAssert: (string * string) -> unit,
                ?message: string
            ) =
            let toCompare = defaultArg expected toFind
            let errorMsg = defaultArg message $"Expected to find {toFind}"

            x.TryFindTag toTest toFind
            |> function
            | Some html ->
                strAssert
                |> function
                | None -> Assert.Pass()
                | Some assertion -> assertion (toCompare, html.InnerText)
            | None -> Assert.Fail(errorMsg)

        [<Test>]
        member x.``Generates JSON-LD tag``() = x.RunTest(Seo, """//script[@type="application/ld+json"]""")

        [<Test>]
        member x.``Generates OpenGraph tags``() =
            let expected =
                $"""//meta [@property="og:type" and @content="{(defaultArg pageInfo.OpenGraphType "article").ToLower()}"]"""

            x.RunTest(Seo, expected)

        [<Test>]
        member x.``Metadata includes Fornax version``() =
            let fornaxVersionInfo = FileVersionInfo.GetVersionInfo((typeof<HtmlElement>).Assembly.Location)

            let expected =
                $"""//meta [@name="generator" and @content="fornax v{fornaxVersionInfo.FileVersion}"]"""

            x.RunTest(Seo, expected)

        [<Test>]
        member x.``Social media links are styled when present``() =
            let expected = ".media-icon"

            x.RunTest(
                Seo,
                "//style",
                expected,
                StringAssert.Contains,
                $"Expected to find {expected} within <style> element"
            )

        [<Test>]
        member x.``No style is generated if no email and no links are present``() =
            "style"
            |> x.TryFindTagName(seo { pageInfo with Author = { pageAuthor with Email = ""; SocialMedia = [] } })
            |> function
            | Some _ -> Assert.Fail($"Expected no <style> tag")
            | None -> Assert.Pass()

        [<Test>]
        member x.``Just an email will also generate style``() =
            "style"
            |> x.TryFindTagName(seo { pageInfo with Author = { pageAuthor with SocialMedia = [] } })
            |> function
            | Some _ -> Assert.Pass()
            | None -> Assert.Fail($"Expected a <style> tag")

        [<Test>]
        member x.``Generates a mailto: link``() =
            let label = $"Contact {pageAuthor.Name} at {pageAuthor.Email}"

            let expected =
                $"""//a [@href="mailto:{pageAuthor.Email}" and @class="navicon" and @aria-label="{label}"]"""
                + """/i [@class="media-icon fa-solid fa-envelope"]"""

            x.RunTest(Meta, expected)

        [<Test>]
        member x.``Generates social media links with titles``() = x.RunTest(Meta, x.XPathFor("LinkedIn", "linkedin"))

        [<Test>]
        member x.``Can parse a Slack profile address from host name only``() =
            x.RunTest(Meta, x.XPathFor("Slack", "slack"))

        [<Test>]
        member x.``Can parse a Snapchat profile address``() = x.RunTest(Meta, x.XPathFor("Snapchat", "snapchat-square"))

        [<Test>]
        member x.``Can parse a Spotify profile address``() = x.RunTest(Meta, x.XPathFor("Spotify", "spotify"))

        [<Test>]
        member x.``Can parse a StackExchange profile address from host name only``() =
            x.RunTest(Meta, x.XPathFor("Stack Exchange", "stack-exchange"))

        [<Test>]
        member x.``Can parse a Telegram profile address``() = x.RunTest(Meta, x.XPathFor("Telegram", "telegram"))

        [<Test>]
        member x.``Can parse a Bēhance profile address``() = x.RunTest(Meta, x.XPathFor("Bēhance", "behance-square"))

        [<Test>]
        member x.``Can parse a Google Scholar citations search result``() =
            x.RunTest(Meta, x.XPathFor("Google Scholar", "graduation-cap", "solid"))

        [<Test>]
        member x.``Can parse a SourceForge profile from host name only``() =
            x.RunTest(Meta, x.XPathFor("SourceForge", "fire", "solid"))

        [<Test>]
        member x.``Bluesky profiles generate Bluesky icons``() = x.RunTest(Meta, x.XPathFor("Bluesky", "bluesky"))

        [<Test>]
        member x.``Discord profiles generate Discord icons``() = x.RunTest(Meta, x.XPathFor("Discord", "discord"))

        [<Test>]
        member x.``Mastodon profiles generate Mastodon icons``() = x.RunTest(Meta, x.XPathFor("Mastodon", "mastodon"))

        [<Test>]
        member x.``TikTok profiles generate TikTok icons``() = x.RunTest(Meta, x.XPathFor("TikTok", "tiktok"))

        [<Test>]
        member x.``Patreon profiles generate Patreon icons``() = x.RunTest(Meta, x.XPathFor("Patreon", "patreon"))

        [<Test>]
        member x.``Xing profiles generate Xing icons``() = x.RunTest(Meta, x.XPathFor("Xing", "xing-square"))

        [<Test>]
        member x.``X profiles generate X icons``() =
            let expected = $"""//a [@href="{xLink}"]/i [@class="media-icon fa-brands fa-x-twitter"]"""
            x.RunTest(Meta, expected)

        [<Test>]
        member x.``Old Twitter profiles also generate X icons``() =
            let expected =
                $"""//a [@href="{twitterLink}"]/i [@class="media-icon fa-brands fa-x-twitter"]"""

            x.RunTest(Meta, expected)

        [<Test>]
        member x.``Generates absolute URLs from relative links to unknown sites``() =
            x.RunTest(Meta, $"""//a [@href="https://{unmapped}"]""")

        [<Test>]
        member x.``JsonLinkData ignores relative urls``() =
            let bad = { pageInfo with BaseUrl = "/public"; Url = "/news/posting.php" }
            let json = JsonLinkData(bad)

            Assert.That(obj.ReferenceEquals(null, json.MainEntityOfPage.Id))

        [<Test>]
        member x.``JsonLinkData ignores invalid urls``() =
            let bad = { pageInfo with BaseUrl = "@#$%$!://nodomain/"; Url = "/news/topics/index.htm" }
            let json = JsonLinkData(bad)

            Assert.That(obj.ReferenceEquals(null, json.MainEntityOfPage.Id))

        [<Test>]
        member x.``Validates ContentObject's Schema.org Type``() =
            let f =
                TestDelegate(fun () -> seo ({ pageInfo with ObjectType = Some "Starbucks" }) |> ignore)

            Assert.Throws<System.ArgumentException>(f) |> ignore

        [<Test>]
        member x.``Validates MainEntity's Schema.org Type``() =
            let f =
                TestDelegate(fun () -> seo ({ pageInfo with ContentType = Some "dog.food" }) |> ignore)

            Assert.Throws<System.ArgumentException>(f) |> ignore

        [<Test>]
        member x.``Validates ContentObject's OpenGraphType Type``() =
            let f =
                TestDelegate(fun () -> seo ({ pageInfo with OpenGraphType = Some "" }) |> ignore)

            Assert.Throws<System.ArgumentException>(f) |> ignore
