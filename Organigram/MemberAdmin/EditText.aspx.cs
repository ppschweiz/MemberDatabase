using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public partial class EditText : CustomPage
  {
    protected override string PageName
    {
      get { return "EditText"; }
    }

    private Dictionary<LinkButton, string> _buttonToId;
    private DropDownList _newIdSectionDropdown;
    private DropDownList _newIdLanguageDropdown;
    private TextBox _subjectTextBox;
    private TextBox _bodyTextBox;
    
    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      this.panel.Width = new Unit(100, UnitType.Percentage);

      if (CurrentUserAuthorizationLevel < 2)
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "ListText.Load user id {0} name {1} has authorization level {2} meaning insufficient access.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel);
        RedirectAccessDenied();
        return;
      }

      var navigationTable = new Table();
      var id = Request.Params["id"];
      var prefix = id.Split(new[] { "." }, StringSplitOptions.None)[0];

      var texts = DataGlobal.DataAccess.GetAllTexts().Where(t => t.Id.StartsWith(prefix));
      _buttonToId = new Dictionary<LinkButton, string>();

      foreach (var text in texts.OrderBy(t => t.Id))
      {
        var editLink = new HyperLink();
        editLink.Text = text.Id;
        editLink.NavigateUrl = "EditText.aspx?id=" + text.Id;
        navigationTable.AddRow(editLink);

        var deleteButton = new LinkButton();
        deleteButton.Text = Resources.Delete;
        deleteButton.Click += new EventHandler(DeleteButton_Click);
        _buttonToId.Add(deleteButton, text.Id);

        navigationTable.AddRow(editLink, deleteButton);
      }

      var newIdPanel = new Panel();

      _newIdSectionDropdown = new DropDownList();
      _newIdSectionDropdown.Items.Add(new ListItem(Resources.SectionMembers, "none.none"));
      var sections = Connection.Search<Section>(Names.Party, LdapScope.Sub);
      foreach (var section in sections)
      {
        _newIdSectionDropdown.Items.Add(new ListItem(section.DisplayNameGerman, section.SectionDnState + "." + section.SectionDnLocation));
      }
      newIdPanel.Controls.Add(_newIdSectionDropdown);

      _newIdLanguageDropdown = new DropDownList();
      foreach (var language in Language.Values)
      {
        _newIdLanguageDropdown.Items.Add(new ListItem(language.Display, language.Value));
      }
      newIdPanel.Controls.Add(_newIdLanguageDropdown);

      var newIdButton = new Button();
      newIdButton.Text = Resources.Create;
      newIdButton.Click += new EventHandler(NewIdButton_Click);
      newIdPanel.Controls.Add(newIdButton);

      navigationTable.AddRow(newIdPanel, 2);

      navigationTable.AddVerticalSpace(20);

      this.panel.Controls.Add(navigationTable);

      var editTable = new Table();
      editTable.Width = new Unit(95, UnitType.Percentage);

      editTable.AddRow(Resources.Id, id);

      editTable.AddRow(Resources.Variables, string.Join(", ", Texts.GetItem(prefix).Fields.Select(f => "$$$" + f.Key).ToArray()));

      var currentText = DataGlobal.DataAccess.GetText(id);
      var fullText = currentText == null ? "Subject\r\nBody" : currentText.Text;
      
      _subjectTextBox = new TextBox();
      _subjectTextBox.Width = new Unit(80, UnitType.Percentage);
      _subjectTextBox.Text = fullText.Head("\r\n");
      editTable.AddRow(Resources.Subject, _subjectTextBox);

      _bodyTextBox = new TextBox();
      _bodyTextBox.TextMode = TextBoxMode.MultiLine;
      _bodyTextBox.Width = new Unit(80, UnitType.Percentage);
      _bodyTextBox.Height = new Unit(20, UnitType.Em);
      _bodyTextBox.Text = fullText.Tail("\r\n");
      editTable.AddRow(Resources.Body, _bodyTextBox);

      var buttonPanel = new Panel();

      var saveButton = new Button();
      saveButton.Width = new Unit(80d, UnitType.Point);
      saveButton.Text = Resources.Save;
      saveButton.Click += new EventHandler(SaveButton_Click);
      buttonPanel.Controls.Add(saveButton);

      var backButton = new Button();
      backButton.Width = new Unit(80d, UnitType.Point);
      backButton.Text = Resources.Back;
      backButton.Click += new EventHandler(BackButton_Click);
      buttonPanel.Controls.Add(backButton);

      editTable.AddRow(string.Empty, buttonPanel);

      this.panel.Controls.Add(editTable);
    }

    private void BackButton_Click(object sender, EventArgs e)
    {
      Redirect("ListText.aspx");
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel < 2)
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "ListText.SaveButton user id {0} name {1} has authorization level {2} meaning insufficient access.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel);
        RedirectAccessDenied();
        return;
      }

      var navigationTable = new Table();
      var id = Request.Params["id"];

      DataGlobal.DataAccess.SetText(new TextObject(id, _subjectTextBox.Text + "\r\n" + _bodyTextBox.Text));
      RedirectSelf();
    }

    private void NewIdButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel < 2)
      {
        BasicGlobal.Logger.Log(LogLevel.Warning, "ListText.SaveButton user id {0} name {1} has authorization level {2} meaning insufficient access.", CurrentUser.Id, CurrentUser.Name, CurrentUserAuthorizationLevel);
        RedirectAccessDenied();
        return;
      }

      var navigationTable = new Table();
      var id = Request.Params["id"];
      var prefix = id.Split(new[] { "." }, StringSplitOptions.None)[0];
      var rawId = prefix + "." + _newIdSectionDropdown.SelectedValue + "." + _newIdLanguageDropdown.SelectedValue;
      var newId = rawId.Replace(".none", string.Empty);
      Redirect("EditText.aspx?id=" + newId);
    }

    private void DeleteButton_Click(object sender, EventArgs e)
    {
      var id = _buttonToId[(LinkButton)sender];

      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      if (CurrentUserAuthorizationLevel >= 2)
      {
        DataGlobal.DataAccess.DeleteText(id);
      }

      RedirectSelf();
    }

    private void Backbutton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}