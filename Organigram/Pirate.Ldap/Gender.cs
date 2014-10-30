using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  /// <summary>
  /// Gender of a Person.
  /// </summary>
  public enum Gender
  {
    /// <summary>
    /// Gender is not known.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Person is male.
    /// </summary>
    Male = 1,

    /// <summary>
    /// Person is female.
    /// </summary>
    Female = 2,

    /// <summary>
    /// Gender is not applicable to that Person.
    /// </summary>
    NotApplicable = 3
  }
}
