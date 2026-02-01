namespace Fornax.Seo.Tests

module XmlUnitTests =
    open System
    open System.IO
    open System.Xml
    open System.Xml.XPath
    open NUnit.Framework
    open Fornax.Seo.Xml

    [<AutoOpen>]
    module internal Models =
        type MarkupSource() =
            class
            end

        [<Sealed>]
        type XmlStream(dom: DocumentBuilder<MarkupSource>) =
            let buffer = new MemoryStream(dom.WriterSettings.Encoding.GetBytes(string dom))
            let stream = new StreamReader(buffer, dom.WriterSettings.Encoding, true)
            let reader = XmlReader.Create(stream, dom.ReaderSettings)
            let mutable disposed = false

            member val Reader = reader

            member this.Dispose(disposing: bool) =
                if disposing && not disposed then
                    let (resources: IDisposable seq) = [ buffer; stream; reader ]
                    resources |> Seq.iter (Option.ofObj >> Option.iter (fun res -> res.Dispose()))

                disposed <- true

            interface IDisposable with
                member this.Dispose() =
                    this.Dispose(true)
                    GC.SuppressFinalize(this)

        let builder =
            { new DocumentBuilder<MarkupSource>() with
                member __.AsMarkup e = string e }

        let hasDTD =
            { new MarkupSource() with
                override __.ToString() =
                    """<?xml version='1.0' encoding='utf-8'?>
                       <!DOCTYPE root [
                          <!ELEMENT root (#PCDATA|pre)*>
                          <!ELEMENT pre (#PCDATA)>
                          <!ATTLIST pre xml:space (default|preserve) 'default'>
                          <!ENTITY first '1.'>
                       ]>
                       <root>
                         <!-- significant whitespace -->
                         <pre xml:space='preserve'>
                           &first; first
                           2. second
                           3. third
                         </pre>
                       </root>
                    """ }

        let hasPIs =
            { new MarkupSource() with
                override __.ToString() =
                    """<?sort alpha-ascending?>
                       <?textinfo whitespace is allowed ?>
                       <begin>
                          <end/>
                       </begin>
                    """ }

        let invalidDocument =
            { new MarkupSource() with
                override __.ToString() =
                    """<!-- declaration follows document root -->
                       <root/>
                       <?xml version='1.0' encoding='utf-8'?>
                    """ }

        let invalidAttributeFragment =
            { new MarkupSource() with
                override __.ToString() =
                    "<root id='cbd55a9b-0fda-4adb-98d6-cb08e9084ca3><!-- missing end quote --></root>" }

        let invalidElementFragment =
            { new MarkupSource() with
                override __.ToString() = "<root><!-- unclosed element -->" }


    [<TestFixture>]
    type UnitTest() =
        let xmlDecl = @"<?xml version=""1.0"" encoding=""utf-8""?>"

        [<Test>]
        member __.``Can parse and generate complete documents``() =
            TestDelegate
                (fun () ->
                    builder.ReaderSettings.DtdProcessing <- DtdProcessing.Parse
                    Assert.That(builder.TryCreate hasDTD, $"Model %A{nameof hasDTD} in not a valid XML document")
                    Assert.That(builder.ToString().StartsWith(xmlDecl), $"Expected document to start with %A{xmlDecl}")

                    use stream = new XmlStream(builder)
                    let navigator = XPathDocument(stream.Reader, XmlSpace.Preserve).CreateNavigator()
                    let xpath = navigator.SelectSingleNode("/root/pre")

                    Assert.That(obj.ReferenceEquals(xpath, null) |> not, "Expected to find a node at /root/pre")
                    Assert.That(xpath.InnerXml.Contains("1. first"), $"Expected entity refs to be expanded"))
            |> Assert.Multiple

        [<Test>]
        member __.``Can parse and generate document fragments``() =
            TestDelegate
                (fun () ->
                    builder.ReaderSettings.IgnoreProcessingInstructions <- false
                    Assert.That(builder.TryCreate hasPIs, $"Model %A{nameof hasPIs} in not a valid XML fragment")

                    use stream = new XmlStream(builder)
                    let navigator = XPathDocument(stream.Reader, XmlSpace.Preserve).CreateNavigator()

                    navigator.MoveToRoot()

                    Assert.That(
                        navigator.MoveToFollowing(XPathNodeType.ProcessingInstruction),
                        "Expected to find a processing instruction"
                    )

                    Assert.That(
                        navigator.MoveToFollowing(XPathNodeType.ProcessingInstruction),
                        "Expected to find a second processing instruction"
                    )

                    Assert.That(navigator.MoveToFollowing(XPathNodeType.Element), "Expected to find the root element")

                    Assert.That(
                        navigator.LocalName.Equals("begin", StringComparison.OrdinalIgnoreCase),
                        @"Expected this element to have the name ""begin"""
                    ))
            |> Assert.Multiple

        [<Test>]
        member __.``Detects invalid documents``() =
            TestDelegate
                (fun () ->
                    Assert.That(builder.TryCreate invalidDocument |> not, "Expected validation to fail")

                    TestDelegate(fun () -> builder.Create invalidDocument)
                    |> (Assert.Throws<System.Xml.XmlException> >> ignore))
            |> Assert.Multiple

        [<Test>]
        member __.``Detects invalid attributes in document fragments``() =
            TestDelegate
                (fun () ->
                    Assert.That(builder.TryCreate invalidAttributeFragment |> not, "Expected validation to fail")

                    TestDelegate(fun () -> builder.Create invalidAttributeFragment)
                    |> (Assert.Throws<System.Xml.XmlException> >> ignore))
            |> Assert.Multiple

        [<Test>]
        member __.``Detects invalid elements in document fragments``() =
            TestDelegate
                (fun () ->
                    Assert.That(builder.TryCreate invalidElementFragment |> not, "Expected validation to fail")

                    TestDelegate(fun () -> builder.Create invalidElementFragment)
                    |> (Assert.Throws<System.Xml.XmlException> >> ignore))
            |> Assert.Multiple
