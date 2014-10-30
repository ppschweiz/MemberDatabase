using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Textile
{
  public class TextParser
  {
    private Element _lastElement;

    public static Text Parse(string text)
    {
      var parser = new TextParser();
      return parser.ParseText(text);
    }

    public Text ParseText(string text)
    {
      var element = new Text();
      var lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

      foreach (var line in lines)
      {
        ParseLine(element, line.Trim());
      }

      return element;
    }

    private void ParseLine(Text element, string text)
    {
      if (text.StartsWith("h1."))
      { 
        _lastElement = new Header(1, text.Substring(3).Trim());
        element.Add(_lastElement);
      }
      else if (text.StartsWith("h2."))
      {
        _lastElement = new Header(2, text.Substring(3).Trim());
        element.Add(_lastElement);
      }
      else if (text.StartsWith("h3."))
      {
        _lastElement = new Header(3, text.Substring(3).Trim());
        element.Add(_lastElement);
      }
      else if ((text.StartsWith("*") && !text.EndsWith("*")) ||
               (text.StartsWith("**") && text.EndsWith("*")))
      {
        var listText = text.Substring(1).Trim();
        var lastList = _lastElement as List;

        if (lastList != null && !lastList.Numbered)
        {
          lastList.Add(listText);
        }
        else
        {
          _lastElement = new List(false, listText);
          element.Add(_lastElement);
        }
      }
      else if (text.StartsWith("#"))
      {
        var listText = text.Substring(1).Trim();
        var lastList = _lastElement as List;

        if (lastList != null && lastList.Numbered)
        {
          lastList.Add(listText);
        }
        else
        {
          _lastElement = new List(true, listText);
          element.Add(_lastElement);
        }
      }
      else if (text == string.Empty)
      {
        _lastElement = null;
      }
      else
      {
        var lastParagraph = _lastElement as Paragraph;

        if (lastParagraph != null)
        {
          lastParagraph.Add(text);
        }
        else
        {
          _lastElement = new Paragraph(text);
          element.Add(_lastElement);
        }
      }
    }
  }
}
