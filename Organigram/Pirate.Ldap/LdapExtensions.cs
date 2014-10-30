using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Pirate.Ldap
{
  public static class LdapExtensions
  {
    private const int MaxResults = 1000000;

    public static IEnumerable<DateTime> GetAttributeDateTimeValues(this LdapEntry entry, string attributeName)
    {
      foreach (var data in entry.GetAttributeStringValues(attributeName))
      {
        yield return ParseDataTime(data);
      }
    }

    public static IEnumerable<string> GetAttributeStringValues(this LdapEntry entry, string attributeName)
    {
      var attribute = entry.getAttribute(attributeName);

      if (attribute != null)
      {
        var values = attribute.StringValues;

        while (values.MoveNext())
        {
          yield return values.Current as string;
        }
      }
    }

    public static IEnumerable<int> GetAttributeIntegerValues(this LdapEntry entry, string attributeName)
    {
      var attribute = entry.getAttribute(attributeName);

      if (attribute != null)
      {
        var values = attribute.StringValues;

        while (values.MoveNext())
        {
          yield return Convert.ToInt32((string)values.Current);
        }
      }
    }

    public static IEnumerable<byte[]> GetAttributeBytesValues(this LdapEntry entry, string attributeName)
    {
      var attribute = entry.getAttribute(attributeName);

      if (attribute != null)
      {
        var values = attribute.ByteValues;

        while (values.MoveNext())
        {
          var sbytes = values.Current as sbyte[];
          byte[] bytes = new byte[sbytes.Length];
          Buffer.BlockCopy(sbytes, 0, bytes, 0, sbytes.Length);
          yield return bytes;
        }
      }
    }

    public static int? GetAttributeIntegerValue(this LdapEntry entry, string attributeName, bool required = false)
    {
      var attribute = entry.getAttribute(attributeName);

      if (attribute != null)
      {
        return Convert.ToInt32(attribute.StringValue);
      }
      else
      {
        if (required)
        {
          throw new ArgumentException("Attribute not present.");
        }
        else
        {
          return null;
        }
      }
    }

    private static DateTime ParseDataTime(string data)
    {
      return DateTime.ParseExact
        (data,
        new[] { "yyyyMMddHHmmssK", "yyyyMMddHHmmK" }, 
        CultureInfo.InvariantCulture,
        DateTimeStyles.AdjustToUniversal);
    }

    public static DateTime? GetAttributeDateTimeValue(this LdapEntry entry, string attributeName, bool required = false)
    {
      string data = entry.GetAttributeStringValue(attributeName, required);

      if (string.IsNullOrEmpty(data))
      {
        return null;
      }
      else
      {
        return ParseDataTime(data);
      }
    }

    public static string GetAttributeStringValue(this LdapEntry entry, string attributeName, bool required = false)
    {
      var attribute = entry.getAttribute(attributeName);

      if (attribute != null)
      {
        return attribute.StringValue;
      }
      else
      {
        if (required)
        {
          throw new ArgumentException("Attribute not present.");
        }
        else
        {
          return string.Empty;
        }
      }
    }

    public static Image GetAttributeJpegValue(this LdapEntry entry, string attributeName, bool required = false)
    {
      var bytes = entry.GetAttributeBytesValue(attributeName, required);

      if (bytes == null)
      {
        return null;
      }
      else
      {
        return Image.FromStream(new MemoryStream(bytes));
      }
    }

    public static byte[] GetAttributeBytesValue(this LdapEntry entry, string attributeName, bool required = false)
    {
      var attribute = entry.getAttribute(attributeName);

      if (attribute != null)
      {
        var sbytes = attribute.ByteValue;
        byte[] bytes = new byte[sbytes.Length];
        Buffer.BlockCopy(sbytes, 0, bytes, 0, sbytes.Length);
        return bytes;
      }
      else
      {
        if (required)
        {
          throw new ArgumentException("Attribute not present.");
        }
        else
        {
          return null;
        }
      }
    }

    private static string GetPassword(this LdapConnection connection, string dn)
    {
      var result = connection.Search(dn, (int)LdapScope.Base, string.Empty, new[] { (string)Attributes.Person.Password }, false);

      if (result.hasMore())
      {
        var entry = result.next();
        return entry.GetAttributeStringValue(Attributes.Person.Password, false);
      }
      else
      {
        return null;
      }
    }

    private static byte[] GenerateSalt(int length)
    {
      var saltBytes = new byte[length];
      System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(saltBytes);
      return saltBytes;
    }

    private static byte[] ComputeSha(byte[] input)
    {
      using (var sha = new System.Security.Cryptography.SHA1Managed())
      {
        return sha.ComputeHash(input);
      }
    }

    public static void SetPassword(this LdapConnection connection, string dn, string password)
    {
      var oldPassword = GetPassword(connection, dn);
      var passwordBytes = Encoding.ASCII.GetBytes(password);
      var saltBytes = GenerateSalt(4);
      var hashBytes = ComputeSha(passwordBytes.Concat(saltBytes));
      var passwordData = "{SSHA}" + Convert.ToBase64String(hashBytes.Concat(saltBytes));

      if (string.IsNullOrEmpty(oldPassword))
      {
        var modification = new LdapModification(LdapModification.ADD, new LdapAttribute(Attributes.Person.Password, passwordData));
        connection.Modify(dn, modification);
      }
      else
      {
        var modification = new LdapModification(LdapModification.REPLACE, new LdapAttribute(Attributes.Person.Password, passwordData));
        connection.Modify(dn, modification);
      }
    }

    public static TObject SearchFirst<TObject>(this LdapConnection connection, string dn, LdapScope scope = LdapScope.Base, LdapFilter filter = null)
      where TObject : LdapObject
    {
      return connection.Search<TObject>(dn, scope, filter).FirstOrDefault();
    }

    public static IEnumerable<TObject> Search<TObject>(this LdapConnection connection, string dn, LdapScope scope = LdapScope.Base)
      where TObject : LdapObject
    {
      return connection.Search<TObject>(dn, scope, null);
    }

    public static IEnumerable<TObject> Search<TObject>(this LdapConnection connection, string dn, LdapScope scope = LdapScope.Base, LdapFilter filter = null)
      where TObject : LdapObject
    {
      var objectFilter = new LdapAttributeFilter(Attributes.ObjectClass, LdapObjectFactory.ObjectClass<TObject>());
      var realFilter = filter == null ? (LdapFilter)objectFilter : new LdapAndFilter(objectFilter, filter);
      var constraints = connection.SearchConstraints;
      constraints.MaxResults = MaxResults;
      var result = connection.Search(dn, (int)scope, realFilter.Text, new string[] { "*", "modifiersName", "modifyTimestamp" }, false, constraints);

      while (result.hasMore())
      {
        LdapEntry entry = null;

        try
        {
          entry = result.next();
        }
        catch (LdapException)
        {
        }

        if (entry != null)
        {
          yield return LdapObjectFactory.Create<TObject>(connection, entry);
        }
      }
    }

    public static IEnumerable<Person> SearchUniquePersons(this LdapConnection connection, IEnumerable<string> dnList, LdapScope scope = LdapScope.Base, LdapFilter filter = null)
    {
      List<int> ids = new List<int>();

      foreach (var dn in dnList)
      {
        var persons = connection.Search<Person>(dn, scope, filter);

        foreach (var person in persons)
        {
          if (!ids.Contains(person.Id))
          {
            ids.Add(person.Id);
            yield return person;
          }
        }
      }
    }

    public static IEnumerable<TObject> Search<TObject>(this LdapConnection connection, IEnumerable<string> dnList, LdapScope scope = LdapScope.Base, LdapFilter filter = null)
      where TObject : LdapObject
    {
      foreach (var dn in dnList)
      {
        var persons = connection.Search<TObject>(dn, scope, filter);

        foreach (var person in persons)
        {
          yield return person;
        }
      }
    }

    public static int MaxValue<TObject>(this LdapConnection connection, string attribute)
      where TObject : LdapObject
    {
      var objectFilter = new LdapAttributeFilter(Attributes.ObjectClass, LdapObjectFactory.ObjectClass<TObject>());
      var constraints = connection.SearchConstraints;
      constraints.MaxResults = MaxResults;
      var result = connection.Search(Names.Party, (int)LdapScope.Sub, objectFilter.Text, new string[] { attribute }, false, constraints);
      int maxValue = 0;

      while (result.hasMore())
      {
        LdapEntry entry = null;

        try
        {
          entry = result.next();

          int? value = entry.GetAttributeIntegerValue(attribute);
          
          if (value.HasValue)
          {
            maxValue = Math.Max(maxValue, value.Value);
          }
        }
        catch (LdapException)
        {
        }
      }

      return maxValue;
    }

    public static int GetNextPersonId(this LdapConnection connection)
    {
      var objectFilter = new LdapAttributeFilter(Attributes.ObjectClass, Attributes.UniqueStorage.ObjectClass);

      bool numberOk = false;
      int number = 0;

      while (!numberOk)
      {
        var result = connection.Search(Names.PersonId, (int)LdapScope.Base, objectFilter.Text, new string[] { Attributes.UniqueStorage.UidNumber }, false);

        if (!result.hasMore())
        {
          throw new InvalidOperationException("Cannot find unique storage.");
        }

        try
        {
          var value = result.next().GetAttributeIntegerValue(Attributes.UniqueStorage.UidNumber);

          if (value.HasValue)
          {
            number = value.Value;
          }
          else
          {
            throw new InvalidOperationException("Unique storage value missing.");
          }
        }
        catch (LdapException)
        {
          throw new InvalidOperationException("Cannot read unique storage.");
        }

        var addNewValue = new LdapModification(LdapModification.ADD, new LdapAttribute(Attributes.UniqueStorage.UidNumber, (number + 1).ToString()));
        var deleteOldValue = new LdapModification(LdapModification.DELETE, new LdapAttribute(Attributes.UniqueStorage.UidNumber, number.ToString()));

        try
        {
          connection.Modify(Names.PersonId, new[] { addNewValue, deleteOldValue });
          numberOk = true;
        }
        catch (LdapException)
        {
        }
      }

      return number;
    }

    public static int Count<TObject>(this LdapConnection connection, string dn, LdapScope scope = LdapScope.Base, LdapFilter filter = null)
      where TObject : LdapObject
    {
      var objectFilter = new LdapAttributeFilter(Attributes.ObjectClass, LdapObjectFactory.ObjectClass<TObject>());
      var realFilter = filter == null ? (LdapFilter)objectFilter : new LdapAndFilter(objectFilter, filter);
      var constraints = connection.SearchConstraints;
      constraints.MaxResults = MaxResults;
      var result = connection.Search(dn, (int)scope, realFilter.Text, new string[] { "entryDN" }, false, constraints);
      int count = 0;

      while (result.hasMore())
      {
        LdapEntry entry = null;

        try
        {
          entry = result.next();
        }
        catch (LdapException)
        {
        }

        if (entry != null)
        {
          count++;
        }
      }

      return count;
    }
  }
}
