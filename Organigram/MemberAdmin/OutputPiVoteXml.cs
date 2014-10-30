using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Xml.Linq;
using Pirate.Ldap;
using Pirate.Ldap.Web;
using Pirate.Util.Logging;

namespace MemberAdmin
{
  /// <summary>
  /// Data output handler for the Pi-Vote CaGui compatible member list in the XML format.
  /// </summary>
  public class OutputPiVoteXml : IHttpHandler, IRequiresSessionState
  {
    private const string DocumentTag = "Document";
    private const string UserTag = "User";
    private const string DateTag = "Date";
    private const string PersonsTag = "Persons";
    private const string PersonTag = "Person";
    private const string UniqueIdentiferTag = "UniqueIdentifer";
    private const string SurnameTag = "Surname";
    private const string GivennameTag = "Givenname";
    private const string EmailTag = "Email";
    private const string StatusTag = "Status";
    private const string StatusValueMember = "Member";
    private const string StatusValueNonMember = "NonMember";
    private const string GroupIdTag = "GroupId";
    private const string VotingRightUntilTag = "VotingRightUntil";

    public bool IsReusable
    {
      get { return false; }
    }

    private XElement Create(string tag, string value)
    {
      var element = new XElement(tag);
      element.Value = value;
      return element;
    }

    private XElement Create(string tag)
    {
      var element = new XElement(tag);
      return element;
    }

    public void ProcessRequest(HttpContext context)
    {
      var access = Ldap.Connect(context.Application, context.Session, context.Request);

      var document = new XDocument();

      if (access != null)
      {
        var currentUser = access.Connection.SearchFirst<Person>(Ldap.GetUserDn(context.Application, context.Session, context.Request), LdapScope.Base);
        var builder = new StringBuilder();

        try
        {
          builder.AppendLine("PiVote Export by user id {0} name {1}", currentUser.Id, currentUser.Name);

          access.Cache.Load<Organization>(Names.Party);
          access.Cache.Load<Section>(Names.Party, LdapScope.Sub);

          var documentElement = Create(DocumentTag);
          document.Add(documentElement);
          documentElement.Add(Create(UserTag, currentUser.Nickname));
          documentElement.Add(Create(DateTag, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
          var personsElement = Create(PersonsTag);
          documentElement.Add(personsElement);

          var filter = (Attributes.Person.EmployeeType == (int)EmployeeType.Sympathizer) |
                       (Attributes.Person.EmployeeType == (int)EmployeeType.Pirate) |
                       (Attributes.Person.EmployeeType == (int)EmployeeType.Veteran) |
                       (Attributes.Person.EmployeeType == (int)EmployeeType.WalkedThePlank);
          var persons = access.Connection.Search<Person>(Names.Party, LdapScope.Sub, filter);
          int counter = 0;

          foreach (var person in persons.OrderBy(p => p.Id))
          {
            var personElement = Create(PersonTag);
            personElement.Add(Create(UniqueIdentiferTag, person.Id.ToString()));
            personElement.Add(Create(SurnameTag, person.Surname));
            personElement.Add(Create(GivennameTag, person.Givenname));

            foreach (var email in person.Emails)
            {
              personElement.Add(Create(EmailTag, email));
            }

            switch (person.EmployeeType)
            {
              case EmployeeType.Pirate:
              case EmployeeType.Sympathizer:
                personElement.Add(Create(StatusTag, StatusValueMember));
                break;
              case EmployeeType.Veteran:
              case EmployeeType.WalkedThePlank:
                personElement.Add(Create(StatusTag, StatusValueNonMember));
                break;
              default:
                throw new InvalidOperationException("Unknown status.");
            }

            personElement.Add(Create(VotingRightUntilTag, person.VotingRightUntil.ToString("yyyy-MM-dd HH:mm:ss")));

            var association = person.GetAssociation(access.Cache);

            while (association != null)
            {
              if (association.PiVoteGroupId.HasValue)
              {
                personElement.Add(Create(GroupIdTag, association.PiVoteGroupId.Value.ToString()));
              }

              association = association.GetParentAssociation(access.Cache);
            }

            personsElement.Add(personElement);
            counter++;
          }

          builder.AppendLine("Results: " + persons.Count());
        }
        catch (Exception exception)
        {
          builder.AppendLine("Error: " + exception.ToString());
        }
        finally
        {
          BasicGlobal.Logger.Log(LogLevel.Info, builder.ToString());
        }
      }
      else
      {
        document.Add(Create(DocumentTag, "Access denied."));
      }

      context.Response.Clear();
      context.Response.ContentEncoding = Encoding.UTF8;
      context.Response.ContentType = "text/xml";
      context.Response.Write(document.ToString());
      context.Response.Flush();
    }
  }
}
