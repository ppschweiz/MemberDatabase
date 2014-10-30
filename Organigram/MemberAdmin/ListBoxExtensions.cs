using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web.UI.WebControls
{
  public static class ListBoxExtensions
  {
    public static IEnumerable<string> GetSelectedValues(this ListBox box, HttpRequest request)
    {
      var selected = request.Params[box.ID] as string;

      if (!string.IsNullOrEmpty(selected))
      {
        return selected.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
      }
      else
      { 
        return new string[0];
      }
    }
  }
}