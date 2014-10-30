using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public partial class ListRequest : CustomPage
  {
    protected override string PageName
    {
      get { return "ListRequest"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel >= 2)
      {
        var table = new Table();

        foreach (var request in DataGlobal.DataAccess.GetallRequests().ToList().OrderBy(r => r.Requested))
        {
          var person = Access.Connection.SearchFirst<Person>(request.OldDn);

          if (person == null)
          {
            DataGlobal.DataAccess.DeleteRequest(request.Id);
          }
          else
          {
            table.AddRow(request.Requested.ToShortDateString(), GetActionText(request.Action), person.Id.ToString(), person.Name);
            table.AddCell(ToLink(Resources.RequestView, "ViewRequest.aspx?id=" + request.Id), 1);
          }
        }

        table.AddVerticalSpace(10);
        table.AddRow(ToLink(Resources.Done, "Default.aspx"));

        this.panel.Controls.Add(table);
      }
      else
      {
        RedirectHome();
      }
    }

    private string GetActionText(RequestAction action)
    {
      switch (action)
      {
        case RequestAction.Join:
          return Resources.JoinRequestTitle;
        case RequestAction.Transfer:
          return Resources.TransferRequestTitle;
        case RequestAction.Leave:
        case RequestAction.LeaveAndRemoveData:
        case RequestAction.LeaveAndDelete:
          return Resources.LeaveRequestTitle;
        default:
          throw new ArgumentOutOfRangeException("Unknown request action.");
      }
    }
  }
}