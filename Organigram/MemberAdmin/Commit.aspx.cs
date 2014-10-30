using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Web;

namespace MemberAdmin
{
  public partial class Commit : CustomPage
  {
    protected override string PageName
    {
      get { return "Commit"; }
    }
    
    private TextBox _deleteSafetyBoxSource;

    private TextBox _deleteSafetyBoxDest;

    private StringBuilder _report;

    private static object _lock = new object();

    protected void Page_Load(object sender, EventArgs e)
    {
      lock (_lock)
      {
        try
        {
          if (!SetupLdap())
          {
            RedirectLogin();
            return;
          }

          var table = new Table();
          table.Width = new Unit(90d, UnitType.Percentage);

          if (UpdateState == null ||
              UpdateState.Current == null)
          {
            table.AddRow(Resources.CommitNoUpdate);
            table.AddRow(ToLink(Resources.Ok, "./" + HomePage + ".aspx"));
          }
          else
          {
            switch (UpdateState.Operation)
            {
              case UpdateOperation.Create:
                DisplayCreate(table);
                break;
              case UpdateOperation.Delete:
                DisplayDelete(table);
                break;
              case UpdateOperation.Update:
                DisplayUpdate(table);
                break;
              default:
                table.AddRow(Resources.CommitNoOperation);
                table.AddRow(ToLink(Resources.Ok, "./" + HomePage + ".aspx"));
                break;
            }
          }

          this.panel.Controls.Add(table);
        }
        catch (Exception exception)
        {
          BasicGlobal.Logger.Log(Pirate.Util.Logging.LogLevel.Error, "Commit.Page_Load error: " + exception.ToString());
          RedirectHome();
        }
      }
    }

    private string GetSectionName(Person person)
    {
      var associaton = person.GetAssociation(Connection);

      if (associaton == null)
      {
        return Resources.NonMember;
      }
      else
      {
        return associaton.DisplayNameGerman;
      }
    }

    private void DisplayDelete(Table table)
    {
      _report = new StringBuilder();

      table.AddHeaderRow(Resources.CommitDelete, 3);

      if (UpdateState.Message != null)
      {
        table.AddRow(UpdateState.Message, 3);
      } 
      
      table.AddHeaderRow(Resources.Attribute);
      table.AddHeaderCell(Resources.Before, 1, HorizontalAlign.Center);

      var fields = UpdateState.Current.LdapFields;
      var original = UpdateState.Current.Reload(Connection);

      if (original is Person)
      {
        _report.AppendLine(string.Format("User id {0} name {1} is deleting a person.", CurrentUser.Id, CurrentUser.Name));
        table.AddRow(LdapResources.Section, 1);
        table.AddCell(GetSectionName((Person)original), 2, HorizontalAlign.Center);
      }
      else
      {
        _report.AppendLine(string.Format("User id {0} name {1} is deleting an object.", CurrentUser.Id, CurrentUser.Name));
        table.AddRow(LdapResources.DN, 1);
        table.AddCell(original.DN, 1, HorizontalAlign.Center);
      }

      _report.AppendLine(string.Format("DN is {0}", UpdateState.Current.DN));

      foreach (var field in fields)
      {
        if (field.Identifiying)
        {
          table.AddRow(field.Attribute.Text(), 1);
          table.AddCell(field.ValueString(MultiValueOutput.MultiLine), 1, HorizontalAlign.Center);
          _report.AppendLine(string.Format("{0} is {1}", field.Attribute.Text(), field.ValueString(MultiValueOutput.MultiLine)));
        }
      }

      Button rollBackDeleteButton = new Button();
      rollBackDeleteButton.Width = new Unit(80d, UnitType.Point);
      rollBackDeleteButton.Text = Resources.Rollback;
      rollBackDeleteButton.Click += new EventHandler(RollBackDeleteButton_Click);
      table.AddRow(string.Empty, rollBackDeleteButton);

      table.AddVerticalSpace(20);

      table.AddRow(string.Empty, Resources.DeleteWarning);

      byte[] buffer = null;

      using (var sha = new SHA256Managed())
      {
        buffer = sha.ComputeHash(Encoding.UTF8.GetBytes(original.DN)).Part(0, 5);
      }

      _deleteSafetyBoxSource = new TextBox();
      _deleteSafetyBoxSource.Text = buffer.ToHexString();
      _deleteSafetyBoxSource.Enabled = false;
      table.AddRow(Resources.SaftyCodeSource, _deleteSafetyBoxSource);

      _deleteSafetyBoxDest = new TextBox();
      table.AddRow(Resources.SaftyCodeDest, _deleteSafetyBoxDest);

      Button commitDeleteButton = new Button();
      commitDeleteButton.Width = new Unit(80d, UnitType.Point);
      commitDeleteButton.Text = Resources.Commit;
      commitDeleteButton.Click += new EventHandler(CommitDeleteButton_Click);
      table.AddRow(string.Empty, commitDeleteButton);
    }

    private void RollBackDeleteButton_Click(object sender, EventArgs e)
    {
      lock (_lock)
      {
        Reject();

        UpdateState = null;
        RedirectEdit();
      }
    }

    private void CommitDeleteButton_Click(object sender, EventArgs e)
    {
      lock (_lock)
      {
        if (!SetupLdap())
        {
          RedirectLogin();
          return;
        }

        ExecuteHandlingError(
          "Commit.Delete",
          () =>
          {
            if (_deleteSafetyBoxDest.Text == _deleteSafetyBoxSource.Text)
            {
              CommitDelete();
              _deleteSafetyBoxDest.Text = string.Empty;
              Redirect("SearchMember.aspx");
            }
            else
            {
              _deleteSafetyBoxDest.Text = string.Empty;
              RedirectSelf();
            }
          });
      }
    }

    private void CommitDelete()
    {
      try
      {
        _report.AppendLine("Commit of Delete");

        var original = UpdateState.Current.Reload(Connection);
        original.Delete(Connection);

        if (UpdateState.AcceptMails != null)
        {
          foreach (var mail in UpdateState.AcceptMails)
          {
            _report.AppendLine("Mail with subject {0} to {1}", mail.Subject, mail.To);
            BasicGlobal.Mailer.SendTextile(mail.Subject, mail.Body, mail.To);
          }
        }

        if (UpdateState.DeleteRequestId.HasValue)
        {
          _report.AppendLine("Deleting request id {0}", UpdateState.DeleteRequestId.HasValue);
          DataGlobal.DataAccess.DeleteRequest(UpdateState.DeleteRequestId.Value);
        }

        UpdateState = null;
        _report.AppendLine("Operation successful");
      }
      catch (Exception exception)
      {
        _report.AppendLine("Operation failed");
        _report.AppendLine("Error: " + exception.ToString());
        throw;
      }
      finally
      {
        BasicGlobal.Logger.Log(Pirate.Util.Logging.LogLevel.Info, _report.ToString()); 
        BasicGlobal.Mailer.SendToMembersQueue("[PPS][MA] Object deletion.", _report.ToString());
      }
    }

    private void DisplayCreate(Table table)
    {
      _report = new StringBuilder();

      table.AddHeaderRow(Resources.CommitCreate, 3);

      if (UpdateState.Message != null)
      {
        table.AddRow(UpdateState.Message, 3);
      }

      table.AddHeaderRow(Resources.Attribute);
      table.AddHeaderCell(Resources.After, 1, HorizontalAlign.Center);

      var fields = UpdateState.Current.LdapFields;

      if (UpdateState.Current is Person)
      {
        _report.AppendLine(string.Format("User id {0} name {1} is creating a person.", CurrentUser.Id, CurrentUser.Name));
        table.AddRow(LdapResources.Section, 1);
        table.AddCell(GetSectionName((Person)UpdateState.Current), 1, HorizontalAlign.Center);
      }
      else
      {
        _report.AppendLine(string.Format("User id {0} name {1} is creating an object.", CurrentUser.Id, CurrentUser.Name));
        table.AddRow(LdapResources.DN, 1);
        table.AddCell(UpdateState.Current.DN, 1, HorizontalAlign.Center);
      }

      _report.AppendLine(string.Format("DN is {0}", UpdateState.Current.DN));

      foreach (var field in fields)
      {
        if (!field.IsNullOrEmpty())
        {
          table.AddRow(field.Attribute.Text(), 1);
          table.AddCell(field.ValueString(MultiValueOutput.MultiLine), 2, HorizontalAlign.Center);

          _report.AppendLine(string.Format("{0} is {1}", field.Attribute.Text(), field.ValueString(MultiValueOutput.MultiLine)));
        }
      }

      var buttonPanel = new Panel();

      Button commitCreateButton = new Button();
      commitCreateButton.Width = new Unit(80d, UnitType.Point);
      commitCreateButton.Text = Resources.Commit;
      commitCreateButton.Click += new EventHandler(CommitCreateButton_Click);
      buttonPanel.Controls.Add(commitCreateButton);

      Button rollbackCreateButton = new Button();
      rollbackCreateButton.Width = new Unit(80d, UnitType.Point);
      rollbackCreateButton.Text = Resources.Rollback;
      rollbackCreateButton.Click += new EventHandler(RollbackCreateButton_Click);
      buttonPanel.Controls.Add(rollbackCreateButton);

      Button editCreateButton = new Button();
      editCreateButton.Width = new Unit(80d, UnitType.Point);
      editCreateButton.Text = Resources.Edit;
      editCreateButton.Click += new EventHandler(EditCreateButton_Click);
      buttonPanel.Controls.Add(editCreateButton);

      table.AddRow(string.Empty, 1);
      table.AddCell(buttonPanel, 3);
    }

    private void EditCreateButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      UpdateState = null;
      RedirectEdit();
    }

    private void RollbackCreateButton_Click(object sender, EventArgs e)
    {
      lock (_lock)
      {
        Reject();

        UpdateState = null;
        RedirectHome();
      }
    }

    private void CommitCreateButton_Click(object sender, EventArgs e)
    {
      lock (_lock)
      {
        if (!SetupLdap())
        {
          RedirectLogin();
          return;
        }

        ExecuteHandlingError(
          "Commit.Create",
          () =>
          {
            CommitCreate();
            RedirectEdit();
          }
        );
      }
    }

    private void CommitCreate()
    {
      try
      {
        _report.AppendLine("Commit of Create");

        UpdateState.Current.Create(Connection);

        if (UpdateState.AcceptMails != null)
        {
          foreach (var mail in UpdateState.AcceptMails)
          {
            _report.AppendLine("Mail with subject {0} to {1}", mail.Subject, mail.To);
            BasicGlobal.Mailer.SendTextile(mail.Subject, mail.Body, mail.To);
          }
        }

        if (UpdateState.DeleteRequestId.HasValue)
        {
          _report.AppendLine("Deleting request id {0}", UpdateState.DeleteRequestId.HasValue);
          DataGlobal.DataAccess.DeleteRequest(UpdateState.DeleteRequestId.Value);
        }

        UpdateState = new MemberAdmin.UpdateState(
          UpdateOperation.Update,
          UpdateState.Current.Reload(Connection));
        _report.AppendLine("Operation successful");
      }
      catch (Exception exception)
      {
        _report.AppendLine("Operation failed");
        _report.AppendLine("Error: " + exception.ToString());
        throw;
      }
      finally
      {
        BasicGlobal.Logger.Log(Pirate.Util.Logging.LogLevel.Info, _report.ToString());
        BasicGlobal.Mailer.SendToMembersQueue("[PPS][MA] Object creation.", _report.ToString());
      }
    }

    private void DisplayUpdate(Table table)
    {
      _report = new StringBuilder();

      table.AddHeaderRow(Resources.CommitUpdate, 3);

      if (UpdateState.Message != null)
      {
        table.AddRow(UpdateState.Message, 3);
      } 
      
      table.AddHeaderRow(Resources.Attribute);
      table.AddHeaderCell(Resources.Before, 1, HorizontalAlign.Center);
      table.AddHeaderCell(Resources.After, 1, HorizontalAlign.Center);

      var fields = UpdateState.Current.LdapFields;
      var original = UpdateState.Current.Reload(Connection);
      var differences = UpdateState.Current.GetDifferences(original, MultiValueOutput.MultiLine);

      if (UpdateState.Current is Person)
      {
        _report.AppendLine(string.Format("User id {0} name {1} is updating a person.", CurrentUser.Id, CurrentUser.Name));

        if (UpdateState.Current.DN == original.DN)
        {
          _report.AppendLine(string.Format("DN is {0}", UpdateState.Current.DN));
          table.AddRow(LdapResources.Section, 1);
          table.AddCell(GetSectionName((Person)UpdateState.Current), 2, HorizontalAlign.Center);
        }
        else
        {
          _report.AppendLine(string.Format("DN was {0}", original.DN));
          _report.AppendLine(string.Format("DN is moved to {0}", UpdateState.Current.DN));
          table.AddRow(LdapResources.Section, 1);
          table.AddCell(GetSectionName((Person)original), 1, HorizontalAlign.Center);
          table.AddCell(GetSectionName((Person)UpdateState.Current), 1, HorizontalAlign.Center);
        }
      }
      else
      {
        _report.AppendLine(string.Format("User id {0} name {1} is updating an object.", CurrentUser.Id, CurrentUser.Name));

        if (UpdateState.Current.DN == original.DN)
        {
          _report.AppendLine(string.Format("DN is {0}", UpdateState.Current.DN));
          table.AddRow(LdapResources.DN, 1);
          table.AddCell(UpdateState.Current.DN, 2, HorizontalAlign.Center);
        }
        else
        {
          _report.AppendLine(string.Format("DN was {0}", original.DN));
          _report.AppendLine(string.Format("DN is moved to {0}", UpdateState.Current.DN));
          table.AddRow(LdapResources.DN, 1);
          table.AddCell(original.DN, 1, HorizontalAlign.Center);
          table.AddCell(UpdateState.Current.DN, 1, HorizontalAlign.Center);
        }
      }

      foreach (var field in fields)
      {
        if (field.Identifiying && !differences.Any(d => d.Attribute.Equals(field.Attribute)))
        {
          _report.AppendLine(string.Format("{0} is {1}", field.Attribute.Text(), field.ValueString(MultiValueOutput.MultiLine)));
          table.AddRow(field.Attribute.Text(), 1);
          table.AddCell(field.ValueString(MultiValueOutput.MultiLine), 2, HorizontalAlign.Center);
        }
      }

      foreach (var difference in differences)
      {
        var before = (string.IsNullOrEmpty(difference.Before) ? "N/A" : difference.Before).Replace(Environment.NewLine, ", ");
        var after = (string.IsNullOrEmpty(difference.After) ? "N/A" : difference.After).Replace(Environment.NewLine, ", ");
        _report.AppendLine(string.Format("{0} is changed from {1} to {2}", difference.Attribute.Text(), before, after));
        table.AddRow(difference.Attribute.Text());
        table.AddCell((difference.Before ?? string.Empty).Replace(Environment.NewLine, NewLine), 1, HorizontalAlign.Center);
        table.AddCell((difference.After ?? string.Empty).Replace(Environment.NewLine, NewLine), 1, HorizontalAlign.Center);
      }

      var buttonPanel = new Panel();

      Button commitUpdateButton = new Button();
      commitUpdateButton.Width = new Unit(80d, UnitType.Point);
      commitUpdateButton.Text = Resources.Commit;
      commitUpdateButton.Click += new EventHandler(CommitUpdateButton_Click);
      buttonPanel.Controls.Add(commitUpdateButton);

      Button rollbackUpdateButton = new Button();
      rollbackUpdateButton.Width = new Unit(80d, UnitType.Point);
      rollbackUpdateButton.Text = Resources.Rollback;
      rollbackUpdateButton.Click += new EventHandler(RollbackUpdateButton_Click);
      buttonPanel.Controls.Add(rollbackUpdateButton);

      Button editUpdateButton = new Button();
      editUpdateButton.Width = new Unit(80d, UnitType.Point);
      editUpdateButton.Text = Resources.Edit;
      editUpdateButton.Click += new EventHandler(EditUpdateButton_Click);
      buttonPanel.Controls.Add(editUpdateButton);

      table.AddRow(string.Empty, 1);
      table.AddCell(buttonPanel, 3);
    }

    private void EditUpdateButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      UpdateState = null;
      RedirectEdit();
    }

    private void RedirectEdit()
    {
      if (UpdateState.Current is Person)
      {
        Redirect("DisplayMember.aspx?member=" + ((Person)UpdateState.Current).Id.ToString());
      }
      else 
      {
        RedirectHome();
      }
    }

    private void RollbackUpdateButton_Click(object sender, EventArgs e)
    {
      lock (_lock)
      {
        Reject();

        UpdateState = new MemberAdmin.UpdateState(
          UpdateOperation.Update,
          UpdateState.Current.Reload(Connection));
        RedirectEdit();
      }
    }

    private void Reject()
    {
      try
      {
        _report.AppendLine("Rollback of {0}", UpdateState.Operation);

        switch (UpdateState.Operation)
        {
          case UpdateOperation.Create:
          case UpdateOperation.Update:
          case UpdateOperation.Delete:
            foreach (var mail in UpdateState.RejectMails)
            {
              _report.AppendLine("Mail with subject {0} to {1}", mail.Subject, mail.To);
              BasicGlobal.Mailer.SendTextile(mail.Subject, mail.Body, mail.To);
            }
            break;
        }

        if (UpdateState.DeleteRequestId.HasValue)
        {
          _report.AppendLine("Deleting request id {0}", UpdateState.DeleteRequestId.HasValue);
          DataGlobal.DataAccess.DeleteRequest(UpdateState.DeleteRequestId.Value);
        }

        _report.AppendLine("Operation successful");
      }
      catch (Exception exception)
      {
        _report.AppendLine("Operation failed");
        _report.AppendLine("Error: " + exception.ToString());
        throw;
      }
      finally
      {
        BasicGlobal.Logger.Log(Pirate.Util.Logging.LogLevel.Info, _report.ToString());
      }
    }

    private void CommitUpdateButton_Click(object sender, EventArgs e)
    {
      lock (_lock)
      {
        if (!SetupLdap())
        {
          RedirectLogin();
          return;
        }

        ExecuteHandlingError(
          "Commit.Update",
          () =>
          {
            CommitUpdate();
            RedirectEdit();
          }
        );
      }
    }

    private void CommitUpdate()
    {
      try
      {
        _report.AppendLine("Commit of Update");

        UpdateState.Current.Modify(Connection);

        if (UpdateState.AcceptMails != null)
        {
          foreach (var mail in UpdateState.AcceptMails)
          {
            _report.AppendLine("Mail with subject {0} to {1}", mail.Subject, mail.To);
            BasicGlobal.Mailer.SendTextile(mail.Subject, mail.Body, mail.To);
          }
        }

        if (UpdateState.DeleteRequestId.HasValue)
        {
          _report.AppendLine("Deleting request id {0}", UpdateState.DeleteRequestId.HasValue);
          DataGlobal.DataAccess.DeleteRequest(UpdateState.DeleteRequestId.Value);
        }

        UpdateState = new MemberAdmin.UpdateState(
          UpdateOperation.Update,
          UpdateState.Current.Reload(Connection));
        _report.AppendLine("Operation successful");
      }
      catch (Exception exception)
      {
        _report.AppendLine("Operation failed");
        _report.AppendLine("Error: " + exception.ToString());
        throw;
      }
      finally
      {
        BasicGlobal.Logger.Log(Pirate.Util.Logging.LogLevel.Info, _report.ToString());
        BasicGlobal.Mailer.SendToMembersQueue("[PPS][MA] Object update.", _report.ToString());
      }
    }
  }
}