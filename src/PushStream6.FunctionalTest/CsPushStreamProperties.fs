module CsPushStreamProperties

open FsCheck

open System.Linq

open PushStream6.CsPushStream

module Common =
  let ofRange x y = 
    let c = max 0 (y - x + 1)
    PushStream.Range(x, c)

  let ofArray vs = PushStream.FromArray(vs)

  let filter  (f : 'T -> bool) (ps : _ PushStream) = ps.Where(f)

  let map     (f : 'T -> 'U)   (ps : _ PushStream) = ps.Select(f)

  let fold    (f : 'S -> 'T -> 'S) (z : 'S) (ps : _ PushStream) : 'S = ps.Aggregate(f, z)

  let toArray (ps : _ PushStream) = ps.ToArray()

open Common


type SingleSearchProperties =
  class

  end

type ShallowSearchProperties =
  class

  end

type DeepSearchProperties =
  class

    static member ``ofRange x y |>> toArray = [|x..y|]`` x y =
      let e = [|x..y|]
      let a = ofRange x y |> toArray
      e = a

    static member ``filter = Array.filter`` f (vs : int array) =
      let e = vs |> Array.filter f
      let a = ofArray vs |> filter f |> toArray
      e = a

    static member ``map = Array.map`` (vs : int array) =
      let f v = int64 (v + 1)
      let e   = vs |> Array.map f
      let a   = ofArray vs |> map f |> toArray
      e = a

    static member ``fold = Array.fold`` (vs : int array) =
      let e = vs |> Array.fold (+) 0
      let a = ofArray vs |> fold (+) 0
      e = a

  end

let run () =
  let singletonSearch = { Config.Default with MaxTest = 1 ; MaxFail = 1 }
  #if DEBUG
  let shallowSearch = Config.Quick
  let deepSearch    = Config.Quick
  #else
  let shallowSearch = { Config.Default with MaxTest = 100 ; MaxFail = 100   }
  let deepSearch    = { Config.Default with MaxTest = 1000; MaxFail = 1000  }
  #endif

  Check.All<SingleSearchProperties>   singletonSearch
  Check.All<ShallowSearchProperties>  shallowSearch
  Check.All<DeepSearchProperties>     deepSearch