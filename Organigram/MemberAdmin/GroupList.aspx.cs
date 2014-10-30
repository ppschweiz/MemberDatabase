using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Novell.Directory.Ldap;
using System.IO;

namespace MemberAdmin
{
  public partial class GroupList : CustomPage
  {
    protected override string PageName
    {
      get { return "GroupList"; }
    }
    
    private Dictionary<object, string> _groupEditList;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var table = new Table();
      _groupEditList = new Dictionary<object, string>();

      var pps = Connection.SearchFirst<Organization>(Names.Party, LdapScope.Base);
      AddOrganization(table, pps, Connection);

      this.panel.Controls.Add(table);
    }

    private void AddOrganization(Table table, Organization organization, LdapConnection connection)
    {
      foreach (var subGroup in organization.Groups(connection))
      {
        AddGroup(table, organization.DisplayNameGerman, subGroup, connection);
      }

      foreach (var container in organization.Containers(connection))
      {
        AddContainer(table, organization.DisplayNameGerman, container, connection);
      }

      foreach (var section in organization.Sections(connection))
      {
        AddSection(table, section, connection);
      }
    }

    private void AddContainer(Table table, string title, Container container, LdapConnection connection)
    {
      foreach (var subGroup in container.Groups(connection))
      {
        AddGroup(table, title, subGroup, connection);
      }
    }

    private void AddSection(Table table, Section section, LdapConnection connection)
    {
      foreach (var subGroup in section.Groups(connection))
      {
        AddGroup(table, section.DisplayNameGerman, subGroup, connection);
      }
    }

    private void AddGroup(Table table, string title, Group group, LdapConnection connection)
    {
      if (!string.IsNullOrEmpty(group.DisplayNameGerman))
      {
        var groupLink = new LinkButton();
        groupLink.Text = group.DisplayNameGerman;
        groupLink.Click += new EventHandler(GroupLink_Click);
        _groupEditList.Add(groupLink, group.DN);
        table.AddRow(title, groupLink);
      }

      foreach (var subGroup in group.Groups(connection))
      {
        AddGroup(table, title, subGroup, connection);
      }
    }

    private void GroupLink_Click(object sender, EventArgs e)
    {
      Redirect("GroupEdit.aspx?groupdn=" + _groupEditList[sender].Replace("=", "-"));
    }
  }
}