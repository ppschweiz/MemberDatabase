using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  /// <summary>
  /// Role LDAP object.
  /// </summary>
  /// <example>
  /// President, vice-president, treasurer, etc.
  /// </example>
  public class Role : LdapObject
  {
    /// <summary>
    /// Gets or sets the distinguished names of the occupants.
    /// </summary>
    /// <value>
    /// The occupant DNS.
    /// </value>
    public List<string> OccupantDns { get; set; }

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
    /// <value>
    /// The ordering.
    /// </value>
    public int Ordering { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Role"/> class.
    /// Reads the attributes from the entry and fills this object's fields.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="entry">The entry.</param>
    public Role(LdapEntry entry)
      : base(entry)
    {
    }

    /// <summary>
    /// Gets the occupants from the LDAP server.
    /// </summary>
    public IEnumerable<Person> Occupants(LdapConnection connection)
    {
      return connection.Search<Person>(OccupantDns);
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return string.Format("Role {0}", Name);
    }

    protected override IEnumerable<Field> CreateFields()
    {
      yield return new LdapStringField(Attributes.Role.CommonName, () => Name, x => Name = x);
      yield return new LdapStringListField(Attributes.Role.RoleOccupant, () => OccupantDns, x => OccupantDns = x.ToList());
      yield return new LdapStringField(Attributes.Role.DisplayNameGerman, () => DisplayNameGerman, x => DisplayNameGerman = x);
      yield return new LdapIntField(Attributes.Role.Ordering, () => Ordering, x => Ordering = x ?? -1);
    }

    public override LdapObject Reload(LdapConnection connection)
    {
      return connection.SearchFirst<Role>(OldDN);
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
      get { return Attributes.Role.ObjectClass; }
    }
  }
}
