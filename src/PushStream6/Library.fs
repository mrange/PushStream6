namespace PushStream6

type 'T PushStream = ('T -> bool) -> bool

module PushStream =

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

  // Collects a PushStream using a collect function
  let inline collect ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream)  : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> (f v) r)

  // Filters a PushStream using a filter function
  let inline filter ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> if f v then r v else true)

  // Maps a PushStream using a mapping function
  let inline map ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream)  : _ PushStream = fun ([<InlineIfLambda>] r) ->
    ps (fun v -> r (f v))

  // Folds a PushStream using a folder function f and an initial value z
  let inline fold ([<InlineIfLambda>] f) z ([<InlineIfLambda>] ps : _ PushStream) =
    let mutable s = z
    ps (fun v -> s <- f s v; true) |> ignore
    s

  // ResizeArray from PushStream
  let inline toResizeArray (initial : int) ([<InlineIfLambda>] ps : _ PushStream) : _ ResizeArray =
    let ra = ResizeArray initial
    ps (fun v -> ra.Add v; true) |> ignore
    ra

  // Array from PushStream
  let inline toArray ([<InlineIfLambda>] ps : _ PushStream) : _ array =
    let ra = toResizeArray 16 ps
    ra.ToArray ()

  // List from PushStream
  let inline toList ([<InlineIfLambda>] ps : _ PushStream) : _ list =
    let l = fold (fun s v -> v::s) [] ps
    List.rev l

  // It turns out that if we pipe using |> the F# compiler don't inlines
  //  the lambdas as we like it to
  //  So define a more restrictive version of |> that applies function f to a function v
  //  As both f and v are restibted to lambas we can apply InlineIfLambda
  let inline (|>>) ([<InlineIfLambda>] v : _ -> _) ([<InlineIfLambda>] f : _ -> _) = f v