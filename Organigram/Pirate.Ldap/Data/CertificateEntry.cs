using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pirate.Ldap.Data
{
  public class CertificateEntry
  {
    public long Key { get; private set; }

    public string Fingerprint { get; private set; }

    public int AuthorizationLevel { get; private set; }

    public int UserUniqueIdentifier { get; private set; }

    public string Comment { get; private set; }

    public string CertificateData { get; private set; }

    public CertificateEntry(long key, string fingerprint, int authorizationLevel, int userUniqueIdentifier, string comment, string certificateData)
    {
      Key = key;
      Fingerprint = fingerprint;
      AuthorizationLevel = authorizationLevel;
      UserUniqueIdentifier = userUniqueIdentifier;
      Comment = comment;
      CertificateData = certificateData;
    }
  }
}