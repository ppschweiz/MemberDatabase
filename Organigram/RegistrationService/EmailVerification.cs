
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using Pirate.Util;

namespace RegistrationService
{
  public class EmailVerification
  {
    private static readonly byte[] MagicHeader = Encoding.UTF8.GetBytes("EmailVerification");

    public string Email { get; private set; }

    public DateTime Created { get; private set; }

    public EmailVerification(string email, DateTime created)
    {
      Email = email;
      Created = created;
    }

    private EmailVerification()
    { 
    }

    public static EmailVerification FromBinary(byte[] data)
    {
      var stream = new MemoryStream(data);

      using (var reader = new BinaryReader(stream))
      {
        var header = reader.ReadBytes(MagicHeader.Length);

        if (!header.Equal(MagicHeader))
        {
          throw new ArgumentException("Email verification.");
        }

        var verification = new EmailVerification();
        verification.Email = reader.ReadString();
        verification.Created = new DateTime(reader.ReadInt64());
        return verification;
      }
    }

    public byte[] ToBinary()
    {
      var stream = new MemoryStream();

      using (var writer = new BinaryWriter(stream))
      {
        writer.Write(MagicHeader);
        writer.Write(Email);
        writer.Write(Created.Ticks);
        writer.Flush();
        return stream.ToArray();
      }
    }
  }
}