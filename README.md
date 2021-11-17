# PushStream6
Push Stream for F#6


## Justification

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1348 (21H2)
Intel Core i5-3570K CPU 3.40GHz (Ivy Bridge), 1 CPU, 4 logical and 4 physical cores
.NET SDK=6.0.100
  [Host]    : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT DEBUG
  RyuJitX64 : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

Job=RyuJitX64  Jit=RyuJit  Platform=X64

|             Method |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 | Allocated |
|------------------- |-----------:|----------:|----------:|------:|--------:|--------:|----------:|
|           Baseline |   6.808 us | 0.0515 us | 0.0482 us |  1.00 |    0.00 |       - |         - |
|               Linq | 149.261 us | 0.2857 us | 0.2672 us | 21.93 |    0.15 |       - |     400 B |
|          ValueLinq |  84.252 us | 0.2420 us | 0.2145 us | 12.38 |    0.08 |       - |     192 B |
|      ValueLinqFast |  19.398 us | 0.0425 us | 0.0397 us |  2.85 |    0.02 |       - |         - |
|              Array | 118.906 us | 0.5686 us | 0.5318 us | 17.47 |    0.15 | 86.1816 | 272,864 B |
|                Seq | 285.724 us | 0.9978 us | 0.9333 us | 41.97 |    0.33 |       - |     480 B |
|         PushStream |  34.634 us | 0.1401 us | 0.1311 us |  5.09 |    0.03 |       - |     168 B |
|   FasterPushStream |   9.078 us | 0.0579 us | 0.0513 us |  1.33 |    0.01 |       - |         - |
|       PushStreamV2 | 153.431 us | 0.6464 us | 0.6047 us | 22.54 |    0.19 |       - |     216 B |
| FasterPushStreamV2 |   9.085 us | 0.0624 us | 0.0583 us |  1.33 |    0.01 |       - |         - |
```
