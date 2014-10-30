using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Pirate.Textile
{
  public class Header : Element
  {
    public int Number { get; private set; }

    public string Text { get; private set; }

    public Header(int number, string text)
    {
      Number = number;
      Text = text;
    }

    public override string ToText()
    {
      return Text + Environment.NewLine + Environment.NewLine;
    }

    public override string ToHtml()
    {
      return string.Format(
        CultureInfo.InvariantCulture,
        "<h{0}>{1}</h{0}>",
        Number,
        Text);
    }
  }
}
