module TraceSession

open System
open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Tracing.Session
open Microsoft.Diagnostics.Tracing.Parsers
open Microsoft.Diagnostics.Tracing

let getTraceLogFromTracePath (tracePath : string) : TraceLog = 
    TraceLog.OpenOrConvert tracePath

let getRealTimeSession : TraceEventSession = 
    let traceEventSession : TraceEventSession = new TraceEventSession($"Session_{Guid.NewGuid()}");
    let keywords : uint64 = uint64(ClrTraceEventParser.Keywords.GC ||| ClrTraceEventParser.Keywords.Threading ||| ClrTraceEventParser.Keywords.All ) 
    traceEventSession.EnableKernelProvider(KernelTraceEventParser.Keywords.All, KernelTraceEventParser.Keywords.None) |> ignore
    traceEventSession.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose, keywords) |> ignore
    traceEventSession