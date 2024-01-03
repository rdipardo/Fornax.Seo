#r "../_lib/Fornax.Core.dll"
#r "../_lib/Fornax.Seo.dll"

open Fornax.Seo

type SiteInfo = {
    title: string
    baseUrl: string
    description: string
    postPageSize: int
}

let contentFileTypes = [".md"; ".markdown"]
let ignoredFileTypes = [ ".fsx"; ".fsproj"; ".json"; ".rst"; ".yml"; ".sh"; ".cmd" ]

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo =
        { title = "Sample Fornax blog"
          baseUrl = "http://example.com"
          description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit"
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
