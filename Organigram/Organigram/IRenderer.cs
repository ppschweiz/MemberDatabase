using System;

namespace Pirate.Ldap.Organigram
{
  public interface IRenderer
  {
    void Create(Organization organization);
  }
}
