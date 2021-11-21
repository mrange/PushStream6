open FsCheck

open System.Linq

open PushStream6
open PushStream

type SingleSearchProperties =
  class

    static member ``empty = Array.empty`` () =
      let e = Array.empty : int array
      let a = empty |>> toArray
      e = a


    static member ``singleton = Array.singleton`` (v : int) =
      let e = v |> Array.singleton
      let a = singleton v |>> toArray
      e = a

  end

type ShallowSearchProperties =
  class

    static member ``ofArray vs |>> toArray = vs`` (vs : int array) =
      let e = vs
      let a = ofArray vs |>> toArray
      e = a

    static member ``ofList vs |>> toList = vs`` (vs : int list) =
      let e = vs
      let a = ofList vs |>> toList
      e = a

    static member ``ofResizeArray vs |>> toResizeArray = vs`` (vs : int ResizeArray) =
      // .ToArray because ResizeArray don't have structural equalness
      let e = vs.ToArray ()
      let a = (ofResizeArray vs |>> toResizeArray 16).ToArray ()
      e = a

    static member ``ofSeq vs |>> toArray = vs`` (vs : int array) =
      let e = vs
      let a = ofSeq vs |>> toArray
      e = a

    static member ``ofValueSeq vs |>> toArray = vs`` (vs : int ResizeArray) =
      // .ToArray because ResizeArray don't have structural equalness
      let e = vs.ToArray ()
      let a = ofValueSeq vs |>> toArray
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

    static member ``collect = Array.collect`` (vs : int array array) =
      let e = vs |> Array.collect id
      let a = ofArray vs |>> collect ofArray |>> toArray
      e = a

    static member ``choose = Array.collect`` (vs : int array) =
      let f v = if (v &&& 1) = 0 then Some (int64 v + 1L) else None
      let e = vs |> Array.choose f
      let a = ofArray vs |>> choose f |>> toArray
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

    static member ``top = Enumerable.Take`` (n : int) (vs : int array) =
      let e = vs.Take(n).ToArray()
      let a = ofArray vs |>> top n |>> toArray
      e = a

    static member ``drop = Enumerable.Skip`` (n : int) (vs : int array) =
      let e = vs.Skip(n).ToArray()
      let a = ofArray vs |>> drop n |>> toArray
      e = a

    static member ``distinctBy = Array.distinctBy`` (vs : (int*int64) array) =
      let e = vs |> Array.distinctBy fst
      let a = ofArray vs |>> distinctBy fst |>> toArray
      e = a

    static member ``unionBy = Enumerable.UnionBy`` (first : (int*int64) array) (second : (int*int64) array) =
      let e = first.UnionBy(second, fst).ToArray()
      let a = ofArray first |>> unionBy fst (ofArray second) |>> toArray
      e = a

    static member ``intersectBy = Enumerable.IntersectBy`` (first : (int*int64) array) (second : (int*int64) array) =
      let e = first.IntersectBy(second.Select(fst), fst).ToArray()
      let a = ofArray first |>> intersectBy fst (ofArray second) |>> toArray
      e = a

    static member ``differenceBy = Enumerable.ExceptBy`` (first : (int*int64) array) (second : (int*int64) array) =
      let e = first.ExceptBy(second.Select(fst), fst).ToArray()
      let a = ofArray first |>> differenceBy fst (ofArray second) |>> toArray
      e = a

    static member ``chunkBySize = Array.chunkBySize`` n (vs : int array) =
      let n = abs n + 1
      let e = vs |> Array.chunkBySize n
      let a = ofArray vs |>> chunkBySize n |>> toArray
      e = a

    static member ``forAll = Array.forAll`` x (vs : int array) =
      let mutable esum = 0
      let f y = esum <- esum + y; x > y
      let mutable asum = 0
      let g y = asum <- asum + y; x > y

      let e = vs |> Array.forall f
      let a = ofArray vs |>> forAll g
      e = a && esum = asum

    static member ``fold = Array.fold`` (vs : int array) =
      let e = vs |> Array.fold (+) 0
      let a = ofArray vs |>> fold (+) 0
      e = a

    static member ``sortBy = Array.sortBy`` (vs : (int*int64) array) =
      let e = vs |> Array.sortBy fst
      let a = ofArray vs |>> sortBy fst |>> toArray
      e = a

    static member ``groupBy = Array.groupBy`` (vs : (int*int64) array) =
      let e = vs |> Array.groupBy fst
      let a = ofArray vs |>> groupBy fst |>> map (fun (k, v) -> k, v.ToArray()) |>> toArray
      e = a

  end

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