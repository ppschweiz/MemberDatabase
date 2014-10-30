using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Pirate.Ldap
{
  public class LdapImageField : LdapField<Image>
  {
    public LdapImageField(LdapAttributeBase attribute, Func<Image> get, Action<Image> set, bool required = false, bool identifying = false, bool defaultDisplay = false)
      : base(attribute, get, set, required, identifying, defaultDisplay)
    { 
    }

    private byte[] ToBinary(Image image)
    {
      if (image == null)
      {
        return null;
      }
      else
      {
        using (var stream = new MemoryStream())
        {
          image.Save(stream, ImageFormat.Jpeg);
          stream.Flush();
          return stream.ToArray();
        }
      }
    }

    public override bool IsModified(LdapField<Image> original)
    {
      return !ToBinary(Value).AreEqual(ToBinary(original.Value));
    }

    public override LdapModification GetModification(LdapField<Image> original)
    {
      if (original.Value == null && Value == null)
      {
        return null;
      }
      else if (original.Value == null)
      {
        return new LdapModification(LdapModification.ADD, new LdapAttribute(Attribute, ToBinary(Value).ToSbytes()));
      }
      else if (Value == null)
      {
        return new LdapModification(LdapModification.DELETE, new LdapAttribute(Attribute));
      }
      else if (!ToBinary(Value).AreEqual(ToBinary(original.Value)))
      {
        return new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attribute, ToBinary(Value).ToSbytes()));
      }
      else
      {
        return null;
      }
    }

    public override string ValueString(MultiValueOutput output)
    {
      if (Value == null)
      {
        return string.Empty;
      }
      else
      {
        return string.Format("Image ({0} bytes", ToBinary(Value).Length);
      }
    }

    public override IComparable CompareObject()
    {
      if (Value == null)
      {
        return 0;
      }
      else
      {
        return ToBinary(Value).Length;
      }
    }

    public override bool IsNullOrEmpty()
    {
      return Value == null;
    }

    public override void Load(LdapEntry entry)
    {
      Value = entry.GetAttributeJpegValue(Attribute);
    }

    public override LdapAttribute GetValue()
    {
      if (Value != null)
      {
        return new LdapAttribute(Attribute, ToBinary(Value).ToSbytes());
      }
      else
      {
        return null;
      }
    }
  }
}
