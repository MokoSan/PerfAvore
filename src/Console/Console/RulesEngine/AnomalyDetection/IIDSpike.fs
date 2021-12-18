module AnomalyDetection.IIDSpike

open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Transforms.TimeSeries
open System.Collections.Generic

open System.Linq
open AnomalyDetection.Service

open RulesEngine.Domain

let ctx : MLContext = MLContext()

type Prediction() = 
    [<DefaultValue>]
    [<VectorType(3)>] // prediction i.e. 0/1 + value i.e. payload + p-value
    val mutable public Prediction : double[]

let getAnomaliesUsingIIDSpikeEstimation (input : AnomalyDetectionContext) 
                                        (service : AnomalyDetectionContextService) 
                                        : AnomalyDetectionResult =
    let retrievedInput = service.TryRetrieve input.Rule.Id 
    let buffer =          
        match retrievedInput with
        | Some b -> b 
        | None   -> failwith $"Failed to look up Anomaly Detection Buffer for rule: {input.Rule.InputRule}" 

    let dataView = 
        ctx.Data.LoadFromEnumerable<AnomalyDetectionInput>(buffer)
        
    // https://github.com/dotnet/machinelearning/blob/510f0112d4fbb4d3ee233b9ca95c83fae1f9da91/src/Microsoft.ML.TimeSeries/SequentialAnomalyDetectionTransformBase.cs
    // Steps:
    // 1. Compute raw anomaly score - for this method, it's simply the input value: https://github.com/dotnet/machinelearning/blob/510f0112d4fbb4d3ee233b9ca95c83fae1f9da91/src/Microsoft.ML.TimeSeries/IidAnomalyDetectionBase.cs#L191
    // 2. Compute p-value based on kernel density estimate: 
    //  -> https://github.com/dotnet/machinelearning/blob/510f0112d4fbb4d3ee233b9ca95c83fae1f9da91/src/Microsoft.ML.TimeSeries/SequentialAnomalyDetectionTransformBase.cs#L562
    //  -> https://github.com/dotnet/machinelearning/blob/510f0112d4fbb4d3ee233b9ca95c83fae1f9da91/src/Microsoft.ML.TimeSeries/SequentialAnomalyDetectionTransformBase.cs#L475 
    // If p-value < (1 - confidence / 100.0) -> Alert i.e. anomaly.
    let anomalyPipeline : IidSpikeEstimator =
        ctx.Transforms.DetectIidSpike(
        outputColumnName    = "Prediction",
        inputColumnName     = "value",
        side                = AnomalySide.TwoSided,
        confidence          = AnomalyDetectionContextService.AnomalyConfidence,  //  Alert Threshold = 1 - options.Confidence / 100;
        pvalueHistoryLength = AnomalyDetectionContextService.AnomalyPValueHistoryLength )

    // For this model, fitting doesn't matter.
    let trainedAnomalyModel : IidSpikeDetector 
        = anomalyPipeline.Fit(ctx.Data.LoadFromEnumerable(List<AnomalyDetectionInput>()))
    let transformedAnomalyData : IDataView 
        = trainedAnomalyModel.Transform(dataView)
    let anomalies : Prediction seq = 
        ctx.Data.CreateEnumerable<Prediction>(transformedAnomalyData, reuseRowObject = false)

    // Last one in the buffer since it's the most recent one. 
    let inputPoint = anomalies.Last()
    { Context   = input 
      IsAnomaly = inputPoint.Prediction[0] = 1
      PValue    = inputPoint.Prediction[2] }