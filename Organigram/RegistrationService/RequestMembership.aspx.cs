using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util;
using Pirate.Util.Logging;

namespace RegistrationService
{
  public partial class RequestMembership : CustomPage
  {
    private abstract class DataField
    {
      public abstract void Set(Person person);

      public abstract bool Validate();

      public abstract void AddTo(Table table);
    }

    private class TextField : DataField
    {
      private string _text;
      private TextBox _box;
      private Label _info;
      private Action<string, Person> _set;
      private Func<string, string> _validate;

      public TextField(string text, Action<string, Person> set, Func<string, string> validate)
      {
        _text = text;
        _set = set;
        _validate = validate;
        _box = new TextBox();
        _box.Width = new Unit(10, UnitType.Em);
        _info = new Label();
      }

      public override void Set(Person person)
      {
        _set(_box.Text, person);
      }

      public override bool Validate()
      {
        if (_validate == null)
        {
          _info.Text = string.Empty;
          return true;
        }
        else
        {
          var result = _validate(_box.Text);

          if (result == null)
          {
            _info.Text = string.Empty;
            return true;
          }
          else
          {
            _info.Text = result;
            return false;
          }
        }
      }

      public override void AddTo(Table table)
      {
        table.AddRow(_text, _box, _info);
      }
    }

    private class DropDownField : DataField
    {
      private string _text;
      private DropDownList _list;
      private Label _info;
      private Action<string, Person> _set;
      private Func<string, string> _validate;

      public DropDownField(string text, Action<string, Person> set, Func<string, string> validate, IEnumerable<DisplayValue> entries, params DisplayValue[] moreEntries)
      {
        _text = text;
        _set = set;
        _validate = validate;
        _list = new DropDownList();
        _list.Width = new Unit(10, UnitType.Em);
        
        foreach (var entry in entries)
        {
          _list.Items.Add(new ListItem(entry.Display, entry.Value));
        }

        foreach (var entry in moreEntries)
        {
          _list.Items.Add(new ListItem(entry.Display, entry.Value));
        }

        _info = new Label();
      }

      public override void Set(Person person)
      {
        _set(_list.SelectedValue, person);
      }

      public override bool Validate()
      {
        if (_validate == null)
        {
          _info.Text = string.Empty;
          return true;
        }
        else
        {
          var result = _validate(_list.SelectedValue);

          if (result == null)
          {
            _info.Text = string.Empty;
            return true;
          }
          else
          {
            _info.Text = result;
            return false;
          }
        }
      }

      public override void AddTo(Table table)
      {
        table.AddRow(_text, _list, _info);
      }
    }

    private class SpaceField : DataField
    {
      public override void Set(Person person)
      {
      }

      public override bool Validate()
      {
        return true;
      }

      public override void AddTo(Table table)
      {
        table.AddVerticalSpace(10);
      }
    }

    private class HeaderField : DataField
    {
      private string _text;

      public override void Set(Person person)
      {
      }

      public override bool Validate()
      {
        return true;
      }

      public override void AddTo(Table table)
      {
        table.AddRow(string.Empty);
        table.AddCell(_text, 2);
      }

      public HeaderField(string text)
      {
        _text = text;
      }
    }

    private List<DataField> _fields;
    private Button _nextButton;
    private Button _cancelButton;
    private string _targetSectionDn;

    protected override string PageName
    {
      get { return "RequestMembership"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      var table = new Table();
      table.AddHeaderRow(Resources.CreateAccountMember, 2);
      table.AddRow(string.Format(Resources.CreateAccountStep, 3, 3), 2);
      table.AddVerticalSpace(10);

      var ciphertext = (Request.Params["data"] ?? string.Empty).TryParseHexBytes();
      var plaintext = ciphertext == null ? null : BasicGlobal.Config.DeSecure(ciphertext);
      var request = plaintext == null ? null : MembershipRequest.FromBinary(plaintext);

      if (request != null &&
          DateTime.Now <= request.Created.AddHours(4))
      {
        var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");
        _fields = new List<DataField>();

        _fields.Add(new HeaderField(Resources.RequestMembershipPersonal));

        _fields.Add(
          new TextField(
            LdapResources.Givenname,
            (v, p) => p.Givenname = v,
            v => v.IsNullOrEmpty() ? Resources.ValidateNotEmpty : null));

        _fields.Add(
          new TextField(
            LdapResources.Surname,
            (v, p) => p.Surname = v,
            v => v.IsNullOrEmpty() ? Resources.ValidateNotEmpty : null));

        _fields.Add(
          new DropDownField(
            LdapResources.Gender,
            (v, p) => p.Gender = (Gender)Convert.ToInt32(v),
            v => v == null ? Resources.ValidateNotEmpty : null,
            GenderExtensions.Values));

        _fields.Add(
          new TextField(
            LdapResources.Birthdate,
            (v, p) => p.BirthDate = Parsing.TryParseDateTime(v),
            v => 
              {
                if (v.IsNullOrEmpty())
                {
                  return Resources.ValidateNotEmpty;
                }
                else if (!Parsing.TryParseDateTime(v).HasValue)
                {
                  return string.Format(Resources.ValidateInvalidDate, System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern);
                }
                else
                {
                  return null;
                }
              }
              ));

        _fields.Add(
          new TextField(
            LdapResources.Street,
            (v, p) => p.Street = v,
            null));

        _fields.Add(
          new TextField(
            LdapResources.Location,
            (v, p) => p.Location = v,
            v => v.IsNullOrEmpty() ? Resources.ValidateNotEmpty : null));

        _fields.Add(
          new TextField(
            LdapResources.PostalCode,
            (v, p) => p.PostalCode = v,
            null));

        _fields.Add(
          new DropDownField(
            LdapResources.State,
            (v, p) => p.State = v,
            v => v == null ? Resources.ValidateNotEmpty : null,
            States.GetList(binDir),
            new DisplayValue(Resources.StateAbroad, string.Empty)));

        _fields.Add(
          new DropDownField(
            LdapResources.Country,
            (v, p) => p.Country = v,
            v => v.IsNullOrEmpty() ? Resources.ValidateNotEmpty : null,
            Countries.GetList(binDir)));

        _fields.Add(new SpaceField());

        _fields.Add(new HeaderField(Resources.RequestMembershipComms));
        
        _fields.Add(
          new TextField(
            LdapResources.Phone,
            (v, p) => p.Phone = v,
            null));

        _fields.Add(
          new TextField(
            LdapResources.Mobile,
            (v, p) => p.Mobile = v,
            null)); 
        
        _fields.Add(new SpaceField());

        _fields.Add(new HeaderField(Resources.RequestMembershipOptions));

        var sectionList = Global.LdapAccess.Connection
          .Search<Section>(Names.Party, LdapScope.Sub)
          .Where(s => s.Ordering > 0)
          .OrderBy(s => s.Ordering)
          .Select(s => new DisplayValue(s.DisplayNameGerman, Names.MembersContainerPrefix + s.DN));

        _fields.Add(
          new DropDownField(
            LdapResources.Section,
            (v, p) => _targetSectionDn = v,
            v => v.IsNullOrEmpty() ? Resources.ValidateNotEmpty : null,
            sectionList,
            new DisplayValue(LdapResources.SectionMembers, Names.Members)));

        _fields.Add(
          new DropDownField(
            LdapResources.PreferredNotificationMethod,
            (v, p) => p.PreferredNotificationMethod = (PreferredNotificationMethod)Convert.ToInt32(v),
            v => v.IsNullOrEmpty() ? Resources.ValidateNotEmpty : null,
            NotificationMethod.Values));

        _fields.Add(new SpaceField());

        _fields.Add(new HeaderField(Resources.RequestMembershipSettings));

        _fields.Add(
          new DropDownField(
            LdapResources.PreferredLanguage,
            (v, p) => p.PreferredLanguage = v,
            v => v.IsNullOrEmpty() ? Resources.ValidateNotEmpty : null,
            Pirate.Ldap.Language.Values));

        _fields.Add(new SpaceField());

        foreach (var field in _fields)
        {
          field.AddTo(table);
        }
        
        var buttonPanel = new Panel();

        _nextButton = new Button();
        _nextButton.Text = Resources.Next;
        _nextButton.Click += new EventHandler(NextButton_Click);
        _nextButton.Width = new Unit(60d, UnitType.Point);
        buttonPanel.Controls.Add(_nextButton);

        _cancelButton = new Button();
        _cancelButton.Text = Resources.Cancel;
        _cancelButton.Click += new EventHandler(CancelButton_Click);
        _cancelButton.Width = new Unit(60d, UnitType.Point);
        buttonPanel.Controls.Add(_cancelButton);

        table.AddRow(string.Empty, buttonPanel);

        table.AddVerticalSpace(10);
      }
      else
      {
        table.AddRow(string.Empty, Resources.PasswordResetNoReset);
      }

      this.panel.Controls.Add(table);
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      var ciphertext = (Request.Params["data"] ?? string.Empty).TryParseHexBytes();
      var plaintext = ciphertext == null ? null : BasicGlobal.Config.DeSecure(ciphertext);
      var request = plaintext == null ? null : MembershipRequest.FromBinary(plaintext); 
      
      if (request != null &&
          DateTime.Now <= request.Created.AddHours(4))
      {
        Redirect("DisplayMessage.aspx?data=" + BasicGlobal.Config.Secure((new Message(new Texts.MembershipRequestAbortMessage().Id, request.Dn, "Default.aspx")).ToBinary()).ToHexString());
      }
    }

    private void NextButton_Click(object sender, EventArgs e)
    {
      var ciphertext = (Request.Params["data"] ?? string.Empty).TryParseHexBytes();
      var plaintext = ciphertext == null ? null : BasicGlobal.Config.DeSecure(ciphertext);
      var request = plaintext == null ? null : MembershipRequest.FromBinary(plaintext);

      if (request != null &&
          DateTime.Now <= request.Created.AddHours(4) &&
          _fields != null)
      {
        var valid = true;

        foreach (var field in _fields)
        {
          valid &= field.Validate();
        }

        if (valid)
        {
          var person = Global.LdapAccess.Connection.SearchFirst<Person>(request.Dn);

          foreach (var field in _fields)
          {
            field.Set(person);
          }

          person.Name = person.Givenname + " " + person.Surname;
          person.Modify(Global.LdapAccess.Connection);

          var requestId = DataGlobal.DataAccess.CreateRequest(new Request(0, RequestAction.Join, person.DN, _targetSectionDn, string.Empty, DateTime.Now));

          var text = Texts.GetText(
            DataGlobal.DataAccess,
            new Texts.MembershipRequestPendingEmail(),
            person);
          var displayName = person.Name.IsNullOrEmpty() ? person.Nickname : person.Name;

          foreach (var address in person.Emails)
          {
            BasicGlobal.Mailer.SendTextile(text.Item1, text.Item2, Mailer.EncodeAddress(address, displayName));
          }

          string viewUrl = BasicGlobal.Config.MemberAdminAddress + "ViewRequest.aspx?id=" + requestId.ToString();

          var builder = new StringBuilder();
          builder.AppendLine("Id {0}", person.Id);
          builder.AppendLine("Name {0}", person.Name);
          builder.AppendLine("Link {0}", viewUrl);

          BasicGlobal.Logger.Log(LogLevel.Info, "New membership request from {0}, {1}, {2}.", person.Id, person.Name, string.Join(", ", person.Emails.ToArray())); 
          BasicGlobal.Mailer.SendToMembersQueue("New membership request", builder.ToString());

          Redirect("DisplayMessage.aspx?data=" + BasicGlobal.Config.Secure((new Message(new Texts.MembershipRequestPendingMessage().Id, person.DN, "Default.aspx")).ToBinary()).ToHexString());
        }
      }
    }
  }
}