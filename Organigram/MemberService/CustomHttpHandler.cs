using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Openssl;
using Pirate.Util.Logging;
using Pirate.Ldap.Web;

namespace MemberService
{
  public abstract class CustomHttpHandler : GeneralHttpHandler
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
    /// Setups the LDAP connection. Will also set the current
    /// user and the UI language.
    /// </summary>
    /// <returns></returns>
    protected bool SetupLdap(HttpContext context)
    {
      Access = null;
      CurrentUser = null;

      return ExecuteHandlingError(
        context,
        "SetupLdap",
        () =>
        {
          if (Access == null)
          {
            Access = Ldap.Connect(context.Application, context.Session, context.Request);

            if (Access != null)
            {
              CurrentUser = Connection.SearchFirst<Person>(Ldap.GetUserDn(context.Application, context.Session, context.Request), LdapScope.Base);
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

    protected string FixEmpty(string input)
    {
      return string.IsNullOrEmpty(input) ? "!N/A!" : input;
    }
  }
}
