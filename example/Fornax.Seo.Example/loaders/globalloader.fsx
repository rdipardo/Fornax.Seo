#r "../_lib/Fornax.Core.dll"
#r "../_lib/Fornax.Seo.dll"

open Fornax.Seo

type SiteInfo = {
    title: string
    baseUrl: string
    description: string
    postPageSize: int
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo =
        { title = "Sample Fornax blog"
          baseUrl = "http://example.com"
          description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit"
          postPageSize = 3 }

    let onTheWeb =
        [ "linkedin.com/in/rdipardo"
          "github.com/rdipardo"
          "bitbucket.org/rdipardo"
          "facebook.com/R.Di.Pardo" ]

    let siteAuthor = { Name = "rdipardo"; Email = "dipardo.r@gmail.com"; SocialMedia = onTheWeb }

    siteContent.Add(siteInfo)
    siteContent.Add(siteAuthor)

    siteContent
