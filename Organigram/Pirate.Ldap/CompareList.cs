using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public abstract class CompareList : IComparable<CompareList>, IComparable
  {
    public abstract int CompareTo(CompareList other);

    public abstract int CompareTo(object obj);
  }

  public class CompareList<T> : CompareList
    where T : IComparable
  {
    private List<T> _list;

    public IEnumerable<T> Values { get { return _list; } }

    public CompareList(IEnumerable<T> list)
    {
      _list = new List<T>(list);
    }

    public override int CompareTo(CompareList other)
    {
      var typedList = other as CompareList<T>;

      if (typedList != null)
      {
        var current = new Queue<T>(_list.OrderBy(x => x));
        var compare = new Queue<T>(typedList._list.OrderBy(x => x));

        while (current.Count > 0 &&
               compare.Count > 0)
        {
          var a = current.Dequeue();
          var b = compare.Dequeue();
          var r = a.CompareTo(b);

          if (r != 0)
          {
            return r;
          }
        }

        if (current.Count == 0 &&
            compare.Count == 0)
        {
          return 0;
        }
        else if (current.Count > 0)
        {
          return 1;
        }
        else
        {
          return -1;
        }
      }
      else
      {
        return -1;
      }
    }

    public override int CompareTo(object obj)
    {
      var list = obj as CompareList;

      if (list != null)
      {
        return CompareTo(list);
      }
      else
      {
        return -1;
      }
    }
  }
}
