using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
  public class Tuple<T1, T2, T3, T4, T5>
  {
    public T1 Item1 { get; private set; }

    public T2 Item2 { get; private set; }

    public T3 Item3 { get; private set; }

    public T4 Item4 { get; private set; }

    public T5 Item5 { get; private set; }

    public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
    {
      Item1 = item1;
      Item2 = item2;
      Item3 = item3;
      Item4 = item4;
      Item5 = item5;
    }
  }

  public class Tuple<T1, T2, T3, T4>
  {
    public T1 Item1 { get; private set; }

    public T2 Item2 { get; private set; }

    public T3 Item3 { get; private set; }

    public T4 Item4 { get; private set; }

    public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
    {
      Item1 = item1;
      Item2 = item2;
      Item3 = item3;
      Item4 = item4;
    }
  }

  public class Tuple<T1, T2, T3>
  {
    public T1 Item1 { get; private set; }

    public T2 Item2 { get; private set; }

    public T3 Item3 { get; private set; }

    public Tuple(T1 item1, T2 item2, T3 item3)
    {
      Item1 = item1;
      Item2 = item2;
      Item3 = item3;
    }
  }

  public class Tuple<T1, T2>
  {
    public T1 Item1 { get; private set; }

    public T2 Item2 { get; private set; }

    public Tuple(T1 item1, T2 item2)
    {
      Item1 = item1;
      Item2 = item2;
    }
  }

  public class StringPair : Tuple<string, string>
  {
    public StringPair(string item1, string item2)
      : base(item1, item2)
    { }
  }
}
