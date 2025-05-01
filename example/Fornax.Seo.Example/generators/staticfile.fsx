#load "../.paket/load/netstandard2.0/fornax_demo/Fornax.Core.fsx"

open System.IO

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    let inputPath = Path.Combine(projectRoot, page)
    File.ReadAllBytes inputPath
