module RulesEngine.Actions.CallStack

open System.IO

open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Symbols

let symbolReader : SymbolReader = new SymbolReader(TextWriter.Null, SymbolPath.SymbolPathFromEnvironment)

// Helper fn responsible for getting the call stack from a particular trace event.
let printCallStack (callStack : TraceCallStack) : unit =

    let printStackFrame (callStack : TraceCallStack) : unit =
        if not (isNull (callStack.CodeAddress.ModuleFile))
        then
            callStack.CodeAddress.CodeAddresses.LookupSymbolsForModule(symbolReader, callStack.CodeAddress.ModuleFile)
            printfn "%s!%s" callStack.CodeAddress.ModuleName callStack.CodeAddress.FullMethodName

    let rec processFrame (callStack : TraceCallStack) : unit =
        if isNull callStack then ()
        else
            printStackFrame callStack
            processFrame callStack.Caller
    
    processFrame callStack