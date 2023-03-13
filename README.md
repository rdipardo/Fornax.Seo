# Fornax.Seo

[![Nuget version][]][Package host]
[![NuGet workflow status][]][NuGet Workflow]
[![Test workflow status][]][Test Workflow]

A SEO meta tag generator for [Fornax](https://ionide.io/Tools/fornax.html)

## Goals

- enhance the search engine visibility of Fornax-generated websites with:

  + [structured data][] in [JSON-LD](https://json-ld.org) format
  + [OpenGraph](https://ogp.me) `<meta>` tags
  + personalized social media links

- try to enforce some SEO best practises, e.g. requiring [absolute URLs][] to all content items


## Usage example

**NOTE**

The following requires `fornax` [0.15.1 or newer][from nuget.org].
Visit [the wiki] to learn how to use this package with earlier `fornax` versions.

- Change into a project directory and scaffold a new website


      fornax new

- Install and set up [`paket`](https://fsprojects.github.io/Paket/index.html):


      dotnet tool install paket
      dotnet paket init

- Configure dependencies, e.g. at minimum:

    ```sh
    # paket.dependencies

    source https://api.nuget.org/v3/index.json
    framework: net6.0, netstandard2.0, netstandard2.1
    generate_load_scripts: true
    storage: none

    # . . .
    nuget Fornax.Seo      >= 1.2.0  # pulls in the Fornax.Core package
    nuget Markdig
    #  . . .
    ```

- Install the packages:


      dotnet paket install


**IMPORTANT**

- Provide the root domain of your website:

    ```fsharp
    // loaders/globalloader.fsx
    #load @"../.paket/load/net6.0/Fornax.Core.fsx"

    type SiteInfo = {
        title: string
        /// The root domain of your website - must be an absolute URL
        baseUrl: string
        description: string
    }
    ```

- Add personal authorship details, e.g.:

    ```fsharp
    // loaders/globalloader.fsx
    #load @"../.paket/load/net6.0/Fornax.Seo.fsx"

    open Fornax.Seo

    let loader (projectRoot: string) (siteContent: SiteContents) =
        let siteInfo =
            { title = "Sample Fornax blog"
              baseUrl = "http://example.com"
              description = "Just a simple blog" }

        let onTheWeb =
            [ "linkedin.com/in/username"
              "github.com/username"
              "bitbucket.org/username"
              "facebook.com/username" ]

        let siteAuthor: ContentCreator =
            { Name = "Moi-même"
              Email = "info@example.com"
              SocialMedia = onTheWeb }

        siteContent.Add(siteInfo)
        siteContent.Add(siteAuthor)

        siteContent
    ```

### Collect metadata from a content item (e.g., a blog posting)

~~~fsharp
// generators/post.fsx
#load @"../.paket/load/net6.0/Fornax.Seo.fsx"
#load @"layout.fsx"

open Html
open Fornax.Seo

let generate' (ctx: SiteContents) (page: string) =
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo>()
    let siteName = siteInfo |> Option.map (fun si -> si.title)

    let tagline =
        siteInfo
        |> Option.map (fun si -> si.description)
        |> Option.defaultValue ""

    let siteAuthor =
        ctx.TryGetValue<ContentCreator>()
        |> Option.defaultValue ContentCreator.Default

    let siteRoot =
        siteInfo
        |> Option.map (fun si -> si.baseUrl)
        |> Option.defaultValue ContentObject.Default.BaseUrl

    let post =
        ctx.TryGetValues<Postloader.Post>()
        |> Option.defaultValue Seq.empty
        |> Seq.find (fun p -> p.file = page)

    let postMeta: ContentObject =
        { Title = post.title
          BaseUrl = siteRoot
          Url = post.file.Replace(System.IO.Path.GetExtension post.file, ".html")
          Description = tagline
          Author = { siteAuthor with Name = defaultArg post.author siteAuthor.Name }
          SiteName = siteName
          Headline = Some post.summary
          ObjectType = Some "Blog"
          ContentType = Some "BlogPosting"
          OpenGraphType = Some "article"
          Locale = Some "en-us"
          Published = post.published
          Modified = post.modified
          Tags = Some post.tags
          Meta =
              Some [ ("Image", defaultArg post.image $"{siteRoot}/images/avatar.jpg")
                     ("Publisher", defaultArg siteName siteAuthor.Name) ] }

    ctx.Add(postMeta)
    // . . .
~~~

### Render SEO metadata in your page layout

~~~fsharp
// generators/layout.fsx
#load @"../.paket/load/net6.0/Fornax.Seo.fsx"

open Html
open Fornax.Seo

// . . .

let layout (ctx: SiteContents) (active: string) (content: HtmlElement seq) =
    let siteAuthor =
        ctx.TryGetValue<ContentCreator>()
        |> Option.defaultValue ContentCreator.Default

    let seoData =
        ctx.TryGetValues<ContentObject>()
        |> Option.defaultValue Seq.empty

    let pageMeta =
        seoData
        |> Seq.tryFind (fun p -> p.Title.Contains(active))
        |> function
        | Some info -> info
        | _ -> { ContentObject.Default with Author = siteAuthor }

    html [] [
        head [] [
            meta [ CharSet "utf-8" ]
            meta [ Name "viewport"; Content "width=device-width, initial-scale=1" ]
            // . . .
            yield! seo pageMeta
        ]
        body [] [
            // . . .
            footer [] [ yield! socialMedia siteAuthor ]
        ]
    ]

    // . . .
~~~


## Similar NuGet libraries (by framework)

### .NET

- [json-ld.net](https://github.com/linked-data-dotnet/json-ld.net)
- [OpenGraph-Net](https://ghorsey.github.io/OpenGraph-Net)

### ASP.NET

- [Winton.AspNetCore.Seo](https://github.com/wintoncode/Winton.AspNetCore.Seo)
- [Definux.Seo](https://github.com/Definux/Seo)

### [Umbraco](https://umbraco.com)

- [Wavenet.Umbraco8.Seo](https://www.nuget.org/packages/Wavenet.Umbraco8.Seo)


## Contributing

A guide to building the project and making pull requests can be found [here](https://github.com/rdipardo/Fornax.Seo/blob/main/CONTRIBUTING.md).


## License

Distributed under the terms of the [Mozilla Public License Version 2.0][].


[structured data]: https://developers.google.com/search/docs/guides/intro-structured-data
[absolute URLs]: https://stackoverflow.com/a/64830732
[the documentation]: https://heredocs.io
[from source]: https://github.com/ionide/Fornax#build-process
[from nuget.org]: https://www.nuget.org/packages/Fornax
[the wiki]: https://github.com/rdipardo/Fornax.Seo/wiki/FAQ#faq

[Nuget version]: https://img.shields.io/nuget/vpre/Fornax.Seo?color=blueviolet&logo=nuget
[Package host]: https://www.nuget.org/packages/Fornax.Seo
[NuGet Workflow]: https://github.com/rdipardo/Fornax.Seo/actions/workflows/nuget.yml
[NuGet workflow status]: https://github.com/rdipardo/Fornax.Seo/actions/workflows/nuget.yml/badge.svg
[Test Workflow]: https://github.com/rdipardo/Fornax.Seo/actions/workflows/ci.yml
[Test workflow status]: https://github.com/rdipardo/Fornax.Seo/actions/workflows/ci.yml/badge.svg

[Fornax CLI tool]: https://github.com/ionide/Fornax
[Mozilla Public License Version 2.0]: https://github.com/rdipardo/Fornax.Seo/blob/main/LICENSE
