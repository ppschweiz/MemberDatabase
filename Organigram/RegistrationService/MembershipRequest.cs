using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using Pirate.Util;

namespace RegistrationService
{
  public class MembershipRequest
  {
    private static readonly byte[] MagicHeader = Encoding.UTF8.GetBytes("MembershipRequest");

    public string Dn { get; private set; }

    public DateTime Created { get; private set; }

    public MembershipRequest(string dn, DateTime created)
    {
      Dn = dn;
      Created = created;
    }

    private MembershipRequest()
    { 
    }

    public static MembershipRequest FromBinary(byte[] data)
    {
      var stream = new MemoryStream(data);

      using (var reader = new BinaryReader(stream))
      {
        var header = reader.ReadBytes(MagicHeader.Length);

        if (!header.Equal(MagicHeader))
        {
          throw new ArgumentException("Not membership request.");
        }

        var request = new MembershipRequest();
        request.Dn = reader.ReadString();
        request.Created = new DateTime(reader.ReadInt64());
        return request;
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