using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public partial class AdminMenu : System.Web.UI.UserControl
  {
    private LdapAccess _access;

    protected void Page_Load(object sender, EventArgs e)
    {
      _access = Ldap.Connect(Application, Session, Request);

      if (_access == null)
      {
        return;
      }

      var user = _access.Connection.SearchFirst<Person>(Ldap.GetUserDn(Application, Session, Request), LdapScope.Base);
      Thread.CurrentThread.CurrentCulture = Language.GetCulture(user.PreferredLanguage);
      Resources.Culture = Thread.CurrentThread.CurrentCulture;
      LdapResources.Culture = Thread.CurrentThread.CurrentCulture;
      
      this.logoffButton.Text = Resources.Logoff;
      this.anomaliesButton.Text = Resources.Anomaies;
      this.groupsButton.Text = Resources.Groups;
      this.searchButton.Text = Resources.Search;
      this.mailButton.Text = Resources.Mailer;
      this.createButton.Text = Resources.Create;
      this.createButton.Enabled = _access.Connection.Count<Person>(Names.People, LdapScope.Sub) > 0;
    }

    protected void logoffButton_Click(object sender, EventArgs e)
    {
      Ldap.Logoff(Context.Application, Context.Session, Request);
      Redirect("Login.aspx");
    }

    private void Redirect(string target)
    {
      Response.Redirect("./" + target, false);
    }

    protected void searchButton_Click(object sender, EventArgs e)
    {
      Redirect("SearchMember.aspx");
    }

    protected void mailButton_Click(object sender, EventArgs e)
    {
      Redirect("PrepareMail.aspx");
    }

    protected void groupsButton_Click(object sender, EventArgs e)
    {
      Redirect("GroupList.aspx");
    }

    protected void anomaliesButton_Click(object sender, EventArgs e)
    {
      Redirect("Anomalies.aspx");
    }

    private UpdateState UpdateState
    {
      get { return Session["updatestate"] as UpdateState; }
      set { Session["updatestate"] = value; }
    }

    protected void createButton_Click(object sender, EventArgs e)
    {
      var person = new Person(Names.People, _access.Connection.GetNextPersonId(), "New Member");
      UpdateState = new UpdateState(
        UpdateOperation.Create,
        person);
      Redirect("DisplayMember.aspx?member=" + person.Id.ToString());
    }

    protected void specialButton_Click(object sender, EventArgs e)
    {
      Redirect("SpecialFunctions.aspx");
    }
  }
}