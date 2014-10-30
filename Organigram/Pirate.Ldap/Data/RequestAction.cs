using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Data
{
  public enum RequestAction
  {
    Invalid = 0,
    Join = 1,
    Transfer = 2,
    Leave = 3,
    LeaveAndRemoveData = 4,
    LeaveAndDelete = 5
  }
}
