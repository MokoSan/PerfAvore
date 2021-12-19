module CommandLine

open Argu

type Arguments = 
    | [<Mandatory>] ProcessName of string
    | TracePath of Path : string
    | RulesPath of Path : string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | TracePath   _ -> "Specify a Path to the Trace."
            | ProcessName _ -> "Specify a Process Name."
            | RulesPath   _ -> "Specify a Path to a Json File With the Rules."