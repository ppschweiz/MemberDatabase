using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Pirate.Web
{
  public class KeyGen : WebControl
  {
    public string KeyData
    {
      get { return Context.Request.Form.Get(ID); }
    }

    protected override void RenderContents(HtmlTextWriter writer)
    {
      writer.Write("<keygen name=\"" + ID + "\"/>");
    }
  }
}
