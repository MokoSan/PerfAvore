module RulesEngine.Actions.Alert

open Spectre.Console
open RulesEngine.Domain

open Microsoft.Diagnostics.Tracing

let printAlert (rule : Rule) (traceEvent : TraceEvent) : unit = 

    // Create a table
    let table = Table();
    table.Title <- TableTitle "[underline red] Alert! [/]"
    table.Title.Style <- Style()

    table.AddColumn("Input Rule")      |> ignore
    table.AddColumn("Timestamp")       |> ignore
    table.AddColumn("Event Name")      |> ignore
    table.AddColumn("Event Property")  |> ignore
    table.AddColumn("Payload")         |> ignore

    table.AddRow( rule.InputRule, 
                  traceEvent.TimeStampRelativeMSec.ToString(), 
                  traceEvent.EventName,
                  rule.Condition.Conditioner.ConditionerProperty,
                  traceEvent.PayloadByName(rule.Condition.Conditioner.ConditionerProperty).ToString() ) |> ignore

    table.Border <- TableBorder.Square

    // Render the table to the console
    AnsiConsole.Write(table);