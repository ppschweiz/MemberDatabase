using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pirate.Ldap.Data
{
  public class Request
  {
    public int Id { get; private set; }

    public RequestAction Action { get; private set; }

    public string OldDn { get; private set; }

    public string NewDn { get; private set; }

    public Dictionary<RequestParameter, string> Parameters { get; private set; }

    public DateTime Requested { get; private set; }

    public Request(int id, RequestAction action, string oldDn, string newDn, string parameter, DateTime requested)
    {
      Id = id;
      Action = action;
      OldDn = oldDn;
      NewDn = newDn;
      LoadParameters(parameter);
      Requested = requested;
    }

    public string Parameter
    {
      get 
      {
        return string.Join(";", Parameters.Select(p => FormatParameter(p)).ToArray());
      }
    }

    private string FormatParameter(KeyValuePair<RequestParameter, string> parameter)
    {
      if (string.IsNullOrEmpty(parameter.Value))
      {
        return parameter.Key.ToString();
      }
      else
      {
        return parameter.Key.ToString() + "=" + Escape(parameter.Value);
      }
    }

    private string Escape(string value)
    {
      return value
        .Replace("=", @"\=")
        .Replace(";", @"\;");
    }

    private string CreateToken(string text)
    {
      var rnd = new Random(DateTime.Now.Millisecond);
      string token;

      do
      {
        token = rnd.Next().ToString();
      }
      while (text.Contains(token));

      return token;
    }

    private void LoadParameters(string parameter)
    {
      Parameters = new Dictionary<RequestParameter, string>();

      string semicolonToken;
      string equalToken;

      do
      {
        semicolonToken = CreateToken(parameter);
        equalToken = CreateToken(parameter);
      }
      while (semicolonToken == equalToken);

      parameter = parameter
        .Replace(@"\;", semicolonToken)
        .Replace(@"\=", equalToken);

      foreach (var part in parameter.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
      {
        var keyValue = part.Split(new[] { "=" }, StringSplitOptions.None);

        if (keyValue.Length == 1)
        {
          var key = (RequestParameter)Enum.Parse(typeof(RequestParameter), keyValue[0]);
          Parameters.Add(key, string.Empty);
        }
        else if (keyValue.Length == 2)
        {
          var key = (RequestParameter)Enum.Parse(typeof(RequestParameter), keyValue[0]);
          Parameters.Add(key, keyValue[1]);
        }
        else
        {
          throw new ArgumentOutOfRangeException();
        }
      }
    }
  }
}
