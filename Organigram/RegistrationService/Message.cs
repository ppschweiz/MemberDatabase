using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using Pirate.Util;

namespace RegistrationService
{
  public class Message
  {
    private static readonly byte[] MagicHeader = Encoding.UTF8.GetBytes("Message");

    public string Text { get; private set; }

    public string TextId { get; private set; }

    public string UserDn { get; private set; }

    public string ContinueTo { get; private set; }

    public Message(string textId, string userDn, string continueTo)
    {
      Text = string.Empty;
      TextId = textId;
      UserDn = userDn;
      ContinueTo = continueTo;
    }

    public Message(string text, string continueTo)
    {
      Text = text;
      ContinueTo = continueTo;
      TextId = string.Empty;
      UserDn = string.Empty;
    }

    private Message()
    { 
    }

    public static Message FromBinary(byte[] data)
    {
      var stream = new MemoryStream(data);

      using (var reader = new BinaryReader(stream))
      {
        var header = reader.ReadBytes(MagicHeader.Length);

        if (!header.Equal(MagicHeader))
        {
          throw new ArgumentException("Not password reset.");
        }

        var message = new Message();
        message.Text = reader.ReadString();
        message.TextId = reader.ReadString();
        message.UserDn = reader.ReadString();
        message.ContinueTo = reader.ReadString();
        return message;
      }
    }

    public byte[] ToBinary()
    {
      var stream = new MemoryStream();

      using (var writer = new BinaryWriter(stream))
      {
        writer.Write(MagicHeader);
        writer.Write(Text);
        writer.Write(TextId);
        writer.Write(UserDn);
        writer.Write(ContinueTo);
        writer.Flush();
        return stream.ToArray();
      }
    }
  }
}