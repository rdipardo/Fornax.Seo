//
// Copyright (c) 2021 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at http://mozilla.org/MPL/2.0/.
//

namespace Fornax.Seo

/// Public representations of web content metadata
[<AutoOpen>]
module Models =

    /// Represents the author of a web content item
    type ContentCreator =
        {
          /// <summary>
          ///   The name of this <see cref="ContentCreator"/>
          /// </summary>
          Name: string

          /// <summary>
          ///   The email of this <see cref="ContentCreator"/> &#x2013; will be rendered with
          ///   <see cref='P:Fornax.Seo.Models.ContentCreator.SocialMedia'/>
          /// </summary>
          Email: string

          /// <summary>
          ///   A list of relative or absolute URLs associated with this <see cref="ContentCreator"/>
          /// </summary>
          SocialMedia: string list }

        static member Default = { Name = "Anonymous"; Email = ""; SocialMedia = [] }

    /// Represents a web content item; by default, represents the host website
    type ContentObject =
        {
          /// The title of a web content item or its host website
          Title: string

          /// The root domain of the host website &#x2013; must be an absolute URL
          BaseUrl: string

          /// <summary>
          ///   The relative or absolute path to the web content item represented by this
          ///   <see cref="ContentObject" />
          /// </summary>
          Url: string

          /// A short description of a web content item or its host website
          Description: string

          /// <summary>
          ///   The identifying name of the host website &#x2013; defaults to
          ///   <see cref="P:Fornax.Seo.Models.ContentObject.Title"/>
          /// </summary>
          SiteName: string option

          /// The author of a web content item or its host website
          Author: ContentCreator

          /// <summary>
          ///   An optional shorter version of <see cref='P:Fornax.Seo.Models.ContentObject.Description'/>
          /// </summary>
          Headline: string option

          /// The <a href="https://schema.org/docs/full.html">Schema.org object type</a> of the host website
          /// &#x2013; validated
          ObjectType: string option

          /// The <a href="https://schema.org/docs/full.html">Schema.org object type</a> of a web content item
          /// &#x2013; validated
          ContentType: string option

          /// The <a href="https://ogp.me/#types">OpenGraph object type</a> of a web content item &#x2013; validated
          OpenGraphType: string option

          /// Publication date of a web content item
          Published: System.DateTime option

          /// Last modification date of a web content item &#x2013; defaults to current date if not provided
          /// and a publication date is found
          Modified: System.DateTime option

          /// <summary>
          ///   The <a href="https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes">ISO 639-1 language code</a> of a
          ///   web content item &#x2013; defaults to <c>en-US</c>
          /// </summary>
          Locale: string option

          /// Optional keywords associated with a web content item
          Tags: string list option

          /// An arbitrary list of <a href="https://schema.org/docs/full.html">Schema.org object types</a>
          /// &#x2013; unrecognized types will be silently ignored
          Meta: list<string * string> option }

        static member Default =
            { Title = ""
              BaseUrl = "https://example.com"
              Url = "/index.html"
              Description = ""
              Author = ContentCreator.Default
              SiteName = None
              Headline = None
              ObjectType = None
              ContentType = None
              OpenGraphType = None
              Locale = None
              Published = Some System.DateTime.Now
              Modified = None
              Tags = None
              Meta = None }
