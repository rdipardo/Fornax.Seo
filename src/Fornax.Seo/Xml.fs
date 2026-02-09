//
// Copyright (c) 2026 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.
//

namespace Fornax.Seo

/// XML generation utilities
module rec Xml =
    open System
    open System.IO
    open System.Text
    open System.Xml
    open System.Xml.Schema
    open System.Runtime.InteropServices

    /// <summary>
    ///  Transforms (X/HT)ML markup text into XML documents or fragments
    /// </summary>
    /// <typeparam name="TSource">Any type that can be represented as an (X/HT)ML string</typeparam>
    [<AbstractClass>]
    type DocumentBuilder<'TSource> =
        /// <summary>
        ///  The underlying XML document object of this <see cref="T:Fornax.Seo.Xml.DocumentBuilder`1"/>
        /// </summary>
        val Root: XmlDocument

        /// <summary>
        ///  The settings used to parse incoming markup text
        /// </summary>
        val ReaderSettings: XmlReaderSettings

        /// <summary>
        ///  The settings used by the underlying document object to create XML
        /// </summary>
        val WriterSettings: XmlWriterSettings

        /// <summary>
        ///  Creates a new <see cref="T:Fornax.Seo.Xml.DocumentBuilder`1"/>
        /// </summary>
        new() =
            { Root = XmlDocument(PreserveWhitespace = true)
              ReaderSettings = Configuration.defaultReaderSettings ()
              WriterSettings = Configuration.defaultWriterSettings () }

        /// <summary>
        ///  Creates a new <see cref="T:Fornax.Seo.Xml.DocumentBuilder`1"/> with the given XML <paramref name="schema"/>,
        ///  optional validation event handler, and output format options
        /// </summary>
        /// <param name="schema">
        ///  The XML schema used to validate source markup text. No validation is performed if this argument is <c>null</c>
        /// </param>
        /// <param name="eventHandler">
        ///  An optional handler for schema validation errors
        /// </param>
        /// <param name="writeXmlDecl">
        ///  <c>true</c> to include the <tt>&lt;?xml ...?&gt;</tt> declaration in generated documents (omitted if the source markup
        ///  is a document fragment). The default is <c>true</c>
        /// </param>
        /// <param name="writeBom">
        ///  <c>true</c> to include a Unicode byte order mark in generated document text. The default is <c>false</c>
        /// </param>
        /// <param name="minify">
        ///  <c>true</c> to remove indentation and whitespace from the generated document or fragment, not including text in the scope
        ///  of the <a href="https://www.w3.org/TR/1998/REC-xml-19980210#sec-white-space"><tt>xml:space="preserve"</tt> attribute</a>.
        ///  The default is <c>false</c>
        /// </param>
        new(schema: XmlSchema,
            [<Optional; DefaultParameterValue(null: ValidationEventHandler)>] eventHandler: ValidationEventHandler,
            [<Optional; DefaultParameterValue(true)>] writeXmlDecl: bool,
            [<Optional; DefaultParameterValue(false)>] writeBom: bool,
            [<Optional; DefaultParameterValue(false)>] minify: bool) as this =
            { Root = XmlDocument()
              ReaderSettings = Configuration.validatingReaderSettings schema eventHandler
              WriterSettings = Configuration.defaultWriterSettings () }
            then
                this.Root.PreserveWhitespace <- not minify
                this.WriterSettings.Indent <- not minify
                this.WriterSettings.Encoding <- UTF8Encoding(writeBom, true)
                this.WriterSettings.OmitXmlDeclaration <- not writeXmlDecl

                try
                    if
                        schema |> (ValueOption.ofObj >> ValueOption.isSome)
                        && this.ReaderSettings.Schemas.Contains(schema)
                    then
                        this.Root.Schemas.Add(schema) |> ignore
                with e -> Handlers.logExnMessage e.Message e

        /// <summary>
        ///  When overridden by a derived class, returns a string of (X/HT)ML markup representing the given <paramref name="instance"/>
        /// </summary>
        /// <example>
        ///  <code>
        ///   open Fornax.Seo.Xml
        ///
        ///   type MarkupSource() =
        ///       class
        ///       end
        ///
        ///   let builder =
        ///       { new DocumentBuilder&lt;MarkupSource&gt;() with
        ///           member __.AsMarkup e = e.ToString() }
        ///
        ///   let doclet =
        ///       { new MarkupSource() with
        ///           override __.ToString() =
        ///               """&lt;?xml version='1.0' encoding='utf-8'?&gt;
        ///                  &lt;root&gt;
        ///                    &lt;pre xml:space='preserve'&gt;
        ///                      1. first
        ///                      2. second
        ///                      3. third
        ///                    &lt;/pre&gt;
        ///                  &lt;/root&gt;
        ///             """ }
        ///
        ///   builder.Create doclet
        ///  </code>
        /// </example>
        abstract member AsMarkup : instance: 'TSource -> string

        /// <summary>
        ///  Parses the markup provided by <paramref name="source"/> and loads it into the underlying document object
        /// </summary>
        /// <param name="source">Any type that can be represented as an (X/HT)ML string</param>
        /// <exception cref="T:System.Xml.XmlException">
        ///  Thrown if <paramref name="source"/> could not be parsed or loaded successfully
        /// </exception>
        member this.Create(source: 'TSource) = this.AsMarkup source |> this.FromText

        /// <summary>
        ///  Non-throwing alternative to <see cref="M:Fornax.Seo.Xml.DocumentBuilder`1.Create(`0)"/>
        /// </summary>
        /// <param name="source">Any type that can be represented as an (X/HT)ML string</param>
        /// <returns>
        ///  <c>true</c> if <paramref name="source"/> was parsed and loaded successfully; otherwise <c>false</c>
        /// </returns>
        member this.TryCreate(source: 'TSource) =
            try
                this.Create source
                true
            with _ -> false

        /// <summary>
        ///  Returns the underlying document object as an XML string
        /// </summary>
        override this.ToString() = this.Root.OuterXml

        /// <summary>
        ///  Builds a DOM tree in the underlying document object from the given <paramref name="markup"/> text
        /// </summary>
        member private this.FromText(markup: string) =
            try
                use outStream = new MemoryStream(1024)
                use inStream = new MemoryStream(this.WriterSettings.Encoding.GetBytes(markup))
                use xmlStream = new StreamReader(inStream, this.WriterSettings.Encoding)
                use reader = XmlReader.Create(xmlStream, this.ReaderSettings)
                use writer = XmlWriter.Create(outStream, this.WriterSettings)
                let mutable lastValidAttribute = String.Empty

                try
                    while reader.Read() do
                        match reader.NodeType with
                        | XmlNodeType.Element ->
                            lastValidAttribute <- String.Empty
                            writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI)

                            while reader.MoveToNextAttribute() do
                                writer.WriteAttributeString(
                                    reader.Prefix,
                                    reader.LocalName,
                                    reader.NamespaceURI,
                                    reader.Value
                                )

                            // Don't report attributes in exception messages if the enclosing element is really at fault
                            lastValidAttribute <- reader.GetAttribute(reader.LocalName, reader.NamespaceURI)
                            reader.MoveToElement() |> ignore
                        | XmlNodeType.EndElement -> writer.WriteEndElement()
                        | XmlNodeType.Text ->
                            writer.WriteString(
                                if reader.XmlSpace <> XmlSpace.Preserve then
                                    reader.Value.Trim()
                                else
                                    reader.Value
                            )
                        | XmlNodeType.CDATA -> writer.WriteCData(reader.Value)
                        | XmlNodeType.Comment -> writer.WriteComment(reader.Value)
                        | XmlNodeType.Whitespace
                        | XmlNodeType.SignificantWhitespace -> writer.WriteWhitespace(reader.Value)
                        | XmlNodeType.ProcessingInstruction
                        | XmlNodeType.XmlDeclaration -> writer.WriteProcessingInstruction(reader.Name, reader.Value)
                        | _ -> writer.WriteNode(reader, true)

                    writer.Flush()
                    writer.Close()
                    outStream.Seek(0L, SeekOrigin.Begin) |> ignore
                    this.Root.Load(outStream)
                with :? XmlException as e when String.IsNullOrWhiteSpace lastValidAttribute ->
                    Handlers.XmlReaderException.Create(&reader, &e) |> raise
            with e -> raise e

    /// <summary>
    ///  Creates a default handler for XML schema validation events
    /// </summary>
    /// <param name="helpLink">
    ///  An optional link to additional information about the last exception
    /// </param>
    /// <remarks>
    ///  Events tagged with a <see cref="T:System.Xml.Schema.XmlSeverityType"/> of <c>Error</c> will rethrow the last exception.
    ///  All other events are logged to either the (Windows) user's temp file directory, or the active application's directory
    /// </remarks>
    [<CompiledName("GetValidationEventHandler")>]
    let getValidationEventHandler ([<Optional; DefaultParameterValue("")>] helpLink: string) =
        ValidationEventHandler
            (fun (_: obj | null) (evt: ValidationEventArgs) ->
                match evt.Severity with
                | XmlSeverityType.Error ->
#if DEBUG
                    Handlers.logEvent evt
#endif
                    XmlSchemaException(Handlers.describeEvent evt, evt.Exception, HelpLink = helpLink)
                    |> raise
                | _ -> Handlers.logEvent evt)


    /// Utilities for reporting XML exceptions
    module internal Handlers =
        let logDir =
            (if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then "TEMP" else "TMPDIR")
            |> Environment.GetEnvironmentVariable
            |> (ValueOption.ofObj >> ValueOption.defaultValue ".")

        let inline locateExnSource
            (e: 'T when 'T :> exn and 'T: (member LineNumber : int) and 'T: (member LinePosition : int))
            =
            $"(line {e.LineNumber}, pos. {e.LinePosition})"

        let describeEvent (e: ValidationEventArgs) = $"{locateExnSource e.Exception}: {e.Message}"

        let logExnMessage message (e: exn) =
            try
                let logPath = Path.Combine(logDir, $"{AppDomain.CurrentDomain.FriendlyName}.log")
                let datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                let info = $"[{datetime}] {e.GetType().FullName}: {message}{Environment.NewLine}"
                File.AppendAllText(logPath, info, Encoding.Default)
            with _ -> ()

        let logEvent (e: ValidationEventArgs) = logExnMessage (describeEvent e) e.Exception

        /// Returns information about the current node when the last XML exception occurred
        [<Struct; NoComparison; NoEquality>]
        type XmlReaderException =
            static member Create(reader: XmlReader inref, inner: XmlException inref) =
                let nodeLocation =
                    if reader |> (ValueOption.ofObj >> ValueOption.isNone) then
                        String.Empty
                    else
                        (match [ reader.Name; reader.Value ] |> List.map String.IsNullOrWhiteSpace with
                         | [ false; false ] -> $" with Name={reader.Name} and Value=%A{reader.Value}"
                         | [ false; true ] -> $" with Name={reader.Name}"
                         | [ true; false ] -> $" with Value=%A{reader.Value}"
                         | _ -> "")
                        |> (Seq.singleton >> Seq.fold (+) $" %A{reader.NodeType}")

                XmlException($"Exception thrown while parsing{nodeLocation} at {locateExnSource inner}", inner)


    /// Utilities to simplify construction of new <see cref="T:Fornax.Seo.Xml.DocumentBuilder`1"/> instances
    module private Configuration =

        /// Returns a set of baseline, non-validating reader settings
        let defaultReaderSettings _ =
            XmlReaderSettings(
                ConformanceLevel = ConformanceLevel.Auto,
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreProcessingInstructions = true,
                Schemas = XmlSchemaSet(XmlResolver = null)
            )

        /// Returns a set of baseline writer settings
        let defaultWriterSettings _ =
            XmlWriterSettings(
                ConformanceLevel = ConformanceLevel.Auto,
                Encoding = UTF8Encoding(false, true),
                Indent = true,
                OmitXmlDeclaration = false
            )

        /// Creates and returns a set of reader settings with the given XML schema and validation event handler
        let validatingReaderSettings (schemaOption: XmlSchema) (handlerOption: ValidationEventHandler) =
            let settings = defaultReaderSettings ()

            schemaOption
            |> ValueOption.ofObj
            |> function
            | ValueNone -> settings
            | ValueSome schema ->
                try
                    handlerOption
                    |> ValueOption.ofObj
                    |> ValueOption.iter
                        (fun handler -> settings.ValidationEventHandler.Add(fun evt -> handler.Invoke(schema, evt)))

                    settings.ValidationType <- ValidationType.Schema

                    settings.ValidationFlags <-
                        XmlSchemaValidationFlags.ReportValidationWarnings
                        ||| XmlSchemaValidationFlags.ProcessIdentityConstraints
                        ||| XmlSchemaValidationFlags.AllowXmlAttributes

                    settings.Schemas.Add(schema) |> ignore
                    settings.Schemas.Compile()
                    settings
                with e ->
                    Handlers.logExnMessage e.Message e
                    defaultReaderSettings ()
