using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;
using System.Text.RegularExpressions;
using System.IO;

namespace RegistrationService
{
  public class Global : System.Web.HttpApplication
  {
    private static LdapAccess _ldapAccess;

    public static LdapAccess LdapAccess
    {
      get
      {
        if (_ldapAccess == null)
        {
          _ldapAccess = Ldap.Connect(null);
        }

        return _ldapAccess;
      }
    }

    public Global()
    {
      string logFileName =
        Environment.OSVersion.Platform == PlatformID.Unix ?
        "/var/log/mdb/registrationservice" :
        Path.GetTempFileName();
      BasicGlobal.Init("RegistrationService", logFileName);
    }

    protected void Application_Start(object sender, EventArgs e)
    {
      BasicGlobal.Logger.Log(LogLevel.Info, "RegistrationService application started.");
      DataGlobal.Init();
      _ldapAccess = Ldap.Connect(null);
    }

    protected void Session_Start(object sender, EventArgs e)
    {

    }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {
      if (Request.UserAgent.Contains("bingbot"))
      {
        Response.StatusCode = 403;
        CompleteRequest();
      }
      else if (Request.Url.AbsolutePath.Contains("authentication"))
      {
        Response.Redirect("/BeginPasswordReset.aspx");
        CompleteRequest();
      }
    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e)
    {

    }

    protected void Application_Error(object sender, EventArgs e)
    {
      var error = Server.GetLastError().GetBaseException();
      BasicGlobal.LogAndReportError(error, Request, new Info("Source", "ApplicationError"));
      Server.ClearError();
    }

    protected void Session_End(object sender, EventArgs e)
    {

    }

    protected void Application_End(object sender, EventArgs e)
    {
      DataGlobal.Dispose();
    }
  }
}