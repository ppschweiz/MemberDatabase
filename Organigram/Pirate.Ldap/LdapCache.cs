using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  /// <summary>
  /// Caches LDAP object like sections for faster access.
  /// </summary>
  public class LdapCache
  {
    private readonly LdapConnection _connection;

    private readonly Dictionary<string, LdapObject> _objects;

    /// <summary>
    /// Initializes a new instance of the <see cref="LdapCache"/> class.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public LdapCache(LdapConnection connection)
    {
      _connection = connection;
      _objects = new Dictionary<string, LdapObject>();
    }

        /// <summary>
    /// Gets the object with the specified DN.
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <param name="dn">The dn.</param>
    /// <returns></returns>
    public void Load<TObject>(string dn, LdapScope scope = LdapScope.Base, LdapFilter filter = null)
      where TObject : LdapObject
    {
      var list = _connection.Search<TObject>(dn, scope, filter);

      foreach (var obj in list)
      {
        if (!_objects.ContainsKey(obj.DN))
        {
          _objects.Add(obj.DN, obj);
        }
      }
    }

    /// <summary>
    /// Gets the object with the specified DN.
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <param name="dn">The dn.</param>
    /// <returns></returns>
    public TObject Get<TObject>(string dn)
      where TObject : LdapObject
    {
      if (_objects.ContainsKey(dn))
      {
        return _objects[dn] as TObject;
      }
      else
      {
        var obj = _connection.SearchFirst<TObject>(dn) as TObject;
        _objects.Add(dn, obj);
        return obj;
      }
    }
  }
}
