using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberService
{
  public partial class ChangePassword : CustomPage
  {
    protected override string PageName
    {
      get { return "ChangePassword"; }
    }
    
    private TextBox _oldPasswordBox;
    private Label _oldPasswordValid;
    private TextBox _newPassword1Box;
    private Label _newPassword1Valid;
    private TextBox _newPassword2Box;
    private Label _newPassword2Valid;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;

      var table = new Table();
      table.AddHeaderRow(Resources.ChangePassword, 2);
      table.AddVerticalSpace(10);

      _oldPasswordBox = new TextBox();
      _oldPasswordBox.TextMode = TextBoxMode.Password;
      _oldPasswordValid = new Label();
      table.AddRow(Resources.OldPassword, _oldPasswordBox, _oldPasswordValid);
      table.AddVerticalSpace(10);

      table.AddRow(string.Empty, Resources.PasswordLength);
      _newPassword1Box = new TextBox();
      _newPassword1Box.TextMode = TextBoxMode.Password;
      _newPassword1Valid = new Label();
      table.AddRow(Resources.NewPassword, _newPassword1Box, _newPassword1Valid);

      _newPassword2Box = new TextBox();
      _newPassword2Box.TextMode = TextBoxMode.Password;
      _newPassword2Valid = new Label();
      table.AddRow(Resources.RepeatPassword, _newPassword2Box, _newPassword2Valid);

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

      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;

      if (!Ldap.TestLogin(Request, user.Nickname, _oldPasswordBox.Text))
      {
        _oldPasswordValid.Text = Resources.PasswordWrong;
        valid = false;
      }
      else
      {
        _oldPasswordValid.Text = string.Empty;
      }

      if (_newPassword1Box.Text.Length < 8)
      {
        _newPassword1Valid.Text = Resources.PasswordTooShort;
        valid = false;
      }
      else
      {
        _newPassword1Valid.Text = string.Empty;
      }

      if (_newPassword2Box.Text != _newPassword1Box.Text)
      {
        _newPassword2Valid.Text = Resources.PasswordNotMatch;
        valid = false;
      }
      else
      {
        _newPassword2Valid.Text = string.Empty;
      }

      if (valid)
      {
        Connection.SetPassword(user.DN, _newPassword1Box.Text);
        BasicGlobal.Logger.Log(LogLevel.Info, "User id {0} name {1} changed password", user.Id, user.Name);
        RedirectHome();
      }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}