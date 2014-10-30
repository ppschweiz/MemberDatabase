using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Novell.Directory.Ldap;
using System.IO;
using PA = Pirate.Ldap.Attributes.Person;

namespace MemberAdmin
{
  public partial class SpecialFunctions : CustomPage
  {
    protected override string PageName
    {
      get { return "SpecialFunctions"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var table = new Table();

      var access =
        Connection.SearchFirst<Group>(Names.GroupAdministration).MemberDns.Contains(CurrentUser.DN);

      if (access)
      {
        var piVoteCaXml = new Label();
        piVoteCaXml.Text = "<a href=\"./OutputPiVoteXml.xml\">Pi-Vote CA XML</a>";
        table.AddRow(piVoteCaXml);
      }

      if (CurrentUserAuthorizationLevel >= 2)
      {
        var certificateListButton = new LinkButton();
        certificateListButton.Text = Resources.CertificateAdmin;
        certificateListButton.Click += new EventHandler(CertificateListButton_Click);
        table.AddRow(certificateListButton);

        var listTextButton = new LinkButton();
        listTextButton.Text = Resources.ListText;
        listTextButton.Click += new EventHandler(ListTextButton_Click);
        table.AddRow(listTextButton);

        var listRequestButton = new LinkButton();
        listRequestButton.Text = Resources.ListRequest;
        listRequestButton.Click += new EventHandler(listRequestButton_Click);
        table.AddRow(listRequestButton);
      }

      panel.Controls.Add(table);
    }

    void listRequestButton_Click(object sender, EventArgs e)
    {
      Redirect("ListRequest.aspx");
    }

    private void ListTextButton_Click(object sender, EventArgs e)
    {
      Redirect("ListText.aspx");
    }

    private void CertificateListButton_Click(object sender, EventArgs e)
    {
      Redirect("ListCertificates.aspx");
    }
  }
}