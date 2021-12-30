module PerfAvore.UnitTests

open RulesEngine.Parser
open RulesEngine.Domain
open System
open FluentAssertions

open NUnit.Framework

// parseCondition.

[<Test>]
let ``parseCondition with Incorrect Format Without Conditions results in ArgumentException``() =
    let incorrectFormat = "GC/AllocationTick.AllocationAmount"
    let parseIncorrect = fun () -> parseCondition incorrectFormat
    parseIncorrect.Invoking(fun y -> y()).Should().Throw<ArgumentException>("Incorrect format") |> ignore

[<Test>]
let ``parseCondition with Incorrect Format Without Proper Event Property results in ArgumentException``() =
    let incorrectFormat = "GC/AllocationTickAllocationAmount >= 4"
    let parseIncorrect = fun () -> parseCondition incorrectFormat
    parseIncorrect.Invoking(fun y -> y()).Should().Throw<ArgumentException>("Incorrect format") |> ignore

[<Test>]
let ``parseCondition with Incorrect Format Without Condition results in ArgumentException``() =
    let incorrectFormat = "GC/AllocationTick.AllocationAmount 4"
    let parseIncorrect = fun () -> parseCondition incorrectFormat
    parseIncorrect.Invoking(fun y -> y()).Should().Throw<ArgumentException>("Incorrect format") |> ignore

[<Test>]
let ``parseCondition with Incorrect Format For Anomaly Detection``() =
    let incorrectFormat = "GC/AllocationTick.AllocationAmount isAnomaly someUnregisteredMethod"
    let parseIncorrect = fun () -> parseCondition incorrectFormat
    parseIncorrect.Invoking(fun y -> y()).Should().Throw<ArgumentException>("Incorrect format") |> ignore

[<Test>]
let ``parseCondition with Correct Format results in Correct Parsing Without Anomaly Detection``() =
    let correctFormat = "GC/AllocationTick.AllocationAmount >= 4"
    let parse = parseCondition correctFormat 
    let parsedCondition = 
        {  Conditioner      = 
            { ConditionerEvent    = "GC/AllocationTick"
              ConditionerProperty = "AllocationAmount" }
           ConditionType    = ConditionType.GreaterThanEqualTo
           ConditionalValue = ConditionalValue.Value 4.0 } 
    parse.Should().BeEquivalentTo(parsedCondition, "Equality of value", null) |> ignore

[<Test>]
let ``parseCondition with Correct Format results in Correct Parsing With Anomaly Detection``() =
    let correctFormat = "GC/AllocationTick.AllocationAmount isAnomaly DetectIIDSpike"
    let parse = parseCondition correctFormat 
    let parsedCondition = 
        {  Conditioner      = 
            { ConditionerEvent    = "GC/AllocationTick"
              ConditionerProperty = "AllocationAmount" }
           ConditionType    = ConditionType.IsAnomaly
           ConditionalValue = ConditionalValue.AnomalyDetectionType DetectIIDSpike } 
    parse.Should().BeEquivalentTo(parsedCondition, "Equality of value", null) |> ignore

// parseAction.

[<Test>]
let ``parseAction with Invalid Operator Throws Exception``() =
    let incorrectFormat = "DoSomething CallStack"
    let parseIncorrect = fun () -> parseAction incorrectFormat 
    parseIncorrect.Invoking(fun y -> y()).Should().Throw<ArgumentException>("Incorrect format") |> ignore

[<Test>]
let ``parseAction with Invalid Operand Throws Exception``() =
    let incorrectFormat = "Print DoSomething"
    let parseIncorrect = fun () -> parseAction incorrectFormat 
    parseIncorrect.Invoking(fun y -> y()).Should().Throw<ArgumentException>("Incorrect format") |> ignore

[<Test>]
let ``parseAction with Valid Action is successful``() =
    let correctFormat = "Print Callstack"
    let parse         = parseAction correctFormat 
    let parsedAction  
        = { ActionOperator = ActionOperator.Print; ActionOperand = ActionOperand.CallStack }
    parse.Should().BeEquivalentTo(parsedAction, "Equivalent", null) |> ignore
