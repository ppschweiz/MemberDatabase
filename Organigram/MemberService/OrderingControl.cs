using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MemberService
{
  /// <summary>
  /// Control for the ordering of a group.
  /// </summary>
  public class OrderingControl : DropDownList
  {
    public OrderingControl()
    {
      for (int i = -1; i < 30; i++)
      {
        Items.Add(new ListItem(i.ToString(), i.ToString()));
      }
    }

    public int Ordering
    {
      get { return Convert.ToInt32(SelectedValue); }
      set { SelectedValue = value.ToString(); }
    }
  }
}