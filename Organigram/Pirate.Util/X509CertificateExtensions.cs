using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Pirate.Util
{
  public static class X509CertificateExtensions
  {
    public static X509Certificate2 Load(string certificateData)
    {
      var parts = certificateData.Split(new[] { "-----BEGIN CERTIFICATE-----", "-----END CERTIFICATE-----" }, StringSplitOptions.None);

      if (parts.Length == 3)
      {
        var data = Convert.FromBase64String(parts[1]);
        return new X509Certificate2(data);
      }
      else
      {
        throw new ArgumentException("Invalid data.");
      }
    }
  }
}
