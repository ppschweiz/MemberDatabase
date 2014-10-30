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
  public partial class AcceptCertificate : CustomPage
  {
    protected override string PageName
    {
      get { return "AcceptCertificate"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel < 1)
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "AcceptCertificate.Load user id {0} name {1} has authorization level {2} meaning insufficient access.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel);
        RedirectAccessDenied();
        return;
      }

      var key = Convert.ToInt64(Request.Params["key"]);
      var entry = DataGlobal.DataAccess.FindCertificateEntry(key);

      var table = new Table();
      var buttonPanel = new Panel();

      if (entry != null)
      {
        var user = Connection.SearchFirst<Person>(Names.Party, LdapScope.Sub, Attributes.Person.UniqueIdentifier == entry.UserUniqueIdentifier);
        var certificate = Pirate.Util.X509CertificateExtensions.Load(entry.CertificateData);

        table.AddRow(LdapResources.UniqueIdentifier, entry.UserUniqueIdentifier.ToString());
        table.AddRow(LdapResources.Username, user.Nickname);
        table.AddRow(LdapResources.CommonName, user.Name);
        table.AddRow(LdapResources.CertificateFingerprint, entry.Fingerprint);
        table.AddRow(LdapResources.CertificateValidFrom, certificate.NotBefore.ToShortDateString());
        table.AddRow(LdapResources.CertificateValidUntil, certificate.NotAfter.ToShortDateString());
        table.AddRow(LdapResources.CertificateComment, entry.Comment);
        table.AddVerticalSpace(20);

        if (CurrentUserAuthorizationLevel >= 2)
        {
          var acceptButton = new Button();
          acceptButton.Text = Resources.CertificateAccept;
          acceptButton.Click += new EventHandler(AcceptButton_Click);
          buttonPanel.Controls.Add(acceptButton);
        }

        if (CurrentUserAuthorizationLevel >= 3)
        {
          var acceptButton = new Button();
          acceptButton.Text = Resources.CertificateAccept + " 2";
          acceptButton.Click += new EventHandler(Accept2Button_Click);
          buttonPanel.Controls.Add(acceptButton);
        }

        if (CurrentUserAuthorizationLevel >= 4)
        {
          var acceptButton = new Button();
          acceptButton.Text = Resources.CertificateAccept + " 3";
          acceptButton.Click += new EventHandler(Accept3Button_Click);
          buttonPanel.Controls.Add(acceptButton);
        }
      }
      else
      {
        table.AddRow(Resources.CertificateNotFound, 9);
        table.AddVerticalSpace(20);
      }

      Button backbutton = new Button();
      backbutton.Text = Resources.Back;
      backbutton.Click += new EventHandler(Backbutton_Click);
      buttonPanel.Controls.Add(backbutton);

      table.AddRow(buttonPanel, 9);

      this.panel.Controls.Add(table);
    }

    private void AcceptButton_Click(object sender, EventArgs e)
    {
      Accept(1);
    }

    private void Accept2Button_Click(object sender, EventArgs e)
    {
      Accept(2);
    }

    private void Accept3Button_Click(object sender, EventArgs e)
    {
      Accept(3);
    }

    private void Accept(int level)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel < level)
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "AcceptCertificate.Load user id {0} name {1} has authorization level {2} meaning insufficient access to authorized at level {3}.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel, level);
        RedirectAccessDenied();
        return;
      }

      var key = Convert.ToInt64(Request.Params["key"]);
      var entry = DataGlobal.DataAccess.FindCertificateEntry(key);

      if (entry.AuthorizationLevel < level)
      {
        DataGlobal.DataAccess.AuthorizeCertificateEntry(key, level);
        BasicGlobal.Logger.Log(LogLevel.Info, "AcceptCertificate.Load user id {0} name {1} has authorized user id {2} with certificate key {3} fingerprint {4} on level {5}", CurrentUser.Id, CurrentUser.Name, entry.UserUniqueIdentifier, entry.Key, entry.Fingerprint, level);
      }

      Redirect("ListCertificates.aspx");
    }

    private void Backbutton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}