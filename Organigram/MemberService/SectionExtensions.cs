using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;
using Novell.Directory.Ldap;

namespace MemberService
{
  /// <summary>
  /// Extensions to handle section lists. Also includes the
  /// dc=piratenpartei, dc=members and dc=people.
  /// </summary>
  public static class SectionExtensions
  {
    private static Dictionary<string, Section> _sections;

    private static void Load(LdapConnection connection)
    {
      if (_sections == null)
      {
        _sections = new Dictionary<string, Section>();
        var sections = connection.Search<Section>(Names.Party, LdapScope.Sub);;

        foreach (var section in sections)
        {
          _sections.Add(section.DN, section);
        }
      }
    }

    public static IEnumerable<DisplayValue> GetValues(LdapConnection connection, bool allowAllMembers)
    {
      Load(connection);

      if (allowAllMembers)
      {
        yield return new DisplayValue(Resources.SectionAll, Names.Party);
        yield return new DisplayValue(Resources.SectionPeople, Names.People);
      }

      yield return new DisplayValue(Resources.SectionMembers, Names.Members);

      foreach (var section in _sections.Values.OrderBy(s => s.Ordering))
      {
        yield return new DisplayValue(section.DisplayNameGerman, Names.MembersContainerPrefix + section.DN);
      }
    }

    public static string Text(LdapConnection connection, string dn)
    {
      Load(connection);

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

          return _sections[dn].DisplayNameGerman;
      }
    }
 }
}