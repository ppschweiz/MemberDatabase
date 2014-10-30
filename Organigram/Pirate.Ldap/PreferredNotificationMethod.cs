using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  /// <summary>
  /// Preferred method of notification for a Person.
  /// </summary>
  public enum PreferredNotificationMethod
  {
    /// <summary>
    /// The preferred notification method is not known.
    /// </summary>
    Unknown = -1,

    /// <summary>
    /// Person wished to get letters.
    /// </summary>
    Letter = 0,

    /// <summary>
    /// Person wished to get emails.
    /// </summary>
    Email = 1
  }
}
