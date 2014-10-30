using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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
  /// <summary>
  /// Data output handler for the photo of a Person.
  /// </summary>
  public class OutputMemberPhoto : IHttpHandler, IRequiresSessionState
  {
    public void ProcessRequest(HttpContext context)
    {
      var uidParam = context.Request.Params["member"] ?? "246";
      var uid = Convert.ToInt32(uidParam);

      var access = Ldap.Connect(context.Application, context.Session, context.Request);

      if (access != null)
      {
        var person = access.Connection.SearchFirst<Person>(Names.Party, LdapScope.Sub, new LdapAttributeFilter("uniqueIdentifier", uid));

        if (person != null)
        {
          context.Response.Clear();
          context.Response.ContentEncoding = Encoding.UTF8;
          context.Response.ContentType = "image/jpeg";
          person.Photo.Save(context.Response.OutputStream, ImageFormat.Jpeg);
          context.Response.OutputStream.Flush();
        }
      }
    }

    public bool IsReusable
    {
      get { return false; }
    }
  }
}
