using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util;
using Pirate.Util.Logging;
using Pirate.Ldap.Config;

namespace RegistrationService
{
  public static class Ldap
  {
    private const string KeyObjectName = "credentialKey";
    private const string CredentialsObjectName = "ldapcredentials";

    public static LdapAccess Connect(HttpRequest request)
    {
      try
      {
        var config = new Config();
        var connection = new LdapConnection();
        connection.Connect(config.LdapServer, 389);
        connection.Bind(config.MdbguiDn, config.MdbguiPassword);

        BasicGlobal.Logger.Log(LogLevel.Debug, "Ldap connected.");
        return new LdapAccess(connection);
      }
      catch (Exception exception)
      {
        BasicGlobal.LogAndReportError(
          exception, 
          request,
          new Info("Action", "Ldap.Connect"));

        return null;
      }
    }
  }
}