using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pirate.Ldap.Organigram.Svg
{
  public class Rect : Element
  {
    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public string Id { get; set; }

    public Color Color { get; set; }

    public Color Stroke { get; set; }

    public double StrokeWidth { get; set; }

    public Rect(string id, double x, double y, double width, double height)
    {
      Id = id;
      X = x;
      Y = y;
      Width = width;
      Height = height;
      Color = Color.White;
      Stroke = Color.Black;
      StrokeWidth = 1d;
    }

    public override void Render(Context context)
    {
      string style = string.Format(
        "fill:#{0};stroke:#{1};stroke-width:{2};",
        Color.ToHtmlColor(), Stroke.ToHtmlColor(), StrokeWidth);
      context.Text.AppendLine(
        "<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" id=\"{4}\" style=\"{5}\" />", 
        X, Y, Width, Height, Id, style);
    }
  }
}
