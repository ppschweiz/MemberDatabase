using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
  public partial class BeginPasswordReset : CustomPage
  {
    private TextBox _usernameOrEmail;
    private Label _infoLabel;
    private Button _okButton;
    private Button _cancelButton;
    private Button _doneButton;

    protected override string PageName
    {
      get { return "BeginPasswordReset"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      var table = new Table();
      table.AddHeaderRow(Resources.PasswordReset, 2);
      table.AddVerticalSpace(10);

      _usernameOrEmail = new TextBox();
      _usernameOrEmail.Width = new Unit(30, UnitType.Em);
      table.AddRow(Resources.PasswordResetUserOrEmail, _usernameOrEmail);

      var buttonPanel = new Panel();

      _okButton = new Button();
      _okButton.Text = Resources.Ok;
      _okButton.Click += new EventHandler(SaveButton_Click);
      _okButton.Width = new Unit(80d, UnitType.Point);
      buttonPanel.Controls.Add(_okButton);

      _cancelButton = new Button();
      _cancelButton.Text = Resources.Cancel;
      _cancelButton.Click += new EventHandler(CancelButton_Click);
      _cancelButton.Width = new Unit(80d, UnitType.Point);
      buttonPanel.Controls.Add(_cancelButton);

      table.AddRow(string.Empty, buttonPanel);

      table.AddVerticalSpace(20);

      _infoLabel = new Label();
      table.AddRow(string.Empty, _infoLabel);

      _doneButton = new Button();
      _doneButton.Visible = false;
      _doneButton.Text = Resources.Ok;
      _doneButton.Click += new EventHandler(CancelButton_Click);
      _doneButton.Width = new Unit(80d, UnitType.Point);
      table.AddRow(string.Empty, _doneButton);
      
      this.panel.Controls.Add(table);
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      var user = Global.LdapAccess.Connection.SearchFirst<Person>(Names.Party, LdapScope.Sub, (Attributes.Person.UserId == _usernameOrEmail.Text) | (Attributes.Person.Mail == _usernameOrEmail.Text));

      if (user == null)
      {
        _infoLabel.Text = Resources.PasswordResetNotFound;
      }
      else
      {
        var code = Crypto.GetRandom(16).ToHexString();
        var reset = new PasswordReset(user.DN, DateTime.Now);
        var data = BasicGlobal.Config.Secure(reset.ToBinary()).ToHexString();
        var url = BasicGlobal.Config.RegistrationServiceAddress + "CompletePasswordReset.aspx?data=" + data;

        var text = Texts.GetText(
          DataGlobal.DataAccess,
          new Texts.PasswordResetEmail(),
          user,
          new StringPair(Texts.PasswordResetEmail.ResetUrlField, url));
        var displayName = user.Name.IsNullOrEmpty() ? user.Nickname : user.Name;

        BasicGlobal.Logger.Log(LogLevel.Info, "User id {0} name {1} initiated password reset", user.Id, displayName);

        foreach (var address in user.Emails)
        {
          BasicGlobal.Mailer.SendTextile(text.Item1, text.Item2, Mailer.EncodeAddress(address, displayName));
        }

        _usernameOrEmail.Text = string.Empty;
        _usernameOrEmail.Enabled = false;
        _okButton.Enabled = false;
        _cancelButton.Enabled = false;
        _doneButton.Visible = true;
        _infoLabel.Text = Resources.PasswordResetInfo;
      }
    }
  }
}