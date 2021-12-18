module RulesEngine.Actions.Alert

open RulesEngine.Domain

open Microsoft.Diagnostics.Tracing

let printAlert (rule : Rule) (traceEvent : TraceEvent) : unit = 
    printfn $"Alert! Rule: {rule.InputRule} invoked for Event: {traceEvent} with payload: {traceEvent.PayloadByName(rule.Condition.Conditioner.ConditionerProperty).ToString()}."