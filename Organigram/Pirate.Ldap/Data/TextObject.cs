using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Data
{
  public class TextObject
  {
    public string Id { get; private set; }

    public string Text { get; private set; }

    public TextObject(string id, string text)
    {
      Id = id;
      Text = text;
    }
  }
}
