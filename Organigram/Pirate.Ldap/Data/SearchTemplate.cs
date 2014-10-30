using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pirate.Ldap.Data
{
  public class SearchTemplate
  {
    public string Name { get; private set; }

    public string Definition { get; private set; }

    public SearchTemplate(string name, string definition)
    {
      Name = name;
      Definition = definition;
    }
  }
}