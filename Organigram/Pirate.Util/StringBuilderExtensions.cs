using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Text
{
  public static class StringBuilderExtensions
  {
    public static void AppendLine(this StringBuilder builder, string format, params object[] arguments)
    {
      builder.AppendLine(string.Format(format, arguments));
    }
  }
}