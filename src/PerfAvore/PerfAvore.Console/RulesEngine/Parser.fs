module RulesEngine.Parser

open System
open RulesEngine.Domain

let parseCondition (conditionAsString : string) : Condition = 

    let splitCondition : string[] = conditionAsString.Split(" ", StringSplitOptions.RemoveEmptyEntries)
    
    // Precondition check
    if splitCondition.Length <> 3
    then invalidArg (nameof conditionAsString) ("Incorrect format of the condition. Format is: Event.Property Condition ConditionalValue. For example: GCEnd.SuspensionTimeMSec >= 298")
    
    // Condition Event and Property
    let parseConditioner : Conditioner = 
        let splitConditioner : string[] = splitCondition.[0].Split(".", StringSplitOptions.RemoveEmptyEntries)

        // Precondition checks.
        if splitConditioner.Length < 2 
        then invalidArg (nameof conditionAsString) ("Incorrect format of the conditioner. Format is Event.Property.")

        let parseConditionEvent : ConditionerEvent = splitConditioner.[0]
        let parseConditionProperty : ConditionerProperty = splitConditioner.[1]

        { ConditionerEvent = parseConditionEvent; ConditionerProperty = parseConditionProperty }

    // Condition Type
    let parseConditionType : ConditionType =
        match splitCondition.[1].ToLower() with
        | ">"  | "greaterthan"                                 -> ConditionType.GreaterThan 
        | "<"  | "lessthan"                                    -> ConditionType.LessThan
        | ">=" | "greaterthanequalto" | "greaterthanorequalto" -> ConditionType.GreaterThanEqualTo
        | "<=" | "lessthanequalto"    | "lessthanorequalto"    -> ConditionType.LessThanEqualTo
        | "="  | "equal"              | "equals"               -> ConditionType.Equal
        | "!=" | "notequal"                                    -> ConditionType.NotEqual
        | "isanomaly"                                          -> ConditionType.IsAnomaly
        | _                                                    -> invalidArg (nameof splitCondition) ("${splitCondition.[1]} is an unrecognized condition type.")

    // Condition Value
    let parseConditionValue : ConditionalValue =
        let conditionalValueAsString = splitCondition.[2].ToLower()
        let checkDouble, doubleValue = Double.TryParse conditionalValueAsString 
        match checkDouble, doubleValue with
        | true, v -> ConditionalValue.Value(v)
        | false, _ -> 
            match conditionalValueAsString with
            | "detectiidspike" -> ConditionalValue.AnomalyDetectionType(AnomalyDetectionType.DetectIIDSpike)
            | _                -> invalidArg (nameof splitCondition) ($"{conditionalValueAsString} is an unrecognized anomaly detection type.")
        
    { Conditioner = parseConditioner; ConditionType = parseConditionType; ConditionalValue = parseConditionValue }

let parseAction (actionAsAString : string) : Action = 
    let splitAction : string[] = actionAsAString.Split(" ", StringSplitOptions.RemoveEmptyEntries)

    if splitAction.Length < 2 
    then invalidArg (nameof actionAsAString) ($"{actionAsAString} is an invalid Action.")

    // ActionOperator
    let parseActionOperator : ActionOperator = 
        match splitAction.[0].ToLower() with
        | "print" -> ActionOperator.Print
        | _       -> invalidArg (nameof splitAction) ($"{splitAction.[0]} is an unrecognized Action Operator.")

    // ActionOperand 
    let parseActionOperand : ActionOperand = 
        match splitAction.[1].ToLower() with
        | "alert"     -> ActionOperand.Alert
        | "callstack" -> ActionOperand.CallStack
        | "chart"     -> ActionOperand.Chart
        | _           -> invalidArg (nameof splitAction) ($"{splitAction.[1]} is an unrecognized Action Operand.")

    { ActionOperator = parseActionOperator; ActionOperand = parseActionOperand }

let parseRule (ruleAsString : string) : Rule = 
    let splitRuleAsAString : string[] = ruleAsString.Split(":")
    let condition : Condition = parseCondition splitRuleAsAString.[0]
    let action : Action = parseAction splitRuleAsAString.[1]
    { Condition = condition; Action = action; InputRule = ruleAsString; Id = Guid.NewGuid() }