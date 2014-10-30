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
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberService
{
  public partial class ConfirmChangeEmail : CustomPage
  {
    protected override string PageName
    {
      get { return "ConfirmChangeEmail"; }
    }
    
    private TextBox _passwordBox;
    private Label _passwordValid;
    private EmailAddressChange _change;

    protected void Page_Load(object sender, EventArgs e)
    {
      string id = Request.Params["id"];

      var table = new Table();
      table.AddHeaderRow(Resources.ChangeEmail, 2);
      table.AddVerticalSpace(10);

      if (string.IsNullOrEmpty(id))
      {
        table.AddRow(Resources.EmailAddressChangeUnknown);
        var link = new HyperLink();
        link.NavigateUrl = "Default.aspx";
        link.Text = Resources.Home;
        table.AddRow(link);
      }
      else
      {
        DataGlobal.DataAccess.RemoveOutdatedEmailAddressChanges();

        _change = DataGlobal.DataAccess.GetEmailAddressChange(id);

        if (_change != null)
        {
          table.AddRow(LdapResources.Username, _change.Username);
          table.AddRow(LdapResources.Email, _change.AddAddress);
          table.AddVerticalSpace(10);

          _passwordBox = new TextBox();
          _passwordBox.TextMode = TextBoxMode.Password;
          _passwordValid = new Label();
          table.AddRow(string.Empty, Resources.EmailRequiresPassword);
          table.AddRow(LdapResources.Password, _passwordBox, _passwordValid);

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
        }
        else
        {
          table.AddRow(Resources.EmailAddressChangeUnknown);
          var link = new HyperLink();
          link.NavigateUrl = "Default.aspx";
          link.Text = Resources.Home;
          table.AddRow(link);
        }
      }

      this.panel.Controls.Add(table);
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      if (Ldap.Login(Application, Session, Request, _change.Username, _passwordBox.Text))
      {
        SetupLdap();
        var user = CurrentUser;

        var report = new StringBuilder();
        report.AppendLine(string.Format("User id {0} name {1} changed his data.", user.Id, user.Name));

        if (!string.IsNullOrEmpty(_change.RemoveAddress) && user.Emails.Contains(_change.RemoveAddress))
        {
          user.Emails.Remove(_change.RemoveAddress);
          report.AppendLine("Removed email address " + _change.RemoveAddress);
        }

        user.Emails.Add(_change.AddAddress);
        user.Modify(Connection);
        DataGlobal.DataAccess.RemoveEmailAddressChange(_change.Id);

        report.AppendLine("Added email address " + _change.AddAddress);
        BasicGlobal.Logger.Log(LogLevel.Info, report.ToString()); 
        BasicGlobal.Mailer.SendToMembersQueue("User data changed", report.ToString());

        RedirectHome();
      }
      else
      {
        _passwordValid.Text = Resources.PasswordWrong;
      }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}