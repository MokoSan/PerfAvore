module RulesEngine.DSL

open System

// A rule consists of a Condition and an Action.
// If the condition is met, the said action will be invoked.
// Rule = Condition + Action. For example: "GCEnd.PauseTimeMSec >= 300 : Print Alert"

// Condition: A condition consists of a Conditioner, A Condition Type and a Condition Value.

type Condition = 
    {  Conditioner      : Conditioner;
       ConditionType    : ConditionType;
       ConditionalValue : ConditionalValue }
and Conditioner = 
    { ConditionerEvent    : ConditionerEvent; 
      ConditionerProperty : ConditionerProperty }
and ConditionType = 
    | LessThan
    | LessThanEqualTo
    | GreaterThan
    | GreaterThanEqualTo
    | Equal
    | NotEqual
    | IsAnomaly
and ConditionalValue =
    | Value of double
    | AnomalyDetectionType of AnomalyDetectionType 
and ConditionerEvent    = string
and ConditionerProperty = string
and AnomalyDetectionType =
    | DetectIIDSpike

// Action: An action is invoked if a condition is met and consists of an operator and an operand.
type Action = 
    { ActionOperator: ActionOperator; ActionOperand: ActionOperand }
and ActionOperator = 
    |  Print
and ActionOperand =
    | Alert
    | CallStack

// Rule: A rule consists of a condition and an action.
type Rule = 
    { Id           : Guid
      Condition    : Condition
      Action       : Action 
      OriginalRule : string }

// Invoked Actions: Once an action is invoked, store the context and the result.
type InvokedActionContext = 
    { Timestamp     : double
      ProcessName   : string
      EventName     : string
      EventProperty : string
      RuleInvoked   : Rule
      Reason        : double }

type InvokedActionResult =
    { Context : InvokedActionContext; Result  : string }
type InvokedActionResults = InvokedActionResult seq