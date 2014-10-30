using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Openssl;
using Pirate.Util.Logging;

namespace MemberService
{
  public class OutputCaCertificate : CustomHttpHandler
  {
    protected override string SourceName
    {
      get { return "OutputCaCertificate"; }
    }
    
    private const string CertificateData = 
      "-----BEGIN CERTIFICATE-----" +
      "MIIEPzCCAyegAwIBAgIJAOduOZlsLZzIMA0GCSqGSIb3DQEBBQUAMIG1MQswCQYD" +
      "VQQGEwJDSDELMAkGA1UECAwCVkQxETAPBgNVBAcMCFZhbGxvcmJlMSEwHwYDVQQK" +
      "DBhQaXJhdGUgUGFydHkgU3dpdHplcmxhbmQxGDAWBgNVBAsMD01lbWJlciBEYXRh" +
      "YmFzZTEeMBwGA1UEAwwVQ2VydGlmaWNhdGUgQXV0aG9yaXR5MSkwJwYJKoZIhvcN" +
      "AQkBFhpyZWdpc3RyYXJAcGlyYXRlbnBhcnRlaS5jaDAeFw0xMzA0MTQxNjA3NDha" +
      "Fw0yMzA0MTIxNjA3NDhaMIG1MQswCQYDVQQGEwJDSDELMAkGA1UECAwCVkQxETAP" +
      "BgNVBAcMCFZhbGxvcmJlMSEwHwYDVQQKDBhQaXJhdGUgUGFydHkgU3dpdHplcmxh" +
      "bmQxGDAWBgNVBAsMD01lbWJlciBEYXRhYmFzZTEeMBwGA1UEAwwVQ2VydGlmaWNh" +
      "dGUgQXV0aG9yaXR5MSkwJwYJKoZIhvcNAQkBFhpyZWdpc3RyYXJAcGlyYXRlbnBh" +
      "cnRlaS5jaDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMU1Ot8c+ncK" +
      "Md6ZhX8b6zqm8zNSKP+uy632Zz/ThNWZBYtqs2EHMQWUqWjXQEB6QG4pAuYyXV4y" +
      "/z8A8puKpGas7sBknhqsY6einIhpCGcRUCleuZ25bFM+Oeu98o1tJbtgBskqfhBq" +
      "7FmNAS4M/A4BQg16iEDaXANmf/U1Iar/xCaoDl91hvC8FtM0RLOEWmMTeOL5qVnT" +
      "CLcMpyWMRB6jjNmcV2JzPEVFqc+KcMN+WVkINwYhyy1g07iLMqYAZ23cQGW5S+xa" +
      "shvzpjj9Qr6ffYEfBt4IpJ5hSgUit1+Yp5QbXmK0wIcufMn8fg9nvkxAl+uArtra" +
      "4TckhWvFfsMCAwEAAaNQME4wHQYDVR0OBBYEFFvd82lKd8IV+PohnZ+JPakds+35" +
      "MB8GA1UdIwQYMBaAFFvd82lKd8IV+PohnZ+JPakds+35MAwGA1UdEwQFMAMBAf8w" +
      "DQYJKoZIhvcNAQEFBQADggEBACwUyaTmtAJA+QHeOROY1xtFExKQIgpcbEVM8VMD" +
      "mNOqn062iPUioPy2IdQ/GhsoPMNT9PMz7FMkHTjp6H/QzTs1D4O6kEVdCeiV999a" +
      "6T0Gv53jDv1YiyBUvJ6rsLh0mFiGEmcQlf2soGJl98Ecw5n6u2HmZ8Sd5szqIxN4" +
      "MQonJS2Cl6mlNT41Ur/8Z10xk81CiIGz26DAcnVkMMTcZVo9JZTcquGXg89aa4Vz" +
      "IwxlmDlcRE92AvXPLa7+A0b2pwSs5dz81V8Z3b/RX1vv6v4XZeM/Vsz1Ru169Br7" +
      "N/EidcdhUplSiziqItPQw16EXE3AjgIwVz5/r8Yhv+CVmas=" +
      "-----END CERTIFICATE-----";

    public override void ProcessRequest(HttpContext context)
    {
      var executor = new Executor();
      var certificateDataDer = executor.ConvertPemToDer(CertificateData);
      context.Response.BinaryWrite(certificateDataDer);
      context.Response.ContentType = "application/x-x509-ca-cert";
    }
  }
}
