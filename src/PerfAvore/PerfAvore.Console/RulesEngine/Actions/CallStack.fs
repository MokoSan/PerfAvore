module RulesEngine.Actions.CallStack

open System.IO

open Microsoft.Diagnostics.Tracing
open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Symbols

open RulesEngine.Domain
open Spectre.Console

let symbolReader : SymbolReader = new SymbolReader(TextWriter.Null, SymbolPath.SymbolPathFromEnvironment)

// Helper fn responsible for getting the call stack from a particular trace event.
let printCallStack (rule: RulesEngine.Domain.Rule) (traceEvent : TraceEvent) : unit =

    let callStack = traceEvent.CallStack()
    if isNull callStack then 
        printfn $"Rule: {rule.InputRule} invoked for Event: {traceEvent} however, the call stack associated with the event is null." 
        ()

    let root = Tree(Rule(rule.InputRule.EscapeMarkup()))

    let printStackFrame (callStack : TraceCallStack) : unit =
        if not (isNull callStack.CodeAddress.ModuleFile)
        then
            callStack.CodeAddress.CodeAddresses.LookupSymbolsForModule(symbolReader, callStack.CodeAddress.ModuleFile)
            let frameValue = sprintf "%s!%s" callStack.CodeAddress.ModuleName callStack.CodeAddress.FullMethodName
            root.AddNode ( frameValue.EscapeMarkup() ) |> ignore

    let rec processFrame (callStack : TraceCallStack) : unit =
        if isNull callStack then ()
        else
            printStackFrame callStack
            processFrame callStack.Caller
    
    processFrame callStack
    AnsiConsole.Write root
    printfn "\n"