using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  /// <summary>
  /// Group LDAP object.
  /// </summary>
  /// <example>
  /// Board, Comittee, Workgroup, etc.
  /// </example>
  public class Group : LdapObject
  {
    /// <summary>
    /// Gets or sets the distingished names of the members.
    /// </summary>
    public List<string> MemberDns { get; set; }

    /// <summary>
    /// Gets or sets the name. This is the LDAP
    /// name (cn) and should not be printed.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the display name german. This
    /// is a plain text german name for printing.
    /// </summary>    
    public string DisplayNameGerman { get; set; }

    /// <summary>
    /// Gets or sets the ordering. This determines the
    /// order in which the role appears on the Organigram.
    /// The value must be greater or equal to 0 for the
    /// role to appear in the Organigram.
    /// </summary>
    public int Ordering { get; set; }

    /// <summary>
    /// Gets the website. This is the url for the website.
    /// </summary>
    public string Website { get; set; }

    /// <summary>
    /// Gets or sets the emails of that group. Their members
    /// may send emails from these addresses.
    /// </summary>
    public List<string> Emails { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Group"/> class.
    /// Reads the attributes from the entry and fills this object's fields.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="entry">The entry.</param>
    public Group(LdapEntry entry)
      : base(entry)
    {
    }

    /// <summary>
    /// Gets the members from the LDAP server.
    /// </summary>
    public IEnumerable<Person> Members(LdapConnection connection)
    {
      return connection.Search<Person>(MemberDns);
    }

    /// <summary>
    /// Gets the roles from the LDAP server.
    /// </summary>
    public IEnumerable<Role> Roles(LdapConnection connection)
    {
      var result = connection.Search(DN, 1, "(objectclass=ppsrole)", new string[] { "*" }, false);

      while (result.hasMore())
      {
        yield return new Role(result.next());
      }
    }

    /// <summary>
    /// Gets the groups from the LDAP server.
    /// </summary>
    public IEnumerable<Group> Groups(LdapConnection connection)
    {
      var result = connection.Search(DN, 1, "(objectclass=ppsgroup)", new string[] { "*" }, false);

      while (result.hasMore())
      {
        yield return new Group(result.next());
      }
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return string.Format("Group {0}", Name);
    }

    protected override IEnumerable<Field> CreateFields()
    {
      yield return new LdapStringField(Attributes.Group.CommonName, () => Name, x => Name = x);
      yield return new LdapStringListField(Attributes.Group.Member, () => MemberDns, x => MemberDns = x.ToList());
      yield return new LdapStringField(Attributes.Group.DisplayNameGerman, () => DisplayNameGerman, x => DisplayNameGerman = x);
      yield return new LdapIntField(Attributes.Group.Ordering, () => Ordering, x => Ordering = x ?? -1);
      yield return new LdapStringField(Attributes.Group.Website, () => Website, x => Website = x);
      yield return new LdapStringListField(Attributes.Group.Mail, () => Emails, x => Emails = x.ToList());
    }

    public override LdapObject Reload(LdapConnection connection)
    {
      return connection.SearchFirst<Group>(OldDN);
    }

    protected override void MoveInternal(LdapConnection connection, string oldDn, string newDn)
    {
      throw new NotSupportedException();
    }

    public override void Delete(LdapConnection connection)
    {
      throw new NotSupportedException();
    }

    public override string ObjectClass
    {
      get { return Attributes.Group.ObjectClass; }
    }
  }
}
