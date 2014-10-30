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

namespace MemberService
{
  public partial class ChangePhone : CustomPage
  {
    protected override string PageName
    {
      get { return "ChangePhone"; }
    }
    
    private TextBox _phoneBox;
    private Label _phoneValid;

    private TextBox _mobileBox;
    private Label _mobileValid;

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
      table.AddHeaderRow(Resources.ChangePhone, 2);
      table.AddVerticalSpace(10);

      _phoneBox = new TextBox();
      _phoneBox.Text = user.Phone;
      _phoneValid = new Label();
      _phoneValid.Text = string.Empty;
      table.AddRow(LdapResources.Phone, _phoneBox, _phoneValid);

      _mobileBox = new TextBox();
      _mobileBox.Text = user.Mobile;
      _mobileValid = new Label();
      _mobileValid.Text = string.Empty;
      table.AddRow(LdapResources.Mobile, _mobileBox, _mobileValid);

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

    private static readonly char[] AllowedChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', '(', ')', '+', '#', '*' };

    private static bool PhoneValid(string text)
    {
      if (string.IsNullOrEmpty(text))
      {
        return true;
      }
      else
      {
        return text.ToCharArray().All(c => AllowedChars.Contains(c));
      }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      bool valid = true;

      if (!PhoneValid(_phoneBox.Text))
      {
        _phoneValid.Text = Resources.PhoneInvalid;
        valid = false;
      }
      else
      {
        _phoneValid.Text = string.Empty;
      }

      if (!PhoneValid(_mobileBox.Text))
      {
        _mobileValid.Text = Resources.PhoneInvalid;
        valid = false;
      }
      else
      {
        _mobileValid.Text = string.Empty;
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

          report.AppendLine(string.Format("User id {0} name {1} changed his data.", user.Id, user.Name));
          report.AppendLine(string.Format("Phone changed from {0} to {1}", FixEmpty(user.Phone), _phoneBox.Text));
          report.AppendLine(string.Format("Mobile changed from {0} to {1}", FixEmpty(user.Mobile), _mobileBox.Text));

          user.Phone = _phoneBox.Text;
          user.Mobile = _mobileBox.Text;

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
          BasicGlobal.Logger.Log(Pirate.Util.Logging.LogLevel.Info, report.ToString()); 
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