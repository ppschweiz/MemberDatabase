using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MemberAdmin
{
  public partial class Default : CustomPage
  {
    protected override string PageName
    {
      get { return "Default"; }
    }
    
    protected void Page_Load(object sender, EventArgs e)
    {
      Redirect("Login.aspx");
    }
  }
}