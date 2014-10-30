using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Util;
using Pirate.Util.Logging;
using Pirate.Ldap.Config;

namespace Pirate.Ldap.Web
{
  public static class Ldap
  {
    private const string KeyObjectName = "credentialKey";
    private const string CredentialsObjectName = "ldapcredentials";

    private static byte[] GetKey(HttpApplicationState application)
    {
      var key = application[KeyObjectName] as byte[];

      if (key == null)
      {
        key = Crypto.GetRandom(32);
        application.Add(KeyObjectName, key);
      }

      return key;
    }

    public static string GetUserDn(HttpApplicationState application, HttpSessionState session, HttpRequest request)
    {
      try
      {
        var key = GetKey(application);
        var data = session[CredentialsObjectName] as byte[];

        if (data == null)
        {
          BasicGlobal.Logger.Log(LogLevel.Info, "No LDAP credential object present at LDAP.GetUserDn.");
          return string.Empty;
        }

        var plain = Crypto.Decrypt(key, data);
        var credentials = new CredentionalObject(plain);
        return credentials.UserDn;
      }
      catch (Exception exception)
      {
        BasicGlobal.LogAndReportError(
          exception,
          request,
          new Info("Action", "Ldap.GetUserDn"));
        return string.Empty;
      }
    }

    public static void Logoff(HttpApplicationState application, HttpSessionState session, HttpRequest request)
    {
      try
      {
        var key = GetKey(application);
        var data = session[CredentialsObjectName] as byte[];

        if (data == null)
        {
          BasicGlobal.Logger.Log(LogLevel.Info, "No LDAP credential object present at LDAP.Logoff.");
          return;
        }

        var plain = Crypto.Decrypt(key, data);
        var credentials = new CredentionalObject(plain);

        credentials.Disable();
        var newPlain = credentials.ToBinary();
        var newData = Crypto.Encrypt(key, newPlain);
        session.Remove(CredentialsObjectName);
        session.Add(CredentialsObjectName, newData);
      }
      catch (Exception exception)
      {
        BasicGlobal.LogAndReportError(
          exception,
          request,
          new Info("Action", "Ldap.Logoff"));
      }

      session.Abandon();
    }

    public static LdapConnection GetBindConnection()
    {
      var config = new Config.Config();
      var connection = new LdapConnection();
      connection.Connect(config.LdapServer, 389);
      connection.Bind(config.BindDn, config.BindPassword);
      BasicGlobal.Logger.Log(LogLevel.Debug, "Bind connection established");
      return connection;
    }

    public static LdapAccess Connect(
      HttpApplicationState application,
      HttpSessionState session,
      HttpRequest request )
    {
      try
      {
        var config = new Config.Config();
        var key = GetKey(application);
        var data = session[CredentialsObjectName] as byte[];

        if (data == null)
        {
          BasicGlobal.Logger.Log(LogLevel.Info, "No LDAP credential object present at LDAP.Connect.");
          return null;
        }

        var plain = Crypto.Decrypt(key, data);
        var credentials = new CredentionalObject(plain);

        BasicGlobal.Logger.Log(LogLevel.Debug, "Ldap.Connect " + credentials.UserDn);

        if (DateTime.Now > credentials.Expires)
        {
          BasicGlobal.Logger.Log(LogLevel.Debug, "Ldap.Connect credentials expired for user {0}.", credentials.UserDn);
          return null;
        }

        credentials.Update();
        var newPlain = credentials.ToBinary();
        var newData = Crypto.Encrypt(key, newPlain);
        session.Remove(CredentialsObjectName);
        session.Add(CredentialsObjectName, newData);

        var connection = new LdapConnection();
        connection.Connect(config.LdapServer, 389);
        connection.Bind(credentials.UserDn, credentials.Password);

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

    public static bool Login(HttpApplicationState application, HttpSessionState session, HttpRequest request, string username, string password)
    {
      if (string.IsNullOrEmpty(username) ||
          string.IsNullOrEmpty(password))
      {
        return false;
      }

      var clientIpAddress = request == null || request.UserHostAddress == null ? "N/A" : request.UserHostAddress;

      var config = new Config.Config();
      var connection = new LdapConnection();
      connection.Connect(config.LdapServer, 389);
      connection.Bind(config.BindDn, config.BindPassword);

      try
      {
        var result = connection.Search(Names.Party, (int)LdapScope.Sub, new LdapAttributeFilter("uid", username).Text, new string[0], false);
        var user = result.next();
        connection = new LdapConnection();
        connection.Connect(config.LdapServer, 389);
        connection.Bind(user.DN, password);

        var key = GetKey(application);
        var credentials = new CredentionalObject(user.DN, password);
        var plain = credentials.ToBinary();
        var data = Crypto.Encrypt(key, plain);
        session.Add(CredentialsObjectName, data);
        BasicGlobal.Logger.Log(LogLevel.Info, "LDAP.Login sucessful for user {0} from IP {1}.", credentials.UserDn, clientIpAddress);

        return true;
      }
      catch (LdapException exception)
      {
        var ldapResult = (LdapResultCode)exception.ResultCode;

        switch (ldapResult)
        {
          case LdapResultCode.LocalError:
            BasicGlobal.Logger.Log(LogLevel.Info, "LDAP.Login failed from IP {1}. No user named {0}.", username, clientIpAddress);
            break;
          case LdapResultCode.InvalidCredentials:
            BasicGlobal.Logger.Log(LogLevel.Info, "LDAP.Login failed. Invalid credentials for user {0} from IP {1}.", username, clientIpAddress);
            break;
          default:
            BasicGlobal.LogAndReportError(
              exception,
              request,
              new Info("Action", "Ldap.Login"),
              new Info("Username", username),
              new Info("LdapResult", ldapResult));
            break;
        }

        return false;
      }
      catch (Exception exception)
      {
        BasicGlobal.LogAndReportError(
          exception,
          request,
          new Info("Action", "Ldap.Login"),
          new Info("Username", username));
        return false;
      }
    }

    public static bool TestLogin(HttpRequest request, string username, string password)
    {
      if (string.IsNullOrEmpty(username) ||
          string.IsNullOrEmpty(password))
      {
        return false;
      } 
      
      var config = new Config.Config();
      var connection = new LdapConnection();
      connection.Connect(config.LdapServer, 389);
      connection.Bind(config.BindDn, config.BindPassword);

      try
      {
        var result = connection.Search(Names.Party, (int)LdapScope.Sub, new LdapAttributeFilter("uid", username).Text, new string[0], false);
        var user = result.next();
        connection = new LdapConnection();
        connection.Connect(config.LdapServer, 389);
        connection.Bind(user.DN, password);
        return true;
      }
      catch (LdapException exception)
      {
        var ldapResult = (LdapResultCode)exception.ResultCode;

        switch (ldapResult)
        {
          case LdapResultCode.LocalError:
            BasicGlobal.Logger.Log(LogLevel.Info, "LDAP.TestLogin failed. No user named {0}.", username);
            break;
          case LdapResultCode.InvalidCredentials:
            BasicGlobal.Logger.Log(LogLevel.Info, "LDAP.TestLogin failed. Invalid credentials for user {0}", username);
            break;
          default:
            BasicGlobal.LogAndReportError(
              exception,
              request,
              new Info("Action", "LDAP.TestLogin"),
              new Info("Username", username),
              new Info("LdapResult", ldapResult));
            break;
        }

        return false;
      }
      catch (Exception exception)
      {
        BasicGlobal.LogAndReportError(
          exception,
          request,
          new Info("Action", "Ldap.TestLogin"),
          new Info("Username", username));
        return false;
      }
    }
  }
}