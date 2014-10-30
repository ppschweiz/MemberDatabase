using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Pirate.Ldap.Web;

namespace MemberService
{
  public partial class Default : CustomPage
  {
    protected override string PageName
    {
      get { return "Default"; }
    }
    
    private static string GetEmployeeTypeName(EmployeeType type)
    {
      switch (type)
      {
        case EmployeeType.Sympathizer:
        case EmployeeType.Pirate:
        case EmployeeType.Fleet:
          return Resources.Member;
        default:
          return Resources.NonMember;
      }
    }

    public static string GetVotingRightUntilText(DateTime votingRightUntil)
    {
      if (votingRightUntil.Year >= 2000)
      {
        return votingRightUntil.ToLongDateString();
      }
      else
      {
        return Resources.NoVotingRight;
      }
    }

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

      table.AddRow(LdapResources.Language, Language.GetName(user.PreferredLanguage));

      var changeLanguage = new LinkButton();
      changeLanguage.Text = Resources.ChangeLanguage;
      changeLanguage.Click += new EventHandler(ChangeLanguage_Click);
      table.AddRow(string.Empty, changeLanguage);

      table.AddVerticalSpace(10);
      table.AddRow(Resources.MembershipStatus, GetEmployeeTypeName(user.EmployeeType));

      switch (user.EmployeeType)
      {
        case EmployeeType.Pirate:
        case EmployeeType.Sympathizer:
        case EmployeeType.Fleet:
          table.AddRow(Resources.MemberNumber, user.EmployeeNumber.ToString());

          table.AddRow(LdapResources.Section, user.GetAssociation(Connection).DisplayNameGerman);
          var changeSectionButton = new LinkButton();
          changeSectionButton.Text = Resources.ChangeSection;
          changeSectionButton.Click += new EventHandler(ChangeSectionButton_Click);
          table.AddRow(string.Empty, changeSectionButton);

          table.AddRow(LdapResources.VotingRightUntil, GetVotingRightUntilText(user.VotingRightUntil));
          var leaveButton = new LinkButton();
          leaveButton.Text = Resources.Leave;
          leaveButton.Click += new EventHandler(LeaveButton_Click);
          table.AddRow(string.Empty, leaveButton);
          break;
        case EmployeeType.LandLubber:
        case EmployeeType.Veteran:
          var joinButton = new LinkButton();
          joinButton.Text = Resources.Join;
          joinButton.Click += new EventHandler(JoinButton_Click);
          table.AddRow(string.Empty, joinButton);
          break;
      }

      AddPositions(table, user);

      table.AddVerticalSpace(10);
      table.AddRow(LdapResources.Surname, user.Surname);
      table.AddRow(LdapResources.Givenname, user.Givenname);
      table.AddRow(LdapResources.Username, user.Nickname);

      var changePasswordButton = new LinkButton();
      changePasswordButton.Text = Resources.ChangePassword;
      changePasswordButton.Click += new EventHandler(ChangePasswordButton_Click);
      table.AddRow(string.Empty, changePasswordButton);

      table.AddVerticalSpace(10);
      table.AddRow(LdapResources.Country, Countries.GetName(binDir, user.Country));
      table.AddRow(LdapResources.State, States.GetName(binDir, user.State));
      table.AddRow(LdapResources.Street, user.Street);
      table.AddRow(LdapResources.Location, user.Location);
      table.AddRow(LdapResources.PostalCode, user.PostalCode);
      var changeAddressButton = new LinkButton();
      changeAddressButton.Text = Resources.ChangeAddress;
      changeAddressButton.Click += new EventHandler(ChangeAddressButton_Click);
      table.AddRow(string.Empty, changeAddressButton);

      table.AddVerticalSpace(10);
      table.AddRow(LdapResources.Email, string.Join(NewLine, user.Emails.ToArray()));
      table.AddRow(LdapResources.AlternateMail, string.Join(NewLine, user.AlternateEmails.ToArray()));
      var changeEmailButton = new LinkButton();
      changeEmailButton.Text = Resources.ChangeEmail;
      changeEmailButton.Click += new EventHandler(ChangeEmailButton_Click);
      table.AddRow(string.Empty, changeEmailButton);

      table.AddVerticalSpace(10);
      table.AddRow(LdapResources.Phone, user.Phone);
      table.AddRow(LdapResources.Mobile, user.Mobile);
      var changePhoneButton = new LinkButton();
      changePhoneButton.Text = Resources.ChangePhone;
      changePhoneButton.Click += new EventHandler(ChangePhoneButton_Click);
      table.AddRow(string.Empty, changePhoneButton);

      table.AddVerticalSpace(10);
      var logoffButton = new LinkButton();
      logoffButton.Text = Resources.Logoff;
      logoffButton.Click += new EventHandler(LogoffButton_Click);
      table.AddRow(string.Empty, logoffButton);

      this.panel.Controls.Add(table);
    }

    private void JoinButton_Click(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      Redirect(BasicGlobal.Config.RegistrationServiceAddress + "RequestMembership.aspx?data=" + BasicGlobal.Config.Secure((new MembershipRequest(CurrentUser.DN, DateTime.Now.AddHours(1))).ToBinary()).ToHexString());
    }

    private void AddPositions(Table table, Person user)
    {
      var positions = Session["positions"] as List<Position>;

      if (positions == null)
      {
        positions = user.ComputeVisiblePositions(Connection).ToList();
        Session["positions"] = positions;
      }

      var first = true;

      foreach (var position in positions)
      {
        var names = new List<string>();

        if (position.Association != null)
        {
          names.Add(position.Association.DisplayNameGerman);
        }

        if (position.Group != null)
        {
          names.Add(position.Group.DisplayNameGerman);
        }

        if (position.Role != null)
        {
          names.Add(position.Role.DisplayNameGerman);
        }

        var fullname = string.Join(", ", names.ToArray());
        WebControl control = null;

        if (position.Group != null && !string.IsNullOrEmpty(position.Group.Website))
        {
          var link = new HyperLink();
          link.NavigateUrl = position.Group.Website;
          link.Text = fullname;
          control = link;
        }
        else if (position.Association != null && !string.IsNullOrEmpty(position.Association.Website))
        {
          var link = new HyperLink();
          link.NavigateUrl = position.Association.Website;
          link.Text = fullname;
          control = link;
        }
        else
        {
          var label = new Label();
          label.Text = fullname;
          control = label;
        }

        if (first)
        {
          table.AddRow(Resources.Groups, control);
          first = false;
        }
        else
        {
          table.AddRow(string.Empty, control);
        }
      }

      if (positions.Count > 0 || 
          Connection.SearchFirst<Group>(Names.GroupAdministration).MemberDns.Contains(CurrentUser.DN))
      {
        var editCertificateButton = new LinkButton();
        editCertificateButton.Text = Resources.CertificateEdit;
        editCertificateButton.Click += new EventHandler(EditCertificateButton_Click);
        table.AddRow(Resources.Certificates, editCertificateButton);
      }
    }

    private void EditCertificateButton_Click(object sender, EventArgs e)
    {
      Redirect("ListCertificates.aspx");
    }

    private void ChangeSectionButton_Click(object sender, EventArgs e)
    {
      Redirect("ChangeSection.aspx");
    }

    private void LeaveButton_Click(object sender, EventArgs e)
    {
      Redirect("Leave.aspx");
    }

    private void LogoffButton_Click(object sender, EventArgs e)
    {
      Ldap.Logoff(Application, Session, Request);
      RedirectHome();
    }

    private void ChangeLanguage_Click(object sender, EventArgs e)
    {
      Redirect("ChangeLang.aspx");
    }

    private void ChangePhoneButton_Click(object sender, EventArgs e)
    {
      Redirect("ChangePhone.aspx");
    }

    private void ChangeEmailButton_Click(object sender, EventArgs e)
    {
      Redirect("ChangeEmail.aspx");
    }

    private void ChangeAddressButton_Click(object sender, EventArgs e)
    {
      Redirect("ChangeAddress.aspx");
    }

    private void ChangePasswordButton_Click(object sender, EventArgs e)
    {
      Redirect("ChangePassword.aspx");
    }
  }
}