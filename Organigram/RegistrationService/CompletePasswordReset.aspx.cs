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
using Pirate.Util.Logging;

namespace RegistrationService
{
  public partial class CompletePasswordReset : CustomPage
  {
    private TextBox _passwordBox;

    private TextBox _repeatBox;

    private Label _infoLabel;

    private Button _okButton;

    private HyperLink _doneLink;

    protected override string PageName
    {
      get { return "CompletePasswordReset"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      var table = new Table();
      table.AddHeaderRow(Resources.PasswordReset, 2);
      table.AddVerticalSpace(10);

      var ciphertext = (Request.Params["data"] ?? string.Empty).TryParseHexBytes();
      var plaintext = ciphertext == null ? null : BasicGlobal.Config.DeSecure(ciphertext);
      var reset = plaintext == null ? null : PasswordReset.FromBinary(plaintext);

      if (reset != null &&
          DateTime.Now <= reset.Created.AddMinutes(15))
      {
        _passwordBox = new TextBox();
        _passwordBox.TextMode = TextBoxMode.Password;
        _passwordBox.Width = new Unit(10, UnitType.Em);
        _passwordBox.Enabled = true;
        table.AddRow(Resources.Password, _passwordBox);

        _repeatBox = new TextBox();
        _repeatBox.TextMode = TextBoxMode.Password;
        _repeatBox.Width = new Unit(10, UnitType.Em);
        _repeatBox.Enabled = true;
        table.AddRow(Resources.RepeatPassword, _repeatBox);

        var buttonPanel = new Panel();

        _okButton = new Button();
        _okButton.Text = Resources.Ok;
        _okButton.Click += new EventHandler(ChangePassword);
        _okButton.Width = new Unit(80d, UnitType.Point);
        _okButton.Enabled = true;
        buttonPanel.Controls.Add(_okButton);

        table.AddRow(string.Empty, buttonPanel);

        table.AddVerticalSpace(10);

        _infoLabel = new Label();
        table.AddRow(string.Empty, _infoLabel);
      }
      else
      {
        table.AddRow(string.Empty, Resources.PasswordResetNoReset);
      }

      _doneLink = new HyperLink();
      _doneLink.Text = Resources.Back;
      _doneLink.NavigateUrl = "Default.aspx";
      table.AddRow(string.Empty, _doneLink);

      this.panel.Controls.Add(table);
    }

    private void DoNotChangePassword(object sender, EventArgs e)
    {
      RedirectHome();
    }

    private void ChangePassword(object sender, EventArgs e)
    {
      var ciphertext = (Request.Params["data"] ?? string.Empty).TryParseHexBytes();
      var plaintext = ciphertext == null ? null : BasicGlobal.Config.DeSecure(ciphertext);

      if (plaintext != null)
      {
        var reset = PasswordReset.FromBinary(plaintext);

        if (DateTime.Now > reset.Created.AddMinutes(15))
        {
          return;
        }

        if (_passwordBox.Text.Length < 8)
        {
          _infoLabel.Text = Resources.PasswordResetTooShort;
        }
        else if (_passwordBox.Text != _repeatBox.Text)
        {
          _infoLabel.Text = Resources.PasswordResetNoMatch;
        }
        else
        {
          Global.LdapAccess.Connection.SetPassword(reset.Dn, _passwordBox.Text);

          var user = Global.LdapAccess.Connection.SearchFirst<Person>(reset.Dn);
          var displayName = user.Name.IsNullOrEmpty() ? user.Nickname : user.Name;

          BasicGlobal.Logger.Log(LogLevel.Info, "User id {0} name {1} completed password reset", user.Id, displayName);
          
          _infoLabel.Text = Resources.PasswordResetDone;
          _okButton.Enabled = false;
          _passwordBox.Enabled = false;
          _repeatBox.Enabled = false;
          _repeatBox.Text = string.Empty;
          _passwordBox.Text = string.Empty;
        }
      }
    }
  }
}