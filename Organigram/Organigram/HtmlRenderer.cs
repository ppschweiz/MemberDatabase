using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Novell.Directory.Ldap;

namespace Pirate.Ldap.Organigram
{
  public class HtmlRenderer : IRenderer
  {
    private LdapConnection _connection;

    private DrawingContext _context;

    private string _folder;

    public HtmlRenderer(LdapConnection connection)
    {
      _connection = connection;
    }

    public void Create(Organization organization)
    {
      _folder = Path.Combine(Application.StartupPath, "org");

      if (!Directory.Exists(_folder))
      {
        Directory.CreateDirectory(_folder);
      }

      _context = new DrawingContext();

      var div = CreateOrganization(organization);
      div.Render(_context);

      string text = File.ReadAllText("template.html");
      text = text.Replace("$$css$$", _context.Css.ToString());
      text = text.Replace("$$body$$", _context.Body.ToString());
      File.WriteAllText(Path.Combine(_folder, "output.html"), text);
    }

    private string Link(string name, string target)
    {
      return string.Format("<a href=\"{0}\">{1}</a>", target, target);
    }

    private string Mail(string name, string address)
    {
      return Link(name, "mailto:" + address);
    }

    private Div CreateOrganization(Organization organization)
    {
      var masterDiv = new VerticalDiv(string.Empty);
      masterDiv.BackColor = Color.White;

      string title = organization.DisplayNameGerman;

      if (organization.Photo != null)
      {
        var filename = _context.NewId + ".png";
        organization.Photo.Save(Path.Combine(_folder, filename));
        title = string.Format("<img height=\"60px\" src=\"{0}\"/>", filename);
      }

      var div = new FlowDiv(1100, title);
      div.Url = organization.Website;
      div.MinimalHeaderHeight = organization.Photo == null ? 0 : 65;
      div.BackColor = Color.FromArgb(249, 178, 0);
      div.ForeColor = Color.Black;

      masterDiv.Add(div);

      foreach (var group in organization.Groups(_connection).Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(group, 0));
      }

      foreach (var group in organization.Containers(_connection).SelectMany(c => c.Groups(_connection)).Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(group, 0));
      }

      foreach (var section in organization.Sections(_connection).Where(s => s.Ordering >= 1).OrderBy(s => s.Ordering))
      {
        masterDiv.Add(CreateSection(section));
      }

      return masterDiv;
    }

    private Div CreateSection(Section section)
    {
      string title = section.DisplayNameGerman;

      if (section.Photo != null)
      {
        var filename = _context.NewId + ".png";
        section.Photo.Save(Path.Combine(_folder, filename));
        title = string.Format("<img height=\"60px\" src=\"{0}\"/>", filename);
      } 
      
      var div = new FlowDiv(1100, title);
      div.Url = section.Website;
      div.MinimalHeaderHeight = section.Photo == null ? 0 : 65;
      div.BackColor = Color.FromArgb(249, 178, 0);
      div.ForeColor = Color.Black;

      foreach (var group in section.Groups(_connection).Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(group, 0));
      }

      foreach (var group in section.Containers(_connection).SelectMany(c => c.Groups(_connection)).Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(group, 0));
      }

      return div;
    }

    private Div CreateGroup(Group group, int layer)
    {
      var div = new HorizontalDiv(group.DisplayNameGerman, 1090);
      div.Url = group.Website;
      div.MinimalWidth = 100;

      switch (layer)
      {
        case 0:
          div.BackColor = Color.Black;
          div.ForeColor = Color.White;
          break;
        case 1:
          div.BackColor = Color.FromArgb(255, 85, 59, 115);
          div.ForeColor = Color.White;
          break;
        default:
          div.BackColor = Color.FromArgb(255, 52, 116, 82);
          div.ForeColor = Color.Black;
          break;
      }

      Dictionary<string, Person> members = new Dictionary<string, Person>();

      foreach (var role in group.Roles(_connection).Where(r => r.Ordering >= 1).OrderBy(r => r.Ordering))
      {
        var occupants = role.Occupants(_connection);

        if (occupants.Count() < 1)
        {
          div.Add(CreatePerson(null, role));
        }

        foreach (var person in occupants)
        {
          div.Add(CreatePerson(person, role));

          if (members.ContainsKey(person.DN))
          {
            members.Remove(person.DN);
          }
        }
      }

      foreach (var person in members.Values)
      {
        div.Add(CreatePerson(person, null));
      }

      foreach (var subgroup in group.Groups(_connection).Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(subgroup, layer + 1));
      }

      return div;
    }

    private Div CreatePerson(Person person, Role role)
    {
      string name = "Vakant";
      string position = string.Empty;
      string photo = "vacant.png";

      if (person != null)
      { 
        name = person.Name;

        string email = person.Emails
          .Where(e =>
            e.EndsWith("piratenpartei.ch") ||
            e.EndsWith("partipirate.ch") ||
            e.EndsWith("partitopirata.ch") ||
            e.EndsWith("pirateparty.ch"))
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(email))
        {
          name = string.Format("<a href=\"mailto:{0}\">{1}</a>", email, name);
        }

        if (person.Photo != null)
        {
          var filename = _context.NewId + ".jpg";
          person.Photo.Save(Path.Combine(_folder, filename));
          photo = filename;
        }
        else
        {
          photo = "nophoto.png";
        }
      }

      if (role != null)
      {
        position = role.DisplayNameGerman;
      }

      string text = string.Format("<div id=\"photo\"><img height=\"120px\" src=\"{0}\"></div><div id=\"text\">{1}<br>{2}</div>", photo, name, position);
      var div = new BaseDiv(140, 180, text);
      div.BackColor = Color.FromArgb(249, 178, 0);
      div.ForeColor = Color.Black;
      return div;
    }
  }
}
