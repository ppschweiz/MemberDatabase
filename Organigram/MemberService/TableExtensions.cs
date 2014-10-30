using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MemberService
{
  /// <summary>
  /// Extensions to the HTML table.
  /// </summary>
  public static class TableExtensions
  {
    /// <summary>
    /// Adds some vertical space.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="space">The space length in pixels.</param>
    public static void AddVerticalSpace(this Table table, int space)
    {
      var tableRow = new TableRow();
      var tableCell = new TableCell();
      tableCell.Height = space;
      tableRow.Cells.Add(tableCell);
      table.Rows.Add(tableRow);
    }

    /// <summary>
    /// Adds the row with some texts.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="texts">The texts.</param>
    public static void AddRow(this Table table, params string[] texts)
    {
      var list = new List<Control>();

      foreach (var text in texts)
      {
        var label = new Label();
        label.Text = text;
        list.Add(label);
      }

      table.AddRow(list.ToArray());
    }

    /// <summary>
    /// Adds the header row with some texts.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="texts">The texts.</param>
    public static void AddHeaderRow(this Table table, params string[] texts)
    {
      var list = new List<Control>();

      foreach (var text in texts)
      {
        var label = new Label();
        label.Font.Bold = true;
        label.Text = text;
        list.Add(label);
      }

      table.AddRow(list.ToArray());
    }

    /// <summary>
    /// Adds the row where a single cell spans some columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="text">The text.</param>
    /// <param name="columnSpan">The column span.</param>
    public static void AddRow(this Table table, string text, int columnSpan, HorizontalAlign horizontalAlign = HorizontalAlign.Left)
    {
      Label label = new Label();
      label.Text = text;
      table.AddRow(label, columnSpan, horizontalAlign);
    }

    /// <summary>
    /// Adds the row where a single cell spans some columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="text">The text.</param>
    /// <param name="columnSpan">The column span.</param>
    public static void AddHeaderRow(this Table table, string text, int columnSpan, HorizontalAlign horizontalAlign = HorizontalAlign.Left)
    {
      Label label = new Label();
      label.Font.Bold = true;
      label.Text = text;
      table.AddRow(label, columnSpan, horizontalAlign);
    }

    /// <summary>
    /// Adds another cell to the last row.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="text">The text.</param>
    /// <param name="columnSpan">The column span.</param>
    public static void AddHeaderCell(this Table table, string text, int columnSpan, HorizontalAlign horizontalAlign = HorizontalAlign.Left)
    {
      Label label = new Label();
      label.Font.Bold = true;
      label.Text = text;
      table.AddCell(label, columnSpan, horizontalAlign);
    }

    /// <summary>
    /// Adds another cell to the last row.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="text">The text.</param>
    /// <param name="columnSpan">The column span.</param>
    public static void AddCell(this Table table, string text, int columnSpan, HorizontalAlign horizontalAlign = HorizontalAlign.Left)
    {
      Label label = new Label();
      label.Text = text;
      table.AddCell(label, columnSpan, horizontalAlign);
    }

    /// <summary>
    /// Adds the row where a single cell with a control spans some columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="control">The control.</param>
    /// <param name="columnSpan">The column span.</param>
    public static void AddRow(this Table table, Control control, int columnSpan, HorizontalAlign horizontalAlign = HorizontalAlign.Left)
    {
      var tableRow = new TableRow();
      var tableCell = new TableCell();
      tableCell.Controls.Add(control);
      tableCell.ColumnSpan = columnSpan;
      tableCell.HorizontalAlign = horizontalAlign;
      tableRow.Cells.Add(tableCell);
      table.Rows.Add(tableRow);
    }

    /// <summary>
    /// Adds another cell to the last row.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="control">The control.</param>
    /// <param name="columnSpan">The column span.</param>
    public static void AddCell(this Table table, Control control, int columnSpan, HorizontalAlign horizontalAlign = HorizontalAlign.Left)
    {
      var tableCell = new TableCell();
      tableCell.Controls.Add(control);
      tableCell.ColumnSpan = columnSpan;
      tableCell.HorizontalAlign = horizontalAlign;
      table.Rows[table.Rows.Count - 1].Cells.Add(tableCell);
    }

    /// <summary>
    /// Adds the row with a title/header cell and multiple control cells.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="title">The title.</param>
    /// <param name="cells">The cells.</param>
    public static void AddRow(this Table table, string title, params Control[] cells)
    {
      List<Control> controls = new List<Control>();
      var label = new Label();
      label.Text = title;
      controls.Add(label);
      controls.AddRange(cells);
      table.AddRow(controls.ToArray());
    }

    /// <summary>
    /// Adds the row with multiple control cells.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="cells">The cells.</param>
    public static void AddRow(this Table table, params Control[] cells)
    {
      var tableRow = new TableRow();

      foreach (var cell in cells)
      {
        var tableCell = new TableCell();
        tableCell.Controls.Add(cell);
        tableRow.Cells.Add(tableCell);
      }

      table.Rows.Add(tableRow);
    }
  }
}