namespace PushStream6.CsPushStream
{
  public delegate bool Receiver<in T>(T v);
  public delegate bool PushStream<out T>(Receiver<T> r);

  public static class PushStream
  {
    public static PushStream<int> Range(int b, int c) => r =>
    {
      var e = b + c;
      for (int i = b; i < e; ++i)
      {
        if (!r(i)) return false;
      }
      return true;
    };

    public static PushStream<T> FromArray<T>(T[] vs) => r =>
    {
      foreach (var v in vs)
      {
        if (!r(v)) return false;
      }
      return true;
    };

    public static PushStream<T> Where<T>(this PushStream<T> ps, Func<T, bool> f) => r =>
      ps(v => f(v) ? r(v) : true);

    public static PushStream<U> Select<T, U>(this PushStream<T> ps, Func<T, U> f) => r =>
      ps(v => r(f(v)));

    public static S Aggregate<T, S>(this PushStream<T> ps, Func<S, T, S> f, S z)
    {
      var s = z;
      ps(v => { s = f(s, v); return true; });
      return s;
    }

    public static T [] ToArray<T>(this PushStream<T> ps)
    {
      var s = new List<T>(16);
      ps(v => { s.Add(v); return true; });
      return s.ToArray();
    }
  }
}
