using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public class LdapAccess
  {
    private Dictionary<string, object> _objectCache;

    public LdapCache Cache { get; private set; }

    public LdapConnection Connection { get; private set; }

    public LdapAccess(LdapConnection connection)
    {
      Connection = connection;
      Cache = new LdapCache(Connection);
      _objectCache = new Dictionary<string, object>();
    }

    public T Get<T>(string key)
      where T : class
    {
      if (_objectCache.ContainsKey(key))
      {
        return (T)_objectCache[key];
      }
      else
      {
        return null;
      }
    }

    public void Set<T>(string key, T value)
    {
      if (_objectCache.ContainsKey(key))
      {
        _objectCache[key] = value;
      }
      else
      {
        _objectCache.Add(key, value);
      }
    }
  }
}
