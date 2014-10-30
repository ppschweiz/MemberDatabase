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
using Pirate.Ldap.Data;
using Pirate.Util.Logging;

namespace MemberService
{
  public partial class ChangeEmail : CustomPage
  {
    protected override string PageName
    {
      get { return "ChangeEmail"; }
    }
    
    private Dictionary<TextBox, Label> _emails;
    private Dictionary<TextBox, Label> _alternates;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;
      var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");

      var table = new Table();
      table.AddHeaderRow(Resources.ChangeEmail, 2);
      table.AddVerticalSpace(10);
      table.AddRow(string.Empty, Resources.EmailInfo2);

      _emails = new Dictionary<TextBox, Label>();

      for (int i = 0; i < 3; i++)
      {
        var emailBox = new TextBox();
        emailBox.Text = user.Emails.Count >= (i + 1) ? user.Emails.Skip(i).First() : string.Empty;
        emailBox.Width = new Unit(20, UnitType.Em);
        var emailValid = new Label();
        emailValid.Text = string.Empty;
        table.AddRow(LdapResources.Email + " " + (i + 1).ToString(), emailBox, emailValid);
        _emails.Add(emailBox, emailValid);
      }

      table.AddRow(string.Empty, Resources.EmailInfo);
      table.AddVerticalSpace(10);

      _alternates = new Dictionary<TextBox, Label>();

      for (int i = 0; i < 3; i++)
      {
        var emailBox = new TextBox();
        emailBox.Text = user.AlternateEmails.Count >= (i + 1) ? user.AlternateEmails.Skip(i).First() : string.Empty;
        emailBox.Width = new Unit(20, UnitType.Em);
        var emailValid = new Label();
        emailValid.Text = string.Empty;
        table.AddRow(LdapResources.AlternateMail + " " + (i + 1).ToString(), emailBox, emailValid);
        _alternates.Add(emailBox, emailValid);
      }

      table.AddRow(string.Empty, Resources.AlternateEmailInfo);

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

    private static bool EmailValid(string text)
    {
      return string.IsNullOrEmpty(text) || Regex.IsMatch(text, @"[0-9a-zA-Z\+\ \-_\.]+@([0-9a-zA-Z\ \-_]{2,255}\.){1,32}[0-9a-zA-Z\ \-_]{2,5}");
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      bool valid = true;

      foreach (var email in _emails)
      {
        if (!EmailValid(email.Key.Text))
        {
          email.Value.Text = Resources.EmailInvalid;
          valid = false;
        }
      }

      if (_emails.All(m => string.IsNullOrEmpty(m.Key.Text)))
      {
        _emails.First().Value.Text = Resources.EmailInvalid;
        valid = false;
      }

      foreach (var email in _alternates)
      {
        if (!EmailValid(email.Key.Text))
        {
          email.Value.Text = Resources.EmailInvalid;
          valid = false;
        }
      }

      if (valid)
      {
        if (!SetupLdap())
        {
          RedirectLogin();
          return;
        }

        var user = CurrentUser;

        var report = new StringBuilder();
        report.AppendLine(string.Format("User id {0} name {1} changed his data.", user.Id, user.Name));

        try
        {
          DataGlobal.DataAccess.RemoveOutdatedEmailAddressChanges();

          var oldMails = user.Emails.ToList();
          var emails = _emails.
            Select(m => m.Key.Text)
            .Where(m => !string.IsNullOrEmpty(m));

          WorkList(user, user.Emails, emails, x => user.Emails.Remove(x));

          foreach (var removed in oldMails.Where(x => !user.Emails.Contains(x)))
          {
            report.AppendLine("Mail removed " + removed);
          }

          var alternates = _alternates.
            Select(m => m.Key.Text)
            .Where(m => !string.IsNullOrEmpty(m));

          foreach (var added in alternates.Where(x => !user.AlternateEmails.Contains(x)))
          {
            report.AppendLine("Alternate mail added " + added);
          }
          foreach (var removed in user.AlternateEmails.Where(x => !alternates.Contains(x)))
          {
            report.AppendLine("Alternate mail removed " + removed);
          }

          user.AlternateEmails = alternates.ToList();
          user.Modify(Connection);

          report.AppendLine("Operation successful");
        }
        catch (Exception exception)
        {
          report.AppendLine("Operation failed");
          report.AppendLine(exception.ToString());
        }
        finally
        {
          BasicGlobal.Logger.Log(LogLevel.Info, report.ToString()); 
          BasicGlobal.Mailer.SendToMembersQueue("User data changed", report.ToString());
        }

        RedirectHome();
      }
    }

    private void WorkList(Person user, IEnumerable<string> oldList, IEnumerable<string> newList, Action<string> removeAction)
    {
      var remove = new Queue<string>(oldList.Where(x => !newList.Contains(x)));
      var add = new Queue<string>(newList.Where(x => !oldList.Contains(x)));

      while (add.Count > 0 && remove.Count > 0)
      {
        UpdateEmail(user, remove.Dequeue(), add.Dequeue());
      }

      while (add.Count > 0)
      {
        UpdateEmail(user, null, add.Dequeue());
      }

      while (remove.Count > 0)
      {
        removeAction(remove.Dequeue());
      }
    }

    private void UpdateEmail(Person user, string oldEmail, string newEmail)
    { 
      var bytes = new byte[32];
      RandomNumberGenerator.Create().GetBytes(bytes);
      string id = bytes.ToHexString();

      DataGlobal.DataAccess.AddEmailAddressChange(new Pirate.Ldap.Data.EmailAddressChange(id, user.Nickname, oldEmail, newEmail, DateTime.Now));

      var url = BasicGlobal.Config.MemberServiceAddress + "ConfirmChangeEmail.aspx?id=" + id;
      var text = Texts.GetText(DataGlobal.DataAccess, new Texts.UpdateMailEmail(), user, new StringPair(Texts.UpdateMailEmail.UpdateUrl, url));
      var displayName = user.Name.IsNullOrEmpty() ? user.Nickname : user.Name;

      BasicGlobal.Mailer.SendTextile(text.Item1, text.Item2, Mailer.EncodeAddress(newEmail, displayName));

      RedirectMessage(new Texts.UpdateMailMessage());
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}