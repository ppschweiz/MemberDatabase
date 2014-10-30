using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap
{
  public static class DN
  {
    public const string Separator = ",";
    public const string Assignment = "=";

    /// <summary>
    /// Builds a new DN from the parent DN and relative DN attribute and value.
    /// </summary>
    /// <param name="parentDn">The parent dn.</param>
    /// <param name="rdnAttribute">The RDN attribute.</param>
    /// <param name="rdnValue">The RDN value.</param>
    /// <returns>The new DN.</returns>
    public static string Build(string parentDn, string rdnAttribute, string rdnValue)
    {
      return rdnAttribute + Assignment + rdnValue + Separator + parentDn;
    }

    /// <summary>
    /// Builds a new DN from the parent DN and relative DN attribute and value.
    /// </summary>
    /// <param name="parentDn">The parent dn.</param>
    /// <param name="rdnAttribute">The RDN attribute.</param>
    /// <param name="rdnValue">The RDN value.</param>
    /// <returns>The new DN.</returns>
    public static string Build(string parentDn, string rdnAttribute, int rdnValue)
    {
      return Build(parentDn, rdnAttribute, rdnValue.ToString());
    }

    /// <summary>
    /// Determines whether the specified DN has some part.
    /// </summary>
    /// <param name="dn">The dn.</param>
    /// <param name="attribute">The attribute.</param>
    /// <returns>
    ///   <c>true</c> if the specified dn has part; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasPart(string dn, string attribute)
    {
      var parts = dn.Split(new string[] { Separator }, StringSplitOptions.None);

      return parts
        .Any(p => p.StartsWith(attribute + Assignment));
    }

    /// <summary>
    /// Gets a part value from a DN or null if not present.
    /// </summary>
    /// <param name="dn">The dn.</param>
    /// <param name="attribute">The attribute.</param>
    /// <returns>Value of that part of null if not present.</returns>
    public static string GetPart(string dn, string attribute)
    {
      var parts = dn.Split(new string[] { Separator }, StringSplitOptions.None);

      return parts
        .Where(p => p.StartsWith(attribute + Assignment))
        .Select(p => p.Substring(attribute.Length + Separator.Length))
        .FirstOrDefault();
    }

    /// <summary>
    /// Gets the RDN.
    /// </summary>
    /// <param name="dn">The dn.</param>
    /// <returns></returns>
    public static string GetRdn(string dn)
    {
      var splitIndex = dn.IndexOf(Separator);
      return dn.Substring(0, splitIndex);
    }

    /// <summary>
    /// Gets the parent dn.
    /// </summary>
    /// <param name="dn">The dn.</param>
    /// <returns></returns>
    public static string GetParentDn(string dn)
    {
      var splitIndex = dn.IndexOf(Separator);
      return dn.Substring(splitIndex + 1);
    }
  }
}
