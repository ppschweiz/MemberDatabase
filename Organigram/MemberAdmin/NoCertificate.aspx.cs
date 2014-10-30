using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Web;

namespace MemberAdmin
{
  public partial class NoCertificate : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      this.loginLink.NavigateUrl = BasicGlobal.Config.MemberServiceAddress;
    }
  }
}