using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Pirate.Textile
{
  public class List : Element
  {
    private List<string> _lines;

    public IEnumerable<string> Lines { get { return _lines; } }

    public bool Numbered { get; private set; }

    public List(bool numbered, string text)
    {
      Numbered = numbered;
      _lines = new List<string>();
      _lines.Add(text);
    }

    public void Add(string text)
    {
      _lines.Add(text);
    }

    public override string ToText()
    {
      if (Numbered)
      {
        var numberedLines = new List<string>();
        var number = 1;

        foreach (var line in Lines)
        {
          numberedLines.Add(number + ". " + Span.RenderText(line));
          number++;
        }

        return string.Join(Environment.NewLine, numberedLines.ToArray()) + Environment.NewLine + Environment.NewLine;
      }
      else
      {
        return string.Join(Environment.NewLine, Lines.Select(line => "* " + Span.RenderText(line)).ToArray()) + Environment.NewLine + Environment.NewLine;
      }
    }

    public override string ToHtml()
    {
      return string.Format(
        CultureInfo.InvariantCulture,
        Numbered ? "<ol>{0}</ol>" : "<ul>{0}</ul>",
        string.Join(Environment.NewLine, Lines.Select(line => "<li>" + Span.RenderHtml(line) + "</li>").ToArray()));
    }
  }
}
