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
using Pirate.Util;
using Pirate.Util.Logging;

namespace MemberService
{
  public partial class ListCertificates : CustomPage
  {
    protected override string PageName
    {
      get { return "ListCertificates"; }
    }
    
    private Dictionary<Button, long> _buttonToKeys;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;

      var table = new Table();
      table.AddHeaderRow(Resources.Certificates, 7);
      table.AddVerticalSpace(10);

      var caDownloadLink = new HyperLink();
      caDownloadLink.NavigateUrl = "OutputCaCertificate.crt";
      caDownloadLink.Text = Resources.DownloadAuthority;
      table.AddRow(caDownloadLink, 7);
      table.AddVerticalSpace(10);

      var entries = DataGlobal.DataAccess.ListCertificateEntries(user.Id);
      _buttonToKeys = new Dictionary<Button, long>();
      table.AddHeaderRow(Resources.CertificateKey, LdapResources.CertificateFingerprint, LdapResources.CertificateValidFrom, LdapResources.CertificateValidUntil, LdapResources.CertificateLevel, LdapResources.CertificateComment, string.Empty);

      foreach (var entry in entries)
      {
        var certificate = X509CertificateExtensions.Load(entry.CertificateData);
        table.AddRow(Code.To(entry.Key), entry.Fingerprint, certificate.NotBefore.ToShortDateString(), certificate.NotAfter.ToShortDateString(), Pirate.Ldap.Data.AuthenticationLevel.Text(entry.AuthorizationLevel), entry.Comment);

        var deleteButton = new Button();
        deleteButton.Text = Resources.CertificateDelete;
        deleteButton.Click += new EventHandler(DeleteButton_Click);
        table.AddCell(deleteButton, 1);

        _buttonToKeys.Add(deleteButton, entry.Key);
      }

      var buttonPanel = new Panel();

      var createButton = new Button();
      createButton.Text = Resources.CreateCertificate;
      createButton.Click += new EventHandler(CreateButton_Click);
      buttonPanel.Controls.Add(createButton);

      var cancelButton = new Button();
      cancelButton.Text = Resources.Cancel;
      cancelButton.Click += new EventHandler(CancelButton_Click);
      buttonPanel.Controls.Add(cancelButton);

      table.AddRow(string.Empty, buttonPanel);

      this.panel.Controls.Add(table);
    }

    public void CreateButton_Click(object sender, EventArgs e)
    {
      Redirect("CreateCertificate.aspx");
    }

    private void DeleteButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;

      var key = _buttonToKeys[(Button)sender];
      var entry = DataGlobal.DataAccess.FindCertificateEntry(key);

      if (entry.UserUniqueIdentifier == user.Id)
      {
        DataGlobal.DataAccess.RemoveCertificateEntry(key);
        BasicGlobal.Logger.Log(LogLevel.Info, "ListCertificates.DeleteButton removed certificate key {0} fingerprint {1} for user id {2} name {3}", entry.Key, entry.Fingerprint, user.Id, user.Name);
      }
      else
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "ListCertificates.DeleteButton remove access to certificate key {0} fingerprint {1} of user id {2} for user id {3} name {4} denied.", entry.Key, entry.Fingerprint, entry.UserUniqueIdentifier, user.Id, user.Name);
      }

      RedirectSelf();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}