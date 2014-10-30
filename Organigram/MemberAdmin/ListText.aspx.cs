using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public partial class ListText : CustomPage
  {
    protected override string PageName
    {
      get { return "ListText"; }
    }
    
    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel < 2)
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "ListText.Load user id {0} name {1} has authorization level {2} meaning insufficient access.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel);
        RedirectAccessDenied();
        return;
      }

      var table = new Table();

      table.AddHeaderRow(Resources.ListText);
      table.AddVerticalSpace(10);

      foreach (var item in Texts.GetTexts().OrderBy(i => i.Id))
      {
        var editLink = new HyperLink();
        editLink.Text = item.Id;
        editLink.NavigateUrl = "EditText.aspx?id=" + item.Id;
        table.AddRow(editLink);
      }

      table.AddVerticalSpace(20);

      var buttonPanel = new Panel();

      Button backbutton = new Button();
      backbutton.Text = Resources.Back;
      backbutton.Click += new EventHandler(Backbutton_Click);
      buttonPanel.Controls.Add(backbutton); 
      
      table.AddRow(buttonPanel);

      this.panel.Controls.Add(table);
    }

    private void Backbutton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}