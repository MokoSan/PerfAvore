open Spectre.Console

open System
open CommandLine
open Argu

open TraceSession

open RulesEngine.Parser
open RulesEngine.Domain
open RulesEngine.ActionEngine
open Microsoft.Diagnostics.Tracing

[<EntryPoint>]
let main argv =
    AnsiConsole.MarkupLine("[underline green]Rule Based Performance Analysis: MokoSan's 2021 F# Advent Submission![/] ");

    let errorHandler      = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser            = ArgumentParser.Create<Arguments>(errorHandler = errorHandler)
    let parsedCommandline = parser.Parse(inputs = argv, raiseOnUsage = true)

    // Process Name is mandatory.
    let processName       = parsedCommandline.GetResult ProcessName

    // TODO: Move to a JSON File.
    let parsedRules : Rule list =
        [ 
          "GC/AllocationTick.AllocationAmount > 108000: Print Alert"; 
          "GC/AllocationTick.AllocationAmount > 200000: Print CallStack"; 
          //"GC/AllocationTick.AllocationAmount isAnomaly DetectIIDSpike : Print Chart"; 
          //"ThreadPoolWorkerThreadAdjustment/Stats.Throughput < 4: Print CallStack"; 
        ]
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
            |> Seq.filter(fun e -> e.ProcessName = processName                      && 
                                   eventNamesToFilter |> List.contains(e.EventName))
            |> Seq.iter(fun e -> 
                rules
                |> List.iter(fun rule -> applyRule rule e ))
        applyRulesForAllEvents events parsedRules

    // Else, start a Real Time Session.
    // Requires admin privileges
    else
        let traceLogEventSource, session = getRealTimeSession
        let callbackForAllEvents : Action<TraceEvent> = 
            Action<TraceEvent>(fun traceEvent -> 
                parsedRules
                |> List.iter(fun rule -> applyRule rule traceEvent))

        traceLogEventSource.Clr.add_All(callbackForAllEvents)    |> ignore
        traceLogEventSource.Kernel.add_All(callbackForAllEvents) |> ignore

        Console.CancelKeyPress.Add(fun e -> session.Dispose() |> ignore )

        traceLogEventSource.Process() |> ignore
        ()

    0