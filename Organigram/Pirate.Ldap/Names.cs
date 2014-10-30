using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  /// <summary>
  /// Distinguisched names of important static LDAP objects.
  /// </summary>
  public static class Names
  {
    /// <summary>
    /// The members (PPS) container object.
    /// </summary>
    public const string Members = "dc=members,dc=piratenpartei,dc=ch";

    /// <summary>
    /// The people (non-member) container object.
    /// </summary>
    public const string People = "dc=people,dc=piratenpartei,dc=ch";

    /// <summary>
    /// The newsletter container object.
    /// </summary>
    public const string Newsletter = "dc=newsletter,dc=piratenpartei,dc=ch";

    /// <summary>
    /// The party organization root object.
    /// </summary>
    public const string Party = "dc=piratenpartei,dc=ch";

    /// <summary>
    /// The person id unique storage object.
    /// </summary>
    public const string PersonId = "dc=personId,dc=piratenpartei,dc=ch";

    /// <summary>
    /// The party's administration (Verwaltung).
    /// </summary>
    public const string GroupAdministration = "cn=Administration,dc=piratenpartei,dc=ch";

    public const string MembersContainerPrefix = "dc=members,";
  }
}
