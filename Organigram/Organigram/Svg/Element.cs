using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Organigram.Svg
{
  public abstract class Element
  {
    public abstract void Render(Context context);
  }
}
