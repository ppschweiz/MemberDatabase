using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Pirate.Ldap;
using Novell.Directory.Ldap;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace MemberAdmin
{
  public partial class SearchMember
  {
    private string Encode(string value)
    {
      return Encoding.UTF8.GetBytes(value).ToHexString();
    }

    private string Decode(string coded)
    {
      return Encoding.UTF8.GetString(coded.ParseHexBytes());
    }

    private string Serialize(ListBox list)
    {
      var values = new List<string>();

      foreach (ListItem item in list.Items)
      {
        if (item.Selected)
        {
          values.Add(Encode(item.Value));
        }
      }

      return string.Join(",", values.ToArray());
    }

    private void Deserialize(ListBox list, string value)
    {
      var values = value
        .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
        .Select(v => Decode(v));

      foreach (ListItem item in list.Items)
      {
        item.Selected = values.Contains(item.Value);
      }
    }
    
    private string Serialize()
    {
      var values = new Dictionary<string, string>();

      values.Add("_searchDn", Serialize(_searchDn));
      values.Add("_fieldList", Serialize(_fieldList));
      values.Add("_orderByList", Serialize(_orderByList));

      foreach (var field in _multiListFields)
      {
        values.Add(field.Key, Serialize(field.Value));
      }

      foreach (var field in _textFields)
      {
        values.Add(field.Key, Encode(field.Value.Text));
      }

      return string.Join(";", values.Select(v => v.Key + "=" + v.Value).ToArray());
    }

    private void Deserialize(string text)
    {
      var values = new Dictionary<string, string>();

      foreach (var textField in _textFields.Values)
      {
        textField.Text = string.Empty;
      }

      foreach (var multiListField in _multiListFields.Values)
      {
        for (int index = 0; index < multiListField.Items.Count; index++)
        {
          multiListField.Items[index].Selected = true;
        }
      }

      foreach (var v in text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
      {
        var vs = v.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

        if (vs.Length >= 2)
        {
          values.Add(vs[0], vs[1]);
        }
      }

      foreach (var value in values)
      {
        switch (value.Key)
        {
          case "_searchDn":
            Deserialize(_searchDn, value.Value);
            break;
          case "_fieldList":
            Deserialize(_fieldList, value.Value);
            break;
          case "_orderByList":
            Deserialize(_orderByList, value.Value);
            break;
          default:
            if (_multiListFields.Any(f => value.Key == f.Key))
            {
              Deserialize(_multiListFields.Where(f => value.Key == f.Key).Single().Value, value.Value);
            }
            else if (_textFields.Any(f => value.Key == f.Key))
            {
              _textFields.Where(f => value.Key == f.Key).Single().Value.Text = Decode(value.Value);
            }
            break;
        }
      }
    }
  }
}