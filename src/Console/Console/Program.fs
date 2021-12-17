open Spectre.Console

open System
open CommandLine
open Argu

[<EntryPoint>]
let main argv =
    AnsiConsole.MarkupLine("[underline green]Welcome to MokoSan's FSharp Advent 2021![/] ");

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)

    let parser      = ArgumentParser.Create<Arguments>(errorHandler = errorHandler)
    let parsedArgs  = parser.Parse(inputs = argv, raiseOnUsage = true)
    let parsedArray = parsedArgs.GetAllResults()

    // TODO: Split paths between the Trace Log vs. Real Time.
    // Parse the results.
    let tracePath = parsedArgs.GetResult TracePath
    printfn "%A" tracePath

    0