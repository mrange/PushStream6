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

|             Method |       Mean |     Error |    StdDev |   Gen 0 | Allocated |
|------------------- |-----------:|----------:|----------:|--------:|----------:|
|           Baseline |   6.857 us | 0.1311 us | 0.1226 us |       - |         - |
|               Linq | 147.636 us | 0.2791 us | 0.2610 us |       - |     400 B |
|              Array | 117.343 us | 1.0478 us | 0.9801 us | 86.1816 | 272,864 B |
|                Seq | 291.286 us | 1.2606 us | 1.1175 us |       - |     480 B |
|         PushStream |  34.249 us | 0.0627 us | 0.0586 us |       - |     168 B |
|   FasterPushStream |   9.034 us | 0.0203 us | 0.0190 us |       - |         - |
|       PushStreamV2 | 151.888 us | 0.2686 us | 0.2513 us |       - |     216 B |
| FasterPushStreamV2 |   9.029 us | 0.0180 us | 0.0168 us |       - |         - |
```
