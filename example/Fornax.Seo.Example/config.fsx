#r "_lib/Fornax.Core.dll"
#load "loaders/globalloader.fsx"

open Config
open System.IO

let postPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    page.StartsWith("posts") && List.contains ext Globalloader.contentFileTypes

let staticPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    let fileShouldBeExcluded =
        List.contains ext (Globalloader.ignoredFileTypes @ Globalloader.contentFileTypes) ||
        page.Contains "_public" ||
        page.Contains "_bin" ||
        page.Contains "_lib" ||
        page.Contains "obj" ||
        page.Contains "_data" ||
        page.Contains "_settings" ||
        page.Contains "_config.yml" ||
        page.Contains ".sass-cache" ||
        page.Contains ".git" ||
        page.Contains ".ionide" ||
        page.Contains ".vscode" ||
        page.Contains "build"
    fileShouldBeExcluded |> not


let config = {
    Generators = [
        // {Script = "less.fsx"; Trigger = OnFileExt ".less"; OutputFile = ChangeExtension "css" }
        // {Script = "sass.fsx"; Trigger = OnFileExt ".scss"; OutputFile = ChangeExtension "css" }
        {Script = "post.fsx"; Trigger = OnFilePredicate postPredicate; OutputFile = ChangeExtension "html" }
        {Script = "staticfile.fsx"; Trigger = OnFilePredicate staticPredicate; OutputFile = SameFileName }
        {Script = "index.fsx"; Trigger = Once; OutputFile = MultipleFiles id }
        {Script = "about.fsx"; Trigger = Once; OutputFile = NewFileName "about.html" }
        {Script = "contact.fsx"; Trigger = Once; OutputFile = NewFileName "contact.html" }
    ]
}
