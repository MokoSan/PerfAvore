module TraceSession

open System
open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Tracing.Session
open Microsoft.Diagnostics.Tracing.Parsers
open Microsoft.Diagnostics.Tracing

open RulesEngine.Domain
open RulesEngine.ActionEngine

let getTraceLogFromTracePath (tracePath : string) : TraceLog = 
    TraceLog.OpenOrConvert tracePath

let getRealTimeSession (processName : string) (parsedRules : Rule list) : TraceLogEventSource * TraceEventSession = 
    // Onus on the caller to dispose.
    let traceEventSession : TraceEventSession = new TraceEventSession($"Session_{Guid.NewGuid()}");

    let keywords : uint64 = uint64(ClrTraceEventParser.Keywords.All) 
    traceEventSession.EnableKernelProvider(KernelTraceEventParser.Keywords.All, KernelTraceEventParser.Keywords.None) |> ignore
    traceEventSession.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose, keywords)             |> ignore

    // Once the pertinent providers are enabled, create the trace log event source. 
    let traceLogEventSource = TraceLog.CreateFromTraceEventSession traceEventSession

    // Add all the necessary callbacks.
    let callbackForAllEvents : Action<TraceEvent> = 
        Action<TraceEvent>(fun traceEvent -> 
            parsedRules
            |> List.iter(fun rule -> applyRule rule traceEvent))

    traceLogEventSource.Clr.add_All(callbackForAllEvents)    |> ignore
    traceLogEventSource.Kernel.add_All(callbackForAllEvents) |> ignore

    // TODO: Enable the GLAD events - only available for real time processing.
    // ala: https://devblogs.microsoft.com/dotnet/556-2/
    traceLogEventSource, traceEventSession