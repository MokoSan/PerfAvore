module RulesEngine.Actions.CallStack

open System.IO
open System.Diagnostics

open Microsoft.Diagnostics.Tracing
open Microsoft.Diagnostics.Tracing.Analysis
open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Tracing.Session
open Microsoft.Diagnostics.Tracing.Parsers.Clr
open Microsoft.Diagnostics.Symbols

// Helper fn responsible for getting the call stack from a particular trace event.
let printCallStack (callStack : TraceCallStack) : unit =
    use symbolReader = new SymbolReader(TextWriter.Null, SymbolPath.SymbolPathFromEnvironment)

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