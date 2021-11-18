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
|           Baseline |   6.807 us | 0.0617 us | 0.0577 us |  1.00 |    0.00 |       - |         - |
|               Linq | 148.106 us | 0.5009 us | 0.4685 us | 21.76 |    0.19 |       - |     400 B |
|          ValueLinq |  83.283 us | 0.1882 us | 0.1761 us | 12.23 |    0.11 |       - |     192 B |
|      ValueLinqFast |  19.165 us | 0.0660 us | 0.0618 us |  2.82 |    0.03 |       - |         - |
|              Array |  53.630 us | 0.1744 us | 0.1631 us |  7.88 |    0.08 | 44.7388 | 141,368 B |
|                Seq | 290.103 us | 0.5075 us | 0.4499 us | 42.59 |    0.34 |       - |     480 B |
|         PushStream |  34.214 us | 0.0966 us | 0.0904 us |  5.03 |    0.04 |       - |     168 B |
|   FasterPushStream |   9.011 us | 0.0231 us | 0.0216 us |  1.32 |    0.01 |       - |         - |
|       PushStreamV2 | 151.564 us | 0.3724 us | 0.3301 us | 22.25 |    0.18 |       - |     216 B |
| FasterPushStreamV2 |   9.012 us | 0.0385 us | 0.0360 us |  1.32 |    0.01 |       - |         - |
```
