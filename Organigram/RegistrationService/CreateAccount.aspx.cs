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
using System.Net;
using Pirate.Util.Logging;

namespace RegistrationService
{
  public partial class CreateAccount : CustomPage
  {
    private TextBox _usernameBox;
    private TextBox _passwordBox;
    private TextBox _repeatBox;
    private Label _infoLabel;
    private Button _nextButton;
    private Button _cancelButton;
    private Action _action;

    private const int VerificationDurationHours = 48;

    protected override string PageName
    {
      get { return "CreateAccount"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      _action = Actions.GetValue(Request);

      var table = new Table();
      switch (_action)
      {
        case Action.RegisterForum:
          table.AddHeaderRow(Resources.CreateAccountForum, 2);
          table.AddRow(string.Format(Resources.CreateAccountStep, 2, 2), 2);
          break;
        case Action.RegisterMember:
          table.AddHeaderRow(Resources.CreateAccountMember, 2);
          table.AddRow(string.Format(Resources.CreateAccountStep, 2, 3), 2);
          break;
        default:
          throw new ArgumentException("Unknown action.");
      }
      table.AddVerticalSpace(10);

      var ciphertext = (Request.Params["data"] ?? string.Empty).TryParseHexBytes();
      var plaintext = ciphertext == null ? null : BasicGlobal.Config.DeSecure(ciphertext);
      var verification = plaintext == null ? null : EmailVerification.FromBinary(plaintext);

      if (verification != null &&
          DateTime.Now <= verification.Created.AddHours(VerificationDurationHours))
      {
        _usernameBox = new TextBox();
        _usernameBox.Width = new Unit(10, UnitType.Em);
        table.AddRow(LdapResources.Username, _usernameBox);

        _passwordBox = new TextBox();
        _passwordBox.TextMode = TextBoxMode.Password;
        _passwordBox.Width = new Unit(10, UnitType.Em);
        table.AddRow(Resources.Password, _passwordBox);

        _repeatBox = new TextBox();
        _repeatBox.TextMode = TextBoxMode.Password;
        _repeatBox.Width = new Unit(10, UnitType.Em);
        table.AddRow(Resources.RepeatPassword, _repeatBox);

        var buttonPanel = new Panel();

        _nextButton = new Button();
        _nextButton.Text = _action == Action.RegisterMember ? Resources.Next : Resources.Finish;
        _nextButton.Click += new EventHandler(NextButton_Click);
        _nextButton.Width = new Unit(60d, UnitType.Point);
        buttonPanel.Controls.Add(_nextButton);

        _cancelButton = new Button();
        _cancelButton.Text = Resources.Cancel;
        _cancelButton.Click += new EventHandler(CancelButton_Click);
        _cancelButton.Width = new Unit(60d, UnitType.Point);
        buttonPanel.Controls.Add(_cancelButton);

        table.AddRow(string.Empty, buttonPanel);

        table.AddVerticalSpace(10);

        _infoLabel = new Label();
        table.AddRow(string.Empty, _infoLabel);
      }
      else
      {
        Redirect("DisplayMessage.aspx?data=" + BasicGlobal.Config.Secure((new Message(Resources.VerifyEmailOutdated, "Default.aspx")).ToBinary()).ToHexString());
      }

      this.panel.Controls.Add(table);
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }

    private void NextButton_Click(object sender, EventArgs e)
    {
      var ciphertext = (Request.Params["data"] ?? string.Empty).TryParseHexBytes();
      var plaintext = ciphertext == null ? null : BasicGlobal.Config.DeSecure(ciphertext);
      var verification = plaintext == null ? null : EmailVerification.FromBinary(plaintext);

      if (verification != null &&
          DateTime.Now <= verification.Created.AddHours(VerificationDurationHours))
      {
        var user = Global.LdapAccess.Connection.SearchFirst<Person>(Names.Party, LdapScope.Sub, Attributes.Person.UserId == _usernameBox.Text);

        if (_usernameBox.Text.Length < 1)
        {
          _infoLabel.Text = Resources.CreateAccountUsernameTaken;
        }
        else if (user != null)
        {
          _infoLabel.Text = Resources.CreateAccountUsernameTaken;
        }
        else if (_passwordBox.Text.Length < 8)
        {
          _infoLabel.Text = Resources.CreateAccountPasswordTooShort;
        }
        else if (_passwordBox.Text != _repeatBox.Text)
        {
          _infoLabel.Text = Resources.CreateAccountPasswordMismatch;
        }
        else
        {
          var uniqueIdentifer = Global.LdapAccess.Connection.GetNextPersonId();
          var person = new Person(Names.People, uniqueIdentifer, _usernameBox.Text);
          person.Emails.Add(verification.Email);
          person.EmployeeType = EmployeeType.LandLubber;
          person.Create(Global.LdapAccess.Connection);
          Global.LdapAccess.Connection.SetPassword(person.DN, _passwordBox.Text);

          BasicGlobal.Logger.Log(LogLevel.Info, "New account id {0} nickname {1} email {2}", person.Id, person.Nickname, person.Emails.FirstOrDefault() ?? string.Empty);

          switch (_action)
          {
            case Action.RegisterForum:
              Redirect("DisplayMessage.aspx?data=" + BasicGlobal.Config.Secure((new Message(new Texts.ForumAccountCreatedMessage().Id, person.DN, "http://forum.piratenpartei.ch")).ToBinary()).ToHexString());
              break;
            case Action.RegisterMember:
              Redirect("RequestMembership.aspx?data=" + BasicGlobal.Config.Secure((new MembershipRequest(person.DN, DateTime.Now.AddHours(1))).ToBinary()).ToHexString());
              break;
            default:
              throw new ArgumentException("Unknown action.");
          }
        }
      }
    }
  }
}