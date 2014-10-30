using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pirate.Ldap.Organigram.Svg
{
  public class Text : Element
  {
    public double X { get; set; }

    public double Y { get; set; }

    public string Id { get; set; }

    public string Content { get; set; }

    public string Font { get; set; }

    public double FontSize { get; set; }

    public Color Color { get; set; }

    public Text(string id, string content, double x, double y)
    {
      Id = id;
      Content = content;
      X = x;
      Y = x;
      Color = Color.Black;
      Font = "Aller";
      FontSize = 14;
    }

    public override void Render(Context context)
    {
      string style = string.Format(
        "fill:#{0};",
        Color.ToHtmlColor());
      string fontStyle = string.Format(
        "font-family:{0};font-weight:normal;font-style:normal;font-stretch:normal;font-variant:normal;font-size:{1}px;",
        Font,
        FontSize);
      context.Text.AppendLine(
        "<text x=\"{0}\" y=\"{1}\" id=\"{2}\" style=\"{3}\"><tspan style=\"{5}\">{4}</tspan></text>",
        X, Y, Id, style, Content, fontStyle);
    }
  }
}
