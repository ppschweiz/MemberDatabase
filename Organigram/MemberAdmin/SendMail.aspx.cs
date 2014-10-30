using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Textile;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public partial class SendMail : CustomPage
  {
    protected override string PageName
    {
      get { return "SendMail"; }
    }
    
    public const string MailSessionObject = "mailmessage";

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var message = Session[MailSessionObject] as MailMessage;

      var table = new Table();

      if (message != null)
      {
        table.AddRow(Resources.From, message.From.Address);
        table.AddRow(Resources.To, Flatten(message.To));
        table.AddRow(Resources.CC, Flatten(message.CC));
        table.AddRow(Resources.BCC, Flatten(message.Bcc));
        table.AddRow(Resources.Subject, message.Subject);
        table.AddRow(Resources.Body, message.Body.Replace(Environment.NewLine, "<br>"));
        table.AddRow(Resources.Attachement, Multiline(message.Attachments.Select(a => a.Name).ToArray()));

        var sendButton = new Button();
        sendButton.Text = Resources.Send;
        sendButton.Click += new EventHandler(SendButton_Click);
        table.AddRow(string.Empty, sendButton);
      }
      else
      {
        table.AddRow(Resources.MailNotReady);
      }

      this.panel.Controls.Add(table);
    }

    private void SendButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      Response.Clear();
      Response.BufferOutput = false;
      var writer = new System.IO.StreamWriter(Response.OutputStream, Encoding.UTF8);
      writer.Write("<html><body>");
      writer.Flush();

      try
      {
        var message = (MailMessage)Session[MailSessionObject];

        BasicGlobal.Logger.Log(LogLevel.Info, "User {0} is sending mail with subject {1}.", CurrentUser.DN, message.Subject);

        writer.Write("<p>Sending mail {0}...</p>", message.Subject);
        writer.Flush();

        var addressText = string.Empty;

        foreach (var address in message.Bcc)
        {
          try
          {
            var newMessage = new MailMessage();
            newMessage.BodyEncoding = Encoding.UTF8;
            newMessage.SubjectEncoding = Encoding.UTF8;
            newMessage.Subject = message.Subject;
            var text = Pirate.Textile.TextParser.Parse(message.Body);

            var textBody = AlternateView.CreateAlternateViewFromString(text.ToText(), Encoding.UTF8, "text/plain");
            textBody.TransferEncoding = TransferEncoding.Base64;
            newMessage.AlternateViews.Add(textBody);

            var htmlBody = AlternateView.CreateAlternateViewFromString(text.ToHtml(), Encoding.UTF8, "text/html");
            htmlBody.TransferEncoding = TransferEncoding.Base64;
            newMessage.AlternateViews.Add(htmlBody);

            foreach (var attachement in message.Attachments)
            {
              attachement.ContentStream.Position = 0;
              newMessage.Attachments.Add(attachement);
            }

            newMessage.From = message.From;
            newMessage.To.Add(address);

            addressText = string.Join("; ", newMessage.To.Select(x => x.Address).ToArray());

            BasicGlobal.Logger.Log(
              LogLevel.Info,
              "User {0} sent mail from {1} to {2} subject {3}",
              CurrentUser.DN,
              newMessage.From.Address,
              addressText,
              newMessage.Subject);

            writer.Write(
              "<p>Message sent to {0}</p>",
              addressText);
            writer.Flush();

            BasicGlobal.Mailer.Send(newMessage);
          }
          catch (Exception exception)
          {
            BasicGlobal.Logger.Log(LogLevel.Error, "Error sending mail by {0}", CurrentUser.DN);
            BasicGlobal.Logger.Log(LogLevel.Error, "Error sending mail to {0}", address.Address);
            BasicGlobal.Logger.Log(LogLevel.Error, "Error sending mail: {0}", exception.Message);
            BasicGlobal.Logger.Log(LogLevel.Error, "Error sending mail: {0}", exception.StackTrace);
            writer.Write("<p>Error sending mail to {0}.</p>", addressText);
            writer.Flush();
          }
        }

        Session.Remove(MailSessionObject);

        BasicGlobal.Logger.Log(LogLevel.Info, "User {0} sent mail.", CurrentUser.DN);

        writer.Write("<p>Mail sent.</p>", CurrentUser.DN);
        writer.Flush();
      }
      catch (Exception exception)
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "User {0} tried to send mail and had error.", exception.ToString());
        writer.Write("<p>Mail send error: {0}</p>", exception.Message);
        writer.Write("<p>Please report this problem with a ticket.");
        writer.Flush();
      }
      finally
      {
        writer.Write("<p><a href=\"./PrepareMail.aspx\">Return</a></p>");
        writer.Flush();
        writer.Write("</body></html>");
        writer.Flush();
      }

      try
      {
        Response.End();
      }
      catch
      { 
      }
    }

    private string Flatten(MailAddressCollection collection)
    {
      List<string> mails = new List<string>();

      foreach (MailAddress mail in collection)
      {
        mails.Add(mail.Address);
      }

      return Multiline(mails);
    }
  }
}