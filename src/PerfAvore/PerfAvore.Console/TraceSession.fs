module TraceSession

open System
open System.Collections.Generic
open System.Runtime.InteropServices

open Microsoft.Diagnostics.NETCore.Client
open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Tracing.Session
open Microsoft.Diagnostics.Tracing.Parsers
open Microsoft.Diagnostics.Tracing

open RulesEngine.Domain
open RulesEngine.ActionEngine
open System.Diagnostics

let getProcessIdForProcessName (processName : string) : int =
        let processes = Process.GetProcessesByName(processName)
        if processes.Length < 1 then invalidArg processName $"No processes with name: {processName} exists."
        // For the sake of simplicity, choose the first process available with the said name. 
        else processes.[0].Id

let getTraceLogFromTracePath (tracePath : string) : TraceLog = 
    TraceLog.OpenOrConvert tracePath

let getRealTimeSession (processName : string) (parsedRules : Rule list) : TraceEventDispatcher * IDisposable = 

    let callbackForAllEvents : Action<TraceEvent> = 
        Action<TraceEvent>(fun traceEvent -> 
            parsedRules
            |> List.iter(fun rule ->
                if processName = traceEvent.ProcessName then applyRule rule traceEvent))

    // Windows.
    if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
        let traceEventSession : TraceEventSession = new TraceEventSession($"Session_{Guid.NewGuid()}");

        let keywords : uint64 = uint64(ClrTraceEventParser.Keywords.All) 

        traceEventSession.EnableKernelProvider(KernelTraceEventParser.Keywords.All, KernelTraceEventParser.Keywords.None) |> ignore
        traceEventSession.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose, keywords)             |> ignore

        // Once the pertinent providers are enabled, create the trace log event source. 
        let traceLogEventSource = TraceLog.CreateFromTraceEventSession traceEventSession

        // Add all the necessary callbacks.
        traceLogEventSource.Clr.add_All(callbackForAllEvents)    |> ignore
        traceLogEventSource.Kernel.add_All(callbackForAllEvents) |> ignore

        // TODO: Enable the GLAD events - only available for real time processing.
        // ala: https://devblogs.microsoft.com/dotnet/556-2/
        traceLogEventSource, traceEventSession

    // Linux / MacOS.
    else
        let keywords : int64 = int64(ClrTraceEventParser.Keywords.All) 
        let eventPipeProvider : EventPipeProvider = 
            EventPipeProvider("Microsoft-Windows-DotNETRuntime", Tracing.EventLevel.Informational, keywords)
        let providers = List<EventPipeProvider>()
        providers.Add eventPipeProvider

        // For the sake of simplicity, choose the first process available with the said name. 
        let processId        = getProcessIdForProcessName processName
        let client           = DiagnosticsClient(processId)
        let eventPipeSession = client.StartEventPipeSession(providers, false)
        let source           = new EventPipeEventSource(eventPipeSession.EventStream)

        source.Clr.add_All(callbackForAllEvents)    |> ignore
        source.Kernel.add_All(callbackForAllEvents) |> ignore

        source, eventPipeSession