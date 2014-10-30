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
  public partial class BeginVerifyEmail : CustomPage
  {
    private TextBox _emailTextBox;
    private Action _action;

    protected override string PageName
    {
      get { return "BeginVerifyEmailAddress"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      _action = Actions.GetValue(Request);

      var table = new Table();
      switch (_action)
      {
        case Action.RegisterForum:
          table.AddHeaderRow(Resources.CreateAccountForum, 2);
          table.AddRow(string.Format(Resources.CreateAccountStep, 1, 2), 2);
          break;
        case Action.RegisterMember:
          table.AddHeaderRow(Resources.CreateAccountMember, 2);
          table.AddRow(string.Format(Resources.CreateAccountStep, 1, 3), 2);
          break;
        default:
          throw new ArgumentException("Unknown action.");
      }
      table.AddVerticalSpace(10);

      _emailTextBox = new TextBox();
      _emailTextBox.Width = new Unit(30, UnitType.Em);
      table.AddRow(LdapResources.Email, _emailTextBox);

      var buttonPanel = new Panel();

      var okButton = new Button();
      okButton.Text = Resources.Ok;
      okButton.Click += new EventHandler(SaveButton_Click);
      okButton.Width = new Unit(80d, UnitType.Point);
      buttonPanel.Controls.Add(okButton);

      var cancelButton = new Button();
      cancelButton.Text = Resources.Cancel;
      cancelButton.Click += new EventHandler(CancelButton_Click);
      cancelButton.Width = new Unit(80d, UnitType.Point);
      buttonPanel.Controls.Add(cancelButton);

      table.AddRow(string.Empty, buttonPanel);

      this.panel.Controls.Add(table);
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      var verification = new EmailVerification(_emailTextBox.Text, DateTime.Now);
      var data = BasicGlobal.Config.Secure(verification.ToBinary()).ToHexString();
      var url = BasicGlobal.Config.RegistrationServiceAddress + "CreateAccount.aspx?data=" + data + "&" + Actions.CreateParamemter(_action);
      StringPair text;

      switch (_action)
      {
        case Action.RegisterForum:
          text = Texts.GetText(
            DataGlobal.DataAccess,
            new Texts.VerifyEmailForForumEmail(),
            Resources.Culture.TwoLetterISOLanguageName.ToLowerInvariant(),
            new StringPair(Texts.VerifyEmailForForumEmail.ContinueUrlField, url));
          break;
        case Action.RegisterMember:
          text = Texts.GetText(
            DataGlobal.DataAccess,
            new Texts.VerifyEmailForMemberEmail(),
            Resources.Culture.TwoLetterISOLanguageName.ToLowerInvariant(),
            new StringPair(Texts.VerifyEmailForMemberEmail.ContinueUrlField, url));
          break;
        default:
          throw new ArgumentException("Unknown action.");
      }

      BasicGlobal.Logger.Log(LogLevel.Info, "Initiated verifcation of email {0}", _emailTextBox.Text);
      BasicGlobal.Mailer.SendTextile(text.Item1, text.Item2, new System.Net.Mail.MailAddress(_emailTextBox.Text));

      Redirect("DisplayMessage.aspx?data=" + BasicGlobal.Config.Secure((new Message(Resources.VerfyEmailSent, "Default.aspx")).ToBinary()).ToHexString());
    }
  }
}