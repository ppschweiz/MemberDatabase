using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Openssl
{
  public class Spkac
  {
    public string KeyData { get; private set; }
    public string CountryName { get; set; }
    public string StateOrProvinceName { get; set; }
    public string LocalityName { get; set; }
    public string OrganizationName { get; set; }
    public string OrganizationalUnitName { get; set; }
    public string CommonName { get; set; }
    public string EmailAddress { get; set; }

    public Spkac(string keyData)
    {
      KeyData = keyData
        .Replace("\r", string.Empty)
        .Replace("\n", string.Empty);
    }

    public string Text
    {
      get 
      {
        var text = new StringBuilder();
        text.AppendLine("SPKAC=" + KeyData);
        text.AppendLine("countryName=" + CountryName);
        text.AppendLine("stateOrProvinceName=" + StateOrProvinceName);
        text.AppendLine("localityName=" + LocalityName);
        text.AppendLine("organizationName=" + OrganizationName);
        text.AppendLine("organizationalUnitName=" + OrganizationalUnitName);
        text.AppendLine("commonName=" + CommonName);
        text.AppendLine("emailAddress=" + EmailAddress);
        return text.ToString();
      }
    }
  }
}
