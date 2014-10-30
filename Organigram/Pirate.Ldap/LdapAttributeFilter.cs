using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public class LdapAttributeFilter : LdapFilter
  {
    public string AttributeName { get; private set; }

    public string AttributeValue { get; private set; }

    public LdapComparison Comparison { get; private set; }

    public LdapAttributeFilter(string attributeName, int attributeValue, LdapComparison comparison = LdapComparison.Equal)
    {
      AttributeName = attributeName;
      AttributeValue = attributeValue.ToString();
      Comparison = comparison;
    }

    public LdapAttributeFilter(string attributeName, string attributeValue, LdapComparison comparison = LdapComparison.Equal)
    {
      AttributeName = attributeName;
      AttributeValue = attributeValue;
      Comparison = comparison;
    }

    public override string Text
    {
      get
      {
        switch (Comparison)
        {
          case LdapComparison.Equal:
            return "(" + AttributeName + "=" + AttributeValue + ")";
          case LdapComparison.GreaterOrEqual:
            return "(" + AttributeName + ">=" + AttributeValue + ")";
          case LdapComparison.SmallerOrEqual:
            return "(" + AttributeName + "<=" + AttributeValue + ")";
          case LdapComparison.Greater:
            return "(!(" + AttributeName + "<=" + AttributeValue + "))";
          case LdapComparison.Smaller:
            return "(!(" + AttributeName + ">=" + AttributeValue + "))";
          default:
            throw new InvalidOperationException("Unknown comparison.");
        }
      }
    }
  }
}
