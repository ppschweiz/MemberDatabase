using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pirate.Ldap;

namespace MemberAdmin
{
  public class SearchParameters
  {
    public List<string> SearchDns { get; private set; }

    public LdapFilter Filter { get; private set; }

    public IEnumerable<LdapAttributeBase> Attributes { get; private set; }

    public IEnumerable<PostFilter> PostFilters { get; private set; }

    public Func<Person, object> OrderBySelector { get; private set; }

    public SearchParameters(IEnumerable<string> searchDns, LdapFilter filter, IEnumerable<LdapAttributeBase> attributes, IEnumerable<PostFilter> postFilters, Func<Person, object> orderBySelector)
    {
      SearchDns = searchDns.ToList();
      Filter = filter;
      Attributes = attributes;
      PostFilters = postFilters;
      OrderBySelector = orderBySelector;
    }
  }
}