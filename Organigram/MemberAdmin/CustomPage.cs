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
using System.Security.Cryptography.X509Certificates;
using Pirate.Ldap.Web;

namespace MemberAdmin
{
  /// <summary>
  /// Parent class for all custom pages.
  /// </summary>
  public abstract class CustomPage : GeneralPage
  {
    private const string CaCertificateData =
      "-----BEGIN CERTIFICATE-----" +
      "MIIEPzCCAyegAwIBAgIJAOduOZlsLZzIMA0GCSqGSIb3DQEBBQUAMIG1MQswCQYD" +
      "VQQGEwJDSDELMAkGA1UECAwCVkQxETAPBgNVBAcMCFZhbGxvcmJlMSEwHwYDVQQK" +
      "DBhQaXJhdGUgUGFydHkgU3dpdHplcmxhbmQxGDAWBgNVBAsMD01lbWJlciBEYXRh" +
      "YmFzZTEeMBwGA1UEAwwVQ2VydGlmaWNhdGUgQXV0aG9yaXR5MSkwJwYJKoZIhvcN" +
      "AQkBFhpyZWdpc3RyYXJAcGlyYXRlbnBhcnRlaS5jaDAeFw0xMzA0MTQxNjA3NDha" +
      "Fw0yMzA0MTIxNjA3NDhaMIG1MQswCQYDVQQGEwJDSDELMAkGA1UECAwCVkQxETAP" +
      "BgNVBAcMCFZhbGxvcmJlMSEwHwYDVQQKDBhQaXJhdGUgUGFydHkgU3dpdHplcmxh" +
      "bmQxGDAWBgNVBAsMD01lbWJlciBEYXRhYmFzZTEeMBwGA1UEAwwVQ2VydGlmaWNh" +
      "dGUgQXV0aG9yaXR5MSkwJwYJKoZIhvcNAQkBFhpyZWdpc3RyYXJAcGlyYXRlbnBh" +
      "cnRlaS5jaDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMU1Ot8c+ncK" +
      "Md6ZhX8b6zqm8zNSKP+uy632Zz/ThNWZBYtqs2EHMQWUqWjXQEB6QG4pAuYyXV4y" +
      "/z8A8puKpGas7sBknhqsY6einIhpCGcRUCleuZ25bFM+Oeu98o1tJbtgBskqfhBq" +
      "7FmNAS4M/A4BQg16iEDaXANmf/U1Iar/xCaoDl91hvC8FtM0RLOEWmMTeOL5qVnT" +
      "CLcMpyWMRB6jjNmcV2JzPEVFqc+KcMN+WVkINwYhyy1g07iLMqYAZ23cQGW5S+xa" +
      "shvzpjj9Qr6ffYEfBt4IpJ5hSgUit1+Yp5QbXmK0wIcufMn8fg9nvkxAl+uArtra" +
      "4TckhWvFfsMCAwEAAaNQME4wHQYDVR0OBBYEFFvd82lKd8IV+PohnZ+JPakds+35" +
      "MB8GA1UdIwQYMBaAFFvd82lKd8IV+PohnZ+JPakds+35MAwGA1UdEwQFMAMBAf8w" +
      "DQYJKoZIhvcNAQEFBQADggEBACwUyaTmtAJA+QHeOROY1xtFExKQIgpcbEVM8VMD" +
      "mNOqn062iPUioPy2IdQ/GhsoPMNT9PMz7FMkHTjp6H/QzTs1D4O6kEVdCeiV999a" +
      "6T0Gv53jDv1YiyBUvJ6rsLh0mFiGEmcQlf2soGJl98Ecw5n6u2HmZ8Sd5szqIxN4" +
      "MQonJS2Cl6mlNT41Ur/8Z10xk81CiIGz26DAcnVkMMTcZVo9JZTcquGXg89aa4Vz" +
      "IwxlmDlcRE92AvXPLa7+A0b2pwSs5dz81V8Z3b/RX1vv6v4XZeM/Vsz1Ru169Br7" +
      "N/EidcdhUplSiziqItPQw16EXE3AjgIwVz5/r8Yhv+CVmas=" +
      "-----END CERTIFICATE-----";

    protected byte[] CaCertificateBinary
    {
      get
      {
        var parts = CaCertificateData.Split(new[] { "-----BEGIN CERTIFICATE-----", "-----END CERTIFICATE-----" }, StringSplitOptions.None);

        if (parts.Length == 3)
        {
          return Convert.FromBase64String(parts[1]);
        }
        else
        {
          throw new InvalidOperationException();
        }
      }
    }

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

    protected string CurrentUserText
    {
      get { return string.Format("User id {0} name {1}", CurrentUser.Id, CurrentUser.Name); }
    }

    /// <summary>
    /// Gets the current user authorization level.
    /// </summary>
    protected int CurrentUserAuthorizationLevel { get; private set; }

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

    protected X509Certificate2 ClientCertificate
    {
      get
      {
        if (Request.ClientCertificate.IsPresent &&
            Request.ClientCertificate.Issuer.Contains("Member Database"))
        {
          return new X509Certificate2(Request.ClientCertificate.Certificate);
        }
        else if (System.Diagnostics.Debugger.IsAttached)
        {
          return new X509Certificate2(@"d:\security\pps\mdb-debug.crt");
        }
        else
        {
          return null;
        }
      }
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
            var access = Ldap.Connect(Application, Session, Request);

            if (access != null)
            {
              var currentUser = access.Connection.SearchFirst<Person>(Ldap.GetUserDn(Application, Session, Request), LdapScope.Base);
              var certificate = ClientCertificate;

              if (certificate != null)
              {
                var authorizationLevel = DataGlobal.DataAccess.GetCertificateAuthorizationLevel(certificate.Thumbprint, currentUser.Id);

                if (authorizationLevel >= 1)
                {
                  Thread.CurrentThread.CurrentCulture = Language.GetCulture(currentUser.PreferredLanguage);
                  Resources.Culture = Thread.CurrentThread.CurrentCulture;
                  LdapResources.Culture = Thread.CurrentThread.CurrentCulture;
                  CurrentUser = currentUser;
                  CurrentUserAuthorizationLevel = authorizationLevel;
                  Access = access;
                  return true;
                }
              }
            }

            return false;
          }
          else if (CurrentUser == null)
          {
            var currentUser = Access.Connection.SearchFirst<Person>(Ldap.GetUserDn(Application, Session, Request), LdapScope.Base);
            var certificate = ClientCertificate;

            if (certificate != null)
            {
              var authorizationLevel = DataGlobal.DataAccess.GetCertificateAuthorizationLevel(certificate.Thumbprint, currentUser.Id);

              if (authorizationLevel >= 1)
              {
                Thread.CurrentThread.CurrentCulture = Language.GetCulture(currentUser.PreferredLanguage);
                Resources.Culture = Thread.CurrentThread.CurrentCulture;
                LdapResources.Culture = Thread.CurrentThread.CurrentCulture;
                CurrentUser = currentUser;
                CurrentUserAuthorizationLevel = authorizationLevel;
                return true;
              }
            }

            return false;
          }
          else
          {
            return true;
          }
        });
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
      return "./" + target;
    }

    /// <summary>
    /// Redirects to the commit page.
    /// </summary>
    protected void RedirectCommit()
    {
      Redirect("Commit.aspx");
    }

    protected override string HomePage
    {
      get { return "SearchMember"; }
    }

    protected override string LoginPage
    {
      get { return "Login"; }
    }
  }
}