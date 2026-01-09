//
// Copyright (c) 2026 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.
//

namespace Fornax.Seo

/// A (partial) implementation of the <a href="https://rslstandard.org/rsl">RSL 1.0</a> specification
module Rsl =
    open System
    open Tags.Helpers
    open Standards.Rsl
    open Standards.Rsl.Entities
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq

    /// An RSL element attribute
    [<Struct; NoComparison>]
    type Attribute =
        { Name: string
          Value: AttributeValue }

        member internal this.ToProperty() = HtmlProperties.Custom(toCamelCase $"{this.Name}", $"{this.Value}")

        static member inline internal TryCreate (inp: 'a option) name value =
            inp |> Option.map (fun v -> { Name = name; Value = value v })

    /// Permitted values of an RSL element attribute
    and [<RequireQualifiedAccess>] AttributeValue =
        | Author of CopyrightHolder
        | Currency of CurrencyCode
        | Date of System.DateTime
        | Email of Net.Mail.MailAddress
        | Legal of Scopes.Legal
        | Optional of bool
        | Payment of Scopes.Payment
        | Permission of Scopes.Permission
        | String of string
        | Url of System.Uri

        override this.ToString() =
            match this with
            | Author a -> a.ToString().ToLower()
            | Currency c -> c.ToString()
            | Date d -> d.ToRfc3339()
            | Email e -> e.Address
            | Legal lt -> lt.ToString().ToLower()
            | Optional opt -> opt.ToString().ToLower()
            | Payment pt -> pt.ToString().ToLower()
            | Permission pt -> pt.ToString().ToLower()
            | String s -> s.ToLower()
            | Url u -> Url.toString u

    /// Provides RSL elements with HTML generation methods
    type IRslElement =
        abstract member ToHtml : unit -> HtmlElement

    /// <param name="elem">An RSL element type provided by the <see cref="T:Fornax.Seo.Rsl.DOM"/> module</param>
    let inline toHtmlElement (elem: 'T :> IRslElement) = elem.ToHtml()

    let inline private (!) (s: string) = Html.string s |> List.singleton

    let inline private (~+) (attr: Attribute) = attr.ToProperty() |> List.singleton

    let inline private (?+) (attr: Attribute option) = attr |> Option.map (fun a -> a.ToProperty()) |> Option.toList

    let inline private (~%) (elem: 'T :> IRslElement) = toHtmlElement elem

    let inline private (!%) (elems: 'T list when 'T :> IRslElement) = List.map toHtmlElement elems

    let inline private (?%) (elem: 'T option when 'T :> IRslElement) = elem |> Option.map toHtmlElement |> Option.toList

    /// An RSL element containing JSON
    [<Struct; NoComparison>]
    type private JsonElement =
        { Name: string
          Type: string
          Body: string }

        interface IRslElement with
            /// TODO: Validate against some published schemas, e.g., the <a href="https://www.x402.org">x402 protocol</a>
            member this.ToHtml() =
                let props = +{ Name = "type"; Value = AttributeValue.String this.Type }

                let content =
                    try
                        let json =
                            JObject.Parse(
                                this.Body.Trim(),
                                JsonLoadSettings(LineInfoHandling = LineInfoHandling.Ignore)
                            )

                        JsonConvert.SerializeObject(json, Formatting.Indented)
                    with :? JsonReaderException -> "{}"
                    |> (fun json -> json.Replace(@"]]>", @"]]]]><![CDATA[>"))
                    |> (sprintf @"<![CDATA[%s]]>" >> (!))

                Html.custom this.Name props content

    /// An RSL element that simply wraps a URL
    [<Struct; NoComparison>]
    type private UrlElement =
        { Name: string
          Url: System.Uri }

        interface IRslElement with
            member this.ToHtml() = (!) $"{AttributeValue.Url this.Url}" |> Html.custom this.Name []

        static member inline TryCreate (url: System.Uri option) name =
            url |> Option.map (fun url -> { Name = name; Url = url })

    /// <summary>
    ///  An RSL element that should not have the same <c>type</c> attribute as any sibling element
    /// </summary>
    type UniqueElement(attr: AttributeValue) =
        member val internal Key = { Name = "type"; Value = attr }

        override this.GetHashCode() = this.Key.Value.GetHashCode()

        override this.Equals(other: obj) =
            match other with
            | :? UniqueElement as elem -> this.Key.Value = elem.Key.Value
            | _ -> false

        static member inline internal AllUnique(elems: 'T list when 'T: equality) =
            elems |> List.distinctBy hash |> List.length |> (=) (List.length elems)

    /// A unique RSL element specifying the permitted or prohibited usage of a licensed work
    type PermittedUsage(kind: Disposition, scope: Scopes.Permission, specifics: string) =
        inherit UniqueElement(AttributeValue.Permission scope)

        let entities : ResizeArray<string> = ResizeArray()

        do
            match scope with
            | Scopes.Permission.Usage ->
                let uses = Entity<Restrictions.Usage> specifics

                if uses.Values |> Seq.isEmpty then
                    invalidArg (nameof uses) "A non-empty list of usage types is required."
                else
                    uses |> (string >> entities.Add)
            | Scopes.Permission.User ->
                let endUsers = Entity<Restrictions.User> specifics

                if endUsers.Values |> Seq.isEmpty then
                    invalidArg (nameof endUsers) "A non-empty list of end users is required."
                else
                    endUsers |> (string >> entities.Add)
            | Scopes.Permission.Geo ->
                let regions = Entity<Restrictions.Geo> specifics

                if regions.Values |> Seq.isEmpty then
                    invalidArg (nameof regions) "A non-empty list of geographic regions is required."
                else
                    regions |> (string >> entities.Add)

        interface IRslElement with
            member this.ToHtml() =
                let content = entities.ToArray() |> Array.tryExactlyOne |> Option.defaultValue ""

                Html.custom $"{kind}" +this.Key !content


    /// The component elements of an RSL document
    module DOM =

        /// The root element of an RSL document
        type Root(content: Content list) =
            do
                if content |> List.isEmpty then
                    invalidArg (nameof content) "At least one <content> element is required."

                if UniqueElement.AllUnique content |> not then
                    invalidArg (nameof content) "One or more <content> elements reference the same resource."

            interface IRslElement with
                member this.ToHtml() =
                    Html.custom "rsl" [ HtmlProperties.Custom("xmlns", "https://rslstandard.org/rsl") ] !%content

        /// <summary>
        ///  The <c>&lt;content&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   One or more of these may be children of <see cref="T:Fornax.Seo.Rsl.DOM.Root"/>, but at least one is required,
        ///   and all instances must be unique.
        ///  </para><para>
        ///   <dl>
        ///    <dt><b>Required attributes</b></dt>
        ///    <dt><code>url</code></dt>
        ///    <dd>
        ///     The canonical path of the licensed work represented by this <code>&lt;content&gt;</code> element;
        ///     also provides the <a href="https://rslstandard.org/rsl#_3-3-1-canonical-license-identifiers">canonical identifier</a>
        ///     for the license
        ///    </dd>
        ///   </dl>
        ///  </para><para>
        ///   <dl>
        ///    <dt><i>Optional attributes</i></dt>
        ///    <dt><code>server</code></dt>
        ///    <dd>
        ///     The URL of an RSL License Server implementing the <a href="https://rslstandard.org/rsl#_5-open-license-protocol-olp">
        ///     Open License Protocol</a>
        ///    </dd>
        ///    <dt><code>encrypted</code></dt>
        ///    <dd>
        ///     If <tt>true</tt>, the work's canonical asset is encrypted, and the specified <code>server</code> must support the
        ///     <a href="https://rslstandard.org/rsl#7-encrypted-media-standard-ems">Encrypted Media Standard</a>
        ///    </dd>
        ///    <dt><code>lastmod</code></dt>
        ///    <dd>
        ///     A timestamp in <a href="https://www.rfc-editor.org/rfc/rfc3339">RFC 3339</a> format indicating when the work's canonical asset was last modified
        ///    </dd>
        ///   </dl>
        ///  </para><para>
        ///   <dl>
        ///    <dt><b>Required content</b></dt>
        ///    <dt><code>license</code></dt>
        ///    <dd>
        ///     One or more <code>&lt;license&gt;</code> elements that collectively define the rights, costs, and restrictions associated with the work
        ///    </dd>
        ///   </dl>
        ///  </para><para>
        ///   <dl>
        ///    <dt><i>Optional content</i></dt>
        ///    <dt><code>schema</code></dt>
        ///    <dd>
        ///      <a href="https://schema.org/docs/full.html">Schema.org</a> metadata associated with the work
        ///    </dd>
        ///    <dt><code>alternate</code></dt>
        ///    <dd>
        ///     One or more alternative digital formats of the work
        ///    </dd>
        ///    <dt><code>copyright</code></dt>
        ///    <dd>
        ///     The individual or organization owning or controlling the rights to the work
        ///    </dd>
        ///    <dt><code>terms</code></dt>
        ///    <dd>
        ///     The URL path to a terms of service document or supplemental legal information which is not encoded in XML
        ///    </dd>
        ///   </dl>
        ///  </para>
        /// </remarks>
        and Content
            (
                url: System.Uri,
                license: License list,
                ?schema: Schema,
                ?alternate: Alternate list,
                ?copyright: Copyright,
                ?terms: System.Uri,
                ?server: System.Uri,
                ?encrypted: bool,
                ?lastmod: System.DateTime
            ) =
            inherit UniqueElement(AttributeValue.Url url)

            do
                if license |> List.isEmpty then
                    invalidArg (nameof license) "At least one <license> element is required."

                if UniqueElement.AllUnique license |> not then
                    invalidArg (nameof license) "One or more <license> elements define the same terms."

            interface IRslElement with
                member this.ToHtml() =
                    let props = +{ this.Key with Name = "url" } |> ResizeArray
                    let content = !%license |> ResizeArray

                    schema
                    |> Option.toList
                    |> List.map
                        (function
                        | Inline jsonld -> %{ Name = (nameof schema); Type = "application/ld+json"; Body = jsonld }
                        | Linked uri -> %{ Name = (nameof schema); Url = uri })
                    |> content.AddRange

                    defaultArg alternate [] |> ((!%) >> content.AddRange)
                    copyright |> ((?%) >> content.AddRange)
                    UrlElement.TryCreate terms (nameof terms) |> ((?%) >> content.AddRange)

                    let tryAddAttribute prop name value =
                        Attribute.TryCreate prop name value |> ((?+) >> props.AddRange)

                    tryAddAttribute server (nameof server) AttributeValue.Url
                    tryAddAttribute encrypted (nameof encrypted) AttributeValue.Optional
                    tryAddAttribute lastmod (nameof lastmod) AttributeValue.Date

                    Html.custom
                        "content"
                        [ yield! props.ToArray() |> List.ofArray ]
                        [ yield! content.ToArray() |> List.ofArray ]

        /// <summary>
        ///  The <c>&lt;license&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  One or more of these may be children of the <see cref="T:Fornax.Seo.Rsl.DOM.Content"/> element,
        ///  but at least one is required, and all instances must be unique
        /// </remarks>
        and [<CustomEquality; NoComparison>] License =
            { /// The activities, users, or jurisdictions that are explicitly allowed under the license
              Permits: Permits list option
              /// The activities that are explicitly forbidden, even if they would otherwise be permitted
              Prohibits: Prohibits list option
              /// Legally binding metadata specifying the authority, guarantees, and disclaimers of the license
              Legal: Legal list option
              /// Defines how users must compensate the licensor for permitted uses of the licensed work
              Payment: Payment option }

            override this.GetHashCode() =
                let hasher (vals: 'T list option when 'T :> UniqueElement) =
                    vals
                    |> (Option.map (Seq.map (id >> hash)) >> Option.map (Seq.fold (^^^) -1))
                    |> Option.defaultValue -1

                [ hasher this.Permits
                  hasher this.Prohibits
                  hasher this.Legal
                  this.Payment |> Option.map List.singleton |> hasher ]
                |> List.fold (^^^) -1

            override this.Equals(other: obj) =
                match other with
                | :? License as license -> hash this = hash license
                | _ -> false

            interface IRslElement with
                member this.ToHtml() =
                    let content = defaultArg this.Permits [] |> ((!%) >> ResizeArray)
                    defaultArg this.Prohibits [] |> ((!%) >> content.AddRange)
                    defaultArg this.Legal [] |> ((!%) >> content.AddRange)
                    (?%) this.Payment |> content.AddRange

                    Html.custom "license" [] [ yield! content.ToArray() |> List.ofArray ]

            /// <returns>
            ///  A default <c>&lt;license&gt;</c> implying freedom to use without payment or attribution in search
            ///  engines only, and not for any kind of AI content generation
            /// </returns>
            static member FreeAndSearchable =
                { Permits = Some [ Permits.WebSearchOnly ]
                  Prohibits = None
                  Legal = None
                  Payment = None }

            /// <returns>
            ///  A default <c>&lt;license&gt;</c> implying freedom to use without payment in search engines, under
            ///  the terms of the specified <paramref name="license"/>, and not for any kind of AI content generation
            /// </returns>
            /// <param name="license">
            ///  A link to the text of an open source licence, or the identifier of a standard license in the
            ///  <a href="https://spdx.org/licenses/">SPDX index</a>
            /// </param>
            static member FreeAndOpenSource(license: string) =
                let url =
                    if (Url.ofString license).IsAbsoluteUri then
                        license.Trim()
                    else
                        $"https://spdx.org/licenses/{license}.html"

                { License.FreeAndSearchable with
                      Payment = Some(Payment(Scopes.Payment.Attribution, standard = url)) }

        /// <summary>
        ///  The <c>&lt;permits&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   One or more of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.License"/>, but each must have a
        ///   unique <a href="/reference/fornax-seo-standards-rsl-scopes-permission.html"><tt>type</tt> attribute</a>.
        ///  </para><para>
        ///   For the <c>specifics</c> parameter, a non-empty, comma-delimited string of one or more
        ///   <a href="https://rslstandard.org/rsl#_3-5-permits-allowed-uses">appropriate terms</a> is required.
        ///  </para>
        /// </remarks>
        /// <example>
        ///  <code>
        ///   open Fornax.Seo.Rsl.DOM
        ///   open Fornax.Seo.Standards.Rsl
        ///
        ///   let allowedUsage   = Permits (Scopes.Permission.Usage, "ai-input")
        ///   let allowedUsers   = Permits (Scopes.Permission.User, "non-commercial,education")
        ///   let allowedRegions = Permits (Scopes.Permission.Geo, "CA,GB,AU,NZ")
        ///  </code>
        /// </example>
        and Permits(scope: Scopes.Permission, specifics: string) =
            inherit PermittedUsage(Disposition.Permits, scope, specifics)
            /// <returns>
            ///  A default <c>&lt;permits&gt;</c> element allowing usage for search engine indexing, excluding AI-assisted
            ///  search summaries or other kinds of AI content generation
            /// </returns>
            static member WebSearchOnly = Permits(Scopes.Permission.Usage, "search")

        /// <summary>
        ///  The <c>&lt;prohibits&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   One or more of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.License"/>, but each must have a
        ///   unique <a href="/reference/fornax-seo-standards-rsl-scopes-permission.html"><tt>type</tt> attribute</a>.
        ///  </para><para>
        ///   For the <c>specifics</c> parameter, a non-empty, comma-delimited string of one or more
        ///   <a href="https://rslstandard.org/rsl#_3-6-prohibits-disallowed-uses">appropriate terms</a> is required.
        ///  </para>
        /// </remarks>
        /// <example>
        ///  <code>
        ///   open Fornax.Seo.Rsl.DOM
        ///   open Fornax.Seo.Standards.Rsl
        ///
        ///   let deniedUsage   = Prohibits (Scopes.Permission.Usage, "ai-train,ai-input")
        ///   let deniedUsers   = Prohibits (Scopes.Permission.User, "commercial")
        ///   let deniedRegions = Prohibits (Scopes.Permission.Geo, "US")
        ///  </code>
        /// </example>
        and Prohibits(scope: Scopes.Permission, specifics: string) =
            inherit PermittedUsage(Disposition.Prohibits, scope, specifics)
            /// <returns>
            ///  A default <c>&lt;prohibits&gt;</c> element denying usage for any kind of AI content generation
            /// </returns>
            static member AllBots = Prohibits(Scopes.Permission.Usage, "ai-all")

        /// <summary>
        ///  The <c>&lt;legal&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   One or more of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.License"/>, but each must have a
        ///   unique <a href="/reference/fornax-seo-standards-rsl-scopes-legal.html"><tt>type</tt> attribute</a>.
        ///  </para><para>
        ///   For the <c>specifics</c> parameter, a non-empty, comma-delimited string of one or more
        ///   <a href="https://rslstandard.org/rsl#_3-12-legal-legal-statements">appropriate terms</a> is required when
        ///   <paramref name="scope"/> is <a href="/reference/fornax-seo-standards-rsl-scopes-legal.html#Warranty">Warranty</a>
        ///   or <a href="/reference/fornax-seo-standards-rsl-scopes-legal.html#Disclaimer">Disclaimer</a>.
        ///  </para>
        /// </remarks>
        /// <example>
        ///  <code>
        ///   open Fornax.Seo.Rsl.DOM
        ///   open Fornax.Seo.Standards.Rsl
        ///
        ///   let hasWarranty    = Legal (Scopes.Legal.Warranty, "ownership,authority,no-infringement")
        ///   let hasDisclaimer  = Legal (Scopes.Legal.Disclaimer, "as-is,no-liability")
        ///   let hasAttestation = Legal (Scopes.Legal.Attestation, "true")
        ///   let hasContact     = Legal (Scopes.Legal.Contact, "mailto:info@example.com")
        ///   let hasProof       = Legal (Scopes.Legal.Proof, "https://blockscan.example/tx/0x9aBcD123")
        ///  </code>
        /// </example>
        and Legal(scope: Scopes.Legal, specifics: string) =
            inherit UniqueElement(AttributeValue.Legal scope)

            let entities : ResizeArray<string> = ResizeArray()

            do
                match scope with
                | Scopes.Legal.Warranty ->
                    let warranties = Entity<Declarations.Warranty> specifics

                    if warranties.Values |> Seq.isEmpty then
                        invalidArg (nameof specifics) "A non-empty list of warranties is required."
                    else
                        warranties |> (string >> entities.Add)
                | Scopes.Legal.Disclaimer ->
                    let disclaimers = Entity<Declarations.Disclaimer> specifics

                    if disclaimers.Values |> Seq.isEmpty then
                        invalidArg (nameof specifics) "A non-empty list of disclaimers is required."
                    else
                        disclaimers |> (string >> entities.Add)
                | Scopes.Legal.Attestation ->
                    specifics
                    |> Boolean.TryParse
                    |> function
                    | (true, value) -> $"{AttributeValue.Optional value}" |> entities.Add
                    | _ -> invalidArg (nameof specifics) @"Expected the string ""true"" or ""false""."
                | Scopes.Legal.Contact
                | Scopes.Legal.Proof ->
                    let tryParse (uri: string) =
                        let u = uri.Trim() |> Url.ofString

                        if u.IsAbsoluteUri then
                            $"{AttributeValue.Url u}"
                        else if scope = Scopes.Legal.Contact then
                            uri.Trim()
                            |> Net.Mail.MailAddress.TryCreate
                            |> function
                            | (true, e) -> e.Address
                            | _ -> invalidArg (nameof specifics) $"Invalid URL or email address: %A{uri}."
                        else
                            invalidArg (nameof specifics) $"URL not absolute: %A{uri}"

                    let specs =
                        specifics
                            .Trim()
                            .Split([| '\x20'; '\x2C'; '\x09'; '\x0A'; '\x0D' |], StringSplitOptions.RemoveEmptyEntries)

                    specs
                    |> Array.tryExactlyOne
                    |> function
                    | Some uri -> tryParse uri |> entities.Add
                    | None ->
                        if scope = Scopes.Legal.Contact then
                            invalidArg (nameof specifics) "A single contact URL or email is required."
                        else
                            let uris = specs |> (Array.map tryParse >> String.concat " ")

                            if String.IsNullOrEmpty uris then
                                invalidArg (nameof specifics) "One or more absolute URLs is required."

                            uris |> entities.Add

            interface IRslElement with
                member this.ToHtml() =
                    let content = entities.ToArray() |> Array.tryExactlyOne |> Option.defaultValue ""

                    Html.custom "legal" +this.Key !content

        /// <summary>
        ///  The <c>&lt;payment&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  <para>
        ///   One of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.License"/>, but each must have a unique
        ///   <a href="/reference/fornax-seo-standards-rsl-scopes-payment.html"><tt>type</tt> attribute</a>.
        ///  </para><para>
        ///   The default is <a href="/reference/fornax-seo-standards-rsl-scopes-payment.html#Free">Free</a> if no
        ///   <paramref name="scope"/> is provided.
        ///  </para><para>
        ///   <dl>
        ///    <dt><i>Optional content</i></dt>
        ///    <dt><code>amount</code></dt>
        ///    <dd>
        ///     Explicit cost and currency
        ///    </dd>
        ///    <dt><code>accepts</code></dt>
        ///    <dd>
        ///     Details regarding supported payment methods
        ///    </dd>
        ///    <dt><code>standard</code></dt>
        ///    <dd>
        ///     The URL path to a public or collective license (e.g., Creative Commons).
        ///    </dd>
        ///    <dt><code>custom</code></dt>
        ///    <dd>
        ///     The URL path to a publisher-specific licensing page (e.g., contact form)
        ///    </dd>
        ///   </dl>
        ///  </para>
        /// </remarks>
        and Payment(?scope: Scopes.Payment, ?amount: Amount, ?accepts: Accepts, ?standard: string, ?custom: string) =
            inherit UniqueElement(defaultArg scope Scopes.Payment.Free |> AttributeValue.Payment)

            interface IRslElement with
                member this.ToHtml() =
                    let content : ResizeArray<HtmlElement> = ResizeArray()

                    (Option.map Url.ofString standard, nameof standard)
                    ||> UrlElement.TryCreate
                    |> ((?%) >> content.AddRange)

                    (Option.map Url.ofString custom, nameof custom)
                    ||> UrlElement.TryCreate
                    |> ((?%) >> content.AddRange)

                    (?%) amount |> content.AddRange
                    (?%) accepts |> content.AddRange

                    Html.custom "payment" +this.Key [ yield! content.ToArray() |> List.ofArray ]

        /// <summary>
        ///  The <c>&lt;amount&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  One of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/>
        /// </remarks>
        and [<Struct; NoComparison>] Amount =
            { /// An ISO 4217 currency code
              Currency: CurrencyCode
              /// The monetary cost for permitted use of the licensed work
              Value: decimal }

            interface IRslElement with
                member this.ToHtml() =
                    Html.custom
                        "amount"
                        +{ Name = "currency"; Value = AttributeValue.Currency this.Currency }
                        !(this.Value.ToString("0.####", Globalization.CultureInfo.InvariantCulture))

        /// <summary>
        ///  The <c>&lt;accepts&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  One of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/>
        /// </remarks>
        and [<Struct; NoComparison>] Accepts =
            { /// A media type identifying the payment protocol and payload format
              Type: string
              /// A stringified JSON object of protocol-specific
              /// <a href="https://rslstandard.org/rsl#_3-11-accepts-payment-methods">metadata</a>,
              /// such as pricing and settlement instructions
              Method: string }

            interface IRslElement with
                member this.ToHtml() = %{ Name = "accepts"; Type = this.Type; Body = this.Method }

        /// <summary>
        ///  The <c>&lt;copyright&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  One of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.Content"/>
        /// </remarks>
        and [<Struct; NoComparison>] Copyright =
            { /// Specifies whether the copyright holder is a person or an organization
              Type: CopyrightHolder option
              /// Email address for inquiries or licensing negotiations
              ContactEmail: string
              /// URL to a web-based contact method, such as a contact form or page
              ContactUrl: string }

            interface IRslElement with
                member this.ToHtml() =
                    let props =
                        [ Attribute.TryCreate this.Type "type" AttributeValue.Author
                          Attribute.TryCreate
                              (Net.Mail.MailAddress.TryCreate(this.ContactEmail) |> (snd >> Option.ofObj))
                              "contactEmail"
                              AttributeValue.Email
                          Attribute.TryCreate
                              (Uri.TryCreate(this.ContactUrl, UriKind.RelativeOrAbsolute)
                               |> (snd >> Option.ofObj))
                              "contactUrl"
                              AttributeValue.Url ]
                        |> (List.map (?+) >> List.collect id)

                    Html.custom "copyright" props []

        /// <summary>
        ///  The <c>&lt;alternate&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  One of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.Content"/>
        /// </remarks>
        and [<Struct; NoComparison>] Alternate =
            { /// <summary>
              ///  The media type of an alternate representation of the licensed work; e.g.,
              ///  <c>text/plain</c>, <c>application/json</c>
              /// </summary>
              Type: string option
              /// The URL of this alternate representation
              Url: System.Uri }

            interface IRslElement with
                member this.ToHtml() =
                    let props = Attribute.TryCreate this.Type "type" AttributeValue.String |> (?+)
                    let content = (!) $"{AttributeValue.Url this.Url}"

                    Html.custom "alternate" props content

        /// <summary>
        ///  The <c>&lt;schema&gt;</c> element of an RSL document
        /// </summary>
        /// <remarks>
        ///  One of these may be a child of <see cref="T:Fornax.Seo.Rsl.DOM.Content"/>
        /// </remarks>
        and Schema =
            /// A stringified <a href="https://schema.org/docs/full.html">Schema.org</a> JSON-LD object
            | Inline of string
            /// The URL of a <a href="https://schema.org/docs/full.html">Schema.org</a> JSON-LD resource
            | Linked of System.Uri
