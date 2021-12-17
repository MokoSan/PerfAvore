module CommandLine

open Argu

type Arguments = 
    | TracePath of Path : string
    | ProcessId of int
    | RulesFile of Path : string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | TracePath _ -> "Specify a Path to the Trace."
            | ProcessId _ -> "Specify a Process ID to Monitor in RealTime."
            | RulesFile _ -> "Specify a Path to a Json File With the Rules."