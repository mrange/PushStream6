namespace PushStream6

// A PushStream is a function that accepts a receiver
//  The receiver is sent values and respond true if it
//  wants more values, false otherwiese.
//  A PushSream returns true if the entire source was
//  consumed, false otherwise
type 'T PushStream = ('T -> bool) -> bool

module PushStream =

  // Empty PushStream
  [<GeneralizableValue>]
  let inline empty<'T> : 'T PushStream = fun ([<InlineIfLambda>] r) ->
    true

  // PushStream from single value
  let inline singleton v : _ PushStream = fun ([<InlineIfLambda>] r) ->
    r v

  // Generates a range of ints in b..e
  let inline ofRange b e : int PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable i = b
    while i <= e && r i do
      i <- i + 1
    i > e

  // PushStream of Array
  let inline ofArray (vs : _ array) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable i = 0
    while i < vs.Length && r vs.[i] do
      i <- i + 1
    i = vs.Length

  // PushStream of List
  let inline ofList (vs : _ list) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable l = vs
    // TODO: This code results in a double eval of .Tail.
    //  It would be better if we could find a code pattern
    //  that only eval .Tail once
    while not l.IsEmpty && r l.Head do
      l <- l.Tail
    l.IsEmpty

  // Collects a PushStream using a collect function
  let inline collect ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream)  : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> (f v) r)

  // Chooses values in a PushStream using a choice function
  let inline choose ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream)  : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> match f v with Some c -> r c | _ -> true)

  // Filters a PushStream using a filter function
  let inline filter ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> if f v then r v else true)

  // Maps a PushStream using a mapping function
  let inline map ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream)  : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> r (f v))

  // Similar to take but don't throw if less than n elements in PushStream
  let inline top n ([<InlineIfLambda>] ps : _ PushStream)  : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable rem = n
    ps (fun v -> if rem > 0 then rem <- rem - 1; r v else false)

  // Similar to skip but don't throw if less than n elements in PushStream
  let inline drop n ([<InlineIfLambda>] ps : _ PushStream)  : _ PushStream = fun ([<InlineIfLambda>] r) ->
    let mutable rem = n
    ps (fun v -> if rem > 0 then rem <- rem - 1; true else r v)

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