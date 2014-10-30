using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirate.Ldap;
using Pirate.Ldap.Data;

namespace Pirate.Ldap.Web
{
  public static class DataGlobal
  {
    private static IDataAccess _dataAccess;

    public static IDataAccess DataAccess
    {
      get
      {
        _dataAccess.Check();
        return _dataAccess;
      }
    }

    public static void Init()
    {
      _dataAccess = new PostgresSqlDataAccess();
    }

    public static void Dispose()
    {
      _dataAccess.Close();
      _dataAccess = null;
    }
  }
}
