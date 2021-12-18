module CommandLine

open Argu

type Arguments = 
    | TracePath of Path : string
    | [<Mandatory>] ProcessName of string
    | RulesPath of Path : string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | TracePath   _ -> "Specify a Path to the Trace."
            | ProcessName _ -> "Specify a Process Name."
            | RulesPath   _ -> "Specify a Path to a Json File With the Rules."