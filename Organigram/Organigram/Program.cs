using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.IO;
using System.Windows.Forms;

namespace Pirate.Ldap.Organigram
{
  class Program
  {
    static void Main(string[] args)
    {
      var configFileName = Path.Combine(Application.StartupPath, "ldap.cfg");

      if (!File.Exists(configFileName))
      {
        Console.WriteLine("Missing ldap.cfg");
        return;
      }

      var lines = File.ReadAllLines(configFileName);

      if (lines.Length < 3)
      {
        Console.WriteLine("No enough lines in ldap.cfg");
        return;
      }

      var serverAddress = lines[0];
      Console.WriteLine("Server: " + serverAddress);

      var rootdn = lines[1];
      Console.WriteLine("Root DN: " + rootdn);

      var userdn = lines[2];
      Console.WriteLine("User DN: " + userdn);

      string password = null;
      
      if (lines.Length >= 4)
      {
        password = lines[3];
      }
      else
      {
        Console.Write("Password: ");
        password = Console.ReadLine();
      }

      LdapConnection ldap = new LdapConnection();
      ldap.Connect(serverAddress, 389);
      Console.WriteLine("Connection established.");

      Console.WriteLine(ldap.Connected);
      ldap.Bind(userdn, password);
      Console.WriteLine("Bind successful.");

      var org = ldap.SearchFirst<Organization>(rootdn);
      Console.WriteLine("Organization found.");

      Console.Write("Creating organigram...");
      var creator = new CssBasedRenderer(ldap);
      creator.Create(org);
      Console.WriteLine("Done");

      ldap.Disconnect();
      Console.WriteLine("Disconnected.");
    }
  }
}
