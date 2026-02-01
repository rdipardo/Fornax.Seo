namespace Fornax.Seo.Tests

module RslTests =
    open System
    open System.IO
    open System.Reflection
    open System.Xml
    open System.Xml.Schema
    open NUnit.Framework
    open Fornax.Seo
    open Fornax.Seo.Rsl.DOM
    open Fornax.Seo.Standards.Rsl
    open UnitTests
    open Mocks

    module internal Shared =
        let domainRoot = Uri(@"/", UriKind.Relative)

        let defaultResource =
            Uri.TryCreate(UnitTest.ContentMeta.Url, UriKind.RelativeOrAbsolute)
            |> (snd >> Option.ofObj >> Option.defaultValue domainRoot)

        let otherResources =
            [ { Type = Some "application/warc"
                Url = Uri("/exports/4f995bd5-799d-4f93-8a21-43ca4e060ffb.warc", UriKind.Relative) }
              { Type = Some "application/json"
                Url = Uri("/exports/c52e2ef9-1dd0-4d82-88d8-58ec79f33c59.json", UriKind.Relative) } ]


    [<TestFixture>]
    type EntitiesTest() =
        let invalid = "foo, bar, baz"

        [<DatapointSource>]
        member val Permissions = Allows.Sample

        [<DatapointSource>]
        member val Restrictions = Forbids.Sample

        [<DatapointSource>]
        member val Provisions = Provides.Sample

        [<DatapointSource>]
        member val UsageTerms = Usage.Sample

        [<DatapointSource>]
        member val UserTerms = User.Sample

        [<DatapointSource>]
        member val GeoTerms = Geo.Sample

        [<DatapointSource>]
        member val WarrantyTerms = Warranty.Sample

        [<DatapointSource>]
        member val DisclaimerTerms = Disclaimer.Sample

        [<DatapointSource>]
        member val ProofContent = Proof.Sample

        [<DatapointSource>]
        member val ContactContent = Contact.Sample

        [<DatapointSource>]
        member val AttestationContent = Attestation.Sample

        [<Theory>]
        member __.``Does not construct an Rsl.DOM.Permits from an empty string``(Allows scope) =
            TestDelegate(fun () -> Permits(scope, String.Empty) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Theory>]
        member __.``Does not construct an Rsl.DOM.Permits from invalid terms``(Allows scope) =
            TestDelegate(fun () -> Permits(scope, invalid) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Theory>]
        member __.``Does not construct an Rsl.DOM.Prohibits from an empty string``(Forbids scope) =
            TestDelegate(fun () -> Prohibits(scope, String.Empty) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Theory>]
        member __.``Does not construct an Rsl.DOM.Prohibits from invalid terms``(Forbids scope) =
            TestDelegate(fun () -> Prohibits(scope, invalid) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Theory>]
        member __.``Does not construct an Rsl.DOM.Legal from an empty string``(Provides scope) =
            TestDelegate(fun () -> Legal(scope, String.Empty) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Theory>]
        member __.``Does not construct an Rsl.DOM.Legal from invalid terms``(Provides scope) =
            TestDelegate(
                match scope with
                | Scopes.Legal.Contact ->
                    let multiple =
                        [ Contact.Sample; Contact.Sample ]
                        |> (Seq.collect id
                            >> Seq.map (fun (Contact content) -> content)
                            >> String.concat ",")

                    (fun () -> Legal(scope, multiple) |> ignore)
                | Scopes.Legal.Proof -> (fun () -> Legal(scope, Shared.domainRoot.OriginalString) |> ignore)
                | _ -> (fun () -> Legal(scope, invalid) |> ignore)
            )
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Theory>]
        member __.``Constructs an instance of Rsl.PermittedUsage from valid usage terms``(Usage terms) =
            Assume.That(String.IsNullOrEmpty terms |> not)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Permits(Scopes.Permission.Usage, terms) |> ignore)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Prohibits(Scopes.Permission.Usage, terms) |> ignore)

        [<Theory>]
        member __.``Constructs an instance of Rsl.PermittedUsage from valid end-user terms``(User terms) =
            Assume.That(String.IsNullOrEmpty terms |> not)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Permits(Scopes.Permission.User, terms) |> ignore)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Prohibits(Scopes.Permission.User, terms) |> ignore)

        [<Theory>]
        member __.``Constructs an instance of Rsl.PermittedUsage from valid geographic regions``(Geo terms) =
            Assume.That(String.IsNullOrEmpty terms |> not)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Permits(Scopes.Permission.Geo, terms) |> ignore)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Prohibits(Scopes.Permission.Geo, terms) |> ignore)

        [<Theory>]
        member __.``Constructs an Rsl.DOM.Legal from valid warranty terms``(Warranty terms) =
            Assume.That(String.IsNullOrEmpty terms |> not)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Legal(Scopes.Legal.Warranty, terms) |> ignore)

        [<Theory>]
        member __.``Constructs an Rsl.DOM.Legal from valid disclaimer terms``(Disclaimer terms) =
            Assume.That(String.IsNullOrEmpty terms |> not)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Legal(Scopes.Legal.Disclaimer, terms) |> ignore)

        [<Theory>]
        member __.``Constructs an Rsl.DOM.Legal from valid proof content``(Proof content) =
            Assume.That(String.IsNullOrEmpty content |> not)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Legal(Scopes.Legal.Proof, content) |> ignore)

        [<Theory>]
        member __.``Constructs an Rsl.DOM.Legal from valid contact content``(Contact content) =
            Assume.That(String.IsNullOrEmpty content |> not)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Legal(Scopes.Legal.Contact, content) |> ignore)

        [<Theory>]
        member __.``Constructs an Rsl.DOM.Legal from valid attestation content``(Attestation content) =
            Assume.That(String.IsNullOrEmpty content |> not)

            Assert.DoesNotThrow
            <| TestDelegate(fun () -> Legal(Scopes.Legal.Attestation, content) |> ignore)

    [<TestFixture>]
    type DomainTest() =
        [<Test>]
        member __.``Does not construct an Rsl.DOM.Root with no content``() =
            TestDelegate
                (fun () ->
                    let emptyList = (ResizeArray<Content>().ToArray()) |> List.ofArray
                    Root(emptyList) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Test>]
        member __.``Does not construct an Rsl.DOM.Content with no license``() =
            TestDelegate
                (fun () ->
                    let emptyList = (ResizeArray<License>().ToArray()) |> List.ofArray
                    Content(Shared.domainRoot, emptyList) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Test>]
        member __.``Requires each instance of Rsl.DOM.Content to reference a unique resource``() =
            TestDelegate
                (fun () ->
                    let licenses = [ License.FreeAndSearchable; License.FreeAndOpenSource("MPL-2.0") ]

                    let duplicates =
                        [ Content(Shared.defaultResource, licenses)
                          Content(Shared.defaultResource, licenses) ]

                    Root(duplicates) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

        [<Test>]
        member __.``Requires each instance of Rsl.DOM.Content to declare a unique set of licenses``() =
            TestDelegate
                (fun () ->
                    let duplicates = [ License.FreeAndSearchable; License.FreeAndSearchable ]

                    let content =
                        [ Content(Shared.domainRoot, duplicates)
                          Content(Shared.defaultResource, duplicates) ]

                    Root(content) |> ignore)
            |> Assert.Throws<ArgumentException>
            |> ignore

    [<TestFixture>]
    type SchemaTest() =
        let specLocation =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "specs", "rsl-1.0.xsd")

        let failWithMessage (e: 'T :> Exception) = AssertionException($"{e.GetType().Name}: {e.Message}", e) |> raise

        let validate (xml: string) =
            try
                let settings = XmlReaderSettings(ValidationType = ValidationType.Schema)
                settings.Schemas.Add("https://rslstandard.org/rsl", specLocation) |> ignore
                settings.ValidationEventHandler.Add(fun evt -> XmlSchemaException(evt.Message) |> raise)

                use stream = new StringReader(xml)
                use reader = XmlReader.Create(stream, settings)

                while reader.Read() do
                    ()
            with
                | :? XmlSchemaException
                | :? AssertionException
                | :? IOException
                | :? ArgumentException as e -> failWithMessage e

        let createDocument =
            let (Usage usage) = Usage.Sample |> Seq.head
            let (User user) = User.Sample |> Seq.head
            let (Geo regions) = Geo.Sample |> Seq.head
            let (Warranty warranties) = Warranty.Sample |> Seq.head
            let (Disclaimer disclaimers) = Disclaimer.Sample |> Seq.head
            let (Contact contact) = Contact.Sample |> Seq.head
            let (Proof proof) = Proof.Sample |> Seq.head
            let (Attestation attestation) = Attestation.Sample |> Seq.head

            let allowedUsage = Permits(Scopes.Permission.Usage, usage)
            let allowedUsers = Permits(Scopes.Permission.User, user)
            let deniedRegions = Prohibits(Scopes.Permission.Geo, regions)
            let hasWarranty = Legal(Scopes.Legal.Warranty, warranties)
            let hasDisclaimer = Legal(Scopes.Legal.Disclaimer, disclaimers)
            let hasAttestation = Legal(Scopes.Legal.Attestation, attestation)
            let hasContact = Legal(Scopes.Legal.Contact, contact)
            let hasProof = Legal(Scopes.Legal.Proof, proof.Split(",") |> Array.randomChoice)

            let crawlRate = { Currency = CurrencyCode.USD; Value = 0.015m }

            let x402 =
                { Type = "application/x402+json"
                  Method =
                      $"""{{"scheme":"exact","price":"${crawlRate.Value}","network":"eip155:84532","payTo":"0xacbd123"}}""" }

            let perCrawl =
                Payment(Scopes.Payment.Crawl, crawlRate, x402, @"https://example.com/licenses/pay-per-crawl")

            let license =
                [ { Permits = Some [ allowedUsage; allowedUsers ]
                    Prohibits = Some [ deniedRegions ]
                    Payment = Some perCrawl
                    Legal = Some [ hasWarranty; hasDisclaimer; hasAttestation; hasContact; hasProof ] } ]

            Content(
                Shared.domainRoot,
                license,
                Schema.Inline $"{Tags.JsonLinkData(UnitTest.ContentMeta)}",
                Shared.otherResources,
                copyright =
                    ({ Type = Some CopyrightHolder.Person
                       ContactEmail = UnitTest.ContentMeta.Author.Email
                       ContactUrl = $"mailto:{UnitTest.ContentMeta.Author.Email}" }),
                server = Uri(@"https://api.example.com", UriKind.Absolute)
            )
            |> (List.singleton >> Root)

        [<Test>]
        member __.``Generates a well-formed RSL document as HTML``() =
            let docRoot = createDocument
            let htmlDoc = docRoot |> Rsl.toHtmlString
            Assert.DoesNotThrow <| TestDelegate(fun () -> htmlDoc |> validate)
            Assert.That(Rsl.Validation.isValid docRoot, $"%A{nameof createDocument} returned invalid HTML")

        [<Test>]
        member __.``Generates a well-formed RSL document as XML``() =
            let docRoot = createDocument
            let builder = Rsl.toXmlDocument docRoot
            Assert.DoesNotThrow <| TestDelegate(fun () -> string builder |> validate)
            Assert.That(Rsl.Validation.isValid docRoot, $"%A{nameof createDocument} returned invalid XML")

        [<Test>]
        member __.``Invalid RSL documents fail to validate``() =
            Assert.That(
                Content(
                    Uri(ContentObject.Default.Url, UriKind.RelativeOrAbsolute),
                    license =
                        [ { Permits = None
                            Prohibits = Some(List.replicate 2 Prohibits.AllBots)
                            Legal = None
                            Payment = None } ]
                )
                |> (List.singleton >> Root >> Rsl.Validation.isValid >> not),
                "Expected validation to fail"
            )
