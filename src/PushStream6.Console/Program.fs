open System

open PushStream6
open PushStream

module CisternValueLinq =
    open Cistern.ValueLinq

    [<Struct>] type AddOne         = interface IFunc<int,int>   with member _.Invoke x = x + 1
    [<Struct>] type FilterEvenInts = interface IFunc<int,bool>  with member _.Invoke x = (x &&& 1) = 0
    [<Struct>] type IntToInt64     = interface IFunc<int,int64> with member _.Invoke x = int64 x
    let Fast () =
        Enumerable
         .Range(0,10001)
         .Select(AddOne ())
         .Where(FilterEvenInts ())
         .Select(IntToInt64 ())
         .Sum()

type [<Struct>] V2 = V2 of int*int

let baseline () =
  let mutable s = 0L
  for i = 0 to 10000 do
    let i = i + 1
    if (i &&& 1) = 0 then
      s <- s + int64 i
  s

let cisternValueLinq () = CisternValueLinq.Fast ()

let pushStream () =
  ofRange   0 10000
  |> map    ((+) 1)
  |> filter (fun v -> (v &&& 1) = 0)
  |> map    int64
  |> fold   (+) 0L

let fasterPushStream () =
  ofRange    0 10000
  |>> map    ((+) 1)
  |>> filter (fun v -> (v &&& 1) = 0)
  |>> map    int64
  |>> fold   (+) 0L

let pushStreamV2 () =
  ofRange   0 10000
  |> map    (fun v -> V2 (v, 0))
  |> map    (fun (V2 (v, w)) -> V2 (v + 1, w))
  |> filter (fun (V2 (v, _)) -> (v &&& 1) = 0)
  |> map    (fun (V2 (v, _)) -> int64 v)
  |> fold   (+) 0L

let fasterPushStreamV2 () =
  ofRange     0 10000
  |>> map     (fun v -> V2 (v, 0))
  |>> map     (fun (V2 (v, w)) -> V2 (v + 1, w))
  |>> filter  (fun (V2 (v, _)) -> (v &&& 1) = 0)
  |>> map     (fun (V2 (v, _)) -> int64 v)
  |>> fold    (+) 0L

let test = cisternValueLinq

printfn "Result: %A" <| test ()

printfn "Warm-up"
for i = 0 to 1000 do
  test () |> ignore

printfn "Attach a debugger and hit any key to continue"
Console.ReadKey () |> ignore

printfn "Running..."
for i = 0 to 10000000 do
  test () |> ignore

printfn "Done"

