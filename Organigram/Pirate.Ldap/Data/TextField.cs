using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Data
{
  public class TextField
  {
    public string Key { get; private set; }

    public Func<Person, string> Get { get; private set; }

    public TextField(string key, Func<Person, string> get)
    {
      Key = key;
      Get = get;
    }
  }
}
