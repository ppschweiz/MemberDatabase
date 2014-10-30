using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;

namespace Pirate.Ldap
{
  public static class GenderExtensions
  {
    public static IEnumerable<DisplayValue> Values
    {
      get
      {
        foreach (Gender gender in Enum.GetValues(typeof(Gender)))
        {
          yield return new DisplayValue(gender.Text(), ((int)gender).ToString());
        }
      }
    }

    public static string Text(this Gender? gender)
    {
      if (gender.HasValue)
      {
        return gender.Value.Text();
      }
      else
      {
        return Gender.Unknown.Text();
      }
    }

    public static string Text(this Gender gender)
    {
      switch (gender)
      {
        case Gender.Female:
          return LdapResources.GenderFemale;
        case Gender.Male:
          return LdapResources.GenderMale;
        case Gender.NotApplicable:
          return LdapResources.GenderNotApplication;
        default:
          return LdapResources.GenderUnknown;
      }
    }
  }
}