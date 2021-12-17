module RulesEngine.ActionEngine

open Microsoft.Diagnostics.Tracing
open Microsoft.Diagnostics.Tracing.Analysis
open Microsoft.Diagnostics.Tracing.Etlx
open Microsoft.Diagnostics.Tracing.Session
open Microsoft.Diagnostics.Tracing.Parsers.Clr
open Microsoft.Diagnostics.Tracing.Analysis.GC

open System
open System.Linq

open RulesEngine.DSL
open RulesEngine.Actions.CallStack
open RulesEngine.Actions.Alert

let applyRule (rule : Rule) (traceEvent : TraceEvent) : unit =

    // Helper fn checks if the condition is met for the traceEvent.
    let checkCondition : bool =
        let condition : Condition = rule.Condition

        // Match the event name.
        let matchEventName (rule : Rule) (traceEvent : TraceEvent) : bool = 
            traceEvent.EventName = condition.Conditioner.ConditionerEvent
        
        // Check if the specified payload exists.
        let checkPayload (rule : Rule) (traceEvent : TraceEvent) : bool = 
            if traceEvent.PayloadNames.Contains condition.Conditioner.ConditionerProperty then true
            else false

        // Check if the condition matches.
        let checkConditionValue (rule : Rule) (traceEvent : TraceEvent) : bool =
            let payload : double   = Double.Parse(traceEvent.PayloadByName(condition.Conditioner.ConditionerProperty).ToString())
            let conditionalValue   : ConditionalValue = rule.Condition.ConditionalValue

            match conditionalValue with
            | ConditionalValue.Value value ->
                match condition.ConditionType with
                | ConditionType.Equal              -> payload = value
                | ConditionType.GreaterThan        -> payload > value
                | ConditionType.GreaterThanEqualTo -> payload >= value
                | ConditionType.LessThan           -> payload < value
                | ConditionType.LessThanEqualTo    -> payload <= value
                | ConditionType.NotEqual           -> payload <> value
            | ConditionalValue.AnomalyDetectionType anomalyDetectionType ->
                match anomalyDetectionType with
                | AnomalyDetectionType.DetectIIDSpike ->
                    false // TODO: Fill This.

        // Match on Event Name, if the payload exists and the condition based on the trace event is met.
        matchEventName rule traceEvent && checkPayload rule traceEvent && checkConditionValue rule traceEvent

    let apply (action : Action) : unit = 

        match action.ActionOperator with
        | ActionOperator.Print ->
            match action.ActionOperand with
            | ActionOperand.Alert     -> printAlert rule traceEvent
            | ActionOperand.CallStack -> printCallStack (traceEvent.CallStack())
    
    if checkCondition = true then apply rule.Action
    else ()