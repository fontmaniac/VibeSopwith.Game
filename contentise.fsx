#!/usr/bin/env -S dotnet fsi

open System
open System.IO
open System.Diagnostics

// -----------------------------
// Command‑line argument parsing
// -----------------------------

type Flags =
    { From : string list
      To : string option
      Manifest : string option
      DeleteFrom : bool }

let empty =
    { From = []
      To = None
      Manifest = None
      DeleteFrom = false }

// -----------------------------
// Command-line argument parsing
// -----------------------------

type Args =
    { From : string list
      To  : string
      Manifest : string
      DeleteFrom : bool }

let rec parseOptionsRec (state: Flags) (args: string list) =
    match args with
    | [] -> state
    | "-from" :: value :: rest ->
        parseOptionsRec { state with From = value :: state.From } rest
    | "-to" :: value :: rest ->
        parseOptionsRec { state with To = Some value } rest
    | "-manifest" :: value :: rest ->
        parseOptionsRec { state with Manifest = Some value } rest
    | "-delete" :: rest ->
        parseOptionsRec { state with DeleteFrom = true } rest
    | unknown :: rest ->
        printfn "Unknown argument: %s" unknown
        parseOptionsRec state rest

let parseArgs (argv: string[]) =
    // Parse flags
    let flags = parseOptionsRec empty (argv |> Array.toList)

    { Args.From = flags.From 
      Args.To = flags.To |> Option.get 
      Args.Manifest = flags.Manifest |> Option.get
      Args.DeleteFrom = flags.DeleteFrom }

type CopyManifest = {
    FromTos : List<string * string>
}

type ResultManifest = {
    Files : List<string>
}

let makeCopyManifest args =
    let fromTos = ResizeArray<string * string>()

    for fromDir in args.From do
        let fullFrom = Path.GetFullPath(fromDir)
        let fullTo   = Path.GetFullPath(args.To)

        if not (Directory.Exists(fullFrom)) then
            failwithf "Source folder does not exist: %s" fullFrom

        // Walk recursively
        for file in Directory.EnumerateFiles(fullFrom, "*", SearchOption.AllDirectories) do
            let rel = Path.GetRelativePath(fullFrom, file)
            let dest = Path.Combine(fullTo, rel)

            fromTos.Add(file, dest)

    { FromTos = List.ofSeq fromTos }

let makeResultManifest args (copyManifest: CopyManifest) =
    let files = ResizeArray<string>()

    let fullTo = Path.GetFullPath(args.To)
    let toName = Path.GetFileName(fullTo)   // e.g. "Content" or "assets"

    for (_, dst) in copyManifest.FromTos do
        let rel = Path.GetRelativePath(fullTo, dst).Replace("\\", "/")
        let vfsPath = "/" + toName + "/" + rel
        files.Add(vfsPath)

    { Files = List.ofSeq files }

let copyFiles args (copyManifest: CopyManifest) =
    for (src, dst) in copyManifest.FromTos do
        let dstDir = Path.GetDirectoryName(dst)
        Directory.CreateDirectory(dstDir) |> ignore
        File.Copy(src, dst, true)

    makeResultManifest args copyManifest

let deleteSources args =
    for fromDir in args.From do
        let full = Path.GetFullPath(fromDir)
        if Directory.Exists(full) then
            Directory.Delete(full, true)


let writeManifest args resultManifest =
    // Need to make sure fileName is appropriately qualified.
    let filePath = args.Manifest
    File.WriteAllLines(filePath, resultManifest.Files);

// -----------------------------
// Main entry point
// -----------------------------

// Usage: dotnet fsi contentise.fsx -from "Content" -to "wwwroot/Content" -manifest "wwwroot/content.txt" -delete

printfn "contentise.fsx run with args %A" fsi.CommandLineArgs

let args = parseArgs fsi.CommandLineArgs.[1..]

printfn "contentise.fsx parsed args %A" args

let copyManifest = makeCopyManifest args
let resultManifest = copyFiles args copyManifest
if args.DeleteFrom then deleteSources args
writeManifest args resultManifest
