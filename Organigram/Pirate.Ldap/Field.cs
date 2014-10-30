using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public enum MultiValueOutput
  {
    MultiLine,
    Separated,
    FirstOnly
  }

  public abstract class Field
  {
    public bool Identifiying { get; private set; }

    public bool DefaultDisplay { get; private set; }

    public abstract string ValueString(MultiValueOutput output);

    public abstract IComparable CompareObject();

    public virtual bool NeedsEvaluation
    {
      get { return false; }
    }

    public virtual string EvaluateString(LdapConnection connection)
    {
      return null;
    }

    public LdapAttributeBase Attribute { get; private set; }

    public Field(LdapAttributeBase attribute, bool identifying, bool defaultDisplay)
    {
      Attribute = attribute;
      Identifiying = identifying;
      DefaultDisplay = defaultDisplay;
    }
  }
}
