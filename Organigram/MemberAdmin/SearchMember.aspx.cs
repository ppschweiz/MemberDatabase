using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Novell.Directory.Ldap;
using Pirate.Ldap;
using Pirate.Ldap.Data;
using Pirate.Ldap.Web;

namespace MemberAdmin
{
  public partial class SearchMember : CustomPage
  {
    protected override string PageName
    {
      get { return "SearchMember"; }
    }
    
    private Button _searchButton;

    private Button _exportButton;

    private DropDownList _listTemplate;

    private ListBox _searchDn;

    private Dictionary<LdapAttributeBase, TextBox> _textFields;

    private Dictionary<LdapAttributeBase, ListBox> _multiListFields;

    private int _multiListCounter = 1;

    private ListBox _fieldList;

    private ListBox _orderByList;

    private TextBox _templateName;

    private ListItem ListItemSelected(string text, string value)
    {
      var item = new ListItem(text, value);
      item.Selected = true;
      return item;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!SetupLdap())
      {
        RedirectLogin();
        return;
      }

      var table = new Table();

      _textFields = new Dictionary<LdapAttributeBase, TextBox>();
      _multiListFields = new Dictionary<LdapAttributeBase, ListBox>();

      table.AddRow(
        ToLabel(Resources.SearchFilter, true),
        ToLink(Resources.HelpText, Resources.HelpUrlSearch));

      _searchDn = new ListBox();
      _searchDn.ID = "sdn";
      _searchDn.SelectionMode = ListSelectionMode.Multiple;
      _searchDn.Width = new Unit(500, UnitType.Pixel);
      _searchDn.Items.Add(new ListItem(Resources.SectionAll, Names.Party.Replace(",", ";")));
      _searchDn.Items[_searchDn.Items.Count - 1].Selected = true;
      _searchDn.Items.Add(new ListItem(Resources.SectionPeople, Names.People.Replace(",", ";")));
      _searchDn.Items[_searchDn.Items.Count - 1].Selected = true;
      _searchDn.Items.Add(new ListItem(Resources.SectionMembers, Names.Members.Replace(",", ";")));
      _searchDn.Items[_searchDn.Items.Count - 1].Selected = true;

      var sections = Connection.Search<Section>(Names.Party, LdapScope.Sub);

      foreach (var section in sections.OrderBy(s => s.OrderingAbsolute))
      {
        _searchDn.Items.Add(new ListItem(section.DisplayNameGerman, section.DN.Replace(",", ";")));
        _searchDn.Items[_searchDn.Items.Count - 1].Selected = true;
      }

      table.AddRow(LdapResources.Section, _searchDn);

      AddMultiList(table, EmployeeTypeExtensions.Values, LdapResources.EmployeeType, Attributes.Person.EmployeeType);
      AddText(table, LdapResources.UniqueIdentifier, Attributes.Person.UniqueIdentifier);
      AddText(table, LdapResources.UserId, Attributes.Person.UserId);
      AddText(table, LdapResources.CommonName, Attributes.Person.CommonName);
      AddText(table, LdapResources.Surname, Attributes.Person.Surname);
      AddText(table, LdapResources.Givenname, Attributes.Person.Givenname);
      AddText(table, LdapResources.Email, Attributes.Person.Mail);
      AddText(table, LdapResources.AlternateMail, Attributes.Person.AlternateMail);

      var binDir = Path.Combine(Request.PhysicalApplicationPath, "bin");
      AddMultiList(table, Countries.GetList(binDir), LdapResources.Country, Attributes.Person.Country);
      AddMultiList(table, States.GetList(binDir), LdapResources.State, Attributes.Person.State);

      AddText(table, LdapResources.PostalCode, Attributes.Person.PostalCode);
      AddText(table, LdapResources.Location, Attributes.Person.Location);
      AddText(table, LdapResources.Street, Attributes.Person.Street);
      AddMultiList(table, Language.Values, LdapResources.PreferredLanguage, Attributes.Person.PreferredLanguage);
      AddMultiList(table, NotificationMethod.Values, LdapResources.PreferredNotificationMethod, Attributes.Person.PreferredNotificationMethod);

      table.AddVerticalSpace(10);
      table.AddRow(
        ToLabel(Resources.SearchOutput, true),
        ToLink(Resources.HelpText, Resources.HelpUrlSearch));

      _fieldList = new ListBox();
      _fieldList.ID = "atr";
      _fieldList.SelectionMode = ListSelectionMode.Multiple;
      var prototype = new Person(Names.Party, 999999, "Prototype");
      _fieldList.Items.Add(new ListItem(Resources.AllItems, string.Empty));

      foreach (var field in prototype.Fields)
      {
        var item = new ListItem(field.Attribute.Text(), field.Attribute);
        item.Selected = field.DefaultDisplay;
        _fieldList.Items.Add(item);
      }

      table.AddRow(Resources.Attributes, _fieldList);

      _orderByList = new ListBox();
      _orderByList.ID = "ord";

      foreach (var field in prototype.Fields)
      {
        _orderByList.Items.Add(new ListItem(field.Attribute.Text(), field.Attribute));
      }

      _orderByList.SelectedIndex = 1;
      table.AddRow(Resources.Ordering, _orderByList);

      table.AddVerticalSpace(10);

      var buttonPanel = new Panel();

      _searchButton = new Button();
      _searchButton.Text = Resources.Search;
      _searchButton.Click += new EventHandler(SearchButton_Click);
      buttonPanel.Controls.Add(_searchButton);

      _exportButton = new Button();
      _exportButton.Text = Resources.Export;
      _exportButton.Click += new EventHandler(ExportButton_Click);
      buttonPanel.Controls.Add(_exportButton);

      table.AddRow(string.Empty, buttonPanel);

      table.AddVerticalSpace(10);

      table.AddRow(
        ToLabel(Resources.Templates, true));

      _listTemplate = new DropDownList();
      _listTemplate.ID = "lt";
      _listTemplate.Width = new Unit(420, UnitType.Pixel);

      var templates = DataGlobal.DataAccess.ListSearchTemplates();

      foreach (var template in templates)
      {
        _listTemplate.Items.Add(new ListItem(template.Name, template.Definition));
      }

      var templateLoadButton = new Button();
      templateLoadButton.Width = new Unit(80, UnitType.Pixel);
      templateLoadButton.Text = Resources.Load;
      templateLoadButton.Click += new EventHandler(TemplateLoadButton_Click);

      table.AddRow(Resources.LoadTemplate, Combine(_listTemplate, templateLoadButton));

      _templateName = new TextBox();
      _templateName.Width = new Unit(420, UnitType.Pixel);

      var templateSaveButton = new Button();
      templateSaveButton.Text = Resources.Save;
      templateSaveButton.Width = new Unit(80, UnitType.Pixel);
      templateSaveButton.Click += new EventHandler(TemplateSaveButton_Click);

      table.AddRow(Resources.SaveTemplate, Combine(_templateName, templateSaveButton));

      this.panel.Controls.Add(table);

      this.form1.DefaultButton = _searchButton.ID;
    }

    private void TemplateSaveButton_Click(object sender, EventArgs e)
    {
      DataGlobal.DataAccess.SaveSearchTemplate(new SearchTemplate(_templateName.Text, Serialize()));
      RedirectSelf();
    }

    private void TemplateLoadButton_Click(object sender, EventArgs e)
    {
      if (_listTemplate.SelectedIndex >= 0)
      {
        Deserialize(_listTemplate.SelectedValue);
      }
    }

    private void ExportButton_Click(object sender, EventArgs e)
    {
      Session[ListMember.SearchParameters] = GetParameters();
      Redirect("OutputMemberList.csv");
    }

    private void AddMultiList(Table table, IEnumerable<DisplayValue> items, string title, LdapAttributeBase attribute)
    {
      var multiList = new ListBox();
      multiList.ID = "ml" + _multiListCounter;
      _multiListCounter++;
      multiList.Width = new Unit(500, UnitType.Pixel);
      multiList.SelectionMode = ListSelectionMode.Multiple;

      multiList.Items.Add(new ListItem(Resources.AllItems, string.Empty));
      multiList.Items[multiList.Items.Count - 1].Selected = true;

      foreach (var item in items)
      {
        multiList.Items.Add(new ListItem(item.Display, item.Value));
        multiList.Items[multiList.Items.Count - 1].Selected = true;
      }

      table.AddRow(title, multiList);
      _multiListFields.Add(attribute, multiList);
    }

    private void AddText(Table table, string title, LdapAttributeBase attribute)
    {
      TextBox textBox = new TextBox();
      textBox.CssClass = "ui-widget-content";
      textBox.Width = new Unit(500, UnitType.Pixel);
      table.AddRow(title, textBox);
      _textFields.Add(attribute, textBox);
    }

    private void SearchButton_Click(object sender, EventArgs e)
    {
      Session[ListMember.SearchParameters] = GetParameters();
      Redirect("ListMember.aspx");
    }

    private SearchParameters GetParameters()
    {
      var andFilters = new List<LdapFilter>();
      var postFilters = new List<PostFilter>();

      foreach (var textField in _textFields)
      {
        var field = textField.Key;
        var text = textField.Value.Text;

        if (!string.IsNullOrEmpty(text))
        {
          bool negation = text.StartsWith("!");

          if (negation)
          {
            text = text.Substring(1);
          }

          var numericGreaterMatch = Regex.Match(text, "^>([0-9]+)$");
          var numericSmallerMatch = Regex.Match(text, "^<([0-9]+)$");
          var numericGreaterOrEqualMatch = Regex.Match(text, "^>=([0-9]+)$");
          var numericSmallerOrEqualMatch = Regex.Match(text, "^<=([0-9]+)$");
          var numericRangeMatch = Regex.Match(text, "^([0-9]+)\\-([0-9]+)$");
          var regexMatch = Regex.Match(text, "\\^.+\\$");
          var stringListMatch = Regex.Match(text, "^\".+?\"(,\".+?\")+$");
          var stringGreaterMatch = Regex.Match(text, "^>\"(.+)\"$");
          var stringSmallerMatch = Regex.Match(text, "^<\"(.+)\"$");
          var stringGreaterOrEqualMatch = Regex.Match(text, "^>=\"(.+)\"$");
          var stringSmallerOrEqualMatch = Regex.Match(text, "^<=\"(.+)\"$");
          var stringRangeMatch = Regex.Match(text, "^\"(.+)\"\\-\"(.+)\"$");

          if (numericGreaterMatch.Success)
          {
            var number = numericGreaterMatch.Groups[1].Value.ToNumber();
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " > " + number, 
              x =>
              {
                return x.GetStringValue(field, MultiValueOutput.FirstOnly).IsGreaterNumber(number) ^ negation;
              }
            ));
          }
          else if (numericSmallerMatch.Success)
          {
            var number = numericSmallerMatch.Groups[1].Value.ToNumber();
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " < " + number, 
              x =>
              {
                return x.GetStringValue(field, MultiValueOutput.FirstOnly).IsSmallerNumber(number) ^ negation;
              }
            ));
          }
          else if (numericGreaterOrEqualMatch.Success)
          {
            var number = numericGreaterOrEqualMatch.Groups[1].Value.ToNumber();
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " >= " + number, 
              x =>
              {
                return x.GetStringValue(field, MultiValueOutput.FirstOnly).IsGreaterOrEqualNumber(number) ^ negation;
              }
            ));
          }
          else if (numericSmallerOrEqualMatch.Success)
          {
            var number = numericSmallerOrEqualMatch.Groups[1].Value.ToNumber();
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " <= " + number, 
              x =>
              {
                return x.GetStringValue(field, MultiValueOutput.FirstOnly).IsSmallerOEqualNumber(number) ^ negation;
              }
            ));
          }
          else if (numericRangeMatch.Success)
          {
            var low = numericRangeMatch.Groups[1].Value.ToNumber();
            var high = numericRangeMatch.Groups[2].Value.ToNumber();
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " between " + low + " and " + high, 
              x =>
              {
                return x.GetStringValue(field, MultiValueOutput.FirstOnly).IsInNumberRange(low, high) ^ negation;
              }
            ));
          }
          else if (regexMatch.Success)
          {
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " matches regex " + text,
              x =>
              {
                return Regex.IsMatch(x.GetStringValue(field, MultiValueOutput.Separated), text) ^ negation;
              }
            ));
          }
          else if (stringListMatch.Success)
          {
            var texts = text.Substring(1, text.Length - 2).Split(new[] { "\",\"" }, StringSplitOptions.RemoveEmptyEntries);

            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " contains one of " + string.Join(", ", texts),
              x =>
              {
                return texts.Contains(x.GetStringValue(field, MultiValueOutput.Separated)) ^ negation;
              }
            ));
          }
          else if (stringGreaterMatch.Success)
          {
            var value = stringGreaterMatch.Groups[1].Value;
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " is lexically greater than " + value,
              x =>
              {
                return (string.Compare(x.GetStringValue(field, MultiValueOutput.Separated), value, true) > 0) ^ negation;
              }
            ));
          }
          else if (stringGreaterOrEqualMatch.Success)
          {
            var value = stringGreaterOrEqualMatch.Groups[1].Value;
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " is lexically greater or equal to " + value,
              x =>
              {
                return (string.Compare(x.GetStringValue(field, MultiValueOutput.Separated), value, true) >= 0) ^ negation;
              }
            ));
          }
          else if (stringSmallerMatch.Success)
          {
            var value = stringSmallerMatch.Groups[1].Value;
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " is lexically smaller than " + value,
              x =>
              {
                return (string.Compare(x.GetStringValue(field, MultiValueOutput.Separated), value, true) < 0) ^ negation;
              }
            ));
          }
          else if (stringSmallerOrEqualMatch.Success)
          {
            var value = stringSmallerOrEqualMatch.Groups[1].Value;
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " is lexically smaller or equal to " + value,
              x =>
              {
                return (string.Compare(x.GetStringValue(field, MultiValueOutput.Separated), value, true) <= 0) ^ negation;
              }
            ));
          }
          else if (stringRangeMatch.Success)
          {
            var low = stringRangeMatch.Groups[1].Value;
            var high = stringRangeMatch.Groups[2].Value;
            postFilters.Add(new PostFilter(
              (negation ? "NOT " : string.Empty) + field.Text() + " is lexically between " + low + " and " + high,
              x =>
              {
                return
                  (string.Compare(x.GetStringValue(field, MultiValueOutput.Separated), low, true) >= 0 &&
                  string.Compare(x.GetStringValue(field, MultiValueOutput.Separated), high, true) <= 0) ^ negation;
              }
            ));
          }
          else
          {
            if (negation)
            {
              andFilters.Add(new LdapNotFilter(new LdapAttributeFilter(textField.Key, text)));
            }
            else
            {
              andFilters.Add(new LdapAttributeFilter(textField.Key, text));
            }
          }
        }
      }

      foreach (var multiList in _multiListFields)
      {
        var orFilters = new List<LdapFilter>();
        var selectedValues = multiList.Value.GetSelectedValues(Request);

        foreach (var selectedValue in selectedValues)
        {
          if (selectedValue == Attributes.Values.Unspecified)
          {
            orFilters.Add(new LdapAttributeFilter(multiList.Key, string.Empty));
          }
          else
          {
            orFilters.Add(new LdapAttributeFilter(multiList.Key, selectedValue));
          }
        }

        if (orFilters.Count < 1)
        {
          andFilters.Add(new LdapAttributeFilter(multiList.Key, int.MaxValue));
        }
        else if (multiList.Value.Items.Count > orFilters.Count + 1)
        {
          andFilters.Add(LdapOrFilter.Multiple(orFilters));
        }
      }

      var attributes = new List<LdapAttributeBase>();

      foreach (var item in _fieldList.GetSelectedValues(Request))
      {
        var attribute = CurrentUser.Fields
          .Select(f => f.Attribute)
          .Where(a => (string)a == item)
          .Single();
        attributes.Add(attribute);
      }

      var searchDns = _searchDn
        .GetSelectedValues(Request)
        .Select(dn => dn.Replace(";", ","));

      if (searchDns.Contains(Names.Party))
      {
        searchDns = new[] { Names.Party };
      }

      var orderByAttribute = CurrentUser.Fields
        .Select(f => f.Attribute)
        .Where(a => (string)a == _orderByList.SelectedValue)
        .Single();

      return new SearchParameters(
        searchDns,
        LdapAndFilter.Multiple(andFilters),
        attributes,
        postFilters,
        p => p.GetField(orderByAttribute).CompareObject());
    }
  }
}