using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberService
{
  public partial class ChangeAddress : CustomPage
  {
    protected override string PageName
    {
      get { return "ChangeAddress"; }
    }

    private TextBox _streetBox;
    private Label _streetValid;

    private TextBox _locationBox;
    private Label _locationValid;

    private TextBox _postcodeBox;
    private Label _postcodeValid;

    private DropDownList _countryList;
    private DropDownList _stateList;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var user = CurrentUser;

      var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");

      var table = new Table();
      table.AddHeaderRow(Resources.ChangeAddress, 2);
      table.AddVerticalSpace(10);

      _countryList = new DropDownList();
      _countryList.Items.Add(new ListItem(Resources.NotDefined, string.Empty));
      foreach (var country in Countries.GetList(binDir))
      {
        _countryList.Items.Add(new ListItem(country.Display, country.Value));
      }
      _countryList.SelectedValue = user.Country;
      table.AddRow(LdapResources.Country, _countryList);

      _stateList = new DropDownList();
      _stateList.Items.Add(new ListItem(Resources.NotDefined, string.Empty));
      foreach (var state in States.GetList(binDir))
      {
        _stateList.Items.Add(new ListItem(state.Display, state.Value));
      }
      _stateList.SelectedValue = user.State;
      table.AddRow(LdapResources.State, _stateList);

      _streetBox = new TextBox();
      _streetBox.Text = user.Street;
      _streetValid = new Label();
      _streetValid.Text = string.Empty;
      table.AddRow(LdapResources.Street, _streetBox, _streetValid);

      _locationBox = new TextBox();
      _locationBox.Text = user.Location;
      _locationValid = new Label();
      _locationValid.Text = string.Empty;
      table.AddRow(LdapResources.Location, _locationBox, _locationValid);

      _postcodeBox = new TextBox();
      _postcodeBox.Text = user.PostalCode;
      _postcodeValid = new Label();
      _postcodeValid.Text = string.Empty;
      table.AddRow(LdapResources.PostalCode, _postcodeBox, _postcodeValid);

      var buttonPanel = new Panel();

      var saveButton = new Button();
      saveButton.Text = Resources.Save;
      saveButton.Click += new EventHandler(SaveButton_Click);
      buttonPanel.Controls.Add(saveButton);

      var cancelButton = new Button();
      cancelButton.Text = Resources.Cancel;
      cancelButton.Click += new EventHandler(CancelButton_Click);
      buttonPanel.Controls.Add(cancelButton);

      table.AddRow(string.Empty, buttonPanel);

      this.panel.Controls.Add(table);
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      bool valid = true;

      if (string.IsNullOrEmpty(_locationBox.Text))
      {
        _locationValid.Text = Resources.LocationNotEmpty;
        valid = false;
      }

      if (valid)
      {
        if (!SetupLdap())
        {
          RedirectLogin();
          return;
        }

        var report = new StringBuilder();

        try
        {
          var user = CurrentUser;
          var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");

          report.AppendLine(string.Format("User id {0} name {1} changed his data.", user.Id, user.Name));
          report.AppendLine(string.Format("Country changed from {0} to {1}", Countries.GetName(binDir, user.Country), Countries.GetName(binDir, _countryList.SelectedValue)));
          report.AppendLine(string.Format("State changed from {0} to {1}", States.GetName(binDir, user.State), States.GetName(binDir, _stateList.SelectedValue)));
          report.AppendLine(string.Format("Location changed from {0} to {1}", FixEmpty(user.Location), _locationBox.Text));
          report.AppendLine(string.Format("Street changed from {0} to {1}", FixEmpty(user.Street), _streetBox.Text));
          report.AppendLine(string.Format("PostalCode changed from {0} to {1}", FixEmpty(user.PostalCode), _postcodeBox.Text));

          user.Country = _countryList.SelectedValue;
          user.State = _stateList.SelectedValue;
          user.Location = _locationBox.Text;
          user.Street = _streetBox.Text;
          user.PostalCode = _postcodeBox.Text;

          user.Modify(Connection);
          RedirectHome();

          report.AppendLine("Operation successful");
        }
        catch (Exception exception)
        {
          report.AppendLine("Operation failed");
          report.AppendLine(exception.ToString());
        }
        finally
        {
          BasicGlobal.Logger.Log(LogLevel.Info, report.ToString());
          BasicGlobal.Mailer.SendToMembersQueue("User data changed", report.ToString());
        }
      }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}