using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Data
{
  public static class AuthenticationLevel
  {
    public static string Text(int authenticationLevel)
    {
      switch (authenticationLevel)
      {
        case 0:
          return LdapResources.CertificateLevel0;
        case 1:
          return LdapResources.CertificateLevel1;
        case 2:
          return LdapResources.CertificateLevel2;
        case 3:
          return LdapResources.CertificateLevel3;
        case 4:
          return LdapResources.CertificateLevel4;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
