using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pirate.Ldap.Organigram
{
  public abstract class Div
  {
    public int Top { get; set; }

    public int Left { get; set; }

    public abstract int Width { get; }

    public abstract int Height { get; }

    public int MinimalWidth { get; set; }

    public int MinimalHeight { get; set; }

    public string Text { get; set; }

    public Color BackColor { get; set; }

    public Color ForeColor { get; set; }

    public string Url { get; set; }

    public string HtmlBackColor
    {
      get
      {
        return string.Format("{0:x2}{1:x2}{2:x2}", BackColor.R, BackColor.G, BackColor.B);
      }
    }

    public string HtmlForeColor
    {
      get
      {
        return string.Format("{0:x2}{1:x2}{2:x2}", ForeColor.R, ForeColor.G, ForeColor.B);
      }
    }

    public abstract void Render(DrawingContext context);
  }

  public class BaseDiv : Div
  {
    private int _width;

    private int _height;

    public override int Width { get { return _width; } }

    public override int Height { get { return _height; } }

    public BaseDiv(int width, int height, string text)
    {
      _width = width;
      _height = height;
      Text = text;
    }

    public override void Render(DrawingContext context)
    {
      var id = context.NewId;

      context.Css.AppendLine("#{0} a:active {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:link {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:visited {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:hover {{ color:#{1}; }}", id, HtmlForeColor);

      string text = string.IsNullOrEmpty(Url) ? Text : string.Format("<a href=\"{0}\">{1}</a>", Url, Text);
      context.Body.AppendLine("<div id=\"{0}\">{1}</div>", id, text);
      context.Css.AppendLine("#{0} {{ position:absolute; left:{1}px; top:{2}px; width:{3}px; height:{4}px; z-index:1; background-color:#{5}; color:#{6}; }}", id, Left, Top, Width, Height, HtmlBackColor, HtmlForeColor);
    }
  }

  public class HorizontalDiv : Div
  {
    private List<Div> _contents;

    public IEnumerable<Div> Contents { get { return _contents; } }

    public HorizontalDiv(string text)
    {
      _contents = new List<Div>();
      Text = text;
    }

    public void Add(Div div)
    {
      _contents.Add(div);
    }

    public override int Width
    {
      get { return Math.Max(MinimalWidth, _contents.Select(d => d.Width).Sum() + (_contents.Count * 5) + 5); }
    }

    public override int Height
    {
      get { return Math.Max(MinimalHeight, (_contents.Count > 0 ? _contents.Max(d => d.Height) : 0) + 10 + HeaderHeight); }
    }

    public int HeaderHeight
    {
      get
      {
        return string.IsNullOrEmpty(Text) ? 0 : 20;
      }
    }
    
    public override void Render(DrawingContext context)
    {
      var id = context.NewId;

      context.Css.AppendLine("#{0} a:active {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:link {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:visited {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:hover {{ color:#{1}; }}", id, HtmlForeColor);

      string text = string.IsNullOrEmpty(Url) ? Text : string.Format("<a href=\"{0}\">{1}</a>", Url, Text);
      context.Body.AppendLine("<div id=\"{0}\">", id);
      context.Body.AppendLine("<div id=\"header\">{0}</div>", text);

      int left = 5;
      int top = 5 + HeaderHeight;

      foreach (var div in _contents)
      {
        div.Left = left;
        div.Top = top;
        div.Render(context);
        left += div.Width + 5;
      }

      context.Css.AppendLine("#{0} {{ position:absolute; left:{1}px; top:{2}px; width:{3}px; height:{4}px; z-index:1; background-color:#{5}; color:#{6}; }}", id, Left, Top, Width, Height, HtmlBackColor, HtmlForeColor);
      context.Body.AppendLine("</div>");
    }
  }

  public class VerticalDiv : Div
  {
    private List<Div> _contents;

    public IEnumerable<Div> Contents { get { return _contents; } }

    public VerticalDiv(string text)
    {
      _contents = new List<Div>();
      Text = text;
    }

    public void Add(Div div)
    {
      _contents.Add(div);
    }

    public override int Width
    {
      get { return Math.Max(MinimalWidth, (_contents.Count > 0 ? _contents.Max(d => d.Width) : 0) + 10); }
    }

    public override int Height
    {
      get { return Math.Max(MinimalHeight, _contents.Select(d => d.Height).Sum() + (_contents.Count * 5) + 5 + HeaderHeight); }
    }

    public int HeaderHeight
    {
      get
      {
        return string.IsNullOrEmpty(Text) ? 0 : 20;
      }
    }

    public override void Render(DrawingContext context)
    {
      var id = context.NewId;

      context.Css.AppendLine("#{0} a:active {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:link {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:visited {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:hover {{ color:#{1}; }}", id, HtmlForeColor);

      string text = string.IsNullOrEmpty(Url) ? Text : string.Format("<a href=\"{0}\">{1}</a>", Url, Text);
      context.Body.AppendLine("<div id=\"{0}\">", id);
      context.Body.AppendLine("<div id=\"header\">{0}</div>", text);

      int left = 5;
      int top = 5 + HeaderHeight;

      foreach (var div in _contents)
      {
        div.Left = left;
        div.Top = top;
        div.Render(context);
        top += div.Height + 5;
      }

      context.Css.AppendLine("#{0} {{ position:absolute; left:{1}px; top:{2}px; width:{3}px; height:{4}px; z-index:1; background-color:#{5}; color:#{6}; }}", id, Left, Top, Width, Height, HtmlBackColor, HtmlForeColor);
      context.Body.AppendLine("</div>");
    }
  }

  public class FlowDiv : Div
  {
    private List<Div> _contents;

    private int _width;

    public IEnumerable<Div> Contents { get { return _contents; } }

    public FlowDiv(int width, string text)
    {
      MinimalHeight = 20;
      _width = width;
      _contents = new List<Div>();
      Text = text;
    }

    public void Add(Div div)
    {
      _contents.Add(div);
    }

    public override int Width
    {
      get { return Math.Max(MinimalWidth, _width); }
    }

    public override int Height
    {
      get { return Math.Max(MinimalHeight, (_contents.Count > 0 ? _contents.Max(d => d.Top + d.Height) : 0) + 5); }
    }

    public int HeaderHeight
    {
      get
      {
        return string.IsNullOrEmpty(Text) ? 0 : 20;
      }
    }
    
    public override void Render(DrawingContext context)
    {
      var id = context.NewId;
     
      context.Css.AppendLine("#{0} a:active {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:link {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:visited {{ color:#{1}; }}", id, HtmlForeColor);
      context.Css.AppendLine("#{0} a:hover {{ color:#{1}; }}", id, HtmlForeColor);

      string text = string.IsNullOrEmpty(Url) ? Text : string.Format("<a href=\"{0}\">{1}</a>", Url, Text);
      context.Body.AppendLine("<div id=\"{0}\">", id);
      context.Body.AppendLine("<div id=\"header\">{0}</div>", text);

      int left = 5;
      int top = 5 + HeaderHeight;

      foreach (var div in _contents)
      {
        if (left + div.Width + 5 > _width)
        {
          left = 5;
          top = _contents.Count > 0 ? _contents.Max(d => d.Top + d.Height) + 5 : 5;
        }

        div.Left = left;
        div.Top = top;
        div.Render(context);
        left += div.Width + 5;
      }

      context.Css.AppendLine("#{0} {{ position:absolute; left:{1}px; top:{2}px; width:{3}px; height:{4}px; z-index:1; background-color:#{5}; color:#{6}; }}", id, Left, Top, Width, Height, HtmlBackColor, HtmlForeColor);
      context.Body.AppendLine("</div>");
    }
  }
}
