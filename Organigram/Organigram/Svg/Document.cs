using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Organigram.Svg
{
  public class Document : Element
  {
    private List<Element> _elements;

    public Document()
    {
      _elements = new List<Element>();
    }

    public void Add(Element element)
    {
      _elements.Add(element);
    }

    public override void Render(Context context)
    {
      context.Text.AppendLine(
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
      context.Text.AppendLine(
        "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"100%\" height=\"100%\">");

      foreach (var element in _elements)
      {
        element.Render(context);
      }

      context.Text.AppendLine("</svg>");
    }
  }
}
