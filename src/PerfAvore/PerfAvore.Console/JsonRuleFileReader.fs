module JsonRuleFileReader

open System.Text.Json
open System.IO

let getJsonRulesFromString (jsonAsString : string) : string list =
    let rules = JsonSerializer.Deserialize<string list>(jsonAsString)
    rules
    
let getJsonRulesFromFile (path : string) : string list =
    let json = File.ReadAllText path
    getJsonRulesFromString json