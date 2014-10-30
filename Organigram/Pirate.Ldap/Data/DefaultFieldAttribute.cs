
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Data
{
  public class DefaultFieldAttribute : Attribute
  {
    public Func<Person, string> Get { get; private set; }

    public DefaultFieldAttribute(Func<Person, string> get)
    {
      Get = get;
    }
  }
}
