using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  /// <summary>
  /// Person LDAP object.
  /// </summary>
  /// <example>
  /// Pirate, Sympathizer, Artefical Person, etc.
  /// </example>
  public class Person : LdapObject
  {
    /// <summary>
    /// Gets or sets the name. This is the common name.
    /// It should be equals to the givenname plus the
    /// surname. It should shorten second and third 
    /// givennames to initials.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the nickname. This is also
    /// the username (uid) used for login.
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// Gets or sets the surname.
    /// </summary>
    public string Surname { get; set; }

    /// <summary>
    /// Gets or sets the givenname.
    /// </summary>
    public string Givenname { get; set; }

    /// <summary>
    /// Gets the state. This is the abbriviation
    /// for the state.
    /// </summary>
    /// <example>
    /// ZH for Zurich, BE for Bern, etc.
    /// </example>
    public string State { get; set; }

    /// <summary>
    /// Gets or sets the country. This is the two
    /// letter abbrivation for the country.
    /// </summary>
    /// <example>
    /// CH for Switzerland, DE for Germany.
    /// </example>
    public string Country { get; set; }

    /// <summary>
    /// Gets or sets the street.
    /// </summary>
    public string Street { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the info.
    /// </summary>
    public string Info { get; set; }

    /// <summary>
    /// Gets or sets the emails. There may be none,
    /// one or more email addresses.
    /// </summary>
    public List<string> Emails { get; set; }

    /// <summary>
    /// Gets or sets the alternate emails. There may
    /// be none, one or more email addresses.
    /// </summary>
    public List<string> AlternateEmails { get; set; }

    /// <summary>
    /// Gets or sets the photo.
    /// </summary>
    public Image Photo { get; set; }

    /// <summary>
    /// Gets or sets the id. This is the unique identifer
    /// used to identify the person.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the birth date. It may be unknown (null).
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gets or sets the joining. This is the date,
    /// on which said person joined the party. Multiple
    /// values indicate rejoining.
    /// </summary>
    public List<DateTime> Joining { get; set; }

    /// <summary>
    /// Gets or sets the leaving. This is the date,
    /// on which said person left the party. Multiple
    /// values indicate rejoing and the leaving again.
    /// </summary>
    public List<DateTime> Leaving { get; set; }

    /// <summary>
    /// Gets or sets the type/status of that person with
    /// respect to the party.
    /// </summary>
    public EmployeeType EmployeeType { get; set; }

    /// <summary>
    /// Gets or sets the employee number. This is the member
    /// number. It is set to the Id (uniqueIdentifer) for
    /// members and former members and to null for everyone
    /// else.
    /// </summary>
    public int? EmployeeNumber { get; set; }

    /// <summary>
    /// Gets or sets the gender. It can be null for non-members.
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Gets or sets the preferred notification method. It can
    /// be null for non-members.
    /// </summary>
    public PreferredNotificationMethod? PreferredNotificationMethod { get; set; }

    /// <summary>
    /// Gets or sets the preferred language. It is the two
    /// letter abbriviation of the language.
    /// </summary>
    /// <example>
    /// de for German, fr for French, it for Italien,
    /// en for English
    /// </example>
    public string PreferredLanguage { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the phone.
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Gets or sets the mobile.
    /// </summary>
    public string Mobile { get; set; }

    /// <summary>
    /// Gets or sets the voting right until.
    /// </summary>
    /// <value>
    /// The voting right until.
    /// </value>
    public DateTime VotingRightUntil { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Person"/> class.
    /// Also creates the object on the LDAP server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="parentDn">The parent dn.</param>
    /// <param name="unqiueIdentifer">The unqiue identifer.</param>
    /// <param name="nickname">The nickname.</param>
    public Person(string parentDn, int uniqueIdentifer, string nickname)
      : base(parentDn, Attributes.Person.UniqueIdentifier, uniqueIdentifer)
    {
      Nickname = nickname;
      Id = uniqueIdentifer;
      EmployeeType = Ldap.EmployeeType.LandLubber;
      Joining = new List<DateTime>();
      Leaving = new List<DateTime>();
      Emails = new List<string>();
      AlternateEmails = new List<string>();
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Person"/> class.
    /// Reads the attributes from the entry and fills this object's fields.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="entry">The entry.</param>
    public Person(LdapEntry entry)
      : base(entry)
    {
    }

    /// <summary>
    /// Moves this Person to the People (non-member) container.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public void MoveToPeople()
    {
      Move(Names.People);
    }

    /// <summary>
    /// Moves this Person to the Members (PPS) container.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public void MoveToMembers()
    {
      Move(Names.Members);
    }

    /// <summary>
    /// Moves this Person to the given Cantonal Section.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="section">The section.</param>
    public void MoveToSection(Section section)
    {
      string parentDn = string.Format("dc=members,{0}", section.DN);
      Move(parentDn);
    }

    protected override void MoveInternal(LdapConnection connection, string oldDn, string newDn)
    {
      var addedToGroups = new List<string>();
      var addedToRole = new List<string>();
      var newRdn = Ldap.DN.GetRdn(newDn);
      var newParentDn = Ldap.DN.GetParentDn(newDn);

      try
      {
        foreach (var group in connection.Search<Group>(Names.Party, LdapScope.Sub))
        {
          if (group.MemberDns.Contains(oldDn))
          {
            group.MemberDns.Add(newDn);
            group.Modify(connection);
            addedToGroups.Add(group.DN);
          }
        }

        foreach (var role in connection.Search<Role>(Names.Party, LdapScope.Sub))
        {
          if (role.OccupantDns.Contains(oldDn))
          {
            role.OccupantDns.Add(newDn);
            role.Modify(connection);
            addedToRole.Add(role.DN);
          }
        }

        connection.Rename(oldDn, newRdn, newParentDn, true);
      }
      catch (Exception)
      {
        foreach (var groupDn in addedToGroups)
        {
          var group = connection.SearchFirst<Group>(groupDn);
          group.MemberDns.Remove(newDn);
          group.Modify(connection);
        }

        foreach (var roleDn in addedToRole)
        {
          var role = connection.SearchFirst<Role>(roleDn);
          role.OccupantDns.Remove(newDn);
          role.Modify(connection);
        }

        throw;
      }

      var errors = new List<Exception>();

      foreach (var group in connection.Search<Group>(Names.Party, LdapScope.Sub))
      {
        if (group.MemberDns.Contains(oldDn))
        {
          try
          {
            group.MemberDns.Remove(oldDn);
            group.Modify(connection);
          }
          catch (Exception exception)
          {
            errors.Add(exception);
          }
        }
      }

      foreach (var role in connection.Search<Role>(Names.Party, LdapScope.Sub))
      {
        if (role.OccupantDns.Contains(oldDn))
        {
          try
          {
            role.OccupantDns.Remove(oldDn);
            role.Modify(connection);
          }
          catch (Exception exception)
          {
            errors.Add(exception);
          }
        }
      }

      if (errors.Count > 0)
      {
        throw errors.First();
      }
    }

    /// <summary>
    /// Deletes the object on the LDAP server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public override void Delete(LdapConnection connection)
    {
      foreach (var group in connection.Search<Group>(Names.Party, LdapScope.Sub))
      {
        if (group.MemberDns.Contains(OldDN))
        {
          group.MemberDns.Remove(OldDN);
          group.Modify(connection);
        }
      }

      foreach (var role in connection.Search<Role>(Names.Party, LdapScope.Sub))
      {
        if (role.OccupantDns.Contains(OldDN))
        {
          role.OccupantDns.Add(OldDN);
          role.Modify(connection);
        }
      }

      connection.Delete(OldDN);
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return string.Format("Person {0}", Name);
    }

    protected override IEnumerable<Field> CreateFields()
    {
      yield return new ComputedField(Attributes.Person.DN, true, false, () => DN);
      yield return new LdapIntField(Attributes.Person.UniqueIdentifier, () => Id, x => Id = (int)x, true, true, true);
      yield return new LdapStringField(Attributes.Person.UserId, () => Nickname, x => Nickname = x, false, true, true);
      yield return new LdapStringField(Attributes.Person.CommonName, () => Name, x => Name = x, false, true, false);
      yield return new LdapStringField(Attributes.Person.Surname, () => Surname, x => Surname = x, false, true, true);
      yield return new LdapStringField(Attributes.Person.Givenname, () => Givenname, x => Givenname = x, false, true, true);
      yield return new LdapStringListField(Attributes.Person.Mail, () => Emails, x => Emails = x.ToList(), false, true, true);
      yield return new LdapStringListField(Attributes.Person.AlternateMail, () => AlternateEmails, x => AlternateEmails = x.ToList(), false, false);
      yield return new LdapImageField(Attributes.Person.JpegPhoto, () => Photo, x => Photo = x);
      yield return new LdapStringField(Attributes.Person.State, () => State, x => State = x);
      yield return new LdapStringField(Attributes.Person.Country, () => Country, x => Country = x);
      yield return new LdapStringField(Attributes.Person.Street, () => Street, x => Street = x);
      yield return new LdapStringField(Attributes.Person.PostalCode, () => PostalCode, x => PostalCode = x);
      yield return new LdapStringField(Attributes.Person.Info, () => Info, x => Info = x);
      yield return new LdapStringField(Attributes.Person.Location, () => Location, x => Location = x);
      yield return new LdapDataTimeField(Attributes.Person.BirthDate, () => BirthDate, x => BirthDate = x);
      yield return new LdapDateTimeListField(Attributes.Person.Joining, () => Joining, x => Joining = x.ToList());
      yield return new LdapDateTimeListField(Attributes.Person.Leaving, () => Leaving, x => Leaving = x.ToList());
      yield return new LdapEnumField<EmployeeType>(Attributes.Person.EmployeeType, () => (int?)EmployeeType, x => EmployeeType = (EmployeeType)x, EmployeeType.LandLubber, x => x.Text());
      yield return new LdapEnumField<PreferredNotificationMethod>(Attributes.Person.PreferredNotificationMethod, () => (int?)PreferredNotificationMethod, x => PreferredNotificationMethod = (PreferredNotificationMethod?)x, Pirate.Ldap.PreferredNotificationMethod.Unknown, x => x.Text());
      yield return new LdapEnumField<Gender>(Attributes.Person.Gender, () => (int?)Gender, x => Gender = (Gender?)x, Pirate.Ldap.Gender.Unknown, x => x.Text());
      yield return new LdapIntField(Attributes.Person.EmployeeNumber, () => EmployeeNumber, x => EmployeeNumber = x);
      yield return new LdapStringField(Attributes.Person.PreferredLanguage, () => PreferredLanguage, x => PreferredLanguage = x);
      yield return new LdapStringField(Attributes.Person.Description, () => Description, x => Description = x);
      yield return new LdapStringField(Attributes.Person.Mobile, () => Mobile, x => Mobile = x);
      yield return new LdapStringField(Attributes.Person.TelephoneNumber, () => Phone, x => Phone = x);
      yield return new LdapDataTimeField(Attributes.Person.VotingRightUntil, () => VotingRightUntil, x => VotingRightUntil = x ?? new DateTime(1970, 1, 1), false, true, true);
      yield return new EvaulateField(Attributes.Person.Computed.Section, false, true, c => MemberOfText(c));
    }

    public override LdapObject Reload(LdapConnection connection)
    {
      return connection.SearchFirst<Person>(OldDN);
    }

    public Association GetAssociation(LdapConnection connection)
    {
      if (DN.EndsWith(Names.People))
      {
        return null;
      }
      else if (DN.EndsWith(Names.Members))
      {
        return connection.SearchFirst<Organization>(Names.Party);
      }
      else if (DN.Contains(",dc=members,"))
      {
        var sectionDn = Ldap.DN.GetParentDn(Ldap.DN.GetParentDn(DN));
        return connection.SearchFirst<Section>(sectionDn);
      }
      else
      {
        return null;
      }
    }

    public Association GetAssociation(LdapCache connection)
    {
      if (DN.EndsWith(Names.People))
      {
        return null;
      }
      else if (DN.EndsWith(Names.Members))
      {
        return connection.Get<Organization>(Names.Party);
      }
      else if (DN.Contains(",dc=members,"))
      {
        var sectionDn = Ldap.DN.GetParentDn(Ldap.DN.GetParentDn(DN));
        return connection.Get<Section>(sectionDn);
      }
      else
      {
        return null;
      }
    }

    public override string ObjectClass
    {
      get { return Attributes.Person.ObjectClass; }
    }

    public static Person Prototype
    {
      get
      {
        return new Person(Names.Party, 999999, "Prototype");
      }
    }

    public string MemberOfText(LdapConnection connection)
    {
      var memberOf = GetAssociation(connection);

      return memberOf != null ? memberOf.DisplayNameGerman : LdapResources.NonMember;
    }

    public bool IsMemberType
    {
      get
      {
        return EmployeeType == EmployeeType.Sympathizer || EmployeeType == EmployeeType.Pirate;
      }
    }

    public bool IsFormerMemberType
    {
      get
      {
        return EmployeeType == EmployeeType.Veteran || EmployeeType == EmployeeType.WalkedThePlank;
      }
    }

    public bool IsNonMemberType
    {
      get
      {
        return !IsMemberType;
      }
    }

    public bool IsPeopleDN
    {
      get
      {
        return DN.EndsWith(Names.People);
      }
    }

    public bool IsMemberDn
    {
      get
      {
        return !IsPeopleDN;
      }
    }

    public Right ComputeRight(LdapConnection connection, LdapObject obj)
    {
      var positions = ComputePositions(connection);

      var relevantPositions = positions
        .Where(p => obj.DN.EndsWith(p.Association.DN))
        .ToList();

      var rolesRight = relevantPositions
        .Where(p => p.Role != null)
        .Aggregate(Right.None, (r, p) => r | GrantedRight(p.Role));

      var groupsRight = relevantPositions
        .Where(p => p.Group != null)
        .Aggregate(Right.None, (r, p) => r | GrantedRight(p.Group));

      return rolesRight | groupsRight;
    }

    public Right GrantedRight(Group group)
    {
      if (group.DN.ToLowerInvariant().StartsWith("cn=diradmin,".ToLowerInvariant()))
      {
        return Right.Manage;
      }
      else if (group.DN.ToLowerInvariant().StartsWith("cn=Board,".ToLowerInvariant()))
      {
        return Right.ReadWrite;
      }
      else if (group.DN.ToLowerInvariant().StartsWith("cn=GPK,".ToLowerInvariant()))
      {
        return Right.Read;
      }
      else
      {
        return Right.None;
      }
    }

    public Right GrantedRight(Role role)
    {
      if (role.DN.ToLowerInvariant().StartsWith("cn=Actuary,".ToLowerInvariant()))
      {
        return Right.Manage;
      }
      else
      {
        return Right.None;
      }
    }

    private string Order(Position position)
    {
      var orders = new List<string>();

      if (position.Association == null)
      {
        orders.Add("999999");
      }
      else if (position.Association.Ordering >= 0)
      {
        orders.Add(position.Association.Ordering.ToString().PadLeft(6, '0'));
      }

      if (position.Group == null)
      {
        orders.Add("999999");
      }
      else if (position.Group.Ordering >= 0)
      {
        if (position.Group.DN.ToLowerInvariant().Contains("workgroup"))
        {
          orders.Add((position.Group.Ordering + 900000).ToString().PadLeft(6, '0'));
        }
        else
        {
          orders.Add(position.Group.Ordering.ToString().PadLeft(6, '0'));
        }
      }

      if (position.Role == null)
      {
        orders.Add("999999");
      }
      else if (position.Role.Ordering >= 0)
      {
        orders.Add(position.Role.Ordering.ToString().PadLeft(6, '0'));
      }

      return string.Join("-", orders.ToArray());
    }

    public IEnumerable<Position> ComputeVisiblePositions(LdapConnection connection)
    {
      foreach (var position in ComputePositions(connection).OrderBy(p => Order(p)))
      {
        if (position.Role != null && position.Role.Ordering > 0)
        {
          yield return position;
        }
        else if (position.Group != null && position.Group.Ordering > 0)
        {
          yield return position;
        }
      }
    }

    public string SectionDnState
    {
      get
      {
        var match = Regex.Match(DN, "^.+,st=([A-Za-z]+),.+$");

        if (match.Success)
        {
          return match.Groups[1].Value;
        }
        else
        {
          return "none";
        }
      }
    }

    public string SectionDnLocation
    {
      get
      {
        var match = Regex.Match(DN, "^.+,l=([A-Za-z]+),.+$");

        if (match.Success)
        {
          return match.Groups[1].Value;
        }
        else
        {
          return "none";
        }
      }
    }

    public IEnumerable<Position> ComputePositions(LdapConnection connection)
    {
      var party = connection.SearchFirst<Organization>(Names.Party);
      var groups = connection.Search<Group>(Names.Party, LdapScope.Sub);
      var roles = connection.Search<Role>(Names.Party, LdapScope.Sub);
      var sections = connection.Search<Section>(Names.Party, LdapScope.Sub);

      foreach (var group in groups.Where(g => g.MemberDns.Contains(DN)))
      {
        var role = roles
          .Where(r => r.OccupantDns.Contains(DN))
          .Where(r => r.DN.EndsWith(group.DN))
          .OrderByDescending(r => r.DN.Length)
          .FirstOrDefault();
        var section = sections
          .Where(s => group.DN.EndsWith(s.DN))
          .OrderByDescending(s => s.DN.Length)
          .FirstOrDefault();

        if (section == null)
        {
          yield return new Position(party, group, role);
        }
        else
        {
          yield return new Position(section, group, role);
        }
      }
    }
  }
}
