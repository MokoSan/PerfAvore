module RulesEngine.Actions.Alert

open RulesEngine.DSL

open System.Diagnostics
open Microsoft.Diagnostics.Tracing
open Microsoft.Diagnostics.Tracing.Analysis
open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Tracing.Session
open Microsoft.Diagnostics.Tracing.Parsers.Clr
open Microsoft.Diagnostics.Symbols

let printAlert (rule : Rule) (traceEvent : TraceEvent) : unit = 
    printfn $"Alert!! {rule.OriginalRule} invoked as payload: {traceEvent.PayloadByName(rule.Condition.Conditioner.ConditionerProperty).ToString()}!"