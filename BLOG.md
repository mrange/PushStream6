# F# Advent 2021  Dec 08 - Fast data pipelines with F#6

There were many interesting improvments for F#6 but one in particular caught my eye, the attribute `InlineIfLambda`.

The purpose of `InlineIfLambda` is to instruct the compiler to inline the lambda argument if possible. One reason is potentially improved performance.

## Example from FSharp.Core 6

```fsharp
let inline iter ([<InlineIfLambda>] action) (array: 'T[]) =
    checkNonNull "array" array
    for i = 0 to array.Length-1 do
        action array.[i]
```

Without `InlineIfLambda` would be inlined but invoking `action` would be a virtual call incurring overhad that _sometimes_ can be important.

```fsharp
// What we write
let mutable sum = 0
myArray |> Array.iter (fun v -> sum <- sum + v)
```

Before `InlineIfLambda` this is what we would get

```fsharp
// What we get before F#6

let sum = ref 0
let action v = sum := !sum + v

// Array.iter inlined
checkNonNull "array" myArray
for i = 0 to myArray.Length-1 do
    // But the action is not inlined
    action array.[i]
```

Even older versions of F# 5 sometimes do inlining but it's based on a complexity analysis that we have little control over.

So the above code could actually be inlined, or not inlined depending on what the complexity analysis thinks.

```fsharp
// What we get with F#6

let mutable sum = 0
checkNonNull "array" myArray
for i = 0 to myArray.Length-1 do
    sum <- sum + array.[i]
```

This avoid virtual calls as well as allocating a `ref` cell and a lambda.

## Arrays vs Seq

Arrays are great but one drawback is that for each step in a pipeline we would create an intermediate array which needs to be garbage collected.

```fsharp
// Creates an array
[|0..10000|]
// Creates a mapped array of ints
|> Array.map    ((+) 1)
// Creates a filtered array of ints
|> Array.filter (fun v -> (v &&& 1) = 0)
// Creates a mapped array of longs
|> Array.map    int64
// Creates a sum
|> Array.fold   (+) 0L
```

One way around this is using `seq`

```fsharp
seq { 0..10000 }
|> Seq.map    ((+) 1)
|> Seq.filter (fun v -> (v &&& 1) = 0)
|> Seq.map    int64
|> Seq.fold   (+) 0L
```

It turns out that the `seq` pipeline above is about 3x slower than the `Array` pipeline even if it doesn't allocate (that much) unnecessary memory.

Can we do better?

## Building a push stream with `InlineIfLambda`

In a dream world it would be great if we could have a data pipeline with very little overhead for both memory and CPU. Let's try to see what we can do with `InlineIfLambda`.

`seq` is an alias for `IEnumerable<_>` which is a so called pull data pipeline.

The consumer pulls value through the pipeline by calling `MoveNext` and `Current` on `IEnumerable<_>`.

Another approach is to let the producer of data push data through the pipeline. This kind of pipeline tends to be simpler to implement and more performant.

What also makes PushStream applicable for `InlineIfLambda` is that it can be implemented as nested lambdas so `InlineIfLambda` should help.

A variant of push streams would look like this:

```fsharp
type PushStream<'T> = ('T -> bool) -> bool
```

A `PushStream<_>` is a function that accepts a receiver function `'T->bool` and calls the receiver function until there are no more values are returned or the receiver function returns `false` indicating it wants no more values. `PushStream<_>` returns `true` if the producer values was fully consumed and `false` if the consumption is stopped before reaching the end of the producer.

A `PushStream` module could then look like this:

```fsharp
// 'T PushStream is an alternative syntax for PushStream<'T>
type 'T PushStream = ('T -> bool) -> bool

module PushStream =
  // Generates a range of ints in b..e
  //  Note the use of [<InlineIfLambda>] to inline the receiver function r
  let inline ofRange b e : int PushStream = fun ([<InlineIfLambda>] r) ->
      // This easy to implement in that we loop over the range b..e and
      //  call the receiver function r until either it returns false
      //  or we reach the end of the range
      //  Thanks to InlineIfLambda r should be inlined
      let mutable i = b
      while i <= e && r i do
        i <- i + 1
      i > e

  // Filters a PushStream using a filter function
  //  Note the use of [<InlineIfLambda>] to inline both the filter function f and the PushStream function ps
  let inline filter ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream) : _ PushStream = fun ([<InlineIfLambda>] r) ->
    // ps is the previous push stream which we invoke with our receiver lambda
    //  Our receiver lambda checks if each received value passes filter function f
    //  If it does we pass the value to r, otherwise we return true to continue
    //  f, ps and r are lambdas that should be inlined due to InlineIfLambda
    ps (fun v -> if f v then r v else true)

  // Maps a PushStream using a mapping function
  let inline map ([<InlineIfLambda>] f) ([<InlineIfLambda>] ps : _ PushStream)  : _ PushStream = fun ([<InlineIfLambda>] r) ->
    // ps is the previous push stream which we invoke with our receiver lambda
    //  Our receiver lambda maps each received value with map function f and
    //  pass the mapped value to r
    //  If it does we pass the value to r, otherwise we return true to continue
    //  f, ps and r are lambdas that should be inlined due to InlineIfLambda
    ps (fun v -> r (f v))

  // Folds a PushStream using a folder function f and an initial value z
  let inline fold ([<InlineIfLambda>] f) z ([<InlineIfLambda>] ps : _ PushStream) =
    let mutable s = z
    // ps is the previous push stream which we invoke with our receiver lambda
    //  Our receiver lambda folds the state and value with folder function f
    //  Returns true to continue looping
    //  f and ps are lambdas that should be inlined due to InlineIfLambda
    //  This also means that s should not need to be a ref cell which avoids
    //  some memory pressure
    ps (fun v -> s <- f s v; true) |> ignore
    s

  // It turns out that if we pipe using |> the F# compiler don't inline
  //  the lambdas as we like it to.
  //  So define a more restrictive version of |> that applies function f
  //  to a function v
  //  As both f and v are restricted to lambas we can apply InlineIfLambda
  let inline (|>>) ([<InlineIfLambda>] v : _ -> _) ([<InlineIfLambda>] f : _ -> _) = f v
```

The previous pipeline examples then with the `PushStream` definition above:

```fsharp
open PushStream
ofRange     0 10000
|>> map     ((+) 1)
|>> filter  (fun v -> (v &&& 1) = 0)
|>> map     int64
|>> fold    (+) 0L
```

Looks pretty good how does it perform?

## Comparing performance with different data pipelines

First let's define a baseline to compare all performance against, a simple for loop that computes the same result as the pipeline above

```fsharp
let mutable s = 0L
for i = 0 to 10000 do
  let i = i + 1
  if (i &&& 1) = 0 then
    s <- s + int64 i
s
```

This should be a reasonable efficient imperative version of the pipeline example.

Then we define a bunch of benchmarks and compare them using [Benchmark.NET](https://benchmarkdotnet.org/).

```fsharp
open PushStream

type [<Struct>] V2 = V2 of int*int

[<MemoryDiagnoser>]
[<RyuJitX64Job>]
type PushStream6Benchmark() =
  class

    [<Benchmark>]
    member x.Baseline() =
      // The baseline performance
      //  We expect this to do the best
      let mutable s = 0L
      for i = 0 to 10000 do
        let i = i + 1
        if (i &&& 1) = 0 then
          s <- s + int64 i
      s

    [<Benchmark>]
    member x.Linq() =
      // LINQ performance
      Enumerable.Range(0,10001).Select((+) 1).Where(fun v -> (v &&& 1) = 0).Select(int64).Sum()

    [<Benchmark>]
    member x.Array () =
      // Array performance
      Array.init 10000 id
      |> Array.map    ((+) 1)
      |> Array.filter (fun v -> (v &&& 1) = 0)
      |> Array.map    int64
      |> Array.fold   (+) 0L

    [<Benchmark>]
    member x.Seq () =
      // Seq performance
      seq { 0..10000 }
      |> Seq.map    ((+) 1)
      |> Seq.filter (fun v -> (v &&& 1) = 0)
      |> Seq.map    int64
      |> Seq.fold   (+) 0L

    [<Benchmark>]
    member x.PushStream () =
      // PushStream using |>
      ofRange   0 10000
      |> map    ((+) 1)
      |> filter (fun v -> (v &&& 1) = 0)
      |> map    int64
      |> fold   (+) 0L

    [<Benchmark>]
    member x.FasterPushStream () =
      // PushStream using |>> as it turns out that
      //  |> prevents inlining of lambdas
      ofRange     0 10000
      |>> map     ((+) 1)
      |>> filter  (fun v -> (v &&& 1) = 0)
      |>> map     int64
      |>> fold    (+) 0L

    [<Benchmark>]
    member x.PushStreamV2 () =
      ofRange   0 10000
      |> map    (fun v -> V2 (v, 0))
      |> map    (fun (V2 (v, w)) -> V2 (v + 1, w))
      |> filter (fun (V2 (v, _)) -> (v &&& 1) = 0)
      |> map    (fun (V2 (v, _)) -> int64 v)
      |> fold   (+) 0L

    [<Benchmark>]
    member x.FasterPushStreamV2 () =
      // Mor
      ofRange     0 10000
      |>> map     (fun v -> V2 (v, 0))
      |>> map     (fun (V2 (v, w)) -> V2 (v + 1, w))
      |>> filter  (fun (V2 (v, _)) -> (v &&& 1) = 0)
      |>> map     (fun (V2 (v, _)) -> int64 v)
      |>> fold    (+) 0L
  end

BenchmarkRunner.Run<PushStream6Benchmark>() |> ignore
```

## Results

On my admittedly aging machine `Benchmark.NET` reports these performance numbers.

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1348 (21H2)
Intel Core i5-3570K CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
.NET SDK=6.0.100
  [Host]    : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT DEBUG
  RyuJitX64 : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=RyuJitX64  Jit=RyuJit  Platform=X64

|             Method |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 | Allocated |
|------------------- |-----------:|----------:|----------:|------:|--------:|--------:|----------:|
|           Baseline |   6.807 us | 0.0617 us | 0.0577 us |  1.00 |    0.00 |       - |         - |
|               Linq | 148.106 us | 0.5009 us | 0.4685 us | 21.76 |    0.19 |       - |     400 B |
|              Array |  53.630 us | 0.1744 us | 0.1631 us |  7.88 |    0.08 | 44.7388 | 141,368 B |
|                Seq | 290.103 us | 0.5075 us | 0.4499 us | 42.59 |    0.34 |       - |     480 B |
|         PushStream |  34.214 us | 0.0966 us | 0.0904 us |  5.03 |    0.04 |       - |     168 B |
|   FasterPushStream |   9.011 us | 0.0231 us | 0.0216 us |  1.32 |    0.01 |       - |         - |
|       PushStreamV2 | 151.564 us | 0.3724 us | 0.3301 us | 22.25 |    0.18 |       - |     216 B |
| FasterPushStreamV2 |   9.012 us | 0.0385 us | 0.0360 us |  1.32 |    0.01 |       - |         - |
```

The imperative `Baseline` does the best as we expect.

`Linq`, `Array` and `Seq` adds significant overhead over the `Baseline`. This is because the lambda functions all are very cheap to make any overhead caused by the pipeline to be clearly visible. It doesn't mean that your code benefits in significant way by a rewrite to an imperative style over using `Seq`. If the lambda functions are expensive or the pipeline processing is a small part of your application using `Seq` is fine.

`Array` we can see allocates significant amount of memory that has to be GC:ed.

We see that `PushStream` does better thanks to PushStreams having less overhead but what's real interesting is `FasterPushStream` where `InlineIfLambda` is applied. The performance of the `PushStream` pipeline + `|>>` is very comparable to the `Baseline` and it also don't allocate any memory.

This is pretty amazing to me. Using appealing abstractions such `PushStream` with little overhead.

But what about `PushStreamV2` why does that pipeline perform so much worse than `PushStream`?

```fsharp
open PushStream

type [<Struct>] V2 = V2 of int*int
ofRange   0 10000
|> map    (fun v -> V2 (v, 0))
|> map    (fun (V2 (v, w)) -> V2 (v + 1, w))
|> filter (fun (V2 (v, _)) -> (v &&& 1) = 0)
|> map    (fun (V2 (v, _)) -> int64 v)
|> fold   (+) 0L
```

Prior to .NET6 the answer would be slow tail calls in .NET, which is why I tried this example. However looking more closely it seems with .NET6 the story of .NET tail calls seems vastly improved.

What is happening in the V2 example is that in the non-inlined version the V2 struct is passed on the stack but in the inlined version the entire V2 struct eliminated.

## Conclusion

IMHO `InlineIfLambda` is pretty awesome as it can allow us to create abstractions with little overhead where before we had to rewrite the code to an imperative style.

[Perhaps](https://github.com/dotnet/fsharp/issues/12388) F# should allow the combination of `InlineIfLambda` and `|>`. Also I would be interested to know if it's possible to combine `InlineIfLambda` with a PushStream that looks like this `type [<Struct>] 'T PushStream = PS of (('T -> bool) -> bool)` as it makes the type signatures of the PushStream module more easy to understand.

In the meantime, I wonder if the presence of `inline` and `InlineIfLambda` makes F# the best `.NET` language to write performant code in.

Merry Christmas

Mårten

## Appendix : Decompiling


Using [dnSpy](https://github.com/dnSpy/dnSpy) we can decompile the compiled IL code into C# to learn what's going on in more details

### Baseline decompiled

```csharp
public long Baseline()
{
	long s = 0L;
	for (int i = 0; i < 10001; i++)
	{
		int j = i + 1;
		if ((j & 1) == 0)
		{
			s += (long)j;
		}
	}
	return s;
}
```

The baseline not very surprisingly becomes a quite efficient loop.

### FasterPushStream decompiled

```csharp
[Benchmark]
public long FasterPushStream()
{
	long num = 0L;
	int num2 = 0;
	for (;;)
	{
		bool flag;
		if (num2 <= 10000)
		{
			int num3 = num2;
			int num4 = 1 + num3;
			if ((num4 & 1) == 0)
			{
				long num5 = (long)num4;
				num += num5;
				flag = true;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = false;
		}
		if (!flag)
		{
			break;
		}
		num2++;
	}
	bool flag2 = num2 > 10000;
	return num;
}
```

While a bit more code one can see that thanks to `inline` and `InlineIfLambda` everything is inlined into something that looks like decently efficient code. We can also spot a reason why `FasterPushStream` does a bit worse than `Baseline` and that is that the PushStream includes a short-cutting mechanism that allows the receiver to say it doesn't want to receive more values. This allows implement `First` efficiently.

### PushStream decompiled

```csharp
public long PushStream()
{
	FSharpFunc<FSharpFunc<int, bool>, bool> _instance = Program.PushStream@55.@_instance;
	FSharpFunc<FSharpFunc<int, bool>, bool> arg = new Program.PushStream@56-2(@_instance);
	FSharpFunc<FSharpFunc<long, bool>, bool> fsharpFunc = new Program.PushStream@57-4(arg);
	FSharpRef<long> fsharpRef = new FSharpRef<long>(0L);
	bool flag = fsharpFunc.Invoke(new Program.PushStream@58-6(fsharpRef));
	return fsharpRef.contents;
}
```
Using `|>` F# don't inline the lambdas and a pipeline is set up. This leads to objects being created and virtual calls for each step in the pipeline. It does surprisingly well but one big problem with this approach is that it might need to fallback to slow tail calls gives a significant performance drop.

## Appendix : Disassembling

### FasterPushStreamV2

How does the `FasterPushStreamV2` look?

```asm
.loop:
  ; Are we done?
  cmp     edx,2710h
  jg      .we_are_done
  ; (fun (V2 (v, w)) -> V2 (v + 1, w))
  lea     ecx,[rdx+1]
  ; filter  (fun (V2 (v, _)) -> (v &&& 1) = 0)
  test    cl,1
  jne     .next
  ; map (fun (V2 (v, _)) -> int64 v)
  movsxd  rcx,ecx
  ; fold (+) 0L
  add     rax,rcx
.next
  ; Increment loop variable
  inc     edx
  jmp     .loop
```

So this looks pretty amazing. `V2` and all virtual calls are completely gone.

### Baseline

```asm
.loop:
  ; Increment loop variable (smart enough to pre increment + 1)
  inc     edx
  mov     ecx,edx
  ; filter  (fun (V2 (v, _)) -> (v &&& 1) = 0)
  test    cl,1
  jne     .next
  ; map (fun (V2 (v, _)) -> int64 v)
  movsxd  rcx,ecx
  add     rax,rcx
.next
  ; Increment loop variable
  cmp     edx,2711h
  jl      .loop
```

Here the jitter was smart enough to pre increment with 1 to avoid incrementing by 1 each loop. In addition, checks the loop condition at the end saves a jmp.

Still, `FasterPushStreamV2` is not far off!

###

One can't tell in the decompiled C# code or the IL code that a slow tail call is applied but one can see that F# instructs the JIT:er to use tail calls if possible.

```asm
IL_000C: tail.
IL_000E: callvirt  instance !1 class [FSharp.Core]Microsoft.FSharp.Core.FSharpFunc`2<class [FSharp.Core]Microsoft.FSharp.Core.FSharpFunc`2<int32, bool>, bool>::Invoke(!0)
IL_0013: ret
```

When invoking the next receiver in the PushStream the F# compiler emits `tail.` attribute.

### Dissassembling fast tail calls

```asm
; ofRange 0 10000
.loop:
  ; Are we done?
  cmp     edi,2710h
  jg      .we_are_done
  ; Setup virtual call to first receiver in the PushStrema
  mov     rcx,rsi
  mov     edx,edi
  mov     rax,qword ptr [rsi]
  mov     rax,qword ptr [rax+40h]
  ; Do non tail call to receiver
  call    qword ptr [rax+20h]
  ; Does the receiver think we should stop?
  test    eax,eax
  je      .we_are_done
  ; Increment loop variable
  inc     edi
  jmp     .loop

; map    ((+) 1)
  mov     rcx,qword ptr [rcx+8]
  ; Increment input
  inc     edx
  ; Setup virtual call to second receiver in the PushStream
  mov     rax,qword ptr [rcx]
  mov     rax,qword ptr [rax+40h]
  mov     rax,qword ptr [rax+20h]
  ; Fast tail call, just a simple jmp
  jmp     rax

; filter (fun v -> (v &&& 1) = 0)
  ; Is input odd?
  test    dl,1
  jne     .bail_out
  ; No it was even
  ; Setup virtual call to third receiver in the PushStream
  mov     rcx,qword ptr [rcx+8]
  mov     rax,qword ptr [rcx]
  mov     rax,qword ptr [rax+40h]
  mov     rax,qword ptr [rax+20h]
  ; Fast tail call, just a simple jmp
  jmp     rax
.bail_out:
  ; No it was odd
  ; Set eax to 1 (true) to continue looping
  mov     eax,1
  ; Return to ofRange loop
  ret

; map int64
  mov     rcx,qword ptr [rcx+8]
  ; Extend to 64 bits
  movsxd  rdx,edx
  ; Setup virtual call to forth receiver in the PushStream
  mov     rax,qword ptr [rcx]
  mov     rax,qword ptr [rax+40h]
  mov     rax,qword ptr [rax+20h]
  ; Fast tail call, just a simple jmp
  jmp     rax

; fold   (+) 0L
  mov     rax,qword ptr [rcx+8]
  mov     rcx,rax
  ; Add state to value
  add     rdx,qword ptr [rax+8]
  ; Store back state
  mov     qword ptr [rcx+8],rdx
  ; Set eax to 1 (true) to continue looping
  mov     eax,1
  ; Return to ofRange loop
  ret
```

Lots of virtual calls in the code but at least it is fast tail calls. What happens when we force slow tail calls by using `V2`. V2 can't be stored in a single register forcing a stack frame to pass the value in.

Let's look at what happens when slow tail calls are used

```asm
; ofRange 0 10000
.loop:
  ; Are we done?
  cmp     edi,2710h
  jg      .we_are_done
  ; Setup virtual call to first receiver in the PushStrema
  mov     rcx,rsi
  mov     edx,edi
  mov     rax,qword ptr [rsi]
  mov     rax,qword ptr [rax+40h]
  ; Do non tail call to receiver
  call    qword ptr [rax+20h]
  ; Does the receiver think we should stop?
  test    eax,eax
  je      .we_are_done
  ; Increment loop variable
  inc     edi
  jmp     .loop

; map (fun v -> V2 (v, 0))
  push    rax
  mov     rcx,qword ptr [rcx+8]
  xor     eax,eax
  mov     dword ptr [rsp],edx
  mov     dword ptr [rsp+4],eax
  mov     rdx,qword ptr [rsp]
  mov     rax,qword ptr [rcx]
  mov     rax,qword ptr [rax+40h]
  mov     rax,qword ptr [rax+20h]
  add     rsp,8
  jmp     rax

; (fun (V2 (v, w)) -> V2 (v + 1, w))
  push    rax
  mov     qword ptr [rsp+18h],rdx
  mov     rcx,qword ptr [rcx+8]
  mov     edx,dword ptr [rsp+18h]
  inc     edx
  mov     eax,dword ptr [rsp+1Ch]
  mov     dword ptr [rsp],edx
  mov     dword ptr [rsp+4],eax
  mov     rdx,qword ptr [rsp]
  mov     rax,qword ptr [rcx]
  mov     rax,qword ptr [rax+40h]
  mov     rax,qword ptr [rax+20h]
  add     rsp,8
  jmp     rax


; filter  (fun (V2 (v, _)) -> (v &&& 1) = 0)
  ; Read the this pointer for V2 but it seems never used
  mov     qword ptr [rsp+10h],rdx
  ; Test to see if the number is odd
  test    byte ptr [rsp+10h],1
  jne     .bail_out
  ; No it's even
  mov     rcx,qword ptr [rcx+8]
  mov     rdx,qword ptr [rsp+10h]
  mov     rax,qword ptr [rcx]
  mov     rax,qword ptr [rax+40h]
  mov     rax,qword ptr [rax+20h]
  ; Use fast tail call as no stack frame was necessary
  jmp     rax
.bail_out:
  ; No it was odd
  ; Set eax to 1 (true) to continue looping
  mov     eax,1
  ; Return to ofRange loop
  ret

; map (fun (V2 (v, _)) -> int64 v)
  mov     qword ptr [rsp+10h],rdx ss:0000005f`3077e888=0000000000001d52
  mov     rcx,qword ptr [rcx+8]
  mov     edx,dword ptr [rsp+10h]
  movsxd  rdx,edx
  mov     rax,qword ptr [rcx]
  mov     rax,qword ptr [rax+40h]
  mov     rax,qword ptr [rax+20h]
  jmp     rax

; fold (+) 0L
  mov     rax,qword ptr [rcx+8]
  mov     rcx,rax
  add     rdx,qword ptr [rax+8]
  mov     qword ptr [rcx+8],rdx
  mov     eax,1
  ret
```

