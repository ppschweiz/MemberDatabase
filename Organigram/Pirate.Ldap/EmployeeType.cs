using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  /// <summary>
  /// Type of membership used by Person objects.
  /// </summary>
  public enum EmployeeType
  {
    /// <summary>
    /// Person is not a member. Used for forum accounts.
    /// </summary>
    LandLubber = 0,

    /// <summary>
    /// Person is a member but has not payed membership fee.
    /// </summary>
    Sympathizer = 1,

    /// <summary>
    /// Person is a pirate and has payed membership fee.
    /// </summary>
    Pirate = 2,

    /// <summary>
    /// Person was once a member.
    /// </summary>
    Veteran = 3,

    /// <summary>
    /// Person was debarred and is no member any more.
    /// </summary>
    WalkedThePlank = 8,

    /// <summary>
    /// Arteficial person member.
    /// </summary>
    Fleet = 9
  }
}
