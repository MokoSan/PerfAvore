module RulesEngine.ActionEngine

open Microsoft.Diagnostics.Tracing
open Microsoft.Diagnostics.Tracing.Etlx

open System
open System.Linq

open RulesEngine.Domain
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
            let conditionalValue : ConditionalValue = rule.Condition.ConditionalValue
            let payload          : double = Double.Parse (traceEvent.PayloadByName(condition.Conditioner.ConditionerProperty).ToString())

            match conditionalValue with
            | ConditionalValue.Value value ->
                match condition.ConditionType with
                | ConditionType.Equal              -> payload = value
                | ConditionType.GreaterThan        -> payload > value
                | ConditionType.GreaterThanEqualTo -> payload >= value
                | ConditionType.LessThan           -> payload < value
                | ConditionType.LessThanEqualTo    -> payload <= value
                | ConditionType.NotEqual           -> payload <> value
                | ConditionType.IsAnomaly          -> false // This case should technically not be reached but adding it to prevent warnings.
            | ConditionalValue.AnomalyDetectionType anomalyDetectionType ->
                match anomalyDetectionType with
                | AnomalyDetectionType.DetectIIDSpike ->
                    false // TODO: Fill This.

        // Match on Event Name, if the payload exists and the condition based on the trace event is met.
        matchEventName rule traceEvent && checkPayload rule traceEvent && checkConditionValue rule traceEvent

    let apply (action : Action) : unit = 

        // TODO: Store the Invoked Action Result.
        let invokedActionContext : InvokedActionContext = 
            { Timestamp     = traceEvent.TimeStampRelativeMSec 
              ProcessName   = traceEvent.ProcessName 
              EventName     = traceEvent.EventName 
              EventProperty = rule.Condition.Conditioner.ConditionerProperty
              RuleInvoked   = rule
              Reason        = Double.Parse(traceEvent.PayloadByName(rule.Condition.Conditioner.ConditionerProperty).ToString()) }

        match action.ActionOperator with
        | ActionOperator.Print ->
            match action.ActionOperand with
            | ActionOperand.Alert     -> printAlert rule traceEvent
            | ActionOperand.CallStack -> printCallStack rule traceEvent
    
    if checkCondition = true then apply rule.Action
    else ()