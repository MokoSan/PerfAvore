module RulesEngine.Actions.Chart 

open System.Linq
open FSharp.Plotly
open RulesEngine.Domain
open AnomalyDetection.Service

let printChart (rule : Rule) (service : AnomalyDetectionContextService) : unit = 

    let v = service.TryRetrieve(rule.Id).Value
    let x = 
        v
        |> Seq.map(fun i -> i.timestamp)
    let y = 
        v
        |> Seq.map(fun i -> i.value)
    let input = Seq.zip x y
    let point = v.Last()
    let scatterPoint = seq { point.timestamp, point.value }

    [
        Chart.Line (input, Name = $"Trend") 
        Chart.Scatter (scatterPoint, mode = StyleParam.Mode.Markers, Name="Anomaly Point")
    ]
    |> Chart.Combine
    |> Chart.withX_AxisStyle(title = "Relative Timestamp (ms)")
    |> Chart.withY_AxisStyle(title = $"{rule.Condition.Conditioner.ConditionerProperty}")
    |> Chart.Show