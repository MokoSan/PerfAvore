open Spectre.Console

open System
open CommandLine
open Argu

open TraceSession
open RulesEngine.Parser
open RulesEngine.Domain
open RulesEngine.ActionEngine
open Microsoft.Diagnostics.Tracing
open Microsoft.Diagnostics.Tracing.Session

[<EntryPoint>]
let main argv =
    AnsiConsole.MarkupLine("[underline green]Rule Based Performance Analysis: MokoSan's 2021 F# Advent Submission![/] ");

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)

    let parser      = ArgumentParser.Create<Arguments>(errorHandler = errorHandler)
    let parsedArgs  = parser.Parse(inputs = argv, raiseOnUsage = true)

    // Get Command Line Args 
    let tracePathArgs = parsedArgs.GetResult TracePath
    let processName   = parsedArgs.GetResult ProcessName

    let rules = 
        [ "GC/AllocationTick.AllocationAmount > 108000: Print CallStack"; 
          "GC/AllocationTick.AllocationAmount > 200000: Print CallStack"; 
          "ThreadPoolWorkerThreadAdjustment/Stats.Throughput < 4: Print CallStack"; ]

    let parsedRules = 
        rules
        |> List.map(parseRule)

    let traceLog = getTraceLogFromTracePath tracePathArgs

    use traceEventSession = new TraceEventSession($"GCRealMonSession_{Guid.NewGuid()}");
    let action = Action<TraceEvent>(fun e -> printfn "%A" e)
    traceEventSession.Source.add_AllEvents (action)
    let thisHasSideEffects = traceEventSession.Source.Process() 


    let events = traceLog.Events 
    let applyRule (events : TraceEvent seq) (rule : Rule) = 
        events
        |> Seq.filter(fun e -> e.ProcessName = processName)
        |> Seq.filter(fun e -> e.EventName = rule.Condition.Conditioner.ConditionerEvent)
        |> Seq.iter(fun e -> applyRule rule e)

    parsedRules
    |> List.map(fun e -> applyRule events e)
    |> ignore

    0