using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;

namespace Pirate.Ldap
{
  public static class PreferredNotificationMethodExtensions
  {
    public static IEnumerable<DisplayValue> Values
    {
      get
      {
        foreach (PreferredNotificationMethod method in Enum.GetValues(typeof(PreferredNotificationMethod)))
        {
          yield return new DisplayValue(method.Text(), ((int)method).ToString());
        }
      }
    }

    public static string Text(this PreferredNotificationMethod? method)
    {
      if (method.HasValue)
      {
        return method.Value.Text();
      }
      else
      {
        return PreferredNotificationMethod.Unknown.Text();
      }
    }
    
    public static string Text(this PreferredNotificationMethod method)
    {
      switch (method)
      {
        case PreferredNotificationMethod.Letter:
          return LdapResources.PreferrredNotificationMethodLetter;
        case PreferredNotificationMethod.Email:
          return LdapResources.PreferrredNotificationMethodEmail;
        default:
          return LdapResources.PreferrredNotificationMethodUnknown;
      }
    }
  }
}