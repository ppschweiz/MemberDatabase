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
using System.Text;

namespace MemberAdmin
{
  public partial class ListMember : CustomPage
  {
    protected override string PageName
    {
      get { return "ListMember"; }
    }

    public const string SearchParameters = "SearchParameters";
    
    private Dictionary<object, string> _buttonIdMap;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      BasicGlobal.Logger.Log(LogLevel.Debug, "ListMember.Load " + Connection.AuthenticationDN);

      var builder = new StringBuilder();
      var table = new Table();

      try
      {
        builder.AppendLine("Search by user id {0} name {1}", CurrentUser.Id, CurrentUser.Name);

        var parameters = Session[SearchParameters] as SearchParameters;

        foreach (var searchDn in parameters.SearchDns)
        {
          builder.AppendLine("Search DN: {0}", searchDn);
        }

        builder.AppendLine("Filter: {0}", parameters.Filter == null ? "N/A" :  parameters.Filter.Text);
        builder.AppendLine("Attributes: {0}", string.Join(", ", parameters.Attributes.Select(a => (string)a).ToArray()));

        foreach (var postFilter in parameters.PostFilters)
        {
          builder.AppendLine("Postfilter: {0}", postFilter.Description);
        }

        var persons =
          parameters.SearchDns.SelectMany(dn =>
            Connection.Search<Person>(dn, LdapScope.Sub, parameters.Filter))
          .Where(p => parameters.PostFilters.All(f => f.Function(p)));

        _buttonIdMap = new Dictionary<object, string>();
        var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");
        var headings = new List<string>();

        foreach (var attribute in parameters.Attributes)
        {
          headings.Add(attribute.Text());
        }

        table.AddHeaderRow(headings.ToArray());

        foreach (var person in persons.OrderBy(parameters.OrderBySelector))
        {
          var controls = new List<Control>();

          foreach (var attribute in parameters.Attributes)
          {
            var label = new Label();
            var field = person.GetField(attribute);

            if (field.NeedsEvaluation)
            {
              label.Text = field.EvaluateString(Connection);
            }
            else
            {
              label.Text = field.ValueString(MultiValueOutput.MultiLine);
            }

            controls.Add(label);
          }

          var editButton = new LinkButton();
          editButton.Text = Resources.Edit;
          editButton.Click += new EventHandler(EditButton_Click);
          _buttonIdMap.Add(editButton, person.Id.ToString());
          controls.Add(editButton);

          table.AddRow(controls.ToArray());
        }

        builder.AppendLine("Results: " + persons.Count());
      }
      catch (Exception exception)
      {
        builder.AppendLine("Error: " + exception.ToString());
        BasicGlobal.Mailer.SendToAdmin("Error", builder.ToString());
      }
      finally
      {
        BasicGlobal.Logger.Log(LogLevel.Info, builder.ToString());
      }

      this.panel.Controls.Add(table);
    }

    private void EditButton_Click(object sender, EventArgs e)
    {
      var id = _buttonIdMap[sender];
      Redirect("DisplayMember.aspx?member=" + id);
    }
  }
}