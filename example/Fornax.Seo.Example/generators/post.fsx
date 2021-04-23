#r "../_lib/Fornax.Core.dll"
#r "../_lib/Fornax.Seo.dll"
#load "layout.fsx"

open Html
open Fornax.Seo

let generate' (ctx: SiteContents) (page: string) =
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo>()
    let siteName = siteInfo |> Option.map (fun si -> si.title)
    let tagline = siteInfo |> Option.map (fun si -> si.description) |> Option.defaultValue ""
    let siteAuthor = ctx.TryGetValue<ContentCreator>() |> Option.defaultValue ContentCreator.Default

    let siteRoot =
        siteInfo
        |> Option.map (fun si -> si.baseUrl)
        |> Option.defaultValue ContentObject.Default.BaseUrl

    let post =
        ctx.TryGetValues<Postloader.Post>()
        |> Option.defaultValue Seq.empty
        |> Seq.find (fun p -> p.file = page)

    let postMeta =
        { Title = post.title
          BaseUrl = siteRoot
          Url = post.file
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

    Layout.layout
        ctx
        post.title
        [ section [ Class "hero is-info is-medium is-bold" ] [
            div [ Class "hero-body" ] [
                div [ Class "container has-text-centered" ] [
                    h1 [ Class "title" ] [ !!tagline ]
                ]
            ]
          ]
          div [ Class "container" ] [
              section [ Class "articles" ] [
                  div [ Class "column is-8 is-offset-2" ] [ Layout.postLayout false post ]
              ]
          ] ]

let generate (ctx: SiteContents) (projectRoot: string) (page: string) = generate' ctx page |> Layout.render ctx
