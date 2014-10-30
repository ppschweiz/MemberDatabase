using System;
using System.Collections.Generic;
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
  public abstract class GeneralHttpHandler : IHttpHandler, IRequiresSessionState
  {
    protected abstract string SourceName { get; }

    public void HandleError(HttpContext context, Exception exception, string action)
    {
      var username = Ldap.GetUserDn(context.Application, context.Session, context.Request);
      BasicGlobal.LogAndReportError(
        exception,
        context.Request,
        new Info("Source", SourceName),
        new Info("Action", action),
        new Info("User", username));
    }

    private void HandleLdapError(HttpContext context, LdapException exception, string action)
    {
      var username = Ldap.GetUserDn(context.Application, context.Session, context.Request);
      var ldapResult = (LdapResultCode)exception.ResultCode;

      switch (ldapResult)
      {
        case LdapResultCode.InsufficientAccessRights:
          BasicGlobal.LogAndReportWarning(
            exception,
            context.Request,
            new Info("Source", SourceName),
            new Info("Action", action),
            new Info("User", username),
            new Info("LdapResult", ldapResult));
          break;
        default:
          BasicGlobal.LogAndReportError(
            exception,
            context.Request,
            new Info("Source", SourceName),
            new Info("Action", action),
            new Info("User", username),
            new Info("LdapResult", ldapResult));
          break;
      }
    }

    protected bool ExecuteHandlingError(HttpContext context, string text, Action action)
    {
      try
      {
        action();
        return true;
      }
      catch (LdapException exception)
      {
        HandleLdapError(context, exception, text);
        return false;
      }
      catch (Exception exception)
      {
        HandleError(context, exception, text);
        return false;
      }
    }

    protected TResult ExecuteHandlingError<TResult>(HttpContext context, string text, Func<TResult> action)
    {
      try
      {
        return action();
      }
      catch (LdapException exception)
      {
        HandleLdapError(context, exception, text);
        return default(TResult);
      }
      catch (Exception exception)
      {
        HandleError(context, exception, text);
        return default(TResult);
      }
    }

    public bool IsReusable
    {
      get { return false; }
    }

    public abstract void ProcessRequest(HttpContext context);
  }
}
