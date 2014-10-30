using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Pirate.Ldap;
using Pirate.Util.Logging;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using System.IO;

namespace MemberAdmin
{
  public class Global : System.Web.HttpApplication
  {
    public Global()
    {
      string logFileName =
        Environment.OSVersion.Platform == PlatformID.Unix ?
        "/var/log/mdb/memberadmin" :
        Path.GetTempFileName();
      BasicGlobal.Init("MemberAdmin", logFileName);
    }

    protected void Application_Start(object sender, EventArgs e)
    {
      BasicGlobal.Logger.Log(LogLevel.Info, "MemberAdmin application started.");
      DataGlobal.Init();
    }

    protected void Session_Start(object sender, EventArgs e)
    {

    }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {

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