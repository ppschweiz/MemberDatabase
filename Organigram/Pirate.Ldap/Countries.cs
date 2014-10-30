using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pirate.Ldap
{
  /// <summary>
  /// Handles the list of countries.
  /// </summary>
  public static class Countries
  {
    private const string FileName = "Countries.txt";

    /// <summary>
    /// Gets the list of countries.
    /// </summary>
    /// <param name="path">The path where the list file can be found.</param>
    /// <returns>List of displayvalues of two letter abbrivations and full names.</returns>
    public static IEnumerable<DisplayValue> GetList(string path)
    {
      foreach (var line in File.ReadAllLines(Path.Combine(path, FileName)))
      {
        yield return new DisplayValue(GetNameFromLine(line), line.Substring(line.Length - 2, 2));
      }
    }

    /// <summary>
    /// Gets the name of a country.
    /// </summary>
    /// <param name="path">The path where the list file can be found.</param>
    /// <param name="code">The two letter code.</param>
    /// <returns></returns>
    public static string GetName(string path, string code)
    {
      return File.ReadAllLines(Path.Combine(path, FileName))
        .Where(x => x.EndsWith(code))
        .Select(x => GetNameFromLine(x))
        .FirstOrDefault();
    }

    private static string GetNameFromLine(string line)
    {
      var nameLarge = line.Substring(0, line.Length - 2).Trim();
      var languages = nameLarge.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
      var english = languages[0];
      var german = languages.Length >= 2 ? languages[1] : english;
      var french = languages.Length >= 3 ? languages[2] : english;
      var italien = languages.Length >= 4 ? languages[3] : english;

      switch (LdapResources.Culture.TwoLetterISOLanguageName)
      {
        case "de":
          return GetNameFromLarge(german);
        case "fr":
          return GetNameFromLarge(french);
        case "it":
          return GetNameFromLarge(italien);
        default:
          return GetNameFromLarge(english);
      }
    }

    private static string GetNameFromLarge(string name)
    {
      if (name.ToUpperInvariant() == name)
      {
        var words = name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Select(x => FormatWord(x)).ToArray());
      }
      else
      {
        return name;
      }
    }

    private static string FormatWord(string word)
    {
      return word.Substring(0, 1).ToUpperInvariant() + word.Substring(1).ToLowerInvariant();
    }
  }
}
