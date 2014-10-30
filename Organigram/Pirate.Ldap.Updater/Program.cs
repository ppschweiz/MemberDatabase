using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.IO;
using Pirate.Ldap;

namespace Pirate.Ldap.Updater
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      LdapConnection ldap = new LdapConnection();
      ldap.Connect("localhost", 389);
      Console.WriteLine("Connection established.");

      ldap.Bind("uniqueIdentifier=246,dc=members,st=zs,dc=piratenpartei,dc=ch", Console.ReadLine());
      Console.WriteLine("Bind successful.");

      var persons = ldap.Search<Person>("dc=piratenpartei,dc=ch", LdapScope.Sub).ToList();
      Console.WriteLine("Ldap data loaded.");

      var filename = @"D:\Security\PPS\member-update-20141002.csv";
      var lines = File.ReadAllLines(filename);
      Console.WriteLine("Update loaded.");

      foreach (var line in lines)
      {
        var parts = line.Split(new string[] { "," }, StringSplitOptions.None);
        var id = Convert.ToInt32(parts[0]);
        var gn = parts[1];
        var sn = parts[2];
        var postalCode = parts[3];
        var location = parts[4];
        var street = parts[5];

        var person = persons.Where(p => p.Id == id).Single();

        Console.WriteLine("Ldap   Id: " + person.Id + ", Name: " + person.Givenname + " " + person.Surname);
        Console.WriteLine("Ldap   Address: " + person.Street + ", " + person.PostalCode + " " + person.Location);
        Console.WriteLine("Update Id: " + id + ", Name: " + gn + " " + sn);
        Console.WriteLine("Update Address: " + street + ", " + postalCode + " " + location);

        person.Street = street;
        person.PostalCode = postalCode;
        person.Location = location;
        person.Modify(ldap);
        Console.WriteLine("Updated.");
      }

      Console.WriteLine("Done");
      Console.ReadLine();
    }
  }
}
