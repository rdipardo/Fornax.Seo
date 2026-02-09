namespace Fornax.Seo.Tests

module Mocks =
    open System
    open Fornax.Seo.Standards.ISO
    open Fornax.Seo.Standards.Rsl

    /// Simulates a union case of type <see cref="T:Fornax.Seo.Standards.Rsl.Entities.Permissions.Usage"/>
    type Usage =
        | Usage of terms: string

        static member inline Sample =
            [| "search"; "all"; "ai-train"; "ai-all"; "ai-index"; "ai-input" |]
            |> (String.concat "," >> Usage.Usage >> Seq.singleton)

    /// Simulates a union case of type <see cref="T:Fornax.Seo.Standards.Rsl.Entities.Permissions.User"/>
    type User =
        | User of terms: string

        static member inline Sample =
            [| "commercial"; "education"; "government"; "non-commercial"; "personal" |]
            |> (String.concat "," >> User.User >> Seq.singleton)

    /// Simulates a union case of type <see cref="T:Fornax.Seo.Standards.Rsl.Entities.Permissions.Geo"/>
    type Geo =
        | Geo of terms: string

        static member inline Sample =
            [| CountryCode.AU
               CountryCode.BD
               CountryCode.CX
               CountryCode.DK
               CountryCode.EU
               CountryCode.FM
               CountryCode.GG
               CountryCode.HK
               CountryCode.ID
               CountryCode.JM
               CountryCode.KZ
               CountryCode.LU
               CountryCode.MZ
               CountryCode.NR
               CountryCode.OM
               CountryCode.PA
               CountryCode.QA
               CountryCode.RU
               CountryCode.SC
               CountryCode.TJ
               CountryCode.US
               CountryCode.VN
               CountryCode.WF
               CountryCode.YE
               CountryCode.YT
               CountryCode.ZW |]
            |> (Seq.map string
                >> Seq.randomSample (Random().Next(1, 26))
                >> String.concat ","
                >> Geo.Geo
                >> Seq.singleton)

    /// Simulates a union case of type <see cref="T:Fornax.Seo.Standards.Rsl.Entities.Declarations.Warranty"/>
    type Warranty =
        | Warranty of terms: string

        static member inline Sample =
            [| "authority"
               "no-infringement"
               "no-malware"
               "ownership"
               "privacy-consent" |]
            |> (String.concat "," >> Warranty.Warranty >> Seq.singleton)

    /// Simulates a union case of type <see cref="T:Fornax.Seo.Standards.Rsl.Entities.Declarations.Disclaimer"/>
    type Disclaimer =
        | Disclaimer of terms: string

        static member inline Sample =
            [| "as-is"; "no-indemnity"; "no-liability"; "no-warranty" |]
            |> (String.concat "," >> Disclaimer.Disclaimer >> Seq.singleton)

    /// Simulated content for entities of type <see cref="T:Fornax.Seo.Standards.Scopes.Legal.Proof"/>
    type Proof =
        | Proof of content: string

        static member inline Sample =
            [| "https://blockscan.example/tx/0x9aBcD123"
               "https://projects/ed47-4512-cda3-5610/logs/cloudaudit.googleapis.com%2Faccess_transparency" |]
            |> (String.concat "," >> Proof.Proof >> Seq.singleton)

    /// Simulated content for entities of type <see cref="T:Fornax.Seo.Standards.Scopes.Legal.Contact"/>
    type Contact =
        | Contact of content: string

        static member inline Sample =
            [| "https://example.com/contact"; "mailto:info@example.com" |]
            |> (Seq.randomChoice >> Contact.Contact >> Seq.singleton)

    /// Simulated content for entities of type <see cref="T:Fornax.Seo.Standards.Scopes.Legal.Attestation"/>
    type Attestation =
        | Attestation of content: string

        static member inline Sample =
            [| "false"; "true" |]
            |> (Seq.randomChoice >> Attestation.Attestation >> Seq.singleton)

    /// Positively enumerates the cases of <see cref="T:Fornax.Seo.Standards.Scopes.Permission"/>
    type Allows =
        | Allows of scope: Scopes.Permission

        static member inline Sample =
            [| Scopes.Permission.Geo; Scopes.Permission.Usage; Scopes.Permission.User |]
            |> Seq.map Allows.Allows

    /// Negatively enumerates the cases of <see cref="T:Fornax.Seo.Standards.Scopes.Permission"/>
    type Forbids =
        | Forbids of scope: Scopes.Permission

        static member inline Sample =
            [| Scopes.Permission.Geo; Scopes.Permission.Usage; Scopes.Permission.User |]
            |> Seq.map Forbids.Forbids

    /// Enumerates the cases of <see cref="T:Fornax.Seo.Standards.Scopes.Legal"/>
    type Provides =
        | Provides of scope: Scopes.Legal

        static member inline Sample =
            [| Scopes.Legal.Attestation
               Scopes.Legal.Contact
               Scopes.Legal.Disclaimer
               Scopes.Legal.Proof
               Scopes.Legal.Warranty |]
            |> Seq.map Provides.Provides
