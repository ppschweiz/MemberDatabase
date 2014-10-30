using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Drawing;

namespace Pirate.Ldap
{
  /// <summary>
  /// Organization LDAP object.
  /// </summary>
  /// <example>
  /// This is only used for the PPS.
  /// </example>
  public class Organization : Association
  {
    /// <summary>
    /// Gets or sets the name. This is the LDAP
    /// name (cn) and should not be printed.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Organization"/> class.
    /// Reads the attributes from the entry and fills this object's fields.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="entry">The entry.</param>
    public Organization(LdapEntry entry)
      : base(entry)
    {
      Name = "Piratenpartei Schweiz";
    }

    /// <summary>
    /// Gets the containers from the LDAP server.
    /// </summary>
    public IEnumerable<Container> Containers(LdapConnection connection)
    {
      var result = connection.Search(DN, 2, "(objectclass=ppscontainer)", null, false);

      while (result.hasMore())
      {
        yield return new Container(result.next());
      }
    }

    /// <summary>
    /// Gets the sections from the LDAP server.
    /// </summary>
    public IEnumerable<Section> Sections(LdapConnection connection)
    {
      var result = connection.Search(DN, 2, "(objectclass=ppssection)", null, false);

      while (result.hasMore())
      {
        yield return new Section(result.next());
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
      return string.Format("Organization {0}", Name);
    }

    protected override IEnumerable<Field> CreateFields()
    {
      yield return new LdapStringField(Attributes.Organization.DisplayNameGerman, () => DisplayNameGerman, x => DisplayNameGerman = x);
      yield return new LdapStringField(Attributes.Organization.Website, () => Website, x => Website = x);
      yield return new LdapImageField(Attributes.Organization.JpegPhoto, () => Photo, x => Photo = x);
      yield return new LdapIntField(Attributes.Organization.PiVoteGroupId, () => PiVoteGroupId, x => PiVoteGroupId = x);
    }

    public override LdapObject Reload(LdapConnection connection)
    {
      return connection.SearchFirst<Organization>(OldDN);
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
      get { return Attributes.Organization.ObjectClass; }
    }

    public override Association GetParentAssociation(LdapConnection connection)
    {
      return null;
    }

    public override Association GetParentAssociation(LdapCache cache)
    {
      return null;
    }

    public override int Ordering
    {
      get
      {
        return 0;
      }
      protected set
      {
      }
    }
  }
}
