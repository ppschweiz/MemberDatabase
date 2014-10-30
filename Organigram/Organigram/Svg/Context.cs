using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pirate.Ldap.Organigram.Svg
{
  public class Context
  {
    public StringBuilder Text { get; private set; }

    public Context()
    {
      Text = new StringBuilder();
    }
  }
}
