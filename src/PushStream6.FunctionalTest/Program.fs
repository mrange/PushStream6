open FsCheck

open PushStream6
open PushStream

type Properties =
  class
    static member ``ofRange x y |>> toArray = [|x..y|]`` x y =
      let e = [|x..y|]
      let a = ofRange x y |>> toArray
      e = a

    static member ``ofArray vs |>> toArray = vs`` (vs : int array) =
      let e = vs
      let a = ofArray vs |>> toArray
      e = a

    static member ``ofList vs |>> toList = vs`` (vs : int list) =
      let e = vs
      let a = ofList vs |>> toList
      e = a

    static member ``filter = Array.filter`` f (vs : int array) =
      let e = vs |> Array.filter f
      let a = ofArray vs |>> filter f |>> toArray
      e = a

    static member ``map = Array.map`` (vs : int array) =
      let f v = int64 (v + 1)
      let e   = vs |> Array.map f
      let a   = ofArray vs |>> map f |>> toArray
      e = a

    static member ``collect = Array.collect`` (vs : int array array) =
      let e = vs |> Array.collect id
      let a = ofArray vs |>> collect ofArray |>> toArray
      e = a
  end

#if DEBUG
let config = Config.Default
#else
let config = { Config.Default with MaxTest = 1000; MaxFail = 1000 }
#endif

Check.All<Properties> config