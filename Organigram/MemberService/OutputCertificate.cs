using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
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
  public class OutputCertificate : CustomHttpHandler, IRequiresSessionState
  {
    protected override string SourceName
    {
      get { return "OutputCertificate"; }
    }

    public override void ProcessRequest(HttpContext context)
    {
      if (SetupLdap(context))
      {
        var user = CurrentUser;

        try
        {
          var key = Convert.ToInt64(context.Request.Params["key"]);
          var entry = DataGlobal.DataAccess.FindCertificateEntry(key);

          if (entry.UserUniqueIdentifier == user.Id)
          {
            var executor = new Executor();
            var certificateDataDer = executor.ConvertPemToDer(entry.CertificateData);
            context.Response.BinaryWrite(certificateDataDer);
            context.Response.ContentType = "application/x-x509-user-cert";
            BasicGlobal.Logger.Log(LogLevel.Info, "OutputCertificate deliverying certificate key {0} fingerprint {1} to user id {2}, name {3}", entry.Key, entry.Fingerprint, user.Id, user.Name);
          }
          else
          {
            BasicGlobal.Logger.Log(LogLevel.Warning, "OutputCertificate access key for user id {0} name {1} to {2} fingerprint {3} of user id {4} denied.", user.Id, user.Name, entry.Key, entry.Fingerprint, entry.UserUniqueIdentifier);
            context.Response.Write("Access Denied");
            context.Response.ContentType = "text/plain";
          }
        }
        catch (Exception exception)
        {
          BasicGlobal.Logger.Log(LogLevel.Error, "OutputCertificate failed: " + exception.Message);
          BasicGlobal.Mailer.SendToAdmin("Error", exception, new Info("Source", "OutputCertificate"), new Info("Action", "ProcessRequest"));
          context.Response.Write("Exception: " + exception.Message);
          context.Response.ContentType = "text/plain";
        }
      }
    }
  }
}
