using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Drawing;

namespace Pirate.Ldap
{
  public abstract class Association : LdapObject
  {
    /// <summary>
    /// Gets or sets the display name german. This
    /// is a plain text german name for printing.
    /// </summary>    
    public string DisplayNameGerman { get; protected set; }

    /// <summary>
    /// Gets the website. This is the url for the website.
    /// </summary>
    public string Website { get; protected set; }

    /// <summary>
    /// Gets the photo. It contains the logo for the organisation.
    /// </summary>
    public Image Photo { get; protected set; }

    /// <summary>
    /// Gets or sets the pi vote group id.
    /// </summary>
    /// <value>
    /// The pi vote group id.
    /// </value>
    public int? PiVoteGroupId { get; protected set; }

    /// <summary>
    /// Gets the parent association.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>Parent association or null.</returns>
    public abstract Association GetParentAssociation(LdapConnection connection);

    /// <summary>
    /// Gets the parent association.
    /// </summary>
    /// <param name="cache">The cache.</param>
    /// <returns>Parent association or null.</returns>
    public abstract Association GetParentAssociation(LdapCache cache);

    /// <summary>
    /// Gets or sets the ordering. Zero or less indicate invisible/defunct status.
    /// </summary>
    public abstract int Ordering { get; protected set; }

    /// <summary>
    /// Gets the absolute ordering.
    /// </summary>
    public int OrderingAbsolute { get { return Ordering > 0 ? Ordering : int.MaxValue; } }

    public Association(LdapEntry entry)
      : base(entry)
    {
    }
  }
}
