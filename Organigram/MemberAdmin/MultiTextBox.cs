using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

namespace MemberAdmin
{
  /// <summary>
  /// Multiple textboxes for multivalue attributes.
  /// </summary>
  public class MultiTextBox : Panel
  {
    private List<TextBox> _boxes;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTextBox"/> class.
    /// </summary>
    public MultiTextBox()
    {
      Warning = false;
      _boxes = new List<TextBox>();
    }

    /// <summary>
    /// Gets or sets a value indicating whether this control should show a warning.
    /// </summary>
    /// <value>
    ///   <c>true</c> if warning; otherwise, <c>false</c>.
    /// </value>
    public bool Warning { get; set; }

    /// <summary>
    /// Gets or sets the values.
    /// </summary>
    /// <value>
    /// The values.
    /// </value>
    public IEnumerable<string> Values
    {
      get
      {
        return _boxes.Select(b => b.Text).Where(t => !string.IsNullOrEmpty(t));
      }
      set
      {
        _boxes.Clear();
        Controls.Clear();

        var table = new Table();
        table.BorderWidth = 0;
        table.CellPadding = 0;
        table.Width = new Unit(100, UnitType.Percentage);

        foreach (var v in value)
        {
          var box = new TextBox();
          box.Width = new Unit(100, UnitType.Percentage);
          box.BackColor = Warning ? Color.Yellow : Color.White;
          box.Text = v;
          _boxes.Add(box);
          table.AddRow(box);
        }

        var addBox = new TextBox();
        addBox.Width = new Unit(100, UnitType.Percentage);
        addBox.BackColor = Warning ? Color.Yellow : Color.White;
        _boxes.Add(addBox);
        table.AddRow(addBox);

        Controls.Add(table);
      }
    }
  }
}