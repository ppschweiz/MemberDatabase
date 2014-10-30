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
using Pirate.Ldap.Data;
using Pirate.Util.Logging;

namespace MemberService
{
  public partial class ChangeSection : CustomPage
  {
    protected override string PageName
    {
      get { return "ChangeSection"; }
    }
    
    private DropDownList _sectionChoice;

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
      table.AddHeaderRow(Resources.ChangeSection, 2);
      table.AddVerticalSpace(10);

      _sectionChoice = new DropDownList();

      foreach (var section in SectionExtensions.GetValues(Access.Connection, false))
      {
        _sectionChoice.Items.Add(new ListItem(section.Display, section.Value));
      }

      table.AddRow(LdapResources.Section, _sectionChoice);
      table.AddRow(string.Empty, Resources.ChangeSectionInfo1);
      table.AddVerticalSpace(10);

      table.AddRow(string.Empty, Resources.ChangeSectionInfo2);
      table.AddVerticalSpace(10);

      var buttonPanel = new Panel();

      var saveButton = new Button();
      saveButton.Text = Resources.ChangeSection;
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

      if (valid)
      {
        if (!SetupLdap())
        {
          RedirectLogin();
          return;
        }

        var person = CurrentUser;
        var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");

        var requestId = DataGlobal.DataAccess.CreateRequest(new Request(0, RequestAction.Transfer, person.DN, _sectionChoice.SelectedValue, string.Empty, DateTime.Now));

        var text = Texts.GetText(
          DataGlobal.DataAccess,
          new Texts.TransferRequestPendingEmail(),
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

        BasicGlobal.Logger.Log(LogLevel.Info, "New transfer request from user id {0} name {1} from {2} to {3}.", person.Id, person.Name, person.DN, _sectionChoice.SelectedValue); 
        BasicGlobal.Mailer.SendToMembersQueue("New tranfer request", builder.ToString());

        RedirectMessage(new Texts.TransferRequestPendingMessage());
      }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      RedirectHome();
    }
  }
}