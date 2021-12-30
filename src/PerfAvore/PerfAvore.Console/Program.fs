open Spectre.Console

open System
open System.IO
open CommandLine
open Argu

open TraceSession

open RulesEngine.Parser
open RulesEngine.Domain
open RulesEngine.ActionEngine
open Microsoft.Diagnostics.Tracing
open System.Runtime.InteropServices

open JsonRuleFileReader

[<EntryPoint>]
let main argv =
    AnsiConsole.MarkupLine("[underline green]Perf-Avore, The Rule Based Performance Analysis![/] ");

    let errorHandler      = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser            = ArgumentParser.Create<Arguments>(errorHandler = errorHandler)
    let parsedCommandline = parser.Parse(inputs = argv, raiseOnUsage = true)

    // Process Name is mandatory.
    let processName = parsedCommandline.GetResult ProcessName

    let parsedRules : Rule list = 
        // Rules are needed, if not provided, fall back to the default rules.
        let jsonFileSupplied : bool = parsedCommandline.Contains RulesPath
        if jsonFileSupplied then 
            let jsonFile = parsedCommandline.GetResult RulesPath
            getJsonRulesFromFile jsonFile 
            |> List.map(parseRule)
        else
            // Fall back to the Sample Rules.
            if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
                getJsonRulesFromFile (Path.Combine( __SOURCE_DIRECTORY__, "SampleRules", "SampleRules.json"))
                |> List.map(parseRule)
            else
                getJsonRulesFromFile (Path.Combine( __SOURCE_DIRECTORY__, "SampleRules", "LinuxSampleRules.json"))
                |> List.map(parseRule)

    let containsTracePath : bool = parsedCommandline.Contains TracePath

    // If the trace log file is provided, use the Trace Log API to traverse through all events.
    if containsTracePath then 
        let tracePathArgs = parsedCommandline.GetResult TracePath
        let traceLog = getTraceLogFromTracePath tracePathArgs
        let events = traceLog.Events 
        let eventNamesToFilter = parsedRules |> List.map(fun r -> r.Condition.Conditioner.ConditionerEvent.ToString())

        let applyRulesForAllEvents (events : TraceEvent seq) (rules : Rule list) = 
            events
            // Consider events with name of the process and if they contain the events defined in the rules.
            |> Seq.filter(fun e -> e.ProcessName = processName && 
                                   eventNamesToFilter |> List.contains(e.EventName))
            |> Seq.iter(fun e -> 
                rules
                |> List.iter(fun rule -> applyRule rule e ))
        applyRulesForAllEvents events parsedRules

    // Else, start a Real Time Session.
    // Requires admin privileges
    else
        let traceLogEventSource, session = getRealTimeSession processName parsedRules
        Console.CancelKeyPress.Add(fun _ -> session.Dispose() |> ignore )
        traceLogEventSource.Process() |> ignore
        ()

    0