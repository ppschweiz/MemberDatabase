using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public static class Attributes
  {
    public static class Values
    {
      public const string Unspecified = "$$UNSPECIFIED$$";
    }

    public static class Organization
    {
      public const string ObjectClass = "organization";
      public static readonly LdapAttributeBase DisplayNameGerman = new LdapAttributeBase("ppsDisplayNameGerman", () => LdapResources.DisplayName);
      public static readonly LdapAttributeBase Website = new LdapAttributeBase("ppsWebsite", () => LdapResources.Website);
      public static readonly LdapAttributeBase JpegPhoto = new LdapAttributeBase("jpegPhoto", () => LdapResources.Photo);
      public static readonly LdapAttributeBase PiVoteGroupId = new LdapAttributeBase("ppsPiVoteGroupId", () => LdapResources.PiVoteGroupId);
    }

    public static class Person
    {
      public const string ObjectClass = "ppsperson";
      public static readonly LdapAttributeBase DN = new LdapAttributeBase("dn", () => LdapResources.DN);
      public static readonly LdapAttributeBase CommonName = new LdapAttributeBase("cn", () => LdapResources.CommonName);
      public static readonly LdapAttributeBase UniqueIdentifier = new LdapAttributeBase("uniqueIdentifier", () => LdapResources.UniqueIdentifier);
      public static readonly LdapAttributeBase JpegPhoto = new LdapAttributeBase("jpegPhoto", () => LdapResources.Photo);
      public static readonly LdapAttributeBase Mail = new LdapAttributeBase("mail", () => LdapResources.Email);
      public static readonly LdapAttributeBase Surname = new LdapAttributeBase("sn", () => LdapResources.Surname);
      public static readonly LdapAttributeBase Givenname = new LdapAttributeBase("givenname", () => LdapResources.Givenname);
      public static readonly LdapAttributeBase State = new LdapAttributeBase("st", () => LdapResources.State);
      public static readonly LdapAttributeBase PreferredLanguage = new LdapAttributeBase("preferredLanguage", () => LdapResources.PreferredLanguage);
      public static readonly LdapAttributeBase Country = new LdapAttributeBase("c", () => LdapResources.Country);
      public static readonly LdapAttributeBase Street = new LdapAttributeBase("street", () => LdapResources.Street);
      public static readonly LdapAttributeBase PostalCode = new LdapAttributeBase("postalCode", () => LdapResources.PostalCode);
      public static readonly LdapAttributeBase Info = new LdapAttributeBase("info", () => LdapResources.Info);
      public static readonly LdapAttributeBase Location = new LdapAttributeBase("l", () => LdapResources.Location);
      public static readonly LdapAttributeBase UserId = new LdapAttributeBase("uid", () => LdapResources.Username);
      public static readonly LdapAttributeBase BirthDate = new LdapAttributeBase("ppsBirthDate", () => LdapResources.Birthdate);
      public static readonly LdapAttributeBase Joining = new LdapAttributeBase("ppsJoining", () => LdapResources.Joining);
      public static readonly LdapAttributeBase Leaving = new LdapAttributeBase("ppsLeaving", () => LdapResources.Leaving);
      public static readonly LdapAttributeBase Gender = new LdapAttributeBase("ppsGender", () => LdapResources.Gender);
      public static readonly LdapAttributeBase PreferredNotificationMethod = new LdapAttributeBase("ppsPreferredNotificationMethod", () => LdapResources.PreferredNotificationMethod);
      public static readonly LdapAttributeBase EmployeeType = new LdapAttributeBase("employeeType", () => LdapResources.EmployeeType);
      public static readonly LdapAttributeBase EmployeeNumber = new LdapAttributeBase("employeeNumber", () => LdapResources.EmployeeNumber);
      public static readonly LdapAttributeBase NewsLetterSelect = new LdapAttributeBase("ppsNewsLetterSelect", () => LdapResources.NewsletterSelect);
      public static readonly LdapAttributeBase LastNewsLetter = new LdapAttributeBase("ppsLastNewsLetter", () => LdapResources.LastNewsLetter);
      public static readonly LdapAttributeBase TelephoneNumber = new LdapAttributeBase("telephoneNumber", () => LdapResources.Phone);
      public static readonly LdapAttributeBase Mobile = new LdapAttributeBase("mobile", () => LdapResources.Mobile);
      public static readonly LdapAttributeBase Description = new LdapAttributeBase("description", () => LdapResources.Description);
      public static readonly LdapAttributeBase VotingRightUntil = new LdapAttributeBase("ppsVotingRightUntil", () => LdapResources.VotingRightUntil);
      public static readonly LdapAttributeBase AlternateMail = new LdapAttributeBase("ppsAlternateMail", () => LdapResources.AlternateMail);
      public static readonly LdapAttributeBase Password = new LdapAttributeBase("userPassword", () => LdapResources.Password);

      public static class Computed
      {
        public static readonly LdapAttributeBase Section = new LdapAttributeBase("section-computed", () => LdapResources.Section);
      }
  }

    public static class Group
    {
      public const string ObjectClass = "ppsgroup";
      public static readonly LdapAttributeBase Mail = new LdapAttributeBase("mail", () => LdapResources.Email);
      public static readonly LdapAttributeBase CommonName = new LdapAttributeBase("cn", () => LdapResources.CommonName);
      public static readonly LdapAttributeBase Member = new LdapAttributeBase("member", () => LdapResources.Member);
      public static readonly LdapAttributeBase DisplayNameGerman = new LdapAttributeBase("ppsDisplayNameGerman", () => LdapResources.DisplayName);
      public static readonly LdapAttributeBase Ordering = new LdapAttributeBase("ppsOrdering", () => LdapResources.Ordering);
      public static readonly LdapAttributeBase Website = new LdapAttributeBase("ppsWebsite", () => LdapResources.Website);
    }

    public static class Role
    {
      public const string ObjectClass = "ppsrole";
      public static readonly LdapAttributeBase CommonName = new LdapAttributeBase("cn", () => LdapResources.CommonName);
      public static readonly LdapAttributeBase RoleOccupant = new LdapAttributeBase("roleOccupant", () => LdapResources.Occupant);
      public static readonly LdapAttributeBase DisplayNameGerman = new LdapAttributeBase("ppsDisplayNameGerman", () => LdapResources.DisplayName);
      public static readonly LdapAttributeBase Ordering = new LdapAttributeBase("ppsOrdering", () => LdapResources.Ordering);
      public static readonly LdapAttributeBase JpegPhoto = new LdapAttributeBase("jpegPhoto", () => LdapResources.Phone);
    }

    public static class Section
    {
      public const string ObjectClass = "ppssection";
      public static readonly LdapAttributeBase State = new LdapAttributeBase("st", () => LdapResources.State);
      public static readonly LdapAttributeBase DisplayNameGerman = new LdapAttributeBase("ppsDisplayNameGerman", () => LdapResources.DisplayName);
      public static readonly LdapAttributeBase Ordering = new LdapAttributeBase("ppsOrdering", () => LdapResources.Ordering);
      public static readonly LdapAttributeBase Website = new LdapAttributeBase("ppsWebsite", () => LdapResources.Website);
      public static readonly LdapAttributeBase JpegPhoto = new LdapAttributeBase("jpegPhoto", () => LdapResources.Photo);
      public static readonly LdapAttributeBase PiVoteGroupId = new LdapAttributeBase("ppsPiVoteGroupId", () => LdapResources.PiVoteGroupId);
    }

    public static class Container
    {
      public const string ObjectClass = "ppscontainer";
      public static readonly LdapAttributeBase DomainComponent = new LdapAttributeBase("dc", () => LdapResources.DomainComponent);
    }

    public static class UniqueStorage
    {
      public const string ObjectClass = "ppsuniquestorage";
      public static readonly LdapAttributeBase UidNumber = new LdapAttributeBase("uidNumber", () => LdapResources.UidNumber);
    }

    public static readonly LdapAttributeBase ObjectClass = new LdapAttributeBase("objectClass", () => LdapResources.ObjectClass);
  }
}
