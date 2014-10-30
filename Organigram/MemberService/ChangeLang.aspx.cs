using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberService
{
  public partial class ChangeLang : CustomPage
  {
    protected override string PageName
    {
      get { return "ChangeLang"; }
    }
    
    private DropDownList _languageList;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;

      var table = new Table();
      table.AddHeaderRow(Resources.ChangeLanguage, 2);
      table.AddVerticalSpace(10);

      _languageList = new DropDownList();
      foreach (var language in Language.Values)
      {
        _languageList.Items.Add(new ListItem(language.Display, language.Value));
      }
      _languageList.SelectedValue = user.PreferredLanguage;
      table.AddRow(LdapResources.Country, _languageList);

      var buttonPanel = new Panel();

      var saveButton = new Button();
      saveButton.Text = Resources.Save;
      saveButton.Click += new EventHandler(SaveButton_Click);
      buttonPanel.Controls.Add(saveButton);

      var cancelButton = new Button();
      cancelButton.Text = Resources.Cancel;
      cancelButton.Click += new EventHandler(CancelButton_Click);
      buttonPanel.Controls.Add(cancelButton);

      table.AddRow(string.Empty, buttonPanel);

      this.panel.Controls.Add(table);
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      bool valid = true;

      if (valid)
      {
        if (!SetupLdap())
        {
          RedirectLogin();
          return;
        }

        var user = CurrentUser;

        user.PreferredLanguage = _languageList.SelectedValue;

        user.Modify(Connection);

        BasicGlobal.Logger.Log(LogLevel.Info, "User id {0} name {1} changed preferred language to {2}", user.Id, user.Name, user.PreferredLanguage);

        RedirectHome();
      }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}