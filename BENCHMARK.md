# PushStream6
Push Stream for F#6


## Justification

### 10000 iterations

```
|             Method | Job |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 | Allocated |
|------------------- |---- |-----------:|----------:|----------:|------:|--------:|--------:|----------:|
|           Baseline | PGO |   6.666 μs | 0.1036 μs | 0.0969 μs |  1.00 |    0.00 |       - |         - |
|               Linq | PGO |  88.306 μs | 1.3255 μs | 1.2398 μs | 13.25 |    0.25 |  0.1221 |     400 B |
|          ValueLinq | PGO |  81.901 μs | 0.7857 μs | 0.7349 μs | 12.29 |    0.18 |       - |     193 B |
|      ValueLinqFast | PGO |  18.728 μs | 0.0441 μs | 0.0413 μs |  2.81 |    0.04 |       - |         - |
|              Array | PGO |  41.591 μs | 0.1992 μs | 0.1664 μs |  6.25 |    0.09 | 44.7388 | 141,368 B |
|                Seq | PGO | 142.978 μs | 0.2711 μs | 0.2403 μs | 21.48 |    0.32 |       - |     480 B |
|         PushStream | PGO |  36.119 μs | 0.0912 μs | 0.0853 μs |  5.42 |    0.08 |       - |     168 B |
|   FasterPushStream | PGO |   8.786 μs | 0.0531 μs | 0.0497 μs |  1.32 |    0.02 |       - |         - |
|       PushStreamV2 | PGO | 146.144 μs | 0.3864 μs | 0.3226 μs | 21.95 |    0.33 |       - |     216 B |
| FasterPushStreamV2 | PGO |   8.776 μs | 0.0387 μs | 0.0362 μs |  1.32 |    0.02 |       - |         - |
|   FasterPumpStream | PGO |  17.901 μs | 0.0537 μs | 0.0503 μs |  2.69 |    0.04 |       - |      80 B |
| FasterPumpStreamV2 | PGO |  17.778 μs | 0.0545 μs | 0.0510 μs |  2.67 |    0.04 |       - |      80 B |
|                    |     |            |           |           |       |         |         |           |
|           Baseline | STD |   5.741 μs | 0.1133 μs | 0.2071 μs |  1.00 |    0.00 |       - |         - |
|               Linq | STD | 144.320 μs | 0.5485 μs | 0.5131 μs | 25.40 |    0.67 |       - |     400 B |
|          ValueLinq | STD |  81.331 μs | 0.2316 μs | 0.2167 μs | 14.32 |    0.38 |       - |     192 B |
|      ValueLinqFast | STD |  18.748 μs | 0.0617 μs | 0.0578 μs |  3.30 |    0.09 |       - |         - |
|              Array | STD |  52.019 μs | 0.2000 μs | 0.1773 μs |  9.18 |    0.24 | 44.7388 | 141,368 B |
|                Seq | STD | 263.660 μs | 0.8360 μs | 0.7820 μs | 46.41 |    1.19 |       - |     480 B |
|         PushStream | STD |  33.362 μs | 0.1057 μs | 0.0989 μs |  5.87 |    0.16 |       - |     168 B |
|   FasterPushStream | STD |   8.457 μs | 0.0523 μs | 0.0489 μs |  1.49 |    0.04 |       - |         - |
|       PushStreamV2 | STD | 148.101 μs | 0.4756 μs | 0.4448 μs | 26.07 |    0.69 |       - |     216 B |
| FasterPushStreamV2 | STD |   8.819 μs | 0.0561 μs | 0.0524 μs |  1.55 |    0.04 |       - |         - |
|   FasterPumpStream | STD |  22.718 μs | 0.0901 μs | 0.0842 μs |  4.00 |    0.11 |       - |      80 B |
| FasterPumpStreamV2 | STD |  22.718 μs | 0.0746 μs | 0.0697 μs |  4.00 |    0.10 |       - |      80 B |
```

### 10 iterations

```
|             Method | Job |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
|------------------- |---- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|
|           Baseline | STD |   8.535 ns | 0.0429 ns | 0.0402 ns |  1.00 |    0.00 |      - |         - |
|               Linq | STD | 280.146 ns | 0.5770 ns | 0.5115 ns | 32.82 |    0.16 | 0.1273 |     400 B |
|          ValueLinq | STD | 178.311 ns | 0.7306 ns | 0.6834 ns | 20.89 |    0.08 | 0.0610 |     192 B |
|      ValueLinqFast | STD |  73.944 ns | 0.2865 ns | 0.2539 ns |  8.66 |    0.04 |      - |         - |
|              Array | STD | 101.660 ns | 0.5682 ns | 0.5315 ns | 11.91 |    0.08 | 0.0764 |     240 B |
|                Seq | STD | 464.694 ns | 2.1452 ns | 2.0066 ns | 54.44 |    0.27 | 0.1526 |     480 B |
|         PushStream | STD |  73.830 ns | 0.1123 ns | 0.0938 ns |  8.65 |    0.04 | 0.0535 |     168 B |
|   FasterPushStream | STD |  10.465 ns | 0.0363 ns | 0.0339 ns |  1.23 |    0.01 |      - |         - |
|       PushStreamV2 | STD | 200.468 ns | 0.6664 ns | 0.5907 ns | 23.48 |    0.13 | 0.0687 |     216 B |
| FasterPushStreamV2 | STD |  10.410 ns | 0.0102 ns | 0.0090 ns |  1.22 |    0.01 |      - |         - |
|                    |     |            |           |           |       |         |        |           |
|           Baseline | PGO |  10.761 ns | 0.0805 ns | 0.0714 ns |  1.00 |    0.00 |      - |         - |
|               Linq | PGO | 208.911 ns | 0.8347 ns | 0.7808 ns | 19.42 |    0.15 | 0.1273 |     400 B |
|          ValueLinq | PGO | 179.789 ns | 0.5231 ns | 0.4893 ns | 16.71 |    0.13 | 0.0610 |     192 B |
|      ValueLinqFast | PGO |  73.129 ns | 0.8426 ns | 0.7882 ns |  6.81 |    0.06 |      - |         - |
|              Array | PGO |  81.717 ns | 0.3463 ns | 0.2892 ns |  7.59 |    0.06 | 0.0764 |     240 B |
|                Seq | PGO | 320.859 ns | 1.3163 ns | 1.2313 ns | 29.82 |    0.20 | 0.1526 |     480 B |
|         PushStream | PGO |  72.503 ns | 0.1660 ns | 0.1386 ns |  6.73 |    0.04 | 0.0535 |     168 B |
|   FasterPushStream | PGO |  10.894 ns | 0.0398 ns | 0.0372 ns |  1.01 |    0.01 |      - |         - |
|       PushStreamV2 | PGO | 198.294 ns | 0.7521 ns | 0.7035 ns | 18.43 |    0.11 | 0.0687 |     216 B |
| FasterPushStreamV2 | PGO |  10.417 ns | 0.0547 ns | 0.0511 ns |  0.97 |    0.01 |      - |         - |
```
