using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Data
{
  public abstract class TextItem
  {
    public string Id
    {
      get { return GetType().Name; }
    }

    public abstract IEnumerable<TextField> Fields { get; }

    public abstract string DefaultText { get; }
  }
}
