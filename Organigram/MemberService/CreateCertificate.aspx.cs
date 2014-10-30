using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Openssl;
using Pirate.Util;
using Pirate.Web;

namespace MemberService
{
  public partial class CreateCertificate : CustomPage
  {
    protected override string PageName
    {
      get { return "CreateCertificate"; }
    }
    
    private KeyGen _keyGen;

    private TextBox _commentBox;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;

      var table = new Table();
      table.AddHeaderRow(Resources.CreateCertificate, 2);
      table.AddVerticalSpace(10);

      _keyGen = new KeyGen();
      _keyGen.ID = "KeyGen";
      table.AddRow(Resources.CertificateGrade, _keyGen);
      table.AddRow(string.Empty, Resources.CertificateGradeInfo);

      _commentBox = new TextBox();
      _commentBox.Width = new Unit(20, UnitType.Em);
      table.AddRow(Resources.Comment, _commentBox);

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

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;

      if (Convert.FromBase64String(_keyGen.KeyData).Length > 512)
      {
        var spkac = new Spkac(_keyGen.KeyData);
        spkac.CommonName = Encode(user.Name);
        spkac.OrganizationName = "Pirate Party Switzerland";
        spkac.OrganizationalUnitName = "Member Database";
        spkac.CountryName = user.Country;
        spkac.StateOrProvinceName = user.State;
        spkac.LocalityName = Encode(user.Location);
        spkac.EmailAddress = user.Emails.Where(a => a.EndsWith("@piratenpartei.ch")).FirstOrDefault();

        var executor = new Executor();
        var certificateData = executor.SignSpkac(spkac);

        var certificate = X509CertificateExtensions.Load(certificateData);
        var entry = new CertificateEntry(GenerateKey(), certificate.Thumbprint, 0, user.Id, _commentBox.Text, certificateData);
        DataGlobal.DataAccess.AddCertificateEntry(entry);

        BasicGlobal.Logger.Log(Pirate.Util.Logging.LogLevel.Info, "Certificate key {0} fingerprint {1} create for user id {2} name {3}", entry.Key, entry.Fingerprint, user.Id, user.Name);

        Redirect("CertificateCreated.aspx?key=" + entry.Key.ToString());
      }
      else
      {
        this.panel.Controls.Add(ToLabel(Resources.CertificateTooLow));
      }
    }

    private string Encode(string text)
    {
      return text
        .Replace("Ä", "Ae")
        .Replace("ä", "ae")
        .Replace("à", "a")
        .Replace("á", "a")
        .Replace("â", "a")
        .Replace("Ü", "Ue")
        .Replace("ü", "ue")
        .Replace("û", "u")
        .Replace("ù", "u")
        .Replace("ú", "u")
        .Replace("Ö", "Oe")
        .Replace("ö", "oe")
        .Replace("ô", "o")
        .Replace("ò", "o")
        .Replace("ó", "o")
        .Replace("é", "e")
        .Replace("è", "e")
        .Replace("ê", "e")
        .Replace("ê", "e")
        .Replace("ç", "c")
        .Replace("ì", "i")
        .Replace("î", "i")
        .Replace("í", "i")
        .Replace("ÿ", "y")
        .Replace("Ø", "O")
        .Replace("ø", "o")
        .Replace("ø", "o")
        .Replace("ø", "o")
        .Replace("Æ", "AE")
        .Replace("æ", "ae")
        .Replace("Å", "A")
        .Replace("å", "a");
    }

    private long GenerateKey()
    {
      var random = RandomNumberGenerator.Create();
      var buffer = new byte[8];
      random.GetBytes(buffer);
      var key = BitConverter.ToInt64(buffer, 0);
      return key & 0xFFFFFFFFF;
    }
  }
}