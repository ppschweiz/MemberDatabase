using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirate.Ldap.Config;
using Pirate.Util;
using Pirate.Util.Logging;
using System.Web;

namespace Pirate.Ldap.Web
{
  public static class BasicGlobal
  {
    private static Dictionary<string, RateLimitEntry> _rateLimitList;

    public static string ApplicationName { get; private set; }

    public static ILogger Logger { get; private set; }

    public static IConfig Config { get; private set; }

    public static Mailer Mailer { get; private set; }

    public static void Init(string applicationName, string logFileName)
    {
      ApplicationName = applicationName;
      _rateLimitList = new Dictionary<string,RateLimitEntry>();
      Logger = new Logger(LogLevel.Debug, logFileName);
      Config = new Config.Config();
      Mailer = new Mailer(Config);
    }

    public static void Dispose()
    { 
    }

    private static IEnumerable<Info> GetRequestInfo(HttpRequest request)
    {
      if (request != null)
      {
        if (request.RawUrl != null)
        {
          yield return new Info("Url", request.RawUrl);
        }

        if (request.HttpMethod != null)
        {
          yield return new Info("Method", request.HttpMethod);
        }

        if (request.UserAgent != null)
        {
          yield return new Info("UserAgent", request.UserAgent);
        }

        if (request.UserHostAddress != null)
        {
          yield return new Info("IpAddress", request.UserHostAddress);
        }
      }
    }

    private static bool CheckRateLimit(Exception exception, HttpRequest request, List<Info> infos)
    {
      var ip = request != null && request.UserHostAddress != null ? request.UserHostAddress : "N/A";
      var key = exception.GetType().FullName + "$$$" + exception.StackTrace + "$$$" + ip;

      if (_rateLimitList.ContainsKey(key))
      {
        var entry = _rateLimitList[key];

        if (DateTime.Now.Subtract(entry.FirstLogged).TotalMinutes >= 60)
        {
          if (entry.Count > 0)
          {
            infos.Add(new Info("Limited since", entry.FirstLogged));
            infos.Add(new Info("Limited entries", entry.Count));
          }

          entry.FirstLogged = DateTime.Now;
          entry.Count = 0;
          return true;
        }
        else
        {
          entry.Count++;
          return false;
        }
      }
      else
      {
        _rateLimitList.Add(key, new RateLimitEntry(key));
        return true;
      }
    }

    public static void LogAndReportError(Exception exception, HttpRequest request, params Info[] infos)
    {
      var allInfos = GetRequestInfo(request).Concat(infos).ToList();

      string data = string.Join(" ", allInfos.Select(i => i.Name + "=" + i.Value).ToArray());
      BasicGlobal.Logger.Log(LogLevel.Error, "Error " + data);
      BasicGlobal.Logger.Log(LogLevel.Error, exception.ToString());

      if (CheckRateLimit(exception, request, allInfos))
      {
        BasicGlobal.Mailer.SendToAdmin("Error", exception, allInfos.ToArray());
      }
    }

    public static void LogAndReportWarning(Exception exception, HttpRequest request, params Info[] infos)
    {
      var allInfos = GetRequestInfo(request).Concat(infos).ToList();

      string data = string.Join(" ", allInfos.Select(i => i.Name + "=" + i.Value).ToArray());
      BasicGlobal.Logger.Log(LogLevel.Warning, "Warning " + data);
      BasicGlobal.Logger.Log(LogLevel.Warning, exception.ToString());

      if (CheckRateLimit(exception, request, allInfos))
      {
        BasicGlobal.Mailer.SendToAdmin("Warning", exception, allInfos.ToArray());
      }
    }
  }
}
