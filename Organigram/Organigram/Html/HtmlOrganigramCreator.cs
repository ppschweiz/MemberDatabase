using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pirate.Ldap.Organigram
{
  public class HtmlOrganigramCreator : Pirate.Ldap.Organigram.IOrganigramCreator
  {
    private DrawingContext _context;

    private string _folder;

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

    private Div CreateOrganization(Organization organization)
    {
      var masterDiv = new VerticalDiv(string.Empty);
      masterDiv.BackColor = Color.White;

      var div = new FlowDiv(1100, organization.Name);
      div.BackColor = Color.FromArgb(255, 220, 0);
      div.ForeColor = Color.Black;

      masterDiv.Add(div);

      foreach (var group in organization.Groups.Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(group));
      }

      foreach (var group in organization.Containers.SelectMany(c => c.Groups).Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(group));
      }

      foreach (var section in organization.Sections.Where(s => s.Ordering >= 1).OrderBy(s => s.Ordering))
      {
        masterDiv.Add(CreateSection(section));
      }

      return masterDiv;
    }

    private Div CreateSection(Section section)
    {
      var div = new FlowDiv(1100, section.DisplayNameGerman);
      div.BackColor = Color.FromArgb(255, 220, 0);
      div.ForeColor = Color.Black;

      foreach (var group in section.Groups.Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(group));
      }

      foreach (var group in section.Containers.SelectMany(c => c.Groups).Where(g => g.Ordering >= 1).OrderBy(g => g.Ordering))
      {
        div.Add(CreateGroup(group));
      }

      return div;
    }

    private Div CreateGroup(Group group)
    {
      var div = new HorizontalDiv(group.DisplayNameGerman);
      div.Url = group.Website;
      div.MinimalWidth = 100;
      div.BackColor = Color.Black;
      div.ForeColor = Color.White;

      Dictionary<string, Person> members = new Dictionary<string, Person>();

      ////foreach (var person in group.Members)
      ////{
      ////  members.Add(person.DN, person);
      ////}

      foreach (var role in group.Roles.Where(r => r.Ordering >= 1).OrderBy(r => r.Ordering))
      {
        var occupants = role.Occupants;

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
