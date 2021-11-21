# PushStream6
Push Stream for F#6


## Justification

### 10000 iterations

```
|             Method | Job |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 | Allocated |
|------------------- |---- |-----------:|----------:|----------:|------:|--------:|--------:|----------:|
|           Baseline | STD |   6.674 μs | 0.0613 μs | 0.0574 μs |  1.00 |    0.00 |       - |         - |
|               Linq | STD | 143.922 μs | 0.1871 μs | 0.1659 μs | 21.57 |    0.19 |       - |     400 B |
|          ValueLinq | STD |  81.678 μs | 0.0974 μs | 0.0863 μs | 12.24 |    0.10 |       - |     192 B |
|      ValueLinqFast | STD |  18.784 μs | 0.0197 μs | 0.0184 μs |  2.81 |    0.02 |       - |         - |
|              Array | STD |  52.400 μs | 0.1745 μs | 0.1633 μs |  7.85 |    0.06 | 44.7388 | 141,368 B |
|                Seq | STD | 273.545 μs | 0.3869 μs | 0.3231 μs | 40.99 |    0.37 |       - |     480 B |
|         PushStream | STD |  33.492 μs | 0.0263 μs | 0.0233 μs |  5.02 |    0.05 |       - |     168 B |
|   FasterPushStream | STD |   8.830 μs | 0.0402 μs | 0.0376 μs |  1.32 |    0.01 |       - |         - |
|       PushStreamV2 | STD | 148.458 μs | 0.2082 μs | 0.1947 μs | 22.25 |    0.20 |       - |     216 B |
| FasterPushStreamV2 | STD |   8.846 μs | 0.0284 μs | 0.0266 μs |  1.33 |    0.01 |       - |         - |
|                    |     |            |           |           |       |         |         |           |
|           Baseline | PGO |   6.743 μs | 0.0770 μs | 0.0720 μs |  1.00 |    0.00 |       - |         - |
|               Linq | PGO |  86.903 μs | 0.1562 μs | 0.1461 μs | 12.89 |    0.14 |  0.1221 |     400 B |
|          ValueLinq | PGO |  81.627 μs | 0.1038 μs | 0.0921 μs | 12.09 |    0.13 |       - |     192 B |
|      ValueLinqFast | PGO |  18.767 μs | 0.0136 μs | 0.0121 μs |  2.78 |    0.03 |       - |         - |
|              Array | PGO |  41.716 μs | 0.1588 μs | 0.1485 μs |  6.19 |    0.07 | 44.7388 | 141,368 B |
|                Seq | PGO | 142.685 μs | 0.5470 μs | 0.5117 μs | 21.16 |    0.27 |       - |     480 B |
|         PushStream | PGO |  34.763 μs | 0.1340 μs | 0.1253 μs |  5.16 |    0.06 |       - |     168 B |
|   FasterPushStream | PGO |   8.784 μs | 0.0568 μs | 0.0531 μs |  1.30 |    0.02 |       - |         - |
|       PushStreamV2 | PGO | 146.289 μs | 0.5811 μs | 0.5436 μs | 21.70 |    0.26 |       - |     216 B |
| FasterPushStreamV2 | PGO |   8.761 μs | 0.0630 μs | 0.0589 μs |  1.30 |    0.02 |       - |         - |
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
