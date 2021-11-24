module PumpStreamProperties

open FsCheck

open System.Linq

open PumpStream6
open PumpStream

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
      let a = ofRange x y |>> toArray
      e = a

    static member ``ofRange x y |>> toSeq = { x..y }`` x y =
      let e = [|x..y|]
      let a = ofRange x y |>> toSeq |> Seq.toArray
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

    static member ``fold = Array.fold`` (vs : int array) =
      let e = vs |> Array.fold (+) 0
      let a = ofArray vs |>> fold (+) 0
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