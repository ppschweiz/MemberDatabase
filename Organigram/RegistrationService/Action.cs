using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RegistrationService
{
  public enum Action
  {
    RegisterMember,
    RegisterForum
  }

  public static class Actions
  {
    public static string CreateParamemter(Action action)
    {
      return "action=" + action.ToString();
    }

    public static Action GetValue(HttpRequest request)
    {
      var actionString = request.Params["action"] ?? string.Empty;

      foreach (Action value in Enum.GetValues(typeof(Action)))
      {
        if (actionString.ToLowerInvariant() == value.ToString().ToLowerInvariant())
        {
          return value;
        }
      }

      return Action.RegisterForum;
    }
  }
}