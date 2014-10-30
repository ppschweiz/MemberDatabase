using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;
using Novell.Directory.Ldap;

namespace MemberAdmin
{
  /// <summary>
  /// Extensions to handle section lists. Also includes the
  /// dc=piratenpartei, dc=members and dc=people.
  /// </summary>
  public static class SectionExtensions
  {
    private const string CacheKeySections = "sections";

    private static Dictionary<string, Section> Load(LdapAccess access)
    {
      Dictionary<string, Section> sections = access.Get<Dictionary<string, Section>>(CacheKeySections);

      if (sections == null)
      {
        sections = new Dictionary<string, Section>();
        var loadedSections = access.Connection.Search<Section>(Names.Party, LdapScope.Sub);

        foreach (var section in loadedSections)
        {
          sections.Add(section.DN, section);
        }

        access.Set(CacheKeySections, sections);
      }

      return sections;
    }

    public static IEnumerable<DisplayValue> GetValues(LdapAccess context, bool allowAllMembers)
    {
      var sections = Load(context);

      if (allowAllMembers)
      {
        yield return new DisplayValue(Resources.SectionAll, Names.Party);
        yield return new DisplayValue(Resources.SectionPeople, Names.People);
      }

      yield return new DisplayValue(Resources.SectionMembers, Names.Members);

      foreach (var section in sections.Values.OrderBy(s => s.Ordering))
      {
        yield return new DisplayValue(section.DisplayNameGerman, Names.MembersContainerPrefix + section.DN);
      }
    }

    public static string Text(LdapAccess context, string dn)
    {
      var sections = Load(context);

      switch (dn)
      {
        case Names.Party:
          return Resources.SectionAll;
        case Names.People:
          return Resources.SectionPeople;
        case Names.Members:
          return Resources.SectionMembers;
        default:
          if (dn.StartsWith(Names.MembersContainerPrefix))
          {
            dn = dn.Substring(Names.MembersContainerPrefix.Length);
          }

          return sections[dn].DisplayNameGerman;
      }
    }
 }
}