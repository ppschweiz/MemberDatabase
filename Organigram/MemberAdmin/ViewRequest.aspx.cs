using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;
using Pirate.Util;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  public partial class ViewRequest : CustomPage
  {
    protected override string PageName
    {
      get { return "ViewRequest"; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var id = Parsing.TryParseInt32(Request.Params["id"], 0);
      var request = DataGlobal.DataAccess.GetRequest(id);

      if (request == null)
      {
        HandleError();
      }
      else 
      {
        switch (request.Action)
        {
          case RequestAction.Join:
            HandleJoin(request);
            break;
          case RequestAction.Transfer:
            HandleTransfer(request);
            break;
          case RequestAction.Leave:
            HandleLeave(request);
            break;
          case RequestAction.LeaveAndRemoveData:
            HandleLeave(request);
            break;
          case RequestAction.LeaveAndDelete:
            HandleLeave(request);
            break;
          default:
            HandleError();
            break;
        }
      }
    }

    private void HandleError()
    {
      var table = new Table();

      table.AddRow(Resources.ViewActionError);
      table.AddVerticalSpace(20);

      var link = new HyperLink();
      link.Text = Resources.Done;
      link.NavigateUrl = "Default.aspx";
      table.AddRow(link);

      this.panel.Controls.Add(table);
    }

    private void HandleLeave(Request request)
    {
      var person = Connection.SearchFirst<Person>(request.OldDn);

      if (person == null)
      {
        HandleError();
      }
      else
      {
        var acceptedMails = new List<UpdateMail>();
        var rejectedMails = new List<UpdateMail>();

        var acceptedText = Texts.GetText(DataGlobal.DataAccess, new Texts.LeaveAcceptedEmail(), person);
        var rejectedText = Texts.GetText(DataGlobal.DataAccess, new Texts.LeaveRejectedEmail(), person);

        foreach (var email in person.Emails)
        {
          acceptedMails.Add(new UpdateMail(acceptedText.Item1, acceptedText.Item2, Mailer.EncodeAddress(email, person.Name)));
          rejectedMails.Add(new UpdateMail(rejectedText.Item1, rejectedText.Item2, Mailer.EncodeAddress(email, person.Name)));
        }

        var sectionAddress = GetSectionAddress(person);
        var additonalAddress = new MailAddress(BasicGlobal.Config.AdditionalNotificationAddress, BasicGlobal.Config.AdditionalNotificationDisplayName);

        var acceptedSectionMailText = Texts.GetText(DataGlobal.DataAccess, new Texts.LeaveAcceptedSectionEmail(), person, new StringPair(Texts.SectionItem.UserMail, acceptedText.Item2));
        var rejectedSectionMailText = Texts.GetText(DataGlobal.DataAccess, new Texts.LeaveRejectedSectionEmail(), person, new StringPair(Texts.SectionItem.UserMail, rejectedText.Item2));
        acceptedMails.Add(new UpdateMail(acceptedSectionMailText.Item1, acceptedSectionMailText.Item2, additonalAddress));

        if (sectionAddress != null)
        {
          acceptedMails.Add(new UpdateMail(acceptedSectionMailText.Item1, acceptedSectionMailText.Item2, sectionAddress));
          rejectedMails.Add(new UpdateMail(rejectedSectionMailText.Item1, rejectedSectionMailText.Item2, sectionAddress));
        }

        switch (request.Action)
        {
          case RequestAction.Leave:
            person.EmployeeType = EmployeeType.Veteran;
            person.Leaving.Add(DateTime.Now);
            person.Move(Names.People);

            UpdateState = new MemberAdmin.UpdateState(
              UpdateOperation.Update,
              person,
              acceptedMails,
              rejectedMails,
              Resources.LeaveRequestInfo,
              request.Id);
            break;
          case RequestAction.LeaveAndRemoveData:
            person.EmployeeType = EmployeeType.Veteran;
            person.Leaving.Add(DateTime.Now);
            person.BirthDate = null;
            person.Country = null;
            person.Description = null;
            person.Gender = null;
            person.Givenname = null;
            person.Info = null;
            person.Location = null;
            person.Mobile = null;
            person.Name = null;
            person.Phone = null;
            person.Photo = null;
            person.PostalCode = null;
            person.State = null;
            person.Street = null;
            person.Surname = null;
            person.VotingRightUntil = DateTime.MinValue;
            person.Move(Names.People);

            UpdateState = new MemberAdmin.UpdateState(
              UpdateOperation.Update,
              person,
              acceptedMails,
              rejectedMails,
              Resources.LeaveRequestInfo,
              request.Id);
            break;
          case RequestAction.LeaveAndDelete:
            UpdateState = new MemberAdmin.UpdateState(
              UpdateOperation.Delete,
              person,
              acceptedMails,
              rejectedMails,
              Resources.LeaveRequestInfo,
              request.Id);
            break;
        }

        RedirectCommit();
      }
    }

    private void HandleTransfer(Request request)
    {
      var person = Connection.SearchFirst<Person>(request.OldDn);

      if (person == null)
      {
        HandleError();
      }
      else
      {
        var acceptedMails = new List<UpdateMail>();
        var rejectedMails = new List<UpdateMail>();
        var newAssociation =
          Connection.SearchFirst<Section>(request.NewDn.Replace(Names.MembersContainerPrefix, string.Empty)) as Association ??
          Connection.SearchFirst<Organization>(Names.Party) as Association;
        var oldSectionName = new StringPair(Texts.TransferItem.OldSectionNameField, person.GetAssociation(Connection).DisplayNameGerman);
        var newSectionName = new StringPair(Texts.TransferItem.NewSectionNameField, newAssociation.DisplayNameGerman);

        var memberAddress = Mailer.EncodeAddress(person.Emails.First(), person.Name);
        var rejectedText = Texts.GetText(DataGlobal.DataAccess, new Texts.TransferRejectedMail(), person, oldSectionName, newSectionName);
        rejectedMails.Add(new UpdateMail(rejectedText.Item1, rejectedText.Item2, memberAddress));

        var oldSectionAddress = GetSectionAddress(person);
        var additonalAddress = new MailAddress(BasicGlobal.Config.AdditionalNotificationAddress, BasicGlobal.Config.AdditionalNotificationDisplayName);

        var acceptedOldSectionMail = Texts.GetText(DataGlobal.DataAccess, new Texts.TransferAcceptedOldSectionMail(), person, oldSectionName, newSectionName);
        var rejectedOldSectionMail = Texts.GetText(DataGlobal.DataAccess, new Texts.TransferRejectedOldSectionMail(), person, oldSectionName, newSectionName);
        rejectedMails.Add(new UpdateMail(acceptedOldSectionMail.Item1, acceptedOldSectionMail.Item2, additonalAddress));

        if (oldSectionAddress != null)
        {
          acceptedMails.Add(new UpdateMail(acceptedOldSectionMail.Item1, acceptedOldSectionMail.Item2, oldSectionAddress));
          rejectedMails.Add(new UpdateMail(rejectedOldSectionMail.Item1, rejectedOldSectionMail.Item2, oldSectionAddress));
        }

        person.Move(request.NewDn);

        var acceptedText = Texts.GetText(DataGlobal.DataAccess, new Texts.TransferAcceptedMail(), person, oldSectionName, newSectionName);
        acceptedMails.Add(new UpdateMail(acceptedText.Item1, acceptedText.Item2, memberAddress));

        var newSectionAddress = GetSectionAddress(person);

        if (newSectionAddress != null)
        {
          var acceptedNewSectionMail = Texts.GetText(DataGlobal.DataAccess, new Texts.TransferAcceptedNewSectionMail(), person, oldSectionName, newSectionName, new StringPair(Texts.SectionItem.UserMail, acceptedText.Item2));
          var rejectedNewSectionMail = Texts.GetText(DataGlobal.DataAccess, new Texts.TransferRejectedNewSectionMail(), person, oldSectionName, newSectionName, new StringPair(Texts.SectionItem.UserMail, rejectedText.Item2));
          acceptedMails.Add(new UpdateMail(acceptedNewSectionMail.Item1, acceptedNewSectionMail.Item2, newSectionAddress));
          rejectedMails.Add(new UpdateMail(rejectedNewSectionMail.Item1, rejectedNewSectionMail.Item2, newSectionAddress));
        }

        UpdateState = new MemberAdmin.UpdateState(
          UpdateOperation.Update,
          person,
          acceptedMails,
          rejectedMails,
          Resources.TransferRequestInfo);
        RedirectCommit();
      }
    }

    private MailAddress GetSectionAddress(Person person)
    {
      var section = person.GetAssociation(Connection) as Section;
      if (section != null)
      {
        var board = section.Groups(Connection).OrderBy(s => s.Ordering).FirstOrDefault();

        if (board != null &&
            board.Emails.Count > 0)
        {
          return Mailer.EncodeAddress(board.Emails.First(), section.DisplayNameGerman);
        }
        else
        {
          BasicGlobal.Mailer.SendToAdmin("Missing email address", "Section without board email address: " + section.DN);
        }
      }

      return null;
    }

    private void HandleJoin(Request request)
    {
      var person = Connection.SearchFirst<Person>(request.OldDn);

      if (person == null)
      {
        HandleError();
      }
      else
      {
        person.EmployeeType = EmployeeType.Sympathizer;
        person.EmployeeNumber = person.Id;
        person.Joining.Add(DateTime.Now);
        person.Move(request.NewDn);

        var newAssociation =
          Connection.SearchFirst<Section>(request.NewDn.Replace(Names.MembersContainerPrefix, string.Empty)) as Association ??
          Connection.SearchFirst<Organization>(Names.Party) as Association;
        var newSectionName = new StringPair(Texts.TransferItem.NewSectionNameField, newAssociation.DisplayNameGerman);

        var acceptedMail = Texts.GetText(DataGlobal.DataAccess, new Texts.MembershipRequestAcceptedEmail(), person, newSectionName);
        var rejectedMail = Texts.GetText(DataGlobal.DataAccess, new Texts.MembershipRequestRejectedMail(), person, newSectionName);
        var memberAddress = Mailer.EncodeAddress(person.Emails.First(), person.Name);
        var acceptedMails = new List<UpdateMail>();
        acceptedMails.Add(new UpdateMail(acceptedMail.Item1, acceptedMail.Item2, memberAddress));
        var rejectedMails = new List<UpdateMail>();
        rejectedMails.Add(new UpdateMail(rejectedMail.Item1, rejectedMail.Item2, memberAddress));

        var sectionAddress = GetSectionAddress(person);

        if (sectionAddress != null)
        {
          var acceptedSectionMail = Texts.GetText(DataGlobal.DataAccess, new Texts.MembershipRequestAcceptedSectionEmail(), person, new StringPair(Texts.SectionItem.UserMail, acceptedMail.Item2), newSectionName);
          var rejecteSectiondMail = Texts.GetText(DataGlobal.DataAccess, new Texts.MembershipRequestRejectedSectionMail(), person, new StringPair(Texts.SectionItem.UserMail, rejectedMail.Item2), newSectionName);
          acceptedMails.Add(new UpdateMail(acceptedSectionMail.Item1, acceptedSectionMail.Item2, sectionAddress));
          rejectedMails.Add(new UpdateMail(rejecteSectiondMail.Item1, rejecteSectiondMail.Item2, sectionAddress));
        }

        UpdateState = new MemberAdmin.UpdateState(
          UpdateOperation.Update,
          person,
          acceptedMails,
          rejectedMails,
          Resources.JoinRequestInfo);
        RedirectCommit();
      }
    }
  }
}