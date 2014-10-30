using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using Pirate.Util;

namespace RegistrationService
{
  public class PasswordReset
  {
    private static readonly byte[] MagicHeader = Encoding.UTF8.GetBytes("PasswordReset");

    public string Dn { get; private set; }

    public DateTime Created { get; private set; }

    public PasswordReset(string dn, DateTime created)
    {
      Dn = dn;
      Created = created;
    }

    private PasswordReset()
    { 
    }

    public static PasswordReset FromBinary(byte[] data)
    {
      var stream = new MemoryStream(data);

      using (var reader = new BinaryReader(stream))
      {
        var header = reader.ReadBytes(MagicHeader.Length);

        if (!header.Equal(MagicHeader))
        {
          throw new ArgumentException("Not password reset.");
        }

        var reset = new PasswordReset();
        reset.Dn = reader.ReadString();
        reset.Created = new DateTime(reader.ReadInt64());
        return reset;
      }
    }

    public byte[] ToBinary()
    {
      var stream = new MemoryStream();

      using (var writer = new BinaryWriter(stream))
      {
        writer.Write(MagicHeader);
        writer.Write(Dn);
        writer.Write(Created.Ticks);
        writer.Flush();
        return stream.ToArray();
      }
    }
  }
}