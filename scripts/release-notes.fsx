open System
open System.IO
open System.Text.RegularExpressions

let noteText = @"release/notes.txt"
let changeLog = Path.Combine(__SOURCE_DIRECTORY__, "..", "CHANGELOG.md")
let isEmptyLine (line: string) = String.IsNullOrEmpty(line.Trim())
let matchHeading line = Regex.IsMatch(line, @"^\#{2,}.*\d+\.")

try
    let changes = File.ReadAllLines(changeLog) |> Array.filter (not << isEmptyLine)
    let latest = changes |> Array.takeWhile matchHeading

    let notes =
        Array.except latest changes
        |> Array.takeWhile (not << matchHeading)
        |> String.concat "!NL!"

    FileInfo(noteText).Directory.Create()
    File.WriteAllText(noteText, notes)

    printfn "---\n%s\n---" <| (File.ReadAllText(noteText)).Replace("!NL!", Environment.NewLine)
with exc -> printfn "%s" <| (sprintf "%s: %s" <| exc.GetType().Name <| exc.Message)
