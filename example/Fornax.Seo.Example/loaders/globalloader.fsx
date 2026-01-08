#load "../.paket/load/netstandard2.0/fornax_demo/Fornax.Seo.fsx"

open Fornax.Seo
open Fornax.Seo.Rsl.DOM

type SiteInfo = {
    title: string
    baseUrl: string
    description: string
    credits: License
    postPageSize: int
}

let contentFileTypes = [".md"; ".markdown"]
let ignoredFileTypes = [ ".fsx"; ".fsproj"; ".json"; ".rst"; ".yml"; ".sh"; ".cmd" ]

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo =
        { title = "Sample Fornax blog"
          baseUrl = "http://example.com"
          description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit"
          credits = License.FreeAndOpenSource(@"https://creativecommons.org/publicdomain/zero/1.0")
          postPageSize = 3 }

    let onTheWeb =
        [ "asciinema.org/~rdipardo"
          "github.com/rdipardo"
          "bitbucket.org/rdipardo"
          "sourceforge.net/u/robertdipardo/profile/" ]

    let siteAuthor = { Name = "rdipardo"; Email = "dipardo.r@gmail.com"; SocialMedia = onTheWeb }

    siteContent.Add(siteInfo)
    siteContent.Add(siteAuthor)

    siteContent
