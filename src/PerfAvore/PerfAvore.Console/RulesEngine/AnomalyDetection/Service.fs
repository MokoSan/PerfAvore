module AnomalyDetection.Service

open System
open System.Collections.Concurrent

open RulesEngine.Domain

type FixedSizeQueue<'T> (capacity : int) =
    // Concurrency might not be necessary but better to be safe than sorry.
    let queue = ConcurrentQueue<'T>()

    member this.Capacity : int = capacity
    member this.Count    : int = queue.Count
    member this.Print() : unit = 
        let stringRepr : string = String.Join(",", queue.ToArray())
        printfn "%A" stringRepr

    member this.Insert (item : 'T) : unit = 
        // If we are at capacity, evict the first item.
        if queue.Count = capacity then 
            queue.TryDequeue() |> ignore
            
        // Enqueue the new item to the list.
        queue.Enqueue(item)

    member this.GetAll() : seq<'T> = 
        queue

type AnomalyDetectionContextService(capacity : int) = 
    // Keyed on the Rule Id and Value is a FixedSizeQueueForTraceEvents.
    // Each Rule that has Anomaly Detection associated with it must have its own Fixed Size Queue.
    let cache = ConcurrentDictionary<Guid, FixedSizeQueue<AnomalyDetectionInput>>()

    static member AnomalyPValueHistoryLength : int    = 30
    static member AnomalyConfidence          : double = 95.

    member this.Upsert (ruleId : Guid) (item : AnomalyDetectionInput) : unit =
        let queueExists, queue = cache.TryGetValue ruleId
        match queueExists, queue with
        | true, q  -> 
            q.Insert item
        | false, _ -> 
            cache.[ruleId] <- FixedSizeQueue( capacity )
            cache.[ruleId].Insert item

    member this.TryRetrieve(ruleId : Guid) : AnomalyDetectionInput seq option = 
        let queueExists, queue = cache.TryGetValue ruleId
        match queueExists, queue with
        | true, q  -> Some (q.GetAll())
        | false, _ -> None