using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public partial class ListCertificates : CustomPage
  {
    protected override string PageName
    {
      get { return "ListCertificates"; }
    }
    
    private TextBox _keyTextBox;

    private Dictionary<Button, long> _buttonToKey;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel < 1)
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "ListCertificates.Load user id {0} name {1} has authorization level {2} meaning insufficient access.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel);
        RedirectAccessDenied();
        return;
      }

      var table = new Table();

      var entries = DataGlobal.DataAccess.ListCertificateEntries();

      var personIds = entries.Select(x => x.UserUniqueIdentifier).Distinct().ToList();
      var persons = Access.Connection.Search<Person>(Names.Party, LdapScope.Sub, Attributes.Person.UniqueIdentifier.AnyOf(personIds)).ToList();

      _buttonToKey = new Dictionary<Button, long>();

      table.AddHeaderRow(
        LdapResources.UniqueIdentifier,
        LdapResources.Username,
        LdapResources.CommonName,
        LdapResources.CertificateFingerprint,
        LdapResources.CertificateValidFrom,
        LdapResources.CertificateValidUntil,
        LdapResources.CertificateComment,
        LdapResources.CertificateLevel,
        string.Empty);

      foreach (var entry in entries.OrderBy(x => x.UserUniqueIdentifier))
      {
        var person = persons.Where(p => entry.UserUniqueIdentifier == p.Id).Single();
        var certificate = Pirate.Util.X509CertificateExtensions.Load(entry.CertificateData);

        table.AddRow(
          person.Id.ToString(),
          person.Nickname,
          person.Name,
          entry.Fingerprint,
          certificate.NotBefore.ToShortDateString(),
          certificate.NotAfter.ToShortDateString(),
          entry.Comment,
          Pirate.Ldap.Data.AuthenticationLevel.Text(entry.AuthorizationLevel));

        if (CurrentUserAuthorizationLevel >= 2 &&
            CurrentUserAuthorizationLevel > entry.AuthorizationLevel)
        {
          var entryButtonPanel = new Panel();

          if (entry.AuthorizationLevel > 0)
          {
            var revokeButton = new Button();
            revokeButton.Text = Resources.CertificateRevoke;
            revokeButton.Click += new EventHandler(RevokeButton_Click);
            _buttonToKey.Add(revokeButton, entry.Key);
            entryButtonPanel.Controls.Add(revokeButton);
          }
          
          var deleteButton = new Button();
          deleteButton.Text = Resources.Delete;
          deleteButton.Click += new EventHandler(DeleteButton_Click);
          _buttonToKey.Add(deleteButton, entry.Key);
          entryButtonPanel.Controls.Add(deleteButton);

          table.AddCell(entryButtonPanel, 1);
        }
        else
        {
          table.AddCell(string.Empty, 1);
        }
      }

      table.AddVerticalSpace(20);

      var acceptPanel = new Panel();

      var acceptLabel = new Label();
      acceptLabel.Text = Resources.CertificateAcceptInfo;
      acceptPanel.Controls.Add(acceptLabel);

      _keyTextBox = new TextBox();
      acceptPanel.Controls.Add(_keyTextBox);

      Button acceptButton = new Button();
      acceptButton.Text = Resources.CertificateFind;
      acceptButton.Click += new EventHandler(AcceptButton_Click);
      acceptPanel.Controls.Add(acceptButton);

      table.AddCell(acceptPanel, 9);

      table.AddVerticalSpace(20);

      var buttonPanel = new Panel();

      Button backbutton = new Button();
      backbutton.Text = Resources.Back;
      backbutton.Click += new EventHandler(Backbutton_Click);
      buttonPanel.Controls.Add(backbutton); 
      
      table.AddCell(buttonPanel, 9);

      this.panel.Controls.Add(table);
    }

    private void AcceptButton_Click(object sender, EventArgs e)
    {
      var key = Pirate.Util.Code.From(_keyTextBox.Text);
      Redirect("AcceptCertificate.aspx?key=" + key.ToString());
    }

    private void Backbutton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }

    private void RevokeButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel < 1)
      {
        RedirectAccessDenied();
        return;
      }

      var key = _buttonToKey[(Button)sender];
      var entry = DataGlobal.DataAccess.FindCertificateEntry(key);

      if (CurrentUserAuthorizationLevel >= 2 &&
          CurrentUserAuthorizationLevel > entry.AuthorizationLevel)
      {
        if (entry.AuthorizationLevel > 0)
        {
          DataGlobal.DataAccess.AuthorizeCertificateEntry(key, entry.AuthorizationLevel - 1);
          BasicGlobal.Logger.Log(LogLevel.Info, "ListCertificates.RevokeButton user id {0} name {1} reduced access of user id {2} with certificate key {3} fingerprint {4}", CurrentUser.Id, CurrentUser.Name, entry.Key, entry.Fingerprint, entry.UserUniqueIdentifier);
        }
      }
      else
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "ListCertificates.RevokeButton user id {0} name {1} has authorization level {2} meaning insufficient access.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel);
      }

      RedirectSelf();
    }

    private void DeleteButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel < 1)
      {
        RedirectAccessDenied();
        return;
      }

      var key = _buttonToKey[(Button)sender];
      var entry = DataGlobal.DataAccess.FindCertificateEntry(key);

      if (CurrentUserAuthorizationLevel >= 2 &&
          CurrentUserAuthorizationLevel > entry.AuthorizationLevel)
      {
        DataGlobal.DataAccess.RemoveCertificateEntry(key);
        BasicGlobal.Logger.Log(LogLevel.Info, "ListCertificates.DeleteButton user id {0} name {1} removed access of user id {2} with certificate key {3} fingerprint {4}", CurrentUser.Id, CurrentUser.Name, entry.Key, entry.Fingerprint, entry.UserUniqueIdentifier);
      }
      else
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "ListCertificates.DeleteButton user id {0} name {1} has authorization level {2} meaning insufficient access.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel);
      }

      RedirectSelf();
    }
  }
}