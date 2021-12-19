module RulesEngine.ActionEngine

open Microsoft.Diagnostics.Tracing

open System
open System.Linq

open RulesEngine.Domain
open RulesEngine.Actions.CallStack
open RulesEngine.Actions.Alert
open RulesEngine.Actions.Chart
open AnomalyDetection.Service

open AnomalyDetection.IIDSpike

let anomalyDetectionContextService : AnomalyDetectionContextService = 
    AnomalyDetectionContextService(AnomalyDetectionContextService.AnomalyPValueHistoryLength)

let applyRule (rule : Rule) (traceEvent : TraceEvent) : unit =

    // Helper fn checks if the condition is met for the traceEvent.
    let checkCondition : bool =
        let condition : Condition = rule.Condition

        // Match the event name.
        let matchEventName (traceEvent : TraceEvent) : bool = 
            traceEvent.EventName = condition.Conditioner.ConditionerEvent
        
        // Check if the specified payload exists.
        let checkPayload (traceEvent : TraceEvent) : bool = 
            if traceEvent.PayloadNames.Contains condition.Conditioner.ConditionerProperty then true
            else false

        // Early return if the payload is unavailable since it will except later if we let it slide. 
        if ( checkPayload traceEvent ) = false then 
            false
        else
            let payload : double = Double.Parse (traceEvent.PayloadByName(condition.Conditioner.ConditionerProperty).ToString())

            // Add the new data point to the anomaly detection dict.
            let anomalyDetectionInput : AnomalyDetectionInput = 
                AnomalyDetectionInput(timestamp = traceEvent.TimeStampRelativeMSec, value = float32(payload))
            anomalyDetectionContextService.Upsert rule.Id anomalyDetectionInput |> ignore

            // Check if the condition matches.
            let checkConditionValue (rule : Rule) (traceEvent : TraceEvent) : bool =
                let conditionalValue : ConditionalValue = rule.Condition.ConditionalValue

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
                        let context = { Rule = rule; Input = anomalyDetectionInput }
                        let result  = getAnomaliesUsingIIDSpikeEstimation context anomalyDetectionContextService 
                        result.IsAnomaly

            // Match on Event Name, if the payload exists and the condition based on the trace event is met.
            matchEventName traceEvent && checkPayload traceEvent && checkConditionValue rule traceEvent

    let apply (action : Action) : unit = 

        // TODO: Store the Invoked Action Result.
        (*
        let invokedActionContext : InvokedActionContext = 
            { Timestamp     = traceEvent.TimeStampRelativeMSec 
              ProcessName   = traceEvent.ProcessName 
              EventName     = traceEvent.EventName 
              EventProperty = rule.Condition.Conditioner.ConditionerProperty
              RuleInvoked   = rule
              Reason        = Double.Parse(traceEvent.PayloadByName(rule.Condition.Conditioner.ConditionerProperty).ToString()) }
        *)

        match action.ActionOperator with
        | ActionOperator.Print ->
            match action.ActionOperand with
            | ActionOperand.Alert     -> printAlert rule traceEvent
            | ActionOperand.CallStack -> printCallStack rule traceEvent
            | ActionOperand.Chart     -> printChart rule anomalyDetectionContextService 
    
    if checkCondition = true then apply rule.Action
    else ()