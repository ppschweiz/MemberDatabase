using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Textile;
using Pirate.Util.Logging;
using Pirate.Ldap.Web;

namespace MemberAdmin
{
  public partial class PrepareMail : CustomPage
  {
    protected override string PageName
    {
      get { return "PrepareMail"; }
    }
    
    private DropDownList _fromList;

    private ListBox _toSectionsList;

    private ListBox _toStatesList;

    private ListBox _toLanguagesList;

    private ListBox _toNotificationMethodsList;

    private TextBox _subjectTextBox;

    private TextBox _bodyTextBox;

    private List<FileUpload> _attchementUploads;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var table = new Table();

      _fromList = new DropDownList();
      _fromList.CssClass = "ui-widget-content";
      _fromList.Width = new Unit(500, UnitType.Pixel);

      foreach (var groupItem in GetGroupsOfSender(Connection))
      {
        foreach (var mail in groupItem.Key.Emails)
        {
          _fromList.Items.Add(new ListItem(groupItem.Value + ", " + groupItem.Key.DisplayNameGerman, mail));
        }
      }

      table.AddRow(Resources.Sender, _fromList);

      _toSectionsList = new ListBox();
      _toSectionsList.ID = "ml1";
      _toSectionsList.SelectionMode = ListSelectionMode.Multiple;
      _toSectionsList.Items.Add(new ListItem(Resources.AllItems, string.Empty));

      foreach (var section in SectionExtensions.GetValues(Access, false))
      {
        _toSectionsList.Items.Add(new ListItem(section.Display, section.Value.Replace(",", ";")));
      }

      foreach (ListItem item in _toSectionsList.Items)
      {
        item.Selected = true;
      }

      table.AddRow(Resources.ToSections, _toSectionsList);

      _toStatesList = new ListBox();
      _toStatesList.ID = "ml2";
      _toStatesList.SelectionMode = ListSelectionMode.Multiple;
      var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");
      var states = States.GetList(binDir);
      _toStatesList.Items.Add(new ListItem(Resources.AllItems, string.Empty));
      _toStatesList.Items.Add(new ListItem(Resources.StateAbroad, Attributes.Values.Unspecified));

      foreach (var state in states)
      {
        _toStatesList.Items.Add(new ListItem(state.Display, state.Value));
      }

      foreach (ListItem item in _toStatesList.Items)
      {
        item.Selected = true;
      }

      table.AddRow(Resources.ToStates, _toStatesList);

      _toLanguagesList = new ListBox();
      _toLanguagesList.ID = "ml3";
      _toLanguagesList.SelectionMode = ListSelectionMode.Multiple;
      _toLanguagesList.Items.Add(new ListItem(Resources.AllItems, string.Empty));

      foreach (var language in Language.Values)
      {
        _toLanguagesList.Items.Add(new ListItem(language.Display, language.Value, true));
      }

      foreach (ListItem item in _toLanguagesList.Items)
      {
        item.Selected = true;
      } 
      
      table.AddRow(Resources.ToLanguages, _toLanguagesList);

      _toNotificationMethodsList = new ListBox();
      _toNotificationMethodsList.ID = "ml4";
      _toNotificationMethodsList.SelectionMode = ListSelectionMode.Multiple;
      _toNotificationMethodsList.Items.Add(new ListItem(Resources.AllItems, string.Empty));

      foreach (var notificationMethod in NotificationMethod.Values)
      {
        _toNotificationMethodsList.Items.Add(new ListItem(notificationMethod.Display, notificationMethod.Value));
      }

      foreach (ListItem item in _toNotificationMethodsList.Items)
      {
        item.Selected = true;
      } 
      
      table.AddRow(Resources.ToNotificationMethods, _toNotificationMethodsList);

      _subjectTextBox = new TextBox();
      _subjectTextBox.CssClass = "ui-widget-content";
      _subjectTextBox.Width = new Unit(700, UnitType.Pixel);
      table.AddRow(Resources.Subject, _subjectTextBox);

      table.AddRow(string.Empty, string.Format(CultureInfo.InvariantCulture, Resources.TextileSupportInfo, TextileInfo.SupportedMarkup));

      _bodyTextBox = new TextBox();
      _bodyTextBox.TextMode = TextBoxMode.MultiLine;
      _bodyTextBox.CssClass = "ui-widget-content";
      _bodyTextBox.Width = new Unit(700, UnitType.Pixel);
      _bodyTextBox.Height = new Unit(350, UnitType.Pixel);
      table.AddRow(Resources.Body, _bodyTextBox);

      _attchementUploads = new List<FileUpload>();

      ////for (int i = 0; i < 10; i++)
      ////{
      ////  var attachementUpload = new FileUpload();
      ////  table.AddRow(Resources.Attachement, attachementUpload);
      ////  _attchementUploads.Add(attachementUpload);
      ////}

      var testButton = new Button();
      testButton.Text = Resources.Test;
      testButton.Click += new EventHandler(TestButton_Click);
      table.AddRow(string.Empty, testButton);

      var prepareButton = new Button();
      prepareButton.Text = Resources.Prepare;
      prepareButton.Click += new EventHandler(PrepareButton_Click);
      table.AddRow(string.Empty, prepareButton);

      this.panel.Controls.Add(table);
    }

    private void TestButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var message = GatherMail();

      foreach (var address in CurrentUser.Emails)
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
          newMessage.To.Add(Mailer.EncodeAddress(address, CurrentUser.Name));

          var addressText = string.Join("; ", newMessage.To.Select(x => x.Address).ToArray());

          BasicGlobal.Logger.Log(
            LogLevel.Info,
            "User {0} sent test mail from {1} to {2} subject {3}",
            CurrentUser.DN,
            newMessage.From.Address,
            addressText,
            newMessage.Subject);

          BasicGlobal.Mailer.Send(newMessage);
        }
        catch (Exception exception)
        {
          BasicGlobal.Logger.Log(LogLevel.Error, "Error sending test mail by {0}", CurrentUser.DN);
          BasicGlobal.Logger.Log(LogLevel.Error, "Error sending test mail to {0}", address);
          BasicGlobal.Logger.Log(LogLevel.Error, "Error sending test mail: {0}", exception.Message);
          BasicGlobal.Logger.Log(LogLevel.Error, "Error sending test mail: {0}", exception.StackTrace);
        }
      }
    }

    private string GetSectionOfGroup(LdapConnection connection, Pirate.Ldap.Group group)
    {
      if (group.DN.Contains("l="))
      {
        var sectionDn = group.DN.Substring(group.DN.IndexOf("l="));
        var section = connection.SearchFirst<Section>(sectionDn, LdapScope.Base);
        return section.DisplayNameGerman;
      }
      else if (group.DN.Contains("st="))
      {
        var sectionDn = group.DN.Substring(group.DN.IndexOf("st="));
        var section = connection.SearchFirst<Section>(sectionDn, LdapScope.Base);
        return section.DisplayNameGerman;
      }
      else
      {
        return null;
      }
    }

    private string GetPpsName(LdapConnection connection)
    {
      return connection.SearchFirst<Organization>(Names.Party, LdapScope.Base).DisplayNameGerman;
    }

    private IEnumerable<KeyValuePair<Pirate.Ldap.Group, string>> GetGroupsOfSender(LdapConnection connection)
    {
      var userdn = Ldap.GetUserDn(Application, Session, Request);
      var groups = connection.Search<Pirate.Ldap.Group>(Names.Party, LdapScope.Sub);

      foreach (var group in groups)
      {
        if (group.MemberDns.Contains(userdn))
        {
          var parentName = GetSectionOfGroup(connection, group) ?? GetPpsName(connection);

          yield return new KeyValuePair<Pirate.Ldap.Group, string>(group, parentName);
        }
      }
    }

    private IEnumerable<string> GetDns()
    {
      return _toSectionsList
        .GetSelectedValues(Request)
        .Select(dn => dn.Replace(";", ","))
        .ToList();
    }

    private LdapFilter GetLanguageFilter()
    {
      var selectedLanguages = _toLanguagesList
        .GetSelectedValues(Request)
        .ToList();

      if (selectedLanguages.Contains(Attributes.Values.Unspecified))
      {
        selectedLanguages.Remove(Attributes.Values.Unspecified);
        selectedLanguages.Add(string.Empty);
      }

      if (_toLanguagesList.Items.Count == selectedLanguages.Count + 1)
      {
        return null;
      }
      else
      {
        return Attributes.Person.PreferredLanguage.AnyOf(selectedLanguages.ToArray());
      }
    }

    private LdapFilter GetNotificationMethodFilter()
    {
      var selectMethods = _toNotificationMethodsList
        .GetSelectedValues(Request)
        .ToList();

      if (selectMethods.Contains(Attributes.Values.Unspecified))
      {
        selectMethods.Remove(Attributes.Values.Unspecified);
        selectMethods.Add(string.Empty);
      }

      if (_toNotificationMethodsList.Items.Count == selectMethods.Count + 1)
      {
        return null;
      }
      else
      {
        return Attributes.Person.PreferredNotificationMethod.AnyOf(selectMethods.ToArray());
      }
    }

    private LdapFilter GetStateFilter()
    {
      var selectedStates = _toStatesList
        .GetSelectedValues(Request)
        .ToList();

      if (selectedStates.Contains(Attributes.Values.Unspecified))
      {
        selectedStates.Remove(Attributes.Values.Unspecified);
        selectedStates.Add(string.Empty);
      }

      if (_toStatesList.Items.Count == selectedStates.Count + 1)
      {
        return null;
      }
      else
      {
        return Attributes.Person.State.AnyOf(selectedStates.ToArray());
      }
    }

    private LdapFilter GetFilter()
    {
      return GetStateFilter() & GetLanguageFilter() & GetNotificationMethodFilter();
    }

    private void PrepareButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var message = GatherMail();

      Session.Add(SendMail.MailSessionObject, message);
      Redirect("SendMail.aspx");
    }

    private MailMessage GatherMail()
    {
      var dns = GetDns();
      var persons = Connection.SearchUniquePersons(dns, LdapScope.Sub, GetFilter());

      var message = new MailMessage();

      foreach (var attachementUpload in _attchementUploads)
      {
        if (attachementUpload.HasFile)
        {
          var reader = new BinaryReader(attachementUpload.PostedFile.InputStream);
          var bytes = reader.ReadBytes((int)attachementUpload.PostedFile.InputStream.Length);
          var stream = new MemoryStream(bytes);

          var attachement = new Attachment(stream, attachementUpload.FileName);
          message.Attachments.Add(attachement);
        }
      }

      message.BodyEncoding = Encoding.UTF8;
      message.SubjectEncoding = Encoding.UTF8;
      message.Subject = _subjectTextBox.Text;
      message.Body = _bodyTextBox.Text;
      message.From = Mailer.EncodeAddress(_fromList.SelectedItem.Value, _fromList.SelectedItem.Text);
      message.To.Add(Mailer.EncodeAddress(_fromList.SelectedItem.Value, _fromList.SelectedItem.Text));

      foreach (var person in persons)
      {
        foreach (var mail in person.Emails)
        {
          try
          {
            message.Bcc.Add(Mailer.EncodeAddress(mail, person.Name));
          }
          catch (Exception exception)
          {
            BasicGlobal.Logger.Log(LogLevel.Error, "Meil address error {0}", exception.Message);
            BasicGlobal.Logger.Log(LogLevel.Error, "Meil address error {0}", exception.StackTrace);
            BasicGlobal.Logger.Log(LogLevel.Error, "Invalid mail address {0}", mail);
          }
        }
      }
      return message;
    }
  }
}