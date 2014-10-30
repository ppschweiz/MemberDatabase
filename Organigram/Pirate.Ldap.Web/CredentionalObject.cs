using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Pirate.Ldap.Web
{
  public class CredentionalObject
  {
    public string UserDn { get; private set; }

    public string Password { get; private set; }

    public DateTime Expires { get; private set; }

    public CredentionalObject(string userDn, string password)
    {
      UserDn = userDn;
      Password = password;
      Expires = DateTime.Now.AddMinutes(30);
    }

    public void Disable()
    {
      Expires = DateTime.MinValue;
      Password = string.Empty;
      UserDn = string.Empty;
    }

    public void Update()
    {
      Expires = DateTime.Now.AddMinutes(30);
    }

    public CredentionalObject(byte[] data)
    {
      var stream = new MemoryStream(data);
      var reader = new BinaryReader(stream);
      UserDn = reader.ReadString();
      Password = reader.ReadString();
      Expires = new DateTime(reader.ReadInt64());
      reader.Close();
    }

    public byte[] ToBinary()
    {
      var stream = new MemoryStream();
      var writer = new BinaryWriter(stream);
      writer.Write(UserDn);
      writer.Write(Password);
      writer.Write(Expires.Ticks);
      writer.Close();
      return stream.ToArray();
    }
  }
}