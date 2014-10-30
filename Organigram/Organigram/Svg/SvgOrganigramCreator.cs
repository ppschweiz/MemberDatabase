using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirate.Ldap.Organigram.Svg;
using System.IO;
using System.Windows.Forms;

namespace Pirate.Ldap.Organigram
{
  public class SvgOrganigramCreator : IOrganigramCreator 
  {
    private string _folder;

    public void Create(Organization organization)
    {
      _folder = Path.Combine(Application.StartupPath, "org");

      var document = new Document();

      var rect = new Rect("x", 10, 10, 50, 50);
      document.Add(rect);
      var text = new Text("y", "Hello World", 20, 20);
      document.Add(text);

      var context = new Context();
      document.Render(context);

      File.WriteAllText(Path.Combine(_folder, "output.svg"), context.Text.ToString());
    }
  }
}
