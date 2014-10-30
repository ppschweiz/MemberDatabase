using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberService
{
  public partial class Leave : CustomPage
  {
    protected override string PageName
    {
      get { return "Leave"; }
    }

    private RadioButton _deleteUserAccount; 
    
    private RadioButton _deletePersonalData;

    private RadioButton _leaveAccount;

    private TextBox _reasonTextbox;

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
      table.AddHeaderRow(Resources.Leave, 2);
      table.AddVerticalSpace(10);

      _deletePersonalData = new RadioButton();
      _deletePersonalData.Text = Resources.DeletePersonalData;
      table.AddRow(_deletePersonalData);
      table.AddRow(Resources.DeletePersonalDataInfo, 1);
      table.AddVerticalSpace(10);

      _deleteUserAccount = new RadioButton();
      _deleteUserAccount.Text = Resources.DeleteUserAccount;
      table.AddRow(_deleteUserAccount);
      table.AddRow(Resources.DeleteUserAccountInfo, 1);
      table.AddVerticalSpace(10);

      _leaveAccount = new RadioButton();
      _leaveAccount.Text = Resources.LeaveAccount;
      table.AddRow(_leaveAccount);
      table.AddVerticalSpace(10);

      table.AddRow(Resources.LeaveReason, 1);
      _reasonTextbox = new TextBox();
      _reasonTextbox.TextMode = TextBoxMode.MultiLine;
      _reasonTextbox.Width = new Unit(50, UnitType.Em);
      _reasonTextbox.Height = new Unit(10, UnitType.Em);
      table.AddRow(_reasonTextbox, 1);
      table.AddVerticalSpace(10);

      table.AddRow(Resources.LeaveInfo, 1);
      table.AddVerticalSpace(10);

      var buttonPanel = new Panel();

      var saveButton = new Button();
      saveButton.Text = Resources.Leave;
      saveButton.Click += new EventHandler(SaveButton_Click);
      buttonPanel.Controls.Add(saveButton);

      var cancelButton = new Button();
      cancelButton.Text = Resources.Cancel;
      cancelButton.Click += new EventHandler(CancelButton_Click);
      buttonPanel.Controls.Add(cancelButton);

      table.AddRow(buttonPanel);

      this.panel.Controls.Add(table);
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      bool valid = true;

      if (valid)
      {
        if (!SetupLdap())
        {
          RedirectLogin();
          return;
        }

        var report = new StringBuilder();

        var person = CurrentUser;
        var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");
        int requestId = -1;

        if (_deleteUserAccount.Checked)
        {
          requestId = DataGlobal.DataAccess.CreateRequest(new Request(0, RequestAction.LeaveAndDelete, person.DN, string.Empty, string.Empty, DateTime.Now));
        }
        else if (_deletePersonalData.Checked)
        {
          requestId = DataGlobal.DataAccess.CreateRequest(new Request(0, RequestAction.LeaveAndRemoveData, person.DN, string.Empty, string.Empty, DateTime.Now));
        }
        else
        {
          requestId = DataGlobal.DataAccess.CreateRequest(new Request(0, RequestAction.Leave, person.DN, string.Empty, string.Empty, DateTime.Now));
        }

        var text = Texts.GetText(
          DataGlobal.DataAccess,
          new Texts.LeaveRequestPendingEmail(),
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

        BasicGlobal.Logger.Log(LogLevel.Info, "New leave request from user id {0} name {1}", person.Id, person.Name);
        BasicGlobal.Mailer.SendToMembersQueue("New leave request", builder.ToString());

        RedirectMessage(new Texts.LeaveRequestPendingMessage()); 
      }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}