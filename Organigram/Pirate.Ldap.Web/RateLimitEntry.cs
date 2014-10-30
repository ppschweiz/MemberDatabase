using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Web
{
  public class RateLimitEntry
  {
    public string Key { get; private set; }

    public DateTime FirstLogged { get; set; }

    public int Count { get; set; }

    public RateLimitEntry(string key)
    {
      Key = key;
      FirstLogged = DateTime.Now;
      Count = 0;
    }
  }
}
