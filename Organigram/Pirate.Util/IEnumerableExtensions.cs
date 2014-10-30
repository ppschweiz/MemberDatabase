using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
  /// <summary>
  /// Extensions to the IEnumerable.
  /// </summary>
  public static class IEnumerableExtensions
  {
    /// <summary>
    /// Determines whether the specified ordered list A has intersection with list B.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="orderedListA">The ordered list A.</param>
    /// <param name="orderedListB">The ordered list B.</param>
    /// <returns>
    ///   <c>true</c> if the specified ordered list A has intersection; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasIntersection<T>(this IEnumerable<T> orderedListA, IEnumerable<T> orderedListB)
      where T : IComparable
    {
      var listA = orderedListA.ToArray();
      var listB = orderedListB.ToArray();
      var indexA = 0;
      var indexB = 0;

      while (indexA < listA.Length &&
             indexB < listB.Length)
      {
        var comp = listA[indexA].CompareTo(listA[indexB]);

        if (comp > 0)
        {
          indexB++;
        }
        else if (comp < 0)
        {
          indexA++;
        }
        else
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Gets the maximum from the list or, if empty, the default value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list.</param>
    /// <param name="selector">The selector.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    public static int MaxOrDefault<T>(this IEnumerable<T> list, Func<T, int> selector, int defaultValue)
    {
      if (list.Count() > 0)
      {
        return list.Max<T, int>(selector);
      }
      else
      {
        return 0;
      }
    }

    public static bool AreEqual<T>(this IEnumerable<T> listA, IEnumerable<T> listB)
    {
      return
        listA.All(x => listB.Contains(x)) &&
        listB.All(x => listA.Contains(x));
    }
  }
}
