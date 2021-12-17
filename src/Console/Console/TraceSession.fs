module TraceSession

open System
open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Tracing.Session

let getTraceLogFromTracePath (tracePath : string) : TraceLog = 
    use session = new TraceEventSession($"RuleSession_{Guid.NewGuid()}", tracePath)
    TraceLog.OpenOrConvert(tracePath)