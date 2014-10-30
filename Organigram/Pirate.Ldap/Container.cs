using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  /// <summary>
  /// PPS Container LDAP object.
  /// </summary>
  public class Container : LdapObject
  {
    /// <summary>
    /// Gets the name. This is the LDAP commen name (cn).
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Container"/> class.
    /// Reads the attributes from the entry and fills this object's fields.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="entry">The entry.</param>
    public Container(LdapEntry entry)
      : base(entry)
    {
    }

    /// <summary>
    /// Gets the groups from the LDAP server.
    /// </summary>
    public IEnumerable<Group> Groups(LdapConnection connection)
    {
      return connection.Search<Group>(DN, LdapScope.One);
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return string.Format("Container {0}", Name);
    }

    protected override IEnumerable<Field> CreateFields()
    {
      yield return new LdapStringField(Attributes.Container.DomainComponent, () => Name, x => Name = x);
    }

    public override LdapObject Reload(LdapConnection connection)
    {
      return connection.SearchFirst<Container>(OldDN);
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
      get { return Attributes.Container.ObjectClass; }
    }
  }
}
