using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pirate.Ldap.Data
{
  public class EmailAddressChange
  {
    public string Id { get; private set; }

    public string Username { get; private set; }

    public string RemoveAddress { get; private set; }

    public string AddAddress { get; private set; }

    public DateTime Creation { get; private set; }

    public EmailAddressChange(string id, string username, string removeAddress, string addAddress, DateTime creation)
    {
      Id = id;
      Username = username;
      RemoveAddress = removeAddress;
      AddAddress = addAddress;
      Creation = creation;
    }
  }
}