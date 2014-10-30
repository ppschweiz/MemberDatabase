using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Textile
{
  public class Text : Element
  {
    private List<Element> _elements;

    public IEnumerable<Element> Elements { get { return _elements; } }

    public Text()
    {
      _elements = new List<Element>();
    }

    public void Add(Element element)
    {
      _elements.Add(element);
    }

    public override string ToText()
    {
      var text = new StringBuilder();

      foreach (var element in _elements)
      {
        text.AppendLine(element.ToText());
      }

      return text.ToString();
    }

    public override string ToHtml()
    {
      var text = new StringBuilder();

      foreach (var element in _elements)
      {
        text.AppendLine(element.ToHtml());
      }

      return text.ToString();
    }
  }
}
