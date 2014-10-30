using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Novell.Directory.Ldap;

namespace Pirate.Ldap.Organigram
{
  public class CssBasedRenderer : IRenderer
  {
    private LdapConnection _connection;

    private string _folder;

    private List<string> _images;

    private StringBuilder _body;

    public CssBasedRenderer(LdapConnection connection)
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

      _images = new List<string>();
      _body = new StringBuilder();
      CreateOrganzation(organization);

      string text = File.ReadAllText("cssbased.html");
      text = text.Replace("$$CONTENTS$$", _body.ToString());
      File.WriteAllText(Path.Combine(_folder, "output.html"), text);
    }

    public string Photo(Image image, string id, int height)
    {
      if (image != null)
      {
        string imageUrl = id + ".png";

        if (!_images.Contains(id))
        {
          int width = (int)Math.Round((double)image.Width * (1d / (double)image.Height * (double)height));
          image = DoctaJonez.Drawing.Imaging.ImageUtilities.ResizeImage(image, width, height);
          DoctaJonez.Drawing.Imaging.ImageUtilities.SaveJpeg(Path.Combine(_folder, imageUrl), image, 100);
          _images.Add(id);
        }

        return Image(imageUrl, "photo");
      }
      else
      {
        string imageUrl = "nophoto.png";
        return Image(imageUrl, "photo");
      }
    }

    private string Image(string imageUrl, string cssClass)
    {
      return string.Format("<img src=\"{0}\" class=\"{1}\">", imageUrl, cssClass);
    }

    private string Link(string name, string target, string cssClass)
    {
      return string.Format("<a href=\"{0}\" class=\"{2}\">{1}</a>", target, name, cssClass);
    }

    private string Mail(string name, string address, string cssClass)
    {
      return Link(name, "mailto:" + address, cssClass);
    }

    private void CreateOrganzation(Organization organization)
    { 
      string name = organization.DisplayNameGerman;

      if (organization.Photo != null)
      {
        name = Photo(organization.Photo, organization.Name, 80);
      }

      if (!string.IsNullOrEmpty(organization.Website))
      {
        name = Link(name, organization.Website, "url");
      }

      _body.AppendLine("<div class=\"page-header\">");
      _body.AppendLine("<h2>{0}</h2>", name);
	    _body.AppendLine("</div>");

      foreach (var group in organization.Groups(_connection)
        .SelectMany(group => group.Groups(_connection).Concat(new Group[] { group }))
        .Where(group => group.Ordering >= 1)
        .OrderBy(group => group.Ordering))
      {
        CreateGroup(group);
      }

      foreach (var group in organization.Containers(_connection)
        .SelectMany(c => c.Groups(_connection))
        .Where(group => group.Ordering >= 1)
        .OrderBy(group => group.Ordering))
      {
        CreateGroup(group);
      }

      foreach (var section in organization.Sections(_connection)
        .Where(section => section.Ordering >= 1)
        .OrderBy(section => section.Ordering))
      {
        CreateSection(section);
      }
    }

    private void CreateSection(Section section)
    {
      string name = section.DisplayNameGerman;

      if (section.Photo != null)
      {
        name = Photo(section.Photo, section.State, 80);
      }

      if (!string.IsNullOrEmpty(section.Website))
      {
        name = Link(name, section.Website, "url");
      }

      _body.AppendLine("<div class=\"page-header\">");
      _body.AppendLine("<h2>{0}</h2>", name);
      _body.AppendLine("</div>");

      foreach (var group in section.Groups(_connection)
        .Where(group => group.Ordering >= 1)
        .OrderBy(group => group.Ordering))
      {
        CreateGroup(group);
      }
    }

    private void CreateGroup(Group group)
    {
      string name = group.DisplayNameGerman;

      if (!string.IsNullOrEmpty(group.Website))
      {
        name = Link(name, group.Website, "url");
      }

      _body.AppendLine("<h2>{0}</h2>", name);

      _body.AppendLine("<div class=\"row\">");

      foreach (var role in group.Roles(_connection)
        .Where(role => role.Ordering >= 1)
        .OrderBy(role => role.Ordering))
      {
        foreach (var person in role.Occupants(_connection)
          .OrderBy(person => person.Name))
        {
          CreatePerson(role, person);
        }

        if (role.Occupants(_connection).Count() == 0)
        {
          CreateVacancy(role);
        }
      }

      _body.AppendLine("</div>");
    }
    
    private void CreatePerson(Role role, Person person)
    {
      _body.AppendLine("<div class=\"portrait span2\">");
      _body.AppendLine(Photo(person.Photo, person.Id.ToString(), 120));

      string name = person.Name;

      string email = person.Emails
        .Where(e =>
          e.EndsWith("piratenpartei.ch") ||
          e.EndsWith("partipirate.ch") ||
          e.EndsWith("partitopirata.ch") ||
          e.EndsWith("pirateparty.ch"))
          .FirstOrDefault();

      if (!string.IsNullOrEmpty(email))
      {
        _body.AppendLine(Mail(name, email, "name"));
      }
      else
      {
        _body.AppendLine(string.Format("<span class=\"name\">{0}</span>", name));
      }

      _body.AppendLine(string.Format("<span class=\"role\">{0}</span>", role.DisplayNameGerman));
      _body.AppendLine("</div>");
    }

    private void CreateVacancy(Role role)
    {
      _body.AppendLine("<div class=\"portrait span2\">");
      _body.AppendLine(Image("vacant.png", "photo"));
      _body.AppendLine(string.Format("<span class=\"name\">Vakant</span>"));
      _body.AppendLine(string.Format("<span class=\"role\">{0}</span>", role.DisplayNameGerman));
      _body.AppendLine("</div>");
    }
  }
}
