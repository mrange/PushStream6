open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

open System.Linq

open PushStream6
open PushStream

type [<Struct>] V2 = V2 of int*int

[<MemoryDiagnoser>]
[<RyuJitX64Job>]
//[<RyuJitX86Job>]
type PushStream6Benchmark() =
  class

    [<Benchmark>]
    member x.Baseline() =
      // The baseline performance
      //  We expect this to do the best
      let mutable s = 0L
      for i = 0 to 10000 do
        let i = i + 1
        if (i &&& 1) = 0 then
          s <- s + int64 i
      s

    [<Benchmark>]
    member x.Linq() =
      // LINQ performance
      Enumerable.Range(0,10001).Select((+) 1).Where(fun v -> (v &&& 1) = 0).Select(int64).Sum()

    [<Benchmark>]
    member x.Array () =
      // Array performance
      [|0..10000|]
      |> Array.map    ((+) 1)
      |> Array.filter (fun v -> (v &&& 1) = 0)
      |> Array.map    int64
      |> Array.fold   (+) 0L

    [<Benchmark>]
    member x.Seq () =
      // Seq performance
      seq { 0..10000 }
      |> Seq.map    ((+) 1)
      |> Seq.filter (fun v -> (v &&& 1) = 0)
      |> Seq.map    int64
      |> Seq.fold   (+) 0L

    [<Benchmark>]
    member x.PushStream () =
      // PushStream using |>
      ofRange   0 10000
      |> map    ((+) 1)
      |> filter (fun v -> (v &&& 1) = 0)
      |> map    int64
      |> fold   (+) 0L

    [<Benchmark>]
    member x.FasterPushStream () =
      // PushStream using |>> as it turns out that
      //  |> prevents inlining of lambdas
      ofRange     0 10000
      |>> map     ((+) 1)
      |>> filter  (fun v -> (v &&& 1) = 0)
      |>> map     int64
      |>> fold    (+) 0L

    [<Benchmark>]
    member x.PushStreamV2 () =
      // This test is hurt by tail-calls
      ofRange   0 10000
      |> map    (fun v -> V2 (v, 0))
      |> map    (fun (V2 (v, w)) -> V2 (v + 1, w))
      |> filter (fun (V2 (v, _)) -> (v &&& 1) = 0)
      |> map    (fun (V2 (v, _)) -> int64 v)
      |> fold   (+) 0L

    [<Benchmark>]
    member x.FasterPushStreamV2 () =
      ofRange     0 10000
      |>> map     (fun v -> V2 (v, 0))
      |>> map     (fun (V2 (v, w)) -> V2 (v + 1, w))
      |>> filter  (fun (V2 (v, _)) -> (v &&& 1) = 0)
      |>> map     (fun (V2 (v, _)) -> int64 v)
      |>> fold    (+) 0L
  end

BenchmarkRunner.Run<PushStream6Benchmark>() |> ignore