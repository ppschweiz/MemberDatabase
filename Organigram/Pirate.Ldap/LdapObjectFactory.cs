using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public static class LdapObjectFactory
  {
    public static TObject Create<TObject>(LdapConnection connection, LdapEntry entry)
      where TObject : LdapObject
    {
      if (typeof(TObject) == typeof(Person))
      {
        return new Person(entry) as TObject;
      }
      else if (typeof(TObject) == typeof(Container))
      {
        return new Container(entry) as TObject;
      }
      else if (typeof(TObject) == typeof(Group))
      {
        return new Group(entry) as TObject;
      }
      else if (typeof(TObject) == typeof(Role))
      {
        return new Role(entry) as TObject;
      }
      else if (typeof(TObject) == typeof(Organization))
      {
        return new Organization(entry) as TObject;
      }
      else if (typeof(TObject) == typeof(Section))
      {
        return new Section(entry) as TObject;
      }
      else
      {
        throw new InvalidOperationException("Unknown LDAP object type.");
      }
    }

    public static string ObjectClass<TObject>()
      where TObject : LdapObject
    {
      if (typeof(TObject) == typeof(Person))
      {
        return Attributes.Person.ObjectClass;
      }
      else if (typeof(TObject) == typeof(Container))
      {
        return Attributes.Container.ObjectClass;
      }
      else if (typeof(TObject) == typeof(Group))
      {
        return Attributes.Group.ObjectClass;
      }
      else if (typeof(TObject) == typeof(Role))
      {
        return Attributes.Role.ObjectClass;
      }
      else if (typeof(TObject) == typeof(Organization))
      {
        return Attributes.Organization.ObjectClass;
      }
      else if (typeof(TObject) == typeof(Section))
      {
        return Attributes.Section.ObjectClass;
      }
      else 
      {
        throw new InvalidOperationException("Unknown LDAP object type.");
      }
    }
  }
}
