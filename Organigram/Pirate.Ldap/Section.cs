using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Drawing;

namespace Pirate.Ldap
{
  /// <summary>
  /// Cantonal Section LDAP object.
  /// </summary>
  public class Section : Association
  {
    /// <summary>
    /// Gets the state. This is the abbriviation
    /// for the state.
    /// </summary>
    /// <example>
    /// ZH for Zurich, BE for Bern, etc.
    /// </example>
    public string State { get; private set; }

    /// <summary>
    /// Gets or sets the ordering. This determines the
    /// order in which the role appears on the Organigram.
    /// The value must be greater or equal to 0 for the
    /// role to appear in the Organigram.
    /// </summary>
    public override int Ordering { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Section"/> class.
    /// Reads the attributes from the entry and fills this object's fields.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="entry">The entry.</param>
    public Section(LdapEntry entry)
      : base(entry)
    {
    }

    public IEnumerable<Container> Containers(LdapConnection connection)
    {
      var result = connection.Search(DN, 2, "(objectclass=ppscontainer)", null, false);

      while (result.hasMore())
      {
        yield return new Container(result.next());
      }
    }

    public IEnumerable<Group> Groups(LdapConnection connection)
    {
      var result = connection.Search(DN, 1, "(objectclass=ppsgroup)", new string[] { "*" }, false);

      while (result.hasMore())
      {
        yield return new Group(result.next());
      }
    }
    
    public override string ToString()
    {
      return string.Format("Sektion {0}", State.ToUpper());
    }

    protected override IEnumerable<Field> CreateFields()
    {
      yield return new LdapStringField(Attributes.Section.State, () => State, x => State = x);
      yield return new LdapStringField(Attributes.Section.DisplayNameGerman, () => DisplayNameGerman, x => DisplayNameGerman = x);
      yield return new LdapIntField(Attributes.Section.Ordering, () => Ordering, x => Ordering = x ?? -1);
      yield return new LdapStringField(Attributes.Section.Website, () => Website, x => Website = x);
      yield return new LdapImageField(Attributes.Section.JpegPhoto, () => Photo, x => Photo = x);
      yield return new LdapIntField(Attributes.Section.PiVoteGroupId, () => PiVoteGroupId, x => PiVoteGroupId = x);
    }

    public override LdapObject Reload(LdapConnection connection)
    {
      return connection.SearchFirst<Section>(OldDN);
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
      get { return Attributes.Section.ObjectClass; }
    }

    public override Association GetParentAssociation(LdapConnection connection)
    {
      var parentDn = Ldap.DN.GetParentDn(DN);

      if (parentDn == Names.Party)
      {
        return connection.SearchFirst<Organization>(parentDn);
      }
      else
      {
        return connection.SearchFirst<Section>(parentDn);
      }
    }

    public override Association GetParentAssociation(LdapCache cache)
    {
      var parentDn = Ldap.DN.GetParentDn(DN);

      if (parentDn == Names.Party)
      {
        return cache.Get<Organization>(parentDn);
      }
      else
      {
        return cache.Get<Section>(parentDn);
      }
    }
  }
}
