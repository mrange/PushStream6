open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Running

open PushStream6.Benchmark

module Literals =
  [<Literal>]
  let Size = 10000

open Literals

type [<Struct>] V2 = V2 of int*int

module SystemLinq =
    open System.Linq

    let Invoke () =
        Enumerable
         .Range(0,(Size + 1))
         .Select((+) 1)
         .Where(fun v -> (v &&& 1) = 0)
         .Select(int64)
         .Sum()

module CisternValueLinq =
    open Cistern.ValueLinq

    let Invoke () =
        Enumerable
         .Range(0,Size + 1)
         .Select((+) 1)
         .Where(fun v -> (v &&& 1) = 0)
         .Select(int64)
         .Sum()

    [<Struct>] type AddOne         = interface IFunc<int,int>   with member _.Invoke x = x + 1
    [<Struct>] type FilterEvenInts = interface IFunc<int,bool>  with member _.Invoke x = (x &&& 1) = 0
    [<Struct>] type IntToInt64     = interface IFunc<int,int64> with member _.Invoke x = int64 x
    let Fast () =
        Enumerable
         .Range(0,Size + 1)
         .Select(AddOne ())
         .Where(FilterEvenInts ())
         .Select(IntToInt64 ())
         .Sum()

module PushStreamTest =
  open PushStream6
  open PushStream

  let pushStream () =
    // PushStream using |>
    ofRange   0 Size
    |> map    ((+) 1)
    |> filter (fun v -> (v &&& 1) = 0)
    |> map    int64
    |> fold   (+) 0L

  let fasterPushStream () =
    // PushStream using |>> as it turns out that
    //  |> prevents inlining of lambdas
    ofRange     0 Size
    |>> map     ((+) 1)
    |>> filter  (fun v -> (v &&& 1) = 0)
    |>> map     int64
    |>> fold    (+) 0L

  let pushStreamV2 () =
    // This test was hurt by tail-calls prior to .NET6
    ofRange   0 Size
    |> map    (fun v -> V2 (v, 0))
    |> map    (fun (V2 (v, w)) -> V2 (v + 1, w))
    |> filter (fun (V2 (v, _)) -> (v &&& 1) = 0)
    |> map    (fun (V2 (v, _)) -> int64 v)
    |> fold   (+) 0L

  let fasterPushStreamV2 () =
    ofRange     0 Size
    |>> map     (fun v -> V2 (v, 0))
    |>> map     (fun (V2 (v, w)) -> V2 (v + 1, w))
    |>> filter  (fun (V2 (v, _)) -> (v &&& 1) = 0)
    |>> map     (fun (V2 (v, _)) -> int64 v)
    |>> fold    (+) 0L


module CsPushStreamTest =
  open PushStream6.CsPushStream
  let Invoke () =
      PushStream
        .Range(0,(Size + 1))
        .Select((+) 1)
        .Where(fun v -> (v &&& 1) = 0)
        .Select(int64)
        .Aggregate((+), 0L)

module PumpStreamTest =
  open PumpStream6
  open PumpStream

  let fasterPumpStream () =
    ofRange     0 Size
    |>> map     ((+) 1)
    |>> filter  (fun v -> (v &&& 1) = 0)
    |>> map     int64
    |>> fold    (+) 0L

  let fasterPumpStreamV2 () =
    ofRange     0 Size
    |>> map     (fun v -> V2 (v, 0))
    |>> map     (fun (V2 (v, w)) -> V2 (v + 1, w))
    |>> filter  (fun (V2 (v, _)) -> (v &&& 1) = 0)
    |>> map     (fun (V2 (v, _)) -> int64 v)
    |>> fold    (+) 0L

[<Config(typeof<BenchmarkConfig>)>]
[<MemoryDiagnoser>]
//[<RyuJitX64Job>]
//[<RyuJitX86Job>]
type PushStream6Benchmark() =
  class
    [<Benchmark(Baseline=true)>]
    member x.Baseline() =
      // The baseline performance
      //  We expect this to do the best
      let mutable s = 0L
      for i = 0 to Size do
        let i = i + 1
        if (i &&& 1) = 0 then
          s <- s + int64 i
      s

    [<Benchmark>]
    member x.Linq() =
      // LINQ performance
      SystemLinq.Invoke ()

    [<Benchmark>]
    member x.ValueLinq() =
      // manofstick's CisternValueLinq performance
      CisternValueLinq.Invoke ()

    [<Benchmark>]
    member x.ValueLinqFast() =
      // manofstick's CisternValueLinq performance
      CisternValueLinq.Fast ()

    [<Benchmark>]
    member x.Array () =
      // Array performance
      Array.init Size id
      |> Array.map    ((+) 1)
      |> Array.filter (fun v -> (v &&& 1) = 0)
      |> Array.map    int64
      |> Array.fold   (+) 0L

    [<Benchmark>]
    member x.Seq () =
      // Seq performance
      seq { 0..Size }
      |> Seq.map    ((+) 1)
      |> Seq.filter (fun v -> (v &&& 1) = 0)
      |> Seq.map    int64
      |> Seq.fold   (+) 0L


    [<Benchmark>]
    member x.PushStream () =
      PushStreamTest.pushStream ()

    [<Benchmark>]
    member x.FasterPushStream () =
      PushStreamTest.fasterPushStream ()

    [<Benchmark>]
    member x.PushStreamV2 () =
      PushStreamTest.pushStreamV2 ()

    [<Benchmark>]
    member x.FasterPushStreamV2 () =
      PushStreamTest.fasterPushStreamV2 ()

    [<Benchmark>]
    member x.FasterPumpStream () =
      PumpStreamTest.fasterPumpStream ()

    [<Benchmark>]
    member x.FasterPumpStreamV2 () =
      PumpStreamTest.fasterPumpStreamV2 ()

    [<Benchmark>]
    member x.CsPushStream () =
      // C# version of push stream
      CsPushStreamTest.Invoke ()
  end

BenchmarkRunner.Run<PushStream6Benchmark>() |> ignore