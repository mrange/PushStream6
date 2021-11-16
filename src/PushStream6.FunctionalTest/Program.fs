open FsCheck

open System.Linq

open PushStream6
open PushStream

type ShallowSearchProperties =
  class
    static member ``singleton = Array.singleton`` (v : int) =
      let e = v |> Array.singleton
      let a = singleton v |>> toArray
      e = a

    static member ``tryHead = Array.tryHead`` (vs : int array) =
      let e = vs |> Array.tryHead
      let a = ofArray vs |>> tryHead
      e = a
  end

type DeepSearchProperties =
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

    static member ``choose = Array.collect`` (vs : int array) =
      let f v = if (v &&& 1) = 0 then Some (int64 v + 1L) else None
      let e = vs |> Array.choose f
      let a = ofArray vs |>> choose f |>> toArray
      e = a

    static member ``top = Enumerable.Take`` (n : int) (vs : int array) =
      let e = vs.Take(n).ToArray()
      let a = ofArray vs |>> top n |>> toArray
      e = a

    static member ``drop = Enumerable.Skip`` (n : int) (vs : int array) =
      let e = vs.Skip(n).ToArray()
      let a = ofArray vs |>> drop n |>> toArray
      e = a

    static member ``fold = Array.fold`` (vs : int array) =
      let e = vs |> Array.fold (+) 0
      let a = ofArray vs |>> fold (+) 0
      e = a

  end

#if DEBUG
let shallowSearch = Config.Quick
let deepSearch    = Config.Quick
#else
let shallowSearch = { Config.Default with MaxTest = 100 ; MaxFail = 100   }
let deepSearch    = { Config.Default with MaxTest = 1000; MaxFail = 1000  }
#endif

Check.All<ShallowSearchProperties>  shallowSearch
Check.All<DeepSearchProperties>     deepSearch