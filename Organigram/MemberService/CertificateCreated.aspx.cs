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
using Pirate.Util.Logging;
using Pirate.Web;

namespace MemberService
{
  public partial class CertificateCreated : CustomPage
  {
    protected override string PageName
    {
      get { return "CertificateCreated"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      var key = Convert.ToInt64(Request.Params["key"]);

      var table = new Table();

      table.AddRow(new LiteralControl("<iframe width=\"1px\" height=\"1px\" src=\"OutputCertificate.crt?key=" + key.ToString() + "\"></iframe>"));

      table.AddHeaderRow(Resources.CertificateCreated, 1, HorizontalAlign.Center);
      var certificateDownloadLink = new HyperLink();
      certificateDownloadLink.NavigateUrl = "OutputCertificate.crt?key=" + key.ToString();
      certificateDownloadLink.Text = Resources.CertificateDownload;
      table.AddRow(certificateDownloadLink, 1, HorizontalAlign.Center);
      table.AddVerticalSpace(20);

      table.AddRow(Resources.CertificateAuthorization, 1, HorizontalAlign.Center);
      table.AddVerticalSpace(10);
      table.AddRow(Resources.CertificateCode + " " + Code.To(key), 1, HorizontalAlign.Center);
      table.AddVerticalSpace(10);
      table.AddRow(Texts.GetText(DataGlobal.DataAccess, new Texts.CertificateCall(), CurrentUser).Item2, 1, HorizontalAlign.Center);
      table.AddVerticalSpace(20);

      var buttonPanel = new Panel();

      var doneButton = new Button();
      doneButton.Text = Resources.Done;
      doneButton.Click += new EventHandler(DoneButton_Click);
      buttonPanel.Controls.Add(doneButton);

      table.AddRow(buttonPanel);

      this.panel.Controls.Add(table);
    }

    private void DoneButton_Click(object sender, EventArgs e)
    {
      Redirect("ListCertificates.aspx");
    }
  }
}