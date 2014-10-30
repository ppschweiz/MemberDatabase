using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Novell.Directory.Ldap;

namespace Pirate.Ldap
{
  public abstract class LdapObject
  {
    private List<Field> _fields;

    private string _oldDn;

    private string _dn;

    protected string OldDN
    {
      get { return _oldDn; }
    }

    public string DN
    {
      get { return _dn; }
      protected set { _dn = value; }
    }

    public abstract string ObjectClass { get; }

    public IEnumerable<Field> Fields
    {
      get { return _fields; }
    }

    public IEnumerable<ComputedField> ComputedFields
    {
      get { return _fields.Where(f => f is ComputedField).Cast<ComputedField>(); }
    }

    public IEnumerable<LdapField> LdapFields
    {
      get { return _fields.Where(f => f is LdapField).Cast<LdapField>(); }
    }

    public LdapObject(string parentDn, LdapAttributeBase identifierAttribute, string identifierValue)
    {
      _dn = Ldap.DN.Build(parentDn, identifierAttribute, identifierValue);
      _oldDn = _dn;
      _fields = CreateFields().ToList();
    }

    public LdapObject(string parentDn, LdapAttributeBase identifierAttribute, int identifierValue)
    {
      _dn = Ldap.DN.Build(parentDn, identifierAttribute, identifierValue);
      _oldDn = _dn;
      _fields = CreateFields().ToList();
    }

    public LdapObject(LdapEntry entry)
    {
      _dn = entry.DN;
      _oldDn = _dn;
      _fields = CreateFields().ToList();

      foreach (var field in LdapFields)
      {
        field.Load(entry);

        if (field.IsNullOrEmpty() && field.Required)
        {
          throw new InvalidOperationException("Required attribute " + field.Attribute + " missing.");
        }
      }
    }

    public string SectionDnState
    {
      get
      {
        var match = Regex.Match(DN, "^.*st=([A-Za-z]+),.+$");

        if (match.Success)
        {
          return match.Groups[1].Value;
        }
        else
        {
          return "none";
        }
      }
    }

    public string SectionDnLocation
    {
      get
      {
        var match = Regex.Match(DN, "^.*l=([A-Za-z]+),.+$");

        if (match.Success)
        {
          return match.Groups[1].Value;
        }
        else
        {
          return "none";
        }
      }
    }

    protected abstract IEnumerable<Field> CreateFields();

    public void Create(LdapConnection connection)
    {
      var set = new LdapAttributeSet();
      set.Add(new LdapAttribute(Attributes.ObjectClass, ObjectClass));

      foreach (var field in LdapFields)
      {
        var value = field.GetValue();

        if (value != null)
        {
          set.Add(value);
        }
        else if (field.Required)
        {
          throw new InvalidOperationException("Required attribute " + field.Attribute + " missing.");
        }
      }

      var entry = new LdapEntry(_dn, set);
      connection.Add(entry);
      _oldDn = _dn;
    }

    public abstract LdapObject Reload(LdapConnection connection);

    public abstract void Delete(LdapConnection connection);

    public void Modify(LdapConnection connection)
    {
      Modify(connection, Reload(connection));
    }

    public void Modify(LdapConnection connection, LdapObject originalObject)
    {
      if (_oldDn != _dn)
      {
        MoveInternal(connection, _oldDn, _dn);
        _oldDn = _dn;
      }

      var mods = LdapFields
        .Select(f => f.GetModification(originalObject))
        .Where(m => m != null)
        .ToArray();

      connection.Modify(_dn, mods);
    }

    public string GetStringValue(LdapAttributeBase attribute, MultiValueOutput output)
    {
      return GetField(attribute).ValueString(output);
    }

    public LdapField<TValue> GetField<TValue>(LdapAttributeBase attribute)
    {
      return _fields.Where(f => f.Attribute.Equals(attribute)).Single() as LdapField<TValue>;
    }

    public Field GetField(LdapAttributeBase attribute)
    {
      return _fields.Where(f => f.Attribute.Equals(attribute)).Single() as Field;
    }

    public IEnumerable<Difference> GetDifferences(LdapObject originalObject, MultiValueOutput output)
    {
      return LdapFields
        .Select(f => f.GetDifference(originalObject, output))
        .Where(f => f != null);
    }

    public IEnumerable<Difference> GetDifferences(LdapConnection connection, MultiValueOutput output)
    {
      return GetDifferences(Reload(connection), output);
    }

    protected abstract void MoveInternal(LdapConnection connection, string oldDn, string newDn);

    /// <summary>
    /// Moves this Person in the LDAP tree.
    /// </summary>
    /// <param name="parentDn">The parent dn.</param>
    public void Move(string parentDn)
    {
      var rdn = Ldap.DN.GetRdn(DN);
      DN = string.Format("{0},{1}", rdn, parentDn);
    }
  }
}
