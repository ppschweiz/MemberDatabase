using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using Pirate.Util.Logging;

namespace MemberService
{
  public static class Mailer
  {
    public static void Send(ILogger logger, string subject, string body, string to)
    {
      Send(logger, subject, body, new[] { to });
    }

    public static void SendTextile(ILogger logger, string subject, string body, string to)
    {
      SendTextile(logger, subject, body, new[] { to });
    }

    public static void SendTextile(ILogger logger, string subject, string body, IEnumerable<string> to)
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

          newMessage.From = new MailAddress("members@piratenpartei.ch", "MemberService");
          newMessage.To.Add(new MailAddress(address));

          var addressText = string.Join("; ", newMessage.To.Select(x => x.Address).ToArray());

          Send(newMessage);
        }
        catch (Exception exception)
        {
          logger.Log(LogLevel.Error, "Cannot send mail with subject {0} to {1} because {2}", subject, address, exception.ToString());
        }
      }
    }

    public static void Send(ILogger logger, string subject, string body, IEnumerable<string> to)
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
          newMessage.From = new MailAddress("members@piratenpartei.ch", "MemberService");
          newMessage.To.Add(address);

          Send(newMessage);
        }
        catch (Exception exception)
        {
          logger.Log(LogLevel.Error, "Cannot send mail with subject {0} to {1} because {2}", subject, address, exception.ToString());
        }
      }
    }

    private static void Send(MailMessage message)
    {
      var client = new SmtpClient();
      client.Host = "mail.piratenpartei.ch";
      client.Port = 25;
      client.Send(message);
    }
  }
}