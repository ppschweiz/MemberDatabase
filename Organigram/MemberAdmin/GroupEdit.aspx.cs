using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Novell.Directory.Ldap;
using System.IO;

namespace MemberAdmin
{
  public partial class GroupEdit : CustomPage
  {
    protected override string PageName
    {
      get { return "GroupEdit"; }
    }
    
    private OrderingControl _groupOrderingBox;

    private TextBox _groupNameBox;

    private Group _currentGroup;

    private TextBox _addMemberTextBox;

    private Dictionary<Role, OrderingControl> _roleOrderingBoxes;

    private Dictionary<Role, TextBox> _roleNameBoxes;

    private Dictionary<Person, DropDownList> _roleAssignmentBoxes;

    private Dictionary<object, Person> _removeButtons;

    private Dictionary<string, Role> _roleList;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var table = new Table();
      var groupDn = Request.Params["groupdn"];
      _roleOrderingBoxes = new Dictionary<Role, OrderingControl>();
      _roleNameBoxes = new Dictionary<Role, TextBox>();
      _roleList = new Dictionary<string, Role>();
      _removeButtons = new Dictionary<object, Person>();
      _roleAssignmentBoxes = new Dictionary<Person, DropDownList>();

      if (groupDn != null)
      {
        _currentGroup = Connection.SearchFirst<Group>(groupDn.Replace("-", "="));

        if (_currentGroup != null)
        {
          table.AddRow("Gruppe", "Anzeigename", "Ordering");

          _groupOrderingBox = new OrderingControl();
          _groupOrderingBox.Ordering = _currentGroup.Ordering;

          _groupNameBox = new TextBox();
          _groupNameBox.Text = _currentGroup.DisplayNameGerman;
          table.AddRow(_currentGroup.Name, _groupNameBox, _groupOrderingBox);

          table.AddVerticalSpace(30);
          table.AddRow("Rolle", "Anzeigename", "Ordering");

          foreach (var role in _currentGroup.Roles(Connection))
          {
            var roleOrderingBox = new OrderingControl();
            roleOrderingBox.Ordering = role.Ordering;
            _roleOrderingBoxes.Add(role, roleOrderingBox);

            var roleNameBox = new TextBox();
            roleNameBox.Text = role.DisplayNameGerman;
            _roleNameBoxes.Add(role, roleNameBox);

            table.AddRow(role.Name, roleNameBox, roleOrderingBox);
          }

          foreach (var role in _currentGroup.Roles(Connection))
          {
            _roleList.Add(role.Name, role);
          }

          table.AddVerticalSpace(30);
          table.AddRow("Mitglied", "Rolle", "Aktion");

          foreach (var person in _currentGroup.Members(Connection))
          {
            var assignmentDropDown = new DropDownList();
            assignmentDropDown.Items.Add(new ListItem("Keine", string.Empty));

            foreach (var role in _currentGroup.Roles(Connection))
            {
              assignmentDropDown.Items.Add(new ListItem(role.Name, role.Name));
            }

            var roleOfPerson = _currentGroup.Roles(Connection).Where(r => r.OccupantDns.Contains(person.DN)).FirstOrDefault();
            assignmentDropDown.SelectedValue = roleOfPerson == null ? string.Empty : roleOfPerson.Name;
            _roleAssignmentBoxes.Add(person, assignmentDropDown);

            var removeButton = new Button();
            removeButton.Text = "Entfernen";
            removeButton.Click += new EventHandler(RemoveButton_Click);
            _removeButtons.Add(removeButton, person);

            table.AddRow(person.Name, assignmentDropDown, removeButton);
          }

          table.AddVerticalSpace(30);
          table.AddRow("Neues Mitglied", 2);

          _addMemberTextBox = new TextBox();
          var addMemberButton = new Button();
          addMemberButton.Text = "Hinzufügen";
          addMemberButton.Click += new EventHandler(AddMemberButton_Click);
          table.AddRow("Id oder Name", _addMemberTextBox, addMemberButton);

          var saveButton = new Button();
          saveButton.Text = "Updaten";
          saveButton.Click += new EventHandler(SaveButton_Click);
          table.AddRow(string.Empty, saveButton);
        }
        else
        {
          table.AddRow("No group.");
        }
      }
      else
      {
        table.AddRow("No group.");
      }

      this.panel.Controls.Add(table);
    }

    private void AddMemberButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      ExecuteHandlingError(
        "GroupEdit.AddMember",
        () =>
        {
          var person = Connection.SearchFirst<Person>(Names.Party, LdapScope.Sub, new LdapAttributeFilter(Attributes.Person.UniqueIdentifier, _addMemberTextBox.Text));

          if (person == null)
          {
            person = Connection.SearchFirst<Person>(Names.Party, LdapScope.Sub, new LdapAttributeFilter(Attributes.Person.CommonName, _addMemberTextBox.Text));
          }

          if (person != null)
          {
            _currentGroup.MemberDns.Add(person.DN);
            _currentGroup.Modify(Connection);
            RedirectSelf();
          }
        });
    }

    private void RemoveButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }
      
      ExecuteHandlingError(
        "GroupEdit.Remove",
        () =>
        {
          var person = _removeButtons[sender];
          var roleOfPerson = _currentGroup.Roles(Connection).Where(r => r.OccupantDns.Contains(person.DN)).FirstOrDefault();

          if (roleOfPerson != null)
          {
            roleOfPerson.OccupantDns.Remove(person.DN);
            roleOfPerson.Modify(Connection);
          }

          _currentGroup.MemberDns.Remove(person.DN);
          _currentGroup.Modify(Connection);
          RedirectSelf();
        });
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }
      
      ExecuteHandlingError(
        "GroupEdit.Save",
        () =>
        {

          if (_currentGroup != null)
          {
            _currentGroup.Ordering = _groupOrderingBox.Ordering;
            _currentGroup.DisplayNameGerman = _groupNameBox.Text;
            _currentGroup.Modify(Connection);

            foreach (var role in _roleNameBoxes.Keys)
            {
              role.Ordering = _roleOrderingBoxes[role].Ordering;
              role.DisplayNameGerman = _roleNameBoxes[role].Text;
              role.Modify(Connection);
            }

            foreach (var assignment in _roleAssignmentBoxes)
            {
              var oldRole = _currentGroup.Roles(Connection).Where(r => r.OccupantDns.Contains(assignment.Key.DN)).FirstOrDefault();

              if (oldRole != null)
              {
                oldRole.OccupantDns.Remove(assignment.Key.DN);
                oldRole.Modify(Connection);
              }

              var newRole = _currentGroup.Roles(Connection).Where(r => r.Name == assignment.Value.SelectedValue).FirstOrDefault();

              if (newRole != null)
              {
                newRole.OccupantDns.Add(assignment.Key.DN);
                newRole.Modify(Connection);
              }
            }

            RedirectSelf();
          }
        });
    }
  }
}