namespace PushStream6

// A PushStream is a function that accepts a receiver
//  The receiver is sent values and respond true if it
//  wants more values, false otherwiese.
//  A PushStream returns true if the entire source was
//  consumed, false otherwise
type 'T PushStream = ('T -> bool) -> bool

module PushStream =
  open System.Collections.Generic

  // Sources

  // Empty PushStream
  [<GeneralizableValue>]
  let inline empty<'T> : 'T PushStream = fun ([<InlineIfLambda>] r) ->
    true

  // PushStream from single value
  let inline singleton v : _ PushStream = fun ([<InlineIfLambda>] r) ->
    r v

  // PushStream of Array
  let inline ofArray (vs : _ array) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable i = 0
    while i < vs.Length && r vs.[i] do
      i <- i + 1
    i = vs.Length

  // PushStream of List
  let inline ofList (vs : _ list) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable complete = false
    let mutable l = vs
    while (match l with hd::tl -> l <- tl; r hd | [] -> complete <- true; false) do ()
    complete

  // Generates a range of ints in b..e
  let inline ofRange b e : int PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable i = b
    while i <= e && r i do
      i <- i + 1
    i > e

  // Pipes

  // Chooses values in a PushStream using a choice function
  let inline choose ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> match f v with Some c -> r c | _ -> true)

  // Collects a PushStream using a collect function
  let inline collect ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> (f v) r)

  // Filters a PushStream using a filter function
  let inline filter ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> if f v then r v else true)

  // Maps a PushStream using a mapping function
  let inline map ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> r (f v))

  // Similar to take but don't throw if less than n elements in PushStream
  let inline top n ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable rem = n
    ps (fun v -> if rem > 0 then rem <- rem - 1; r v else false)

  // Similar to skip but don't throw if less than n elements in PushStream
  let inline drop n ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable rem = n
    ps (fun v -> if rem > 0 then rem <- rem - 1; true else r v)

  let inline distinctBy ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let set = HashSet<_> ()
    ps (fun v -> if set.Add (f v) then r v else true)

  let inline unionBy ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps1 : _ PushStream) ([<InlineIfLambda>] ps0 : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let set = HashSet<_> ()
    ps0 (fun v -> if set.Add (f v) then r v else true)
    && ps1 (fun v -> if set.Add (f v) then r v else true)

  let inline intersectBy ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps1 : _ PushStream) ([<InlineIfLambda>] ps0 : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let set = HashSet<_> ()
    ps1 (fun v -> let _ = set.Add (f v) in true)
    && ps0 (fun v -> if set.Remove (f v) then r v else true)

  let inline differenceBy ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps1 : _ PushStream) ([<InlineIfLambda>] ps0 : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let set = HashSet<_> ()
    ps1 (fun v -> let _ = set.Add (f v) in true)
    && ps0 (fun v -> if set.Add (f v) then r v else true)

  let inline chunkBySize (sz : int) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    if sz <= 0 then failwith "sz must be 1 or greater"
    let vs = ResizeArray sz
    ps (fun v -> vs.Add v; if vs.Count < sz then true else r (vs.ToArray()) && (vs.Clear (); true))
    && (if vs.Count > 0 then r (vs.ToArray()) else true)

  // Sinks

  // Folds a PushStream using a folder function f and an initial value z
  let inline fold ([<InlineIfLambda>] f) z ([<InlineIfLambda>] ps : _ PushStream) =
    let mutable s = z
    let _ = ps (fun v -> s <- f s v; true)
    s

  // Try get first item from PushStream
  let inline tryHead ([<InlineIfLambda>] ps : _ PushStream) : _ =
    let mutable s = None
    let _ = ps (fun v -> s <- Some v; false)
    s

  // ResizeArray from PushStream
  let inline toResizeArray (initial : int) ([<InlineIfLambda>] ps : _ PushStream) : _ ResizeArray =
    let ra = ResizeArray initial
    let _ = ps (fun v -> ra.Add v; true)
    ra

  // Array from PushStream
  let inline toArray ([<InlineIfLambda>] ps : _ PushStream) : _ array =
    let ra = toResizeArray 16 ps
    ra.ToArray ()

  // Reverse list from PushStream
  let inline toReverseList ([<InlineIfLambda>] ps : _ PushStream) : _ list =
    fold (fun s v -> v::s) [] ps

  // List from PushStream
  let inline toList ([<InlineIfLambda>] ps : _ PushStream) : _ list =
    List.rev (toReverseList ps)

  // It turns out that if we pipe using |> the F# compiler don't inlines
  //  the lambdas as we like it to
  //  So define a more restrictive version of |> that applies function f to a function v
  //  As both f and v are restibted to lambas we can apply InlineIfLambda
  let inline (|>>) ([<InlineIfLambda>] v : _ -> _) ([<InlineIfLambda>] f : _ -> _) = f v