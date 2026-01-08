//
// Copyright (c) 2026 Robert Di Pardo and Contributors
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at https://mozilla.org/MPL/2.0/.
//

namespace Fornax.Seo

/// RSL attribute types, formal vocabulary and related specifications
module Standards =

    /// Marker interface implemented by RSL entities
    type internal IRslEntity =
        interface
        end

    /// ISO specifications followed by RSL
    module ISO =

        /// A country designator complying with <a href="https://www.iso.org/iso-3166-country-codes.html">ISO 3166-1</a> (alpha-2)
        type CountryCode =
            | AD
            | AE
            | AF
            | AG
            | AI
            | AL
            | AM
            | AO
            | AQ
            | AR
            | AS
            | AT
            | AU
            | AW
            | AX
            | AZ
            | BA
            | BB
            | BD
            | BE
            | BF
            | BG
            | BH
            | BI
            | BJ
            | BL
            | BM
            | BN
            | BO
            | BQ
            | BR
            | BS
            | BT
            | BV
            | BW
            | BY
            | BZ
            | CA
            | CC
            | CD
            | CF
            | CG
            | CH
            | CI
            | CK
            | CL
            | CM
            | CN
            | CO
            | CR
            | CU
            | CV
            | CW
            | CX
            | CY
            | CZ
            | DE
            | DJ
            | DK
            | DM
            | DO
            | DZ
            | EC
            | EE
            | EG
            | EH
            | ER
            | ES
            | ET
            /// <summary>
            ///  European Union member states(?)
            /// </summary>
            /// <remarks>
            ///  Non-standard RSL addition. See the definition of <c>geoToken</c> in the
            ///  <a href="https://rslstandard.org/rsl#appendix-a-rsl-relax-ng-compact-schema">schema</a>
            /// </remarks>
            | EU
            | FI
            | FJ
            | FK
            | FM
            | FO
            | FR
            | GA
            | GB
            | GD
            | GE
            | GF
            | GG
            | GH
            | GI
            | GL
            | GM
            | GN
            | GP
            | GQ
            | GR
            | GS
            | GT
            | GU
            | GW
            | GY
            | HK
            | HM
            | HN
            | HR
            | HT
            | HU
            | ID
            | IE
            | IL
            | IM
            | IN
            | IO
            | IQ
            | IR
            | IS
            | IT
            | JE
            | JM
            | JO
            | JP
            | KE
            | KG
            | KH
            | KI
            | KM
            | KN
            | KP
            | KR
            | KW
            | KY
            | KZ
            | LA
            | LB
            | LC
            | LI
            | LK
            | LR
            | LS
            | LT
            | LU
            | LV
            | LY
            | MA
            | MC
            | MD
            | ME
            | MF
            | MG
            | MH
            | MK
            | ML
            | MM
            | MN
            | MO
            | MP
            | MQ
            | MR
            | MS
            | MT
            | MU
            | MV
            | MW
            | MX
            | MY
            | MZ
            | NA
            | NC
            | NE
            | NF
            | NG
            | NI
            | NL
            | NO
            | NP
            | NR
            | NU
            | NZ
            | OM
            | PA
            | PE
            | PF
            | PG
            | PH
            | PK
            | PL
            | PM
            | PN
            | PR
            | PS
            | PT
            | PW
            | PY
            | QA
            | RE
            | RO
            | RS
            | RU
            | RW
            | SA
            | SB
            | SC
            | SD
            | SE
            | SG
            | SH
            | SI
            | SJ
            | SK
            | SL
            | SM
            | SN
            | SO
            | SR
            | SS
            | ST
            | SV
            | SX
            | SY
            | SZ
            | TC
            | TD
            | TF
            | TG
            | TH
            | TJ
            | TK
            | TL
            | TM
            | TN
            | TO
            | TR
            | TT
            | TV
            | TW
            | TZ
            | UA
            | UG
            | UM
            | US
            | UY
            | UZ
            | VA
            | VC
            | VE
            | VG
            | VI
            | VN
            | VU
            | WF
            | WS
            | YE
            | YT
            | ZA
            | ZM
            | ZW

            interface IRslEntity

        /// A currency designator complying with <a href="https://www.iso.org/iso-4217-currency-codes.html">ISO 4217</a>
        type CurrencyCode =
            | AED
            | AFN
            | ALL
            | AMD
            | AOA
            | ARS
            | AUD
            | AWG
            | AZN
            | BAM
            | BBD
            | BDT
            | BGN
            | BHD
            | BIF
            | BMD
            | BND
            | BOB
            | BOV
            | BRL
            | BSD
            | BTN
            | BWP
            | BYN
            | BZD
            | CAD
            | CDF
            | CHE
            | CHF
            | CHW
            | CLF
            | CLP
            | CNY
            | COP
            | COU
            | CRC
            | CUP
            | CVE
            | CZK
            | DJF
            | DKK
            | DOP
            | DZD
            | EGP
            | ERN
            | ETB
            | EUR
            | FJD
            | FKP
            | GBP
            | GEL
            | GHS
            | GIP
            | GMD
            | GNF
            | GTQ
            | GYD
            | HKD
            | HNL
            | HTG
            | HUF
            | IDR
            | ILS
            | INR
            | IQD
            | IRR
            | ISK
            | JMD
            | JOD
            | JPY
            | KES
            | KGS
            | KHR
            | KMF
            | KPW
            | KRW
            | KWD
            | KYD
            | KZT
            | LAK
            | LBP
            | LKR
            | LRD
            | LSL
            | LYD
            | MAD
            | MDL
            | MGA
            | MKD
            | MMK
            | MNT
            | MOP
            | MRU
            | MUR
            | MVR
            | MWK
            | MXN
            | MXV
            | MYR
            | MZN
            | NAD
            | NGN
            | NIO
            | NOK
            | NPR
            | NZD
            | OMR
            | PAB
            | PEN
            | PGK
            | PHP
            | PKR
            | PLN
            | PYG
            | QAR
            | RON
            | RSD
            | RUB
            | RWF
            | SAR
            | SBD
            | SCR
            | SDG
            | SEK
            | SGD
            | SHP
            | SLE
            | SOS
            | SRD
            | SSP
            | STN
            | SVC
            | SYP
            | SZL
            | THB
            | TJS
            | TMT
            | TND
            | TOP
            | TRY
            | TTD
            | TWD
            | TZS
            | UAH
            | UGX
            | USD
            | USN
            | UYI
            | UYU
            | UYW
            | UZS
            | VED
            | VES
            | VND
            | VUV
            | WST
            | XBT
            | YER
            | ZAR
            | ZMW
            | ZWG


    /// Element attribute types and qualifiers
    module Rsl =

        /// <summary>
        ///  Permitted values of <see cref="P:Fornax.Seo.Rsl.DOM.Copyright.Type"/>
        /// </summary>
        [<RequireQualifiedAccess>]
        type CopyrightHolder =
            /// An incorporated copyright holder
            | Organization
            /// An individual copyright holder
            | Person

        /// <summary>
        ///  Permitted values of <see cref="P:Fornax.Seo.Rsl.DOM.Amount.Currency"/>
        /// </summary>
        [<RequireQualifiedAccess>]
        type CurrencyCode = ISO.CurrencyCode

        /// <summary>
        ///  Specifies whether an element defines permissions or restrictions regarding usage of a licensed work
        /// </summary>
        /// <remarks>Used to specialize an instance of <see cref="T:Fornax.Seo.Rsl.PermittedUsage"/></remarks>
        [<RequireQualifiedAccess>]
        type Disposition =
            /// <summary>Specifies the role of a <c>&lt;permits&gt;</c> element</summary>
            | Permits
            /// <summary>Specifies the role of a <c>&lt;prohibits&gt;</c> element</summary>
            | Prohibits

            override __.ToString() =
                __
                |> function
                | Permits -> "permits"
                | Prohibits -> "prohibits"


        /// <summary>
        ///  Permitted <c>type</c> attribute values for element children of <see cref="T:Fornax.Seo.Rsl.DOM.License"/>
        /// </summary>
        module Scopes =
            /// <summary>
            ///  Uniquely identifies an element of type <see cref="T:Fornax.Seo.Rsl.DOM.Permits"/> or
            ///  <see cref="T:Fornax.Seo.Rsl.DOM.Prohibits"/>
            /// </summary>
            [<RequireQualifiedAccess>]
            type Permission =
                /// <summary>
                ///  Qualifies an instance of <see cref="T:Fornax.Seo.Rsl.PermittedUsage"/>, or a derived type, by restricting usage
                ///  to one or more geographic regions
                /// </summary>
                | Geo
                /// <summary>
                ///  Qualifies an instance of <see cref="T:Fornax.Seo.Rsl.PermittedUsage"/>, or a derived type, by specifying
                ///  allowed forms of usage by an AI system
                /// </summary>
                | Usage
                /// <summary>
                ///  Qualifies an instance of <see cref="T:Fornax.Seo.Rsl.PermittedUsage"/>, or a derived type, by restricting a
                ///  license to certain classes of end users
                /// </summary>
                | User

            /// <summary>
            ///  Uniquely identifies an element of type <see cref="T:Fornax.Seo.Rsl.DOM.Legal"/>
            /// </summary>
            [<RequireQualifiedAccess>]
            type Legal =
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Legal"/> element by affirming the publisher's authority to assert
                ///  the rights described in the license
                /// </summary>
                | Attestation
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Legal"/> element by providing the web address or email of the
                ///  licensed work's copyright holder
                /// </summary>
                | Contact
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Legal"/> element by disclaiming or qualifying any warranties offered
                ///  on the licensed work
                /// </summary>
                | Disclaimer
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Legal"/> element by providing one or more hyperlinks proving the
                ///  identity of the licensed work's copyright holder
                /// </summary>
                | Proof
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Legal"/> element by declaring one or more warranties offered on the
                ///  licensed work
                /// </summary>
                | Warranty

            /// <summary>
            ///  Uniquely identifies an element of type <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/>
            /// </summary>
            [<RequireQualifiedAccess>]
            type Payment =
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/> element by requiring visible credit and a functional
                ///  link to the original of the licensed work
                /// </summary>
                | Attribution
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/> element by requiring a monetary or in-kind contribution
                ///  to support the development or maintenance of the licensed work
                /// </summary>
                | Contribution
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/> element by requiring compensation every time the licensed
                ///  work is crawled
                /// </summary>
                | Crawl
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/> element by waiving any demand for compensation
                /// </summary>
                | Free
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/> element by requiring the purchase of the licensed work
                /// </summary>
                | Purchase
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/> element by requiring a recurring payment to access the
                ///  licensed work
                /// </summary>
                | Subscription
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/> element by requiring compensation every time the licensed
                ///  work is used for AI training
                /// </summary>
                | Training
                /// <summary>
                ///  Qualifies a <see cref="T:Fornax.Seo.Rsl.DOM.Payment"/> element by requiring compensation every time the licensed
                ///  work contributes to AI-generated output
                /// </summary>
                | Use


        /// Type representations of RSL's formal vocabulary
        module internal Entities =
            open System
            open System.Reflection
            open Microsoft.FSharp.Reflection
            open type System.StringComparison

            /// A unique set of vocabulary terms, convertible to machine-readable text
            type Entity<'T when 'T :> IRslEntity>(names: string, ?delimiters: char array) =
                let mutable values : 'T option seq = Seq.empty

                do
                    // Adapted from http://fssnip.net/7VM
                    let tryParse (name: string) =
                        let flags = BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly

                        try
                            FSharpType.GetUnionCases(typeof<'T>, flags)
                            |> Seq.tryFind
                                (fun ci ->
                                    String.Equals(name.Trim().Replace("-", ""), ci.Name, InvariantCultureIgnoreCase))
                            |> Option.map
                                (fun ci ->
                                    let args : obj array = Array.zeroCreate (ci.GetFields().Length)
                                    FSharpValue.MakeUnion(ci, args, flags) :?> 'T)
                        with
                            | :? ArgumentException
                            | :? InvalidCastException -> None

                    values <-
                        names.Split(defaultArg delimiters [| ',' |], StringSplitOptions.RemoveEmptyEntries)
                        |> (Seq.map tryParse >> Seq.where Option.isSome)

                member __.Values = values

                override __.ToString() =
                    values
                    |> (Seq.map ((Option.map string) >> Option.defaultValue "") >> String.concat " ")


            /// <summary>
            ///  Types defining the legal obligations associated with a license
            /// </summary>
            /// <remarks>See https://rslstandard.org/rsl#_3-12-legal-legal-statements</remarks>
            module Declarations =

                /// <summary>
                ///  Qualifies a license of type <see cref="T:Fornax.Seo.Standards.Rsl.Scope.Warranty"/>
                /// </summary>
                [<RequireQualifiedAccess>]
                type Warranty =
                    | Authority
                    | NoInfringement
                    | NoMalware
                    | Ownership
                    | PrivacyConsent

                    interface IRslEntity

                    override __.ToString() =
                        __
                        |> function
                        | Authority -> "authority"
                        | NoInfringement -> "no-infringement"
                        | NoMalware -> "no-malware"
                        | Ownership -> "ownership"
                        | PrivacyConsent -> "privacy-consent"

                /// <summary>
                ///  Qualifies a license of type <see cref="T:Fornax.Seo.Standards.Rsl.Scope.Disclaimer"/>
                /// </summary>
                [<RequireQualifiedAccess>]
                type Disclaimer =
                    | AsIs
                    | NoIndemnity
                    | NoLiability
                    | NoWarranty

                    interface IRslEntity

                    override __.ToString() =
                        __
                        |> function
                        | AsIs -> "as-is"
                        | NoIndemnity -> "no-indemnity"
                        | NoLiability -> "no-liability"
                        | NoWarranty -> "no-warranty"


            /// <summary>
            ///  Types defining the kind of usage permitted or prohibited by a license
            /// </summary>
            /// <remarks>
            /// See:
            /// <ul>
            ///  <li>https://rslstandard.org/rsl#_3-5-permits-allowed-usess</li>
            ///  <li>https://rslstandard.org/rsl#_3-6-prohibits-disallowed-uses</li>
            /// </ul>
            /// </remarks>
            module Restrictions =

                /// <summary>
                ///  Qualifies a permission of type <see cref="T:Fornax.Seo.Standards.Rsl.Scope.Usage"/>
                /// </summary>
                [<RequireQualifiedAccess>]
                type Usage =
                    | All
                    | AiAll
                    | AiIndex
                    | AiInput
                    | AiTrain
                    | Search

                    interface IRslEntity

                    override __.ToString() =
                        __
                        |> function
                        | All -> "all"
                        | AiAll -> "ai-all"
                        | AiIndex -> "ai-index"
                        | AiInput -> "ai-input"
                        | AiTrain -> "ai-train"
                        | Search -> "search"

                /// <summary>
                ///  Qualifies a permission of type <see cref="T:Fornax.Seo.Standards.Rsl.Scope.User"/>
                /// </summary>
                [<RequireQualifiedAccess>]
                type User =
                    | Commercial
                    | Education
                    | Government
                    | NonCommercial
                    | Personal

                    interface IRslEntity

                    override __.ToString() =
                        __
                        |> function
                        | Commercial -> "commercial"
                        | Education -> "education"
                        | Government -> "government"
                        | NonCommercial -> "non-commercial"
                        | Personal -> "personal"

                /// <summary>
                ///  Qualifies a permission of type <see cref="T:Fornax.Seo.Standards.Rsl.Scope.Geo"/>
                /// </summary>
                [<RequireQualifiedAccess>]
                type Geo = ISO.CountryCode
