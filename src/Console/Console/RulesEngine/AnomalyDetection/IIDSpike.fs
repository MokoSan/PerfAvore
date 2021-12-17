module RulesEngine.AnomalyDetection.IIDSpike

open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Transforms.TimeSeries
open System.Collections.Generic
open System.Linq

let ctx : MLContext = MLContext()

type Input() =
    [<DefaultValue>]
    [<LoadColumn(0)>]
    val mutable public timestamp : double 

    [<DefaultValue>]
    [<LoadColumn(1)>]
    val mutable public value : float32

type Prediction() = 
    [<DefaultValue>]
    [<VectorType(3)>] // Prediction + value + p-value
    val mutable public Prediction : double[]

// double * double -> Timestamp * Value
let getAnomalies (input : (double * double) seq ) =
    let dataView = 
        ctx
            .Data
            .LoadFromEnumerable<Input>(input |> Seq.map(fun (timestamp, value) -> Input(timestamp = timestamp, value = float32 value) ))
        
    let anomalyPValueHistoryLength = 30
    let anomalyConfidence = 95.

    // https://github.com/dotnet/machinelearning/blob/510f0112d4fbb4d3ee233b9ca95c83fae1f9da91/src/Microsoft.ML.TimeSeries/SequentialAnomalyDetectionTransformBase.cs
    // Steps:
    // 1. Compute raw anomaly score - for this method, it's simply the input value: https://github.com/dotnet/machinelearning/blob/510f0112d4fbb4d3ee233b9ca95c83fae1f9da91/src/Microsoft.ML.TimeSeries/IidAnomalyDetectionBase.cs#L191
    // 2. Compute p-value based on kernel density estimate: 
    //  -> https://github.com/dotnet/machinelearning/blob/510f0112d4fbb4d3ee233b9ca95c83fae1f9da91/src/Microsoft.ML.TimeSeries/SequentialAnomalyDetectionTransformBase.cs#L562
    //  -> https://github.com/dotnet/machinelearning/blob/510f0112d4fbb4d3ee233b9ca95c83fae1f9da91/src/Microsoft.ML.TimeSeries/SequentialAnomalyDetectionTransformBase.cs#L475 
    // If p-value < (1 - confidence / 100.0) -> Alert i.e. anomaly.
    let anomalyPipeline =
        ctx.Transforms.DetectIidSpike(
        outputColumnName = "Prediction",
        inputColumnName = "value",
        side = AnomalySide.TwoSided,
        confidence = anomalyConfidence,  //  Alert Threshold = 1 - options.Confidence / 100;
        pvalueHistoryLength = anomalyPValueHistoryLength)

    // For this model, fitting doesn't matter.
    let trainedAnomalyModel = anomalyPipeline.Fit(ctx.Data.LoadFromEnumerable(List<Input>()))
    let transformedAnomalyData = trainedAnomalyModel.Transform(dataView)
    let anomalies = 
        ctx.Data.CreateEnumerable<Prediction>(transformedAnomalyData, reuseRowObject = false)
    let anomaliesWithTimeStamp : (double * double * double) seq = 
        anomalies
        |> Seq.mapi(fun i p -> p.Prediction.[0], p.Prediction.[1], fst (input.ElementAt(i)))

    anomalies