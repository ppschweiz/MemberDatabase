using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Pirate.Textile
{
  public class Paragraph : Element
  {
    private List<string> _lines;

    public IEnumerable<string> Lines { get { return _lines; } }

    public Paragraph(string text)
    {
      _lines = new List<string>();
      _lines.Add(text);
    }

    public void Add(string text)
    {
      _lines.Add(text);
    }

    public override string ToText()
    {
      return string.Join(Environment.NewLine, Lines.Select(line => Span.RenderText(line)).ToArray()) + Environment.NewLine + Environment.NewLine;
    }

    public override string ToHtml()
    {
      return string.Format(
        CultureInfo.InvariantCulture,
        "<p>{0}</p>",
        string.Join("<br/>" + Environment.NewLine, Lines.Select(line => Span.RenderHtml(line)).ToArray()));
    }
  }
}
