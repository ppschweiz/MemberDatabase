using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using Pirate.Util.Logging;
using System.Diagnostics;

namespace Pirate.Ldap.Web
{
  public class Mailer
  {
    private Config.IConfig _config;

    public Mailer(Config.IConfig config)
    {
      _config = config;
    }

    public void SendToMembersQueue(string subject, string body)
    {
      Send(subject, body, EncodeAddress(_config.RegistrarNotificationAddress, _config.RegistrarNotificationDisplayName));
    }

    public void Send(string subject, string body, MailAddress to)
    {
      Send(subject, body, new[] { to });
    }

    private static string EncodeDisplayName(string displayName)
    {
      return "=?utf-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(displayName)) + "?=";
    }

    public static MailAddress EncodeAddress(string address, string displayName)
    { 
      return new MailAddress(address, EncodeDisplayName(displayName), Encoding.ASCII);
    }

    public void SendToAdmin(string prefix, Exception exception, params Info[] infos)
    {
      var text = new StringBuilder();
      text.AppendLine("Time: {0}", DateTime.Now);
      text.AppendLine("Server: {0}", Environment.MachineName);
      text.AppendLine("Application: {0}", BasicGlobal.ApplicationName);

      foreach (var info in infos)
      {
        text.AppendLine("{0}: {1}", info.Name, info.Value);
      }

      text.AppendLine(exception.ToString());
      string subject = string.Format("{0} on {1} in {2}", prefix, Environment.MachineName, BasicGlobal.ApplicationName);

      SendToAdmin(subject, text.ToString());
    }

    public void SendToAdmin(string subject, string body)
    {
      Send(subject, body, EncodeAddress(_config.AdminAddress, _config.AdminDisplayName));
    }

    public void SendTextile(string subject, string body, MailAddress to)
    {
      SendTextile(subject, body, new[] { to });
    }

    public void SendTextile(string subject, string body, IEnumerable<MailAddress> to)
    {
      foreach (var address in to)
      {
        try
        {
          var newMessage = new MailMessage();
          newMessage.BodyEncoding = Encoding.UTF8;
          newMessage.SubjectEncoding = Encoding.UTF8;
          newMessage.Subject = subject;
          var text = Pirate.Textile.TextParser.Parse(body);

          var textBody = AlternateView.CreateAlternateViewFromString(text.ToText(), Encoding.UTF8, "text/plain");
          textBody.TransferEncoding = TransferEncoding.Base64;
          newMessage.AlternateViews.Add(textBody);

          var htmlBody = AlternateView.CreateAlternateViewFromString(text.ToHtml(), Encoding.UTF8, "text/html");
          htmlBody.TransferEncoding = TransferEncoding.Base64;
          newMessage.AlternateViews.Add(htmlBody);

          newMessage.From = new MailAddress(_config.SystemSenderAddress, _config.SystemSenderDisplayName);
          newMessage.To.Add(address);

          var addressText = string.Join("; ", newMessage.To.Select(x => x.Address).ToArray());

          Send(newMessage);
        }
        catch (Exception exception)
        {
          BasicGlobal.Logger.Log(LogLevel.Error, "Cannot send mail with subject {0} to {1} because {2}", subject, address, exception.ToString());
        }
      }
    }

    public void Send(string subject, string body, IEnumerable<MailAddress> to)
    {
      foreach (var address in to)
      {
        try
        {
          var newMessage = new MailMessage();
          newMessage.BodyEncoding = Encoding.UTF8;
          newMessage.SubjectEncoding = Encoding.UTF8;
          newMessage.Subject = subject;
          newMessage.Body = body.Replace(Environment.NewLine, "\r\n");
          newMessage.From = new MailAddress(_config.SystemSenderAddress, _config.SystemSenderDisplayName);
          newMessage.To.Add(address);

          Send(newMessage);
        }
        catch (Exception exception)
        {
          BasicGlobal.Logger.Log(LogLevel.Error, "Cannot send mail with subject {0} to {1} because {2}", subject, address, exception.ToString());
        }
      }
    }

    public void Send(MailMessage message)
    {
      if (Debugger.IsAttached)
      {
        message.To.Clear();
        message.To.Add(EncodeAddress("stefan.thoeni@piratenpartei.ch", "Stefan Thöni"));
      }

      var client = new SmtpClient();
      client.Host = _config.MailServerAddress;
      client.Port = _config.MailServerPort;
      client.Send(message);
    }
  }
}