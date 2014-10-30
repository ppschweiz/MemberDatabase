using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;

namespace Pirate.Ldap
{
  public static class EmployeeTypeExtensions
  {
    public static IEnumerable<DisplayValue> Values
    {
      get
      {
        foreach (EmployeeType type in Enum.GetValues(typeof(EmployeeType)))
        {
          yield return new DisplayValue(type.Text(), ((int)type).ToString());
        }
      }
    }

    public static string Text(this EmployeeType? method)
    {
      if (method.HasValue)
      {
        return method.Value.Text();
      }
      else
      {
        return LdapResources.EmployeeTypeUnknown;
      }
    }

    public static string Text(this EmployeeType type)
    {
      switch (type)
      {
        case EmployeeType.LandLubber:
          return LdapResources.EmployeeTypeLandLubber;
        case EmployeeType.Sympathizer:
          return LdapResources.EmployeeTypeSympathizer;
        case EmployeeType.Pirate:
          return LdapResources.EmployeeTypePirate;
        case EmployeeType.Veteran:
          return LdapResources.EmployeeTypeVeteran;
        case EmployeeType.WalkedThePlank:
          return LdapResources.EmployeeTypeWalkedThePlank;
        case EmployeeType.Fleet:
          return LdapResources.EmployeeTypeFleet;
        default:
          return LdapResources.EmployeeTypeUnknown;
      }
    }
  }
}