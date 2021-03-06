# Perf Avore: A Rule Based Performance Analysis and Monitoring Tool in FSharp

![Unit Tests](https://github.com/MokoSan/PerfAvore/actions/workflows/run_tests.yml/badge.svg)
![Nuget](https://img.shields.io/nuget/v/PerfAvore?style=flat-square)

## Introduction 

For my 2021 F# Advent Submission (5 years of submissions!!!!), I developed a Performance Based Monitoring and Analysis Tool called __"Perf Avore"__ that uses Rules specified by the user to match conditions and invoke actions on either an .ETL Trace files or real time processing. More in depth details about the implementation, process amongst other topics are mentioned in the accompanying [notebook](https://github.com/MokoSan/PerfAvore/blob/main/AdventSubmission.ipynb).

The use case of Perf Avore would be to detect and diagnose performance issues effectively by specifying details that are pertinent to performance issues in the rule itself. For example, spikes in allocations can put pressure on the GC and inevitably slow down the process; specifying a rule that tracks ``AllocationAmount`` on the ``GC/AllocationTick`` event if it goes above a specified amount and then print the call stack for it can shed light on the impetus behind the increased pressure.

### High Level Overview

1. Users provide rules.
   1. Rules consist of conditions and actions.
   2. Conditions Include: 
      1. The Name of the Trace Event and the property they'd like to trace. 
      2. The condition or case for which they'd like to act on.
      3. Examples:
         1. ``GC/AllocationTick.AllocationAmount > 200000 : Print Alert``
         2. ``ThreadPoolWorkerThreadAdjustment/Stats.Throughput < 4: Print CallStack``
         3. ``GC/HeapStats.GenerationSize0 isAnomaly DetectIIDSpike : Print Chart``
2. Based on either a given trace or by real time monitoring, conditions are checked for and actions are invoked.

![High Level Idea](Images/HighlevelIdea.png)

## Rules

A rule consists of a __Condition__ and an __Action__. An explanation of a rule is best explained with an example:

``GC/AllocationTick.AllocationAmount > 200000 : Print Alert``

Here, the user requests that for the said process, an alert will be printed if the ``AllocationAmount`` of the ``GC/AllocationTick`` event is greater than 200,000 bytes. The action if the condition is met is that of alerting the user by outputting a message. 

A rule, more generally, is of the following format: 
``EventName.PropertyName ConditionalOperator ConditionalOperand : ActionOperator ActionOperand``

where:

| Part | Description | 
| ----------- | ----------- |
| Event Name | The event name from the trace / real time analysis for which we want to look up the property | 
| Property Name | A double property (this may change in the future) for which we'd want to construct a rule for | 
| Conditional Operator | An operator that, along with the Conditional Operand, will dictate situation for which we'll invoke an action for. |   
| Conditional Operand | The value or name of the anomaly detection operator along with the Conditional Operator that'll dictate the situation for which we'll invoke an action for. | 
| Action Operator | The operator that, along with the action operand will be invoked if a condition is met. |  
| Action Operand | The operand for which the action operator will be applied to in case a condition is met | 

### Condition Types

The following are the currently implemented Conditions worth mentioning:

| Condition Operation | Description | 
| ----------- | ----------- |
| IsAnomaly | The condition to match on an anomaly detection algorithm. | 
| > >= < <= != = | Self explanatory conditional matching based on the value of the event property specified by the rule | 

The currently implemented algorithm is that of __IID Spike Detector__ based on the ML.NET algorithm that can be found [here](https://github.com/dotnet/machinelearning/blob/main/src/Microsoft.ML.TimeSeries/IidSpikeDetector.cs). More details about this algorithm can be found in the [Notebook](https://nbviewer.org/github/MokoSan/PerfAvore/blob/main/AdventSubmission.ipynb).

### Action Types

The following are the currently implemented action types:

| Name of Action Type | Description | 
| ----------- | ----------- |
| Alert | Alerting Mechanism that'll print out pertinent details about the rule invoked and why it was invoked. |
| Call Stack | If a call stack is available, it will be printed out on the console. |
| Chart | A chart of data points preceding and including the one that triggered the condition of the rule is generated and rendered as an html file | 

## Running Perf-Avore

Perf-Avore can be run by cd'ing into the ``src/PerfAvore/PerfAvore.Console`` directory and then running:

1. ``dotnet restore``
2.  ``dotnet run -- --processname <ProcessName> [--tracepath <TracePath>] [--rulespath <RulesPath>]``.

### Command Line Arguments 

| Command Line Option | Description | 
| ----------- | ----------- |
| ``processname`` | Name of the Process to analyze. This is the only mandatory parameter. |
| ``tracepath`` | The path of the trace file (.ETL / .ETLX). The absence of this command line will trigger a real time session. Note: For real time sessions, admin privileges are required. |
| ``rulespath`` | The path to a json file that contains a list of all the rules. By default, the ``SampleRules.json`` file will be used if this argument isn't specified. The location of this file is ``src\PerfAvore\PerfAvore.Console\SampleRules\SampleRules.json``  | 

#### Examples of Action Invocation

1. Alert

![Alert](Images/Example_Alert.jpeg)


2. Call Stack

![Call Stack](Images/Example_Callstack.jpeg)


3. Chart

![Chart](Images/Example_Charting.jpeg)

## Developing Perf-Avore

This project was heavily inspired by [maoni0's](https://twitter.com/maoni0) [realmon](https://github.com/Maoni0/realmon), a project I had such a great time contributing to, I wanted to generalize the use case. The name is a play on Italian's ``Per Favore`` meaning please where the purpose of the project is to please the most disgruntled of perf engineers by aiding them with investigations. 

This project was developed in VSCode using Ionide and the dotnet cli. The prototypes were all done using [DotNet Interactive](https://github.com/dotnet/interactive) [Notebooks](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode).

### Prototypes

The following were some of the prototypes created while developing Perf-Avore and can be found in ``src/Prototypes``:

1. Charting with the Trace Log API
2. Anomaly Detection using ML.NET
3. Rule Engine Domain 
4. Call Stack With Symbol Resolution

## ~~TODO For the FSharp Advent Submission~~

1. ~~RealTime Monitoring as a Console App~~
2. ~~Integrating Anomaly Detection into the Rules Engine~~
3. ~~Creating a Console App that'll demonstrate spikes~~
   1. ~~On a timer allocate a significant amount of memory~~
4. ~~Write the f*cking post~~

## TODO For Later On

1. ~~Linux / MacOs Compatibility.~~
2. Audit report.
3. Unit Testing!

## References

1. [Taking Stock of Anomalies with F# And ML.NET](https://www.codesuji.com/2019/05/24/F-and-MLNet-Anomaly/)
2. [A CPU Sampling Profiler in Less Than 200 Lines](https://lowleveldesign.org/2020/10/13/a-cpu-sampling-profiler-in-less-than-200-lines/)
3. [Tutorial: Detect anomalies in time series with ML.NET](https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/phone-calls-anomaly-detection)
4. [Plug-in martingales for testing exchangeability on-line - arXiv:1204.3251](https://arxiv.org/pdf/1204.3251.pdf)