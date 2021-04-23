namespace Fornax.Seo.Tests

module UnitTests =
    open NUnit.Framework
    open Fornax.Seo
    open Fornax.Seo.Tags

    [<TestFixture>]
    type UnitTest() =
        let links =
            [ "acmeinc.enterprise.slack.com"
              "https://story.snapchat.com/u/webmaster"
              "https://open.spotify.com/user/er811nzvdw2cy2qgkrlei9sqe/playlist/2lzTTRqhYS6AkHPIvdX9u3?si=KcZxfwiIR7OBPCzj20utaQ"
              "ell.stackexchange.com/users/00001/webmaster"
              "https://t.me/s/webmaster"
              "https://www.linkedin.com/in/webmaster" ]

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
              Meta = Some [ ("Image", "/avatar.jpg" ); ("Publisher", pageAuthor.Name ) ]}

        member private x.TryFindSeoTag(content: string) =
            seo pageInfo
            |> List.tryFind (fun tag -> (HtmlElement.ToString tag).Contains(content))

        member private x.TryFindLink(content: string) =
            socialMedia pageAuthor
            |> List.tryFind (fun tag -> (HtmlElement.ToString tag).Contains(content))

        member private x.Get opt = defaultArg opt ""

        [<Test>]
        member x.``Generates JSON-LD tag``() =
            let expected = """<script type="application/ld+json">"""

            x.TryFindSeoTag(expected)
            |> function
            | Some _ -> Assert.Pass()
            | None -> Assert.Fail($"Expected to find {expected}")

        [<Test>]
        member x.``Generates OpenGraph tags``() =
            let expected =
                $"""<meta property="og:type" content="{(defaultArg pageInfo.OpenGraphType "article").ToLower()}"/>"""

            x.TryFindSeoTag(expected)
            |> function
            | Some _ -> Assert.Pass()
            | None -> Assert.Fail($"Expected to find {expected}")

        [<Test>]
        member x.``Social media links are styled when present``() =
            let expected = ".media-icon"

            x.TryFindSeoTag("<style>")
            |> function
            | Some css -> StringAssert.Contains(expected, HtmlElement.ToString css)
            | None -> Assert.Fail($"Expected to find {expected} within <style> element")

        [<Test>]
        member x.``No style is generated if no email and no links are present``() =
            seo
                { pageInfo with
                      Author = { pageAuthor with Email = ""; SocialMedia = [] } }
            |> List.tryFind (fun tag -> (HtmlElement.ToString tag).Contains("<style>"))
            |> function
            | Some _ -> Assert.Fail($"Expected no <style> tag")
            | None -> Assert.Pass()

        [<Test>]
        member x.``Generates a mailto: link``() =
            let expected = $"""<a href="mailto:{pageAuthor.Email}" class="navicon">"""
            let linkContent = """<i class="media-icon fa fa-envelope" aria-hidden="true"></i>"""

            x.TryFindLink(linkContent)
            |> function
            | Some link -> StringAssert.Contains(expected, HtmlElement.ToString link)
            | None -> Assert.Fail($"Expected to find {expected}")

        [<Test>]
        member x.``Generates social media links with title``() =
            let expected = $"""title="Find {pageAuthor.Name} on linkedin" class="navicon">"""
            let linkContent = """<i class="media-icon fa fa-linkedin-square" aria-hidden="true"></i>"""

            x.TryFindLink(linkContent)
            |> function
            | Some link -> StringAssert.Contains(expected, HtmlElement.ToString link)
            | None -> Assert.Fail($"Expected to find {linkContent}")

        [<Test>]
        member x.``Can parse a Slack profile address from host name only``() =
            let expected = $"""title="Find {pageAuthor.Name} on slack" class="navicon">"""
            let linkContent = $"""href="https://{links.[0]}" """

            x.TryFindLink(linkContent)
            |> function
            | Some link -> StringAssert.Contains(expected, HtmlElement.ToString link)
            | None -> Assert.Fail($"Expected to find {linkContent}")

        [<Test>]
        member x.``Can parse a Snapchat profile address``() =
            let expected = $"""title="Find {pageAuthor.Name} on snapchat" class="navicon">"""
            let linkContent = $"""href="{links.[1]}" """

            x.TryFindLink(linkContent)
            |> function
            | Some link -> StringAssert.Contains(expected, HtmlElement.ToString link)
            | None -> Assert.Fail($"Expected to find {linkContent}")

        [<Test>]
        member x.``Can parse a Spotify profile address``() =
            let expected = $"""title="Find {pageAuthor.Name} on spotify" class="navicon">"""
            let linkContent = $"""href="{links.[2]}" """

            x.TryFindLink(linkContent)
            |> function
            | Some link -> StringAssert.Contains(expected, HtmlElement.ToString link)
            | None -> Assert.Fail($"Expected to find {linkContent}")

        [<Test>]
        member x.``Can parse a StackExchange profile address from host name only``() =
            let expected = $"""title="Find {pageAuthor.Name} on stackexchange" class="navicon">"""
            let linkContent = $"""href="https://{links.[3]}" """

            x.TryFindLink(linkContent)
            |> function
            | Some link -> StringAssert.Contains(expected, HtmlElement.ToString link)
            | None -> Assert.Fail($"Expected to find {linkContent}")


        [<Test>]
        member x.``Can parse a Telegram profile address``() =
            let expected = $"""title="Find {pageAuthor.Name} on telegram" class="navicon">"""
            let linkContent = $"""href="{links.[4]}" """

            x.TryFindLink(linkContent)
            |> function
            | Some link -> StringAssert.Contains(expected, HtmlElement.ToString link)
            | None -> Assert.Fail($"Expected to find {linkContent}")

        [<Test>]
        member x.``JsonLinkData ignores relative urls``() =
            let bad = { pageInfo with BaseUrl = "/public"; Url = "/news/posting.php"}
            let json = JsonLinkData(bad)

            Assert.Null(json.MainEntityOfPage.Id)

        [<Test>]
        member x.``JsonLinkData ignores invalid urls``() =
            let bad = { pageInfo with BaseUrl = "@#$%$!://nodomain/"; Url = "/news/topics/index.htm" }
            let json = JsonLinkData(bad)

            Assert.Null(json.MainEntityOfPage.Id)

        [<Test>]
        member x.``Validates mainEntity's Schema.org Type``() =
            let f =
                TestDelegate(fun () -> seo ({ pageInfo with ObjectType = Some "Starbucks" }) |> ignore)

            Assert.Throws<System.ArgumentException>(f) |> ignore

        [<Test>]
        member x.``Validates page content's Schema.org Type``() =
            let f =
                TestDelegate(fun () -> seo ({ pageInfo with ContentType = Some "dog.food" }) |> ignore)

            Assert.Throws<System.ArgumentException>(f) |> ignore

module Program =
    [<EntryPoint>]
    let main _ = 0
