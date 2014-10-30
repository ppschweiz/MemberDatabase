using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Pirate.Ldap;

namespace RegistrationService
{
  public static class NotificationMethod
  {
    public static IEnumerable<DisplayValue> Values
    {
      get
      {
        yield return new DisplayValue(LdapResources.Letter, ((int)PreferredNotificationMethod.Letter).ToString());
        yield return new DisplayValue(LdapResources.Email, ((int)PreferredNotificationMethod.Email).ToString());
      }
    }

    public static string GetName(string code)
    {
      return Values.Where(x => x.Value == code).Single().Display;
    }
  }
}
