using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util;

namespace MemberService
{
  public partial class DisplayMessage : CustomPage
  {
    public const string SessionDataKey = "message";

    protected override string PageName
    {
      get { return "MemberService"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var table = new Table();

      var message = Session[SessionDataKey] as Message;

      if (message != null)
      {
        var user = CurrentUser;
        var preText = Texts.GetText(DataGlobal.DataAccess, Texts.GetItem(message.TextId), user);
        var text = Pirate.Textile.TextParser.Parse(preText.Item2);
        table.AddRow(text.ToHtml());

        table.AddVerticalSpace(20);

        var link = new HyperLink();
        link.Text = Resources.Ok;
        link.NavigateUrl = message.ContinueTo;
        table.AddRow(link);
      }
      else
      {
        RedirectHome();
      }

      this.panel.Controls.Add(table);
    }
  }
}