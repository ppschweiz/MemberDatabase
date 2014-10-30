using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public class NextIdService : IHttpHandler
  {
    public bool IsReusable
    {
      get { return true; }
    }

    public void ProcessRequest(HttpContext context)
    {
      var connection = Ldap.GetBindConnection();
      var nextId = connection.GetNextPersonId();
      BasicGlobal.Logger.Log(LogLevel.Debug, "Unique id {0} handed out.", nextId);

      context.Response.Clear();
      context.Response.ContentEncoding = Encoding.UTF8;
      context.Response.ContentType = "text/plain";
      context.Response.Write(nextId.ToString());
      context.Response.Flush();
    }
  }
}
