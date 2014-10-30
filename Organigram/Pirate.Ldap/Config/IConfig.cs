using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Config
{
  public interface IConfig
  {
    string BindDn { get; }

    string BindPassword { get; }

    string MdbguiDn { get; }

    string MdbguiPassword { get; }

    string PostgresConnectionString { get; }

    byte[] Secure(byte[] plaintext);

    byte[] DeSecure(byte[] ciphertext);

    string MailServerAddress { get; }

    int MailServerPort { get; }

    string RegistrationServiceAddress { get; }

    string MemberServiceAddress { get; }

    string MemberAdminAddress { get; }

    string AdminAddress { get; }

    string AdminDisplayName { get; }

    string RegistrarNotificationAddress { get; }

    string RegistrarNotificationDisplayName { get; }

    string AdditionalNotificationAddress { get; }

    string AdditionalNotificationDisplayName { get; }

    string SystemSenderAddress { get; }

    string SystemSenderDisplayName { get; }
  }
}
