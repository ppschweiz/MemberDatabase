using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Web
{
  public class Info
  {
    public string Name { get; private set; }

    public string Value { get; private set; }

    public Info(string name, object value)
    {
      Name = name;
      Value = value.ToString();
    }
  }
}
