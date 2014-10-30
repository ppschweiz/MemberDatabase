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
using Pirate.Ldap.Data;
using Pirate.Util.Logging;
using Pirate.Ldap.Web;

namespace MemberService
{
  /// <summary>
  /// Parent class for all custom pages.
  /// </summary>
  public abstract class CustomPage : GeneralPage
  {
    /// <summary>
    /// Gets the LDAP context.
    /// </summary>
    protected LdapAccess Access { get; private set; }

    /// <summary>
    /// Gets the LDAP connection.
    /// </summary>
    protected LdapConnection Connection { get { return Access.Connection; } }

    /// <summary>
    /// Gets the current LDAP user.
    /// </summary>
    protected Person CurrentUser { get; private set; }

    /// <summary>
    /// Gets or sets the state of the update.
    /// </summary>
    /// <value>
    /// The state of the update.
    /// </value>
    protected UpdateState UpdateState
    {
      get { return Session["updatestate"] as UpdateState; }
      set { Session["updatestate"] = value; }
    }

    /// <summary>
    /// Setups the LDAP connection. Will also set the current
    /// user and the UI language.
    /// </summary>
    /// <returns></returns>
    protected bool SetupLdap()
    {
      return ExecuteHandlingError(
        "SetupLdap",
        () =>
        {
          if (Access == null)
          {
            Access = Ldap.Connect(Application, Session, Request);

            if (Access != null)
            {
              CurrentUser = Connection.SearchFirst<Person>(Ldap.GetUserDn(Application, Session, Request), LdapScope.Base);
              Thread.CurrentThread.CurrentCulture = Language.GetCulture(CurrentUser.PreferredLanguage);
              Resources.Culture = Thread.CurrentThread.CurrentCulture;
              LdapResources.Culture = Thread.CurrentThread.CurrentCulture;
              return true;
            }
            else
            {
              return false;
            }
          }
          else
          {
            return true;
          }
        });
    }

    /// <summary>
    /// Redirects to the unauthorized page.
    /// </summary>
    protected void RedirectUnauthorized()
    {
      Redirect("Unauthorized.aspx");
    }

    /// <summary>
    /// Redirects to the commit page.
    /// </summary>
    protected void RedirectCommit()
    {
      Redirect("Commit.aspx");
    }

    protected void RedirectMessage(Texts.UserItem item, string continueTo = "Default.aspx")
    {
      Session[DisplayMessage.SessionDataKey] = new Message(item.Id, continueTo);
      Redirect("DisplayMessage.aspx");
    }

    /// <summary>
    /// Redirects the specified target.
    /// </summary>
    /// <param name="target">The target.</param>
    protected override void Redirect(string target)
    {
      Response.Redirect(GetTarget(target), false);
    }

    /// <summary>
    /// Gets the target.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <returns></returns>
    protected string GetTarget(string target)
    {
      return target;
    }

    protected override string HomePage
    {
      get { return "Default"; }
    }

    protected override string LoginPage
    {
      get { return "Login"; }
    }
  }
}