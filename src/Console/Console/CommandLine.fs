module CommandLine

open Argu

type Arguments = 
    | TracePath of Path : string
    | ProcessName of string
    | RulesFile of Path : string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | TracePath _   -> "Specify a Path to the Trace."
            | ProcessName _ -> "Specify a Process Name."
            | RulesFile _   -> "Specify a Path to a Json File With the Rules."