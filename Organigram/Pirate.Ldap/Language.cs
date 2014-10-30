using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Pirate.Ldap
{
  public static class Language
  {
    public static IEnumerable<DisplayValue> Values
    {
      get
      {
        yield return new DisplayValue(LdapResources.German, "de");
        yield return new DisplayValue(LdapResources.French, "fr");
        yield return new DisplayValue(LdapResources.Italien, "it");
        yield return new DisplayValue(LdapResources.English, "en");
        yield return new DisplayValue(LdapResources.LanguageUnspecified, Attributes.Values.Unspecified);
      }
    }

    public static CultureInfo GetCulture(string code)
    {
      switch (code)
      {
        case "de":
          return CultureInfo.CreateSpecificCulture("de-DE");
        case "fr":
          return CultureInfo.CreateSpecificCulture("fr-FR");
        case "it":
          return CultureInfo.CreateSpecificCulture("it-IT");
        case "en":
          return CultureInfo.CreateSpecificCulture("en-US");
        default:
          return CultureInfo.CreateSpecificCulture("en-US");
      }
    }

    public static string GetName(string code)
    {
      return Values.Where(x => x.Value == code).Single().Display;
    }
  }
}
