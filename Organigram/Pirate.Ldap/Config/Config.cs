using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Pirate.Util;

namespace Pirate.Ldap.Config
{
  public class Config : IConfig
  {
    private const string LinuxFilename = "/etc/member.config";
    private const string WindowsFilename = @"D:\Security\PPS\member.config";

    private const string LdapServerKey = "LdapServer";
    private const string BindDnKey = "BindDn";
    private const string BindPasswordKey = "BindPassword";
    private const string MdbguiDnKey = "MdbguiDn";
    private const string MdbguiPasswordKey = "MdbguiPassword";
    private const string PostgresConnectionStringKey = "PostgresConnectionString";
    private const string EncryptionKeyKey = "EncryptionKey";
    private const string AuthenicationKeyKey = "AuthenticationKey";
    private const string MailServerPortKey = "MailServerPort";
    private const string MailServerAddressKey = "MailServerAddress";
    private const string RegistrationServiceAddressKey = "RegistrationServiceAddress";
    private const string MemberServiceAddressKey = "MemberServiceAddress";
    private const string MemberAdminAddressKey = "MemberAdminAddress";
    private const string AdminAddressKey = "AdminAddress";
    private const string AdminDisplayNameKey = "AdminDisplayName";
    private const string RegistrarNotificationAddressKey = "RegistrarNotificationAddress";
    private const string RegistrarNotificationDisplayNameKey = "RegistrarNotificationDisplayName";
    private const string SystemSenderAddressKey = "SystemSenderAddress";
    private const string SystemSenderDisplayNameKey = "SystemSenderDisplayName";
    private const string AdditionalNotificationAddressKey = "AdditionalNotificationAddress";
    private const string AdditionalNotificationDisplayNameKey = "AdditionalNotificationDisplayName";

    public string LdapServer { get; private set; }

    public string BindDn { get; private set; }

    public string BindPassword { get; private set; }

    public string MdbguiDn { get; private set; }

    public string MdbguiPassword { get; private set; }

    public string PostgresConnectionString { get; private set; }

    public byte[] EncryptionKey { get; private set; }

    public byte[] AuthenticationKey { get; private set; }

    public string MailServerAddress { get; private set; }

    public int MailServerPort { get; private set; }

    public string RegistrationServiceAddress { get; private set; }

    public string MemberServiceAddress { get; private set; }

    public string MemberAdminAddress { get; private set; }

    public string AdminAddress { get; private set; }

    public string AdminDisplayName { get; private set; }

    public string RegistrarNotificationAddress { get; private set; }

    public string RegistrarNotificationDisplayName { get; private set; }

    public string AdditionalNotificationAddress { get; private set; }

    public string AdditionalNotificationDisplayName { get; private set; }

    public string SystemSenderAddress { get; private set; }

    public string SystemSenderDisplayName { get; private set; }

    public Config()
    {
      if (Environment.OSVersion.Platform == PlatformID.Unix)
      {
        Load(LinuxFilename);
      }
      else
      {
        Load(WindowsFilename);
      }
    }

    public Config(string filename)
    {
      Load(filename);
    }

    private void Load(string filename)
    {
      Parse(File.ReadAllText(filename));
      Check();
    }

    private void Check()
    {
      LdapServer.ArgumentNotNull();
      BindDn.ArgumentNotNull();
      BindPassword.ArgumentNotNull();
      MdbguiDn.ArgumentNotNull();
      MdbguiPassword.ArgumentNotNull();
      PostgresConnectionString.ArgumentNotNull();
      EncryptionKey.ArgumentNotNull();
      AuthenticationKey.ArgumentNotNull();
      MailServerAddress.ArgumentNotNull();
      MailServerPort.ArgumentInRange(1, ushort.MaxValue);
      MemberServiceAddress.ArgumentNotNull();
      MemberAdminAddress.ArgumentNotNull();
      RegistrationServiceAddress.ArgumentNotNull();
      AdminAddress.ArgumentNotNull();
      AdminDisplayName.ArgumentNotNull();
      RegistrarNotificationAddress.ArgumentNotNull();
      RegistrarNotificationDisplayName.ArgumentNotNull();
      SystemSenderAddress.ArgumentNotNull();
      SystemSenderDisplayName.ArgumentNotNull();
      AdditionalNotificationAddress.ArgumentNotNull();
      AdditionalNotificationDisplayName.ArgumentNotNull();
    }

    private void Parse(string text)
    {
      var lines = text.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

      foreach (var line in lines)
      {
        ParseLine(line.Trim());
      }
    }

    private void ParseLine(string line)
    {
      if (!line.StartsWith("#"))
      {
        var equalIndex = line.IndexOf("=");
        var key = line.Substring(0, equalIndex).Trim();
        var value = line.Substring(equalIndex + 1).Trim();
        Apply(key, value);
      }
    }

    private void Apply(string key, string value)
    {
      switch (key)
      { 
        case LdapServerKey:
          LdapServer = value;
          break;
        case BindDnKey:
          BindDn = value;
          break;
        case BindPasswordKey:
          BindPassword = value;
          break;
        case MdbguiDnKey:
          MdbguiDn = value;
          break;
        case MdbguiPasswordKey:
          MdbguiPassword = value;
          break;
        case PostgresConnectionStringKey:
          PostgresConnectionString = value;
          break;
        case EncryptionKeyKey:
          EncryptionKey = value.ParseHexBytes();
          EncryptionKey.Length.ArgumentInRange(32, 32);
          break;
        case AuthenicationKeyKey:
          AuthenticationKey = value.ParseHexBytes();
          AuthenticationKey.Length.ArgumentInRange(32, 32);
          break;
        case MailServerAddressKey:
          MailServerAddress = value;
          break;
        case MailServerPortKey:
          MailServerPort = int.Parse(value);
          break;
        case RegistrationServiceAddressKey:
          RegistrationServiceAddress = value;
          break;
        case MemberAdminAddressKey:
          MemberAdminAddress = value;
          break;
        case MemberServiceAddressKey:
          MemberServiceAddress = value;
          break;
        case AdminAddressKey:
          AdminAddress = value;
          break;
        case AdminDisplayNameKey:
          AdminDisplayName = value;
          break;
        case RegistrarNotificationAddressKey:
          RegistrarNotificationAddress = value;
          break;
        case RegistrarNotificationDisplayNameKey:
          RegistrarNotificationDisplayName = value;
          break;
        case SystemSenderAddressKey:
          SystemSenderAddress = value;
          break;
        case SystemSenderDisplayNameKey:
          SystemSenderDisplayName = value;
          break;
        case AdditionalNotificationAddressKey:
          AdditionalNotificationAddress = value;
          break;
        case AdditionalNotificationDisplayNameKey:
          AdditionalNotificationDisplayName = value;
          break;
        default:
          throw new InvalidOperationException("Config key not understood: " + key);
      }
    }

    public byte[] Secure(byte[] plaintext)
    {
      return Crypto.AppendHashMac(Crypto.Encrypt(EncryptionKey, plaintext), AuthenticationKey);
    }

    public byte[] DeSecure(byte[] ciphertext)
    {
      var verfied = Crypto.VerifyAndRemoveHashMac(ciphertext, AuthenticationKey);
      return verfied == null ? null : Crypto.Decrypt(EncryptionKey, verfied);
    }
  }
}