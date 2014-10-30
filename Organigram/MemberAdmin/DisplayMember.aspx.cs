using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Novell.Directory.Ldap;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace MemberAdmin
{
  public partial class DisplayMember : CustomPage
  {
    protected override string PageName
    {
      get { return "DisplayMember"; }
    }

    private Person _currentPerson;

    private DropDownList _currentSection;

    private Dictionary<TextBox, Action<Person, string>> _textBoxes;

    private Dictionary<DropDownList, Action<Person, string>> _dropDowns;

    private Dictionary<MultiTextBox, Action<Person, List<string>>> _multiTextBoxes;

    private Dictionary<TextBox, Action<Person, DateTime?>> _dateTimeBoxes;

    private Dictionary<TextBox, Action<Person, IEnumerable<DateTime>>> _multiDateTimeBoxes;

    private FileUpload _photoUpload;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var table = new Table();

      var uidParam = this.Request.Params["member"] ?? "246";
      var uid = Convert.ToInt32(uidParam);

      if (UpdateState == null ||
          UpdateState.Current == null ||
          (!(UpdateState.Current is Person)) ||
          ((Person)UpdateState.Current).Id != uid)
      {
        UpdateState = new UpdateState(
          UpdateOperation.Update, 
          Connection.SearchFirst<Person>(Names.Party, LdapScope.Sub, Attributes.Person.UniqueIdentifier == uid));
      }
      else if (UpdateState.Operation == UpdateOperation.None)
      {
        UpdateState = new UpdateState(
          UpdateOperation.Update,
          UpdateState.Current);
      }

      _currentPerson = UpdateState.Current as Person;

      if (_currentPerson != null)
      {
        _textBoxes = new Dictionary<TextBox, Action<Person, string>>();
        _dropDowns = new Dictionary<DropDownList, Action<Person, string>>();
        _multiTextBoxes = new Dictionary<MultiTextBox, Action<Person, List<string>>>();
        _dateTimeBoxes = new Dictionary<TextBox, Action<Person, DateTime?>>();
        _multiDateTimeBoxes = new Dictionary<TextBox,Action<Person,IEnumerable<DateTime>>>();

        table.AddRow(LdapResources.DN, _currentPerson.DN);
        table.AddRow(LdapResources.UniqueIdentifier, _currentPerson.Id.ToString());
        table.AddRow(LdapResources.EmployeeNumber, _currentPerson.EmployeeNumber.ToString());

        _currentSection = new DropDownList();

        if (_currentPerson.EmployeeType == EmployeeType.Sympathizer ||
            _currentPerson.EmployeeType == EmployeeType.Pirate)
        {
          _currentSection.Items.Add(new ListItem(Resources.SectionMembers, Names.Members));
          var sections = Connection.Search<Section>(Names.Party, LdapScope.Sub);

          foreach (var section in sections)
          {
            _currentSection.Items.Add(new ListItem(section.DisplayNameGerman, "dc=members," + section.DN));
          }
        }
        else
        {
          _currentSection.Items.Add(new ListItem(Resources.SectionPeople, Names.People));
        }

        _currentSection.SelectedValue = _currentPerson.DN.Substring(_currentPerson.DN.IndexOf(",") + 1);
        table.AddRow(LdapResources.Section, _currentSection);

        table.AddRow(LdapResources.EmployeeType, _currentPerson.EmployeeType.ToString());

        if (_currentPerson.IsMemberType == _currentPerson.IsMemberDn &&
           ((_currentPerson.IsMemberType && _currentPerson.Joining.Count != _currentPerson.Leaving.Count + 1) ||
            (_currentPerson.IsNonMemberType && _currentPerson.Leaving.Count != _currentPerson.Joining.Count) ||
            (_currentPerson.IsFormerMemberType && _currentPerson.Leaving.Count < 1)))
        {
          Add(table, LdapResources.Joining, (p, t) => p.Joining = new List<DateTime>(t), _currentPerson.Joining, true);
          Add(table, LdapResources.Leaving, (p, t) => p.Leaving = new List<DateTime>(t), _currentPerson.Leaving, true);
        }
        else
        {
          table.AddRow(LdapResources.Joining, Multiline(_currentPerson.Joining.Select(x => x.ToShortDateString())));
          table.AddRow(LdapResources.Leaving, Multiline(_currentPerson.Leaving.Select(x => x.ToShortDateString())));
        }

        var buttonPanel = new Panel();

        if (_currentPerson.IsMemberType != _currentPerson.IsMemberDn)
        {
          var fixToMemberButton = new Button();
          fixToMemberButton.Width = new Unit(120d, UnitType.Point);
          fixToMemberButton.Text = Resources.FixToMember;
          fixToMemberButton.Click += new EventHandler(FixToMemberButton_Click);
          buttonPanel.Controls.Add(fixToMemberButton);

          var fixToNonMemberButton = new Button();
          fixToNonMemberButton.Width = new Unit(120d, UnitType.Point);
          fixToNonMemberButton.Text = Resources.FixToNonMember;
          fixToNonMemberButton.Click += new EventHandler(FixToNonMemberButton_Click);
          buttonPanel.Controls.Add(fixToNonMemberButton);
        }
        else
        {
          if (_currentPerson.EmployeeType == EmployeeType.LandLubber)
          {
            var joinButton = new Button();
            joinButton.Width = new Unit(80d, UnitType.Point);
            joinButton.Text = Resources.Join;
            joinButton.Click += new EventHandler(JoinButton_Click);
            buttonPanel.Controls.Add(joinButton);
          }

          if ((_currentPerson.EmployeeType == EmployeeType.Sympathizer ||
              _currentPerson.EmployeeType == EmployeeType.Pirate) &&
              !_currentPerson.EmployeeNumber.HasValue)
          {
            var acceptButton = new Button();
            acceptButton.Text = Resources.Accept;
            acceptButton.Click += new EventHandler(AcceptButton_Click);
            buttonPanel.Controls.Add(acceptButton);
          }

          if (_currentPerson.EmployeeType == EmployeeType.Pirate ||
              _currentPerson.EmployeeType == EmployeeType.Sympathizer)
          {
            var leaveButton = new Button();
            leaveButton.Width = new Unit(80d, UnitType.Point);
            leaveButton.Text = Resources.Leave;
            leaveButton.Click += new EventHandler(LeaveButton_Click);
            buttonPanel.Controls.Add(leaveButton);

            var kickButton = new Button();
            kickButton.Width = new Unit(80d, UnitType.Point);
            kickButton.Text = Resources.Kick;
            kickButton.Click += new EventHandler(KickButton_Click);
            buttonPanel.Controls.Add(kickButton);
          }
          else if (_currentPerson.EmployeeType == EmployeeType.Veteran ||
                   _currentPerson.EmployeeType == EmployeeType.WalkedThePlank)
          {
            var rejoinButton = new Button();
            rejoinButton.Width = new Unit(80d, UnitType.Point);
            rejoinButton.Text = Resources.Reaccept;
            rejoinButton.Click += new EventHandler(RejoinButton_Click);
            buttonPanel.Controls.Add(rejoinButton);
          }
        }

        table.AddRow(string.Empty, buttonPanel);

        if (_currentPerson.EmployeeType == EmployeeType.Sympathizer ||
            _currentPerson.EmployeeType == EmployeeType.Pirate)
        {
          table.AddRow(LdapResources.VotingRightUntil, _currentPerson.VotingRightUntil.ToShortDateString());
        }
        
        Add(table, LdapResources.CommonName, (p, t) => p.Name = t, _currentPerson.Name, string.IsNullOrEmpty(_currentPerson.Name) != string.IsNullOrEmpty(_currentPerson.Surname));
        Add(table, LdapResources.Surname, (p, t) => p.Surname = t, _currentPerson.Surname);
        Add(table, LdapResources.Givenname, (p, t) => p.Givenname = t, _currentPerson.Givenname);
        Add(table, LdapResources.UserId, (p, t) => p.Nickname = t, _currentPerson.Nickname);

        if (_currentPerson.Photo != null)
        {
          var image = new System.Web.UI.WebControls.Image();
          image.ImageUrl = "~/OutputMemberPhoto.aspx?member=" + uid.ToString();
          image.Height = new Unit(100, UnitType.Pixel);
          table.AddRow(LdapResources.Photo, image);
        }

        _photoUpload = new FileUpload();
        table.AddRow(string.Empty, _photoUpload);

        Add(table, LdapResources.Email, (p, m) => p.Emails = m, _currentPerson.Emails, (_currentPerson.Emails.Count < 1) && (_currentPerson.PreferredNotificationMethod == PreferredNotificationMethod.Email));
        Add(table, LdapResources.AlternateMail, (p, m) => p.AlternateEmails = m, _currentPerson.AlternateEmails);
        Add(table, LdapResources.Phone, (p, t) => p.Phone = t, _currentPerson.Phone);
        Add(table, LdapResources.Mobile, (p, t) => p.Mobile = t, _currentPerson.Mobile);
        Add(table, LdapResources.Gender, GenderExtensions.Values, (p, v) => p.Gender = (Gender?)ToIntOrNull(v), ToStringOrEmpty((int?)_currentPerson.Gender));
        Add(table, LdapResources.PreferredNotificationMethod, PreferredNotificationMethodExtensions.Values, (p, v) => p.PreferredNotificationMethod = (PreferredNotificationMethod?)ToIntOrNull(v), ToStringOrEmpty((int?)_currentPerson.PreferredNotificationMethod));
        Add(table, LdapResources.PreferredLanguage, Language.Values, (p, v) => p.PreferredLanguage = v, _currentPerson.PreferredLanguage);

        var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");
        var countries = Countries.GetList(binDir);
        Add(table, LdapResources.Country, countries, (p, v) => p.Country = v, _currentPerson.Country);

        var states = States.GetList(binDir);
        Add(table, LdapResources.State, states, (p, v) => p.State = v, _currentPerson.State);

        Add(table, LdapResources.Street, (p, t) => p.Street = t, _currentPerson.Street);
        Add(table, LdapResources.PostalCode, (p, t) => p.PostalCode = t, _currentPerson.PostalCode, string.IsNullOrEmpty(_currentPerson.PostalCode) && (_currentPerson.PreferredNotificationMethod == PreferredNotificationMethod.Letter));
        Add(table, LdapResources.Location, (p, t) => p.Location = t, _currentPerson.Location, string.IsNullOrEmpty(_currentPerson.Location) && (_currentPerson.PreferredNotificationMethod == PreferredNotificationMethod.Letter));
        Add(table, LdapResources.Info, (p, t) => p.Info = t, _currentPerson.Info);
        Add(table, LdapResources.Description, (p, t) => p.Description = t, _currentPerson.Description);

        Add(table, LdapResources.Birthdate, (p, d) => p.BirthDate = d, _currentPerson.BirthDate);

        /*
        _newsletterSelectBox = new CheckBoxList();

        foreach (NewsletterSelect item in Enum.GetValues(typeof(NewsletterSelect)))
        {
          if ((int)item > 0)
          {
            var listItem = new ListItem(item.ToString(), ((int)item).ToString());
            listItem.Selected = _currentPerson.NewsLetterSelect.Contains(item);
            _newsletterSelectBox.Items.Add(listItem);
          }
        }

        table.AddRow(LdapResources.NewsletterSelect, _newsletterSelectBox);
        */

        Panel editButtonPanel = new Panel();

        var modifyButton = new Button();
        modifyButton.Width = new Unit(80d, UnitType.Point);
        modifyButton.Text = UpdateState.Operation == UpdateOperation.Create ? Resources.Create : Resources.Modify;
        modifyButton.Click += new EventHandler(ModifyButton_Click);
        editButtonPanel.Controls.Add(modifyButton);

        var revertButton = new Button();
        revertButton.Width = new Unit(80d, UnitType.Point);
        revertButton.Text = Resources.Rollback;
        revertButton.Click += new EventHandler(RevertButton_Click);
        editButtonPanel.Controls.Add(revertButton);

        if (UpdateState.Operation == UpdateOperation.Update)
        {
          var deleteButton = new Button();
          deleteButton.Width = new Unit(80d, UnitType.Point);
          deleteButton.Text = Resources.Delete;
          deleteButton.Click += new EventHandler(DeleteButton_Click);
          editButtonPanel.Controls.Add(deleteButton);
        }

        table.AddRow(string.Empty, editButtonPanel);
      }
      else
      {
        table.AddRow(Resources.UnknownPerson);
      }

      this.panel.Controls.Add(table);
    }

    private void FixToNonMemberButton_Click(object sender, EventArgs e)
    {
      if (_currentPerson.IsMemberDn)
      {
        _currentPerson.MoveToPeople();
      }

      if (_currentPerson.IsMemberType)
      {
        if (_currentPerson.Joining.Count + _currentPerson.Leaving.Count > 0)
        {
          _currentPerson.EmployeeType = EmployeeType.Veteran;
        }
        else
        {
          _currentPerson.EmployeeType = EmployeeType.LandLubber;
        }
      }

      RedirectSelf();
    }

    private void FixToMemberButton_Click(object sender, EventArgs e)
    {
      if (!_currentPerson.IsMemberDn)
      {
        _currentPerson.MoveToMembers();
      }

      if (_currentPerson.IsNonMemberType)
      {
        _currentPerson.EmployeeType = EmployeeType.Sympathizer;
      }

      RedirectSelf();
    }

    private void RevertButton_Click(object sender, EventArgs e)
    {
      UpdateState = null;
      RedirectSelf();
    }

    private void DeleteButton_Click(object sender, EventArgs e)
    {
      UpdateState = new UpdateState(
        UpdateOperation.Delete,
        UpdateState.Current);
      RedirectCommit();
    }

    private void JoinButton_Click(object sender, EventArgs e)
    {
      _currentPerson.EmployeeNumber = _currentPerson.Id;
      _currentPerson.EmployeeType = EmployeeType.Sympathizer;
      _currentPerson.Joining.Add(DateTime.UtcNow);
      _currentPerson.MoveToMembers();
      RedirectSelf();
    }

    private void RejoinButton_Click(object sender, EventArgs e)
    {
      _currentPerson.EmployeeType = EmployeeType.Sympathizer;
      _currentPerson.Joining.Add(DateTime.UtcNow);
      _currentPerson.MoveToMembers();
      RedirectSelf();
    }

    private void KickButton_Click(object sender, EventArgs e)
    {
      _currentPerson.EmployeeType = EmployeeType.WalkedThePlank;
      _currentPerson.Leaving.Add(DateTime.UtcNow);
      _currentPerson.MoveToPeople();
      RedirectSelf();
    }

    /*
    private IEnumerable<NewsletterSelect> GetNewsletterSelect()
    {
      foreach (ListItem item in _newsletterSelectBox.Items)
      {
        if (item.Selected)
        {
          yield return (NewsletterSelect)Convert.ToInt32(item.Value);
        }
      }
    }
    */

    private void LeaveButton_Click(object sender, EventArgs e)
    {
      _currentPerson.EmployeeType = EmployeeType.Veteran;
      _currentPerson.Leaving.Add(DateTime.UtcNow);
      _currentPerson.MoveToPeople();
      RedirectSelf();
    }

    private void AcceptButton_Click(object sender, EventArgs e)
    {
      _currentPerson.EmployeeNumber = _currentPerson.Id;
      RedirectSelf();
    }

    private void ModifyButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      ExecuteHandlingError(
        "DisplayMember.Modify",
        () =>
        {
          _currentPerson.Move(_currentSection.SelectedValue);

          foreach (var textBoxItem in _textBoxes)
          {
            textBoxItem.Value(_currentPerson, textBoxItem.Key.Text);
          }

          foreach (var dropDownItem in _dropDowns)
          {
            dropDownItem.Value(_currentPerson, dropDownItem.Key.Text);
          }

          foreach (var multiTextBoxItem in _multiTextBoxes)
          {
            multiTextBoxItem.Value(_currentPerson, new List<string>(multiTextBoxItem.Key.Values));
          }

          foreach (var calendarItem in _dateTimeBoxes)
          {
            DateTime result = DateTime.MinValue;

            if (calendarItem.Key.Text.IsNullOrEmpty())
            {
              calendarItem.Value(_currentPerson, null);
            }
            else if (DateTime.TryParse(calendarItem.Key.Text, out result))
            {
              calendarItem.Value(_currentPerson, result);
            }
          }

          foreach (var calendarItem in _multiDateTimeBoxes)
          {
            var lines = calendarItem.Key.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<DateTime>();

            foreach (var line in lines)
            {
              DateTime result = DateTime.MinValue;

              if (DateTime.TryParse(line, out result))
              {
                list.Add(result);
              }
            }

            calendarItem.Value(_currentPerson, list);
          }

          if (_photoUpload.HasFile)
          {
            try
            {
              _currentPerson.Photo = Bitmap.FromStream(_photoUpload.PostedFile.InputStream);
            }
            catch
            {
              _currentPerson.Photo = null;
            }
          }

          RedirectCommit();
        });
    }

    private void Add(Table table, string title, Action<Person, int?> updateAction, int? value, bool warning = false)
    {
      var textBox = new TextBox();
      textBox.Text = value.ToString();
      textBox.Width = new Unit(300, UnitType.Pixel);
      textBox.BackColor = warning ? Color.Yellow : Color.White;
      table.AddRow(title, textBox);
      _textBoxes.Add(textBox, (p, t) => TryAssignInt(p, t, updateAction));
    }

    private void Add(Table table, string title, Action<Person, string> updateAction, string value, bool warning = false)
    {
      var textBox = new TextBox();
      textBox.Text = value;
      textBox.Width = new Unit(300, UnitType.Pixel);
      textBox.BackColor = warning ? Color.Yellow : Color.White;
      table.AddRow(title, textBox);
      _textBoxes.Add(textBox, updateAction);
    }

    private void Add(Table table, string title, Action<Person, IEnumerable<DateTime>> updateAction, IEnumerable<DateTime> value, bool warning = false)
    {
      var textBox = new TextBox();
      textBox.TextMode = TextBoxMode.MultiLine;

      textBox.Text = string.Join(Environment.NewLine, value.Select(x => x.ToShortDateString()).ToArray());

      var panel = new Panel();

      textBox.Width = new Unit(300, UnitType.Pixel);
      textBox.BackColor = warning ? Color.Yellow : Color.White;
      panel.Controls.Add(textBox);

      var info = new Label();
      info.Text = string.Format(Resources.MultiDateInfo, DateTime.Now.ToShortDateString());
      panel.Controls.Add(info);

      table.AddRow(title, panel);

      _multiDateTimeBoxes.Add(textBox, updateAction);
    }

    private void Add(Table table, string title, Action<Person, DateTime?> updateAction, DateTime? value, bool warning = false)
    {
      var textBox = new TextBox();

      if (value.HasValue)
      {
        textBox.Text = value.Value.ToShortDateString();
      }
      else
      {
        textBox.Text = string.Empty;
      }

      var panel = new Panel();

      textBox.Width = new Unit(300, UnitType.Pixel);
      textBox.BackColor = warning ? Color.Yellow : Color.White;
      panel.Controls.Add(textBox);

      var info = new Label();
      info.Text = string.Format(Resources.DateInfo, DateTime.Now.ToShortDateString());
      panel.Controls.Add(info);

      table.AddRow(title, panel);
      _dateTimeBoxes.Add(textBox, updateAction);
    }

    private void Add(Table table, string title, Action<Person, List<string>> updateAction, List<string> values, bool warning = false)
    {
      var multiTextBox = new MultiTextBox();
      multiTextBox.Width = new Unit(300, UnitType.Pixel);
      multiTextBox.Values = values;
      table.AddRow(title, multiTextBox);
      _multiTextBoxes.Add(multiTextBox, updateAction);
    }

    private void Add(Table table, string title, IEnumerable<DisplayValue> items, Action<Person, string> updateAction, string value, bool addNotDefined = true, bool warning = false)
    {
      var dropDown = new DropDownList();

      if (addNotDefined)
      {
        dropDown.Items.Add(new ListItem(Resources.NotDefined, string.Empty));
      }

      foreach (var item in items)
      {
        dropDown.Items.Add(new ListItem(item.Display, item.Value));
      }

      dropDown.SelectedValue = value;
      dropDown.Width = new Unit(300, UnitType.Pixel);
      dropDown.BackColor = warning ? Color.Yellow : Color.White;
      table.AddRow(title, dropDown);
      _dropDowns.Add(dropDown, updateAction);
    }

    private int? ToIntOrNull(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return null;
      }
      else
      {
        return Convert.ToInt32(value);
      }
    }

    private string ToStringOrEmpty(int? value)
    {
      if (value.HasValue)
      {
        return value.Value.ToString();
      }
      else
      {
        return string.Empty;
      }
    }

    private void TryAssignInt(Person person, string value, Action<Person, int?> updateAction)
    {
      if (string.IsNullOrEmpty(value))
      {
        updateAction(person, null);
      }
      else
      {
        int number = 0;

        if (int.TryParse(value, out number))
        {
          updateAction(person, number);
        }
      }
    }
  }
}