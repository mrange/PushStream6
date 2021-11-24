﻿namespace PumpStream6

type 'T PumpStream = ('T -> bool)->(unit -> bool)

module PumpStream =
  open System
  open System.Collections.Generic

  let inline ofArray (vs : _ array) : _ PumpStream = fun ([<InlineIfLambda>] r) ->
    let mutable i = 0
    fun () ->
      let mutable result = 0
      if i < vs.Length && r vs.[i] then
        i <- i + 1
        true
      else
        false


  // Generates a range of ints in b..e
  let inline ofRange b e : int PumpStream = fun ([<InlineIfLambda>] r) ->
    let mutable i = b
    fun () ->
      let mutable result = 0
      if i <= e && r i then
        i <- i + 1
        true
      else
        false

  // Filters a PushStream using a filter function
  let inline filter ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PumpStream) : _ PumpStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> if f v then r v else true)

  // Maps a PushStream using a mapping function
  let inline map ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PumpStream) : _ PumpStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> r (f v))

  // Folds a PushStream using a folder function f and an initial value z
  let inline fold ([<InlineIfLambda>] f) z ([<InlineIfLambda>] ps : _ PumpStream) =
    let mutable s = z
    let p = ps (fun v -> s <- f s v; true)
    while p () do ()
    s

  let inline toResizeArray (sz : int) ([<InlineIfLambda>] ps : _ PumpStream) =
    let ra = ResizeArray sz
    let p = ps (fun v -> ra.Add v; true)
    while p () do ()
    ra

  let inline toArray ([<InlineIfLambda>] ps : _ PumpStream) =
    let ra = toResizeArray 16 ps
    ra.ToArray ()

  let inline toSeq ([<InlineIfLambda>] ps : 'T PumpStream) =
    { new IEnumerable<'T> with
      override x.GetEnumerator () : IEnumerator<'T>         = 
        let mutable current = ValueNone
        let p = ps (fun v -> current <- ValueSome v; true)
        { new IEnumerator<'T> with
          // TODO: Implement Dispose
          member x.Dispose ()     = ()
          member x.Reset ()       = raise (NotSupportedException ())
          member x.Current : 'T   = current.Value
          member x.Current : obj  = current.Value
          member x.MoveNext ()    =
            current <- ValueNone
            while p () && current.IsNone do ()
            current.IsSome
        }
      override x.GetEnumerator () : Collections.IEnumerator = 
        x.GetEnumerator ()
    }

  // It turns out that if we pipe using |> the F# compiler don't inlines
  //  the lambdas as we like it to
  //  So define a more restrictive version of |> that applies function f to a function v
  //  As both f and v are restibted to lambas we can apply InlineIfLambda
  let inline (|>>) ([<InlineIfLambda>] v : _ -> _) ([<InlineIfLambda>] f : _ -> _) = f v

