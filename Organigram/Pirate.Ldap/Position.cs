using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public class Position
  {
    public Association Association { get; private set; }

    public Group Group { get; private set; }

    public Role Role { get; private set; }

    public Position(Association association, Group group, Role role)
    {
      Association = association;
      Group = group;
      Role = role;
    }
  }
}
